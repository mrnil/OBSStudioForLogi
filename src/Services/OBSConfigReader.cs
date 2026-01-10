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
            if (!this.ConfigExists)
                return null;

            try
            {
                var json = File.ReadAllText(this.ConfigPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var serverEnabled = root.GetProperty("server_enabled").GetBoolean();
                if (!serverEnabled)
                    return null;

                var port = root.GetProperty("server_port").GetInt32();
                var password = root.GetProperty("server_password").GetString();

                return new OBSConnectionSettings
                {
                    IpAddress = "127.0.0.1",
                    Port = port,
                    Password = password ?? ""
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
