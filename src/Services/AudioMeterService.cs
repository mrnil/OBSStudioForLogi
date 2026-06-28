namespace Loupedeck.OBSStudioForLogiPlugin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Timers;

    public class AudioMeterService : IDisposable
    {
        private readonly Dictionary<String, (Single peakL, Single peakR)> _levels = new Dictionary<String, (Single, Single)>();
        private readonly Object _lock = new Object();
        private Timer _refreshTimer;
        private Boolean _disposed = false;

        public Boolean IsActive { get; private set; }
        public event EventHandler MetersUpdated;

        public void Start()
        {
            if (this.IsActive)
                return;

            this.IsActive = true;

            if (this._refreshTimer == null)
            {
                this._refreshTimer = new Timer(100); // 10fps
                this._refreshTimer.Elapsed += this.OnRefreshTimer;
                this._refreshTimer.AutoReset = true;
            }

            this._refreshTimer.Start();
        }

        public void Stop()
        {
            this.IsActive = false;
            this._refreshTimer?.Stop();
            this.Clear();
        }

        public void UpdateLevels(Dictionary<String, (Single peakL, Single peakR)> levels)
        {
            lock (this._lock)
            {
                foreach (var kvp in levels)
                {
                    this._levels[kvp.Key] = kvp.Value;
                }
            }
        }

        public (Single peakL, Single peakR) GetLevels(String inputName)
        {
            lock (this._lock)
            {
                if (this._levels.TryGetValue(inputName, out var levels))
                    return levels;
                return (0f, 0f);
            }
        }

        public String[] GetInputNames()
        {
            lock (this._lock)
            {
                var names = new String[this._levels.Count];
                this._levels.Keys.CopyTo(names, 0);
                return names;
            }
        }

        public void Clear()
        {
            lock (this._lock)
            {
                this._levels.Clear();
            }
        }

        private void OnRefreshTimer(Object sender, ElapsedEventArgs e)
        {
            if (!this.IsActive || this._disposed)
                return;

            this.MetersUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (this._disposed)
                return;

            this._disposed = true;
            this.IsActive = false;

            if (this._refreshTimer != null)
            {
                this._refreshTimer.Stop();
                this._refreshTimer.Elapsed -= this.OnRefreshTimer;
                this._refreshTimer.Dispose();
                this._refreshTimer = null;
            }
        }
    }
}
