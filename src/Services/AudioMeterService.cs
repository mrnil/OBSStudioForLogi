namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    public class AudioMeterService
    {
        private readonly Dictionary<String, LevelEntry> _levels = new Dictionary<String, LevelEntry>();
        private readonly Dictionary<String, Boolean> _muteStates = new Dictionary<String, Boolean>();
        private readonly HashSet<String> _pendingMuteLookups = new HashSet<String>();
        private readonly Object _lock = new Object();
        private readonly Func<DateTime> _utcNow;
        private readonly TimeSpan _staleAfter = TimeSpan.FromMilliseconds(OBSTimings.AudioMeterStaleThreshold);
        private Int64 _nextOrder = 0;

        public AudioMeterService() : this(() => DateTime.UtcNow)
        {
        }

        public AudioMeterService(Func<DateTime> utcNow)
        {
            this._utcNow = utcNow ?? (() => DateTime.UtcNow);
        }

        public void UpdateLevels(String inputName, Models.AudioMeterLevels levels)
        {
            if (String.IsNullOrEmpty(inputName) || levels == null)
            {
                return;
            }

            lock (this._lock)
            {
                // Keep the first-seen order across updates (and across an input going stale and
                // coming back) so the folder's buttons don't reshuffle on every event.
                Int64 order = this._levels.TryGetValue(inputName, out LevelEntry existing) ? existing.Order : this._nextOrder++;
                this._levels[inputName] = new LevelEntry(levels.ChannelPeaks ?? new Single[0], this._utcNow(), order);
            }
        }

        // OBS only includes an input in InputVolumeMeters while it is active, so an input that
        // drops out of the event (scene switched away, source hidden/removed) simply stops being
        // updated. Levels older than the stale threshold are treated as "no data" rather than
        // left frozen on the last value OBS happened to send.
        public Models.AudioMeterLevels GetLevels(String inputName)
        {
            if (String.IsNullOrEmpty(inputName))
            {
                return Models.AudioMeterLevels.Empty;
            }

            lock (this._lock)
            {
                Boolean isMuted = this._muteStates.TryGetValue(inputName, out Boolean muted) && muted;

                if (!this._levels.TryGetValue(inputName, out LevelEntry entry) || this.IsStale(entry))
                {
                    return new Models.AudioMeterLevels { IsMuted = isMuted };
                }

                return new Models.AudioMeterLevels { ChannelPeaks = entry.ChannelPeaks, IsMuted = isMuted, IsLive = true };
            }
        }

        // Inputs OBS is currently reporting as active, in first-seen order. This includes inputs
        // reported with zero channels (e.g. a browser source with no audio flowing through OBS yet) -
        // they are live, just silent, and the user still expects to see and mute them.
        public String[] GetLiveInputs()
        {
            lock (this._lock)
            {
                return this._levels
                    .Where(pair => !this.IsStale(pair.Value))
                    .OrderBy(pair => pair.Value.Order)
                    .Select(pair => pair.Key)
                    .ToArray();
            }
        }

        // Authoritative mute state, from the InputMuteStateChanged event.
        public void SetMuted(String inputName, Boolean isMuted)
        {
            if (String.IsNullOrEmpty(inputName))
            {
                return;
            }

            lock (this._lock)
            {
                this._muteStates[inputName] = isMuted;
                this._pendingMuteLookups.Remove(inputName);
            }
        }

        // Returns true exactly once for an input whose mute state is unknown, so the caller can
        // query OBS for it off the render path without issuing a request per meter event.
        public Boolean TryBeginMuteLookup(String inputName)
        {
            if (String.IsNullOrEmpty(inputName))
            {
                return false;
            }

            lock (this._lock)
            {
                if (this._muteStates.ContainsKey(inputName) || this._pendingMuteLookups.Contains(inputName))
                {
                    return false;
                }

                this._pendingMuteLookups.Add(inputName);
                return true;
            }
        }

        // Applies a queried mute state only if the lookup is still pending - an event-sourced
        // SetMuted, or a Clear, since the lookup began wins over the (possibly older) query result.
        public void CompleteMuteLookup(String inputName, Boolean isMuted)
        {
            if (String.IsNullOrEmpty(inputName))
            {
                return;
            }

            lock (this._lock)
            {
                if (this._pendingMuteLookups.Remove(inputName))
                {
                    this._muteStates[inputName] = isMuted;
                }
            }
        }

        // Called when OBS renames an input. Levels (keeping their place in the live-input order) and
        // mute state move to the new name, so the meter folder doesn't briefly show the input twice
        // while the old name goes stale. Anything already recorded under the new name is newer and
        // is kept.
        public void RenameInput(String oldInputName, String newInputName)
        {
            if (String.IsNullOrEmpty(oldInputName) || String.IsNullOrEmpty(newInputName) || oldInputName == newInputName)
            {
                return;
            }

            lock (this._lock)
            {
                if (this._levels.TryGetValue(oldInputName, out LevelEntry levels))
                {
                    this._levels.Remove(oldInputName);
                    if (!this._levels.ContainsKey(newInputName))
                    {
                        this._levels[newInputName] = levels;
                    }
                }

                if (this._muteStates.TryGetValue(oldInputName, out Boolean isMuted))
                {
                    this._muteStates.Remove(oldInputName);
                    if (!this._muteStates.ContainsKey(newInputName))
                    {
                        this._muteStates[newInputName] = isMuted;
                        this._pendingMuteLookups.Remove(newInputName);
                    }
                }

                this._pendingMuteLookups.Remove(oldInputName);
            }
        }

        public void Clear()
        {
            lock (this._lock)
            {
                this._levels.Clear();
                this._muteStates.Clear();
                this._pendingMuteLookups.Clear();
                this._nextOrder = 0;
            }
        }

        private Boolean IsStale(LevelEntry entry) => this._utcNow() - entry.UpdatedUtc > this._staleAfter;

        private sealed class LevelEntry
        {
            public LevelEntry(Single[] channelPeaks, DateTime updatedUtc, Int64 order)
            {
                this.ChannelPeaks = channelPeaks;
                this.UpdatedUtc = updatedUtc;
                this.Order = order;
            }

            public Single[] ChannelPeaks { get; }
            public DateTime UpdatedUtc { get; }
            public Int64 Order { get; }
        }
    }
}
