namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingStartCommand : PluginDynamicCommand
    {
        public RecordingStartCommand()
            : base(displayName: "Start Recording", description: "Start OBS recording", groupName: "3. Recording")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.StartRecording();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var isRecording = OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;
            var iconName = isRecording ? "RecordingStartDisabled.svg" : "RecordingStart.svg";
            
            return EmbeddedResources.ReadImage($"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconName}");
        }
    }
}
