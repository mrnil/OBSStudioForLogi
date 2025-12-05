namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class OBSConnectionSettings
    {
        public String IpAddress { get; set; } = "127.0.0.1";
        public Int32 Port { get; set; } = 4455;
        public String Password { get; set; } = "";

        public String GetWebSocketUrl() => $"ws://{this.IpAddress}:{this.Port}";
    }
}
