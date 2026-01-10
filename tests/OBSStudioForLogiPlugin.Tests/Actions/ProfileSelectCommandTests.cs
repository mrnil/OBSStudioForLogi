namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions;

public class ProfileSelectCommandTests
{
    [Fact]
    public void Constructor_SetsStaticInstance()
    {
        var command = new ProfileSelectCommand();

        Assert.NotNull(ProfileSelectCommand.Instance);
    }
}
