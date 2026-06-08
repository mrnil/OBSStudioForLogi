namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Moq;
using OBSWebsocketDotNet.Types;

/// <summary>
/// Tests verifying that OBSWebSocketManager state properties correctly
/// reflect state changes made through the Actions executor.
/// This validates the observable outcome of the event handlers
/// (OnStreamStateChanged, OnRecordStateChanged, etc.) which set state
/// via Actions.SetXxxState() calls.
/// </summary>
public class OBSWebSocketManagerEventDispatchTests
{
    private readonly Mock<IPluginLog> _mockLog;
    private readonly OBSWebSocketManager _manager;

    public OBSWebSocketManagerEventDispatchTests()
    {
        this._mockLog = new Mock<IPluginLog>();
        this._manager = new OBSWebSocketManager(this._mockLog.Object);
    }

    // --- Streaming state dispatch ---

    [Fact]
    public void IsStreaming_AfterSetStreamingStateStarted_ReturnsTrue()
    {
        this._manager.Actions.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        Assert.True(this._manager.IsStreaming);
    }

    [Fact]
    public void IsStreaming_AfterSetStreamingStateStopped_ReturnsFalse()
    {
        this._manager.Actions.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);
        this._manager.Actions.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        Assert.False(this._manager.IsStreaming);
    }

    [Fact]
    public void IsStreamingChanging_AfterSetStreamingStateStarting_ReturnsTrue()
    {
        this._manager.Actions.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTING);

        Assert.True(this._manager.IsStreamingChanging);
    }

    [Fact]
    public void IsStreamingChanging_AfterSetStreamingStateStopping_ReturnsTrue()
    {
        this._manager.Actions.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPING);

        Assert.True(this._manager.IsStreamingChanging);
    }

    // --- Recording state dispatch ---

    [Fact]
    public void IsRecording_AfterSetRecordingStateStarted_ReturnsTrue()
    {
        this._manager.Actions.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        Assert.True(this._manager.IsRecording);
    }

    [Fact]
    public void IsRecording_AfterSetRecordingStateStopped_ReturnsFalse()
    {
        this._manager.Actions.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);
        this._manager.Actions.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        Assert.False(this._manager.IsRecording);
    }

    [Fact]
    public void IsRecording_AfterSetRecordingStatePaused_ReturnsTrue()
    {
        this._manager.Actions.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_PAUSED);

        Assert.True(this._manager.IsRecording);
    }

    [Fact]
    public void IsRecordingChanging_AfterSetRecordingStateStarting_ReturnsTrue()
    {
        this._manager.Actions.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTING);

        Assert.True(this._manager.IsRecordingChanging);
    }

    // --- Virtual camera state dispatch ---

    [Fact]
    public void Actions_VirtualCameraActive_AfterSetStateStarted()
    {
        this._manager.Actions.SetVirtualCameraState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        Assert.True(this._manager.Actions.IsVirtualCameraActive);
    }

    [Fact]
    public void Actions_VirtualCameraInactive_AfterSetStateStopped()
    {
        this._manager.Actions.SetVirtualCameraState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);
        this._manager.Actions.SetVirtualCameraState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        Assert.False(this._manager.Actions.IsVirtualCameraActive);
    }

    // --- Replay buffer state dispatch ---

    [Fact]
    public void Actions_ReplayBufferActive_AfterSetStateStarted()
    {
        this._manager.Actions.SetReplayBufferState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        Assert.True(this._manager.Actions.IsReplayBufferActive);
    }

    [Fact]
    public void Actions_ReplayBufferInactive_AfterSetStateStopped()
    {
        this._manager.Actions.SetReplayBufferState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);
        this._manager.Actions.SetReplayBufferState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        Assert.False(this._manager.Actions.IsReplayBufferActive);
    }

    // --- Scene state dispatch ---

    [Fact]
    public void Actions_CurrentScene_AfterSetCurrentSceneState()
    {
        this._manager.Actions.SetCurrentSceneState("Gaming");

        Assert.Equal("Gaming", this._manager.Actions.CurrentScene);
    }

    [Fact]
    public void Actions_CurrentScene_UpdatesWhenChanged()
    {
        this._manager.Actions.SetCurrentSceneState("Scene 1");
        this._manager.Actions.SetCurrentSceneState("Scene 2");

        Assert.Equal("Scene 2", this._manager.Actions.CurrentScene);
    }

    // --- Studio mode state dispatch ---

    [Fact]
    public void Actions_StudioModeEnabled_AfterSetStateTrue()
    {
        this._manager.Actions.SetStudioModeState(true);

        Assert.True(this._manager.Actions.IsStudioModeEnabled);
    }

    [Fact]
    public void Actions_StudioModeDisabled_AfterSetStateFalse()
    {
        this._manager.Actions.SetStudioModeState(true);
        this._manager.Actions.SetStudioModeState(false);

        Assert.False(this._manager.Actions.IsStudioModeEnabled);
    }

    // --- Disconnect behaviour ---

    [Fact]
    public void Disconnect_DoesNotResetOutputStates()
    {
        // Disconnect() only sets _shouldReconnect=false and stops timer.
        // Output state reset happens in OnDisconnected event handler
        // which fires asynchronously from the OBS WebSocket library.
        this._manager.Actions.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        this._manager.Disconnect();

        // Streaming state is NOT reset by Disconnect() - only by OnDisconnected
        Assert.True(this._manager.IsStreaming);
        Assert.False(this._manager.ShouldReconnect);
    }

    [Fact]
    public void Disconnect_DisablesReconnection()
    {
        this._manager.Disconnect();

        Assert.False(this._manager.IsConnected);
        Assert.False(this._manager.ShouldReconnect);
    }
}
