namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions
{
    using System;
    using Xunit;

    public class AudioMonitoringCycleAdjustableCommandTests
    {
        [Fact]
        public void Constructor_SetsInstanceProperty()
        {
            var command = new AudioMonitoringCycleAdjustableCommand();

            Assert.NotNull(AudioMonitoringCycleAdjustableCommand.Instance);
            Assert.Same(command, AudioMonitoringCycleAdjustableCommand.Instance);
        }

        [Fact]
        public void OnConnected_DoesNotThrow()
        {
            var command = new AudioMonitoringCycleAdjustableCommand();

            var exception = Record.Exception(() => command.OnConnected());

            Assert.Null(exception);
        }

        [Fact]
        public void OnDisconnected_DoesNotThrow()
        {
            var command = new AudioMonitoringCycleAdjustableCommand();

            var exception = Record.Exception(() => command.OnDisconnected());

            Assert.Null(exception);
        }
    }
}
