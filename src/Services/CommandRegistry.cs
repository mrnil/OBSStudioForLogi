namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CommandRegistry
    {
        private readonly List<IObsCommand> _commands = new List<IObsCommand>();

        public void Register(IObsCommand command)
        {
            if (command != null && !this._commands.Contains(command))
            {
                this._commands.Add(command);
            }
        }

        public void NotifyConnected()
        {
            foreach (var command in this._commands)
            {
                command.OnConnected();
            }
        }

        public void NotifyDisconnected()
        {
            foreach (var command in this._commands)
            {
                command.OnDisconnected();
            }
        }

        public void NotifyProfileChanged(String oldProfile, String newProfile)
        {
            foreach (var command in this._commands.OfType<IProfileAwareCommand>())
            {
                command.OnProfileChanged(oldProfile, newProfile);
            }
        }

        public void NotifySceneCollectionChanged(String oldSceneCollection, String newSceneCollection)
        {
            foreach (var command in this._commands.OfType<ISceneCollectionAwareCommand>())
            {
                command.OnSceneCollectionChanged(oldSceneCollection, newSceneCollection);
            }
        }

        public void NotifySceneChanged(String sceneName)
        {
            foreach (var command in this._commands.OfType<ISceneAwareCommand>())
            {
                command.OnSceneChanged(sceneName);
            }
        }

        public void NotifyScenesChanged(String[] scenes)
        {
            foreach (var command in this._commands.OfType<IScenesListAwareCommand>())
            {
                command.OnScenesChanged(scenes);
            }
        }

        public void NotifyProfilesChanged(String[] profiles, String currentProfile)
        {
            foreach (var command in this._commands.OfType<IProfilesListAwareCommand>())
            {
                command.OnProfilesChanged(profiles, currentProfile);
            }
        }

        public void NotifySourceVisibilityChanged(String sceneName, String sourceName)
        {
            foreach (var command in this._commands.OfType<ISourceVisibilityAwareCommand>())
            {
                command.OnSourceVisibilityChanged(sceneName, sourceName);
            }
        }

        public void NotifyInputMuteChanged(String inputName)
        {
            foreach (var command in this._commands.OfType<IInputMuteAwareCommand>())
            {
                command.OnInputMuteChanged(inputName);
            }
        }

        public void NotifyInputVolumeChanged(String inputName)
        {
            foreach (var command in this._commands.OfType<IInputVolumeAwareCommand>())
            {
                command.OnInputVolumeChanged(inputName);
            }
        }

        public void NotifyInputsChanged(String[] inputs)
        {
            foreach (var command in this._commands.OfType<IInputsListAwareCommand>())
            {
                command.OnInputsChanged(inputs);
            }
        }

        public void NotifyVirtualCameraStateChanged()
        {
            foreach (var command in this._commands.OfType<IVirtualCameraAwareCommand>())
            {
                command.OnVirtualCameraStateChanged();
            }
        }

        public void NotifyReplayBufferStateChanged()
        {
            foreach (var command in this._commands.OfType<IReplayBufferAwareCommand>())
            {
                command.OnReplayBufferStateChanged();
            }
        }

        public void NotifyStudioModeStateChanged()
        {
            foreach (var command in this._commands.OfType<IStudioModeAwareCommand>())
            {
                command.OnStudioModeStateChanged();
            }
        }
    }
}
