namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class ReplayBufferToggleCommand : PluginDynamicCommand, IReplayBufferAwareCommand
    {
        public static ReplayBufferToggleCommand Instance { get; private set; }

        public ReplayBufferToggleCommand()
            : base(displayName: "Toggle Replay Buffer", description: "Start/stop OBS replay buffer", groupName: "4. Replay Buffer")
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
        }

        public void OnConnected() { }
        public void OnDisconnected() { }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.ToggleReplayBuffer();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isActive = OBSStudioForLogiPlugin.Instance?.IsReplayBufferActive ?? false;
            return ButtonImageHelper.StateIcon(isActive, "ReplayBufferToggleStop.svg", "ReplayBufferToggleStart.svg");
        }

        public void OnReplayBufferStateChanged()
        {
            this.ActionImageChanged();
        }
    }
}
