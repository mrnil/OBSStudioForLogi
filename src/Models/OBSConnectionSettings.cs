namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class OBSConnectionSettings
    {
        private String _ipAddress = "127.0.0.1";
        
        public String IpAddress 
        { 
            get => this._ipAddress;
            set
            {
                if (value != "127.0.0.1" && value != "::1")
                {
                    PluginLog.Warning($"Only localhost connections allowed, rejecting: {value}");
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

        public String GetWebSocketUrl() => $"ws://{this.IpAddress}:{this.Port}";
    }
}
