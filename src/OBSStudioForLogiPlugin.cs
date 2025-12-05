namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class OBSStudioForLogiPlugin : Plugin
    {
        public static OBSStudioForLogiPlugin Instance { get; private set; }
        private OBSWebSocketManager _obsManager;

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
            this._obsManager = new OBSWebSocketManager();
            _ = this._obsManager.ConnectAsync("ws://localhost:4455", "");
        }

        public override void Unload()
        {
            this._obsManager?.Dispose();
        }

        public void SwitchScene(String sceneName)
        {
            if (!this._obsManager.IsConnected)
                return;

            this._obsManager.SetCurrentScene(sceneName);
        }
    }
}
