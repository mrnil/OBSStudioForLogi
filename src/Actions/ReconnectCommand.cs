namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class ReconnectCommand : PluginDynamicCommand, IObsCommand
    {
        public ReconnectCommand()
            : base(displayName: "Reconnect to OBS", description: "Manually reconnect to OBS Studio", groupName: "1. OBS")
        {
            this.IsWidget = true;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.ManualReconnect();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isConnected = OBSStudioForLogiPlugin.Instance?.IsConnected ?? false;
            Boolean isServerDisabled = OBSStudioForLogiPlugin.Instance?.IsWebSocketServerDisabled ?? false;

            BitmapColor backgroundColor;
            if (isConnected)
            {
                backgroundColor = new BitmapColor(0, 128, 0);
            }
            else if (isServerDisabled)
            {
                backgroundColor = new BitmapColor(200, 120, 0);
            }
            else
            {
                backgroundColor = new BitmapColor(128, 0, 0);
            }

            return ButtonImageHelper.IconWithBackground("Reconnect.svg", imageSize, backgroundColor);
        }

        public void OnConnected()
        {
            this.ActionImageChanged();
        }

        public void OnDisconnected()
        {
            this.ActionImageChanged();
        }
    }
}
