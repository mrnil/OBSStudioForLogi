namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingResumeCommand : PluginDynamicCommand
    {
        public RecordingResumeCommand()
            : base(displayName: "Resume Recording", description: "Resume OBS recording", groupName: "OBS")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.ResumeRecording();
        }
    }
}
