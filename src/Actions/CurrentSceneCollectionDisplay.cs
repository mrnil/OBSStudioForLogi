namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class CurrentSceneCollectionDisplay : PluginDynamicCommand, IObsCommand, ISceneCollectionAwareCommand
    {
        public static CurrentSceneCollectionDisplay Instance { get; private set; }

        private String _currentSceneCollection = "Not Connected";

        public CurrentSceneCollectionDisplay()
            : base(displayName: "Current Collection", description: "Shows current OBS scene collection", groupName: "7. Scenes")
        {
            Instance = this;
            this.IsWidget = true;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.AddParameter("", "", groupName: "7. Scenes");
        }

        public void OnSceneCollectionChanged(String oldSceneCollection, String newSceneCollection)
        {
            this.UpdateSceneCollection(newSceneCollection);
        }

        public void OnConnected()
        {
            this.ActionImageChanged("");
        }

        public void OnDisconnected()
        {
            this._currentSceneCollection = "Not Connected";
            this.ActionImageChanged("");
        }

        private void UpdateSceneCollection(String sceneCollectionName)
        {
            if (String.IsNullOrEmpty(sceneCollectionName))
            {
                PluginLog.Warning("Cannot update scene collection display - name is empty");
                return;
            }

            PluginLog.Debug($"Updating scene collection display to '{sceneCollectionName}'");
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
            
            return ButtonTextRenderer.RenderText(displayText, imageSize, backgroundColor, textColor);
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }
    }
}
