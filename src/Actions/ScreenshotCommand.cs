namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class ScreenshotCommand : PluginDynamicCommand
    {
        public ScreenshotCommand()
            : base(displayName: "Screenshot", description: "Save OBS screenshot", groupName: "1. OBS")
        {
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var imagePath = "Loupedeck.OBSStudioForLogiPlugin.Icons.Screenshot.svg";
            return PluginResources.ReadImage(imagePath);
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.SaveScreenshot();
        }
    }
}
