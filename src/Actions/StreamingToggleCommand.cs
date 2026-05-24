namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class StreamingToggleCommand : ToggleCommandBase
    {
        public StreamingToggleCommand()
            : base(displayName: "Toggle Streaming", description: "Start/stop OBS streaming", groupName: "2. Streaming")
        {
        }

        protected override void ExecuteToggle()
        {
            OBSStudioForLogiPlugin.Instance?.ToggleStreaming();
        }

        protected override Boolean GetState()
        {
            return OBSStudioForLogiPlugin.Instance?.IsStreaming ?? false;
        }

        protected override String GetActiveIcon() => "StreamingToggleOff.svg";

        protected override String GetInactiveIcon() => "StreamingToggleOn.svg";
    }
}
