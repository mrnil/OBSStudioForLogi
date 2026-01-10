namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ScenesDynamicFolder : PluginDynamicFolder
    {
        public static ScenesDynamicFolder Instance { get; private set; }

        private String[] _scenes = new String[0];

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

        public void UpdateScenes(String[] scenes)
        {
            this._scenes = scenes ?? new String[0];
            this.ButtonActionNamesChanged();
        }

        public override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return actionParameter;
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.SwitchScene(actionParameter);
        }
    }
}
