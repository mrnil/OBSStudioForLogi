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

        public void NotifyConnected()
        {
            this._registry.NotifyConnected();
        }

        public void NotifyDisconnected()
        {
            this._registry.NotifyDisconnected();
        }

        public void NotifyProfileChanged(String oldProfile, String newProfile)
        {
            this._registry.NotifyProfileChanged(oldProfile, newProfile);
        }

        public void NotifySceneCollectionChanged(String oldSceneCollection, String newSceneCollection)
        {
            this._registry.NotifySceneCollectionChanged(oldSceneCollection, newSceneCollection);
        }

        public void NotifySceneChanged(String sceneName)
        {
            this._registry.NotifySceneChanged(sceneName);
        }

        public void NotifyScenesChanged(String[] scenes)
        {
            this._registry.NotifyScenesChanged(scenes);
        }

        public void NotifyProfilesChanged(String[] profiles, String currentProfile)
        {
            this._registry.NotifyProfilesChanged(profiles, currentProfile);
        }

        public void NotifySourceVisibilityChanged(String sceneName, String sourceName)
        {
            this._registry.NotifySourceVisibilityChanged(sceneName, sourceName);
        }

        public void NotifyInputMuteChanged(String inputName)
        {
            this._registry.NotifyInputMuteChanged(inputName);
        }

        public void NotifyInputVolumeChanged(String inputName)
        {
            this._registry.NotifyInputVolumeChanged(inputName);
        }

        public void NotifyInputsChanged(String[] inputs)
        {
            this._registry.NotifyInputsChanged(inputs);
        }

        public void NotifyVirtualCameraStateChanged()
        {
            this._registry.NotifyVirtualCameraStateChanged();
        }

        public void NotifyReplayBufferStateChanged()
        {
            this._registry.NotifyReplayBufferStateChanged();
        }

        public void NotifyStudioModeStateChanged()
        {
            this._registry.NotifyStudioModeStateChanged();
        }

        public void NotifyInputMonitorTypeChanged(String inputName)
        {
            this._registry.NotifyInputMonitorTypeChanged(inputName);
        }
    }
}
