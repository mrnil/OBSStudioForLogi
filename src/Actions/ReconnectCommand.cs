namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class ReconnectCommand : PluginDynamicCommand
    {
        public ReconnectCommand()
            : base(displayName: "Reconnect to OBS", description: "Manually reconnect to OBS Studio", groupName: "1. OBS")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.ManualReconnect();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return null;
        }
    }
}
