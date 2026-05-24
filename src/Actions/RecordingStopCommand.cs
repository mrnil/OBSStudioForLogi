namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingStopCommand : StartStopCommandBase
    {
        public RecordingStopCommand()
            : base(displayName: "Stop Recording", description: "Stop OBS recording", groupName: "3. Recording", isStartCommand: false)
        {
        }

        protected override void ExecuteStart()
        {
            // Not used for stop command
        }

        protected override void ExecuteStop()
        {
            OBSStudioForLogiPlugin.Instance?.StopRecording();
        }

        protected override Boolean GetState()
        {
            return OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;
        }

        protected override String GetEnabledIcon()
        {
            return "RecordingStop.svg";
        }

        protected override String GetDisabledIcon()
        {
            return "RecordingStopDisabled.svg";
        }
    }
}
