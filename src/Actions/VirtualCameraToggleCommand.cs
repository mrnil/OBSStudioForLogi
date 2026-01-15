namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class VirtualCameraToggleCommand : PluginDynamicCommand
    {
        public static VirtualCameraToggleCommand Instance { get; private set; }

        public VirtualCameraToggleCommand()
        {
            Instance = this;
            this.Description = "Toggle virtual camera on/off in OBS Studio";
            this.GroupName = "1. OBS";
        }

        protected override Boolean OnLoad()
        {
            this.IsEnabled = false;
            return true;
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.ToggleVirtualCamera();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var isActive = OBSStudioForLogiPlugin.Instance?.IsVirtualCameraActive ?? false;
            var iconName = isActive ? "VirtualCameraOn.svg" : "VirtualCameraOff.svg";
            return PluginResources.ReadImage(iconName);
        }

        public void OnConnected()
        {
            this.IsEnabled = true;
            this.ActionImageChanged();
        }

        public void OnDisconnected()
        {
            this.IsEnabled = false;
            this.ActionImageChanged();
        }

        public void OnVirtualCameraStateChanged()
        {
            this.ActionImageChanged();
        }
    }
}
