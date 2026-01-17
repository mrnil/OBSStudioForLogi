namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class VirtualCameraToggleCommand : PluginDynamicCommand
    {
        public static VirtualCameraToggleCommand Instance { get; private set; }

        public VirtualCameraToggleCommand()
            : base(displayName: "Toggle Virtual Camera", description: "Start/stop OBS virtual camera", groupName: "1. OBS")
        {
            Instance = this;
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.ToggleVirtualCamera();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var isActive = OBSStudioForLogiPlugin.Instance?.IsVirtualCameraActive ?? false;
            var iconName = isActive ? "VirtualCameraOn.svg" : "VirtualCameraOff.svg";
            return EmbeddedResources.ReadImage($"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconName}");
        }

        public void OnVirtualCameraStateChanged()
        {
            this.ActionImageChanged();
        }
    }
}
