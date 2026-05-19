namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class ScreenshotCommand : PluginDynamicCommand
    {
        public ScreenshotCommand()
            : base(displayName: "Screenshot", 
                   description: String.IsNullOrEmpty(OBSStudioForLogiPlugin.ScreenshotPath) 
                       ? "Cannot find folder for screenshot saving, feature disabled" 
                       : $"Takes a screenshot of currently active scene and saves it to {OBSStudioForLogiPlugin.ScreenshotPath}", 
                   groupName: "1. OBS")
        {
        }

        protected override Boolean OnLoad()
        {
            this.IsEnabled = !String.IsNullOrEmpty(OBSStudioForLogiPlugin.ScreenshotPath);
            return true;
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return ButtonImageHelper.Icon("Screenshot.svg");
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.SaveScreenshot();
        }
    }
}
