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

        public Boolean IsOpen => Volatile.Read(ref this._open) == 1;

        // Returns true only for the call that opened the session.
        public Boolean TryOpen() => Interlocked.Exchange(ref this._open, 1) == 0;

        public void Close() => Interlocked.Exchange(ref this._open, 0);
    }
}
