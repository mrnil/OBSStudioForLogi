namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingStartCommand : StartStopCommandBase
    {
        public RecordingStartCommand()
            : base(displayName: "Start Recording", description: "Start OBS recording", groupName: "3. Recording", isStartCommand: true)
        {
        }

        protected override void ExecuteStart()
        {
            OBSStudioForLogiPlugin.Instance?.StartRecording();
        }

        protected override void ExecuteStop()
        {
            // Not used for start command
        }

        protected override Boolean GetState()
        {
            return OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;
        }

        protected override String GetEnabledIcon()
        {
            return "RecordingStart.svg";
        }

        protected override String GetDisabledIcon()
        {
            return "RecordingStartDisabled.svg";
        }
    }
}
