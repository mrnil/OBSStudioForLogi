namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System;
using System.Collections.Generic;
using Moq;
using OBSWebsocketDotNet.Communication;
using OBSWebsocketDotNet.Types;
using OBSWebsocketDotNet.Types.Events;

/// <summary>
/// Drives OBSWebSocketManager's OBS event handlers through a mocked IOBSWebsocket and checks what
/// the manager does with each event: the state it records and what it reports to its
/// IOBSEventListener. The manager runs its background work inline here (assessment #20).
/// </summary>
public class OBSWebSocketManagerEventHandlingTests : IDisposable
{
    private readonly Mock<IOBSWebsocket> _mockObs;
    private readonly Mock<IOBSEventListener> _mockListener;
    private readonly Mock<IPluginLog> _mockLog;
    private readonly OBSWebSocketManager _manager;

    public OBSWebSocketManagerEventHandlingTests()
    {
        this._mockObs = new Mock<IOBSWebsocket>();
        this._mockObs.SetupProperty(x => x.RequestTimeout);
        this._mockListener = new Mock<IOBSEventListener>();
        this._mockLog = new Mock<IPluginLog>();
        this._manager = new OBSWebSocketManager(this._mockObs.Object, this._mockListener.Object, this._mockLog.Object, action => action());
    }

    public void Dispose()
    {
        this._manager.Dispose();
    }

    private static OutputStateChanged Output(OutputState state)
    {
        return new OutputStateChanged { StateStr = state.ToString() };
    }

    // --- Construction ---

    [Fact]
    public void Constructor_SetsRequestTimeoutBeforeConnecting()
    {
        Assert.Equal(TimeSpan.FromMilliseconds(Helpers.OBSTimings.RequestTimeout), this._mockObs.Object.RequestTimeout);
    }

