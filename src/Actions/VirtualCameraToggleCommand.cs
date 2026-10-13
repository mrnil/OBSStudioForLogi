namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class VirtualCameraToggleCommand : ToggleCommandBase, IVirtualCameraAwareCommand
    {
        public static VirtualCameraToggleCommand Instance { get; private set; }

        public VirtualCameraToggleCommand()
            : base(displayName: "Toggle Virtual Camera", description: "Start/stop OBS virtual camera", groupName: "5. Virtual Camera")
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
        }

        // Greyed while disconnected, and the manager resets the virtual camera to stopped when the
        // connection drops, so redraw both ways.
        public void OnConnected()
        {
            this.ActionImageChanged();
        }

        public void OnDisconnected()
        {
            this.ActionImageChanged();
        }

        protected override void ExecuteToggle()
        {
            OBSStudioForLogiPlugin.Instance?.ToggleVirtualCamera();
        }

        protected override Boolean GetState()
        {
            return OBSStudioForLogiPlugin.Instance?.IsVirtualCameraActive ?? false;
        }

        protected override String GetActiveIcon() => "VirtualCameraOn.svg";

        protected override String GetInactiveIcon() => "VirtualCameraOff.svg";

        public void OnVirtualCameraStateChanged()
        {
            this.ActionImageChanged();
        }
    }
}
