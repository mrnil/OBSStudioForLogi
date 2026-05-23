namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions
{
    using System;
    using Xunit;

    public class SceneSwitchAdjustableCommandTests
    {
        [Fact]
        public void Constructor_SetsInstanceProperty()
        {
            var command = new SceneSwitchAdjustableCommand();

            Assert.NotNull(SceneSwitchAdjustableCommand.Instance);
            Assert.Same(command, SceneSwitchAdjustableCommand.Instance);
        }

        [Fact]
        public void OnConnected_DoesNotThrow()
        {
            var command = new SceneSwitchAdjustableCommand();

            var exception = Record.Exception(() => command.OnConnected());

            Assert.Null(exception);
        }

        [Fact]
        public void OnDisconnected_DoesNotThrow()
        {
            var command = new SceneSwitchAdjustableCommand();

            var exception = Record.Exception(() => command.OnDisconnected());

            Assert.Null(exception);
        }

        [Fact]
        public void OnProfileChanged_DoesNotThrow()
        {
            var command = new SceneSwitchAdjustableCommand();

            var exception = Record.Exception(() => command.OnProfileChanged());

            Assert.Null(exception);
        }

        [Fact]
        public void OnSceneCollectionChanged_DoesNotThrow()
        {
            var command = new SceneSwitchAdjustableCommand();

            var exception = Record.Exception(() => command.OnSceneCollectionChanged());

            Assert.Null(exception);
        }

        [Fact]
        public void OnScenesChanged_DoesNotThrow()
        {
            var command = new SceneSwitchAdjustableCommand();

            var exception = Record.Exception(() => command.OnScenesChanged());

            Assert.Null(exception);
        }
    }
}
