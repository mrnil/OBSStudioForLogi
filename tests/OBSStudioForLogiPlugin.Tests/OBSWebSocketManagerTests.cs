namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

public class OBSWebSocketManagerTests
{
    [Fact]
    public void Constructor_ShouldInitializeDisconnected()
    {
        var manager = new OBSWebSocketManager();
        
        Assert.False(manager.IsConnected);
    }

    [Fact]
    public void Dispose_ShouldCleanupResources()
    {
        var manager = new OBSWebSocketManager();
        
        manager.Dispose();
        
        Assert.False(manager.IsConnected);
        Assert.False(manager.ShouldReconnect);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_ShouldBeIdempotent()
    {
        var manager = new OBSWebSocketManager();
        
        manager.Dispose();
        manager.Dispose();
        manager.Dispose();
        
        Assert.False(manager.IsConnected);
    }

    [Fact]
    public void Dispose_WhileReconnecting_ShouldStopReconnection()
    {
        var manager = new OBSWebSocketManager();
        manager.ConnectAsync("ws://localhost:4455", "").Wait();
        System.Threading.Thread.Sleep(100);
        
        manager.Dispose();
        
        Assert.False(manager.ShouldReconnect);
    }
}
