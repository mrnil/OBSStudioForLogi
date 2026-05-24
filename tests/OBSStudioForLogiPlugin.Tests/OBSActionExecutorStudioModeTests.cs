namespace Loupedeck.OBSStudioForLogiPlugin.Tests
{
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;
    using Moq;
    using Xunit;

    public class OBSActionExecutorStudioModeTests
    {
        private readonly Mock<IOBSWebsocket> _mockObs;
        private readonly Mock<IPluginLog> _mockLog;
        private readonly OBSActionExecutor _executor;

        public OBSActionExecutorStudioModeTests()
        {
            this._mockObs = new Mock<IOBSWebsocket>();
            this._mockLog = new Mock<IPluginLog>();
            this._executor = new OBSActionExecutor(this._mockObs.Object, this._mockLog.Object);
        }

        [Fact]
        public void GetStudioModeEnabled_WhenConnected_ReturnsState()
        {
            this._mockObs.Setup(x => x.IsConnected).Returns(true);
            this._mockObs.Setup(x => x.GetStudioModeEnabled()).Returns(true);

            var result = this._executor.GetStudioModeEnabled();

            Assert.True(result);
            this._mockObs.Verify(x => x.GetStudioModeEnabled(), Times.Once);
        }

        [Fact]
        public void GetStudioModeEnabled_WhenNotConnected_ReturnsFalse()
        {
            this._mockObs.Setup(x => x.IsConnected).Returns(false);

            var result = this._executor.GetStudioModeEnabled();

            Assert.False(result);
            this._mockObs.Verify(x => x.GetStudioModeEnabled(), Times.Never);
        }

        [Fact]
        public void ToggleStudioMode_WhenConnected_CallsObs()
        {
            this._mockObs.Setup(x => x.IsConnected).Returns(true);
            this._executor.SetStudioModeState(false);

            this._executor.ToggleStudioMode();

            System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
            this._mockObs.Verify(x => x.SetStudioModeEnabled(true), Times.Once);
        }

        [Fact]
        public void ToggleStudioMode_WhenNotConnected_DoesNotCallObs()
        {
            this._mockObs.Setup(x => x.IsConnected).Returns(false);

            this._executor.ToggleStudioMode();

            System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
            this._mockObs.Verify(x => x.SetStudioModeEnabled(It.IsAny<Boolean>()), Times.Never);
        }

        [Fact]
        public void SetStudioModeState_UpdatesProperty()
        {
            this._executor.SetStudioModeState(true);

            Assert.True(this._executor.IsStudioModeEnabled);
        }

        [Fact]
        public void IsStudioModeEnabled_DefaultsToFalse()
        {
            Assert.False(this._executor.IsStudioModeEnabled);
        }

        [Fact]
        public void ToggleStudioMode_TogglesFromFalseToTrue()
        {
            this._mockObs.Setup(x => x.IsConnected).Returns(true);
            this._executor.SetStudioModeState(false);

            this._executor.ToggleStudioMode();

            System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
            this._mockObs.Verify(x => x.SetStudioModeEnabled(true), Times.Once);
        }

        [Fact]
        public void ToggleStudioMode_TogglesFromTrueToFalse()
        {
            this._mockObs.Setup(x => x.IsConnected).Returns(true);
            this._executor.SetStudioModeState(true);

            this._executor.ToggleStudioMode();

            System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
            this._mockObs.Verify(x => x.SetStudioModeEnabled(false), Times.Once);
        }
    }
}

