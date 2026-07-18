namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions;

public class SceneCollectionsDynamicFolderTests
{
    [Fact]
    public void Constructor_SetsInstance()
    {
        var folder = new SceneCollectionsDynamicFolder();

        Assert.NotNull(SceneCollectionsDynamicFolder.Instance);
    }

    [Fact]
    public void OnConnected_DoesNotThrow()
    {
        var folder = new SceneCollectionsDynamicFolder();

        var exception = Record.Exception(() => folder.OnConnected());

        Assert.Null(exception);
    }

    [Fact]
    public void OnDisconnected_DoesNotThrow()
    {
        var folder = new SceneCollectionsDynamicFolder();

        var exception = Record.Exception(() => folder.OnDisconnected());

        Assert.Null(exception);
    }

    [Fact]
    public void OnSceneCollectionChanged_WithNullValues_DoesNotThrow()
    {
        var folder = new SceneCollectionsDynamicFolder();

        // null/empty values return early before calling SDK CommandImageChanged
        var exception = Record.Exception(() => folder.OnSceneCollectionChanged(null, null));

        Assert.Null(exception);
    }

    [Fact]
    public void RunCommand_WithEmptyParameter_DoesNotThrow()
    {
        var folder = new SceneCollectionsDynamicFolder();

        var exception = Record.Exception(() => folder.RunCommand(String.Empty));

        Assert.Null(exception);
    }
}
