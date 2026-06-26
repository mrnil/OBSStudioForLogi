namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class CurrentSceneDisplay : PluginDynamicCommand, IObsCommand, ISceneAwareCommand
    {
        public static CurrentSceneDisplay Instance { get; private set; }

        private String _currentScene = "Not Connected";

        public CurrentSceneDisplay()
            : base(displayName: "Current Scene", description: "Shows current OBS scene", groupName: "7. Scenes")
        {
            Instance = this;
            this.IsWidget = true;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.AddParameter("", "", groupName: "7. Scenes");
        }

        public void OnSceneChanged(String sceneName)
        {
            this.UpdateScene(sceneName);
        }

        public void OnConnected()
        {
            this.ActionImageChanged("");
        }

        public void OnDisconnected()
        {
            this._currentScene = "Not Connected";
            this.ActionImageChanged("");
        }

        private void UpdateScene(String sceneName)
        {
            if (String.IsNullOrEmpty(sceneName))
            {
                PluginLog.Warning("Cannot update scene display - scene name is empty");
                return;
            }

            PluginLog.Debug($"Updating scene display to '{sceneName}'");
            this._currentScene = sceneName;
            this.ActionImageChanged("");
        }

        public void UpdateDisplay()
        {
            this.ActionImageChanged("");
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isConnected = OBSStudioForLogiPlugin.Instance?.IsConnected ?? false;
            String displayText = isConnected ? this._currentScene : "Not Connected";
            BitmapColor backgroundColor = isConnected ? new BitmapColor(57, 180, 120) : BitmapColor.Black;
            BitmapColor textColor = isConnected ? BitmapColor.White : new BitmapColor(128, 128, 128);
            
            return ButtonTextRenderer.RenderText(displayText, imageSize, backgroundColor, textColor);
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }
    }
}
