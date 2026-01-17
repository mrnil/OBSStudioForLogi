namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class CurrentProfileDisplay : PluginDynamicCommand
    {
        public static CurrentProfileDisplay Instance { get; private set; }

        private String _currentProfile = "Not Connected";

        public CurrentProfileDisplay()
            : base(displayName: "Current Profile", description: "Shows current OBS profile", groupName: "1. OBS")
        {
            Instance = this;
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            if (!OBSStudioForLogiPlugin.Instance?.IsConnected ?? true)
            {
                return "Not Connected";
            }
            return this._currentProfile;
        }

        public void UpdateProfile(String profileName)
        {
            if (String.IsNullOrEmpty(profileName))
            {
                PluginLog.Warning("Cannot update profile display - profile name is empty");
                return;
            }

            PluginLog.Info($"Updating profile display to '{profileName}'");
            this._currentProfile = profileName;
            this.ActionImageChanged();
        }

        public void UpdateDisplay()
        {
            this.ActionImageChanged();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return null;
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }
    }
}
