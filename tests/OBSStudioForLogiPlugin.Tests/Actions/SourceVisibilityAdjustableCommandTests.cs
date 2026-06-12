namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions
{
    using System;
    using Xunit;

    public class SourceVisibilityAdjustableCommandTests
    {
        [Fact]
        public void Constructor_SetsInstanceProperty()
        {
            var command = new SourceVisibilityAdjustableCommand();

            Assert.NotNull(SourceVisibilityAdjustableCommand.Instance);
            Assert.Same(command, SourceVisibilityAdjustableCommand.Instance);
        }

        [Fact]
        public void OnConnected_DoesNotThrow()
        {
            var command = new SourceVisibilityAdjustableCommand();

            var exception = Record.Exception(() => command.OnConnected());

            Assert.Null(exception);
        }

        [Fact]
        public void OnDisconnected_DoesNotThrow()
        {
            var command = new SourceVisibilityAdjustableCommand();

            var exception = Record.Exception(() => command.OnDisconnected());

            Assert.Null(exception);
        }
    }
}
