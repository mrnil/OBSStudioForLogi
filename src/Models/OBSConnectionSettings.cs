namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Net;

    public class OBSConnectionSettings
    {
        private String _ipAddress = "127.0.0.1";
        
        public String IpAddress 
        { 
            get => this._ipAddress;
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    PluginLog.Warning("IP address cannot be empty, using default localhost");
                    this._ipAddress = "127.0.0.1";
                }
                else if (!IPAddress.TryParse(value, out _))
                {
                    PluginLog.Warning($"Invalid IP address format: {value}, using default localhost");
                    this._ipAddress = "127.0.0.1";
                }
                else
                {
                    this._ipAddress = value;
                }
            }
        }
        
        public Int32 Port { get; set; } = 4455;
        public String Password { get; set; } = "";

        public Boolean IsLocalhost => this._ipAddress == "127.0.0.1" || this._ipAddress == "::1";

        public String GetWebSocketUrl() => $"ws://{this.IpAddress}:{this.Port}";
    }
}
