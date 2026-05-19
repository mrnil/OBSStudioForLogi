namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class CurrentSceneCollectionDisplay : PluginDynamicCommand
    {
        public static CurrentSceneCollectionDisplay Instance { get; private set; }

        private String _currentSceneCollection = "Not Connected";

        public CurrentSceneCollectionDisplay()
            : base(displayName: "", description: "Shows current OBS scene collection", groupName: "6. Scenes")
        {
            Instance = this;
            this.AddParameter("", "", groupName: "6. Scenes");
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return String.Empty;
        }

        public void UpdateSceneCollection(String sceneCollectionName)
        {
            if (String.IsNullOrEmpty(sceneCollectionName))
            {
                PluginLog.Warning("Cannot update scene collection display - name is empty");
                return;
            }

            PluginLog.Info($"Updating scene collection display to '{sceneCollectionName}'");
            this._currentSceneCollection = sceneCollectionName;
            this.ActionImageChanged("");
        }

        public void UpdateDisplay()
        {
            this.ActionImageChanged("");
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isConnected = OBSStudioForLogiPlugin.Instance?.IsConnected ?? false;
            String displayText = isConnected ? this._currentSceneCollection : "Not Connected";
            BitmapColor backgroundColor = isConnected ? new BitmapColor(128, 57, 246) : BitmapColor.Black;
            BitmapColor textColor = isConnected ? BitmapColor.White : new BitmapColor(128, 128, 128);
            
            return ButtonImageHelper.Text(displayText, imageSize, backgroundColor, textColor);
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }
    }
}
