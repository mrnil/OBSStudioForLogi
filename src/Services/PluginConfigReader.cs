namespace Loupedeck.OBSStudioForLogiPlugin.Services
{
    using System;
    using System.IO;
    using System.Text.Json;
    using Loupedeck.OBSStudioForLogiPlugin.Models;

    /// <summary>
    /// Reads and writes plugin configuration from/to file.
    /// </summary>
    public class PluginConfigReader
    {
        private readonly String _configPath;

        public PluginConfigReader()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configDir = Path.Combine(appDataPath, "Loupedeck", "OBSStudioForLogiPlugin");
            this._configPath = Path.Combine(configDir, "config.json");
        }

        /// <summary>
        /// Reads the plugin configuration from file.
        /// </summary>
        /// <returns>Plugin configuration, or null if file doesn't exist or is invalid.</returns>
        public PluginConfig ReadConfig()
        {
            if (!File.Exists(this._configPath))
            {
                return null;
            }

            try
            {
                var json = File.ReadAllText(this._configPath);
                var config = JsonSerializer.Deserialize<PluginConfig>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return config;
            }
            catch (Exception ex)
            {
                PluginLog.Warning($"Failed to read plugin config from '{this._configPath}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Saves the plugin configuration to file.
        /// </summary>
        public Boolean SaveConfig(PluginConfig config)
        {
            if (config == null)
                return false;

            try
            {
                var directory = Path.GetDirectoryName(this._configPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(this._configPath, json);
                PluginLog.Info($"Plugin config saved to '{this._configPath}'");
                return true;
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to save plugin config to '{this._configPath}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the configuration file path.
        /// </summary>
        public String ConfigPath => this._configPath;
    }
}
