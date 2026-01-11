namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.IO;
    using System.Text.Json;

    public class OBSConfigReader
    {
        public String ConfigPath { get; set; }

        public Boolean ConfigExists => File.Exists(this.ConfigPath);

        public OBSConfigReader()
        {
            this.ConfigPath = this.GetDefaultConfigPath();
        }

        private String GetDefaultConfigPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "obs-studio", "plugin_config", "obs-websocket", "config.json");
        }

        public OBSConnectionSettings ReadConfig()
        {
            PluginLog.Info("Reading OBS config from AppData");
            
            if (!this.ConfigExists)
            {
                PluginLog.Warning("OBS config file not found");
                return null;
            }

            try
            {
                var json = File.ReadAllText(this.ConfigPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var serverEnabled = root.GetProperty("server_enabled").GetBoolean();
                if (!serverEnabled)
                {
                    PluginLog.Warning("OBS WebSocket server is disabled in config");
                    return null;
                }

                var port = root.GetProperty("server_port").GetInt32();
                
                if (port < 1 || port > 65535)
                {
                    PluginLog.Warning($"Invalid port number: {port}");
                    return null;
                }
                
                var password = root.GetProperty("server_password").GetString();

                PluginLog.Info($"OBS config loaded: port={port}");
                
                return new OBSConnectionSettings
                {
                    IpAddress = "127.0.0.1",
                    Port = port,
                    Password = password ?? ""
                };
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to read OBS config: {ex.Message}");
                return null;
            }
        }
    }
}
