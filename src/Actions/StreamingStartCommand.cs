namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class StreamingStartCommand : StartStopCommandBase, IStreamingAwareCommand
    {
        public static StreamingStartCommand Instance { get; private set; }

        public StreamingStartCommand()
            : base(displayName: "Start Streaming", description: "Start OBS streaming", groupName: "2. Streaming", isStartCommand: true)
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
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

        protected override Boolean IsChanging()
        {
            return OBSStudioForLogiPlugin.Instance?.IsStreamingChanging ?? false;
        }

        protected override String GetEnabledIcon()
        {
            return "StreamingOn.svg";
        }

        protected override String GetDisabledIcon()
        {
            return "StreamingOnDisabled.svg";
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
