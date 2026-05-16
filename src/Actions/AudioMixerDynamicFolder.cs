namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AudioMixerDynamicFolder : PluginDynamicFolder
    {
        public static AudioMixerDynamicFolder Instance { get; private set; }

        private String[] _inputs = new String[0];
        private Dictionary<String, String[]> _inputScenes = new Dictionary<String, String[]>();

        public AudioMixerDynamicFolder()
        {
            Instance = this;
            this.DisplayName = "Audio Mixer";
            this.GroupName = "6. Audio";
            this.Description = "Folder of audio inputs with mute/unmute controls";
        }

        public override PluginDynamicFolderNavigation GetNavigationArea(DeviceType _)
        {
            return PluginDynamicFolderNavigation.ButtonArea;
        }

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType)
        {
            return this._inputs.Select(input => this.CreateCommandName(input));
        }

        public void UpdateInputs(String[] inputs)
        {
            this._inputs = inputs ?? new String[0];
            this._inputScenes.Clear();
            
            PluginLog.Info($"=== AudioMixerDynamicFolder updated with {this._inputs.Length} inputs ===");
            
            foreach (var input in this._inputs)
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
            this._inputs = new String[0];
            this.ButtonActionNamesChanged();
        }

        public override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return actionParameter;
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var isMuted = OBSStudioForLogiPlugin.Instance?.GetInputMute(actionParameter) ?? false;
            var iconName = isMuted ? "AudioMixerMuted.svg" : "AudioMixerUnmuted.svg";
            var imagePath = $"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconName}";
            var textColor = isMuted ? BitmapColor.Red : BitmapColor.Green;
            
            return ButtonTextRenderer.RenderIconWithText(imagePath, actionParameter, imageSize, textColor);
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.ToggleInputMute(actionParameter);
        }

        public void OnInputMuteChanged(String inputName)
        {
            if (this._inputs.Contains(inputName))
            {
                this.CommandImageChanged(this.CreateCommandName(inputName));
            }
        }
    }
}
