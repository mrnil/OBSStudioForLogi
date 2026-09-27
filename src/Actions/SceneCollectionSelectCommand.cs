namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class SceneCollectionSelectCommand : PluginMultistateDynamicCommand, IObsCommand, ISceneCollectionAwareCommand, ISceneCollectionsListAwareCommand
    {
        private const Int16 SCENE_COLLECTION_UNSELECTED = 0;
        private const Int16 SCENE_COLLECTION_SELECTED = 1;

        public static SceneCollectionSelectCommand Instance { get; private set; }

        public SceneCollectionSelectCommand()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.Description = "Switches to a specific scene collection in OBS Studio";
            this.GroupName = "7. Scenes###Available Collections";
            this.AddState("", "Scene collection unselected");
            this.AddState("", "Scene collection selected");
        }

        protected override Boolean OnLoad()
        {
            this.IsEnabled = false;
            this.ResetParameters(new String[0], String.Empty);
            return true;
        }

        protected override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.SwitchSceneCollection(actionParameter);
        }

        private void ResetParameters(String[] sceneCollections, String currentSceneCollection)
        {
            this.RemoveAllParameters();

            PluginLog.Debug($"Adding {sceneCollections.Length} scene collections");

            foreach (String sceneCollection in sceneCollections)
            {
                this.AddParameter(sceneCollection, $"{sceneCollection} Collection", this.GroupName).Description = $"Switch to scene collection \"{sceneCollection}\"";
                this.SetCurrentState(sceneCollection, sceneCollection == currentSceneCollection ? SCENE_COLLECTION_SELECTED : SCENE_COLLECTION_UNSELECTED);
            }

            this.ParametersChanged();
            this.ActionImageChanged();
        }

        // OBSWebSocketManager loads the scene collection list once per connection and pushes it
        // here, rather than every collection-aware command querying OBS itself.
        public void OnSceneCollectionsChanged(String[] sceneCollections, String currentSceneCollection)
        {
            this.ResetParameters(sceneCollections ?? new String[0], currentSceneCollection ?? String.Empty);
        }

        public void OnSceneCollectionChanged(String oldSceneCollection, String newSceneCollection)
        {
            this.OnCurrentSceneCollectionChanged(oldSceneCollection, newSceneCollection);
        }

        private void OnCurrentSceneCollectionChanged(String oldSceneCollection, String newSceneCollection)
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
        }

        public void OnDisconnected()
        {
            this.IsEnabled = false;
            this.ResetParameters(new String[0], String.Empty);
        }

        protected override BitmapImage GetCommandImage(String actionParameter, Int32 stateIndex, PluginImageSize imageSize)
        {
            Boolean isConnected = OBSStudioForLogiPlugin.Instance?.IsConnected ?? false;
            
            if (!isConnected)
            {
                return ButtonImageHelper.Icon("SceneCollectionUnselected.svg");
            }
            
            Boolean isSelected = stateIndex == SCENE_COLLECTION_SELECTED;
            return ButtonImageHelper.Icon(isSelected ? "SceneCollectionSelected.svg" : "SceneCollectionUnselected.svg");
        }
    }
}
