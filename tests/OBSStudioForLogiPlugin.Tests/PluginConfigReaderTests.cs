namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System;
using System.IO;
using Loupedeck.OBSStudioForLogiPlugin.Models;
using Loupedeck.OBSStudioForLogiPlugin.Services;

public class PluginConfigReaderTests : IDisposable
{
    private readonly String _tempDir;
    private readonly String _configPath;
    private readonly PluginConfigReader _reader;

    public PluginConfigReaderTests()
    {
        this._tempDir = Path.Combine(Path.GetTempPath(), $"OBSPluginTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(this._tempDir);
        this._configPath = Path.Combine(this._tempDir, "config.json");

        // Use reflection to set the private config path for testing
        this._reader = new PluginConfigReader();
        var field = typeof(PluginConfigReader).GetField("_configPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(this._reader, this._configPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(this._tempDir))
        {
            Directory.Delete(this._tempDir, true);
        }
    }

    [Fact]
    public void ReadConfig_WhenFileDoesNotExist_ReturnsNull()
    {
        var result = this._reader.ReadConfig();

        Assert.Null(result);
    }

    [Fact]
    public void SaveConfig_WritesFileAndReturnsTrue()
    {
        var config = new PluginConfig
        {
            UseLocalObs = false,
            RemoteIpAddress = "192.168.1.50",
            RemotePort = 4456,
            RemotePassword = "secret",
            StatsPollingInterval = 2000
        };

        var result = this._reader.SaveConfig(config);

        Assert.True(result);
        Assert.True(File.Exists(this._configPath));
    }

    [Fact]
    public void SaveConfig_ThenReadConfig_RoundTripsValues()
    {
        var config = new PluginConfig
        {
            UseLocalObs = false,
            RemoteIpAddress = "10.0.0.5",
            RemotePort = 4460,
            RemotePassword = "pw123",
            StatsPollingInterval = 10000
        };

        this._reader.SaveConfig(config);
        var loaded = this._reader.ReadConfig();

        Assert.NotNull(loaded);
        Assert.False(loaded.UseLocalObs);
        Assert.Equal("10.0.0.5", loaded.RemoteIpAddress);
        Assert.Equal(4460, loaded.RemotePort);
        Assert.Equal("pw123", loaded.RemotePassword);
        Assert.Equal(10000, loaded.StatsPollingInterval);
    }

    [Fact]
    public void SaveConfig_WhenNull_ReturnsFalse()
    {
        var result = this._reader.SaveConfig(null);

        Assert.False(result);
    }

    [Fact]
    public void SaveConfig_CreatesDirectoryIfNotExists()
    {
        var subDir = Path.Combine(this._tempDir, "sub", "dir");
        var subPath = Path.Combine(subDir, "config.json");
        var field = typeof(PluginConfigReader).GetField("_configPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(this._reader, subPath);

        var config = new PluginConfig { UseLocalObs = true };
        var result = this._reader.SaveConfig(config);

        Assert.True(result);
        Assert.True(File.Exists(subPath));
    }

    [Fact]
    public void ReadConfig_WhenFileIsInvalidJson_ReturnsNull()
    {
        File.WriteAllText(this._configPath, "not valid json {{{");

        var result = this._reader.ReadConfig();

        Assert.Null(result);
    }
}
