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
            PluginLog.Info($"GetCommandImage called for: '{actionParameter}', imageSize: {imageSize}");
            
            Boolean isMuted = OBSStudioForLogiPlugin.Instance?.GetInputMute(actionParameter) ?? false;
            Single volumeLevel = OBSStudioForLogiPlugin.Instance?.GetInputVolume(actionParameter) ?? 1.0f;
            
            PluginLog.Info($"  isMuted: {isMuted}, volumeLevel: {volumeLevel:F2}");
            
            BitmapColor textColor = isMuted ? new BitmapColor(255, 0, 0) : new BitmapColor(0, 255, 0);
            Int32 volumePercent = (Int32)(volumeLevel * 100);
            String displayText = $"{actionParameter}\n\n{volumePercent}%";
            
            PluginLog.Info($"  Generating image with text: '{displayText}', color: {(isMuted ? "Red" : "Green")}");
            return ButtonTextRenderer.RenderIconWithText(String.Empty, displayText, imageSize, textColor);
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
                PluginLog.Info($"Triggering ButtonActionNamesChanged to refresh all buttons");
                this.ButtonActionNamesChanged();
            }
        }

        public void OnInputVolumeChanged(String inputName)
        {
            PluginLog.Info($"OnInputVolumeChanged called for: '{inputName}', AudioInputs contains: {this.AudioInputs.Contains(inputName)}");
            if (this.AudioInputs.Contains(inputName))
            {
                PluginLog.Info($"Triggering ButtonActionNamesChanged to refresh all buttons");
                this.ButtonActionNamesChanged();
            }
        }
    }
}
