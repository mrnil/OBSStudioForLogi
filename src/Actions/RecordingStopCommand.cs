namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingStopCommand : StartStopCommandBase, IRecordingAwareCommand
    {
        public static RecordingStopCommand Instance { get; private set; }

        public RecordingStopCommand()
            : base(displayName: "Stop Recording", description: "Stop OBS recording", groupName: "3. Recording", isStartCommand: false)
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
            OBSStudioForLogiPlugin.Instance?.StopRecording();
        }

        protected override Boolean GetState()
        {
            return OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;
        }

        protected override Boolean IsChanging()
        {
            return OBSStudioForLogiPlugin.Instance?.IsRecordingChanging ?? false;
        }

        protected override String GetEnabledIcon()
        {
            return "RecordingOff.svg";
        }

        protected override String GetDisabledIcon()
        {
            return "RecordingOffDisabled.svg";
        }

        // OBS changes this state asynchronously after a press, and from its own window too, so
        // the button redraws when OBS reports it rather than when it is pressed (assessment #27).
        public void OnRecordingStateChanged()
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
