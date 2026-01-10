namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Linq;

    public class ProfileSwitchCommand : PluginDynamicCommand
    {
        public ProfileSwitchCommand()
            : base(displayName: "Switch Profile", description: "Switch OBS profile", groupName: "Profiles")
        {
        }

        protected override Boolean OnLoad()
        {
            var profiles = OBSStudioForLogiPlugin.Instance?.GetProfileList() ?? new String[0];
            
            foreach (var profile in profiles)
            {
                this.AddParameter(profile, profile, "OBS");
            }

            return true;
        }

        protected override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.SwitchProfile(actionParameter);
        }
    }
}
