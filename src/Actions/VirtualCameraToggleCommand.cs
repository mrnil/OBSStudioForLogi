namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class VirtualCameraToggleCommand : PluginDynamicCommand
    {
        public static VirtualCameraToggleCommand Instance { get; private set; }
        private readonly ActionImageStore<StateImageData> imageStore;

        public VirtualCameraToggleCommand()
            : base(displayName: "Toggle Virtual Camera", description: "Start/stop OBS virtual camera", groupName: "1. OBS")
        {
            Instance = this;
            this.imageStore = new ActionImageStore<StateImageData>(new StateImageFactory());
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.ToggleVirtualCamera();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isActive = OBSStudioForLogiPlugin.Instance?.IsVirtualCameraActive ?? false;

            StateImageData imageData = new StateImageData
            {
                Id = "virtual-camera-toggle",
                IsActive = isActive,
                ActiveIconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.VirtualCameraOn.svg",
                InactiveIconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.VirtualCameraOff.svg"
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
