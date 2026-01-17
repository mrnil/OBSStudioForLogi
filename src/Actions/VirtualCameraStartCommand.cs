namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class VirtualCameraStartCommand : PluginDynamicCommand
    {
        public static VirtualCameraStartCommand Instance { get; private set; }

        public VirtualCameraStartCommand()
            : base(displayName: "Start Virtual Camera", description: "Start OBS virtual camera", groupName: "1. OBS")
        {
            Instance = this;
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.StartVirtualCamera();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var isActive = OBSStudioForLogiPlugin.Instance?.IsVirtualCameraActive ?? false;
            var iconName = isActive ? "VirtualCameraStartDisabled.svg" : "VirtualCameraStart.svg";
            return EmbeddedResources.ReadImage($"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconName}");
        }

        public void OnVirtualCameraStateChanged()
        {
            this.ActionImageChanged();
        }
    }
}
