namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ProfilesDynamicFolder : PluginDynamicFolder
    {
        private const Int16 PROFILE_UNSELECTED = 0;
        private const Int16 PROFILE_SELECTED = 1;

        public static ProfilesDynamicFolder Instance { get; private set; }

        private String[] _profiles = new String[0];
        private String _currentProfile = String.Empty;

        public ProfilesDynamicFolder()
        {
            Instance = this;
            this.DisplayName = "OBS Profiles";
            this.GroupName = "4. Profiles";
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
            PluginLog.Info($"ProfilesDynamicFolder updated with {this._profiles.Length} profiles, current: '{this._currentProfile}'");
            this.ButtonActionNamesChanged();
        }

        public void OnCurrentProfileChanged(String profileName)
        {
            this._currentProfile = profileName ?? String.Empty;
            this.ButtonActionNamesChanged();
        }

        public void OnDisconnected()
        {
            this._profiles = new String[0];
            this._currentProfile = String.Empty;
            this.ButtonActionNamesChanged();
        }

        public override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return null;
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var isSelected = actionParameter == this._currentProfile;
            
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                bitmapBuilder.Clear(isSelected ? BitmapColor.White : BitmapColor.Black);
                bitmapBuilder.DrawText(actionParameter, isSelected ? BitmapColor.Black : BitmapColor.White, imageSize == PluginImageSize.Width90 ? 12 : 9);
                return bitmapBuilder.ToImage();
            }
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.SwitchProfile(actionParameter);
        }
    }
}
