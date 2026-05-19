namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class VirtualCameraStopCommand : PluginDynamicCommand
    {
        public static VirtualCameraStopCommand Instance { get; private set; }

        public VirtualCameraStopCommand()
            : base(displayName: "Stop Virtual Camera", description: "Stop OBS virtual camera", groupName: "5. Virtual Camera")
        {
            Instance = this;
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.StopVirtualCamera();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isActive = OBSStudioForLogiPlugin.Instance?.IsVirtualCameraActive ?? false;
            return ButtonImageHelper.StateIcon(isActive, "VirtualCameraStop.svg", "VirtualCameraStopDisabled.svg");
        }

        public void OnVirtualCameraStateChanged()
        {
            this.ActionImageChanged();
        }
    }
}
