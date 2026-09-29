namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading;

    // Marks whether the current OBS connection's session has been established, so work that must
    // happen once per connection (the initial state load) runs exactly once even when the
    // websocket library reports Connected again for the same connection - obs-websocket-dotnet
    // 5.7.0 raises Connected for every Identified message, and OBS sends one to confirm each
    // ReIdentify (e.g. every InputVolumeMeters subscribe/unsubscribe).
    public class SessionGate
    {
        private Int32 _open;
        private Int64 _generation;

        public Boolean IsOpen => Volatile.Read(ref this._open) == 1;

        // Identifies the session TryOpen last opened, so work started for one connection (like
        // the initial state load's retries) can tell when that connection has gone, even if a
        // new one has opened since.
        public Int64 Generation => Interlocked.Read(ref this._generation);

        // Returns true only for the call that opened the session.
        public Boolean TryOpen()
        {
            if (Interlocked.Exchange(ref this._open, 1) != 0)
            {
                return false;
            }

            Interlocked.Increment(ref this._generation);
            return true;
        }

        public void Close() => Interlocked.Exchange(ref this._open, 0);

        // True while the session with this generation is still the open one.
        public Boolean IsCurrent(Int64 generation) => this.IsOpen && this.Generation == generation;
    }
}
