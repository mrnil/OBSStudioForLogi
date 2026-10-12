namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class StreamingStopCommand : StartStopCommandBase, IStreamingAwareCommand
    {
        public static StreamingStopCommand Instance { get; private set; }

        public StreamingStopCommand()
            : base(displayName: "Stop Streaming", description: "Stop OBS streaming", groupName: "2. Streaming", isStartCommand: false)
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
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
            return "StreamingOff.svg";
        }

        protected override String GetDisabledIcon()
        {
            return "StreamingOffDisabled.svg";
        }

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
