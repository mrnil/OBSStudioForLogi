namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions
{
    using System;
    using Xunit;

    public class AudioSelectDynamicFolderTests
    {
        [Fact]
        public void Constructor_SetsInstanceProperty()
        {
            var folder = new AudioSelectDynamicFolder();

            Assert.NotNull(AudioSelectDynamicFolder.Instance);
            Assert.Same(folder, AudioSelectDynamicFolder.Instance);
        }

        [Fact]
        public void OnConnected_DoesNotThrow()
        {
            var folder = new AudioSelectDynamicFolder();

            var exception = Record.Exception(() => folder.OnConnected());

            Assert.Null(exception);
        }

        [Fact]
        public void OnDisconnected_DoesNotThrow()
        {
            var folder = new AudioSelectDynamicFolder();

            var exception = Record.Exception(() => folder.OnDisconnected());

            Assert.Null(exception);
        }

        [Fact]
        public void OnDisconnected_DeselectsAudioSelection()
        {
            var folder = new AudioSelectDynamicFolder();
            AudioSelectionState.Select("TestInput");

            folder.OnDisconnected();

            Assert.Null(AudioSelectionState.SelectedInput);
        }

        [Fact]
        public void OnInputMuteChanged_DoesNotThrow()
        {
            var folder = new AudioSelectDynamicFolder();

            var exception = Record.Exception(() => folder.OnInputMuteChanged("TestInput"));

            Assert.Null(exception);
        }

        [Fact]
        public void OnInputVolumeChanged_DoesNotThrow()
        {
            var folder = new AudioSelectDynamicFolder();

            var exception = Record.Exception(() => folder.OnInputVolumeChanged("TestInput"));

            Assert.Null(exception);
        }
    }
}
