namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingStopCommand : PluginDynamicCommand
    {
        public RecordingStopCommand()
            : base(displayName: "Stop Recording", description: "Stop OBS recording", groupName: "OBS")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.StopRecording();
        }
    }
}
