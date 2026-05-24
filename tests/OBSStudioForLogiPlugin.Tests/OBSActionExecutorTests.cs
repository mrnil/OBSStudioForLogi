namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Loupedeck.OBSStudioForLogiPlugin.Helpers;
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

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
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

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
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

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.ToggleStream(), Times.Once);
    }

    [Fact]
    public void ToggleStreaming_WhenNotConnected_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        this._executor.ToggleStreaming();

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.ToggleStream(), Times.Never);
    }

    [Fact]
    public void StartStreaming_WhenConnected_CallsObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        this._executor.StartStreaming();

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.StartStream(), Times.Once);
    }

    [Fact]
    public void StartStreaming_WhenAlreadyStreaming_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._executor.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        this._executor.StartStreaming();

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.StartStream(), Times.Never);
    }

    [Fact]
    public void StopStreaming_WhenStreaming_CallsObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._executor.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        this._executor.StopStreaming();

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.StopStream(), Times.Once);
    }

    [Fact]
    public void StopStreaming_WhenNotStreaming_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._executor.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        this._executor.StopStreaming();

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
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

    // --- Error logging tests ---

    [Fact]
    public void SetCurrentScene_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.SetCurrentProgramScene(It.IsAny<String>())).Throws(new Exception("OBS error"));

        this._executor.SetCurrentScene("Scene1");
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("Scene1") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void ToggleRecording_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.ToggleRecord()).Throws(new Exception("OBS error"));
        this._executor.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        this._executor.ToggleRecording();
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("toggle recording") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void StartRecording_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.StartRecord()).Throws(new Exception("OBS error"));
        this._executor.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        this._executor.StartRecording();
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("start recording") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void StopRecording_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.StopRecord()).Throws(new Exception("OBS error"));
        this._executor.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        this._executor.StopRecording();
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("stop recording") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void ToggleRecordingPause_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.PauseRecord()).Throws(new Exception("OBS error"));
        this._executor.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        this._executor.ToggleRecordingPause();
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("toggle recording pause") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void GetProfileList_WhenOBSThrows_LogsErrorAndReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetProfileList()).Throws(new Exception("OBS error"));

        var result = this._executor.GetProfileList();

        Assert.Empty(result);
        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("profile list") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void SetCurrentProfile_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.SetCurrentProfile(It.IsAny<String>())).Throws(new Exception("OBS error"));

        this._executor.SetCurrentProfile("TestProfile");
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("TestProfile") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void GetSceneCollectionList_WhenOBSThrows_LogsErrorAndReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetSceneCollectionList()).Throws(new Exception("OBS error"));

        var result = this._executor.GetSceneCollectionList();

        Assert.Empty(result);
        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("scene collection list") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void SetCurrentSceneCollection_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.SetCurrentSceneCollection(It.IsAny<String>())).Throws(new Exception("OBS error"));

        this._executor.SetCurrentSceneCollection("TestCollection");
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("TestCollection") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void GetSceneList_WhenOBSThrows_LogsErrorAndReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetSceneList()).Throws(new Exception("OBS error"));

        var result = this._executor.GetSceneList();

        Assert.Empty(result);
        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("scene list") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void SaveScreenshot_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.SaveSourceScreenshot(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>(), It.IsAny<Int32>(), It.IsAny<Int32>()))
                     .Throws(new Exception("OBS error"));
        this._executor.SetCurrentSceneState("Scene1");

        this._executor.SaveScreenshot("C:\\Screenshots");
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("screenshot") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void ToggleStreaming_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.ToggleStream()).Throws(new Exception("OBS error"));
        this._executor.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        this._executor.ToggleStreaming();
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("toggle streaming") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void StartStreaming_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.StartStream()).Throws(new Exception("OBS error"));
        this._executor.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        this._executor.StartStreaming();
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("start streaming") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void StopStreaming_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.StopStream()).Throws(new Exception("OBS error"));
        this._executor.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        this._executor.StopStreaming();
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("stop streaming") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void ToggleVirtualCamera_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.StartVirtualCam()).Throws(new Exception("OBS error"));
        this._executor.SetVirtualCameraState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        this._executor.ToggleVirtualCamera();
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("toggle virtual camera") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void StartVirtualCamera_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.StartVirtualCam()).Throws(new Exception("OBS error"));
        this._executor.SetVirtualCameraState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        this._executor.StartVirtualCamera();
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("start virtual camera") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void StopVirtualCamera_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.StopVirtualCam()).Throws(new Exception("OBS error"));
        this._executor.SetVirtualCameraState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        this._executor.StopVirtualCamera();
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("stop virtual camera") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void GetSceneItemList_WhenOBSThrows_LogsErrorAndReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetSceneItemList(It.IsAny<String>())).Throws(new Exception("OBS error"));

        var result = this._executor.GetSceneItemList("Scene1");

        Assert.Empty(result);
        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("Scene1") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void GetSceneItemEnabled_WhenOBSThrows_LogsErrorAndReturnsFalse()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetSceneItemEnabled(It.IsAny<String>(), It.IsAny<String>())).Throws(new Exception("OBS error"));

        var result = this._executor.GetSceneItemEnabled("Scene1", "Source1");

        Assert.False(result);
        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("Source1") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void ToggleSourceVisibility_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetSceneItemEnabled(It.IsAny<String>(), It.IsAny<String>())).Throws(new Exception("OBS error"));

        this._executor.ToggleSourceVisibility("Scene1", "Source1");
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("Source1") && s.Contains("OBS error"))), Times.Once);
    }

    // --- Audio Mixer tests ---

    [Fact]
    public void GetInputList_WhenConnected_ReturnsInputs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetInputList()).Returns(new[] { "Microphone", "Desktop Audio" });

        var result = this._executor.GetInputList();

        Assert.Equal(2, result.Length);
        Assert.Contains("Microphone", result);
        Assert.Contains("Desktop Audio", result);
    }

    [Fact]
    public void GetInputList_WhenNotConnected_ReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        var result = this._executor.GetInputList();

        Assert.Empty(result);
    }

    [Fact]
    public void GetInputList_WhenOBSThrows_LogsErrorAndReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetInputList()).Throws(new Exception("OBS error"));

        var result = this._executor.GetInputList();

        Assert.Empty(result);
        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("input list") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void GetInputMute_WhenConnected_ReturnsMuteState()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetInputMute("Microphone")).Returns(true);

        var result = this._executor.GetInputMute("Microphone");

        Assert.True(result);
    }

    [Fact]
    public void GetInputMute_WhenNotConnected_ReturnsFalse()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        var result = this._executor.GetInputMute("Microphone");

        Assert.False(result);
    }

    [Fact]
    public void GetInputMute_WhenOBSThrows_LogsErrorAndReturnsFalse()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetInputMute(It.IsAny<String>())).Throws(new Exception("OBS error"));

        var result = this._executor.GetInputMute("Microphone");

        Assert.False(result);
        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("Microphone") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void ToggleInputMute_WhenConnected_CallsObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        this._executor.ToggleInputMute("Microphone");

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.ToggleInputMute("Microphone"), Times.Once);
    }

    [Fact]
    public void ToggleInputMute_WhenNotConnected_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        this._executor.ToggleInputMute("Microphone");

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.ToggleInputMute(It.IsAny<String>()), Times.Never);
    }

    [Fact]
    public void ToggleInputMute_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.ToggleInputMute(It.IsAny<String>())).Throws(new Exception("OBS error"));

        this._executor.ToggleInputMute("Microphone");
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("Microphone") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public void GetAudioSourcesInScene_WhenConnected_ReturnsAudioSources()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetAudioSourcesInScene("Scene1")).Returns(new[] { "Microphone", "Desktop Audio" });

        var result = this._executor.GetAudioSourcesInScene("Scene1");

        Assert.Equal(2, result.Length);
        Assert.Contains("Microphone", result);
        Assert.Contains("Desktop Audio", result);
    }

    [Fact]
    public void GetAudioSourcesInScene_WhenNotConnected_ReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        var result = this._executor.GetAudioSourcesInScene("Scene1");

        Assert.Empty(result);
    }

    [Fact]
    public void GetAudioSourcesInScene_WhenSceneNameEmpty_ReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        var result = this._executor.GetAudioSourcesInScene("");

        Assert.Empty(result);
        this._mockLog.Verify(x => x.Warning(It.Is<String>(s => s.Contains("scene name is empty"))), Times.Once);
    }

    [Fact]
    public void GetAudioSourcesInScene_WhenOBSThrows_LogsErrorAndReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetAudioSourcesInScene(It.IsAny<String>())).Throws(new Exception("OBS error"));

        var result = this._executor.GetAudioSourcesInScene("Scene1");

        Assert.Empty(result);
        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("Scene1") && s.Contains("OBS error"))), Times.Once);
    }
}
