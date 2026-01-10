namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class SceneCollectionSelectCommand : PluginMultistateDynamicCommand
    {
        private const Int16 SCENE_COLLECTION_UNSELECTED = 0;
        private const Int16 SCENE_COLLECTION_SELECTED = 1;

        public static SceneCollectionSelectCommand Instance { get; private set; }

        public SceneCollectionSelectCommand()
        {
            Instance = this;
            this.Description = "Switches to a specific scene collection in OBS Studio";
            this.GroupName = "Scene Collections";
            this.AddState("", "Scene collection unselected");
            this.AddState("", "Scene collection selected");
        }

        protected override Boolean OnLoad()
        {
            this.IsEnabled = false;
            this.ResetParameters(false);
            return true;
        }

        protected override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.SwitchSceneCollection(actionParameter);
        }

        private void ResetParameters(Boolean readContent)
        {
            this.RemoveAllParameters();

            if (readContent)
            {
                var sceneCollections = OBSStudioForLogiPlugin.Instance?.GetSceneCollectionList() ?? new String[0];
                var currentSceneCollection = OBSStudioForLogiPlugin.Instance?.CurrentSceneCollection ?? String.Empty;

                PluginLog.Info($"Adding {sceneCollections.Length} scene collections");

                foreach (var sceneCollection in sceneCollections)
                {
                    this.AddParameter(sceneCollection, sceneCollection, this.GroupName).Description = $"Switch to scene collection \"{sceneCollection}\"";
                    this.SetCurrentState(sceneCollection, sceneCollection == currentSceneCollection ? SCENE_COLLECTION_SELECTED : SCENE_COLLECTION_UNSELECTED);
                }
            }

            this.ParametersChanged();
            this.ActionImageChanged();
        }

        public void OnSceneCollectionsChanged()
        {
            this.ResetParameters(true);
        }

        public void OnCurrentSceneCollectionChanged(String oldSceneCollection, String newSceneCollection)
        {
            if (!String.IsNullOrEmpty(oldSceneCollection))
            {
                this.SetCurrentState(oldSceneCollection, SCENE_COLLECTION_UNSELECTED);
            }

            if (!String.IsNullOrEmpty(newSceneCollection))
            {
                this.SetCurrentState(newSceneCollection, SCENE_COLLECTION_SELECTED);
            }

            this.ActionImageChanged();
        }

        public void OnConnected()
        {
            this.IsEnabled = true;
            this.ResetParameters(true);
        }

        public void OnDisconnected()
        {
            this.IsEnabled = false;
            this.ResetParameters(false);
        }

        protected override BitmapImage GetCommandImage(String actionParameter, Int32 stateIndex, PluginImageSize imageSize)
        {
            // Return null to use default icon for now
            return null;
        }
    }
}
