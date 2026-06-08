namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Loupedeck.OBSStudioForLogiPlugin.Helpers;
using Moq;
using OBSWebsocketDotNet.Types;

public class OBSActionExecutorReplayBufferTests
{
    private readonly Mock<IOBSWebsocket> _mockObs;
    private readonly Mock<IPluginLog> _mockLog;
    private readonly OBSActionExecutor _executor;

    public OBSActionExecutorReplayBufferTests()
    {
        this._mockObs = new Mock<IOBSWebsocket>();
        this._mockLog = new Mock<IPluginLog>();
        this._executor = new OBSActionExecutor(this._mockObs.Object, this._mockLog.Object);
    }

    // --- IsReplayBufferActive ---

    [Fact]
    public void IsReplayBufferActive_WhenStarted_ReturnsTrue()
    {
        this._executor.SetReplayBufferState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        Assert.True(this._executor.IsReplayBufferActive);
    }

    [Fact]
    public void IsReplayBufferActive_WhenStopped_ReturnsFalse()
    {
        this._executor.SetReplayBufferState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        Assert.False(this._executor.IsReplayBufferActive);
    }

    // --- ToggleReplayBuffer ---

    [Fact]
    public void ToggleReplayBuffer_WhenConnected_CallsObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        this._executor.ToggleReplayBuffer();

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.ToggleReplayBuffer(), Times.Once);
    }

    [Fact]
    public void ToggleReplayBuffer_WhenNotConnected_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        this._executor.ToggleReplayBuffer();

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.ToggleReplayBuffer(), Times.Never);
    }

    [Fact]
    public void ToggleReplayBuffer_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.ToggleReplayBuffer()).Throws(new Exception("OBS error"));

        this._executor.ToggleReplayBuffer();
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("toggle replay buffer") && s.Contains("OBS error"))), Times.Once);
    }

    // --- StartReplayBuffer ---

    [Fact]
    public void StartReplayBuffer_WhenConnected_CallsObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._executor.SetReplayBufferState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        this._executor.StartReplayBuffer();

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.StartReplayBuffer(), Times.Once);
    }

    [Fact]
    public void StartReplayBuffer_WhenNotConnected_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        this._executor.StartReplayBuffer();

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.StartReplayBuffer(), Times.Never);
    }

    [Fact]
    public void StartReplayBuffer_WhenAlreadyActive_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._executor.SetReplayBufferState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        this._executor.StartReplayBuffer();

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.StartReplayBuffer(), Times.Never);
    }

    [Fact]
    public void StartReplayBuffer_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.StartReplayBuffer()).Throws(new Exception("OBS error"));
        this._executor.SetReplayBufferState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        this._executor.StartReplayBuffer();
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("start replay buffer") && s.Contains("OBS error"))), Times.Once);
    }

    // --- StopReplayBuffer ---

    [Fact]
    public void StopReplayBuffer_WhenActive_CallsObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._executor.SetReplayBufferState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        this._executor.StopReplayBuffer();

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.StopReplayBuffer(), Times.Once);
    }

    [Fact]
    public void StopReplayBuffer_WhenNotConnected_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        this._executor.StopReplayBuffer();

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.StopReplayBuffer(), Times.Never);
    }

    [Fact]
    public void StopReplayBuffer_WhenNotActive_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._executor.SetReplayBufferState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        this._executor.StopReplayBuffer();

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.StopReplayBuffer(), Times.Never);
    }

    [Fact]
    public void StopReplayBuffer_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.StopReplayBuffer()).Throws(new Exception("OBS error"));
        this._executor.SetReplayBufferState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        this._executor.StopReplayBuffer();
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("stop replay buffer") && s.Contains("OBS error"))), Times.Once);
    }

    // --- SaveReplayBuffer ---

    [Fact]
    public void SaveReplayBuffer_WhenActive_CallsObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._executor.SetReplayBufferState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        this._executor.SaveReplayBuffer();

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.SaveReplayBuffer(), Times.Once);
    }

    [Fact]
    public void SaveReplayBuffer_WhenNotConnected_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        this._executor.SaveReplayBuffer();

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.SaveReplayBuffer(), Times.Never);
    }

    [Fact]
    public void SaveReplayBuffer_WhenNotActive_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._executor.SetReplayBufferState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        this._executor.SaveReplayBuffer();

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.SaveReplayBuffer(), Times.Never);
    }

    [Fact]
    public void SaveReplayBuffer_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.SaveReplayBuffer()).Throws(new Exception("OBS error"));
        this._executor.SetReplayBufferState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        this._executor.SaveReplayBuffer();
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("save replay buffer") && s.Contains("OBS error"))), Times.Once);
    }
}
