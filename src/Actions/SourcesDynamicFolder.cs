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
        private Dictionary<Int32, String> _sceneItemIdToSourceName = new Dictionary<Int32, String>();

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
            this.BuildSceneItemIdMapping();
            this.ButtonActionNamesChanged();
        }

        public void OnDisconnected()
        {
            this._sources = new String[0];
            this._currentScene = String.Empty;
            this._sceneItemIdToSourceName.Clear();
            this.ButtonActionNamesChanged();
        }

        public override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return String.Empty;
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var isVisible = OBSStudioForLogiPlugin.Instance?.GetSourceVisibility(this._currentScene, actionParameter) ?? false;
            var iconName = isVisible ? "SourceVisibilityOn.svg" : "SourceVisibilityOff.svg";
            var iconPath = $"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconName}";
            var textColor = isVisible ? BitmapColor.White : new BitmapColor(128, 128, 128);
            
            return ButtonTextRenderer.RenderIconWithText(iconPath, actionParameter, imageSize, textColor);
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.ToggleSourceVisibility(this._currentScene, actionParameter);
        }

        private void BuildSceneItemIdMapping()
        {
            this._sceneItemIdToSourceName.Clear();

            if (String.IsNullOrEmpty(this._currentScene))
                return;

            var sceneItems = OBSStudioForLogiPlugin.Instance?.GetSceneItemListWithIds(this._currentScene);
            if (sceneItems != null)
            {
                foreach (var item in sceneItems)
                {
                    this._sceneItemIdToSourceName[item.ItemId] = item.SourceName;
                }
            }
        }

        public void OnSceneItemEnableStateChanged(String sceneName, Int32 sceneItemId)
        {
            if (sceneName != this._currentScene)
                return;

            if (this._sceneItemIdToSourceName.TryGetValue(sceneItemId, out var sourceName))
            {
                this.CommandImageChanged(sourceName);
            }
        }
    }
}
