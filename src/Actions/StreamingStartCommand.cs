namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class StreamingStartCommand : PluginDynamicCommand
    {
        private readonly ActionImageStore<StateImageData> imageStore;

        public StreamingStartCommand()
            : base(displayName: "Start Streaming", description: "Start OBS streaming", groupName: "2. Streaming")
        {
            this.imageStore = new ActionImageStore<StateImageData>(new StateImageFactory());
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.StartStreaming();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isStreaming = OBSStudioForLogiPlugin.Instance?.IsStreaming ?? false;

            StateImageData imageData = new StateImageData
            {
                Id = "streaming-start",
                IsActive = isStreaming,
                ActiveIconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.StreamingToggleOff.svg",
                InactiveIconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.StreamingToggleOn.svg"
            };

            this.imageStore.UpdateImage(imageData.Id, imageData);

            if (this.imageStore.TryGetImage(imageData.Id, imageSize, out BitmapImage image))
            {
                return image;
            }

            return EmbeddedResources.ReadImage(imageData.InactiveIconPath);
        }
    }
}
