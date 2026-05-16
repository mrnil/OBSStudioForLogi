namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class CurrentProfileDisplay : PluginDynamicCommand
    {
        public static CurrentProfileDisplay Instance { get; private set; }

        private String _currentProfile = "Not Connected";

        public CurrentProfileDisplay()
            : base(displayName: "Current Profile", description: "Shows current OBS profile", groupName: "4. Profiles")
        {
            Instance = this;
            this.AddParameter("", " ", groupName: "4. Profiles");
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return null;
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
            this.ActionImageChanged("");
        }

        public void UpdateDisplay()
        {
            PluginLog.Info("CurrentProfileDisplay.UpdateDisplay called");
            this.ActionImageChanged("");
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            PluginLog.Info($"CurrentProfileDisplay.GetCommandImage called - actionParameter: '{actionParameter}', imageSize: {imageSize}, _currentProfile: '{this._currentProfile}'");
            
            if (!OBSStudioForLogiPlugin.Instance?.IsConnected ?? true)
            {
                PluginLog.Info("Rendering 'Not Connected' state");
                return ButtonTextRenderer.RenderText(
                    "Not Connected",
                    imageSize,
                    BitmapColor.Black,
                    new BitmapColor(128, 128, 128));
            }
            
            PluginLog.Info($"Rendering profile: '{this._currentProfile}'");
            return ButtonTextRenderer.RenderText(
                this._currentProfile,
                imageSize,
                new BitmapColor(57, 108, 246),
                BitmapColor.White);
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }
    }
}
