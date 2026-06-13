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
            this.IsWidget = true;
            this.AddParameter("", "Connection Status", groupName: "1. OBS");
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isConnected = OBSStudioForLogiPlugin.Instance?.IsConnected ?? false;
            String text = isConnected ? "Connected" : "Disconnected";
            BitmapColor bgColor = isConnected ? new BitmapColor(0, 128, 0) : new BitmapColor(128, 0, 0);
            return ButtonTextRenderer.RenderText(text, imageSize, bgColor, BitmapColor.White);
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }

        public void UpdateStatus()
        {
            this.ActionImageChanged("");
        }
    }
}
