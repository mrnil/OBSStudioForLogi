namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class VirtualCameraToggleCommand : PluginDynamicCommand, IVirtualCameraAwareCommand
    {
        public static VirtualCameraToggleCommand Instance { get; private set; }

        public VirtualCameraToggleCommand()
            : base(displayName: "Toggle Virtual Camera", description: "Start/stop OBS virtual camera", groupName: "5. Virtual Camera")
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
        }

        public void OnConnected() { }
        public void OnDisconnected() { }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.ToggleVirtualCamera();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isActive = OBSStudioForLogiPlugin.Instance?.IsVirtualCameraActive ?? false;
            return ButtonImageHelper.StateIcon(isActive, "VirtualCameraOn.svg", "VirtualCameraOff.svg");
        }

        public void OnVirtualCameraStateChanged()
        {
            this.ActionImageChanged();
        }
    }
}
