namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class ReplayBufferToggleCommand : ToggleCommandBase, IReplayBufferAwareCommand
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

        protected override void ExecuteToggle()
        {
            OBSStudioForLogiPlugin.Instance?.ToggleReplayBuffer();
        }

        protected override Boolean GetState()
        {
            return OBSStudioForLogiPlugin.Instance?.IsReplayBufferActive ?? false;
        }

        protected override String GetActiveIcon() => "ReplayBufferToggleStop.svg";

        protected override String GetInactiveIcon() => "ReplayBufferToggleStart.svg";

        public void OnReplayBufferStateChanged()
        {
            this.ActionImageChanged();
        }
    }
}
