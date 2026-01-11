namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

public class OBSConnectionSettingsTests
{
    [Fact]
    public void IpAddress_WhenSetToLocalhost_AcceptsValue()
    {
        var settings = new OBSConnectionSettings();
        settings.IpAddress = "127.0.0.1";

        Assert.Equal("127.0.0.1", settings.IpAddress);
    }

    [Fact]
    public void IpAddress_WhenSetToIPv6Localhost_AcceptsValue()
    {
        var settings = new OBSConnectionSettings();
        settings.IpAddress = "::1";

        Assert.Equal("::1", settings.IpAddress);
    }

    [Fact]
    public void IpAddress_WhenSetToNonLocalhost_RejectsAndUsesDefault()
    {
        var settings = new OBSConnectionSettings();
        settings.IpAddress = "192.168.1.1";

        Assert.Equal("127.0.0.1", settings.IpAddress);
    }

    [Fact]
    public void IpAddress_DefaultValue_IsLocalhost()
    {
        var settings = new OBSConnectionSettings();

        Assert.Equal("127.0.0.1", settings.IpAddress);
    }

    [Fact]
    public void GetWebSocketUrl_ReturnsCorrectFormat()
    {
        var settings = new OBSConnectionSettings
        {
            IpAddress = "127.0.0.1",
            Port = 4455
        };

        Assert.Equal("ws://127.0.0.1:4455", settings.GetWebSocketUrl());
    }
}
