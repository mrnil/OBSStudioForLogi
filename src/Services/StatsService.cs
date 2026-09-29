namespace Loupedeck.OBSStudioForLogiPlugin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using Loupedeck.OBSStudioForLogiPlugin.Models;

    // Polls OBS stats on a timer, but only while connected and while something that shows stats
    // is visible: each stats folder registers as a viewer while open, and the summary button while
    // the SDK keeps asking for its image. The first viewer gets an immediate poll rather than
    // waiting a full interval for its first numbers (assessment.md #25).
    public class StatsService : IDisposable
    {
        private readonly System.Timers.Timer _pollTimer;
        private readonly Object _lock = new Object();
        private readonly Func<OBSStats> _getStats;
        private readonly Func<OBSStreamStats> _getStreamStats;
        private readonly Action<Action> _runInBackground;
        private readonly HashSet<String> _viewers = new HashSet<String>();
        private Boolean _started;
        private volatile Boolean _paused;
        private Int32 _pollInProgress;
        private Boolean _disposed = false;

        public OBSStats CurrentStats { get; private set; }
        public OBSStreamStats CurrentStreamStats { get; private set; }
        public event EventHandler StatsUpdated;

        public StatsService(Int32 intervalMs = 5000)
            : this(intervalMs, () => OBSStudioForLogiPlugin.Instance?.GetStats(), () => OBSStudioForLogiPlugin.Instance?.GetStreamStatus())
        {
        }

        public StatsService(Int32 intervalMs, Func<OBSStats> getStats, Func<OBSStreamStats> getStreamStats)
            : this(intervalMs, getStats, getStreamStats, action => Task.Run(action))
        {
        }

        public StatsService(Int32 intervalMs, Func<OBSStats> getStats, Func<OBSStreamStats> getStreamStats, Action<Action> runInBackground)
        {
            this._getStats = getStats;
            this._getStreamStats = getStreamStats;
            this._runInBackground = runInBackground ?? (action => Task.Run(action));
            this._pollTimer = new System.Timers.Timer(intervalMs);
            this._pollTimer.Elapsed += this.OnPollTimer;
            this._pollTimer.AutoReset = true;
        }

        // True while the poll timer is running.
        public Boolean IsPolling => this._pollTimer.Enabled;

        // Connected to OBS: polling may run from now on, and does if anything shows stats.
        public void Start()
        {
            lock (this._lock)
            {
                if (this._disposed)
                {
                    return;
                }

                this._started = true;
                this._paused = false;
                this.UpdatePolling("connected");
            }
        }

        // Disconnected: stop polling and drop the last stats so nothing shows stale numbers.
        public void Stop()
        {
            lock (this._lock)
            {
                this._started = false;
                this._paused = false;
                this.UpdatePolling("disconnected");
                this.CurrentStats = null;
                this.CurrentStreamStats = null;
            }
        }

        // OBS answers every request with "not ready" while it switches scene collection, so polling
        // stops until the switch finishes. The last stats stay on screen meanwhile. Start and Stop
        // clear the pause, so a switch that never reports finishing can't leave polling off.
        public void Pause(String reason)
        {
            lock (this._lock)
            {
                if (this._paused)
                {
                    return;
                }

                this._paused = true;
                PluginLog.Info($"StatsService paused ({reason})");
                this.UpdatePolling(reason);
            }
        }

        public void Resume(String reason)
        {
            lock (this._lock)
            {
                if (!this._paused)
                {
                    return;
                }

                this._paused = false;
                PluginLog.Info($"StatsService resumed ({reason})");
                this.UpdatePolling(reason, "resumed");
            }
        }

        public void AddViewer(String owner)
        {
            if (String.IsNullOrEmpty(owner))
            {
                return;
            }

            lock (this._lock)
            {
                if (this._disposed || !this._viewers.Add(owner))
                {
                    return;
                }

                PluginLog.Info($"StatsService: stats wanted by '{owner}' ({this._viewers.Count} viewer(s))");
                this.UpdatePolling($"'{owner}' is showing stats");
            }
        }

        public void RemoveViewer(String owner)
        {
            if (String.IsNullOrEmpty(owner))
            {
                return;
            }

            lock (this._lock)
            {
                if (!this._viewers.Remove(owner))
                {
                    return;
                }

                PluginLog.Info($"StatsService: stats no longer wanted by '{owner}' ({this._viewers.Count} viewer(s))");
                this.UpdatePolling($"'{owner}' stopped showing stats");
            }
        }

        public void SetInterval(Int32 intervalMs)
        {
            this._pollTimer.Interval = intervalMs;
            PluginLog.Info($"StatsService polling interval changed to {intervalMs}ms");
        }

        // Called under _lock. Starts the timer (plus one poll straight away, so a newly shown stats
        // button doesn't wait a whole interval) when connected, not paused and with viewers, and
        // stops it otherwise.
        private void UpdatePolling(String reason, String firstPollTrigger = "first viewer")
        {
            Boolean shouldPoll = this._started && !this._paused && this._viewers.Count > 0 && !this._disposed;

            if (shouldPoll == this._pollTimer.Enabled)
            {
                return;
            }

            if (shouldPoll)
            {
                this._pollTimer.Start();
                PluginLog.Info($"StatsService polling started every {this._pollTimer.Interval}ms ({reason})");
                this._runInBackground(() => this.Poll(firstPollTrigger));
            }
            else
            {
                this._pollTimer.Stop();
                PluginLog.Info($"StatsService polling stopped ({reason})");
            }
        }

        private void OnPollTimer(Object sender, ElapsedEventArgs e) => this.Poll("timer");

        internal void Poll() => this.Poll("manual");

        // The timer fires every interval whether or not the previous poll has finished, and a poll
        // blocks on OBS for up to the request timeout - so skip this tick rather than stack polls up
        // behind a slow OBS.
        private void Poll(String trigger)
        {
            // A timer tick can already be queued when polling pauses.
            if (this._disposed || this._paused)
                return;

            if (Interlocked.Exchange(ref this._pollInProgress, 1) == 1)
            {
                PluginLog.Debug("StatsService: previous poll still running, skipping this tick");
                return;
            }

            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                OBSStats stats = this._getStats?.Invoke();
                OBSStreamStats streamStats = this._getStreamStats?.Invoke();
                PluginLog.Debug($"StatsService: polled stats in {stopwatch.ElapsedMilliseconds} ms ({trigger})");
                if (stats != null)
                {
                    lock (this._lock)
                    {
                        this.CurrentStats = stats;
                        this.CurrentStreamStats = streamStats;
                    }
                    this.StatsUpdated?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                PluginLog.Warning($"StatsService: Failed to poll stats: {ex.Message}");
            }
            finally
            {
                Interlocked.Exchange(ref this._pollInProgress, 0);
            }
        }

        public void Dispose()
        {
            lock (this._lock)
            {
                if (this._disposed)
                    return;

                this._disposed = true;
                this._pollTimer.Stop();
                this._pollTimer.Elapsed -= this.OnPollTimer;
                this._pollTimer.Dispose();
            }
        }
    }
}
