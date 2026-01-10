namespace Loupedeck.OBSStudioForLogiPlugin.Tests
{
    using System;
    using Xunit;
    using Moq;
    using OBSWebsocketDotNet;
    using OBSWebsocketDotNet.Types;

    public class OBSActionExecutorTests
    {
        [Fact]
        public void SetCurrentScene_WhenConnected_CallsOBSWebsocket()
        {
            var mockObs = new Mock<IOBSWebsocket>();
            mockObs.Setup(o => o.IsConnected).Returns(true);
            var mockLog = new Mock<IPluginLog>();
            var executor = new OBSActionExecutor(mockObs.Object, mockLog.Object);

            executor.SetCurrentScene("Scene1");

            mockObs.Verify(o => o.SetCurrentProgramScene("Scene1"), Times.Once);
        }

        [Fact]
        public void SetCurrentScene_WhenNotConnected_DoesNotCallOBSWebsocket()
        {
            var mockObs = new Mock<IOBSWebsocket>();
            mockObs.Setup(o => o.IsConnected).Returns(false);
            var mockLog = new Mock<IPluginLog>();
            var executor = new OBSActionExecutor(mockObs.Object, mockLog.Object);

            executor.SetCurrentScene("Scene1");

            mockObs.Verify(o => o.SetCurrentProgramScene(It.IsAny<String>()), Times.Never);
        }

        [Fact]
        public void ToggleRecording_WhenConnectedAndNotChanging_CallsOBSWebsocket()
        {
            var mockObs = new Mock<IOBSWebsocket>();
            mockObs.Setup(o => o.IsConnected).Returns(true);
            var mockLog = new Mock<IPluginLog>();
            var executor = new OBSActionExecutor(mockObs.Object, mockLog.Object);
            executor.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

            executor.ToggleRecording();

            mockObs.Verify(o => o.ToggleRecord(), Times.Once);
        }

        [Fact]
        public void ToggleRecording_WhenNotConnected_DoesNotCallOBSWebsocket()
        {
            var mockObs = new Mock<IOBSWebsocket>();
            mockObs.Setup(o => o.IsConnected).Returns(false);
            var mockLog = new Mock<IPluginLog>();
            var executor = new OBSActionExecutor(mockObs.Object, mockLog.Object);

            executor.ToggleRecording();

            mockObs.Verify(o => o.ToggleRecord(), Times.Never);
        }

        [Fact]
        public void ToggleRecording_WhenStateChanging_DoesNotCallOBSWebsocket()
        {
            var mockObs = new Mock<IOBSWebsocket>();
            mockObs.Setup(o => o.IsConnected).Returns(true);
            var mockLog = new Mock<IPluginLog>();
            var executor = new OBSActionExecutor(mockObs.Object, mockLog.Object);
            executor.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTING);

            executor.ToggleRecording();

            mockObs.Verify(o => o.ToggleRecord(), Times.Never);
        }

        [Fact]
        public void IsRecording_WhenStarted_ReturnsTrue()
        {
            var mockObs = new Mock<IOBSWebsocket>();
            var mockLog = new Mock<IPluginLog>();
            var executor = new OBSActionExecutor(mockObs.Object, mockLog.Object);
            executor.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);

            Assert.True(executor.IsRecording);
        }

        [Fact]
        public void IsRecording_WhenStopped_ReturnsFalse()
        {
            var mockObs = new Mock<IOBSWebsocket>();
            var mockLog = new Mock<IPluginLog>();
            var executor = new OBSActionExecutor(mockObs.Object, mockLog.Object);
            executor.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);

            Assert.False(executor.IsRecording);
        }
    }
}
