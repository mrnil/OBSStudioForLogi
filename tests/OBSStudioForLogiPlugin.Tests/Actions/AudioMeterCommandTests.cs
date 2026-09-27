namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions;

public class AudioMeterCommandTests
{
    [Fact]
    public void Constructor_SetsStaticInstance()
    {
        var command = new AudioMeterCommand();

        Assert.NotNull(AudioMeterCommand.Instance);
    }
}
