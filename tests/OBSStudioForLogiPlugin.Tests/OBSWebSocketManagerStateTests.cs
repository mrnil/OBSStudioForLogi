namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using OBSWebsocketDotNet.Types;

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

    [Fact]
    public void IsStreaming_DelegatesToActions()
    {
        var manager = new OBSWebSocketManager();
        
        // Initially false
        Assert.False(manager.IsStreaming);
        Assert.False(manager.Actions.IsStreaming);
        
        // Set streaming state via Actions
        manager.Actions.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTED);
        
        // Both should reflect the change
        Assert.True(manager.Actions.IsStreaming);
        Assert.True(manager.IsStreaming);
    }

    [Fact]
    public void IsStreamingChanging_DelegatesToActions()
    {
        var manager = new OBSWebSocketManager();
        
        // Initially false
        Assert.False(manager.IsStreamingChanging);
        Assert.False(manager.Actions.IsStreamingChanging);
        
        // Set streaming state to STARTING
        manager.Actions.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STARTING);
        
        // Both should reflect the change
        Assert.True(manager.Actions.IsStreamingChanging);
        Assert.True(manager.IsStreamingChanging);
    }
}
