namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ProfilesDynamicFolder : PluginDynamicFolder, IObsCommand, IProfileAwareCommand, IProfilesListAwareCommand
    {
        private const Int16 PROFILE_UNSELECTED = 0;
        private const Int16 PROFILE_SELECTED = 1;

        public static ProfilesDynamicFolder Instance { get; private set; }

        private String[] _profiles = new String[0];
        private String _currentProfile = String.Empty;

        public ProfilesDynamicFolder()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.DisplayName = "OBS Profiles";
            this.GroupName = "6. Profiles###Available Profiles";
            this.Description = "Folder of available OBS profiles";
        }

        public override PluginDynamicFolderNavigation GetNavigationArea(DeviceType _)
        {
            return PluginDynamicFolderNavigation.ButtonArea;
        }

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType)
        {
            return this._profiles.Select(profile => this.CreateCommandName(profile));
        }

        public void UpdateProfiles(String[] profiles, String currentProfile)
        {
            this._profiles = profiles ?? new String[0];
            this._currentProfile = currentProfile ?? String.Empty;
            PluginLog.Debug($"ProfilesDynamicFolder updated with {this._profiles.Length} profiles, current: '{this._currentProfile}'");
            this.ButtonActionNamesChanged();
        }

        public void OnProfileChanged(String oldProfile, String newProfile)
        {
            this.OnCurrentProfileChanged(newProfile);
        }

        public void OnProfilesChanged(String[] profiles, String currentProfile)
        {
            this.UpdateProfiles(profiles, currentProfile);
        }

        public void OnConnected()
        {
            this.ButtonActionNamesChanged();
        }

        private void OnCurrentProfileChanged(String profileName)
        {
            var oldProfile = this._currentProfile;
            this._currentProfile = profileName ?? String.Empty;
            
            if (!String.IsNullOrEmpty(oldProfile) && oldProfile != this._currentProfile)
            {
                this.CommandImageChanged(oldProfile);
            }
            
            if (!String.IsNullOrEmpty(this._currentProfile))
            {
                this.CommandImageChanged(this._currentProfile);
            }
        }

        public void OnDisconnected()
        {
            this._profiles = new String[0];
            this._currentProfile = String.Empty;
            this.ButtonActionNamesChanged();
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isSelected = actionParameter == this._currentProfile;
            return ButtonImageHelper.Icon(isSelected ? "ProfileSelected.svg" : "ProfileUnselected.svg");
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.SwitchProfile(actionParameter);
        }
    }
}
