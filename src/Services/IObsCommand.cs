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

    public interface ISceneCollectionsListAwareCommand : IObsCommand
    {
        void OnSceneCollectionsChanged(String[] sceneCollections, String currentSceneCollection);
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

    // Streaming started, stopped, or is starting/stopping. Read the state from the plugin's
    // IsStreaming.
    public interface IStreamingAwareCommand : IObsCommand
    {
        void OnStreamingStateChanged();
    }

    // Recording started, stopped, paused or resumed, or is starting/stopping. Read the state from
    // the plugin's IsRecording and IsRecordingPaused.
    public interface IRecordingAwareCommand : IObsCommand
    {
        void OnRecordingStateChanged();
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

    public interface ISceneSourcesAwareCommand : IObsCommand
    {
        void OnSceneSourcesChanged(String sceneName, String[] sources, String[] audioSources);
    }

    // StatsService finished a poll; read the new values from OBSStudioForLogiPlugin.GetCurrentStats
    // and GetCurrentStreamStats.
    public interface IStatsAwareCommand : IObsCommand
    {
        void OnStatsUpdated();
    }

    public interface IMediaStateAwareCommand : IObsCommand
    {
        void OnMediaStateChanged(String inputName);
    }

    // Connected, disconnected, or found the WebSocket server disabled in OBS's config. Unlike
    // OnDisconnected, it is sent every time, including the server-disabled case.
    public interface IConnectionStatusAwareCommand : IObsCommand
    {
        void OnConnectionStatusChanged();
    }
}
