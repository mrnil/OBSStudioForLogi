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
    public void GetReconnectDelay_ShouldReturnDelayWithinJitterRange()
    {
        var manager = new OBSWebSocketManager();
        
        // GetReconnectDelay delegates to ReconnectionStrategy
        // Initial state (0 attempts) returns first tier delay
        var delay = manager.GetReconnectDelay(0);
        Assert.InRange(delay, 850, 1150); // 1000 * 0.85-1.15
    }
}
