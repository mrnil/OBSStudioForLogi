namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingStartCommand : StartStopCommandBase, IRecordingAwareCommand
    {
        public static RecordingStartCommand Instance { get; private set; }

        public RecordingStartCommand()
            : base(displayName: "Start Recording", description: "Start OBS recording", groupName: "3. Recording", isStartCommand: true)
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
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

        protected override Boolean IsChanging()
        {
            return OBSStudioForLogiPlugin.Instance?.IsRecordingChanging ?? false;
        }

        protected override String GetEnabledIcon()
        {
            return "RecordingOn.svg";
        }

        protected override String GetDisabledIcon()
        {
            return "RecordingOnDisabled.svg";
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
