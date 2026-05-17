namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class VirtualCameraStartCommand : PluginDynamicCommand
    {
        public static VirtualCameraStartCommand Instance { get; private set; }
        private readonly ActionImageStore<StateImageData> imageStore;

        public VirtualCameraStartCommand()
            : base(displayName: "Start Virtual Camera", description: "Start OBS virtual camera", groupName: "1. OBS")
        {
            Instance = this;
            this.imageStore = new ActionImageStore<StateImageData>(new StateImageFactory());
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.StartVirtualCamera();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isActive = OBSStudioForLogiPlugin.Instance?.IsVirtualCameraActive ?? false;

            StateImageData imageData = new StateImageData
            {
                Id = "virtual-camera-start",
                IsActive = isActive,
                ActiveIconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.VirtualCameraStartDisabled.svg",
                InactiveIconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.VirtualCameraStart.svg"
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
