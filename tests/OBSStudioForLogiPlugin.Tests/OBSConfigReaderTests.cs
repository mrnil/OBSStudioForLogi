namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System.IO;

public class OBSConfigReaderTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultConfigPath()
    {
        var reader = new OBSConfigReader();
        
        Assert.NotNull(reader.ConfigPath);
        Assert.Contains("obs-studio", reader.ConfigPath);
    }

    [Fact]
    public void ConfigExists_WhenFileNotFound_ReturnsFalse()
    {
        var reader = new OBSConfigReader();
        reader.ConfigPath = "nonexistent.json";
        
        Assert.False(reader.ConfigExists);
    }

    [Fact]
    public void ReadConfig_WhenValidConfig_ReturnsSettings()
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, @"{
            ""server_enabled"": true,
            ""auth_required"": true,
            ""server_port"": 4455,
            ""server_password"": ""test123""
        }");

        var reader = new OBSConfigReader();
        reader.ConfigPath = tempFile;
        var settings = reader.ReadConfig();

        Assert.NotNull(settings);
        Assert.Equal(4455, settings.Port);
        Assert.Equal("test123", settings.Password);

        File.Delete(tempFile);
    }

    [Fact]
    public void ReadConfig_WhenServerDisabled_ReturnsNull()
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, @"{
            ""server_enabled"": false,
            ""auth_required"": true,
            ""server_port"": 4455,
            ""server_password"": ""test123""
        }");

        var reader = new OBSConfigReader();
        reader.ConfigPath = tempFile;
        var settings = reader.ReadConfig();

        Assert.Null(settings);

        File.Delete(tempFile);
    }

    [Fact]
    public void ReadConfig_WhenFileNotExists_ReturnsNull()
    {
        var reader = new OBSConfigReader();
        reader.ConfigPath = "nonexistent.json";
        
        var settings = reader.ReadConfig();
        
        Assert.Null(settings);
    }

    [Fact]
    public void ReadConfig_WhenPortInvalid_ReturnsNull()
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, @"{
            ""server_enabled"": true,
            ""auth_required"": true,
            ""server_port"": 99999,
            ""server_password"": ""test123""
        }");

        var reader = new OBSConfigReader();
        reader.ConfigPath = tempFile;
        var settings = reader.ReadConfig();

        Assert.Null(settings);

        File.Delete(tempFile);
    }

    [Fact]
    public void ReadConfig_WhenPortZero_ReturnsNull()
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, @"{
            ""server_enabled"": true,
            ""auth_required"": true,
            ""server_port"": 0,
            ""server_password"": ""test123""
        }");

        var reader = new OBSConfigReader();
        reader.ConfigPath = tempFile;
        var settings = reader.ReadConfig();

        Assert.Null(settings);

        File.Delete(tempFile);
    }
}
