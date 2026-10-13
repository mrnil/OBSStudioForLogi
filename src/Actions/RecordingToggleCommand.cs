namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingToggleCommand : ToggleCommandBase, IRecordingAwareCommand
    {
        public static RecordingToggleCommand Instance { get; private set; }

        public RecordingToggleCommand()
            : base(displayName: "Toggle Recording", description: "Start/stop OBS recording", groupName: "3. Recording")
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
        }

        protected override void ExecuteToggle()
        {
            OBSStudioForLogiPlugin.Instance?.ToggleRecording();
        }

        protected override Boolean GetState()
        {
            return OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;
        }

        // A status key: the glyph is red while recording (OBS's own "live" colour) and green while
        // clear, so the state reads at a glance.
        protected override String GetActiveIcon() => "RecordingOnRed.svg";

        protected override String GetInactiveIcon() => "RecordingOffGreen.svg";

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
