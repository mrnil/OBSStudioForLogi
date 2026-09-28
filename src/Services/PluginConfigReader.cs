namespace Loupedeck.OBSStudioForLogiPlugin.Services
{
    using System;
    using System.IO;
    using System.Text.Json;
    using Loupedeck.OBSStudioForLogiPlugin.Models;

    /// <summary>
    /// Reads and writes plugin configuration from/to file. The remote OBS password is kept out of
    /// the file and stored in the <see cref="ISecretStore"/> instead.
    /// </summary>
    public class PluginConfigReader
    {
        public const String RemotePasswordSecretName = "RemotePassword";

        private readonly String _configPath;
        private readonly ISecretStore _secretStore;

        // Without a secret store the password is neither read from nor written to secure storage.
        // Callers that only need non-secret values (log level, refresh intervals) use this.
        public PluginConfigReader()
            : this(null)
        {
        }

        public PluginConfigReader(ISecretStore secretStore)
            : this(secretStore, DefaultConfigPath())
        {
        }

        public PluginConfigReader(ISecretStore secretStore, String configPath)
        {
            this._secretStore = secretStore;
            this._configPath = configPath;
        }

        private static String DefaultConfigPath()
        {
            String appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            String configDir = Path.Combine(appDataPath, "Loupedeck", "OBSStudioForLogiPlugin");
            return Path.Combine(configDir, "config.json");
        }

        /// <summary>
        /// Reads the plugin configuration from file. With a secret store, a plaintext password left
        /// in the file by an older version is moved into the store and removed from the file.
        /// </summary>
        /// <returns>Plugin configuration, or null if file doesn't exist or is invalid.</returns>
        public PluginConfig ReadConfig()
        {
            if (!File.Exists(this._configPath))
            {
                return null;
            }

            PluginConfig config;
            String legacyPassword;
            try
            {
                String json = File.ReadAllText(this._configPath);
                config = JsonSerializer.Deserialize<PluginConfig>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                legacyPassword = ReadLegacyPassword(json);
            }
            catch (Exception ex)
            {
                PluginLog.Warning($"Failed to read plugin config from '{this._configPath}': {ex.Message}");
                return null;
            }

            if (config == null)
            {
                return null;
            }

            if (this._secretStore == null)
            {
                config.RemotePassword = legacyPassword ?? "";
                return config;
            }

            // If the store rejects the password, keep using the plaintext copy so the connection
            // still works; the file is left as it was and the next read tries again.
            if (!String.IsNullOrEmpty(legacyPassword) && !this.MigrateLegacyPassword(config, legacyPassword))
            {
                config.RemotePassword = legacyPassword;
                return config;
            }

            config.RemotePassword = this.ReadStoredPassword();
            return config;
        }

        /// <summary>
        /// Saves the plugin configuration to file, and the remote password to the secret store.
        /// </summary>
        public Boolean SaveConfig(PluginConfig config)
        {
            if (config == null)
                return false;

            if (this._secretStore == null)
            {
                if (!String.IsNullOrEmpty(config.RemotePassword))
                {
                    PluginLog.Warning("Plugin config saved without a secret store - the remote password was not saved");
                }
            }
            else if (!this.SavePassword(config.RemotePassword))
            {
                return false;
            }

            return this.WriteConfigFile(config);
        }

        /// <summary>
        /// Saves settings entered in the Plugin Settings action. The Logi Plugin Service keeps
        /// Action Editor values in plaintext in the device profile, so users are asked to clear the
        /// Password field after saving: an empty field keeps the stored password, and only
        /// <paramref name="clearStoredPassword"/> removes it. On return, config.RemotePassword holds
        /// the password in effect, so the caller can connect with it.
        /// </summary>
        public Boolean SaveEditedConfig(PluginConfig config, Boolean clearStoredPassword)
        {
            if (config == null)
                return false;

            if (clearStoredPassword)
            {
                config.RemotePassword = "";
            }
            else if (String.IsNullOrEmpty(config.RemotePassword) && this._secretStore != null)
            {
                config.RemotePassword = this.ReadStoredPassword();
            }

            return this.SaveConfig(config);
        }

        /// <summary>
        /// Gets the configuration file path.
        /// </summary>
        public String ConfigPath => this._configPath;

        // RemotePassword is [JsonIgnore], so a value written by an older version has to be read
        // from the raw JSON. Property names are matched case-insensitively, like the deserializer.
        private static String ReadLegacyPassword(String json)
        {
            using (JsonDocument document = JsonDocument.Parse(json))
            {
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                foreach (JsonProperty property in document.RootElement.EnumerateObject())
                {
                    if (property.Name.Equals(RemotePasswordSecretName, StringComparison.OrdinalIgnoreCase) && property.Value.ValueKind == JsonValueKind.String)
                    {
                        return property.Value.GetString();
                    }
                }
            }

            return null;
        }

        // The store is written before the file is rewritten: if the rewrite fails, the plaintext
        // copy is still there and the next read migrates it again, so the password is never lost.
        private Boolean MigrateLegacyPassword(PluginConfig config, String legacyPassword)
        {
            if (!this.SavePassword(legacyPassword))
            {
                return false;
            }

            if (this.WriteConfigFile(config))
            {
                PluginLog.Info("Moved the remote OBS password from the config file to encrypted plugin settings");
            }

            return true;
        }

        private String ReadStoredPassword()
        {
            try
            {
                return this._secretStore.TryGet(RemotePasswordSecretName, out String password) ? password ?? "" : "";
            }
            catch (Exception ex)
            {
                PluginLog.Warning($"Failed to read the remote OBS password from plugin settings: {ex.Message}");
                return "";
            }
        }

        private Boolean SavePassword(String password)
        {
            try
            {
                if (String.IsNullOrEmpty(password))
                {
                    this._secretStore.Delete(RemotePasswordSecretName);
                }
                else
                {
                    this._secretStore.Set(RemotePasswordSecretName, password);
                }

                return true;
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to save the remote OBS password to plugin settings: {ex.Message}");
                return false;
            }
        }

        private Boolean WriteConfigFile(PluginConfig config)
        {
            try
            {
                String directory = Path.GetDirectoryName(this._configPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                String json = JsonSerializer.Serialize(config, new JsonSerializerOptions
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
    }
}
