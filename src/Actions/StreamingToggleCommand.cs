namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class StreamingToggleCommand : PluginDynamicCommand
    {
        public StreamingToggleCommand()
            : base(displayName: "Toggle Streaming", description: "Start/stop OBS streaming", groupName: "2. Streaming")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.ToggleStreaming();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isStreaming = OBSStudioForLogiPlugin.Instance?.IsStreaming ?? false;
            return ButtonImageHelper.StateIcon(isStreaming, "StreamingToggleOff.svg", "StreamingToggleOn.svg");
        }
    }
}
