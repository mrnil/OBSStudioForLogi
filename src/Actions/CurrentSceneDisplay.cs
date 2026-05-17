namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class CurrentSceneDisplay : PluginDynamicCommand
    {
        public static CurrentSceneDisplay Instance { get; private set; }

        private String _currentScene = "Not Connected";
        private readonly ActionImageStore<TextImageData> imageStore;

        public CurrentSceneDisplay()
            : base(displayName: "Current Scene", description: "Shows current OBS scene", groupName: "5. Scenes")
        {
            Instance = this;
            this.imageStore = new ActionImageStore<TextImageData>(new TextImageFactory());
            this.AddParameter("", "", groupName: "5. Scenes");
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return String.Empty;
        }

        public void UpdateScene(String sceneName)
        {
            if (String.IsNullOrEmpty(sceneName))
            {
                PluginLog.Warning("Cannot update scene display - scene name is empty");
                return;
            }

            PluginLog.Info($"Updating scene display to '{sceneName}'");
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
            
            var imageData = new TextImageData
            {
                Id = "scene_display",
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
