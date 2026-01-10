namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingStartCommand : PluginDynamicCommand
    {
        public RecordingStartCommand()
            : base(displayName: "Start Recording", description: "Start OBS recording", groupName: "OBS")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.StartRecording();
        }
    }
}
