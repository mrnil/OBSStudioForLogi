namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class VirtualCameraStartCommand : PluginDynamicCommand
    {
        public static VirtualCameraStartCommand Instance { get; private set; }

        public VirtualCameraStartCommand()
            : base(displayName: "Start Virtual Camera", description: "Start OBS virtual camera", groupName: "5. Virtual Camera")
        {
            Instance = this;
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.StartVirtualCamera();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isActive = OBSStudioForLogiPlugin.Instance?.IsVirtualCameraActive ?? false;
            return ButtonImageHelper.StateIcon(isActive, "VirtualCameraStartDisabled.svg", "VirtualCameraStart.svg");
        }

        public void OnVirtualCameraStateChanged()
        {
            this.ActionImageChanged();
        }
    }
}
