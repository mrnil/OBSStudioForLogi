namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Loupedeck.OBSStudioForLogiPlugin.Helpers;
using Moq;
using OBSWebsocketDotNet.Types;

public class OBSActionExecutorAudioTests
{
    private readonly Mock<IOBSWebsocket> _mockObs;
    private readonly Mock<IPluginLog> _mockLog;
    private readonly OBSActionExecutor _executor;

    public OBSActionExecutorAudioTests()
    {
        this._mockObs = new Mock<IOBSWebsocket>();
        this._mockLog = new Mock<IPluginLog>();
        this._executor = new OBSActionExecutor(this._mockObs.Object, this._mockLog.Object);
    }

    // --- GetInputVolume ---

    [Fact]
    public void GetInputVolume_WhenConnected_ReturnsVolume()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetInputVolume("Microphone")).Returns(0.75f);

        var result = this._executor.GetInputVolume("Microphone");

        Assert.Equal(0.75f, result);
    }

    [Fact]
    public void GetInputVolume_WhenNotConnected_ReturnsDefault()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        var result = this._executor.GetInputVolume("Microphone");

        Assert.Equal(1.0f, result);
    }

    [Fact]
    public void GetInputVolume_WhenOBSThrows_LogsErrorAndReturnsDefault()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetInputVolume(It.IsAny<String>())).Throws(new Exception("OBS error"));

        var result = this._executor.GetInputVolume("Microphone");

        Assert.Equal(1.0f, result);
        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("Microphone") && s.Contains("OBS error"))), Times.Once);
    }

    // --- SetInputVolume ---

    [Fact]
    public void SetInputVolume_WhenConnected_CallsObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        this._executor.SetInputVolume("Microphone", 0.5f);

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.SetInputVolume("Microphone", 0.5f), Times.Once);
    }

    [Fact]
    public void SetInputVolume_WhenNotConnected_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        this._executor.SetInputVolume("Microphone", 0.5f);

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.SetInputVolume(It.IsAny<String>(), It.IsAny<Single>()), Times.Never);
    }

    [Fact]
    public void SetInputVolume_WhenInputNameEmpty_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        this._executor.SetInputVolume("", 0.5f);

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.SetInputVolume(It.IsAny<String>(), It.IsAny<Single>()), Times.Never);
    }

    [Fact]
    public void SetInputVolume_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.SetInputVolume(It.IsAny<String>(), It.IsAny<Single>())).Throws(new Exception("OBS error"));

        this._executor.SetInputVolume("Microphone", 0.5f);
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("Microphone") && s.Contains("OBS error"))), Times.Once);
    }

    // --- GetInputAudioMonitorType ---

    [Fact]
    public void GetInputAudioMonitorType_WhenConnected_ReturnsType()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetInputAudioMonitorType("Microphone")).Returns("OBS_MONITORING_TYPE_MONITOR_ONLY");

        var result = this._executor.GetInputAudioMonitorType("Microphone");

        Assert.Equal("OBS_MONITORING_TYPE_MONITOR_ONLY", result);
    }

    [Fact]
    public void GetInputAudioMonitorType_WhenNotConnected_ReturnsNone()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        var result = this._executor.GetInputAudioMonitorType("Microphone");

        Assert.Equal("OBS_MONITORING_TYPE_NONE", result);
    }

    [Fact]
    public void GetInputAudioMonitorType_WhenOBSThrows_LogsErrorAndReturnsNone()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetInputAudioMonitorType(It.IsAny<String>())).Throws(new Exception("OBS error"));

        var result = this._executor.GetInputAudioMonitorType("Microphone");

        Assert.Equal("OBS_MONITORING_TYPE_NONE", result);
        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("Microphone") && s.Contains("OBS error"))), Times.Once);
    }

    // --- CycleInputAudioMonitorType ---

    [Fact]
    public void CycleInputAudioMonitorType_FromNone_SetsMonitorOnly()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetInputAudioMonitorType("Mic")).Returns("OBS_MONITORING_TYPE_NONE");

        this._executor.CycleInputAudioMonitorType("Mic");
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockObs.Verify(x => x.SetInputAudioMonitorType("Mic", "OBS_MONITORING_TYPE_MONITOR_ONLY"), Times.Once);
    }

    [Fact]
    public void CycleInputAudioMonitorType_FromMonitorOnly_SetsMonitorAndOutput()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetInputAudioMonitorType("Mic")).Returns("OBS_MONITORING_TYPE_MONITOR_ONLY");

        this._executor.CycleInputAudioMonitorType("Mic");
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockObs.Verify(x => x.SetInputAudioMonitorType("Mic", "OBS_MONITORING_TYPE_MONITOR_AND_OUTPUT"), Times.Once);
    }

    [Fact]
    public void CycleInputAudioMonitorType_FromMonitorAndOutput_SetsNone()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetInputAudioMonitorType("Mic")).Returns("OBS_MONITORING_TYPE_MONITOR_AND_OUTPUT");

        this._executor.CycleInputAudioMonitorType("Mic");
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockObs.Verify(x => x.SetInputAudioMonitorType("Mic", "OBS_MONITORING_TYPE_NONE"), Times.Once);
    }

    [Fact]
    public void CycleInputAudioMonitorType_WhenNotConnected_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        this._executor.CycleInputAudioMonitorType("Mic");
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockObs.Verify(x => x.SetInputAudioMonitorType(It.IsAny<String>(), It.IsAny<String>()), Times.Never);
    }

    [Fact]
    public void CycleInputAudioMonitorType_WhenInputNameEmpty_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        this._executor.CycleInputAudioMonitorType("");
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockObs.Verify(x => x.SetInputAudioMonitorType(It.IsAny<String>(), It.IsAny<String>()), Times.Never);
    }

    [Fact]
    public void CycleInputAudioMonitorType_WhenOBSThrows_LogsError()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetInputAudioMonitorType(It.IsAny<String>())).Throws(new Exception("OBS error"));

        this._executor.CycleInputAudioMonitorType("Mic");
        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);

        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("Mic") && s.Contains("OBS error"))), Times.Once);
    }

    // --- GetInputKind ---

    [Fact]
    public void GetInputKind_WhenConnected_ReturnsKind()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetInputKind("Microphone")).Returns("wasapi_input_capture");

        var result = this._executor.GetInputKind("Microphone");

        Assert.Equal("wasapi_input_capture", result);
    }

    [Fact]
    public void GetInputKind_WhenNotConnected_ReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        var result = this._executor.GetInputKind("Microphone");

        Assert.Equal(String.Empty, result);
    }

    [Fact]
    public void GetInputKind_WhenOBSThrows_LogsErrorAndReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetInputKind(It.IsAny<String>())).Throws(new Exception("OBS error"));

        var result = this._executor.GetInputKind("Microphone");

        Assert.Equal(String.Empty, result);
        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("Microphone") && s.Contains("OBS error"))), Times.Once);
    }

    // --- GetScenesForInput ---

    [Fact]
    public void GetScenesForInput_WhenConnected_ReturnsScenes()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetScenesForInput("Microphone")).Returns(new[] { "Scene 1", "Scene 2" });

        var result = this._executor.GetScenesForInput("Microphone");

        Assert.Equal(2, result.Length);
        Assert.Contains("Scene 1", result);
    }

    [Fact]
    public void GetScenesForInput_WhenNotConnected_ReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        var result = this._executor.GetScenesForInput("Microphone");

        Assert.Empty(result);
    }

    [Fact]
    public void GetScenesForInput_WhenInputNameEmpty_ReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        var result = this._executor.GetScenesForInput("");

        Assert.Empty(result);
    }

    [Fact]
    public void GetScenesForInput_WhenOBSThrows_LogsErrorAndReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetScenesForInput(It.IsAny<String>())).Throws(new Exception("OBS error"));

        var result = this._executor.GetScenesForInput("Microphone");

        Assert.Empty(result);
        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("Microphone") && s.Contains("OBS error"))), Times.Once);
    }

    // --- GetAudioInputsNotInAnyScene ---

    [Fact]
    public void GetAudioInputsNotInAnyScene_WhenConnected_ReturnsInputs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetAudioInputsNotInAnyScene()).Returns(new[] { "Global Mic" });

        var result = this._executor.GetAudioInputsNotInAnyScene();

        Assert.Single(result);
        Assert.Contains("Global Mic", result);
    }

    [Fact]
    public void GetAudioInputsNotInAnyScene_WhenNotConnected_ReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        var result = this._executor.GetAudioInputsNotInAnyScene();

        Assert.Empty(result);
    }

    [Fact]
    public void GetAudioInputsNotInAnyScene_WhenOBSThrows_LogsErrorAndReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetAudioInputsNotInAnyScene()).Throws(new Exception("OBS error"));

        var result = this._executor.GetAudioInputsNotInAnyScene();

        Assert.Empty(result);
        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("audio inputs not in any scene") && s.Contains("OBS error"))), Times.Once);
    }

    // --- ToggleInputMute edge case ---

    [Fact]
    public void ToggleInputMute_WhenInputNameEmpty_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        this._executor.ToggleInputMute("");

        System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
        this._mockObs.Verify(x => x.ToggleInputMute(It.IsAny<String>()), Times.Never);
    }
}
