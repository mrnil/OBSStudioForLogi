namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class ReplayBufferSaveCommand : PluginDynamicCommand
    {
        public ReplayBufferSaveCommand()
            : base(displayName: "Save Replay", description: "Save OBS replay buffer", groupName: "4. Replay Buffer")
        {
            this.IsWidget = true;
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.SaveReplayBuffer();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return ButtonImageHelper.Icon("ReplayBufferSave.svg");
        }
    }
}
