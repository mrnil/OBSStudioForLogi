namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingPauseToggleCommand : ToggleCommandBase
    {
        public RecordingPauseToggleCommand()
            : base(displayName: "Recording Pause", description: "Pause/resume OBS recording", groupName: "3. Recording")
        {
        }

        protected override void ExecuteToggle()
        {
            OBSStudioForLogiPlugin.Instance?.ToggleRecordingPause();
        }

        protected override Boolean GetState()
        {
            return OBSStudioForLogiPlugin.Instance?.IsRecordingPaused ?? false;
        }

        protected override String GetActiveIcon() => "RecordingPause.svg";

        protected override String GetInactiveIcon() => "RecordingResume.svg";
    }
}
