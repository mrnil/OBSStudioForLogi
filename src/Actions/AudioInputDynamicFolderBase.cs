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
            return actionParameter;
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isMuted = OBSStudioForLogiPlugin.Instance?.GetInputMute(actionParameter) ?? false;
            Single volumeLevel = OBSStudioForLogiPlugin.Instance?.GetInputVolume(actionParameter) ?? 1.0f;
            String iconName = isMuted ? "AudioMixerMuted.svg" : "AudioMixerUnmuted.svg";
            String iconPath = $"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconName}";
            
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
                return image;
            }
            
            BitmapColor textColor = isMuted ? BitmapColor.Red : BitmapColor.Green;
            return ButtonTextRenderer.RenderIconWithText(iconPath, actionParameter, imageSize, textColor);
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.ToggleInputMute(actionParameter);
        }

        public void OnInputMuteChanged(String inputName)
        {
            if (this.AudioInputs.Contains(inputName))
            {
                this.CommandImageChanged(this.CreateCommandName(inputName));
            }
        }

        public void OnInputVolumeChanged(String inputName)
        {
            if (this.AudioInputs.Contains(inputName))
            {
                this.CommandImageChanged(this.CreateCommandName(inputName));
            }
        }
    }
}
