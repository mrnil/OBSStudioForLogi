namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class StreamingStopCommand : StartStopCommandBase
    {
        public StreamingStopCommand()
            : base(displayName: "Stop Streaming", description: "Stop OBS streaming", groupName: "2. Streaming", isStartCommand: false)
        {
        }

        protected override void ExecuteStart()
        {
            // Not used for stop command
        }

        protected override void ExecuteStop()
        {
            OBSStudioForLogiPlugin.Instance?.StopStreaming();
        }

        protected override Boolean GetState()
        {
            return OBSStudioForLogiPlugin.Instance?.IsStreaming ?? false;
        }

        protected override String GetEnabledIcon()
        {
            return "StreamingToggleOff.svg";
        }

        protected override String GetDisabledIcon()
        {
            return "StreamingToggleOn.svg";
        }
    }
}
