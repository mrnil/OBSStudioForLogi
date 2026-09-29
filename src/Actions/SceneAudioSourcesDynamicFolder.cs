namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // Audio source lists are only loaded while this folder is open (Activate/Deactivate), and it
    // shows loading tiles until the first list arrives.
    public class SceneAudioSourcesDynamicFolder : AudioInputDynamicFolderBase, IObsCommand, ISceneSourcesAwareCommand
    {
        public static SceneAudioSourcesDynamicFolder Instance { get; private set; }

        private const String ViewerName = nameof(SceneAudioSourcesDynamicFolder);

        private readonly LoadingTiles _loading = new LoadingTiles("audio sources");
        private String _currentScene = String.Empty;
        private volatile Boolean _isOpen;

        public SceneAudioSourcesDynamicFolder()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.DisplayName = "Mixer for Scene Audio";
            this.GroupName = "8. Audio";
            this.Description = "Folder of scene audio inputs with controls for mute, monitor and volume [Volume - Loupedeck only]";
        }

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType)
        {
            if (this._loading.IsLoading)
            {
                PluginLog.Debug($"SceneAudioSourcesDynamicFolder showing {LoadingTiles.TileCount} loading tiles on {deviceType}");
                return this._loading.ActionParameters.Select(this.CreateCommandName);
            }

            PluginLog.Debug($"SceneAudioSourcesDynamicFolder showing {this.AudioInputs.Length} audio sources on {deviceType}");
            return this.AudioInputs.Select(this.CreateCommandName);
        }

        public override Boolean Activate()
        {
            // Loading is set before the load starts, since the load can finish (and clear it)
            // before OpenSceneSources returns.
            this._isOpen = true;
            this._loading.Begin();
            Boolean loading = OBSStudioForLogiPlugin.Instance?.OpenSceneSources(ViewerName) ?? false;
            PluginLog.Info($"SceneAudioSourcesDynamicFolder opened - {(loading ? "loading audio sources" : "not connected, waiting for OBS")}");

            if (!loading)
            {
                this._loading.End();
            }

            this.ButtonActionNamesChanged();
            return true;
        }

        // The audio selection is left alone: the other audio folders share it.
        public override Boolean Deactivate()
        {
            PluginLog.Info("SceneAudioSourcesDynamicFolder closed - releasing audio source lists");
            this._isOpen = false;
            OBSStudioForLogiPlugin.Instance?.CloseSceneSources(ViewerName);
            this._loading.End();
            this.AudioInputs = new String[0];
            this._currentScene = String.Empty;
            this.ButtonActionNamesChanged();
            return true;
        }

        public void UpdateAudioSources(String sceneName, String[] audioSources)
        {
            var previousSelection = AudioSelectionState.SelectedInput;

            this._currentScene = sceneName ?? String.Empty;
            this.AudioInputs = audioSources ?? new String[0];
            this._loading.End();

            // Deselect if selected input is no longer in the list
            if (!String.IsNullOrEmpty(previousSelection) && !this.AudioInputs.Contains(previousSelection))
            {
                AudioSelectionState.Deselect();
            }

            PluginLog.Debug($"SceneAudioSourcesDynamicFolder updated with {this.AudioInputs.Length} audio sources for scene '{this._currentScene}'");
            this.ButtonActionNamesChanged();
        }

        // Lists loaded for the other scene source folder arrive here too. A closed folder ignores
        // them - applying them would also clear an audio selection made in another audio folder.
        public void OnSceneSourcesChanged(String sceneName, String[] sources, String[] audioSources)
        {
            if (!this._isOpen)
            {
                return;
            }

            this.UpdateAudioSources(sceneName, audioSources);
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            if (LoadingTiles.IsTile(actionParameter))
            {
                return this._loading.Render(actionParameter, imageSize);
            }

            return base.GetCommandImage(actionParameter, imageSize);
        }

        public override void RunCommand(String actionParameter)
        {
            if (LoadingTiles.IsTile(actionParameter))
            {
                return;
            }

            base.RunCommand(actionParameter);
        }

        // The initial state load after connecting loads the current scene's audio sources, since
        // this folder is registered as a viewer while it is open.
        public override void OnConnected()
        {
            if (this._isOpen)
            {
                PluginLog.Debug("SceneAudioSourcesDynamicFolder open when OBS connected - loading audio sources");
                this._loading.Begin();
            }

            this.ButtonActionNamesChanged();
        }

        public override void OnDisconnected()
        {
            this._loading.End();
            this.AudioInputs = new String[0];
            this._currentScene = String.Empty;
            AudioSelectionState.Deselect();
            this.ButtonActionNamesChanged();
        }
    }
}
