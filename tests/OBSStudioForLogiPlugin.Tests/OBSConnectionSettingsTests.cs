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
    public void IpAddress_WhenSetToRemoteAddress_AcceptsValue()
    {
        var settings = new OBSConnectionSettings();
        settings.IpAddress = "192.168.1.100";

        Assert.Equal("192.168.1.100", settings.IpAddress);
    }

    [Fact]
    public void IpAddress_WhenSetToInvalidFormat_UsesDefault()
    {
        var settings = new OBSConnectionSettings();
        settings.IpAddress = "not-an-ip";

        Assert.Equal("127.0.0.1", settings.IpAddress);
    }

    [Fact]
    public void IpAddress_WhenSetToEmpty_UsesDefault()
    {
        var settings = new OBSConnectionSettings();
        settings.IpAddress = "";

        Assert.Equal("127.0.0.1", settings.IpAddress);
    }

    [Fact]
    public void IpAddress_DefaultValue_IsLocalhost()
    {
        var settings = new OBSConnectionSettings();

        Assert.Equal("127.0.0.1", settings.IpAddress);
    }

    [Fact]
    public void IsLocalhost_WhenLocalhost_ReturnsTrue()
    {
        var settings = new OBSConnectionSettings { IpAddress = "127.0.0.1" };

        Assert.True(settings.IsLocalhost);
    }

    [Fact]
    public void IsLocalhost_WhenIPv6Localhost_ReturnsTrue()
    {
        var settings = new OBSConnectionSettings { IpAddress = "::1" };

        Assert.True(settings.IsLocalhost);
    }

    [Fact]
    public void IsLocalhost_WhenRemoteAddress_ReturnsFalse()
    {
        var settings = new OBSConnectionSettings { IpAddress = "192.168.1.100" };

        Assert.False(settings.IsLocalhost);
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

    [Fact]
    public void GetWebSocketUrl_WithRemoteAddress_ReturnsCorrectFormat()
    {
        var settings = new OBSConnectionSettings
        {
            IpAddress = "192.168.1.50",
            Port = 4456
        };

        Assert.Equal("ws://192.168.1.50:4456", settings.GetWebSocketUrl());
    }
}
