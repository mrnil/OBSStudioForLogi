namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System.IO;
using System.Threading.Tasks;
using Moq;

public class ConnectionManagerTests : IDisposable
{
    private readonly String _configFile;
    private readonly Mock<OBSLifecycleManager> _lifecycle;
    private readonly TaskCompletionSource<Boolean> _portReady = new TaskCompletionSource<Boolean>();
    private readonly ConnectionManager _connectionManager;

    public ConnectionManagerTests()
    {
        this._configFile = Path.GetTempFileName();
        File.WriteAllText(this._configFile, @"{ ""server_enabled"": true, ""server_port"": 4455, ""server_password"": """" }");

        OBSConfigReader configReader = new OBSConfigReader { ConfigPath = this._configFile };

        // Hold every connection attempt in its port wait until the test releases it.
        this._lifecycle = new Mock<OBSLifecycleManager>(new Mock<IPluginLog>().Object);
        this._lifecycle
            .Setup(x => x.WaitForPortAsync(It.IsAny<String>(), It.IsAny<Int32>(), It.IsAny<Int32>(), It.IsAny<Int32>()))
            .Returns(this._portReady.Task);

        this._connectionManager = new ConnectionManager(new OBSWebSocketManager(new Mock<IPluginLog>().Object), configReader, this._lifecycle.Object);
    }

    public void Dispose()
    {
        this._portReady.TrySetResult(false);
        this._connectionManager.Dispose();
        File.Delete(this._configFile);
    }

    [Fact]
    public void IsConnecting_Initially_ReturnsFalse()
    {
        Assert.False(this._connectionManager.IsConnecting);
    }

    [Fact]
    public void IsConnecting_WhileConnectAttemptWaitsForPort_ReturnsTrue()
    {
        Task attempt = this._connectionManager.ConnectAsync();

        Assert.True(this._connectionManager.IsConnecting);

        this._portReady.SetResult(false);
        attempt.Wait(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void IsConnecting_AfterConnectAttemptGivesUp_ReturnsFalse()
    {
        Task attempt = this._connectionManager.ConnectAsync();

        this._portReady.SetResult(false);
        attempt.Wait(TimeSpan.FromSeconds(5));

        Assert.False(this._connectionManager.IsConnecting);
    }

    [Fact]
    public void ReconnectAsync_WhileConnectAttemptInProgress_DoesNotStartAnotherAttempt()
    {
        Task firstAttempt = this._connectionManager.ConnectAsync();

        Task reconnect = this._connectionManager.ReconnectAsync();

        Assert.True(reconnect.IsCompleted);
        this._lifecycle.Verify(
            x => x.WaitForPortAsync(It.IsAny<String>(), It.IsAny<Int32>(), It.IsAny<Int32>(), It.IsAny<Int32>()),
            Times.Once);

        this._portReady.SetResult(false);
        firstAttempt.Wait(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ReconnectAsync_WhenIdle_StartsConnectAttempt()
    {
        Task reconnect = this._connectionManager.ReconnectAsync();

        this._lifecycle.Verify(
            x => x.WaitForPortAsync(It.IsAny<String>(), It.IsAny<Int32>(), It.IsAny<Int32>(), It.IsAny<Int32>()),
            Times.Once);

        this._portReady.SetResult(false);
        reconnect.Wait(TimeSpan.FromSeconds(5));
    }
}
