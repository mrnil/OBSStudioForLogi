namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;
    using Loupedeck.OBSStudioForLogiPlugin.Models;

    // Owns the connection lifecycle and is the only place that retries (assessment #29). Every
    // attempt goes through ConnectAsync, which reads the connection settings afresh - OBS's own
    // config for a local OBS, the plugin config for a remote one - so a password or port changed
    // while the plugin is running is picked up by the next attempt.
    //
    // Two kinds of retry:
    // - OBS isn't up yet (local port closed): retry after a fixed delay.
    // - The connection dropped, or an attempt failed (OBS refused it, wrong password): retry with
    //   exponential backoff, reset once connected.
    // Both stop when Disconnect or Dispose is called, until the next ConnectAsync.
    public class ConnectionManager
    {
        private readonly OBSWebSocketManager _obsManager;
        private readonly OBSConfigReader _configReader;
        private readonly OBSLifecycleManager _lifecycleManager;
        private readonly TimeSpan _retryDelay;
        private readonly TimeSpan _connectionDelay;
        private readonly ReconnectionStrategy _reconnection;
        private PluginConfig _pluginConfig;
        private Int32 _connectAttempts;
        private CancellationTokenSource _pendingRetry;
        private volatile Boolean _shouldReconnect;
        private volatile Boolean _disposed;

        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler WebSocketServerDisabled;

        public ConnectionManager(OBSWebSocketManager obsManager, OBSConfigReader configReader, OBSLifecycleManager lifecycleManager)
            : this(obsManager, configReader, lifecycleManager, TimeSpan.FromMilliseconds(OBSTimings.ConnectRetryDelay))
        {
        }

        public ConnectionManager(OBSWebSocketManager obsManager, OBSConfigReader configReader, OBSLifecycleManager lifecycleManager, TimeSpan retryDelay)
            : this(obsManager, configReader, lifecycleManager, retryDelay,
                TimeSpan.FromMilliseconds(OBSTimings.ConnectionDelay), new ReconnectionStrategy(new PluginLogAdapter()))
        {
        }

        // connectionDelay and reconnection let tests run attempts without the real delays.
        internal ConnectionManager(OBSWebSocketManager obsManager, OBSConfigReader configReader, OBSLifecycleManager lifecycleManager,
            TimeSpan retryDelay, TimeSpan connectionDelay, ReconnectionStrategy reconnection)
        {
            this._retryDelay = retryDelay;
            this._connectionDelay = connectionDelay;
            this._reconnection = reconnection;
            this._obsManager = obsManager;
            this._configReader = configReader;
            this._lifecycleManager = lifecycleManager;

            this._obsManager.ConnectionEstablished += this.OnConnectionEstablished;
            this._obsManager.ConnectionLost += this.OnConnectionLost;
        }

        public Boolean IsConnected => this._obsManager?.IsConnected ?? false;

        // True while a ConnectAsync call is still waiting for the port, the connection delay or
        // the websocket start, and while the websocket handshake that follows is in progress.
        public Boolean IsConnecting => Volatile.Read(ref this._connectAttempts) > 0 || (this._obsManager?.IsConnecting ?? false);

        public Boolean IsWebSocketServerDisabled => this._configReader?.IsServerDisabled ?? false;

        private void OnWebSocketServerDisabled()
        {
            this.WebSocketServerDisabled?.Invoke(this, EventArgs.Empty);
        }

        private void OnConnectionEstablished(Object sender, EventArgs e)
        {
            this.CancelPendingRetry();
            this._reconnection.Reset();
            this.Connected?.Invoke(this, EventArgs.Empty);
        }

        // Raised for a dropped connection and for a failed attempt alike. A deliberate Disconnect
        // raises it too, later and on another thread, so _shouldReconnect decides whether to retry.
        private void OnConnectionLost(Object sender, EventArgs e)
        {
            this.Disconnected?.Invoke(this, EventArgs.Empty);

            if (!this._shouldReconnect || this._disposed)
            {
                return;
            }

            Int32 delayMs = this._reconnection.BeginAttempt();
            PluginLog.Info($"Connection to OBS lost - reconnecting in {delayMs}ms");
            this.ScheduleRetry(TimeSpan.FromMilliseconds(delayMs));
        }

        public void SetPluginConfig(PluginConfig config)
        {
            this._pluginConfig = config;
        }

        public OBSConnectionSettings GetLocalSettings()
        {
            return this._configReader.ReadConfig();
        }

        // User-initiated reconnect. Pressing Reconnect while connected used to tear down a working
        // connection, and pressing it again while the first attempt was still waiting on the port
        // started a second, overlapping attempt - so both cases are ignored.
        public Task ReconnectAsync()
        {
            if (this.IsConnected)
            {
                PluginLog.Info("Reconnect ignored - already connected to OBS");
                return Task.CompletedTask;
            }

            if (this.IsConnecting)
            {
                PluginLog.Info("Reconnect ignored - a connection attempt is already in progress");
                return Task.CompletedTask;
            }

            return this.ConnectAsync();
        }

        public async Task ConnectAsync()
        {
            // This attempt supersedes any retry that was waiting to run.
            this.CancelPendingRetry();
            this._shouldReconnect = true;
            Interlocked.Increment(ref this._connectAttempts);
            try
            {
                await this.ConnectCoreAsync();
            }
            finally
            {
                Interlocked.Decrement(ref this._connectAttempts);
            }
        }

        private async Task ConnectCoreAsync()
        {
            PluginLog.Debug("Attempting connection to OBS");

            OBSConnectionSettings settings;

            if (this._pluginConfig != null && !this._pluginConfig.UseLocalObs)
            {
                settings = new OBSConnectionSettings
                {
                    IpAddress = this._pluginConfig.RemoteIpAddress,
                    Port = this._pluginConfig.RemotePort,
                    Password = this._pluginConfig.RemotePassword
                };
                PluginLog.Info($"Using remote OBS connection: {settings.IpAddress}:{settings.Port}");

                await Task.Delay(this._connectionDelay);
                PluginLog.Info($"Initiating connection to remote OBS at {settings.GetWebSocketUrl()}");
                await this._obsManager.ConnectAsync(settings.GetWebSocketUrl(), settings.Password);
            }
            else
            {
                PluginLog.Debug($"Using local OBS connection (pluginConfig is {(this._pluginConfig == null ? "null" : "UseLocalObs=" + this._pluginConfig.UseLocalObs)})");
                settings = this._configReader.ReadConfig();
                if (settings == null)
                {
                    if (this._configReader.IsServerDisabled)
                    {
                        PluginLog.Warning("OBS WebSocket server is disabled in config");
                        this.OnWebSocketServerDisabled();
                    }
                    else
                    {
                        PluginLog.Warning("No valid OBS configuration found");
                    }
                    return;
                }

                PluginLog.Debug($"Waiting for local OBS WebSocket port {settings.Port} to be ready");
                Boolean portReady = await this._lifecycleManager.WaitForPortAsync("127.0.0.1", settings.Port);

                if (portReady)
                {
                    await Task.Delay(this._connectionDelay);
                    PluginLog.Info($"Initiating connection to local OBS at {settings.GetWebSocketUrl()}");
                    await this._obsManager.ConnectAsync(settings.GetWebSocketUrl(), settings.Password);
                }
                else
                {
                    // OBS isn't running (yet). Without this the plugin stayed disconnected until
                    // the user pressed Reconnect.
                    PluginLog.Warning($"OBS WebSocket port did not become available - retrying in {this._retryDelay.TotalSeconds:0}s");
                    this.ScheduleRetry(this._retryDelay);
                }
            }
        }

        public void Disconnect()
        {
            this._shouldReconnect = false;
            this.CancelPendingRetry();
            this._obsManager?.Disconnect();
        }

        private void ScheduleRetry(TimeSpan delay)
        {
            if (this._disposed)
            {
                return;
            }

            CancellationTokenSource retry = new CancellationTokenSource();
            Interlocked.Exchange(ref this._pendingRetry, retry)?.Cancel();
            _ = this.RetryAfterDelayAsync(delay, retry.Token);
        }

        private async Task RetryAfterDelayAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(delay, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            // A Disconnected for an earlier connection can arrive after a newer attempt has
            // started (the library reports it on another thread), so its retry may find the
            // plugin connected, or connecting, by the time it runs.
            if (this.IsConnected || this.IsConnecting)
            {
                PluginLog.Debug("Retry skipped - already connected or connecting to OBS");
                return;
            }

            try
            {
                await this.ConnectAsync();
            }
            catch (Exception ex)
            {
                // Nothing awaits a retry, so an attempt that throws (an invalid remote address, say)
                // would otherwise end the retries silently.
                PluginLog.Warning($"Connection attempt failed: {ex.Message}");
                if (this._shouldReconnect && !this._disposed)
                {
                    this.ScheduleRetry(TimeSpan.FromMilliseconds(this._reconnection.BeginAttempt()));
                }
            }
        }

        private void CancelPendingRetry()
        {
            Interlocked.Exchange(ref this._pendingRetry, null)?.Cancel();
        }

        public void Dispose()
        {
            this._disposed = true;
            this._shouldReconnect = false;
            this.CancelPendingRetry();
            this._obsManager.ConnectionEstablished -= this.OnConnectionEstablished;
            this._obsManager.ConnectionLost -= this.OnConnectionLost;
            this._obsManager?.Dispose();
        }
    }
}
