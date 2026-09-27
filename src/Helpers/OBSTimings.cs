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
        /// after the SDK last asked for one of their images (3000ms).
        /// </summary>
        public const Int32 AudioMeterRenderLease = 3000;

        /// <summary>
        /// While no audio meter button is known to be visible, how often their images are
        /// invalidated so a visible button asks to be redrawn and renews the lease (1000ms).
        /// </summary>
        public const Int32 AudioMeterIdleProbeInterval = 1000;

        /// <summary>
        /// After failing to read an input's audio state from OBS, how long the audio state cache
        /// waits before trying that input again, so a stalled OBS isn't hit on every redraw (3000ms).
        /// </summary>
        public const Int32 AudioStateRetryDelay = 3000;

        /// <summary>
        /// Test delay for async operations in unit tests.
        /// 500ms for CI environments (slower), 100ms for local development.
        /// </summary>
        public const Int32 TestAsyncDelay = 500;

        /// <summary>
        /// Extended test delay for slower async operations in unit tests.
        /// 750ms for CI environments (slower), 200ms for local development.
        /// </summary>
        public const Int32 TestAsyncDelayExtended = 750;
    }
}
