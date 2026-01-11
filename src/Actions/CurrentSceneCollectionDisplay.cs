namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class CurrentSceneCollectionDisplay : PluginDynamicCommand
    {
        public static CurrentSceneCollectionDisplay Instance { get; private set; }

        private String _currentSceneCollection = "Collection";

        public CurrentSceneCollectionDisplay()
            : base(displayName: "Current Scene Collection", description: "Shows current OBS scene collection", groupName: "1. OBS")
        {
            Instance = this;
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return this._currentSceneCollection;
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
            this.ActionImageChanged();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return null;
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }
    }
}
