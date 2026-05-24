namespace Loupedeck.OBSStudioForLogiPlugin.Tests
{
    using Moq;
    using Xunit;

    public class OBSActionExecutorSceneSwitchingTests
    {
        private readonly Mock<IOBSWebsocket> _mockObs;
        private readonly Mock<IPluginLog> _mockLog;
        private readonly OBSActionExecutor _executor;

        public OBSActionExecutorSceneSwitchingTests()
        {
            this._mockObs = new Mock<IOBSWebsocket>();
            this._mockLog = new Mock<IPluginLog>();
            this._executor = new OBSActionExecutor(this._mockObs.Object, this._mockLog.Object);
        }

        [Fact]
        public void SetCurrentScene_WhenStudioModeDisabled_SetsProgramScene()
        {
            this._mockObs.Setup(x => x.IsConnected).Returns(true);
            this._executor.SetStudioModeState(false);

            this._executor.SetCurrentScene("TestScene");

            System.Threading.Thread.Sleep(100);
            this._mockObs.Verify(x => x.SetCurrentProgramScene("TestScene"), Times.Once);
            this._mockObs.Verify(x => x.SetCurrentPreviewScene(It.IsAny<String>()), Times.Never);
        }

        [Fact]
        public void SetCurrentScene_WhenStudioModeEnabled_SetsPreviewScene()
        {
            this._mockObs.Setup(x => x.IsConnected).Returns(true);
            this._executor.SetStudioModeState(true);

            this._executor.SetCurrentScene("TestScene");

            System.Threading.Thread.Sleep(200);
            this._mockObs.Verify(x => x.SetCurrentPreviewScene("TestScene"), Times.Once);
            this._mockObs.Verify(x => x.SetCurrentProgramScene(It.IsAny<String>()), Times.Never);
        }

        [Fact]
        public void SetCurrentScene_WhenStudioModeDisabled_LogsProgramScene()
        {
            this._mockObs.Setup(x => x.IsConnected).Returns(true);
            this._executor.SetStudioModeState(false);

            this._executor.SetCurrentScene("TestScene");

            System.Threading.Thread.Sleep(100);
            this._mockLog.Verify(x => x.Info("Setting current program scene to 'TestScene'"), Times.Once);
        }

        [Fact]
        public void SetCurrentScene_WhenStudioModeEnabled_LogsPreviewScene()
        {
            this._mockObs.Setup(x => x.IsConnected).Returns(true);
            this._executor.SetStudioModeState(true);

            this._executor.SetCurrentScene("TestScene");

            System.Threading.Thread.Sleep(100);
            this._mockLog.Verify(x => x.Info("Setting current preview scene to 'TestScene' (studio mode enabled)"), Times.Once);
        }

        [Fact]
        public void SetCurrentScene_WhenNotConnected_DoesNotSetScene()
        {
            this._mockObs.Setup(x => x.IsConnected).Returns(false);

            this._executor.SetCurrentScene("TestScene");

            System.Threading.Thread.Sleep(100);
            this._mockObs.Verify(x => x.SetCurrentProgramScene(It.IsAny<String>()), Times.Never);
            this._mockObs.Verify(x => x.SetCurrentPreviewScene(It.IsAny<String>()), Times.Never);
        }
    }
}
