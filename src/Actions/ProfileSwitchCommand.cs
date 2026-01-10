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
            this.RefreshProfiles();
            return true;
        }

        private void RefreshProfiles()
        {
            this.RemoveAllParameters();
            
            var profiles = OBSStudioForLogiPlugin.Instance?.GetProfileList() ?? new String[0];
            
            if (profiles.Length == 0)
            {
                this.AddParameter("", "No profiles available", "Profiles");
                return;
            }
            
            foreach (var profile in profiles)
            {
                this.AddParameter(profile, profile, "Profiles");
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
            {
                this.RefreshProfiles();
                return;
            }

            OBSStudioForLogiPlugin.Instance?.SwitchProfile(actionParameter);
        }
    }
}
