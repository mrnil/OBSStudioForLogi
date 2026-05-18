namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class ConnectionStatusDisplay : PluginDynamicCommand
    {
        public static ConnectionStatusDisplay Instance { get; private set; }
        private readonly ActionImageStore<TextImageData> imageStore;

        public ConnectionStatusDisplay()
            : base(displayName: "", description: "Shows OBS connection status", groupName: "1. OBS")
        {
            Instance = this;
            this.imageStore = new ActionImageStore<TextImageData>(new TextWithBackgroundImageFactory());
            this.AddParameter("", "Connection Status", groupName: "1. OBS");
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return String.Empty;
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isConnected = OBSStudioForLogiPlugin.Instance?.IsConnected ?? false;

            TextImageData imageData = new TextImageData
            {
                Id = "connection-status",
                DisplayText = isConnected ? "Connected" : "Disconnected",
                BackgroundColor = isConnected ? new BitmapColor(0, 128, 0) : new BitmapColor(128, 0, 0),
                TextColor = BitmapColor.White
            };

            this.imageStore.UpdateImage(imageData.Id, imageData);

            if (this.imageStore.TryGetImage(imageData.Id, imageSize, out BitmapImage image))
            {
                return image;
            }

            return ButtonTextRenderer.RenderConnectionStatus(isConnected, imageSize);
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
