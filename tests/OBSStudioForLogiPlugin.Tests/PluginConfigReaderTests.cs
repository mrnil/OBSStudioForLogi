namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using Loupedeck.OBSStudioForLogiPlugin.Models;
using Loupedeck.OBSStudioForLogiPlugin.Services;

public class PluginConfigReaderTests : IDisposable
{
    private readonly String _tempDir;
    private readonly String _configPath;
    private readonly FakeSecretStore _secretStore;
    private readonly PluginConfigReader _reader;

    public PluginConfigReaderTests()
    {
        this._tempDir = Path.Combine(Path.GetTempPath(), $"OBSPluginTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(this._tempDir);
        this._configPath = Path.Combine(this._tempDir, "config.json");
        this._secretStore = new FakeSecretStore();
        this._reader = new PluginConfigReader(this._secretStore, this._configPath);
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
        PluginConfig result = this._reader.ReadConfig();

        Assert.Null(result);
    }

    [Fact]
    public void SaveConfig_WritesFileAndReturnsTrue()
    {
        PluginConfig config = new PluginConfig
        {
            UseLocalObs = false,
            RemoteIpAddress = "192.168.1.50",
            RemotePort = 4456,
            RemotePassword = "secret",
            StatsPollingInterval = 2000
        };

        Boolean result = this._reader.SaveConfig(config);

        Assert.True(result);
        Assert.True(File.Exists(this._configPath));
    }

    [Fact]
    public void SaveConfig_ThenReadConfig_RoundTripsValues()
    {
        PluginConfig config = new PluginConfig
        {
            UseLocalObs = false,
            RemoteIpAddress = "10.0.0.5",
            RemotePort = 4460,
            RemotePassword = "pw123",
            StatsPollingInterval = 10000,
            AudioMeterRefreshInterval = 50
        };

        this._reader.SaveConfig(config);
        PluginConfig loaded = this._reader.ReadConfig();

        Assert.NotNull(loaded);
        Assert.False(loaded.UseLocalObs);
        Assert.Equal("10.0.0.5", loaded.RemoteIpAddress);
        Assert.Equal(4460, loaded.RemotePort);
        Assert.Equal("pw123", loaded.RemotePassword);
        Assert.Equal(10000, loaded.StatsPollingInterval);
        Assert.Equal(50, loaded.AudioMeterRefreshInterval);
    }

    [Fact]
    public void SaveConfig_WhenNull_ReturnsFalse()
    {
        Boolean result = this._reader.SaveConfig(null);

        Assert.False(result);
    }

    [Fact]
    public void SaveConfig_CreatesDirectoryIfNotExists()
    {
        String subPath = Path.Combine(this._tempDir, "sub", "dir", "config.json");
        PluginConfigReader reader = new PluginConfigReader(this._secretStore, subPath);

        Boolean result = reader.SaveConfig(new PluginConfig { UseLocalObs = true });

        Assert.True(result);
        Assert.True(File.Exists(subPath));
    }

    [Fact]
    public void ReadConfig_WhenFileIsInvalidJson_ReturnsNull()
    {
        File.WriteAllText(this._configPath, "not valid json {{{");

        PluginConfig result = this._reader.ReadConfig();

        Assert.Null(result);
    }

    [Fact]
    public void SaveConfig_StoresPasswordInSecretStoreNotInFile()
    {
        this._reader.SaveConfig(new PluginConfig { UseLocalObs = false, RemotePassword = "hunter2" });

        String json = File.ReadAllText(this._configPath);
        Assert.DoesNotContain("hunter2", json);
        Assert.DoesNotContain("RemotePassword", json, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("hunter2", this._secretStore.Values[PluginConfigReader.RemotePasswordSecretName]);
    }

    [Fact]
    public void SaveConfig_WithEmptyPassword_DeletesStoredPassword()
    {
        this._secretStore.Values[PluginConfigReader.RemotePasswordSecretName] = "old";

        this._reader.SaveConfig(new PluginConfig { RemotePassword = "" });

        Assert.False(this._secretStore.Values.ContainsKey(PluginConfigReader.RemotePasswordSecretName));
    }

    [Fact]
    public void SaveConfig_WhenSecretStoreFails_ReturnsFalseAndWritesNoFile()
    {
        this._secretStore.ThrowOnSet = true;

        Boolean result = this._reader.SaveConfig(new PluginConfig { RemotePassword = "hunter2" });

        Assert.False(result);
        Assert.False(File.Exists(this._configPath));
    }

    [Fact]
    public void ReadConfig_WithLegacyPlaintextPassword_MovesItToSecretStore()
    {
        File.WriteAllText(this._configPath, "{ \"UseLocalObs\": false, \"RemoteIpAddress\": \"10.0.0.9\", \"RemotePassword\": \"legacy\" }");

        PluginConfig loaded = this._reader.ReadConfig();

        Assert.Equal("legacy", loaded.RemotePassword);
        Assert.Equal("legacy", this._secretStore.Values[PluginConfigReader.RemotePasswordSecretName]);
        String json = File.ReadAllText(this._configPath);
        Assert.DoesNotContain("legacy", json);
        Assert.Contains("10.0.0.9", json);
    }

    [Fact]
    public void ReadConfig_WithLegacyPasswordInOtherCase_MovesItToSecretStore()
    {
        File.WriteAllText(this._configPath, "{ \"remotePassword\": \"legacy\" }");

        PluginConfig loaded = this._reader.ReadConfig();

        Assert.Equal("legacy", loaded.RemotePassword);
        Assert.DoesNotContain("legacy", File.ReadAllText(this._configPath));
    }

    [Fact]
    public void ReadConfig_WhenMigrationCannotStorePassword_KeepsPlaintextAndStillReturnsIt()
    {
        String original = "{ \"UseLocalObs\": false, \"RemotePassword\": \"legacy\" }";
        File.WriteAllText(this._configPath, original);
        this._secretStore.ThrowOnSet = true;

        PluginConfig loaded = this._reader.ReadConfig();

        Assert.Equal("legacy", loaded.RemotePassword);
        Assert.Equal(original, File.ReadAllText(this._configPath));
    }

    [Fact]
    public void ReadConfig_WhenSecretStoreReadFails_ReturnsEmptyPassword()
    {
        this._reader.SaveConfig(new PluginConfig { RemotePassword = "hunter2" });
        this._secretStore.ThrowOnGet = true;

        PluginConfig loaded = this._reader.ReadConfig();

        Assert.NotNull(loaded);
        Assert.Equal("", loaded.RemotePassword);
    }

    [Fact]
    public void ReadConfig_WithoutSecretStore_ReturnsLegacyPasswordAndLeavesFileAlone()
    {
        String original = "{ \"RemotePassword\": \"legacy\" }";
        File.WriteAllText(this._configPath, original);
        PluginConfigReader reader = new PluginConfigReader(null, this._configPath);

        PluginConfig loaded = reader.ReadConfig();

        Assert.Equal("legacy", loaded.RemotePassword);
        Assert.Equal(original, File.ReadAllText(this._configPath));
    }

    [Fact]
    public void SaveConfig_WithoutSecretStore_NeverWritesPassword()
    {
        PluginConfigReader reader = new PluginConfigReader(null, this._configPath);

        reader.SaveConfig(new PluginConfig { RemotePassword = "hunter2" });

        Assert.DoesNotContain("hunter2", File.ReadAllText(this._configPath));
    }

    private sealed class FakeSecretStore : ISecretStore
    {
        public Dictionary<String, String> Values { get; } = new Dictionary<String, String>();

        public Boolean ThrowOnGet { get; set; }

        public Boolean ThrowOnSet { get; set; }

        public Boolean TryGet(String name, out String value)
        {
            if (this.ThrowOnGet)
            {
                throw new InvalidOperationException("store unavailable");
            }

            Boolean found = this.Values.TryGetValue(name, out String? stored);
            value = stored ?? "";
            return found;
        }

        public void Set(String name, String value)
        {
            if (this.ThrowOnSet)
            {
                throw new InvalidOperationException("store unavailable");
            }

            this.Values[name] = value;
        }

        public void Delete(String name) => this.Values.Remove(name);
    }
}
