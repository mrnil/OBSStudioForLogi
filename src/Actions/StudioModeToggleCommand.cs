namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class StudioModeToggleCommand : ToggleCommandBase, IStudioModeAwareCommand
    {
        public static StudioModeToggleCommand Instance { get; private set; }

        public StudioModeToggleCommand()
            : base(displayName: "Toggle Studio Mode", description: "Enable/disable OBS studio mode", groupName: "1. OBS")
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
        }

        public void OnConnected() { }
        public void OnDisconnected() { }

        protected override void ExecuteToggle()
        {
            OBSStudioForLogiPlugin.Instance?.ToggleStudioMode();
        }

        protected override Boolean GetState()
        {
            return OBSStudioForLogiPlugin.Instance?.IsStudioModeEnabled ?? false;
        }

        protected override String GetActiveIcon() => "StudioModeOn.svg";

        protected override String GetInactiveIcon() => "StudioModeOff.svg";

        public void OnStudioModeStateChanged()
        {
            this.ActionImageChanged();
        }
    }
}
