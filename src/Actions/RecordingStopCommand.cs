namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingStopCommand : PluginDynamicCommand
    {
        public RecordingStopCommand()
            : base(displayName: "Stop Recording", description: "Stop OBS recording", groupName: "OBS")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.StopRecording();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var isRecording = OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;
            var iconName = isRecording ? "RecordingStop.svg" : "RecordingStopDisabled.svg";
            
            return EmbeddedResources.ReadImage($"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconName}");
        }
    }
}
