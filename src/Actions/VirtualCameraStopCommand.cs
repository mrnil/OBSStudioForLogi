namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class VirtualCameraStopCommand : PluginDynamicCommand
    {
        public static VirtualCameraStopCommand Instance { get; private set; }

        public VirtualCameraStopCommand()
        {
            Instance = this;
            this.Description = "Stop virtual camera in OBS Studio";
            this.GroupName = "4. Virtual Camera";
        }

        protected override Boolean OnLoad()
        {
            this.IsEnabled = false;
            return true;
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.StopVirtualCamera();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var isActive = OBSStudioForLogiPlugin.Instance?.IsVirtualCameraActive ?? false;
            var iconName = isActive ? "VirtualCameraStop.svg" : "VirtualCameraStopDisabled.svg";
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
