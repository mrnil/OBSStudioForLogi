namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class CurrentSceneDisplay : PluginDynamicCommand
    {
        public static CurrentSceneDisplay Instance { get; private set; }

        private String _currentScene = String.Empty;

        public CurrentSceneDisplay()
            : base(displayName: "Current Scene", description: "Shows current OBS scene", groupName: "1. OBS")
        {
            Instance = this;
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return "Scene";
        }

        public void UpdateScene(String sceneName)
        {
            if (String.IsNullOrEmpty(sceneName))
                return;

            this._currentScene = sceneName;
            this.ActionImageChanged();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                bitmapBuilder.Clear(BitmapColor.Black);
                bitmapBuilder.DrawText("Scene", BitmapColor.White, 8);
                bitmapBuilder.DrawText(this._currentScene, BitmapColor.White, imageSize == PluginImageSize.Width90 ? 14 : 11);
                return bitmapBuilder.ToImage();
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }
    }
}
