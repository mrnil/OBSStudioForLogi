namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions;

public class SceneSelectCommandTests
{
    [Fact]
    public void Constructor_SetsStaticInstance()
    {
        var command = new SceneSelectCommand();

        Assert.NotNull(SceneSelectCommand.Instance);
    }
}
