namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SourcesDynamicFolder : PluginDynamicFolder
    {
        public static SourcesDynamicFolder Instance { get; private set; }

        private String[] _sources = new String[0];
        private String _currentScene = String.Empty;

        public SourcesDynamicFolder()
        {
            Instance = this;
            this.DisplayName = "Scene Sources";
            this.GroupName = "5. Scenes";
            this.Description = "Folder of sources in the current scene";
        }

        public override PluginDynamicFolderNavigation GetNavigationArea(DeviceType _)
        {
            return PluginDynamicFolderNavigation.ButtonArea;
        }

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType)
        {
            return this._sources.Select(source => this.CreateCommandName(source));
        }

        public void UpdateSources(String sceneName, String[] sources)
        {
            this._currentScene = sceneName ?? String.Empty;
            this._sources = sources ?? new String[0];
            PluginLog.Info($"SourcesDynamicFolder updated with {this._sources.Length} sources for scene '{this._currentScene}'");
            this.ButtonActionNamesChanged();
        }

        public override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return actionParameter;
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var isVisible = OBSStudioForLogiPlugin.Instance?.GetSourceVisibility(this._currentScene, actionParameter) ?? false;
            var iconName = isVisible ? "SourceVisibilityOn.svg" : "SourceVisibilityOff.svg";
            var imagePath = $"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconName}";
            
            var bitmapImage = PluginResources.ReadImage(imagePath);
            if (bitmapImage == null)
            {
                using (var bitmapBuilder = new BitmapBuilder(imageSize))
                {
                    bitmapBuilder.Clear(isVisible ? BitmapColor.White : BitmapColor.Black);
                    bitmapBuilder.DrawText(actionParameter, isVisible ? BitmapColor.Black : BitmapColor.White, imageSize == PluginImageSize.Width90 ? 12 : 9);
                    return bitmapBuilder.ToImage();
                }
            }
            
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                bitmapBuilder.DrawImage(bitmapImage);
                bitmapBuilder.DrawText(actionParameter, BitmapColor.Black, imageSize == PluginImageSize.Width90 ? 12 : 9);
                return bitmapBuilder.ToImage();
            }
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.ToggleSourceVisibility(this._currentScene, actionParameter);
        }
    }
}
