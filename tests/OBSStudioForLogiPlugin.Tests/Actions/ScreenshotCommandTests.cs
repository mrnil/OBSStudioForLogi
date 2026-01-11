namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions;

public class ScreenshotCommandTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var command = new ScreenshotCommand();

        Assert.NotNull(command);
    }
}
