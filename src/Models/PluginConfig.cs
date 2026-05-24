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
    }
}
