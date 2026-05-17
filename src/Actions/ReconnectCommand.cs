namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class ReconnectCommand : PluginDynamicCommand
    {
        private readonly ActionImageStore<SimpleIconImageData> imageStore;

        public ReconnectCommand()
            : base(displayName: "Reconnect to OBS", description: "Manually reconnect to OBS Studio", groupName: "1. OBS")
        {
            this.imageStore = new ActionImageStore<SimpleIconImageData>(new SimpleIconImageFactory());
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.ManualReconnect();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            SimpleIconImageData imageData = new SimpleIconImageData
            {
                Id = "reconnect",
                IconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.StreamingToggleOn.svg"
            };

            this.imageStore.UpdateImage(imageData.Id, imageData);

            if (this.imageStore.TryGetImage(imageData.Id, imageSize, out BitmapImage image))
            {
                return image;
            }

            return EmbeddedResources.ReadImage(imageData.IconPath);
        }
    }
}
