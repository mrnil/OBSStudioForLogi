namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public interface IObsCommand
    {
        void OnConnected();
        void OnDisconnected();
    }

    public interface IProfileAwareCommand : IObsCommand
    {
        void OnProfileChanged(String oldProfile, String newProfile);
    }

    public interface ISceneCollectionAwareCommand : IObsCommand
    {
        void OnSceneCollectionChanged(String oldSceneCollection, String newSceneCollection);
    }

    public interface ISceneAwareCommand : IObsCommand
    {
        void OnSceneChanged(String sceneName);
    }

    public interface IScenesListAwareCommand : IObsCommand
    {
        void OnScenesChanged(String[] scenes);
    }

    public interface IProfilesListAwareCommand : IObsCommand
    {
        void OnProfilesChanged(String[] profiles, String currentProfile);
    }

    public interface ISourceVisibilityAwareCommand : IObsCommand
    {
        void OnSourceVisibilityChanged(String sceneName, String sourceName);
    }

    public interface IInputMuteAwareCommand : IObsCommand
    {
        void OnInputMuteChanged(String inputName);
    }

    public interface IInputVolumeAwareCommand : IObsCommand
    {
        void OnInputVolumeChanged(String inputName);
    }

    public interface IInputsListAwareCommand : IObsCommand
    {
        void OnInputsChanged(String[] inputs);
    }

    public interface IVirtualCameraAwareCommand : IObsCommand
    {
        void OnVirtualCameraStateChanged();
    }

    public interface IReplayBufferAwareCommand : IObsCommand
    {
        void OnReplayBufferStateChanged();
    }

    public interface IReplayBufferSavedAwareCommand : IObsCommand
    {
        void OnReplayBufferSaved(String savedReplayPath);
    }

    public interface IStudioModeAwareCommand : IObsCommand
    {
        void OnStudioModeStateChanged();
    }

    public interface IInputMonitorAwareCommand : IObsCommand
    {
        void OnInputMonitorTypeChanged(String inputName);
    }
}
