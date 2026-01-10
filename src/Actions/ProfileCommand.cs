namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class ProfileCommand : PluginDynamicCommand
    {
        public ProfileCommand()
            : base(displayName: "Profile", description: "Switch to OBS profile", groupName: "Profiles")
        {
        }

        protected override Boolean OnLoad()
        {
            this.LoadProfiles();
            return true;
        }

        private void LoadProfiles()
        {
            this.RemoveAllParameters();
            
            var profiles = OBSStudioForLogiPlugin.Instance?.GetProfileList() ?? new String[0];
            
            foreach (var profile in profiles)
            {
                this.AddParameter(profile, profile, "Profiles");
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.SwitchProfile(actionParameter);
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return null;

            var currentProfile = OBSStudioForLogiPlugin.Instance?.CurrentProfile ?? String.Empty;
            var isActive = currentProfile == actionParameter;

            // Return different visual state based on whether this profile is active
            // For now, return null to use default icon
            return null;
        }
    }
}
