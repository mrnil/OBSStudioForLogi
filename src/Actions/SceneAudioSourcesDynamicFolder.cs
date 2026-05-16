namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SceneAudioSourcesDynamicFolder : PluginDynamicFolder
    {
        public static SceneAudioSourcesDynamicFolder Instance { get; private set; }

        private String[] _audioSources = new String[0];
        private String _currentScene = String.Empty;

        public SceneAudioSourcesDynamicFolder()
        {
            Instance = this;
            this.DisplayName = "Scene Audio";
            this.GroupName = "6. Audio";
            this.Description = "Audio sources in the current scene";
        }

        public override PluginDynamicFolderNavigation GetNavigationArea(DeviceType _)
        {
            return PluginDynamicFolderNavigation.ButtonArea;
        }

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType)
        {
            return this._audioSources.Select(source => this.CreateCommandName(source));
        }

        public void UpdateAudioSources(String sceneName, String[] audioSources)
        {
            this._currentScene = sceneName ?? String.Empty;
            this._audioSources = audioSources ?? new String[0];
            PluginLog.Info($"SceneAudioSourcesDynamicFolder updated with {this._audioSources.Length} audio sources for scene '{this._currentScene}'");
            this.ButtonActionNamesChanged();
        }

        public void OnDisconnected()
        {
            this._audioSources = new String[0];
            this._currentScene = String.Empty;
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
            if (this._audioSources.Contains(inputName))
            {
                this.CommandImageChanged(this.CreateCommandName(inputName));
            }
        }
    }
}
