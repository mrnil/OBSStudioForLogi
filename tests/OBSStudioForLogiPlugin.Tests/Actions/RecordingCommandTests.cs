namespace Loupedeck.OBSStudioForLogiPlugin.Tests.Actions;

public class RecordingCommandTests
{
    [Fact]
    public void RecordingStartCommand_CanBeConstructed()
    {
        var command = new RecordingStartCommand();

        Assert.NotNull(command);
    }

    [Fact]
    public void RecordingStopCommand_CanBeConstructed()
    {
        var command = new RecordingStopCommand();

        Assert.NotNull(command);
    }

    [Fact]
    public void RecordingPauseToggleCommand_CanBeConstructed()
    {
        var command = new RecordingPauseToggleCommand();

        Assert.NotNull(command);
    }
}
