namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Moq;
using OBSWebsocketDotNet.Types;

public class VirtualCameraCommandTests
{
    [Fact]
    public void ToggleVirtualCamera_WhenConnectedAndOff_StartsVirtualCamera()
    {
        var mockObs = new Mock<IOBSWebsocket>();
        var mockLog = new Mock<IPluginLog>();
        mockObs.Setup(x => x.IsConnected).Returns(true);
        var executor = new OBSActionExecutor(mockObs.Object, mockLog.Object);
        executor.SetVirtualCameraState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        executor.ToggleVirtualCamera();

        System.Threading.Thread.Sleep(100);
        mockObs.Verify(x => x.StartVirtualCam(), Times.Once);
    }

    [Fact]
    public void ToggleVirtualCamera_WhenConnectedAndOn_StopsVirtualCamera()
    {
        var mockObs = new Mock<IOBSWebsocket>();
        var mockLog = new Mock<IPluginLog>();
        mockObs.Setup(x => x.IsConnected).Returns(true);
        var executor = new OBSActionExecutor(mockObs.Object, mockLog.Object);
        executor.SetVirtualCameraState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        executor.ToggleVirtualCamera();

        System.Threading.Thread.Sleep(100);
        mockObs.Verify(x => x.StopVirtualCam(), Times.Once);
    }

    [Fact]
    public void StartVirtualCamera_WhenConnected_StartsVirtualCamera()
    {
        var mockObs = new Mock<IOBSWebsocket>();
        var mockLog = new Mock<IPluginLog>();
        mockObs.Setup(x => x.IsConnected).Returns(true);
        var executor = new OBSActionExecutor(mockObs.Object, mockLog.Object);

        executor.StartVirtualCamera();

        System.Threading.Thread.Sleep(100);
        mockObs.Verify(x => x.StartVirtualCam(), Times.Once);
    }

    [Fact]
    public void StopVirtualCamera_WhenConnected_StopsVirtualCamera()
    {
        var mockObs = new Mock<IOBSWebsocket>();
        var mockLog = new Mock<IPluginLog>();
        mockObs.Setup(x => x.IsConnected).Returns(true);
        var executor = new OBSActionExecutor(mockObs.Object, mockLog.Object);
        executor.SetVirtualCameraState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        executor.StopVirtualCamera();

        System.Threading.Thread.Sleep(100);
        mockObs.Verify(x => x.StopVirtualCam(), Times.Once);
    }

    [Fact]
    public void VirtualCameraState_WhenStarted_ReturnsTrue()
    {
        var mockObs = new Mock<IOBSWebsocket>();
        var mockLog = new Mock<IPluginLog>();
        var executor = new OBSActionExecutor(mockObs.Object, mockLog.Object);

        executor.SetVirtualCameraState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

        Assert.True(executor.IsVirtualCameraActive);
    }

    [Fact]
    public void VirtualCameraState_WhenStopped_ReturnsFalse()
    {
        var mockObs = new Mock<IOBSWebsocket>();
        var mockLog = new Mock<IPluginLog>();
        var executor = new OBSActionExecutor(mockObs.Object, mockLog.Object);

        executor.SetVirtualCameraState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

        Assert.False(executor.IsVirtualCameraActive);
    }
}
