namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading.Tasks;

    public class ReplayBufferSaveCommand : PluginDynamicCommand, IObsCommand, IReplayBufferSavedAwareCommand
    {
        public static ReplayBufferSaveCommand Instance { get; private set; }

        private Boolean _showingSaved = false;

        public ReplayBufferSaveCommand()
            : base(displayName: "Save Replay", description: "Save OBS replay buffer", groupName: "4. Replay Buffer")
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.IsWidget = true;
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.SaveReplayBuffer();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            if (this._showingSaved)
            {
                return ButtonImageHelper.Icon("ReplayBufferSaved.svg");
            }

            return ButtonImageHelper.Icon("ReplayBufferSave.svg");
        }

        public void OnConnected()
        {
            this.IsEnabled = true;
        }

        public void OnDisconnected()
        {
            this.IsEnabled = false;
        }

        public void OnReplayBufferSaved(String savedReplayPath)
        {
            PluginLog.Info($"Replay saved: {System.IO.Path.GetFileName(savedReplayPath)}");
            this._showingSaved = true;
            this.ActionImageChanged();

            Task.Run(async () =>
            {
                await Task.Delay(2000);
                this._showingSaved = false;
                this.ActionImageChanged();
            });
        }
    }
}
