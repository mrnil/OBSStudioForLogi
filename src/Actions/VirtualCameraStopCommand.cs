namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class VirtualCameraStopCommand : PluginDynamicCommand
    {
        public static VirtualCameraStopCommand Instance { get; private set; }

        public VirtualCameraStopCommand()
            : base(displayName: "Stop Virtual Camera", description: "Stop OBS virtual camera", groupName: "1. OBS")
        {
            Instance = this;
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.StopVirtualCamera();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var isActive = OBSStudioForLogiPlugin.Instance?.IsVirtualCameraActive ?? false;
            var iconName = isActive ? "VirtualCameraStop.svg" : "VirtualCameraStopDisabled.svg";
            return EmbeddedResources.ReadImage($"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconName}");
        }

        public void OnVirtualCameraStateChanged()
        {
            this.ActionImageChanged();
        }
    }
}
