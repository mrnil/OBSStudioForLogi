namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class VirtualCameraStopCommand : StartStopCommandBase, IVirtualCameraAwareCommand
    {
        public static VirtualCameraStopCommand Instance { get; private set; }

        public VirtualCameraStopCommand()
            : base(displayName: "Stop Virtual Camera", description: "Stop OBS virtual camera", groupName: "5. Virtual Camera", isStartCommand: false)
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

        protected override void ExecuteStart()
        {
            // Not used for stop command
        }

        protected override void ExecuteStop()
        {
            OBSStudioForLogiPlugin.Instance?.StopVirtualCamera();
        }

        protected override Boolean GetState()
        {
            return OBSStudioForLogiPlugin.Instance?.IsVirtualCameraActive ?? false;
        }

        protected override String GetEnabledIcon()
        {
            return "VirtualCameraOff.svg";
        }

        protected override String GetDisabledIcon()
        {
            return "VirtualCameraOffDisabled.svg";
        }

        public void OnVirtualCameraStateChanged()
        {
            this.ActionImageChanged();
        }
    }
}
