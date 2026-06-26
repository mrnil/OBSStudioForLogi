namespace Loupedeck.OBSStudioForLogiPlugin.Services
{
    using System;
    using System.Timers;
    using Loupedeck.OBSStudioForLogiPlugin.Models;

    public class StatsService : IDisposable
    {
        private readonly Timer _pollTimer;
        private readonly Object _lock = new Object();
        private Boolean _disposed = false;

        public OBSStats CurrentStats { get; private set; }
        public OBSStreamStats CurrentStreamStats { get; private set; }
        public event EventHandler StatsUpdated;

        public StatsService(Int32 intervalMs = 5000)
        {
            this._pollTimer = new Timer(intervalMs);
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

        private void OnPollTimer(Object sender, ElapsedEventArgs e)
        {
            if (this._disposed)
                return;

            try
            {
                var stats = OBSStudioForLogiPlugin.Instance?.GetStats();
                var streamStats = OBSStudioForLogiPlugin.Instance?.GetStreamStatus();
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
