namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ScenesDynamicFolder : PluginDynamicFolder, IObsCommand, ISceneAwareCommand, IScenesListAwareCommand
    {
        private const Int16 SCENE_UNSELECTED = 0;
        private const Int16 SCENE_SELECTED = 1;

        public static ScenesDynamicFolder Instance { get; private set; }

        private String[] _scenes = new String[0];
        private String _currentScene = String.Empty;

        public ScenesDynamicFolder()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.DisplayName = "OBS Scenes";
            this.GroupName = "7. Scenes";
            this.Description = "Folder of scenes from the current collection";
        }

        public override PluginDynamicFolderNavigation GetNavigationArea(DeviceType _)
        {
            return PluginDynamicFolderNavigation.ButtonArea;
        }

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType)
        {
            return this._scenes.Select(scene => this.CreateCommandName(scene));
        }

        public void UpdateScenes(String[] scenes, String currentScene)
        {
            this._scenes = scenes ?? new String[0];
            this._currentScene = currentScene ?? String.Empty;
            PluginLog.Info($"ScenesDynamicFolder updated with {this._scenes.Length} scenes, current: '{this._currentScene}'");
            this.ButtonActionNamesChanged();
        }

        public void OnScenesChanged(String[] scenes)
        {
            var currentScene = OBSStudioForLogiPlugin.Instance?.GetCurrentScene() ?? String.Empty;
            this.UpdateScenes(scenes, currentScene);
        }

        public void OnSceneChanged(String sceneName)
        {
            this.OnCurrentSceneChanged(sceneName);
        }

        public void OnConnected()
        {
            this.ButtonActionNamesChanged();
        }

        private void OnCurrentSceneChanged(String sceneName)
        {
            var oldScene = this._currentScene;
            this._currentScene = sceneName ?? String.Empty;
            
            if (!String.IsNullOrEmpty(oldScene) && oldScene != this._currentScene)
            {
                this.CommandImageChanged(oldScene);
            }
            
            if (!String.IsNullOrEmpty(this._currentScene))
            {
                this.CommandImageChanged(this._currentScene);
            }
        }

        public void OnDisconnected()
        {
            this._scenes = new String[0];
            this._currentScene = String.Empty;
            this.ButtonActionNamesChanged();
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isSelected = actionParameter == this._currentScene;
            return ButtonImageHelper.Icon(isSelected ? "ScenesSelected.svg" : "ScenesUnselected.svg");
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.SwitchScene(actionParameter);
        }
    }
}
