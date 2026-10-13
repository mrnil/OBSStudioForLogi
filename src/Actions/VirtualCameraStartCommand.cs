namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class VirtualCameraStartCommand : StartStopCommandBase, IVirtualCameraAwareCommand
    {
        public static VirtualCameraStartCommand Instance { get; private set; }

        public VirtualCameraStartCommand()
            : base(displayName: "Start Virtual Camera", description: "Start OBS virtual camera", groupName: "5. Virtual Camera", isStartCommand: true)
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
            OBSStudioForLogiPlugin.Instance?.StartVirtualCamera();
        }

        protected override void ExecuteStop()
        {
            // Not used for start command
        }

        protected override Boolean GetState()
        {
            return OBSStudioForLogiPlugin.Instance?.IsVirtualCameraActive ?? false;
        }

        protected override String GetEnabledIcon()
        {
            return "VirtualCameraOn.svg";
        }

        protected override String GetDisabledIcon()
        {
            return "VirtualCameraOnDisabled.svg";
        }

        public void OnVirtualCameraStateChanged()
        {
            this.ActionImageChanged();
        }
    }
}
