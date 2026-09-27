namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // Collapses bursts of an identical log message into one line per window. A single stalled OBS
    // used to produce the same error for every button on every redraw - hundreds of lines for one
    // incident. The first occurrence is written immediately; repeats inside the window are counted
    // and reported on the next occurrence after it.
    public class LogThrottle
    {
        private const Int32 PruneThreshold = 256;

        private readonly Dictionary<String, Entry> _entries = new Dictionary<String, Entry>();
        private readonly Object _lock = new Object();
        private readonly TimeSpan _window;
        private readonly Func<DateTime> _utcNow;

        public LogThrottle(TimeSpan window) : this(window, () => DateTime.UtcNow)
        {
        }

        public LogThrottle(TimeSpan window, Func<DateTime> utcNow)
        {
            this._window = window;
            this._utcNow = utcNow ?? (() => DateTime.UtcNow);
        }

        internal Int32 TrackedMessageCount
        {
            get
            {
                lock (this._lock)
                {
                    return this._entries.Count;
                }
            }
        }

        // suppressedCount is how many identical messages were dropped since this one was last written.
        public Boolean ShouldWrite(String message, out Int32 suppressedCount)
        {
            suppressedCount = 0;
            message ??= String.Empty;
            DateTime now = this._utcNow();

            lock (this._lock)
            {
                if (this._entries.Count > PruneThreshold)
                {
                    this.PruneExpired(now);
                }

                if (this._entries.TryGetValue(message, out Entry entry) && now - entry.LastWrittenUtc <= this._window)
                {
                    entry.Suppressed++;
                    return false;
                }

                suppressedCount = entry?.Suppressed ?? 0;
                this._entries[message] = new Entry { LastWrittenUtc = now };
                return true;
            }
        }

        private void PruneExpired(DateTime now)
        {
            foreach (String expired in this._entries.Where(pair => now - pair.Value.LastWrittenUtc > this._window).Select(pair => pair.Key).ToList())
            {
                this._entries.Remove(expired);
            }
        }

        private sealed class Entry
        {
            public DateTime LastWrittenUtc { get; set; }
            public Int32 Suppressed { get; set; }
        }
    }
}
