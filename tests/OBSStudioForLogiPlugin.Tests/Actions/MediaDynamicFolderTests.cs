namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions;

using Xunit;

public class MediaDynamicFolderTests
{
    [Fact]
    public void Constructor_SetsInstance()
    {
        var folder = new MediaDynamicFolder();

        Assert.NotNull(MediaDynamicFolder.Instance);
        Assert.Same(folder, MediaDynamicFolder.Instance);
    }

    [Fact]
    public void OnConnected_DoesNotThrow()
    {
        var folder = new MediaDynamicFolder();

        var exception = Record.Exception(() => folder.OnConnected());

        Assert.Null(exception);
    }

    [Fact]
    public void OnDisconnected_DoesNotThrow()
    {
        var folder = new MediaDynamicFolder();

        var exception = Record.Exception(() => folder.OnDisconnected());

        Assert.Null(exception);
    }

    [Fact]
    public void OnInputsChanged_DoesNotThrow()
    {
        var folder = new MediaDynamicFolder();

        var exception = Record.Exception(() => folder.OnInputsChanged(new String[0]));

        Assert.Null(exception);
    }

    [Fact]
    public void OnInputsChanged_WithNullInputs_DoesNotThrow()
    {
        var folder = new MediaDynamicFolder();

        var exception = Record.Exception(() => folder.OnInputsChanged(null));

        Assert.Null(exception);
    }
}
