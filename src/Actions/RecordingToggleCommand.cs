namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingToggleCommand : PluginDynamicCommand
    {
        public RecordingToggleCommand()
            : base(displayName: "Toggle Recording", description: "Start/stop OBS recording", groupName: "3. Recording")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.ToggleRecording();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isRecording = OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;
            return ButtonImageHelper.StateIcon(isRecording, "RecordingOn.svg", "RecordingOff.svg");
        }
    }
}
