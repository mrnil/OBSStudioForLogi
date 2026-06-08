namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class ReconnectionStrategy
    {
        private readonly IPluginLog _log;
        private readonly Int32[] _backoffDelays = { 1000, 2000, 4000, 8000, 15000, 30000 };

        public Int32 Attempts { get; private set; }

        public ReconnectionStrategy(IPluginLog log)
        {
            this._log = log;
        }

        public void Reset()
        {
            this.Attempts = 0;
        }

        public Int32 GetNextDelay()
        {
            var index = Math.Min(this.Attempts, this._backoffDelays.Length - 1);
            var baseDelay = this._backoffDelays[index];
            var jitter = Random.Shared.NextDouble() * 0.3 + 0.85;
            return (Int32)(baseDelay * jitter);
        }

        public Boolean TryReconnect(Action connectAction)
        {
            this.Attempts++;
            this._log.Info($"Reconnection attempt {this.Attempts}");

            try
            {
                connectAction();
                return true;
            }
            catch (Exception ex)
            {
                this._log.Warning($"Connection attempt failed: {ex.Message}");
                return false;
            }
        }
    }
}
