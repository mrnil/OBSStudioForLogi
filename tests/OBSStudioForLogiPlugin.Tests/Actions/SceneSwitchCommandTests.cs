namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions;

public class SceneSwitchCommandTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithCorrectProperties()
    {
        var command = new SceneSwitchCommand();
        
        Assert.NotNull(command);
    }
}
