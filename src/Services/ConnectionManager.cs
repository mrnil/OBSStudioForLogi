namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading.Tasks;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;
    using Loupedeck.OBSStudioForLogiPlugin.Models;

    public class ConnectionManager
    {
        private readonly OBSWebSocketManager _obsManager;
        private readonly OBSConfigReader _configReader;
        private readonly OBSLifecycleManager _lifecycleManager;
        private PluginConfig _pluginConfig;

        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public ConnectionManager(OBSWebSocketManager obsManager, OBSConfigReader configReader, OBSLifecycleManager lifecycleManager)
        {
            this._obsManager = obsManager;
            this._configReader = configReader;
            this._lifecycleManager = lifecycleManager;

            this._obsManager.ConnectionEstablished += this.OnConnectionEstablished;
            this._obsManager.ConnectionLost += this.OnConnectionLost;
        }

        public Boolean IsConnected => this._obsManager?.IsConnected ?? false;

        private void OnConnectionEstablished(Object sender, EventArgs e)
        {
            this.Connected?.Invoke(this, EventArgs.Empty);
        }

        private void OnConnectionLost(Object sender, EventArgs e)
        {
            this.Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public void SetPluginConfig(PluginConfig config)
        {
            this._pluginConfig = config;
        }

        public OBSConnectionSettings GetLocalSettings()
        {
            return this._configReader.ReadConfig();
        }

        public async Task ConnectAsync()
        {
            PluginLog.Info("Attempting connection to OBS");

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

                await Task.Delay(OBSTimings.ConnectionDelay);
                PluginLog.Info($"Initiating connection to remote OBS at {settings.GetWebSocketUrl()}");
                await this._obsManager.ConnectAsync(settings.GetWebSocketUrl(), settings.Password);
            }
            else
            {
                PluginLog.Info($"Using local OBS connection (pluginConfig is {(this._pluginConfig == null ? "null" : "UseLocalObs=" + this._pluginConfig.UseLocalObs)})");
                settings = this._configReader.ReadConfig();
                if (settings == null)
                {
                    PluginLog.Warning("No valid OBS configuration found");
                    return;
                }

                PluginLog.Info($"Waiting for local OBS WebSocket port {settings.Port} to be ready");
                var portReady = await this._lifecycleManager.WaitForPortAsync("127.0.0.1", settings.Port);

                if (portReady)
                {
                    await Task.Delay(OBSTimings.ConnectionDelay);
                    PluginLog.Info($"Initiating connection to local OBS at {settings.GetWebSocketUrl()}");
                    await this._obsManager.ConnectAsync(settings.GetWebSocketUrl(), settings.Password);
                }
                else
                {
                    PluginLog.Error("OBS WebSocket port did not become available");
                }
            }
        }

        public void Disconnect()
        {
            this._obsManager?.Disconnect();
        }

        public void Dispose()
        {
            this._obsManager.ConnectionEstablished -= this.OnConnectionEstablished;
            this._obsManager.ConnectionLost -= this.OnConnectionLost;
            this._obsManager?.Dispose();
        }
    }
}
