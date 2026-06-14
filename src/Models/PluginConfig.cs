namespace Loupedeck.OBSStudioForLogiPlugin.Models
{
    using System;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    /// <summary>
    /// Configuration settings for the plugin.
    /// </summary>
    public class PluginConfig
    {
        /// <summary>
        /// Gets or sets the logging level for the plugin.
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// When true, auto-discovers local OBS WebSocket settings. When false, uses manual connection settings.
        /// </summary>
        public Boolean UseLocalObs { get; set; } = true;

        /// <summary>
        /// Remote OBS IP address (used when UseLocalObs is false).
        /// </summary>
        public String RemoteIpAddress { get; set; } = "127.0.0.1";

        /// <summary>
        /// Remote OBS WebSocket port (used when UseLocalObs is false).
        /// </summary>
        public Int32 RemotePort { get; set; } = 4455;

        /// <summary>
        /// Remote OBS WebSocket password (used when UseLocalObs is false).
        /// </summary>
        public String RemotePassword { get; set; } = "";

        /// <summary>
        /// Stats polling interval in milliseconds (2000, 5000, or 10000).
        /// </summary>
        public Int32 StatsPollingInterval { get; set; } = 5000;
    }
}
