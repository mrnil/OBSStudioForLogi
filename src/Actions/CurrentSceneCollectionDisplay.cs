namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class CurrentSceneCollectionDisplay : PluginDynamicCommand
    {
        public static CurrentSceneCollectionDisplay Instance { get; private set; }

        private String _currentSceneCollection = "Not Connected";

        public CurrentSceneCollectionDisplay()
            : base("Current Scene Collection", "Shows current OBS scene collection", "OBS")
        {
            Instance = this;
        }

        public void UpdateSceneCollection(String sceneCollectionName)
        {
            this._currentSceneCollection = String.IsNullOrEmpty(sceneCollectionName) ? "Not Connected" : sceneCollectionName;
            this.ActionImageChanged();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                bitmapBuilder.Clear(BitmapColor.Black);
                bitmapBuilder.DrawText("Collection:");
                bitmapBuilder.DrawText(this._currentSceneCollection, BitmapColor.White, imageSize == PluginImageSize.Width90 ? 11 : 9);
                return bitmapBuilder.ToImage();
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }
    }
}
