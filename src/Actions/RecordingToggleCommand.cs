namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingToggleCommand : PluginDynamicCommand
    {
        public RecordingToggleCommand()
            : base(displayName: "Toggle Recording", description: "Start/stop OBS recording", groupName: "Recording")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.ToggleRecording();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var isRecording = OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;
            var iconName = isRecording ? "RecordingOff.svg" : "RecordingOn.svg";
            
            return EmbeddedResources.ReadImage($"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconName}");
        }
    }
}
