namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class CurrentProfileDisplay : PluginDynamicCommand
    {
        public static CurrentProfileDisplay Instance { get; private set; }

        private String _currentProfile = "Not Connected";

        public CurrentProfileDisplay()
            : base("Current Profile", "Shows current OBS profile", "OBS")
        {
            Instance = this;
        }

        public void UpdateProfile(String profileName)
        {
            this._currentProfile = String.IsNullOrEmpty(profileName) ? "Not Connected" : profileName;
            this.ActionImageChanged();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                bitmapBuilder.Clear(BitmapColor.Black);
                bitmapBuilder.DrawText(this._currentProfile, BitmapColor.White, imageSize == PluginImageSize.Width90 ? 12 : 9);
                return bitmapBuilder.ToImage();
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }
    }
}
