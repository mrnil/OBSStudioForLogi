namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ScenesDynamicFolder : PluginDynamicFolder
    {
        private const Int16 SCENE_UNSELECTED = 0;
        private const Int16 SCENE_SELECTED = 1;

        public static ScenesDynamicFolder Instance { get; private set; }

        private String[] _scenes = new String[0];
        private String _currentScene = String.Empty;

        public ScenesDynamicFolder()
        {
            Instance = this;
            this.DisplayName = "OBS Scenes";
            this.GroupName = "OBS";
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

        public void OnCurrentSceneChanged(String sceneName)
        {
            this._currentScene = sceneName ?? String.Empty;
            this.ButtonActionNamesChanged();
        }

        public override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return actionParameter;
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var isSelected = actionParameter == this._currentScene;
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                bitmapBuilder.Clear(isSelected ? BitmapColor.White : BitmapColor.Black);
                bitmapBuilder.DrawText(actionParameter, BitmapColor.Black, imageSize == PluginImageSize.Width90 ? 12 : 9);
                return bitmapBuilder.ToImage();
            }
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.SwitchScene(actionParameter);
        }
    }
}
