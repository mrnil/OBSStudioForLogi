namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class ConnectionStatusDisplay : PluginDynamicCommand, IObsCommand, IConnectionStatusAwareCommand
    {
        public static ConnectionStatusDisplay Instance { get; private set; }

        public ConnectionStatusDisplay()
            : base(displayName: "Connection Status", description: "Shows OBS connection status", groupName: "1. OBS")
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.IsWidget = true;
            this.AddParameter("", "Connection Status", groupName: "1. OBS");
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isConnected = OBSStudioForLogiPlugin.Instance?.IsConnected ?? false;
            Boolean isServerDisabled = OBSStudioForLogiPlugin.Instance?.IsWebSocketServerDisabled ?? false;

            String text;
            BitmapColor bgColor;

            if (isConnected)
            {
                text = "Connected";
                bgColor = ButtonImageHelper.StatusGreen;
            }
            else if (isServerDisabled)
            {
                text = "WebSocket\nDisabled";
                bgColor = new BitmapColor(200, 120, 0);
            }
            else
            {
                text = "Disconnected";
                bgColor = ButtonImageHelper.StatusRed;
            }

            return ButtonTextRenderer.RenderText(text, imageSize, bgColor, BitmapColor.White);
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }

        public void OnConnectionStatusChanged()
        {
            this.ActionImageChanged("");
        }

        // OnConnectionStatusChanged covers connect and disconnect too, and also the WebSocket
        // server being disabled, which OnDisconnected doesn't.
        public void OnConnected()
        {
        }

        public void OnDisconnected()
        {
        }
    }
}
