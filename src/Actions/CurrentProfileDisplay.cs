namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class CurrentProfileDisplay : PluginDynamicCommand, IObsCommand, IProfileAwareCommand
    {
        public static CurrentProfileDisplay Instance { get; private set; }

        private String _currentProfile = "Not Connected";

        public CurrentProfileDisplay()
            : base(displayName: "Current Profile", description: "Shows current OBS profile", groupName: "6. Profiles")
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.AddParameter("", " ", groupName: "6. Profiles");
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return String.Empty;
        }

        public void OnProfileChanged(String oldProfile, String newProfile)
        {
            this.UpdateProfile(newProfile);
        }

        public void OnConnected()
        {
            this.ActionImageChanged("");
        }

        public void OnDisconnected()
        {
            this._currentProfile = "Not Connected";
            this.ActionImageChanged("");
        }

        private void UpdateProfile(String profileName)
        {
            if (String.IsNullOrEmpty(profileName))
            {
                PluginLog.Warning("Cannot update profile display - profile name is empty");
                return;
            }

            PluginLog.Info($"Updating profile display to '{profileName}'");
            this._currentProfile = profileName;
            this.ActionImageChanged("");
        }

        public void UpdateDisplay()
        {
            this.ActionImageChanged("");
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isConnected = OBSStudioForLogiPlugin.Instance?.IsConnected ?? false;
            String displayText = isConnected ? this._currentProfile : "Not Connected";
            BitmapColor backgroundColor = isConnected ? new BitmapColor(57, 108, 246) : BitmapColor.Black;
            BitmapColor textColor = isConnected ? BitmapColor.White : new BitmapColor(128, 128, 128);
            
            return ButtonImageHelper.Text(displayText, imageSize, backgroundColor, textColor);
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }
    }
}
