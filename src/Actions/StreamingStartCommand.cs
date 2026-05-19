namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class StreamingStartCommand : PluginDynamicCommand
    {
        public StreamingStartCommand()
            : base(displayName: "Start Streaming", description: "Start OBS streaming", groupName: "2. Streaming")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.StartStreaming();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isStreaming = OBSStudioForLogiPlugin.Instance?.IsStreaming ?? false;
            return ButtonImageHelper.StateIcon(isStreaming, "StreamingToggleOff.svg", "StreamingToggleOn.svg");
        }
    }
}
