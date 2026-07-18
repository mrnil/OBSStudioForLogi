namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SceneAudioSourcesDynamicFolder : AudioInputDynamicFolderBase, IObsCommand, ISceneSourcesAwareCommand
    {
        public static SceneAudioSourcesDynamicFolder Instance { get; private set; }

        private String _currentScene = String.Empty;

        public SceneAudioSourcesDynamicFolder()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.DisplayName = "Mixer for Scene Audio";
            this.GroupName = "8. Audio";
            this.Description = "Folder of scene audio inputs with controls for mute, monitor and volume [Volume - Loupedeck only]";
        }

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType)
            => this.AudioInputs.Select(this.CreateCommandName);

        public void UpdateAudioSources(String sceneName, String[] audioSources)
        {
            var previousSelection = AudioSelectionState.SelectedInput;
            
            this._currentScene = sceneName ?? String.Empty;
            this.AudioInputs = audioSources ?? new String[0];
            
            // Deselect if selected input is no longer in the list
            if (!String.IsNullOrEmpty(previousSelection) && !this.AudioInputs.Contains(previousSelection))
            {
                AudioSelectionState.Deselect();
            }
            
            PluginLog.Debug($"SceneAudioSourcesDynamicFolder updated with {this.AudioInputs.Length} audio sources for scene '{this._currentScene}'");
            this.ButtonActionNamesChanged();
        }

        public void OnSceneSourcesChanged(String sceneName, String[] sources, String[] audioSources)
        {
            this.UpdateAudioSources(sceneName, audioSources);
        }

        public override void OnConnected()
        {
            this.ButtonActionNamesChanged();
        }

        public override void OnDisconnected()
        {
            this.AudioInputs = new String[0];
            this._currentScene = String.Empty;
            AudioSelectionState.Deselect();
            this.ButtonActionNamesChanged();
        }
    }
}
