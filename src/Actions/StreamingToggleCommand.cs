namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class StreamingToggleCommand : PluginDynamicCommand
    {
        private readonly ActionImageStore<StateImageData> imageStore;

        public StreamingToggleCommand()
            : base(displayName: "Toggle Streaming", description: "Start/stop OBS streaming", groupName: "2. Streaming")
        {
            this.imageStore = new ActionImageStore<StateImageData>(new StateImageFactory());
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.ToggleStreaming();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isStreaming = OBSStudioForLogiPlugin.Instance?.IsStreaming ?? false;

            StateImageData imageData = new StateImageData
            {
                Id = "streaming-toggle",
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
