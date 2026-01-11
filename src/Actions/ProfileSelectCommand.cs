namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class ProfileSelectCommand : PluginMultistateDynamicCommand
    {
        private const Int16 PROFILE_UNSELECTED = 0;
        private const Int16 PROFILE_SELECTED = 1;

        public static ProfileSelectCommand Instance { get; private set; }

        public ProfileSelectCommand()
        {
            Instance = this;
            this.Description = "Switches to a specific profile in OBS Studio";
            this.GroupName = "4. Profiles";
            this.AddState("", "Profile unselected");
            this.AddState("", "Profile selected");
        }

        protected override Boolean OnLoad()
        {
            this.IsEnabled = false;
            this.ResetParameters(false);
            return true;
        }

        protected override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.SwitchProfile(actionParameter);
        }

        private void ResetParameters(Boolean readContent)
        {
            this.RemoveAllParameters();

            if (readContent)
            {
                var profiles = OBSStudioForLogiPlugin.Instance?.GetProfileList() ?? new String[0];
                var currentProfile = OBSStudioForLogiPlugin.Instance?.CurrentProfile ?? String.Empty;

                PluginLog.Info($"Adding {profiles.Length} profiles");

                foreach (var profile in profiles)
                {
                    this.AddParameter(profile, profile, this.GroupName).Description = $"Switch to profile \"{profile}\"";
                    this.SetCurrentState(profile, profile == currentProfile ? PROFILE_SELECTED : PROFILE_UNSELECTED);
                }
            }

            this.ParametersChanged();
            this.ActionImageChanged();
        }

        public void OnProfilesChanged()
        {
            this.ResetParameters(true);
        }

        public void OnCurrentProfileChanged(String oldProfile, String newProfile)
        {
            if (!String.IsNullOrEmpty(oldProfile))
            {
                this.SetCurrentState(oldProfile, PROFILE_UNSELECTED);
            }

            if (!String.IsNullOrEmpty(newProfile))
            {
                this.SetCurrentState(newProfile, PROFILE_SELECTED);
            }

            this.ActionImageChanged();
        }

        public void OnConnected()
        {
            this.IsEnabled = true;
            this.ResetParameters(true);
        }

        public void OnDisconnected()
        {
            this.IsEnabled = false;
            this.ResetParameters(false);
        }

        protected override BitmapImage GetCommandImage(String actionParameter, Int32 stateIndex, PluginImageSize imageSize)
        {
            // Return null to use default icon for now
            return null;
        }
    }
}
