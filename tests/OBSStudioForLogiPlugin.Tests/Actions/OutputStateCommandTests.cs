namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions;

using System;

/// <summary>
/// The streaming and recording buttons only redraw if they receive the state notifications, so
/// each must implement the matching interface (assessment #27). The redraw itself needs the SDK
/// and is checked on a device.
/// </summary>
public class OutputStateCommandTests
{
    [Theory]
    [InlineData(typeof(StreamingToggleCommand))]
    [InlineData(typeof(StreamingStartCommand))]
    [InlineData(typeof(StreamingStopCommand))]
    public void StreamingCommands_ReceiveStreamingStateNotifications(Type commandType)
    {
        Assert.True(typeof(IStreamingAwareCommand).IsAssignableFrom(commandType));
    }

    [Theory]
    [InlineData(typeof(RecordingToggleCommand))]
    [InlineData(typeof(RecordingStartCommand))]
    [InlineData(typeof(RecordingStopCommand))]
    [InlineData(typeof(RecordingPauseToggleCommand))]
    public void RecordingCommands_ReceiveRecordingStateNotifications(Type commandType)
    {
        Assert.True(typeof(IRecordingAwareCommand).IsAssignableFrom(commandType));
    }
}
