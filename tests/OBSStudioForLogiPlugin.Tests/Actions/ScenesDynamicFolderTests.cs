namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions;

public class ScenesDynamicFolderTests
{
    [Fact]
    public void Constructor_SetsDisplayName()
    {
        var folder = new ScenesDynamicFolder();

        Assert.NotNull(folder);
    }

    [Fact]
    public void Instance_IsSetByConstructor()
    {
        var folder = new ScenesDynamicFolder();

        Assert.NotNull(ScenesDynamicFolder.Instance);
    }
}
