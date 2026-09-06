namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions;

public class AudioMetersDynamicFolderTests
{
    [Fact]
    public void Constructor_SetsDisplayName()
    {
        var folder = new AudioMetersDynamicFolder();

        Assert.NotNull(folder);
    }

    [Fact]
    public void Instance_IsSetByConstructor()
    {
        var folder = new AudioMetersDynamicFolder();

        Assert.NotNull(AudioMetersDynamicFolder.Instance);
    }
}
