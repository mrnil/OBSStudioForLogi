namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Loupedeck.OBSStudioForLogiPlugin.Helpers;

public class MediaInputStatesTests
{
    [Theory]
    [InlineData("OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PLAY", "OBS_MEDIA_STATE_PLAYING")]
    [InlineData("OBS_WEBSOCKET_MEDIA_INPUT_ACTION_RESTART", "OBS_MEDIA_STATE_PLAYING")]
    [InlineData("OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PAUSE", "OBS_MEDIA_STATE_PAUSED")]
    [InlineData("OBS_WEBSOCKET_MEDIA_INPUT_ACTION_STOP", "OBS_MEDIA_STATE_STOPPED")]
    public void StateAfterAction_ForKnownAction_ReturnsResultingState(String action, String expectedState)
    {
        Assert.Equal(expectedState, MediaInputStates.StateAfterAction(action));
    }

    [Theory]
    [InlineData("OBS_WEBSOCKET_MEDIA_INPUT_ACTION_NEXT")]
    [InlineData("OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PREVIOUS")]
    [InlineData("OBS_WEBSOCKET_MEDIA_INPUT_ACTION_NONE")]
    [InlineData("")]
    [InlineData(null)]
    public void StateAfterAction_WhenStateCannotBeKnown_ReturnsNull(String? action)
    {
        Assert.Null(MediaInputStates.StateAfterAction(action!));
    }

    [Theory]
    [InlineData("OBS_MEDIA_STATE_PLAYING", "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PAUSE")]
    [InlineData("OBS_MEDIA_STATE_PAUSED", "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PLAY")]
    [InlineData("OBS_MEDIA_STATE_STOPPED", "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_RESTART")]
    [InlineData("OBS_MEDIA_STATE_ENDED", "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_RESTART")]
    [InlineData("OBS_MEDIA_STATE_NONE", "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_RESTART")]
    public void ToggleActionFor_ReturnsSingleTapAction(String state, String expectedAction)
    {
        Assert.Equal(expectedAction, MediaInputStates.ToggleActionFor(state));
    }
}
