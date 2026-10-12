namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class StreamingToggleCommand : ToggleCommandBase, IStreamingAwareCommand
    {
        public static StreamingToggleCommand Instance { get; private set; }

        public StreamingToggleCommand()
            : base(displayName: "Toggle Streaming", description: "Start/stop OBS streaming", groupName: "2. Streaming")
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
        }

        protected override void ExecuteToggle()
        {
            OBSStudioForLogiPlugin.Instance?.ToggleStreaming();
        }

        protected override Boolean GetState()
        {
            return OBSStudioForLogiPlugin.Instance?.IsStreaming ?? false;
        }

        protected override String GetActiveIcon() => "StreamingOn.svg";

        protected override String GetInactiveIcon() => "StreamingOff.svg";

        // OBS changes this state asynchronously after a press, and from its own window too, so
        // the button redraws when OBS reports it rather than when it is pressed (assessment #27).
        public void OnStreamingStateChanged()
        {
            this.ActionImageChanged();
        }

        public void OnConnected()
        {
            this.ActionImageChanged();
        }

        // The manager resets the output state to stopped on disconnect.
        public void OnDisconnected()
        {
            this.ActionImageChanged();
        }
    }
}
