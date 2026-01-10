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
    public void Disconnect_ShouldDisableReconnection()
    {
        var manager = new OBSWebSocketManager();
        
        manager.Disconnect();
        
        Assert.False(manager.ShouldReconnect);
    }

    [Fact]
    public void GetReconnectDelay_ShouldReturnExponentialBackoff()
    {
        var manager = new OBSWebSocketManager();
        
        Assert.Equal(1000, manager.GetReconnectDelay(0));
        Assert.Equal(2000, manager.GetReconnectDelay(1));
        Assert.Equal(4000, manager.GetReconnectDelay(2));
        Assert.Equal(8000, manager.GetReconnectDelay(3));
        Assert.Equal(15000, manager.GetReconnectDelay(4));
        Assert.Equal(30000, manager.GetReconnectDelay(5));
        Assert.Equal(30000, manager.GetReconnectDelay(10)); // Max delay
    }
}
