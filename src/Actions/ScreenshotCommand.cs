namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class ScreenshotCommand : PluginDynamicCommand
    {
        private readonly ActionImageStore<SimpleIconImageData> imageStore;

        public ScreenshotCommand()
            : base(displayName: "Screenshot", 
                   description: String.IsNullOrEmpty(OBSStudioForLogiPlugin.ScreenshotPath) 
                       ? "Cannot find folder for screenshot saving, feature disabled" 
                       : $"Takes a screenshot of currently active scene and saves it to {OBSStudioForLogiPlugin.ScreenshotPath}", 
                   groupName: "1. OBS")
        {
            this.imageStore = new ActionImageStore<SimpleIconImageData>(new SimpleIconImageFactory());
        }

        protected override Boolean OnLoad()
        {
            this.IsEnabled = !String.IsNullOrEmpty(OBSStudioForLogiPlugin.ScreenshotPath);
            return true;
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            SimpleIconImageData imageData = new SimpleIconImageData
            {
                Id = "screenshot",
                IconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.Screenshot.svg"
            };

            this.imageStore.UpdateImage(imageData.Id, imageData);

            if (this.imageStore.TryGetImage(imageData.Id, imageSize, out BitmapImage image))
            {
                return image;
            }

            return EmbeddedResources.ReadImage(imageData.IconPath);
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.SaveScreenshot();
        }
    }
}
