namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions;

public class StatusDisplayCommandTests
{
    [Fact]
    public void CurrentProfileDisplay_CanBeConstructed()
    {
        var command = new CurrentProfileDisplay();

        Assert.NotNull(command);
        Assert.NotNull(CurrentProfileDisplay.Instance);
    }

    [Fact]
    public void CurrentSceneCollectionDisplay_CanBeConstructed()
    {
        var command = new CurrentSceneCollectionDisplay();

        Assert.NotNull(command);
        Assert.NotNull(CurrentSceneCollectionDisplay.Instance);
    }

    [Fact]
    public void CurrentSceneDisplay_CanBeConstructed()
    {
        var command = new CurrentSceneDisplay();

        Assert.NotNull(command);
        Assert.NotNull(CurrentSceneDisplay.Instance);
    }
}
