namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // Source lists are only loaded while this folder is open (Activate/Deactivate), and it shows
    // loading tiles until the first list arrives.
    public class SourcesDynamicFolder : PluginDynamicFolder, IObsCommand, ISourceVisibilityAwareCommand, ISceneSourcesAwareCommand
    {
        public static SourcesDynamicFolder Instance { get; private set; }

        private const String ViewerName = nameof(SourcesDynamicFolder);

        private readonly LoadingTiles _loading = new LoadingTiles("sources");
        private String[] _sources = new String[0];
        private String _currentScene = String.Empty;
        private volatile Boolean _isOpen;

        public SourcesDynamicFolder()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.DisplayName = "Scene Sources";
            this.GroupName = "7. Scenes";
            this.Description = "Folder of sources in the current scene";
        }

        public override PluginDynamicFolderNavigation GetNavigationArea(DeviceType _)
        {
            return PluginDynamicFolderNavigation.ButtonArea;
        }

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType)
        {
            if (this._loading.IsLoading)
            {
                PluginLog.Debug($"SourcesDynamicFolder showing {LoadingTiles.TileCount} loading tiles on {deviceType}");
                return this._loading.ActionParameters.Select(this.CreateCommandName);
            }

            PluginLog.Debug($"SourcesDynamicFolder showing {this._sources.Length} sources on {deviceType}");
            return this._sources.Select(source => this.CreateCommandName(source));
        }

        public override Boolean Activate()
        {
            // Loading is set before the load starts, since the load can finish (and clear it)
            // before OpenSceneSources returns.
            this._isOpen = true;
            this._loading.Begin();
            Boolean loading = OBSStudioForLogiPlugin.Instance?.OpenSceneSources(ViewerName) ?? false;
            PluginLog.Info($"SourcesDynamicFolder opened - {(loading ? "loading sources" : "not connected, waiting for OBS")}");

            if (!loading)
            {
                this._loading.End();
            }

            this.ButtonActionNamesChanged();
            return true;
        }

        public override Boolean Deactivate()
        {
            PluginLog.Info("SourcesDynamicFolder closed - releasing source lists");
            this._isOpen = false;
            OBSStudioForLogiPlugin.Instance?.CloseSceneSources(ViewerName);
            this._loading.End();
            this._sources = new String[0];
            this._currentScene = String.Empty;
            this.ButtonActionNamesChanged();
            return true;
        }

        public void UpdateSources(String sceneName, String[] sources)
        {
            this._currentScene = sceneName ?? String.Empty;
            this._sources = sources ?? new String[0];
            this._loading.End();
            PluginLog.Debug($"SourcesDynamicFolder updated with {this._sources.Length} sources for scene '{this._currentScene}'");
            this.ButtonActionNamesChanged();
        }

        public void OnDisconnected()
        {
            this._loading.End();
            this._sources = new String[0];
            this._currentScene = String.Empty;
            this.ButtonActionNamesChanged();
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            if (LoadingTiles.IsTile(actionParameter))
            {
                return this._loading.Render(actionParameter, imageSize);
            }

            Boolean isVisible = OBSStudioForLogiPlugin.Instance?.GetSourceVisibility(this._currentScene, actionParameter) ?? false;
            return ButtonImageHelper.Icon(isVisible ? "SourceVisibilityOn.svg" : "SourceVisibilityOff.svg");
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter) || LoadingTiles.IsTile(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.ToggleSourceVisibility(this._currentScene, actionParameter);
        }

        // The initial state load after connecting loads the current scene's sources, since this
        // folder is registered as a viewer while it is open.
        public void OnConnected()
        {
            if (this._isOpen)
            {
                PluginLog.Debug("SourcesDynamicFolder open when OBS connected - loading sources");
                this._loading.Begin();
            }

            this.ButtonActionNamesChanged();
        }

        // Lists loaded for the other scene source folder arrive here too; a closed folder has no
        // use for them.
        public void OnSceneSourcesChanged(String sceneName, String[] sources, String[] audioSources)
        {
            if (!this._isOpen)
            {
                return;
            }

            this.UpdateSources(sceneName, sources);
        }

        public void OnSourceVisibilityChanged(String sceneName, String sourceName)
        {
            if (sceneName != this._currentScene)
                return;

            this.CommandImageChanged(sourceName);
        }
    }
}
