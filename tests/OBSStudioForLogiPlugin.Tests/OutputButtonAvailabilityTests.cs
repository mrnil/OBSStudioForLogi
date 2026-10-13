namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Loupedeck.OBSStudioForLogiPlugin.Helpers;

/// <summary>
/// When the Start, Stop and Recording Pause buttons are available. An unavailable button is drawn
/// greyed and ignores presses; the drawing itself needs the SDK and is checked on a device.
/// </summary>
public class OutputButtonAvailabilityTests
{
    [Theory]
    // Start: only while stopped
    [InlineData(true, true, false, false, true)]
    [InlineData(true, true, true, false, false)]
    // Stop: only while running
    [InlineData(false, true, true, false, true)]
    [InlineData(false, true, false, false, false)]
    // Neither while OBS is starting or stopping the output
    [InlineData(true, true, false, true, false)]
    [InlineData(false, true, false, true, false)]
    [InlineData(true, true, true, true, false)]
    [InlineData(false, true, true, true, false)]
    // Neither while disconnected
    [InlineData(true, false, false, false, false)]
    [InlineData(false, false, true, false, false)]
    public void StartStop_IsAvailableOnlyWhenThePressCanDoSomething(
        Boolean isStartButton, Boolean isConnected, Boolean isActive, Boolean isChanging, Boolean expected)
    {
        Assert.Equal(expected, OutputButtonAvailability.StartStop(isStartButton, isConnected, isActive, isChanging));
    }

    [Theory]
    // Running (including paused, which IsRecording counts as running)
    [InlineData(true, true, false, true)]
    // Not recording
    [InlineData(true, false, false, false)]
    // Starting or stopping
    [InlineData(true, false, true, false)]
    [InlineData(true, true, true, false)]
    // Disconnected
    [InlineData(false, true, false, false)]
    public void RecordingPause_IsAvailableOnlyWhileARecordingRuns(
        Boolean isConnected, Boolean isRecording, Boolean isRecordingChanging, Boolean expected)
    {
        Assert.Equal(expected, OutputButtonAvailability.RecordingPause(isConnected, isRecording, isRecordingChanging));
    }
}
