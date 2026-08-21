namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class CommandCoordinator
    {
        private readonly CommandRegistry _registry;

        public CommandCoordinator()
        {
            this._registry = new CommandRegistry();
        }

        public void RegisterCommand(IObsCommand command)
        {
            this._registry.Register(command);
        }

        public void NotifyConnected() =>
            this.NotifyEach<IObsCommand>(nameof(IObsCommand.OnConnected), c => c.OnConnected());

        public void NotifyDisconnected() =>
            this.NotifyEach<IObsCommand>(nameof(IObsCommand.OnDisconnected), c => c.OnDisconnected());

        public void NotifyProfileChanged(String oldProfile, String newProfile) =>
            this.NotifyEach<IProfileAwareCommand>(nameof(IProfileAwareCommand.OnProfileChanged), c => c.OnProfileChanged(oldProfile, newProfile));

        public void NotifySceneCollectionChanged(String oldSceneCollection, String newSceneCollection) =>
            this.NotifyEach<ISceneCollectionAwareCommand>(nameof(ISceneCollectionAwareCommand.OnSceneCollectionChanged), c => c.OnSceneCollectionChanged(oldSceneCollection, newSceneCollection));

        public void NotifySceneChanged(String sceneName) =>
            this.NotifyEach<ISceneAwareCommand>(nameof(ISceneAwareCommand.OnSceneChanged), c => c.OnSceneChanged(sceneName));

        public void NotifyScenesChanged(String[] scenes) =>
            this.NotifyEach<IScenesListAwareCommand>(nameof(IScenesListAwareCommand.OnScenesChanged), c => c.OnScenesChanged(scenes));

        public void NotifyProfilesChanged(String[] profiles, String currentProfile) =>
            this.NotifyEach<IProfilesListAwareCommand>(nameof(IProfilesListAwareCommand.OnProfilesChanged), c => c.OnProfilesChanged(profiles, currentProfile));

        public void NotifySourceVisibilityChanged(String sceneName, String sourceName) =>
            this.NotifyEach<ISourceVisibilityAwareCommand>(nameof(ISourceVisibilityAwareCommand.OnSourceVisibilityChanged), c => c.OnSourceVisibilityChanged(sceneName, sourceName));

        public void NotifyInputMuteChanged(String inputName) =>
            this.NotifyEach<IInputMuteAwareCommand>(nameof(IInputMuteAwareCommand.OnInputMuteChanged), c => c.OnInputMuteChanged(inputName));

        public void NotifyInputVolumeChanged(String inputName) =>
            this.NotifyEach<IInputVolumeAwareCommand>(nameof(IInputVolumeAwareCommand.OnInputVolumeChanged), c => c.OnInputVolumeChanged(inputName));

        public void NotifyInputsChanged(String[] inputs) =>
            this.NotifyEach<IInputsListAwareCommand>(nameof(IInputsListAwareCommand.OnInputsChanged), c => c.OnInputsChanged(inputs));

        public void NotifyVirtualCameraStateChanged() =>
            this.NotifyEach<IVirtualCameraAwareCommand>(nameof(IVirtualCameraAwareCommand.OnVirtualCameraStateChanged), c => c.OnVirtualCameraStateChanged());

        public void NotifyReplayBufferStateChanged() =>
            this.NotifyEach<IReplayBufferAwareCommand>(nameof(IReplayBufferAwareCommand.OnReplayBufferStateChanged), c => c.OnReplayBufferStateChanged());

        public void NotifyReplayBufferSaved(String savedReplayPath) =>
            this.NotifyEach<IReplayBufferSavedAwareCommand>(nameof(IReplayBufferSavedAwareCommand.OnReplayBufferSaved), c => c.OnReplayBufferSaved(savedReplayPath));

        public void NotifyStudioModeStateChanged() =>
            this.NotifyEach<IStudioModeAwareCommand>(nameof(IStudioModeAwareCommand.OnStudioModeStateChanged), c => c.OnStudioModeStateChanged());

        public void NotifyInputMonitorTypeChanged(String inputName) =>
            this.NotifyEach<IInputMonitorAwareCommand>(nameof(IInputMonitorAwareCommand.OnInputMonitorTypeChanged), c => c.OnInputMonitorTypeChanged(inputName));

        public void NotifySceneSourcesChanged(String sceneName, String[] sources, String[] audioSources) =>
            this.NotifyEach<ISceneSourcesAwareCommand>(nameof(ISceneSourcesAwareCommand.OnSceneSourcesChanged), c => c.OnSceneSourcesChanged(sceneName, sources, audioSources));

        // Each command is invoked in isolation so one throwing does not stop the remaining commands from being notified.
        private void NotifyEach<T>(String eventName, Action<T> action) where T : IObsCommand
        {
            foreach (var command in this._registry.GetCommands<T>())
            {
                try
                {
                    action(command);
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"Command {command.GetType().Name} threw on {eventName}: {ex.Message}");
                }
            }
        }
    }
}
