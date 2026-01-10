namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions;

public class SceneCollectionSelectCommandTests
{
    [Fact]
    public void Constructor_SetsStaticInstance()
    {
        var command = new SceneCollectionSelectCommand();

        Assert.NotNull(SceneCollectionSelectCommand.Instance);
    }
}
