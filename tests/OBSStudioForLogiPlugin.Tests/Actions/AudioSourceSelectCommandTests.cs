namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions;

public class AudioSourceSelectCommandTests
{
    [Fact]
    public void Constructor_SetsStaticInstance()
    {
        var command = new AudioSourceSelectCommand();

        Assert.NotNull(AudioSourceSelectCommand.Instance);
    }
}
