namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    // One OBS-derived value that is expensive to fetch and changes only on known OBS events: the
    // first TryGet fetches it on the caller's thread, later calls return the stored copy until
    // Invalidate. Callers must be off the render and OBS event threads, since a miss blocks on OBS.
    public class CachedValue<T> where T : class
    {
        // Returns false when the value could not be read (not connected, request failed), so a
        // failure is never cached as if OBS had answered.
        public delegate Boolean TryFetch(out T value);

        private readonly TryFetch _fetch;
        private readonly String _description;
        private readonly Object _lock = new Object();
        private T _value;
        private Int64 _generation;

        public CachedValue(TryFetch fetch, String description)
        {
            this._fetch = fetch ?? throw new ArgumentNullException(nameof(fetch));
            this._description = description ?? "value";
        }

        public Boolean TryGet(out T value)
        {
            Int64 generation;
            lock (this._lock)
            {
                if (this._value != null)
                {
                    PluginLog.Debug($"Cache hit for {this._description}");
                    value = this._value;
                    return true;
                }

                generation = this._generation;
            }

            PluginLog.Debug($"Cache miss for {this._description} - fetching from OBS");
            if (!this._fetch(out value) || value == null)
            {
                value = null;
                return false;
            }

            lock (this._lock)
            {
                // An Invalidate during the fetch means OBS changed while it ran, so the result may
                // already be stale: hand it to this caller but don't keep it.
                if (generation == this._generation)
                {
                    this._value = value;
                }
            }

            return true;
        }

        public void Invalidate(String reason)
        {
            lock (this._lock)
            {
                this._generation++;
                if (this._value == null)
                {
                    return;
                }

                this._value = null;
            }

            PluginLog.Debug($"Cleared cached {this._description} ({reason})");
        }
    }
}
