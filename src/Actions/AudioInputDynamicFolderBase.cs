namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Linq;

    public abstract class AudioInputDynamicFolderBase : PluginDynamicFolder
    {
        protected String[] AudioInputs = new String[0];
        private readonly ActionImageStore<AudioInputImageData> imageStore;

        protected AudioInputDynamicFolderBase()
        {
            this.imageStore = new ActionImageStore<AudioInputImageData>(new AudioInputImageFactory());
        }

        public override PluginDynamicFolderNavigation GetNavigationArea(DeviceType _)
        {
            return PluginDynamicFolderNavigation.ButtonArea;
        }

        public override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return String.Empty;
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            PluginLog.Info($"GetCommandImage called for: '{actionParameter}', imageSize: {imageSize}");
            
            Boolean isMuted = OBSStudioForLogiPlugin.Instance?.GetInputMute(actionParameter) ?? false;
            Single volumeLevel = OBSStudioForLogiPlugin.Instance?.GetInputVolume(actionParameter) ?? 1.0f;
            String iconName = isMuted ? "AudioMixerMuted.svg" : "AudioMixerUnmuted.svg";
            String iconPath = $"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconName}";
            
            PluginLog.Info($"  isMuted: {isMuted}, volumeLevel: {volumeLevel:F2}, iconPath: {iconPath}");
            
            var imageData = new AudioInputImageData
            {
                Id = actionParameter,
                InputName = actionParameter,
                IsMuted = isMuted,
                VolumeLevel = volumeLevel,
                IconPath = iconPath
            };
            
            this.imageStore.UpdateImage(imageData.Id, imageData);
            
            if (this.imageStore.TryGetImage(imageData.Id, imageSize, out var image))
            {
                PluginLog.Info($"  Returning cached image from store");
                return image;
            }
            
            PluginLog.Info($"  Generating new image with ButtonTextRenderer");
            BitmapColor textColor = isMuted ? BitmapColor.Red : BitmapColor.Green;
            return ButtonTextRenderer.RenderIconWithText(iconPath, actionParameter, imageSize, textColor);
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
            {
                PluginLog.Warning("RunCommand called with empty actionParameter");
                return;
            }

            PluginLog.Info($"Audio button pressed: '{actionParameter}'");
            OBSStudioForLogiPlugin.Instance?.ToggleInputMute(actionParameter);
        }

        public void OnInputMuteChanged(String inputName)
        {
            PluginLog.Info($"OnInputMuteChanged called for: '{inputName}', AudioInputs contains: {this.AudioInputs.Contains(inputName)}");
            if (this.AudioInputs.Contains(inputName))
            {
                PluginLog.Info($"Triggering CommandImageChanged for: '{inputName}'");
                this.CommandImageChanged(this.CreateCommandName(inputName));
            }
        }

        public void OnInputVolumeChanged(String inputName)
        {
            PluginLog.Info($"OnInputVolumeChanged called for: '{inputName}', AudioInputs contains: {this.AudioInputs.Contains(inputName)}");
            if (this.AudioInputs.Contains(inputName))
            {
                PluginLog.Info($"Triggering CommandImageChanged for: '{inputName}'");
                this.CommandImageChanged(this.CreateCommandName(inputName));
            }
        }
    }
}
