namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Moq;
using OBSWebsocketDotNet.Types;

public class OBSActionExecutorTests
{
    private readonly Mock<IOBSWebsocket> _mockObs;
    private readonly Mock<IPluginLog> _mockLog;
    private readonly OBSActionExecutor _executor;

    public OBSActionExecutorTests()
    {
        this._mockObs = new Mock<IOBSWebsocket>();
        this._mockLog = new Mock<IPluginLog>();
        this._executor = new OBSActionExecutor(this._mockObs.Object, this._mockLog.Object);
    }

    [Fact]
    public void GetProfileList_WhenConnected_ReturnsProfiles()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetProfileList()).Returns(new[] { "profile1", "profile2" });

        var result = this._executor.GetProfileList();

        Assert.Equal(2, result.Length);
        Assert.Contains("profile1", result);
    }

    [Fact]
    public void GetProfileList_WhenNotConnected_ReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        var result = this._executor.GetProfileList();

        Assert.Empty(result);
    }

    [Fact]
    public void SetCurrentProfile_WhenConnected_CallsObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        this._executor.SetCurrentProfile("test");

        System.Threading.Thread.Sleep(100);
        this._mockObs.Verify(x => x.SetCurrentProfile("test"), Times.Once);
    }

    [Fact]
    public void SetCurrentProfileState_UpdatesCurrentProfile()
    {
        this._executor.SetCurrentProfileState("myprofile");

        Assert.Equal("myprofile", this._executor.CurrentProfile);
    }

    [Fact]
    public void GetSceneCollectionList_WhenConnected_ReturnsCollections()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetSceneCollectionList()).Returns(new[] { "collection1", "collection2" });

        var result = this._executor.GetSceneCollectionList();

        Assert.Equal(2, result.Length);
        Assert.Contains("collection1", result);
    }

    [Fact]
    public void GetSceneCollectionList_WhenNotConnected_ReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        var result = this._executor.GetSceneCollectionList();

        Assert.Empty(result);
    }

    [Fact]
    public void SetCurrentSceneCollection_WhenConnected_CallsObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        this._executor.SetCurrentSceneCollection("test");

        System.Threading.Thread.Sleep(100);
        this._mockObs.Verify(x => x.SetCurrentSceneCollection("test"), Times.Once);
    }

    [Fact]
    public void SetCurrentSceneCollectionState_UpdatesCurrentSceneCollection()
    {
        this._executor.SetCurrentSceneCollectionState("mycollection");

        Assert.Equal("mycollection", this._executor.CurrentSceneCollection);
    }

    [Fact]
    public void IsRecording_WhenStarted_ReturnsTrue()
    {
        this._executor.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        Assert.True(this._executor.IsRecording);
    }

    [Fact]
    public void IsRecording_WhenPaused_ReturnsTrue()
    {
        this._executor.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_PAUSED);

        Assert.True(this._executor.IsRecording);
    }

    [Fact]
    public void IsRecording_WhenStopped_ReturnsFalse()
    {
        this._executor.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        Assert.False(this._executor.IsRecording);
    }

    [Fact]
    public void IsRecordingPaused_WhenPaused_ReturnsTrue()
    {
        this._executor.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_PAUSED);

        Assert.True(this._executor.IsRecordingPaused);
    }

    [Fact]
    public void IsRecordingPaused_WhenStarted_ReturnsFalse()
    {
        this._executor.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        Assert.False(this._executor.IsRecordingPaused);
    }

    [Fact]
    public void GetSceneList_WhenConnected_ReturnsScenes()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetSceneList()).Returns(new[] { "Scene 1", "Scene 2" });

        var result = this._executor.GetSceneList();

        Assert.Equal(2, result.Length);
        Assert.Contains("Scene 1", result);
    }

    [Fact]
    public void GetSceneList_WhenNotConnected_ReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        var result = this._executor.GetSceneList();

        Assert.Empty(result);
    }

    [Fact]
    public void SetCurrentSceneState_UpdatesCurrentScene()
    {
        this._executor.SetCurrentSceneState("Scene 1");

        Assert.Equal("Scene 1", this._executor.CurrentScene);
    }

    [Fact]
    public void ToggleStreaming_WhenConnected_CallsObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        this._executor.ToggleStreaming();

        System.Threading.Thread.Sleep(100);
        this._mockObs.Verify(x => x.ToggleStream(), Times.Once);
    }

    [Fact]
    public void ToggleStreaming_WhenNotConnected_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        this._executor.ToggleStreaming();

        System.Threading.Thread.Sleep(100);
        this._mockObs.Verify(x => x.ToggleStream(), Times.Never);
    }

    [Fact]
    public void StartStreaming_WhenConnected_CallsObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        this._executor.StartStreaming();

        System.Threading.Thread.Sleep(100);
        this._mockObs.Verify(x => x.StartStream(), Times.Once);
    }

    [Fact]
    public void StartStreaming_WhenAlreadyStreaming_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._executor.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        this._executor.StartStreaming();

        System.Threading.Thread.Sleep(100);
        this._mockObs.Verify(x => x.StartStream(), Times.Never);
    }

    [Fact]
    public void StopStreaming_WhenStreaming_CallsObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._executor.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        this._executor.StopStreaming();

        System.Threading.Thread.Sleep(100);
        this._mockObs.Verify(x => x.StopStream(), Times.Once);
    }

    [Fact]
    public void StopStreaming_WhenNotStreaming_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._executor.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        this._executor.StopStreaming();

        System.Threading.Thread.Sleep(100);
        this._mockObs.Verify(x => x.StopStream(), Times.Never);
    }

    [Fact]
    public void IsStreaming_WhenStarted_ReturnsTrue()
    {
        this._executor.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        Assert.True(this._executor.IsStreaming);
    }

    [Fact]
    public void IsStreaming_WhenStopped_ReturnsFalse()
    {
        this._executor.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        Assert.False(this._executor.IsStreaming);
    }

    [Fact]
    public void IsStreamingChanging_WhenStarting_ReturnsTrue()
    {
        this._executor.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTING);

        Assert.True(this._executor.IsStreamingChanging);
    }

    [Fact]
    public void IsStreamingChanging_WhenStopping_ReturnsTrue()
    {
        this._executor.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPING);

        Assert.True(this._executor.IsStreamingChanging);
    }
}
