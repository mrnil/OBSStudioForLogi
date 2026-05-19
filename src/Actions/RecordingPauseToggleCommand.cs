namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingPauseToggleCommand : PluginDynamicCommand
    {
        public RecordingPauseToggleCommand()
            : base(displayName: "Recording Pause", description: "Pause/resume OBS recording", groupName: "3. Recording")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.ToggleRecordingPause();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isPaused = OBSStudioForLogiPlugin.Instance?.IsRecordingPaused ?? false;
            return ButtonImageHelper.StateIcon(isPaused, "RecordingPause.svg", "RecordingResume.svg");
        }
    }
}
