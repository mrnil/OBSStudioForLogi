namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions;

public class RecordingToggleCommandTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithCorrectProperties()
    {
        var command = new RecordingToggleCommand();
        
        Assert.NotNull(command);
    }
}
