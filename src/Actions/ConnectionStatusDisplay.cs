namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class ConnectionStatusDisplay : PluginDynamicCommand
    {
        public static ConnectionStatusDisplay Instance { get; private set; }

        public ConnectionStatusDisplay()
            : base(displayName: "Connection Status", description: "Shows OBS connection status", groupName: "1. OBS")
        {
            Instance = this;
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            if (OBSStudioForLogiPlugin.Instance?.IsConnected ?? false)
            {
                return "Connected";
            }
            return "Disconnected";
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                var isConnected = OBSStudioForLogiPlugin.Instance?.IsConnected ?? false;
                bitmapBuilder.Clear(isConnected ? BitmapColor.Black : BitmapColor.Black);
                var displayText = isConnected ? "Connected" : "Disconnected";
                var textColor = isConnected ? BitmapColor.Green : BitmapColor.Red;
                bitmapBuilder.DrawText(displayText, textColor, imageSize == PluginImageSize.Width90 ? 13 : 11);
                return bitmapBuilder.ToImage();
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }

        public void UpdateStatus()
        {
            this.ActionImageChanged();
        }
    }
}
