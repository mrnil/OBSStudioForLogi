namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading;

    // Exponential backoff with jitter for reconnecting after the connection to OBS drops or an
    // attempt fails. ConnectionManager counts each attempt with BeginAttempt and resets on connect;
    // connects and drops arrive on different threads, so the count is updated atomically.
    public class ReconnectionStrategy
    {
        private static readonly Int32[] DefaultBackoffDelays = { 1000, 2000, 4000, 8000, 15000, 30000 };

        private readonly IPluginLog _log;
        private readonly Int32[] _backoffDelays;
        private Int32 _attempts;

        public Int32 Attempts => Volatile.Read(ref this._attempts);

        public ReconnectionStrategy(IPluginLog log)
            : this(log, null)
        {
        }

        // backoffDelays (ms, one per attempt, the last repeating) lets tests use short delays.
        public ReconnectionStrategy(IPluginLog log, Int32[] backoffDelays)
        {
            this._log = log;
            this._backoffDelays = backoffDelays != null && backoffDelays.Length > 0 ? backoffDelays : DefaultBackoffDelays;
        }

        public void Reset()
        {
            Interlocked.Exchange(ref this._attempts, 0);
        }

        // The delay before the next attempt, from the attempts counted so far.
        public Int32 GetNextDelay()
        {
            Int32 index = Math.Min(this.Attempts, this._backoffDelays.Length - 1);
            Int32 baseDelay = this._backoffDelays[index];
            Double jitter = Random.Shared.NextDouble() * 0.3 + 0.85;
            return (Int32)(baseDelay * jitter);
        }

        // Counts a reconnection attempt and returns how long to wait before making it. Each attempt
        // waits longer than the last, up to the final delay, until Reset.
        public Int32 BeginAttempt()
        {
            Int32 delay = this.GetNextDelay();
            Int32 attempt = Interlocked.Increment(ref this._attempts);
            this._log.Info($"Reconnection attempt {attempt} in {delay}ms");
            return delay;
        }
    }
}
