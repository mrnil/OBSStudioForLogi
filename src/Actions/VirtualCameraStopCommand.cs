namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class VirtualCameraStopCommand : PluginDynamicCommand
    {
        public static VirtualCameraStopCommand Instance { get; private set; }
        private readonly ActionImageStore<StateImageData> imageStore;

        public VirtualCameraStopCommand()
            : base(displayName: "Stop Virtual Camera", description: "Stop OBS virtual camera", groupName: "1. OBS")
        {
            Instance = this;
            this.imageStore = new ActionImageStore<StateImageData>(new StateImageFactory());
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.StopVirtualCamera();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isActive = OBSStudioForLogiPlugin.Instance?.IsVirtualCameraActive ?? false;

            StateImageData imageData = new StateImageData
            {
                Id = "virtual-camera-stop",
                IsActive = isActive,
                ActiveIconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.VirtualCameraStop.svg",
                InactiveIconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.VirtualCameraStopDisabled.svg"
            };

            this.imageStore.UpdateImage(imageData.Id, imageData);

            if (this.imageStore.TryGetImage(imageData.Id, imageSize, out BitmapImage image))
            {
                return image;
            }

            return EmbeddedResources.ReadImage(imageData.InactiveIconPath);
        }

        public void OnVirtualCameraStateChanged()
        {
            this.ActionImageChanged();
        }
    }
}
