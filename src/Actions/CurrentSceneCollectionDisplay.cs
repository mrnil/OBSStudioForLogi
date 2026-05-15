namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class CurrentSceneCollectionDisplay : PluginDynamicCommand
    {
        public static CurrentSceneCollectionDisplay Instance { get; private set; }

        private String _currentSceneCollection = "Not Connected";

        public CurrentSceneCollectionDisplay()
            : base(displayName: "Current Scene Collection", description: "Shows current OBS scene collection", groupName: "5. Scenes")
        {
            Instance = this;
            this.AddParameter("", "", groupName: "5. Scenes");
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
            if (!OBSStudioForLogiPlugin.Instance?.IsConnected ?? true)
            {
                return ButtonTextRenderer.RenderText(
                    "Not Connected",
                    imageSize,
                    BitmapColor.Black,
                    new BitmapColor(128, 128, 128));
            }
            return ButtonTextRenderer.RenderText(
                this._currentSceneCollection,
                imageSize,
                new BitmapColor(128, 57, 246),
                BitmapColor.White);
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }
    }
}
