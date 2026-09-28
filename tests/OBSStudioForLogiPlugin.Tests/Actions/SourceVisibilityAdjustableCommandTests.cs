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
    }
}
