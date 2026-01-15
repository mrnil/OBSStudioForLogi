namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class VirtualCameraStartCommand : PluginDynamicCommand
    {
        public static VirtualCameraStartCommand Instance { get; private set; }

        public VirtualCameraStartCommand()
        {
            Instance = this;
            this.Description = "Start virtual camera in OBS Studio";
            this.GroupName = "4. Virtual Camera";
        }

        protected override Boolean OnLoad()
        {
            this.IsEnabled = false;
            return true;
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.StartVirtualCamera();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var isActive = OBSStudioForLogiPlugin.Instance?.IsVirtualCameraActive ?? false;
            var iconName = isActive ? "VirtualCameraStartDisabled.svg" : "VirtualCameraStart.svg";
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
