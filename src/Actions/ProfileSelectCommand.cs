namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class ProfileSelectCommand : PluginMultistateDynamicCommand, IObsCommand, IProfileAwareCommand, IProfilesListAwareCommand
    {
        private const Int16 PROFILE_UNSELECTED = 0;
        private const Int16 PROFILE_SELECTED = 1;

        public static ProfileSelectCommand Instance { get; private set; }

        public ProfileSelectCommand()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.Description = "Switches to a specific profile in OBS Studio";
            this.GroupName = "6. Profiles###Available Profiles";
            this.AddState("", "Profile unselected");
            this.AddState("", "Profile selected");
        }

        protected override Boolean OnLoad()
        {
            this.IsEnabled = false;
            this.ResetParameters(new String[0], String.Empty);
            return true;
        }

        protected override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.SwitchProfile(actionParameter);
        }

        private void ResetParameters(String[] profiles, String currentProfile)
        {
            this.RemoveAllParameters();

            PluginLog.Debug($"Adding {profiles.Length} profiles");

            foreach (String profile in profiles)
            {
                this.AddParameter(profile, profile, this.GroupName).Description = $"Switch to profile \"{profile}\"";
                this.SetCurrentState(profile, profile == currentProfile ? PROFILE_SELECTED : PROFILE_UNSELECTED);
            }

            this.ParametersChanged();
            this.ActionImageChanged();
        }

        // OBSWebSocketManager loads the profile list once per connection (and on profile changes)
        // and pushes it here, rather than every profile-aware command querying OBS itself.
        public void OnProfilesChanged(String[] profiles, String currentProfile)
        {
            this.ResetParameters(profiles ?? new String[0], currentProfile ?? String.Empty);
        }

        public void OnProfileChanged(String oldProfile, String newProfile)
        {
            this.OnCurrentProfileChanged(oldProfile, newProfile);
        }

        private void OnCurrentProfileChanged(String oldProfile, String newProfile)
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
        }

        public void OnDisconnected()
        {
            this.IsEnabled = false;
            this.ResetParameters(new String[0], String.Empty);
        }

        protected override BitmapImage GetCommandImage(String actionParameter, Int32 stateIndex, PluginImageSize imageSize)
        {
            Boolean isConnected = OBSStudioForLogiPlugin.Instance?.IsConnected ?? false;
            
            if (!isConnected)
            {
                return ButtonImageHelper.Icon("ProfileUnselected.svg");
            }
            
            Boolean isSelected = stateIndex == PROFILE_SELECTED;
            return ButtonImageHelper.Icon(isSelected ? "ProfileSelected.svg" : "ProfileUnselected.svg");
        }
    }
}
