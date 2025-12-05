namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

public class OBSConnectionSettingsTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        var settings = new OBSConnectionSettings();
        
        Assert.Equal("127.0.0.1", settings.IpAddress);
        Assert.Equal(4455, settings.Port);
        Assert.Equal("", settings.Password);
    }

    [Fact]
    public void GetWebSocketUrl_ShouldFormatCorrectly()
    {
        var settings = new OBSConnectionSettings { IpAddress = "192.168.1.100", Port = 4455 };
        
        var url = settings.GetWebSocketUrl();
        
        Assert.Equal("ws://192.168.1.100:4455", url);
    }
}
