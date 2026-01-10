namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingPauseCommand : PluginDynamicCommand
    {
        public RecordingPauseCommand()
            : base(displayName: "Pause Recording", description: "Pause OBS recording", groupName: "OBS")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.PauseRecording();
        }
    }
}
