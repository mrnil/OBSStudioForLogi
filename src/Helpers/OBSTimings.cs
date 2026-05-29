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
        /// Test delay for async operations in unit tests (100ms).
        /// </summary>
        public const Int32 TestAsyncDelay = 100;

        /// <summary>
        /// Extended test delay for slower async operations in unit tests (200ms).
        /// </summary>
        public const Int32 TestAsyncDelayExtended = 200;
    }
}
