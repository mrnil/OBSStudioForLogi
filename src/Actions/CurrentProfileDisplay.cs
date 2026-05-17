namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class CurrentProfileDisplay : PluginDynamicCommand
    {
        public static CurrentProfileDisplay Instance { get; private set; }

        private String _currentProfile = "Not Connected";
        private readonly ActionImageStore<TextImageData> imageStore;

        public CurrentProfileDisplay()
            : base(displayName: "Current Profile", description: "Shows current OBS profile", groupName: "4. Profiles")
        {
            Instance = this;
            this.imageStore = new ActionImageStore<TextImageData>(new TextImageFactory());
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
            this.ActionImageChanged("");
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isConnected = OBSStudioForLogiPlugin.Instance?.IsConnected ?? false;
            String displayText = isConnected ? this._currentProfile : "Not Connected";
            BitmapColor backgroundColor = isConnected ? new BitmapColor(57, 108, 246) : BitmapColor.Black;
            BitmapColor textColor = isConnected ? BitmapColor.White : new BitmapColor(128, 128, 128);
            
            var imageData = new TextImageData
            {
                Id = "profile_display",
                DisplayText = displayText,
                BackgroundColor = backgroundColor,
                TextColor = textColor
            };
            
            this.imageStore.UpdateImage(imageData.Id, imageData);
            
            if (this.imageStore.TryGetImage(imageData.Id, imageSize, out var image))
            {
                return image;
            }
            
            return ButtonTextRenderer.RenderText(displayText, imageSize, backgroundColor, textColor);
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }
    }
}
