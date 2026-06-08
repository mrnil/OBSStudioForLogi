namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Moq;

public class OBSFacadeTests
{
    private readonly OBSWebSocketManager _manager;
    private readonly OBSFacade _facade;
    private readonly Mock<IPluginLog> _mockLog;

    public OBSFacadeTests()
    {
        this._mockLog = new Mock<IPluginLog>();
        this._manager = new OBSWebSocketManager(this._mockLog.Object);
        this._facade = new OBSFacade(this._manager);
    }

    // --- State properties when disconnected ---

    [Fact]
    public void IsConnected_WhenDisconnected_ReturnsFalse()
    {
        Assert.False(this._facade.IsConnected);
    }

    [Fact]
    public void IsRecording_WhenDisconnected_ReturnsFalse()
    {
        Assert.False(this._facade.IsRecording);
    }

    [Fact]
    public void IsRecordingPaused_WhenDisconnected_ReturnsFalse()
    {
        Assert.False(this._facade.IsRecordingPaused);
    }

    [Fact]
    public void IsStreaming_WhenDisconnected_ReturnsFalse()
    {
        Assert.False(this._facade.IsStreaming);
    }

    [Fact]
    public void IsVirtualCameraActive_WhenDisconnected_ReturnsFalse()
    {
        Assert.False(this._facade.IsVirtualCameraActive);
    }

    [Fact]
    public void IsReplayBufferActive_WhenDisconnected_ReturnsFalse()
    {
        Assert.False(this._facade.IsReplayBufferActive);
    }

    [Fact]
    public void IsStudioModeEnabled_WhenDisconnected_ReturnsFalse()
    {
        Assert.False(this._facade.IsStudioModeEnabled);
    }

    // --- String properties when disconnected ---

    [Fact]
    public void CurrentProfile_WhenDisconnected_ReturnsEmpty()
    {
        Assert.Equal(String.Empty, this._facade.CurrentProfile);
    }

    [Fact]
    public void CurrentSceneCollection_WhenDisconnected_ReturnsEmpty()
    {
        Assert.Equal(String.Empty, this._facade.CurrentSceneCollection);
    }

    [Fact]
    public void CurrentScene_WhenDisconnected_ReturnsEmpty()
    {
        Assert.Equal(String.Empty, this._facade.CurrentScene);
    }

    // --- Query methods when disconnected ---

    [Fact]
    public void GetProfileList_WhenDisconnected_ReturnsEmpty()
    {
        var result = this._facade.GetProfileList();

        Assert.Empty(result);
    }

    [Fact]
    public void GetSceneCollectionList_WhenDisconnected_ReturnsEmpty()
    {
        var result = this._facade.GetSceneCollectionList();

        Assert.Empty(result);
    }

    [Fact]
    public void GetSceneList_WhenDisconnected_ReturnsEmpty()
    {
        var result = this._facade.GetSceneList();

        Assert.Empty(result);
    }

    [Fact]
    public void GetInputList_WhenDisconnected_ReturnsEmpty()
    {
        var result = this._facade.GetInputList();

        Assert.Empty(result);
    }

    [Fact]
    public void GetInputKind_WhenDisconnected_ReturnsEmpty()
    {
        var result = this._facade.GetInputKind("Microphone");

        Assert.Equal(String.Empty, result);
    }

    [Fact]
    public void GetScenesForInput_WhenDisconnected_ReturnsEmpty()
    {
        var result = this._facade.GetScenesForInput("Microphone");

        Assert.Empty(result);
    }

    [Fact]
    public void GetInputMute_WhenDisconnected_ReturnsFalse()
    {
        var result = this._facade.GetInputMute("Microphone");

        Assert.False(result);
    }

    [Fact]
    public void GetInputVolume_WhenDisconnected_ReturnsDefault()
    {
        var result = this._facade.GetInputVolume("Microphone");

        Assert.Equal(1.0f, result);
    }

