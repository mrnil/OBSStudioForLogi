namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AudioMixerDynamicFolder : AudioInputDynamicFolderBase
    {
        public static AudioMixerDynamicFolder Instance { get; private set; }

        private Dictionary<String, String[]> _inputScenes = new Dictionary<String, String[]>();

        public AudioMixerDynamicFolder()
        {
            Instance = this;
            this.DisplayName = "Audio Mixer";
            this.GroupName = "7. Audio";
            this.Description = "Folder of audio inputs with mute/unmute controls";
        }

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType)
        {
            return this.AudioInputs.Select(input => this.CreateCommandName(input));
        }

        public void UpdateInputs(String[] inputs)
        {
            this.AudioInputs = inputs ?? new String[0];
            this._inputScenes.Clear();
            
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

        public void OnConnected()
        {
            var inputs = OBSStudioForLogiPlugin.Instance?.GetInputList() ?? new String[0];
            this.UpdateInputs(inputs);
        }

        public void OnDisconnected()
        {
            this.AudioInputs = new String[0];
            this.ButtonActionNamesChanged();
        }
    }
}
