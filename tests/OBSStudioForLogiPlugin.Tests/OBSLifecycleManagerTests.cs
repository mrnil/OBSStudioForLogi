namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System.Threading.Tasks;

public class OBSLifecycleManagerTests
{
    [Fact]
    public async Task IsPortListening_WhenPortClosed_ReturnsFalse()
    {
        var manager = new OBSLifecycleManager();
        
        var result = await manager.IsPortListeningAsync("127.0.0.1", 9999);
        
        Assert.False(result);
    }

    [Fact]
    public async Task WaitForPortAsync_ShouldReturnFalseAfterMaxAttempts()
    {
        var manager = new OBSLifecycleManager();
        
        var result = await manager.WaitForPortAsync("127.0.0.1", 9999, maxAttempts: 2, delayMs: 100);
        
        Assert.False(result);
    }
}
