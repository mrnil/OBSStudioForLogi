namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System.Threading.Tasks;

public class OBSWebSocketManagerReconnectionTests
{
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

}
