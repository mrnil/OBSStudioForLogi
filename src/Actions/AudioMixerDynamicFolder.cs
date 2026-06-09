namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AudioMixerDynamicFolder : AudioInputDynamicFolderBase, IObsCommand, IInputsListAwareCommand
    {
        public static AudioMixerDynamicFolder Instance { get; private set; }

        private Dictionary<String, String[]> _inputScenes = new Dictionary<String, String[]>();

        public AudioMixerDynamicFolder()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.DisplayName = "Audio Mixer";
            this.GroupName = "8. Audio";
            this.Description = "Folder of audio inputs with mute/unmute controls";
        }

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType) 
            => this.AudioInputs.Select(this.CreateCommandName);

        public void OnInputsChanged(String[] inputs) => this.UpdateInputs(inputs);

        private void UpdateInputs(String[] inputs)
        {
            String previousSelection = AudioSelectionState.SelectedInput;
            
            this.AudioInputs = inputs ?? new String[0];
            this._inputScenes.Clear();
            
            // Deselect if selected input is no longer in the list
            if (!String.IsNullOrEmpty(previousSelection) && !this.AudioInputs.Contains(previousSelection))
            {
                AudioSelectionState.Deselect();
            }
            
            PluginLog.Info($"=== AudioMixerDynamicFolder updated with {this.AudioInputs.Length} inputs ===");
            
            foreach (var input in this.AudioInputs)
            {
                var kind = OBSStudioForLogiPlugin.Instance?.GetInputKind(input) ?? String.Empty;
                var scenes = OBSStudioForLogiPlugin.Instance?.GetScenesForInput(input) ?? new String[0];
                this._inputScenes[input] = scenes;
                
                var scenesText = scenes.Length > 0 ? String.Join(", ", scenes) : "(no scenes)";
                PluginLog.Info($"  Input: '{input}' - Kind: '{kind}' - Scenes: {scenesText}");
            }
            
            this.ButtonActionNamesChanged();
        }

        public override void OnConnected()
        {
            var inputs = OBSStudioForLogiPlugin.Instance?.GetInputList() ?? new String[0];
            this.UpdateInputs(inputs);
        }

        public override void OnDisconnected()
        {
            this.AudioInputs = new String[0];
            AudioSelectionState.Deselect();
            this.ButtonActionNamesChanged();
        }
    }
}
