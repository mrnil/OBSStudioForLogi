namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Moq;
using Loupedeck.OBSStudioForLogiPlugin.Models;
using Loupedeck.OBSStudioForLogiPlugin.Helpers;

public class OBSActionExecutorStatsAndMediaTests
{
    private readonly Mock<IOBSWebsocket> _mockObs;
    private readonly Mock<IPluginLog> _mockLog;
    private readonly OBSActionExecutor _executor;

    public OBSActionExecutorStatsAndMediaTests()
    {
        this._mockObs = new Mock<IOBSWebsocket>();
        this._mockLog = new Mock<IPluginLog>();
        this._executor = new OBSActionExecutor(this._mockObs.Object, this._mockLog.Object);
    }

    // GetStats tests

    [Fact]
    public void GetStats_WhenConnected_ReturnsStats()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetStats()).Returns(new OBSStats { Fps = 60.0, CpuUsage = 5.0, MemoryUsage = 512 });

        var result = this._executor.GetStats();

        Assert.NotNull(result);
        Assert.Equal(60.0, result.Fps);
        Assert.Equal(5.0, result.CpuUsage);
        Assert.Equal(512, result.MemoryUsage);
    }

    [Fact]
    public void GetStats_WhenNotConnected_ReturnsNull()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        var result = this._executor.GetStats();

        Assert.Null(result);
    }

    [Fact]
    public void GetStats_WhenOBSThrows_ReturnsNull()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetStats()).Throws(new Exception("OBS error"));

        var result = this._executor.GetStats();

        Assert.Null(result);
        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("Failed to get stats"))), Times.Once);
    }

    // GetStreamStatus tests

    [Fact]
    public void GetStreamStatus_WhenConnected_ReturnsStatus()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetStreamStatus()).Returns(new OBSStreamStats { IsActive = true, BytesSent = 1024 });

        var result = this._executor.GetStreamStatus();

        Assert.NotNull(result);
        Assert.True(result.IsActive);
        Assert.Equal(1024, result.BytesSent);
    }

    [Fact]
    public void GetStreamStatus_WhenNotConnected_ReturnsNull()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        var result = this._executor.GetStreamStatus();

        Assert.Null(result);
    }

    [Fact]
    public void GetStreamStatus_WhenOBSThrows_ReturnsNull()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetStreamStatus()).Throws(new Exception("OBS error"));

        var result = this._executor.GetStreamStatus();

        Assert.Null(result);
        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("Failed to get stream status"))), Times.Once);
    }

    // GetMediaInputStatus tests

    [Fact]
    public void GetMediaInputStatus_WhenConnected_ReturnsState()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetMediaInputStatus("Video")).Returns("OBS_MEDIA_STATE_PLAYING");

        var result = this._executor.GetMediaInputStatus("Video");

        Assert.Equal("OBS_MEDIA_STATE_PLAYING", result);
    }

    [Fact]
    public void GetMediaInputStatus_WhenNotConnected_ReturnsNone()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        var result = this._executor.GetMediaInputStatus("Video");

        Assert.Equal("OBS_MEDIA_STATE_NONE", result);
    }

    [Fact]
    public void GetMediaInputStatus_WhenInputNameEmpty_ReturnsNone()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        var result = this._executor.GetMediaInputStatus("");

        Assert.Equal("OBS_MEDIA_STATE_NONE", result);
    }

    [Fact]
    public void GetMediaInputStatus_WhenOBSThrows_ReturnsNone()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetMediaInputStatus(It.IsAny<String>())).Throws(new Exception("error"));

        var result = this._executor.GetMediaInputStatus("Video");

        Assert.Equal("OBS_MEDIA_STATE_NONE", result);
    }

    // TriggerMediaInputAction tests

    [Fact]
    public void TriggerMediaInputAction_WhenConnected_CallsObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        this._executor.TriggerMediaInputAction("Video", "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PLAY");

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.TriggerMediaInputAction("Video", "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PLAY"), Times.Once);
    }

    [Fact]
    public void TriggerMediaInputAction_WhenNotConnected_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        this._executor.TriggerMediaInputAction("Video", "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PLAY");

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.TriggerMediaInputAction(It.IsAny<String>(), It.IsAny<String>()), Times.Never);
    }

    [Fact]
    public void TriggerMediaInputAction_WhenInputNameEmpty_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        this._executor.TriggerMediaInputAction("", "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PLAY");

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.TriggerMediaInputAction(It.IsAny<String>(), It.IsAny<String>()), Times.Never);
    }

    [Fact]
    public void TriggerMediaInputAction_WhenActionEmpty_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        this._executor.TriggerMediaInputAction("Video", "");

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.TriggerMediaInputAction(It.IsAny<String>(), It.IsAny<String>()), Times.Never);
    }

    // GetMediaInputList tests

    [Fact]
    public void GetMediaInputList_WhenConnected_ReturnsList()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetMediaInputList()).Returns(new[] { "Video", "Music" });

        var result = this._executor.GetMediaInputList();

        Assert.Equal(2, result.Length);
        Assert.Contains("Video", result);
        Assert.Contains("Music", result);
    }

    [Fact]
    public void GetMediaInputList_WhenNotConnected_ReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        var result = this._executor.GetMediaInputList();

        Assert.Empty(result);
    }

    [Fact]
    public void GetMediaInputList_WhenOBSThrows_ReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetMediaInputList()).Throws(new Exception("error"));

        var result = this._executor.GetMediaInputList();

        Assert.Empty(result);
    }
}
