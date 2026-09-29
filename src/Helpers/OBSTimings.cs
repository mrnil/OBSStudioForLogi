namespace Loupedeck.OBSStudioForLogiPlugin.Helpers
{
    using System;

    /// <summary>
    /// Centralized timing constants for OBS operations.
    /// </summary>
    public static class OBSTimings
    {
        /// <summary>
        /// Delay after switching OBS profile to allow profile to fully load (1500ms).
        /// </summary>
        public const Int32 ProfileSwitchDelay = 1500;

        /// <summary>
        /// Delay after switching scene collection to allow collection to fully load (1500ms).
        /// </summary>
        public const Int32 CollectionSwitchDelay = 1500;

        /// <summary>
        /// Delay after switching scene to allow scene to fully load (500ms).
        /// </summary>
        public const Int32 SceneSwitchDelay = 500;

        /// <summary>
        /// Delay after OBS state change to allow state to propagate (100ms).
        /// </summary>
        public const Int32 StateUpdateDelay = 100;

        /// <summary>
        /// How long an OBS request waits for a reply before failing (3000ms). The library default is
        /// 10s, and every request blocks its caller for the whole wait, so a stalled OBS held threads
        /// (and anything waiting on them) for 10s per request. OBS normally replies in milliseconds.
        /// </summary>
        public const Int32 RequestTimeout = 3000;

        /// <summary>
        /// While OBS is still starting it accepts connections but answers requests with "not
        /// ready", so the initial state load waits this long between attempts (500ms).
        /// </summary>
        public const Int32 InitialStateRetryDelay = 500;

        /// <summary>
        /// How many times the initial state load tries before giving up while OBS keeps answering
        /// "not ready" (60, about 30s at <see cref="InitialStateRetryDelay"/>).
        /// </summary>
        public const Int32 InitialStateMaxAttempts = 60;

        /// <summary>
        /// When OBS isn't running, how long to wait after a connection attempt's port wait gives up
        /// before starting the next attempt (30000ms). Each attempt already polls the port for about a
        /// minute (20 probes, each ~2s to fail on Windows plus a 1s gap).
        /// </summary>
        public const Int32 ConnectRetryDelay = 30000;

        /// <summary>
        /// Delay after OBS application starts before attempting connection (2000ms).
        /// </summary>
        public const Int32 ConnectionDelay = 2000;

        /// <summary>
        /// How long an input's audio meter levels stay valid without a fresh InputVolumeMeters
        /// update before the input is treated as inactive (500ms, ~10 missed OBS meter events).
        /// </summary>
        public const Int32 AudioMeterStaleThreshold = 500;

        /// <summary>
        /// How long individual audio meter buttons keep the InputVolumeMeters subscription alive
        /// after the SDK last asked for one of their images (15000ms). Each subscribe/unsubscribe
        /// sends OBS a ReIdentify, so this is long enough to ride out paging away and back rather
        /// than toggling the subscription on every page flip; idle meters still unsubscribe.
        /// </summary>
        public const Int32 AudioMeterRenderLease = 15000;

        /// <summary>
        /// While no audio meter button is known to be visible, how often their images are
        /// invalidated so a visible button asks to be redrawn and renews the lease (1000ms).
        /// </summary>
        public const Int32 AudioMeterIdleProbeInterval = 1000;

        /// <summary>
        /// How long the stats summary button keeps stats polling alive after the SDK last asked for
        /// its image (5000ms). Starting and stopping polling costs nothing on the OBS side, so this
        /// only needs to ride out a quick page flip.
        /// </summary>
        public const Int32 StatsRenderLease = 5000;

        /// <summary>
        /// How often the stats summary button's image is invalidated so a visible button asks to be
        /// redrawn and renews its lease (1000ms). The redraw reads cached stats, not OBS.
        /// </summary>
        public const Int32 StatsIdleProbeInterval = 1000;

        /// <summary>
        /// After failing to read an input's audio state from OBS, how long the audio state cache
        /// waits before trying that input again, so a stalled OBS isn't hit on every redraw (3000ms).
        /// </summary>
        public const Int32 AudioStateRetryDelay = 3000;

        /// <summary>
        /// The same retry delay for KeyedStateCache (source visibility and media state) (3000ms).
        /// </summary>
        public const Int32 RenderStateRetryDelay = 3000;

        /// <summary>
        /// Identical warnings/errors within this window are written once, and the next occurrence
        /// after it reports how many were suppressed (60000ms).
        /// </summary>
        public const Int32 LogRepeatWindow = 60000;

        /// <summary>
        /// Test wait for code driven by a real timer, such as DoubleTapHelper's tap window.
        /// Not for OBSActionExecutor tests: pass an inline background runner to its constructor instead.
        /// </summary>
        public const Int32 TestAsyncDelay = 500;
    }
}
