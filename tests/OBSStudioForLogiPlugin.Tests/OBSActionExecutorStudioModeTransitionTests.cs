namespace Loupedeck.OBSStudioForLogiPlugin.Tests
{
    using Moq;
    using Xunit;

    public class OBSActionExecutorStudioModeTransitionTests
    {
        private readonly Mock<IOBSWebsocket> _mockObs;
        private readonly Mock<IPluginLog> _mockLog;
        private readonly OBSActionExecutor _executor;

        public OBSActionExecutorStudioModeTransitionTests()
        {
            this._mockObs = new Mock<IOBSWebsocket>();
            this._mockLog = new Mock<IPluginLog>();
            this._executor = new OBSActionExecutor(this._mockObs.Object, this._mockLog.Object);
        }

        [Fact]
        public void TriggerStudioModeTransition_WhenConnectedAndEnabled_CallsObs()
        {
            this._mockObs.Setup(x => x.IsConnected).Returns(true);
            this._executor.SetStudioModeState(true);

            this._executor.TriggerStudioModeTransition();

            System.Threading.Thread.Sleep(100);
            this._mockObs.Verify(x => x.TriggerStudioModeTransition(), Times.Once);
        }

        [Fact]
        public void TriggerStudioModeTransition_WhenNotConnected_DoesNotCallObs()
        {
            this._mockObs.Setup(x => x.IsConnected).Returns(false);
            this._executor.SetStudioModeState(true);

            this._executor.TriggerStudioModeTransition();

            System.Threading.Thread.Sleep(100);
            this._mockObs.Verify(x => x.TriggerStudioModeTransition(), Times.Never);
        }

        [Fact]
        public void TriggerStudioModeTransition_WhenStudioModeDisabled_DoesNotCallObs()
        {
            this._mockObs.Setup(x => x.IsConnected).Returns(true);
            this._executor.SetStudioModeState(false);

            this._executor.TriggerStudioModeTransition();

            System.Threading.Thread.Sleep(100);
            this._mockObs.Verify(x => x.TriggerStudioModeTransition(), Times.Never);
        }

        [Fact]
        public void TriggerStudioModeTransition_WhenNotConnected_LogsWarning()
        {
            this._mockObs.Setup(x => x.IsConnected).Returns(false);
            this._executor.SetStudioModeState(true);

            this._executor.TriggerStudioModeTransition();

            System.Threading.Thread.Sleep(100);
            this._mockLog.Verify(x => x.Warning("Cannot trigger studio mode transition - not connected"), Times.Once);
        }

        [Fact]
        public void TriggerStudioModeTransition_WhenStudioModeDisabled_LogsWarning()
        {
            this._mockObs.Setup(x => x.IsConnected).Returns(true);
            this._executor.SetStudioModeState(false);

            this._executor.TriggerStudioModeTransition();

            System.Threading.Thread.Sleep(100);
            this._mockLog.Verify(x => x.Warning("Cannot trigger studio mode transition - studio mode not enabled"), Times.Once);
        }

        [Fact]
        public void TriggerStudioModeTransition_WhenConnectedAndEnabled_LogsInfo()
        {
            this._mockObs.Setup(x => x.IsConnected).Returns(true);
            this._executor.SetStudioModeState(true);

            this._executor.TriggerStudioModeTransition();

            System.Threading.Thread.Sleep(100);
            this._mockLog.Verify(x => x.Info("Triggering studio mode transition"), Times.Once);
        }
    }
}
