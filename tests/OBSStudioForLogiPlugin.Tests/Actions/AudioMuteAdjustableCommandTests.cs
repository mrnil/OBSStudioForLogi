namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions
{
    using System;
    using Xunit;

    public class AudioMuteAdjustableCommandTests
    {
        [Fact]
        public void Constructor_SetsInstanceProperty()
        {
            var command = new AudioMuteAdjustableCommand();

            Assert.NotNull(AudioMuteAdjustableCommand.Instance);
            Assert.Same(command, AudioMuteAdjustableCommand.Instance);
        }

        [Fact]
        public void OnConnected_DoesNotThrow()
        {
            var command = new AudioMuteAdjustableCommand();

            var exception = Record.Exception(() => command.OnConnected());

            Assert.Null(exception);
        }

        [Fact]
        public void OnDisconnected_DoesNotThrow()
        {
            var command = new AudioMuteAdjustableCommand();

            var exception = Record.Exception(() => command.OnDisconnected());

            Assert.Null(exception);
        }
    }
}
