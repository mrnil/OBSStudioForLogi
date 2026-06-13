namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions
{
    using System;
    using Xunit;

    public class AudioStatusDisplayCommandTests
    {
        [Fact]
        public void Constructor_SetsInstanceProperty()
        {
            var command = new AudioStatusDisplayCommand();

            Assert.NotNull(AudioStatusDisplayCommand.Instance);
            Assert.Same(command, AudioStatusDisplayCommand.Instance);
        }

        [Fact]
        public void OnConnected_DoesNotThrow()
        {
            var command = new AudioStatusDisplayCommand();

            var exception = Record.Exception(() => command.OnConnected());

            Assert.Null(exception);
        }

        [Fact]
        public void OnDisconnected_DoesNotThrow()
        {
            var command = new AudioStatusDisplayCommand();

            var exception = Record.Exception(() => command.OnDisconnected());

            Assert.Null(exception);
        }

        [Fact]
        public void OnInputMuteChanged_DoesNotThrow()
        {
            var command = new AudioStatusDisplayCommand();

            var exception = Record.Exception(() => command.OnInputMuteChanged("TestInput"));

            Assert.Null(exception);
        }

        [Fact]
        public void OnInputVolumeChanged_DoesNotThrow()
        {
            var command = new AudioStatusDisplayCommand();

            var exception = Record.Exception(() => command.OnInputVolumeChanged("TestInput"));

            Assert.Null(exception);
        }
    }
}
