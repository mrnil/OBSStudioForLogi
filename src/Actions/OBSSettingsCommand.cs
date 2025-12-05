namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class OBSSettingsCommand : PluginDynamicCommand
    {
        public OBSSettingsCommand()
            : base(displayName: "OBS Settings", description: "Configure OBS connection", groupName: "OBS")
        {
        }

        protected override Boolean OnLoad()
        {
            this.AddParameter("IpAddress", "IP Address", "OBS Settings");
            this.AddParameter("Port", "Port", "OBS Settings");
            this.AddParameter("Password", "Password", "OBS Settings");
            return true;
        }

        protected override void RunCommand(String actionParameter)
        {
        }
    }
}
