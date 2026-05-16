namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SceneAudioSourcesDynamicFolder : AudioInputDynamicFolderBase
    {
        public static SceneAudioSourcesDynamicFolder Instance { get; private set; }

        private String _currentScene = String.Empty;

        public SceneAudioSourcesDynamicFolder()
        {
            Instance = this;
            this.DisplayName = "Scene Audio";
            this.GroupName = "6. Audio";
            this.Description = "Audio sources in the current scene";
        }

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType)
        {
            return this.AudioInputs.Select(source => this.CreateCommandName(source));
        }

        public void UpdateAudioSources(String sceneName, String[] audioSources)
        {
            this._currentScene = sceneName ?? String.Empty;
            this.AudioInputs = audioSources ?? new String[0];
            PluginLog.Info($"SceneAudioSourcesDynamicFolder updated with {this.AudioInputs.Length} audio sources for scene '{this._currentScene}'");
            this.ButtonActionNamesChanged();
        }

        public void OnDisconnected()
        {
            this.AudioInputs = new String[0];
            this._currentScene = String.Empty;
            this.ButtonActionNamesChanged();
        }
    }
}
