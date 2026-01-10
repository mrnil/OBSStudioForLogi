namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingPauseToggleCommand : PluginDynamicCommand
    {
        public RecordingPauseToggleCommand()
            : base(displayName: "Recording Pause", description: "Pause/resume OBS recording", groupName: "Recording")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.ToggleRecordingPause();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var isPaused = OBSStudioForLogiPlugin.Instance?.IsRecordingPaused ?? false;
            var iconName = isPaused ? "RecordingPause.svg" : "RecordingResume.svg";
            
            return EmbeddedResources.ReadImage($"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconName}");
        }
    }
}
