namespace Loupedeck.OBSStudioForLogiPlugin.Helpers
{
    using System;

    /// <summary>
    /// Defines logging levels for the plugin.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Very detailed logging for debugging specific issues. Disabled in production.
        /// </summary>
        Trace = 0,

        /// <summary>
        /// Detailed logging for development and troubleshooting.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// General informational messages about plugin operation.
        /// </summary>
        Info = 2,

        /// <summary>
        /// Warning messages for non-critical issues.
        /// </summary>
        Warning = 3,

        /// <summary>
        /// Error messages for failures and exceptions.
        /// </summary>
        Error = 4
    }
}
