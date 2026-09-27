namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System.Threading.Tasks;

public class OBSWebSocketManagerReconnectionTests
{
    [Fact]
    public void Constructor_ShouldInitializeReconnectionDisabled()
    {
        var manager = new OBSWebSocketManager();
        
        Assert.False(manager.ShouldReconnect);
    }

    [Fact]
    public async Task ConnectAsync_ShouldEnableReconnection()
    {
        var manager = new OBSWebSocketManager();
        
        await manager.ConnectAsync("ws://localhost:4455", "");
        
        Assert.True(manager.ShouldReconnect);
    }

    [Fact]
    public async Task ConnectAsync_WhenNothingIsListening_ClearsConnectingFlag()
    {
        OBSWebSocketManager manager = new OBSWebSocketManager(new Moq.Mock<IPluginLog>().Object);
        Int32 closedPort = GetUnusedPort();

        await manager.ConnectAsync($"ws://127.0.0.1:{closedPort}", "");

        // The failed start is reported asynchronously (Disconnected), so poll for the flag to clear.
        DateTime deadline = DateTime.UtcNow.AddSeconds(5);
        while (manager.IsConnecting && DateTime.UtcNow < deadline)
        {
            await Task.Delay(20);
        }

        Assert.False(manager.IsConnecting);
        Assert.False(manager.IsConnected);
        manager.Dispose();
    }

    [Fact]
    public void Constructor_SetsShortRequestTimeout()
    {
        using OBSWebSocketManager manager = new OBSWebSocketManager(new Moq.Mock<IPluginLog>().Object);

        Assert.Equal(TimeSpan.FromMilliseconds(Helpers.OBSTimings.RequestTimeout), manager.RequestTimeout);
    }

    // ConnectAsync replaces the websocket client; the request timeout must survive that.
    [Fact]
    public async Task ConnectAsync_KeepsRequestTimeout()
    {
        using OBSWebSocketManager manager = new OBSWebSocketManager(new Moq.Mock<IPluginLog>().Object);

        await manager.ConnectAsync($"ws://127.0.0.1:{GetUnusedPort()}", "");

        Assert.Equal(TimeSpan.FromMilliseconds(Helpers.OBSTimings.RequestTimeout), manager.RequestTimeout);
    }

    private static Int32 GetUnusedPort()
    {
        System.Net.Sockets.TcpListener listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        Int32 port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    [Fact]
    public void Disconnect_ShouldDisableReconnection()
    {
        var manager = new OBSWebSocketManager();
        
        manager.Disconnect();
        
        Assert.False(manager.ShouldReconnect);
    }

    [Fact]
    public void GetReconnectDelay_ShouldReturnDelayWithinJitterRange()
    {
        var manager = new OBSWebSocketManager();
        
        // GetReconnectDelay delegates to ReconnectionStrategy
        // Initial state (0 attempts) returns first tier delay
        var delay = manager.GetReconnectDelay(0);
        Assert.InRange(delay, 850, 1150); // 1000 * 0.85-1.15
    }
}
