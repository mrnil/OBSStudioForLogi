namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class OBSStudioForLogiPlugin : Plugin
    {
        public static OBSStudioForLogiPlugin Instance { get; private set; }
        private OBSWebSocketManager _obsManager;
        private OBSConnectionSettings _settings;

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
            this._settings = new OBSConnectionSettings();
            this.LoadSettings();
            
            this._obsManager = new OBSWebSocketManager();
            _ = this._obsManager.ConnectAsync(this._settings.GetWebSocketUrl(), this._settings.Password);
        }

        public override void Unload()
        {
            this._obsManager?.Dispose();
        }

        private void LoadSettings()
        {
            if (this.TryGetPluginSetting("IpAddress", out var ip))
                this._settings.IpAddress = ip;
            if (this.TryGetPluginSetting("Port", out var port) && Int32.TryParse(port, out var portNum))
                this._settings.Port = portNum;
            if (this.TryGetPluginSetting("Password", out var pwd))
                this._settings.Password = pwd;
        }

        private void SaveSettings()
        {
            this.SetPluginSetting("IpAddress", this._settings.IpAddress);
            this.SetPluginSetting("Port", this._settings.Port.ToString());
            this.SetPluginSetting("Password", this._settings.Password);
        }

        public void UpdateSettings(String ipAddress, Int32 port, String password)
        {
            this._settings.IpAddress = ipAddress;
            this._settings.Port = port;
            this._settings.Password = password;
            this.SaveSettings();
            
            this._obsManager?.Disconnect();
            _ = this._obsManager.ConnectAsync(this._settings.GetWebSocketUrl(), this._settings.Password);
        }

        public void SwitchScene(String sceneName)
        {
            if (!this._obsManager.IsConnected)
                return;

            this._obsManager.SetCurrentScene(sceneName);
        }
    }
}
