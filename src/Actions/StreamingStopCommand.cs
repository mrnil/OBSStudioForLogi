namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class StreamingStopCommand : PluginDynamicCommand
    {
        public StreamingStopCommand()
            : base(displayName: "Stop Streaming", description: "Stop OBS streaming", groupName: "2. Streaming")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.StopStreaming();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var isStreaming = OBSStudioForLogiPlugin.Instance?.IsStreaming ?? false;
            var iconName = isStreaming ? "StreamingToggleOff.svg" : "StreamingToggleOn.svg";
            
            return EmbeddedResources.ReadImage($"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconName}");
        }
    }
}
