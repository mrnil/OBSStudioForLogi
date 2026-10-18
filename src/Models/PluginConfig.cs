namespace Loupedeck.OBSStudioForLogiPlugin.Models
{
    using System;
    using System.Text.Json.Serialization;
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
        /// Remote OBS WebSocket password (used when UseLocalObs is false). Never serialized:
        /// <see cref="Services.PluginConfigReader"/> keeps it in the encrypted secret store.
        /// </summary>
        [JsonIgnore]
        public String RemotePassword { get; set; } = "";

        /// <summary>
        /// Stats polling interval in milliseconds (2000, 5000, or 10000).
        /// </summary>
        public Int32 StatsPollingInterval { get; set; } = 5000;

        /// <summary>
        /// Audio meter refresh interval in milliseconds (50, 100, or 200 - i.e. 20fps, 10fps, or 5fps).
        /// </summary>
        public Int32 AudioMeterRefreshInterval { get; set; } = 100;

        /// <summary>
        /// The windows Plugin Settings offers for <see cref="DoubleTapWindow"/>, in milliseconds.
        /// 0 turns double-tap off.
        /// </summary>
        public static readonly Int32[] DoubleTapWindowOptions = { 0, 200, 300, 500 };

        /// <summary>
        /// How long the audio mixer and media folders wait for a second tap, in milliseconds
        /// (one of <see cref="DoubleTapWindowOptions"/>). 0 runs every tap as a single tap at once.
        /// </summary>
        public Int32 DoubleTapWindow { get; set; } = OBSTimings.DoubleTapWindow;

        /// <summary>
        /// Returns <paramref name="windowMs"/> if Plugin Settings offers it, otherwise the default,
        /// so a hand-edited config can't set a window nobody can tap within.
        /// </summary>
        public static Int32 NormalizeDoubleTapWindow(Int32 windowMs)
        {
            return Array.IndexOf(DoubleTapWindowOptions, windowMs) >= 0 ? windowMs : OBSTimings.DoubleTapWindow;
        }

        /// <summary>
        /// Parses a Plugin Settings listbox value, falling back to the default when it isn't an offered window.
        /// </summary>
        public static Int32 ParseDoubleTapWindow(String value)
        {
            return Int32.TryParse(value?.Trim(), out Int32 windowMs)
                ? NormalizeDoubleTapWindow(windowMs)
                : OBSTimings.DoubleTapWindow;
        }
    }
}
