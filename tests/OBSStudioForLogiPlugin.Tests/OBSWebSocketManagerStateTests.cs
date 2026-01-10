namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

public class OBSWebSocketManagerStateTests
{
    [Fact]
    public void IsStreaming_InitiallyFalse()
    {
        var manager = new OBSWebSocketManager();
        
        Assert.False(manager.IsStreaming);
    }

    [Fact]
    public void IsRecording_InitiallyFalse()
    {
        var manager = new OBSWebSocketManager();
        
        Assert.False(manager.IsRecording);
    }

    [Fact]
    public void IsStreamingChanging_InitiallyFalse()
    {
        var manager = new OBSWebSocketManager();
        
        Assert.False(manager.IsStreamingChanging);
    }

    [Fact]
    public void IsRecordingChanging_InitiallyFalse()
    {
        var manager = new OBSWebSocketManager();
        
        Assert.False(manager.IsRecordingChanging);
    }
}
