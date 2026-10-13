namespace Loupedeck.OBSStudioForLogiPlugin.Helpers
{
    using System;

    // Whether a Start, Stop or Pause button for an output (stream, recording, virtual camera) can
    // do anything right now. An unavailable button is drawn greyed and ignores presses, rather than
    // passing the press on for the executor to refuse.
    public static class OutputButtonAvailability
    {
        // Start only while the output is stopped, Stop only while it runs. Neither while OBS is
        // starting or stopping it - a second start or stop then just fails in OBS - nor while
        // disconnected.
        public static Boolean StartStop(Boolean isStartButton, Boolean isConnected, Boolean isActive, Boolean isChanging)
        {
            if (!isConnected || isChanging)
            {
                return false;
            }

            return isStartButton ? !isActive : isActive;
        }

        // Pause/resume only while a recording runs (paused counts as running), not while it is
        // starting or stopping.
        public static Boolean RecordingPause(Boolean isConnected, Boolean isRecording, Boolean isRecordingChanging)
        {
            return isConnected && isRecording && !isRecordingChanging;
        }
    }
}
