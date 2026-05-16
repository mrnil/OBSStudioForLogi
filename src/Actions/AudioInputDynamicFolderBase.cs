namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Linq;

    public abstract class AudioInputDynamicFolderBase : PluginDynamicFolder
    {
        protected String[] AudioInputs = new String[0];

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
            if (this.AudioInputs.Contains(inputName))
            {
                this.CommandImageChanged(this.CreateCommandName(inputName));
            }
        }
    }
}
