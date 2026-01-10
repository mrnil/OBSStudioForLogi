namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class CurrentSceneDisplay : PluginDynamicCommand
    {
        public static CurrentSceneDisplay Instance { get; private set; }

        private String _currentScene = "Not Connected";

        public CurrentSceneDisplay()
            : base("Current Scene", "Shows current OBS scene", "OBS")
        {
            Instance = this;
        }

        public void UpdateScene(String sceneName)
        {
            this._currentScene = String.IsNullOrEmpty(sceneName) ? "Not Connected" : sceneName;
            this.ActionImageChanged();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                bitmapBuilder.Clear(BitmapColor.Black);
                bitmapBuilder.DrawText("Scene:");
                bitmapBuilder.DrawText(this._currentScene, BitmapColor.White, imageSize == PluginImageSize.Width90 ? 11 : 9);
                return bitmapBuilder.ToImage();
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }
    }
}
