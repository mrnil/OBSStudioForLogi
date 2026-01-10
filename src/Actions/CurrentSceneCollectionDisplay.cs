namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class CurrentSceneCollectionDisplay : PluginDynamicCommand
    {
        public static CurrentSceneCollectionDisplay Instance { get; private set; }

        private String _currentSceneCollection = String.Empty;

        public CurrentSceneCollectionDisplay()
            : base(displayName: "Current Scene Collection", description: "Shows current OBS scene collection", groupName: "OBS")
        {
            Instance = this;
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return "Collection";
        }

        public void UpdateSceneCollection(String sceneCollectionName)
        {
            if (String.IsNullOrEmpty(sceneCollectionName))
                return;

            this._currentSceneCollection = sceneCollectionName;
            this.ActionImageChanged();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                bitmapBuilder.Clear(BitmapColor.Black);
                bitmapBuilder.DrawText("Collection", BitmapColor.White, 8);
                bitmapBuilder.DrawText(this._currentSceneCollection, BitmapColor.White, imageSize == PluginImageSize.Width90 ? 14 : 11);
                return bitmapBuilder.ToImage();
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }
    }
}
