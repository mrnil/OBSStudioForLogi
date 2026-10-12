namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Loupedeck.OBSStudioForLogiPlugin.Helpers;

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
}

