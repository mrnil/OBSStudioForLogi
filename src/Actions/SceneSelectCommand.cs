namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class SceneSelectCommand : PluginMultistateDynamicCommand, IObsCommand, ISceneAwareCommand, IScenesListAwareCommand
    {
        private const Int16 SCENE_UNSELECTED = 0;
        private const Int16 SCENE_SELECTED = 1;

        public static SceneSelectCommand Instance { get; private set; }

        public SceneSelectCommand()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.Description = "Switches to a specific scene in OBS Studio";
            this.GroupName = "7. Scenes###Available Scenes";
            this.AddState("", "Scene unselected");
            this.AddState("", "Scene selected");
        }

        protected override Boolean OnLoad()
        {
            this.IsEnabled = false;
            this.ResetParameters(new String[0]);
            return true;
        }

        protected override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.SwitchScene(actionParameter);
        }

        private void ResetParameters(String[] scenes)
        {
            this.RemoveAllParameters();

            // The current scene is tracked state, not an OBS request.
            String currentScene = OBSStudioForLogiPlugin.Instance?.GetCurrentScene() ?? String.Empty;

            PluginLog.Debug($"Adding {scenes.Length} scenes");

            foreach (String scene in scenes)
            {
                this.AddParameter(scene, scene, this.GroupName).Description = $"Switch to scene \"{scene}\"";
                this.SetCurrentState(scene, scene == currentScene ? SCENE_SELECTED : SCENE_UNSELECTED);
            }

            this.ParametersChanged();
            this.ActionImageChanged();
        }

        // Use the list OBSWebSocketManager already loaded - this used to discard it and query OBS
        // for the same list again.
        public void OnScenesChanged(String[] scenes)
        {
            this.ResetParameters(scenes ?? new String[0]);
        }

        public void OnSceneChanged(String sceneName)
        {
            this.OnCurrentSceneChanged(OBSStudioForLogiPlugin.Instance?.GetCurrentScene() ?? String.Empty, sceneName);
        }

        private void OnCurrentSceneChanged(String oldScene, String newScene)
        {
            if (!String.IsNullOrEmpty(oldScene))
            {
                this.SetCurrentState(oldScene, SCENE_UNSELECTED);
            }

            if (!String.IsNullOrEmpty(newScene))
            {
                this.SetCurrentState(newScene, SCENE_SELECTED);
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
            this.ResetParameters(new String[0]);
        }

        protected override BitmapImage GetCommandImage(String actionParameter, Int32 stateIndex, PluginImageSize imageSize)
        {
            Boolean isConnected = OBSStudioForLogiPlugin.Instance?.IsConnected ?? false;

            if (!isConnected)
            {
                return ButtonImageHelper.Icon("SceneUnselected.svg");
            }

            Boolean isSelected = stateIndex == SCENE_SELECTED;
            return ButtonImageHelper.Icon(isSelected ? "SceneSelected.svg" : "SceneUnselected.svg");
        }
    }
}
