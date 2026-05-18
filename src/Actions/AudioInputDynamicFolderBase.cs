namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Linq;

    public abstract class AudioInputDynamicFolderBase : PluginDynamicFolder
    {
        protected String[] AudioInputs = new String[0];

        protected AudioInputDynamicFolderBase()
        {
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
            Boolean isMuted = OBSStudioForLogiPlugin.Instance?.GetInputMute(actionParameter) ?? false;
            Single volumeLevel = OBSStudioForLogiPlugin.Instance?.GetInputVolume(actionParameter) ?? 1.0f;
            
            BitmapColor textColor = isMuted ? BitmapColor.Red : BitmapColor.Green;
            Int32 volumePercent = (Int32)(volumeLevel * 100);
            String displayText = $"{actionParameter}\n\n{volumePercent}%";
            
            PluginLog.Info($"[AUDIO IMAGE] '{actionParameter}' - Muted:{isMuted} Vol:{volumePercent}% Color:{(isMuted ? "RED" : "GREEN")}");
            return ButtonTextRenderer.RenderIconWithText(String.Empty, displayText, imageSize, textColor);
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            PluginLog.Info($"[AUDIO PRESS] '{actionParameter}'");
            OBSStudioForLogiPlugin.Instance?.ToggleInputMute(actionParameter);
        }

        public void OnInputMuteChanged(String inputName)
        {
            if (this.AudioInputs.Contains(inputName))
            {
                PluginLog.Info($"[AUDIO MUTE EVENT] '{inputName}' - Refreshing button");
                this.ButtonActionNamesChanged();
            }
        }

        public void OnInputVolumeChanged(String inputName)
        {
            if (this.AudioInputs.Contains(inputName))
            {
                PluginLog.Info($"[AUDIO VOLUME EVENT] '{inputName}' - Refreshing button");
                this.ButtonActionNamesChanged();
            }
        }
    }
}
