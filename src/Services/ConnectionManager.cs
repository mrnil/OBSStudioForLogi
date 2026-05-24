namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading.Tasks;

    public class ConnectionManager
    {
        private readonly OBSWebSocketManager _obsManager;
        private readonly OBSConfigReader _configReader;
        private readonly OBSLifecycleManager _lifecycleManager;

        public ConnectionManager(OBSWebSocketManager obsManager, OBSConfigReader configReader, OBSLifecycleManager lifecycleManager)
        {
            this._obsManager = obsManager;
            this._configReader = configReader;
            this._lifecycleManager = lifecycleManager;
        }

        public Boolean IsConnected => this._obsManager?.IsConnected ?? false;

        public async Task ConnectAsync()
        {
            PluginLog.Info("Attempting connection to OBS");
            
            var settings = this._configReader.ReadConfig();
            if (settings == null)
            {
                PluginLog.Warning("No valid OBS configuration found");
                return;
            }

            PluginLog.Info($"Waiting for OBS WebSocket port {settings.Port} to be ready");
            var portReady = await this._lifecycleManager.WaitForPortAsync("127.0.0.1", settings.Port);
            
            if (portReady)
            {
                await Task.Delay(2000);
                PluginLog.Info("Initiating connection to OBS");
                await this._obsManager.ConnectAsync(settings.GetWebSocketUrl(), settings.Password);
            }
            else
            {
                PluginLog.Error("OBS WebSocket port did not become available");
            }
        }

        public void Disconnect()
        {
            this._obsManager?.Disconnect();
        }

        public void Dispose()
        {
            this._obsManager?.Dispose();
        }
    }
}
