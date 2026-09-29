namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading.Tasks;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;
    using OBSWebsocketDotNet;

    // Runs a connection's initial state load. OBS accepts websocket connections while it is still
    // starting, and until it has loaded its scene collection it answers every request with "not
    // ready" (207). The load used to run once at connect time, so connecting to a starting OBS left
    // the input list, profiles and studio mode unloaded until something changed in OBS
    // (assessment #26). This retries while OBS says it isn't ready, for as long as the connection
    // the load was started for is still the current one.
    public class InitialStateLoader
    {
        public const Int32 NotReadyErrorCode = 207;

        private readonly IPluginLog _log;
        private readonly Func<Int32, Task> _delay;
        private readonly Int32 _retryDelayMs;
        private readonly Int32 _maxAttempts;

        public InitialStateLoader(IPluginLog log)
            : this(log, delayMs => Task.Delay(delayMs), OBSTimings.InitialStateRetryDelay, OBSTimings.InitialStateMaxAttempts)
        {
        }

        public InitialStateLoader(IPluginLog log, Func<Int32, Task> delay, Int32 retryDelayMs, Int32 maxAttempts)
        {
            this._log = log ?? throw new ArgumentNullException(nameof(log));
            this._delay = delay ?? (delayMs => Task.Delay(delayMs));
            this._retryDelayMs = retryDelayMs;
            this._maxAttempts = Math.Max(1, maxAttempts);
        }

        // Returns true once load has run to completion. Returns false if the connection closed
        // first, OBS was still not ready after the last attempt, or load failed for another
        // reason (not retried: only "not ready" is known to clear up by itself).
        public async Task<Boolean> RunAsync(Action load, Func<Boolean> isCurrentConnection)
        {
            for (Int32 attempt = 1; attempt <= this._maxAttempts; attempt++)
            {
                if (!isCurrentConnection())
                {
                    this._log.Debug("Initial state load abandoned - the connection it was for has closed");
                    return false;
                }

                try
                {
                    load();

                    if (attempt > 1)
                    {
                        this._log.Info($"Initial state loaded once OBS was ready (attempt {attempt})");
                    }

                    return true;
                }
                catch (Exception ex) when (IsNotReady(ex))
                {
                    if (attempt == this._maxAttempts)
                    {
                        break;
                    }

                    // One Info line when the wait starts; the rest at Debug so a slow OBS start
                    // doesn't fill the log.
                    String message = $"OBS is not ready for the initial state load yet (attempt {attempt}/{this._maxAttempts}) - retrying in {this._retryDelayMs}ms";
                    if (attempt == 1)
                    {
                        this._log.Info(message);
                    }
                    else
                    {
                        this._log.Debug(message);
                    }

                    await this._delay(this._retryDelayMs);
                }
                catch (Exception ex)
                {
                    this._log.Warning($"Failed to get initial state: {ex.Message}");
                    return false;
                }
            }

            this._log.Warning($"Failed to get initial state: OBS was still not ready after {this._maxAttempts} attempts");
            return false;
        }

        internal static Boolean IsNotReady(Exception ex)
        {
            return ex is ErrorResponseException response && response.ErrorCode == NotReadyErrorCode;
        }
    }
}
