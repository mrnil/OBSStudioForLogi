namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class CurrentSceneCollectionDisplay : PluginDynamicCommand
    {
        public static CurrentSceneCollectionDisplay Instance { get; private set; }

        private String _currentSceneCollection = "Not Connected";
        private readonly ActionImageStore<TextImageData> imageStore;

        public CurrentSceneCollectionDisplay()
            : base(displayName: "Current Scene Collection", description: "Shows current OBS scene collection", groupName: "5. Scenes")
        {
            Instance = this;
            this.imageStore = new ActionImageStore<TextImageData>(new TextImageFactory());
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
            Boolean isConnected = OBSStudioForLogiPlugin.Instance?.IsConnected ?? false;
            String displayText = isConnected ? this._currentSceneCollection : "Not Connected";
            BitmapColor backgroundColor = isConnected ? new BitmapColor(128, 57, 246) : BitmapColor.Black;
            BitmapColor textColor = isConnected ? BitmapColor.White : new BitmapColor(128, 128, 128);
            
            var imageData = new TextImageData
            {
                Id = "scene_collection_display",
                DisplayText = displayText,
                BackgroundColor = backgroundColor,
                TextColor = textColor
            };
            
            this.imageStore.UpdateImage(imageData.Id, imageData);
            
            if (this.imageStore.TryGetImage(imageData.Id, imageSize, out var image))
            {
                return image;
            }
            
            return ButtonTextRenderer.RenderText(displayText, imageSize, backgroundColor, textColor);
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }
    }
}
