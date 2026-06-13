namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public static class AudioHelpers
    {
        public static String GetMonitoringDisplayText(String monitorType)
        {
            switch (monitorType)
            {
                case "OBS_MONITORING_TYPE_MONITOR_ONLY":
                    return "Monitor only";
                case "OBS_MONITORING_TYPE_MONITOR_AND_OUTPUT":
                    return "Monitor & output";
                default:
                    return "Monitor off";
            }
        }

        public static BitmapImage RenderAudioStateImage(String inputName, PluginImageSize imageSize)
        {
            Boolean isMuted = OBSStudioForLogiPlugin.Instance?.GetInputMute(inputName) ?? false;
            Boolean isSelected = AudioSelectionState.IsSelected(inputName);
            Single volumeLevel = OBSStudioForLogiPlugin.Instance?.GetInputVolume(inputName) ?? 1.0f;
            Int32 volumePercent = (Int32)(volumeLevel * 100);
            String monitorType = OBSStudioForLogiPlugin.Instance?.GetInputAudioMonitorType(inputName) ?? "OBS_MONITORING_TYPE_NONE";
            String mode = GetMonitoringDisplayText(monitorType);

            String text = $"{inputName}\n\n{volumePercent}%\n\n{mode}";

            return ButtonTextRenderer.RenderTextWithBorder(text, imageSize,
                !isMuted ? BitmapColor.Green : BitmapColor.Red, isSelected);
        }

        public static BitmapImage RenderAudioStateImage(String inputName, Int32 imageWidth, Int32 imageHeight)
        {
            Boolean isMuted = OBSStudioForLogiPlugin.Instance?.GetInputMute(inputName) ?? false;
            Boolean isSelected = AudioSelectionState.IsSelected(inputName);
            Single volumeLevel = OBSStudioForLogiPlugin.Instance?.GetInputVolume(inputName) ?? 1.0f;
            Int32 volumePercent = (Int32)(volumeLevel * 100);
            String monitorType = OBSStudioForLogiPlugin.Instance?.GetInputAudioMonitorType(inputName) ?? "OBS_MONITORING_TYPE_NONE";
            String mode = GetMonitoringDisplayText(monitorType);

            String text = $"{inputName}\n\n{volumePercent}%\n\n{mode}";

            return ButtonTextRenderer.RenderTextWithBorder(text, imageWidth, imageHeight,
                !isMuted ? BitmapColor.Green : BitmapColor.Red, isSelected);
        }
    }
}
