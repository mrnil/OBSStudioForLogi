namespace Loupedeck.OBSStudioForLogiPlugin.Helpers
{
    using System;

    // OBS media state and media action names, and what state each action leaves an input in.
    public static class MediaInputStates
    {
        public const String None = "OBS_MEDIA_STATE_NONE";
        public const String Playing = "OBS_MEDIA_STATE_PLAYING";
        public const String Paused = "OBS_MEDIA_STATE_PAUSED";
        public const String Stopped = "OBS_MEDIA_STATE_STOPPED";
        public const String Ended = "OBS_MEDIA_STATE_ENDED";

        public const String ActionPlay = "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PLAY";
        public const String ActionPause = "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PAUSE";
        public const String ActionStop = "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_STOP";
        public const String ActionRestart = "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_RESTART";

        // Null when the resulting state can't be known from the action alone (next/previous
        // playlist item, or an action this plugin doesn't recognise) - the caller should re-read it.
        public static String StateAfterAction(String mediaAction)
        {
            switch (mediaAction)
            {
                case ActionPlay:
                case ActionRestart:
                    return Playing;
                case ActionPause:
                    return Paused;
                case ActionStop:
                    return Stopped;
                default:
                    return null;
            }
        }

        // The single-tap action for a media button: pause while playing, resume while paused,
        // otherwise start again from the beginning.
        public static String ToggleActionFor(String state)
        {
            switch (state)
            {
                case Playing:
                    return ActionPause;
                case Paused:
                    return ActionPlay;
                default:
                    return ActionRestart;
            }
        }
    }
}
