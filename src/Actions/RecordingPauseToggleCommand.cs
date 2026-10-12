namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingPauseToggleCommand : ToggleCommandBase, IRecordingAwareCommand
    {
        public static RecordingPauseToggleCommand Instance { get; private set; }

        public RecordingPauseToggleCommand()
            : base(displayName: "Recording Pause", description: "Pause/resume OBS recording", groupName: "3. Recording")
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
        }

        protected override void ExecuteToggle()
        {
            OBSStudioForLogiPlugin.Instance?.ToggleRecordingPause();
        }

        protected override Boolean GetState()
        {
            return OBSStudioForLogiPlugin.Instance?.IsRecordingPaused ?? false;
        }

        protected override String GetActiveIcon() => "RecordingPaused.svg";

        protected override String GetInactiveIcon() => "RecordingOn.svg";

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
