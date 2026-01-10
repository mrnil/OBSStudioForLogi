namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class OBSStudioForLogiPlugin : Plugin
    {
        public static OBSStudioForLogiPlugin Instance { get; private set; }
        private OBSWebSocketManager _obsManager;
        private OBSConfigReader _configReader;

        public override Boolean UsesApplicationApiOnly => true;
        public override Boolean HasNoApplication => true;

        public OBSStudioForLogiPlugin()
        {
            Instance = this;
            PluginLog.Init(this.Log);
            PluginResources.Init(this.Assembly);
        }

        public override void Load()
        {
            PluginLog.Info("Plugin loading...");
            
            this._configReader = new OBSConfigReader();
            this._obsManager = new OBSWebSocketManager();
            
            var settings = this._configReader.ReadConfig();
            if (settings != null)
            {
                PluginLog.Info("Initiating connection to OBS");
                _ = this._obsManager.ConnectAsync(settings.GetWebSocketUrl(), settings.Password);
            }
            else
            {
                PluginLog.Warning("No valid OBS configuration found");
            }
            
            PluginLog.Info("Plugin loaded");
        }

        public override void Unload()
        {
            PluginLog.Info("Plugin unloading...");
            this._obsManager?.Dispose();
            PluginLog.Info("Plugin unloaded");
        }

        public void SwitchScene(String sceneName)
        {
            if (!this._obsManager.IsConnected)
            {
                PluginLog.Warning($"Cannot switch to scene '{sceneName}' - not connected to OBS");
                return;
            }

            this._obsManager.SetCurrentScene(sceneName);
        }
    }
}
