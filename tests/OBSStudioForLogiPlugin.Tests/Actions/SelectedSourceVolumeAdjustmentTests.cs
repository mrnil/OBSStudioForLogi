namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions
{
    using System;
    using Xunit;

    public class SelectedSourceVolumeAdjustmentTests
    {
        [Fact]
        public void Constructor_SetsInstanceProperty()
        {
            var adjustment = new SelectedSourceVolumeAdjustment();

            Assert.NotNull(SelectedSourceVolumeAdjustment.Instance);
            Assert.Same(adjustment, SelectedSourceVolumeAdjustment.Instance);
        }

        [Fact]
        public void OnConnected_DoesNotThrow()
        {
            var adjustment = new SelectedSourceVolumeAdjustment();

            var exception = Record.Exception(() => adjustment.OnConnected());

            Assert.Null(exception);
        }

        [Fact]
        public void OnDisconnected_DoesNotThrow()
        {
            var adjustment = new SelectedSourceVolumeAdjustment();

            var exception = Record.Exception(() => adjustment.OnDisconnected());

            Assert.Null(exception);
        }
    }
}
