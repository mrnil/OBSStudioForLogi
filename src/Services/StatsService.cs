namespace Loupedeck.OBSStudioForLogiPlugin.Services
{
    using System;
    using System.Threading;
    using System.Timers;
    using Loupedeck.OBSStudioForLogiPlugin.Models;

    public class StatsService : IDisposable
    {
        private readonly System.Timers.Timer _pollTimer;
        private readonly Object _lock = new Object();
        private readonly Func<OBSStats> _getStats;
        private readonly Func<OBSStreamStats> _getStreamStats;
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
        {
            this._getStats = getStats;
            this._getStreamStats = getStreamStats;
            this._pollTimer = new System.Timers.Timer(intervalMs);
            this._pollTimer.Elapsed += this.OnPollTimer;
            this._pollTimer.AutoReset = true;
        }

        public void Start()
        {
            if (!this._disposed)
            {
                this._pollTimer.Start();
                PluginLog.Info($"StatsService started with {this._pollTimer.Interval}ms interval");
            }
        }

        public void Stop()
        {
            this._pollTimer.Stop();
            this.CurrentStats = null;
            this.CurrentStreamStats = null;
            PluginLog.Info("StatsService stopped");
        }

        public void SetInterval(Int32 intervalMs)
        {
            this._pollTimer.Interval = intervalMs;
            PluginLog.Info($"StatsService polling interval changed to {intervalMs}ms");
        }

        private void OnPollTimer(Object sender, ElapsedEventArgs e) => this.Poll();

        // The timer fires every interval whether or not the previous poll has finished, and a poll
        // blocks on OBS for up to the request timeout - so skip this tick rather than stack polls up
        // behind a slow OBS.
        internal void Poll()
        {
            if (this._disposed)
                return;

            if (Interlocked.Exchange(ref this._pollInProgress, 1) == 1)
            {
                PluginLog.Debug("StatsService: previous poll still running, skipping this tick");
                return;
            }

            try
            {
                OBSStats stats = this._getStats?.Invoke();
                OBSStreamStats streamStats = this._getStreamStats?.Invoke();
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
            if (this._disposed)
                return;

            this._disposed = true;
            this._pollTimer.Stop();
            this._pollTimer.Elapsed -= this.OnPollTimer;
            this._pollTimer.Dispose();
        }
    }
}
