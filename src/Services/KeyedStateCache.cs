namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    // One OBS value per key, for button rendering - the single-value counterpart of AudioStateCache,
    // used for source visibility and media state. Get never blocks: a miss returns the default and
    // fetches that key once in the background, then onFetched asks the buttons to redraw. OBS change
    // events keep cached values current through Set.
    public class KeyedStateCache<TKey, TValue>
    {
        // Returns false when the value could not be read (not connected, request failed), so a
        // failure is never cached as if OBS had answered with the default.
        public delegate Boolean TryFetch(TKey key, out TValue value);

        private readonly Dictionary<TKey, TValue> _values = new Dictionary<TKey, TValue>();
        private readonly Dictionary<TKey, Object> _pendingFetches = new Dictionary<TKey, Object>();
        private readonly Dictionary<TKey, DateTime> _failedFetches = new Dictionary<TKey, DateTime>();
        private readonly Object _lock = new Object();
        private readonly TryFetch _fetch;
        private readonly Action<TKey> _onFetched;
        private readonly TValue _defaultValue;
        private readonly String _description;
        private readonly Action<Action> _runInBackground;
        private readonly Func<DateTime> _utcNow;
        private readonly TimeSpan _retryDelay;

        public KeyedStateCache(TryFetch fetch, Action<TKey> onFetched, TValue defaultValue, String description)
            : this(fetch, onFetched, defaultValue, description, action => Task.Run(action), () => DateTime.UtcNow)
        {
        }

        public KeyedStateCache(TryFetch fetch, Action<TKey> onFetched, TValue defaultValue, String description, Action<Action> runInBackground, Func<DateTime> utcNow)
        {
            this._fetch = fetch ?? throw new ArgumentNullException(nameof(fetch));
            this._onFetched = onFetched ?? (_ => { });
            this._defaultValue = defaultValue;
            this._description = description ?? "state";
            this._runInBackground = runInBackground ?? (action => Task.Run(action));
            this._utcNow = utcNow ?? (() => DateTime.UtcNow);
            this._retryDelay = TimeSpan.FromMilliseconds(OBSTimings.RenderStateRetryDelay);
        }

        public TValue Get(TKey key)
        {
            if (key == null)
            {
                return this._defaultValue;
            }

            Object fetchToken = null;
            TValue value;

            lock (this._lock)
            {
                if (this._values.TryGetValue(key, out value))
                {
                    return value;
                }

                value = this._defaultValue;

                if (this.ShouldFetch(key))
                {
                    fetchToken = new Object();
                    this._pendingFetches[key] = fetchToken;
                }
            }

            if (fetchToken != null)
            {
                this._runInBackground(() => this.Fetch(key, fetchToken));
            }

            return value;
        }

        // Called from OBS change events. The event is newer than any fetch in flight, so that
        // fetch's result is dropped rather than allowed to overwrite it.
        public void Set(TKey key, TValue value)
        {
            if (key == null)
            {
                return;
            }

            lock (this._lock)
            {
                this._values[key] = value;
                this._pendingFetches.Remove(key);
                this._failedFetches.Remove(key);
            }
        }

        public void Remove(TKey key)
        {
            if (key == null)
            {
                return;
            }

            lock (this._lock)
            {
                this._values.Remove(key);
                this._pendingFetches.Remove(key);
                this._failedFetches.Remove(key);
            }
        }

        public void RemoveWhere(Func<TKey, Boolean> predicate)
        {
            if (predicate == null)
            {
                return;
            }

            lock (this._lock)
            {
                foreach (TKey key in this._values.Keys.Where(predicate).ToList())
                {
                    this._values.Remove(key);
                }

                foreach (TKey key in this._pendingFetches.Keys.Where(predicate).ToList())
                {
                    this._pendingFetches.Remove(key);
                }

                foreach (TKey key in this._failedFetches.Keys.Where(predicate).ToList())
                {
                    this._failedFetches.Remove(key);
                }
            }
        }

        // Called when OBS renames a scene or input: rename returns each key's new key, or the key
        // unchanged. A value moves to its new key unless that key already has one, which was read
        // from OBS after the rename and so is newer. Fetches in flight for the old keys, and their
        // failure backoff, are dropped.
        public void RenameKeys(Func<TKey, TKey> rename)
        {
            if (rename == null)
            {
                return;
            }

            lock (this._lock)
            {
                foreach (KeyValuePair<TKey, TValue> pair in this._values.ToList())
                {
                    TKey newKey = rename(pair.Key);
                    if (EqualityComparer<TKey>.Default.Equals(newKey, pair.Key))
                    {
                        continue;
                    }

                    this._values.Remove(pair.Key);
                    if (newKey != null && !this._values.ContainsKey(newKey))
                    {
                        this._values[newKey] = pair.Value;
                        this._pendingFetches.Remove(newKey);
                        this._failedFetches.Remove(newKey);
                    }
                }

                foreach (TKey key in this._pendingFetches.Keys.Where(key => !EqualityComparer<TKey>.Default.Equals(rename(key), key)).ToList())
                {
                    this._pendingFetches.Remove(key);
                }

                foreach (TKey key in this._failedFetches.Keys.Where(key => !EqualityComparer<TKey>.Default.Equals(rename(key), key)).ToList())
                {
                    this._failedFetches.Remove(key);
                }
            }
        }

        public void Clear()
        {
            lock (this._lock)
            {
                this._values.Clear();
                this._pendingFetches.Clear();
                this._failedFetches.Clear();
            }
        }

        private Boolean ShouldFetch(TKey key)
        {
            if (this._pendingFetches.ContainsKey(key))
            {
                return false;
            }

            return !this._failedFetches.TryGetValue(key, out DateTime failedAt) || this._utcNow() - failedAt > this._retryDelay;
        }

        private void Fetch(TKey key, Object fetchToken)
        {
            Boolean fetched = false;
            TValue value = this._defaultValue;
            try
            {
                fetched = this._fetch(key, out value);
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to fetch {this._description} for '{key}': {ex.Message}");
            }

            lock (this._lock)
            {
                // Drop the result if a Clear, Remove, Set or newer fetch superseded this one.
                if (!this._pendingFetches.TryGetValue(key, out Object current) || current != fetchToken)
                {
                    return;
                }

                this._pendingFetches.Remove(key);

                if (!fetched)
                {
                    this._failedFetches[key] = this._utcNow();
                    return;
                }

                this._failedFetches.Remove(key);
                this._values[key] = value;
            }

            this._onFetched(key);
        }
    }
}
