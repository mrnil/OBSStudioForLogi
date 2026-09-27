namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    // A lease that stays active while it keeps being touched and lapses once it has gone
    // untouched for longer than its duration. Used where the SDK gives no explicit "visible" /
    // "hidden" lifecycle (e.g. individual PluginDynamicCommand buttons): image requests renew the
    // lease, and a periodic check expires it once they stop.
    public class ActivityLease
    {
        private readonly TimeSpan _duration;
        private readonly Func<DateTime> _utcNow;
        private readonly Object _lock = new Object();
        private DateTime _lastTouchUtc;
        private Boolean _isActive;

        public ActivityLease(TimeSpan duration) : this(duration, () => DateTime.UtcNow)
        {
        }

        public ActivityLease(TimeSpan duration, Func<DateTime> utcNow)
        {
            this._duration = duration;
            this._utcNow = utcNow ?? (() => DateTime.UtcNow);
        }

        public Boolean IsActive
        {
            get
            {
                lock (this._lock)
                {
                    return this._isActive;
                }
            }
        }

        // Renews the lease. Returns true only when this touch activated it.
        public Boolean Touch()
        {
            lock (this._lock)
            {
                this._lastTouchUtc = this._utcNow();

                if (this._isActive)
                {
                    return false;
                }

                this._isActive = true;
                return true;
            }
        }

        // Lapses the lease if it has gone untouched past its duration. Returns true only when
        // this call deactivated it.
        public Boolean ExpireIfIdle()
        {
            lock (this._lock)
            {
                if (!this._isActive || this._utcNow() - this._lastTouchUtc <= this._duration)
                {
                    return false;
                }

                this._isActive = false;
                return true;
            }
        }

        // Ends the lease immediately. Returns true if it was active.
        public Boolean Release()
        {
            lock (this._lock)
            {
                Boolean wasActive = this._isActive;
                this._isActive = false;
                return wasActive;
            }
        }
    }
}
