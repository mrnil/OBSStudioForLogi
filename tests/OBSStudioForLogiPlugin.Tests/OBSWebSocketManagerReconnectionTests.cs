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
    public void GetReconnectDelay_ShouldReturnExponentialBackoffWithJitter()
    {
        var manager = new OBSWebSocketManager();
        
        // Test that delays are within jitter range (0.85-1.15x base delay)
        var delay0 = manager.GetReconnectDelay(0);
        Assert.InRange(delay0, 850, 1150); // 1000 * 0.85-1.15
        
        var delay1 = manager.GetReconnectDelay(1);
        Assert.InRange(delay1, 1700, 2300); // 2000 * 0.85-1.15
        
        var delay2 = manager.GetReconnectDelay(2);
        Assert.InRange(delay2, 3400, 4600); // 4000 * 0.85-1.15
        
        var delay5 = manager.GetReconnectDelay(5);
        Assert.InRange(delay5, 25500, 34500); // 30000 * 0.85-1.15
        
        var delay10 = manager.GetReconnectDelay(10);
        Assert.InRange(delay10, 25500, 34500); // Max delay with jitter
    }
}