    [Fact]
    public void IsConnected_ReflectsTheWebsocket()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        Assert.True(this._manager.IsConnected);
    }

    // --- Connection ---

    [Fact]
    public void Connected_LoadsInitialStateAndReportsIt()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetCurrentProfileAndList()).Returns(("Profile1", new[] { "Profile1", "Profile2" }));
        this._mockObs.Setup(x => x.GetCurrentSceneCollection()).Returns("Collection1");
        this._mockObs.Setup(x => x.GetSceneCollectionList()).Returns(new[] { "Collection1" });
        this._mockObs.Setup(x => x.GetCurrentProgramScene()).Returns("Scene1");
        this._mockObs.Setup(x => x.GetSceneList()).Returns(new[] { "Scene1", "Scene2" });
        this._mockObs.Setup(x => x.GetInputList()).Returns(new[] { "Mic" });
        this._mockObs.Setup(x => x.GetStudioModeEnabled()).Returns(true);
        Boolean established = false;
        Boolean loadFinished = false;
        this._manager.ConnectionEstablished += (s, e) => established = true;
        this._manager.InitialStateLoadFinished += (s, e) => loadFinished = true;

        this._mockObs.Raise(x => x.Connected += null, EventArgs.Empty);

        Assert.True(established);
        Assert.True(loadFinished);
        Assert.Equal("Profile1", this._manager.Actions.CurrentProfile);
        Assert.Equal("Collection1", this._manager.Actions.CurrentSceneCollection);
        Assert.Equal("Scene1", this._manager.Actions.CurrentScene);
        Assert.True(this._manager.Actions.IsStudioModeEnabled);
        this._mockListener.Verify(x => x.OnProfileChanged(String.Empty, "Profile1"), Times.Once);
        this._mockListener.Verify(x => x.OnProfilesChanged(new[] { "Profile1", "Profile2" }, "Profile1"), Times.Once);
        this._mockListener.Verify(x => x.OnSceneCollectionChanged(String.Empty, "Collection1"), Times.Once);
        this._mockListener.Verify(x => x.OnSceneCollectionsChanged(new[] { "Collection1" }, "Collection1"), Times.Once);
        this._mockListener.Verify(x => x.OnCurrentSceneChanged("Scene1"), Times.Once);
        this._mockListener.Verify(x => x.OnScenesChanged(new[] { "Scene1", "Scene2" }), Times.Once);
        this._mockListener.Verify(x => x.OnInputsChanged(new[] { "Mic" }), Times.Once);
        this._mockListener.Verify(x => x.OnStudioModeStateChanged(), Times.Once);
    }

    [Fact]
    public void Connected_RepeatedForTheSameSession_LoadsOnce()
    {
        this._mockObs.Setup(x => x.GetCurrentProfileAndList()).Returns(("Profile1", new[] { "Profile1" }));

        this._mockObs.Raise(x => x.Connected += null, EventArgs.Empty);
        this._mockObs.Raise(x => x.Connected += null, EventArgs.Empty);

        this._mockObs.Verify(x => x.GetCurrentProfileAndList(), Times.Once);
    }

    [Fact]
    public void Disconnected_ResetsOutputStatesAndRaisesConnectionLost()
    {
        this._manager.Actions.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);
        this._manager.Actions.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);
        this._manager.Actions.SetStudioModeState(true);
        Boolean lost = false;
        this._manager.ConnectionLost += (s, e) => lost = true;

        ObsDisconnectionInfo info = new ObsDisconnectionInfo(ObsCloseCodes.UnknownReason, "OBS closed", null);

        // ObsDisconnectionInfo isn't an EventArgs, so Moq needs the sender as well.
        this._mockObs.Raise(x => x.Disconnected += null, this._mockObs.Object, info);

        Assert.True(lost);
        Assert.False(this._manager.IsStreaming);
        Assert.False(this._manager.IsRecording);
        Assert.False(this._manager.Actions.IsStudioModeEnabled);
    }

    // --- Outputs ---

    [Fact]
    public void VirtualcamStateChanged_RecordsStateAndNotifies()
    {
        this._mockObs.Raise(x => x.VirtualcamStateChanged += null, new VirtualcamStateChangedEventArgs(Output(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED)));

        Assert.True(this._manager.Actions.IsVirtualCameraActive);
        this._mockListener.Verify(x => x.OnVirtualCameraStateChanged(), Times.Once);
    }

    [Fact]
    public void ReplayBufferStateChanged_RecordsStateAndNotifies()
    {
        this._mockObs.Raise(x => x.ReplayBufferStateChanged += null, new ReplayBufferStateChangedEventArgs(Output(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED)));

        Assert.True(this._manager.Actions.IsReplayBufferActive);
        this._mockListener.Verify(x => x.OnReplayBufferStateChanged(), Times.Once);
    }

    [Fact]
    public void ReplayBufferSaved_ReportsThePath()
    {
        this._mockObs.Raise(x => x.ReplayBufferSaved += null, new ReplayBufferSavedEventArgs(@"C:\Videos\replay.mkv"));

        this._mockListener.Verify(x => x.OnReplayBufferSaved(@"C:\Videos\replay.mkv"), Times.Once);
    }

    [Fact]
    public void StudioModeStateChanged_RecordsStateAndNotifies()
    {
        this._mockObs.Raise(x => x.StudioModeStateChanged += null, new StudioModeStateChangedEventArgs(true));

        Assert.True(this._manager.Actions.IsStudioModeEnabled);
        this._mockListener.Verify(x => x.OnStudioModeStateChanged(), Times.Once);
    }

    // --- Profiles, collections and scenes ---

    [Fact]
    public void CurrentProfileChanged_ReadsTheProfileAndReportsTheChangeAndList()
    {
        this._manager.Actions.SetCurrentProfileState("Old");
        this._mockObs.Setup(x => x.GetCurrentProfileAndList()).Returns(("New", new[] { "Old", "New" }));

        this._mockObs.Raise(x => x.CurrentProfileChanged += null, new CurrentProfileChangedEventArgs("New"));

        Assert.Equal("New", this._manager.Actions.CurrentProfile);
        this._mockListener.Verify(x => x.OnProfileChanged("Old", "New"), Times.Once);
        this._mockListener.Verify(x => x.OnProfilesChanged(new[] { "Old", "New" }, "New"), Times.Once);
    }

    [Fact]
    public void CurrentProfileChanged_WhenTheReadFails_LogsAndReportsNothing()
    {
        this._mockObs.Setup(x => x.GetCurrentProfileAndList()).Throws(new Exception("OBS error"));

        this._mockObs.Raise(x => x.CurrentProfileChanged += null, new CurrentProfileChangedEventArgs("New"));

        this._mockListener.Verify(x => x.OnProfileChanged(It.IsAny<String>(), It.IsAny<String>()), Times.Never);
        this._mockLog.Verify(x => x.Warning(It.Is<String>(s => s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void ProfileListChanged_ReportsTheListFromTheEvent()
    {
        this._manager.Actions.SetCurrentProfileState("Profile1");

        this._mockObs.Raise(x => x.ProfileListChanged += null, new ProfileListChangedEventArgs(new List<String> { "Profile1", "Profile3" }));

        this._mockListener.Verify(x => x.OnProfilesChanged(new[] { "Profile1", "Profile3" }, "Profile1"), Times.Once);
        this._mockObs.Verify(x => x.GetCurrentProfileAndList(), Times.Never);
    }

    [Fact]
    public void CurrentSceneCollectionChanged_ReportsCollectionScenesCurrentSceneAndList()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._manager.Actions.SetCurrentSceneCollectionState("Old");
        this._mockObs.Setup(x => x.GetSceneList()).Returns(new[] { "SceneA" });
        this._mockObs.Setup(x => x.GetCurrentProgramScene()).Returns("SceneA");
        this._mockObs.Setup(x => x.GetSceneCollectionList()).Returns(new[] { "Old", "New" });
        Boolean switchFinished = false;
        this._manager.SceneCollectionSwitchFinished += (s, e) => switchFinished = true;

        this._mockObs.Raise(x => x.CurrentSceneCollectionChanged += null, new CurrentSceneCollectionChangedEventArgs("New"));

        Assert.True(switchFinished);
        Assert.Equal("New", this._manager.Actions.CurrentSceneCollection);
        Assert.Equal("SceneA", this._manager.Actions.CurrentScene);
        this._mockListener.Verify(x => x.OnSceneCollectionChanged("Old", "New"), Times.Once);
        this._mockListener.Verify(x => x.OnScenesChanged(new[] { "SceneA" }), Times.Once);
        this._mockListener.Verify(x => x.OnCurrentSceneChanged("SceneA"), Times.Once);
        this._mockListener.Verify(x => x.OnSceneCollectionsChanged(new[] { "Old", "New" }, "New"), Times.Once);
    }

    [Fact]
    public void CurrentSceneCollectionChanging_RaisesSwitchStarted()
    {
        Boolean switchStarted = false;
        this._manager.SceneCollectionSwitchStarted += (s, e) => switchStarted = true;

        this._mockObs.Raise(x => x.CurrentSceneCollectionChanging += null, new CurrentSceneCollectionChangingEventArgs("Old"));

        Assert.True(switchStarted);
    }

    [Fact]
    public void CurrentProgramSceneChanged_RecordsSceneAndNotifies()
    {
        this._mockObs.Raise(x => x.CurrentProgramSceneChanged += null, new ProgramSceneChangedEventArgs("Scene2"));

        Assert.Equal("Scene2", this._manager.Actions.CurrentScene);
        this._mockListener.Verify(x => x.OnCurrentSceneChanged("Scene2"), Times.Once);
    }

    [Fact]
    public void CurrentProgramSceneChanged_WithoutSceneName_IsIgnored()
    {
        this._mockObs.Raise(x => x.CurrentProgramSceneChanged += null, new ProgramSceneChangedEventArgs(null));

        this._mockListener.Verify(x => x.OnCurrentSceneChanged(It.IsAny<String>()), Times.Never);
    }

    [Fact]
    public void SceneListChanged_ReportsTheNewList()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetSceneList()).Returns(new[] { "Scene1", "Scene3" });

        this._mockObs.Raise(x => x.SceneListChanged += null, new SceneListChangedEventArgs(null));

        this._mockListener.Verify(x => x.OnScenesChanged(new[] { "Scene1", "Scene3" }), Times.Once);
    }

    [Fact]
    public void SceneNameChanged_ForTheCurrentScene_ReportsTheNewName()
    {
        this._manager.Actions.SetCurrentSceneState("Old");

        this._mockObs.Raise(x => x.SceneNameChanged += null, new SceneNameChangedEventArgs("Old", "New"));

        Assert.Equal("New", this._manager.Actions.CurrentScene);
        this._mockListener.Verify(x => x.OnCurrentSceneChanged("New"), Times.Once);
    }

    // --- Scene items ---

    [Fact]
    public void SceneItemEnableStateChanged_CachesVisibilityAndNotifies()
    {
        this._mockObs.Setup(x => x.GetSceneItems("Scene1")).Returns(new[] { (1, "Camera"), (2, "Overlay") });

        this._mockObs.Raise(x => x.SceneItemEnableStateChanged += null, new SceneItemEnableStateChangedEventArgs("Scene1", 2, true));

        Assert.True(this._manager.SourceVisibility.Get(("Scene1", "Overlay")));
        this._mockListener.Verify(x => x.OnSourceVisibilityChanged("Scene1", "Overlay"), Times.Once);
    }

    [Fact]
    public void SceneItemEnableStateChanged_ForADuplicateItem_DoesNotOverwriteTheFirstItemsState()
    {
        this._manager.SourceVisibility.Set(("Scene1", "Camera"), true);
        this._mockObs.Setup(x => x.GetSceneItems("Scene1")).Returns(new[] { (1, "Camera"), (5, "Camera") });

        this._mockObs.Raise(x => x.SceneItemEnableStateChanged += null, new SceneItemEnableStateChangedEventArgs("Scene1", 5, false));

        Assert.True(this._manager.SourceVisibility.Get(("Scene1", "Camera")));
        this._mockListener.Verify(x => x.OnSourceVisibilityChanged("Scene1", "Camera"), Times.Once);
    }

    [Fact]
    public void SceneItemEnableStateChanged_WhenTheItemLookupFails_DropsTheScenesCachedVisibility()
    {
        this._manager.SourceVisibility.Set(("Scene1", "Camera"), true);
        this._mockObs.Setup(x => x.GetSceneItems("Scene1")).Throws(new Exception("OBS error"));

        this._mockObs.Raise(x => x.SceneItemEnableStateChanged += null, new SceneItemEnableStateChangedEventArgs("Scene1", 1, false));

        // Disconnected, so the re-read the cache starts fails and the default (hidden) is returned.
        Assert.False(this._manager.SourceVisibility.Get(("Scene1", "Camera")));
        this._mockListener.Verify(x => x.OnSourceVisibilityChanged(It.IsAny<String>(), It.IsAny<String>()), Times.Never);
    }

    [Fact]
    public void SceneItemCreated_ReportsTheScenesItemsChanged()
    {
        this._mockObs.Raise(x => x.SceneItemCreated += null, new SceneItemCreatedEventArgs("Scene1", "Camera", 3, 0));

        this._mockListener.Verify(x => x.OnSceneItemsChanged("Scene1"), Times.Once);
    }

    [Fact]
    public void SceneItemRemoved_ReportsTheScenesItemsChanged()
    {
        this._mockObs.Raise(x => x.SceneItemRemoved += null, new SceneItemRemovedEventArgs("Scene1", "Camera", 3));

        this._mockListener.Verify(x => x.OnSceneItemsChanged("Scene1"), Times.Once);
    }

    // --- Inputs and audio ---

    [Fact]
    public void InputCreated_ReportsTheRefreshedInputList()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetInputList()).Returns(new[] { "Mic", "Desktop" });

        this._mockObs.Raise(x => x.InputCreated += null, new InputCreatedEventArgs("Desktop", "wasapi_output_capture", "wasapi_output_capture", null, null));

        this._mockListener.Verify(x => x.OnInputsChanged(new[] { "Mic", "Desktop" }), Times.Once);
    }

    [Fact]
    public void InputRemoved_ReportsTheRefreshedInputList()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetInputList()).Returns(new[] { "Mic" });

        this._mockObs.Raise(x => x.InputRemoved += null, new InputRemovedEventArgs("Desktop"));

        this._mockListener.Verify(x => x.OnInputsChanged(new[] { "Mic" }), Times.Once);
    }

    [Fact]
    public void InputNameChanged_RefreshesInputsAndTheCurrentScenesItems()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetInputList()).Returns(new[] { "Voice" });
        this._manager.Actions.SetCurrentSceneState("Scene1");

        this._mockObs.Raise(x => x.InputNameChanged += null, new InputNameChangedEventArgs("Mic", "Voice"));

        this._mockListener.Verify(x => x.OnInputsChanged(new[] { "Voice" }), Times.Once);
        this._mockListener.Verify(x => x.OnSceneItemsChanged("Scene1"), Times.Once);
    }

    [Fact]
    public void InputMuteStateChanged_RecordsMuteAndNotifies()
    {
        this._mockObs.Raise(x => x.InputMuteStateChanged += null, new InputMuteStateChangedEventArgs("Mic", true));

        Assert.True(this._manager.AudioState.Get("Mic").IsMuted);
        this._mockListener.Verify(x => x.OnInputMuteChanged("Mic"), Times.Once);
    }

    [Fact]
    public void InputVolumeChanged_RecordsVolumeAndNotifies()
    {
        this._mockObs.Raise(x => x.InputVolumeChanged += null, new InputVolumeChangedEventArgs(new InputVolume { InputName = "Mic", InputVolumeMul = 0.5f }));

        Assert.Equal(0.5f, this._manager.AudioState.Get("Mic").VolumeMul);
        this._mockListener.Verify(x => x.OnInputVolumeChanged("Mic"), Times.Once);
    }

    [Fact]
    public void InputAudioMonitorTypeChanged_RecordsTypeAndNotifies()
    {
        this._mockObs.Raise(x => x.InputAudioMonitorTypeChanged += null, new InputAudioMonitorTypeChangedEventArgs("Mic", "OBS_MONITORING_TYPE_MONITOR_ONLY"));

        Assert.Equal("OBS_MONITORING_TYPE_MONITOR_ONLY", this._manager.AudioState.Get("Mic").MonitorType);
        this._mockListener.Verify(x => x.OnInputMonitorTypeChanged("Mic"), Times.Once);
    }

    // --- Media ---

    [Fact]
    public void MediaInputPlaybackStarted_RecordsPlayingAndNotifies()
    {
        this._mockObs.Raise(x => x.MediaInputPlaybackStarted += null, new MediaInputPlaybackStartedEventArgs("Video"));

        Assert.Equal(Helpers.MediaInputStates.Playing, this._manager.MediaState.Get("Video"));
        this._mockListener.Verify(x => x.OnMediaPlaybackStateChanged("Video"), Times.Once);
    }

    [Fact]
    public void MediaInputPlaybackEnded_RecordsEndedAndNotifies()
    {
        this._mockObs.Raise(x => x.MediaInputPlaybackEnded += null, new MediaInputPlaybackEndedEventArgs("Video"));

        Assert.Equal(Helpers.MediaInputStates.Ended, this._manager.MediaState.Get("Video"));
        this._mockListener.Verify(x => x.OnMediaPlaybackStateChanged("Video"), Times.Once);
    }

    [Fact]
    public void MediaInputActionTriggered_RecordsTheResultingStateAndNotifies()
    {
        this._mockObs.Raise(x => x.MediaInputActionTriggered += null, new MediaInputActionTriggeredEventArgs("Video", Helpers.MediaInputStates.ActionPause));

        Assert.Equal(Helpers.MediaInputStates.Paused, this._manager.MediaState.Get("Video"));
        this._mockListener.Verify(x => x.OnMediaPlaybackStateChanged("Video"), Times.Once);
    }

    // --- Disposal ---

    [Fact]
    public void Dispose_UnsubscribesFromTheWebsocket()
    {
        this._manager.Dispose();

        this._mockObs.Raise(x => x.CurrentProgramSceneChanged += null, new ProgramSceneChangedEventArgs("Scene2"));

        this._mockListener.Verify(x => x.OnCurrentSceneChanged(It.IsAny<String>()), Times.Never);
        this._mockObs.Verify(x => x.Disconnect(), Times.Once);
    }
}
