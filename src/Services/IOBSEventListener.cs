namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    // What OBSWebSocketManager reports after it has applied an OBS event to its own state. The
    // plugin implements it and forwards each call to CommandCoordinator; tests pass a mock. The
    // manager used to call OBSStudioForLogiPlugin.Instance directly, which left its event handling
    // untestable (assessment #20).
    public interface IOBSEventListener
    {
        void OnProfileChanged(String oldProfile, String newProfile);
        void OnProfilesChanged(String[] profiles, String currentProfile);
        void OnSceneCollectionChanged(String oldSceneCollection, String newSceneCollection);
        void OnSceneCollectionsChanged(String[] sceneCollections, String currentSceneCollection);
        void OnScenesChanged(String[] scenes);
        void OnCurrentSceneChanged(String sceneName);

        // Items were added to or removed from a scene, or one was renamed.
        void OnSceneItemsChanged(String sceneName);

        void OnInputsChanged(String[] inputs);
        void OnInputMuteChanged(String inputName);
        void OnInputVolumeChanged(String inputName);
        void OnInputMonitorTypeChanged(String inputName);
        void OnSourceVisibilityChanged(String sceneName, String sourceName);
        void OnStudioModeStateChanged();
        void OnVirtualCameraStateChanged();
        void OnReplayBufferStateChanged();
        void OnReplayBufferSaved(String savedReplayPath);
        void OnMediaPlaybackStateChanged(String inputName);
    }
}
