namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;
    using Loupedeck.OBSStudioForLogiPlugin.Models;

    // Mute, volume and monitor type per audio input, for button rendering. The SDK renders on its
    // own threads and every OBS request blocks until OBS answers (up to the websocket timeout), so
    // rendering straight from OBS stalled the device whenever requests backed up. Get never blocks:
    // a miss returns defaults and fetches that input once in the background, then onFetched asks
    // the buttons to redraw. OBS change events keep cached values current from then on.
    public class AudioStateCache
    {
        private readonly Dictionary<String, Entry> _entries = new Dictionary<String, Entry>();
        private readonly Dictionary<String, Object> _pendingFetches = new Dictionary<String, Object>();
        private readonly Dictionary<String, DateTime> _failedFetches = new Dictionary<String, DateTime>();
        private readonly Object _lock = new Object();
        private readonly Func<String, AudioInputState> _fetch;
        private readonly Action<String> _onFetched;
        private readonly Action<Action> _runInBackground;
        private readonly Func<DateTime> _utcNow;
        private readonly TimeSpan _retryDelay = TimeSpan.FromMilliseconds(OBSTimings.AudioStateRetryDelay);

        // fetch returns null when the state could not be read (not connected, request failed).
        public AudioStateCache(Func<String, AudioInputState> fetch, Action<String> onFetched)
            : this(fetch, onFetched, action => Task.Run(action), () => DateTime.UtcNow)
        {
        }

        public AudioStateCache(Func<String, AudioInputState> fetch, Action<String> onFetched, Action<Action> runInBackground, Func<DateTime> utcNow)
        {
            this._fetch = fetch ?? throw new ArgumentNullException(nameof(fetch));
            this._onFetched = onFetched ?? (_ => { });
            this._runInBackground = runInBackground ?? (action => Task.Run(action));
            this._utcNow = utcNow ?? (() => DateTime.UtcNow);
        }

        public AudioInputState Get(String inputName)
        {
            if (String.IsNullOrEmpty(inputName))
            {
                return AudioInputState.Default;
            }

            Object fetchToken = null;
            AudioInputState state;

            lock (this._lock)
            {
                this._entries.TryGetValue(inputName, out Entry entry);
                state = entry?.ToState() ?? AudioInputState.Default;

                if (entry?.IsComplete != true && this.ShouldFetch(inputName))
                {
                    fetchToken = new Object();
                    this._pendingFetches[inputName] = fetchToken;
                }
            }

            if (fetchToken != null)
            {
                this._runInBackground(() => this.Fetch(inputName, fetchToken));
            }

            return state;
        }

        public void SetMuted(String inputName, Boolean isMuted) => this.Update(inputName, entry => entry.IsMuted = isMuted);

        public void SetVolume(String inputName, Single volumeMul) => this.Update(inputName, entry => entry.VolumeMul = volumeMul);

        public void SetMonitorType(String inputName, String monitorType) => this.Update(inputName, entry => entry.MonitorType = monitorType);

        public void Remove(String inputName)
        {
            if (String.IsNullOrEmpty(inputName))
            {
                return;
            }

            lock (this._lock)
            {
                this._entries.Remove(inputName);
                this._pendingFetches.Remove(inputName);
                this._failedFetches.Remove(inputName);
            }
        }

        public void Clear()
        {
            lock (this._lock)
            {
                this._entries.Clear();
                this._pendingFetches.Clear();
                this._failedFetches.Clear();
            }
        }

        private void Update(String inputName, Action<Entry> apply)
        {
            if (String.IsNullOrEmpty(inputName))
            {
                return;
            }

            lock (this._lock)
            {
                if (!this._entries.TryGetValue(inputName, out Entry entry))
                {
                    entry = new Entry();
                    this._entries[inputName] = entry;
                }

                apply(entry);
            }
        }

        private Boolean ShouldFetch(String inputName)
        {
            if (this._pendingFetches.ContainsKey(inputName))
            {
                return false;
            }

            return !this._failedFetches.TryGetValue(inputName, out DateTime failedAt) || this._utcNow() - failedAt > this._retryDelay;
        }

        private void Fetch(String inputName, Object fetchToken)
        {
            AudioInputState fetched = null;
            try
            {
                fetched = this._fetch(inputName);
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to fetch audio state for '{inputName}': {ex.Message}");
            }

            lock (this._lock)
            {
                // Drop the result if a Clear, Remove or newer fetch superseded this one.
                if (!this._pendingFetches.TryGetValue(inputName, out Object current) || current != fetchToken)
                {
                    return;
                }

                this._pendingFetches.Remove(inputName);

                if (fetched == null)
                {
                    this._failedFetches[inputName] = this._utcNow();
                    return;
                }

                this._failedFetches.Remove(inputName);

                if (!this._entries.TryGetValue(inputName, out Entry entry))
                {
                    entry = new Entry();
                    this._entries[inputName] = entry;
                }

                // Values set by change events while the fetch was in flight are newer - keep them.
                entry.IsMuted ??= fetched.IsMuted;
                entry.VolumeMul ??= fetched.VolumeMul;
                entry.MonitorType ??= fetched.MonitorType;
            }

            this._onFetched(inputName);
        }

        private sealed class Entry
        {
            public Boolean? IsMuted { get; set; }
            public Single? VolumeMul { get; set; }
            public String MonitorType { get; set; }

            public Boolean IsComplete => this.IsMuted.HasValue && this.VolumeMul.HasValue && this.MonitorType != null;

            public AudioInputState ToState() => new AudioInputState
            {
                IsMuted = this.IsMuted ?? false,
                VolumeMul = this.VolumeMul ?? 1.0f,
                MonitorType = this.MonitorType ?? AudioInputState.MonitorTypeNone
            };
        }
    }
}
