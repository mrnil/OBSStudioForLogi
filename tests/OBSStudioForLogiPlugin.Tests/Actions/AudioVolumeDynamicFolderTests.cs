namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions
{
    using System;
    using Xunit;

    [Collection(AudioSelectionStateCollection.Name)]
    public class AudioVolumeDynamicFolderTests
    {
        [Fact]
        public void Constructor_SetsInstanceProperty()
        {
            var folder = new AudioVolumeDynamicFolder();

            Assert.NotNull(AudioVolumeDynamicFolder.Instance);
            Assert.Same(folder, AudioVolumeDynamicFolder.Instance);
        }

        [Fact]
        public void OnConnected_DoesNotThrow()
        {
            var folder = new AudioVolumeDynamicFolder();

            var exception = Record.Exception(() => folder.OnConnected());

            Assert.Null(exception);
        }

        [Fact]
        public void OnDisconnected_DoesNotThrow()
        {
            var folder = new AudioVolumeDynamicFolder();

            var exception = Record.Exception(() => folder.OnDisconnected());

            Assert.Null(exception);
        }

        [Fact]
        public void OnInputVolumeChanged_DoesNotThrow()
        {
            var folder = new AudioVolumeDynamicFolder();

            var exception = Record.Exception(() => folder.OnInputVolumeChanged("TestInput"));

            Assert.Null(exception);
        }

        [Fact]
        public void OnInputMuteChanged_DoesNotThrow()
        {
            var folder = new AudioVolumeDynamicFolder();

            var exception = Record.Exception(() => folder.OnInputMuteChanged("TestInput"));

            Assert.Null(exception);
        }

        // The SDK draws a display name over the image, which already shows the name and volume.
        [Fact]
        public void GetAdjustmentDisplayName_ReturnsEmptySoTheImageIsNotOverlaid()
        {
            var folder = new AudioVolumeDynamicFolder();

            String displayName = folder.GetAdjustmentDisplayName("Mic/Aux", PluginImageSize.Width90);

            Assert.Equal(String.Empty, displayName);
        }
    }
}
