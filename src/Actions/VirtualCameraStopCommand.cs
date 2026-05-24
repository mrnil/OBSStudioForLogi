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

        public void OnConnected() { }
        public void OnDisconnected() { }

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
            return "VirtualCameraStop.svg";
        }

        protected override String GetDisabledIcon()
        {
            return "VirtualCameraStopDisabled.svg";
        }

        public void OnVirtualCameraStateChanged()
        {
            this.ActionImageChanged();
        }
    }
}