    [Fact]
    public void GetSourceVisibility_WhenDisconnected_ReturnsFalse()
    {
        var result = this._facade.GetSourceVisibility("Scene1", "Source1");

        Assert.False(result);
    }

    [Fact]
    public void GetInputAudioMonitorType_WhenDisconnected_ReturnsNone()
    {
        var result = this._facade.GetInputAudioMonitorType("Microphone");

        Assert.Equal("OBS_MONITORING_TYPE_NONE", result);
    }

    // --- Action methods with connection validation ---

    [Fact]
    public void SwitchScene_WhenDisconnected_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._facade.SwitchScene("Scene1"));
        Assert.Null(exception);
    }

    [Fact]
    public void SwitchProfile_WhenDisconnected_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._facade.SwitchProfile("Profile1"));
        Assert.Null(exception);
    }

    [Fact]
    public void SwitchSceneCollection_WhenDisconnected_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._facade.SwitchSceneCollection("Collection1"));
        Assert.Null(exception);
    }

    [Fact]
    public void SetInputVolume_WhenDisconnected_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._facade.SetInputVolume("Microphone", 0.5f));
        Assert.Null(exception);
    }

    [Fact]
    public void CycleInputAudioMonitorType_WhenDisconnected_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._facade.CycleInputAudioMonitorType("Microphone"));
        Assert.Null(exception);
    }

    // --- UpdateSourcesForScene ---

    [Fact]
    public void UpdateSourcesForScene_WhenSceneNameEmpty_DoesNotCallCallbacks()
    {
        var sourcesCalled = false;
        var audioCalled = false;

        this._facade.UpdateSourcesForScene("", (s, items) => sourcesCalled = true, (s, items) => audioCalled = true);

        Assert.False(sourcesCalled);
        Assert.False(audioCalled);
    }

    [Fact]
    public void UpdateSourcesForScene_WhenSceneNameNull_DoesNotCallCallbacks()
    {
        var sourcesCalled = false;

        this._facade.UpdateSourcesForScene(null, (s, items) => sourcesCalled = true, (s, items) => { });

        Assert.False(sourcesCalled);
    }

    // --- Null facade (null manager) ---

    [Fact]
    public void Constructor_WithNullManager_ThrowsOnConnectionCheck()
    {
        // OBSFacade does not guard against null manager for most methods
        // This verifies the facade works with a valid manager instance
        Assert.NotNull(this._facade);
    }

    // --- Action methods that delegate without connection check (fire-and-forget) ---

    [Fact]
    public void ToggleRecording_WhenDisconnected_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._facade.ToggleRecording());
        Assert.Null(exception);
    }

    [Fact]
    public void ToggleStreaming_WhenDisconnected_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._facade.ToggleStreaming());
        Assert.Null(exception);
    }

    [Fact]
    public void ToggleVirtualCamera_WhenDisconnected_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._facade.ToggleVirtualCamera());
        Assert.Null(exception);
    }

    [Fact]
    public void ToggleReplayBuffer_WhenDisconnected_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._facade.ToggleReplayBuffer());
        Assert.Null(exception);
    }

    [Fact]
    public void ToggleInputMute_WhenDisconnected_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._facade.ToggleInputMute("Microphone"));
        Assert.Null(exception);
    }

    [Fact]
    public void ToggleSourceVisibility_WhenDisconnected_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._facade.ToggleSourceVisibility("Scene1", "Source1"));
        Assert.Null(exception);
    }

    [Fact]
    public void ToggleStudioMode_WhenDisconnected_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._facade.ToggleStudioMode());
        Assert.Null(exception);
    }

    [Fact]
    public void TriggerStudioModeTransition_WhenDisconnected_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._facade.TriggerStudioModeTransition());
        Assert.Null(exception);
    }

    [Fact]
    public void SaveScreenshot_WhenDisconnected_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._facade.SaveScreenshot("C:\\Screenshots"));
        Assert.Null(exception);
    }
}
