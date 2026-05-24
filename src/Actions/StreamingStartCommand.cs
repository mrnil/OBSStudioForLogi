namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class StreamingStartCommand : StartStopCommandBase
    {
        public StreamingStartCommand()
            : base(displayName: "Start Streaming", description: "Start OBS streaming", groupName: "2. Streaming", isStartCommand: true)
        {
        }

        protected override void ExecuteStart()
        {
            OBSStudioForLogiPlugin.Instance?.StartStreaming();
        }

        protected override void ExecuteStop()
        {
            // Not used for start command
        }

        protected override Boolean GetState()
        {
            return OBSStudioForLogiPlugin.Instance?.IsStreaming ?? false;
        }

        protected override String GetEnabledIcon()
        {
            return "StreamingToggleOn.svg";
        }

        protected override String GetDisabledIcon()
        {
            return "StreamingToggleOff.svg";
        }
    }
}
