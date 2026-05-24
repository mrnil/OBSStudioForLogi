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

        public void OnConnected() { }
        public void OnDisconnected() { }

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
            return "VirtualCameraStart.svg";
        }

        protected override String GetDisabledIcon()
        {
            return "VirtualCameraStartDisabled.svg";
        }

        public void OnVirtualCameraStateChanged()
        {
            this.ActionImageChanged();
        }
    }
}
