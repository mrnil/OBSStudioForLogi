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
            return null;
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
