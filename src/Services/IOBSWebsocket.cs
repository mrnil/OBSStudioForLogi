namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using OBSWebsocketDotNet.Communication;
    using OBSWebsocketDotNet.Types.Events;

    // Everything the plugin uses from obs-websocket-dotnet, so the executor and OBSWebSocketManager
    // can be driven by a mock in tests. Requests throw when OBS rejects them or times out; the
    // executor turns that into safe defaults, the manager catches it itself.
    public interface IOBSWebsocket
    {
        // Connected and identified - requests are only valid once OBS has confirmed identification.
        Boolean IsConnected { get; }
        TimeSpan RequestTimeout { get; set; }
        void Connect(String url, String password);
        void Disconnect();

        void SetCurrentProgramScene(String sceneName);
        void SetCurrentPreviewScene(String sceneName);
        void StartRecord();
        void StopRecord();
        void ToggleRecord();
        void PauseRecord();
        void ResumeRecord();
        void StartStream();
        void StopStream();
        void ToggleStream();
        void StartVirtualCam();
        void StopVirtualCam();
        void ToggleVirtualCam();
        void StartReplayBuffer();
        void StopReplayBuffer();
        void ToggleReplayBuffer();
        void SaveReplayBuffer();
        String[] GetProfileList();
        (String CurrentProfile, String[] Profiles) GetCurrentProfileAndList();
        void SetCurrentProfile(String profileName);
        String[] GetSceneCollectionList();
        String GetCurrentSceneCollection();
        void SetCurrentSceneCollection(String sceneCollectionName);
        String[] GetSceneList();
        String GetCurrentProgramScene();
        void SaveSourceScreenshot(String sourceName, String imageFormat, String imageFilePath, Int32 imageWidth, Int32 imageHeight);
        String[] GetSceneItemList(String sceneName);
        // The scene's items in OBS order (top first), with the IDs that scene item events carry and
        // each item's visibility - one request.
        (Int32 ItemId, String SourceName, Boolean Enabled)[] GetSceneItems(String sceneName);
        // The visibility of the source's first item in the scene, or false if it isn't there.
        Boolean GetSceneItemEnabled(String sceneName, String sourceName);
        void SetSceneItemEnabled(String sceneName, Int32 sceneItemId, Boolean enabled);
        String[] GetInputList();
        Boolean GetInputMute(String inputName);
        void ToggleInputMute(String inputName);
        void SetInputMute(String inputName, Boolean muted);
        Single GetInputVolume(String inputName);
        void SetInputVolume(String inputName, Single volumeMul);
        String GetInputAudioMonitorType(String inputName);
        void SetInputAudioMonitorType(String inputName, String monitorType);
        String[] GetAudioInputsNotInAnyScene();
        Boolean GetStudioModeEnabled();
        void SetStudioModeEnabled(Boolean enabled);
        void TriggerStudioModeTransition();
        Models.OBSStats GetStats();
        Models.OBSStreamStats GetStreamStatus();
        String GetMediaInputStatus(String inputName);
        void TriggerMediaInputAction(String inputName, String mediaAction);
        String[] GetMediaInputList();

        event EventHandler Connected;
        event EventHandler<ObsDisconnectionInfo> Disconnected;
        event EventHandler<StreamStateChangedEventArgs> StreamStateChanged;
        event EventHandler<RecordStateChangedEventArgs> RecordStateChanged;
        event EventHandler<VirtualcamStateChangedEventArgs> VirtualcamStateChanged;
        event EventHandler<ReplayBufferStateChangedEventArgs> ReplayBufferStateChanged;
        event EventHandler<ReplayBufferSavedEventArgs> ReplayBufferSaved;
        event EventHandler<CurrentProfileChangedEventArgs> CurrentProfileChanged;
        event EventHandler<ProfileListChangedEventArgs> ProfileListChanged;
        event EventHandler<CurrentSceneCollectionChangingEventArgs> CurrentSceneCollectionChanging;
        event EventHandler<CurrentSceneCollectionChangedEventArgs> CurrentSceneCollectionChanged;
        event EventHandler<SceneCollectionListChangedEventArgs> SceneCollectionListChanged;
        event EventHandler<SceneListChangedEventArgs> SceneListChanged;
        event EventHandler<ProgramSceneChangedEventArgs> CurrentProgramSceneChanged;
        event EventHandler<SceneNameChangedEventArgs> SceneNameChanged;
        event EventHandler<StudioModeStateChangedEventArgs> StudioModeStateChanged;
        event EventHandler<SceneItemEnableStateChangedEventArgs> SceneItemEnableStateChanged;
        event EventHandler<SceneItemCreatedEventArgs> SceneItemCreated;
        event EventHandler<SceneItemRemovedEventArgs> SceneItemRemoved;
        event EventHandler<InputCreatedEventArgs> InputCreated;
        event EventHandler<InputRemovedEventArgs> InputRemoved;
        event EventHandler<InputNameChangedEventArgs> InputNameChanged;
        event EventHandler<InputMuteStateChangedEventArgs> InputMuteStateChanged;
        event EventHandler<InputVolumeChangedEventArgs> InputVolumeChanged;
        event EventHandler<InputAudioMonitorTypeChangedEventArgs> InputAudioMonitorTypeChanged;
        event EventHandler<MediaInputPlaybackStartedEventArgs> MediaInputPlaybackStarted;
        event EventHandler<MediaInputPlaybackEndedEventArgs> MediaInputPlaybackEnded;
        event EventHandler<MediaInputActionTriggeredEventArgs> MediaInputActionTriggered;

        // High volume (~20 per second). The library sends the ReIdentify that opts in when the first
        // handler is added and opts back out when the last is removed.
        event EventHandler<InputVolumeMetersEventArgs> InputVolumeMeters;
    }
}
