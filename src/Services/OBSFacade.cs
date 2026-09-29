namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    public class OBSFacade
    {
        private readonly OBSWebSocketManager _obsManager;
        private readonly SceneSourcesLoader _sceneSourcesLoader;

        public OBSFacade(OBSWebSocketManager obsManager)
            : this(obsManager, action => Task.Run(action))
        {
        }

        public OBSFacade(OBSWebSocketManager obsManager, Action<Action> runInBackground)
        {
            this._obsManager = obsManager;
            this._sceneSourcesLoader = new SceneSourcesLoader(this.FetchSceneSources, () => this.IsConnected, runInBackground);
        }

        public Boolean IsConnected => this._obsManager?.IsConnected ?? false;
        public Boolean IsRecording => this._obsManager?.IsRecording ?? false;
        public Boolean IsRecordingPaused => this._obsManager?.Actions.IsRecordingPaused ?? false;
        public Boolean IsStreaming => this._obsManager?.IsStreaming ?? false;
        public Boolean IsVirtualCameraActive => this._obsManager?.Actions.IsVirtualCameraActive ?? false;
        public Boolean IsReplayBufferActive => this._obsManager?.Actions.IsReplayBufferActive ?? false;
        public Boolean IsStudioModeEnabled => this._obsManager?.Actions.IsStudioModeEnabled ?? false;

        public String CurrentProfile => this._obsManager?.Actions.CurrentProfile ?? String.Empty;
        public String CurrentSceneCollection => this._obsManager?.Actions.CurrentSceneCollection ?? String.Empty;
        public String CurrentScene => this._obsManager?.Actions.CurrentScene ?? String.Empty;

        public String[] GetProfileList()
        {
            return this._obsManager?.Actions.GetProfileList() ?? new String[0];
        }

        public String[] GetSceneCollectionList()
        {
            return this._obsManager?.Actions.GetSceneCollectionList() ?? new String[0];
        }

        public String[] GetSceneList()
        {
            return this._obsManager?.Actions.GetSceneList() ?? new String[0];
        }

        public String[] GetInputList()
        {
            return this._obsManager?.Actions.GetInputList() ?? new String[0];
        }

        // Audio state getters are called from button rendering, so they read the cache and never
        // wait on OBS - see AudioStateCache.
        public Boolean GetInputMute(String inputName)
        {
            return this._obsManager?.AudioState.Get(inputName).IsMuted ?? false;
        }

        public Single GetInputVolume(String inputName)
        {
            return this._obsManager?.AudioState.Get(inputName).VolumeMul ?? 1.0f;
        }

        // Also called from button rendering - reads the cache, never waits on OBS.
        public Boolean GetSourceVisibility(String sceneName, String sourceName)
        {
            if (String.IsNullOrEmpty(sceneName) || String.IsNullOrEmpty(sourceName))
            {
                return false;
            }

            return this._obsManager?.SourceVisibility.Get((sceneName, sourceName)) ?? false;
        }

        public void SwitchScene(String sceneName)
        {
            if (!this._obsManager.IsConnected)
            {
                PluginLog.Warning($"Cannot switch to scene '{sceneName}' - not connected to OBS");
                return;
            }

            this._obsManager.Actions.SetCurrentScene(sceneName);
        }

        public void SwitchProfile(String profileName)
        {
            if (!this._obsManager.IsConnected)
            {
                PluginLog.Warning($"Cannot switch to profile '{profileName}' - not connected to OBS");
                return;
            }

            this._obsManager.Actions.SetCurrentProfile(profileName);
        }

        public void SwitchSceneCollection(String sceneCollectionName)
        {
            if (!this._obsManager.IsConnected)
            {
                PluginLog.Warning($"Cannot switch to scene collection '{sceneCollectionName}' - not connected to OBS");
                return;
            }

            this._obsManager.Actions.SetCurrentSceneCollection(sceneCollectionName);
        }

        public void ToggleRecording()
        {
            this._obsManager?.Actions.ToggleRecording();
        }

        public void StartRecording()
        {
            this._obsManager?.Actions.StartRecording();
        }

        public void StopRecording()
        {
            this._obsManager?.Actions.StopRecording();
        }

        public void ToggleRecordingPause()
        {
            this._obsManager?.Actions.ToggleRecordingPause();
        }

        public void ToggleStreaming()
        {
            this._obsManager?.Actions.ToggleStreaming();
        }

        public void StartStreaming()
        {
            this._obsManager?.Actions.StartStreaming();
        }

        public void StopStreaming()
        {
            this._obsManager?.Actions.StopStreaming();
        }

        public void ToggleVirtualCamera()
        {
            this._obsManager?.Actions.ToggleVirtualCamera();
        }

        public void StartVirtualCamera()
        {
            this._obsManager?.Actions.StartVirtualCamera();
        }

        public void StopVirtualCamera()
        {
            this._obsManager?.Actions.StopVirtualCamera();
        }

        public void ToggleReplayBuffer()
        {
            this._obsManager?.Actions.ToggleReplayBuffer();
        }

        public void StartReplayBuffer()
        {
            this._obsManager?.Actions.StartReplayBuffer();
        }

        public void StopReplayBuffer()
        {
            this._obsManager?.Actions.StopReplayBuffer();
        }

        public void SaveReplayBuffer()
        {
            this._obsManager?.Actions.SaveReplayBuffer();
        }

        public void ToggleInputMute(String inputName)
        {
            this._obsManager?.Actions.ToggleInputMute(inputName);
        }

        public void SetInputVolume(String inputName, Single volumeMul)
        {
            if (!this._obsManager.IsConnected)
            {
                PluginLog.Warning($"Cannot set volume for '{inputName}' - not connected to OBS");
                return;
            }

            // Record the target right away so the next dial tick builds on it instead of on the
            // value from before this change, which OBS only confirms with InputVolumeChanged later.
            this._obsManager.AudioState.SetVolume(inputName, volumeMul);
            this._obsManager.Actions.SetInputVolume(inputName, volumeMul);
        }

        public String GetInputAudioMonitorType(String inputName)
        {
            return this._obsManager?.AudioState.Get(inputName).MonitorType ?? Models.AudioInputState.MonitorTypeNone;
        }

        public void CycleInputAudioMonitorType(String inputName)
        {
            if (!this._obsManager.IsConnected)
            {
                PluginLog.Warning($"Cannot cycle audio monitoring for '{inputName}' - not connected to OBS");
                return;
            }

            this._obsManager.Actions.CycleInputAudioMonitorType(inputName);
        }

        public void SubscribeToVolumeMeters(String owner)
        {
            this._obsManager?.SubscribeToVolumeMeters(owner);
        }

        public void UnsubscribeFromVolumeMeters(String owner)
        {
            this._obsManager?.UnsubscribeFromVolumeMeters(owner);
        }

        public Models.AudioMeterLevels GetAudioMeterLevels(String inputName)
        {
            return this._obsManager?.AudioMeters.GetLevels(inputName) ?? Models.AudioMeterLevels.Empty;
        }

        public String[] GetLiveAudioMeterInputs()
        {
            return this._obsManager?.AudioMeters.GetLiveInputs() ?? new String[0];
        }

        public void ToggleSourceVisibility(String sceneName, String sourceName)
        {
            this._obsManager?.Actions.ToggleSourceVisibility(sceneName, sourceName);
        }

        public void ToggleStudioMode()
        {
            this._obsManager?.Actions.ToggleStudioMode();
        }

        public void TriggerStudioModeTransition()
        {
            this._obsManager?.Actions.TriggerStudioModeTransition();
        }

        public void SaveScreenshot(String path)
        {
            this._obsManager?.Actions.SaveScreenshot(path);
        }

        public Models.OBSStats GetStats()
        {
            try
            {
                return this._obsManager?.Actions.GetStats() ?? Models.OBSStats.Empty;
            }
            catch (Exception ex)
            {
                PluginLog.Warning($"Failed to get stats: {ex.Message}");
                return Models.OBSStats.Empty;
            }
        }

        public Models.OBSStreamStats GetStreamStatus()
        {
            try
            {
                return this._obsManager?.Actions.GetStreamStatus() ?? Models.OBSStreamStats.Empty;
            }
            catch (Exception ex)
            {
                PluginLog.Warning($"Failed to get stream status: {ex.Message}");
                return Models.OBSStreamStats.Empty;
            }
        }

        // Called from button rendering - reads the cache, never waits on OBS.
        public String GetMediaInputStatus(String inputName)
        {
            if (String.IsNullOrEmpty(inputName))
            {
                return MediaInputStates.None;
            }

            return this._obsManager?.MediaState.Get(inputName) ?? MediaInputStates.None;
        }

        public void ToggleMediaInputPlayback(String inputName)
        {
            this._obsManager?.Actions.ToggleMediaInputPlayback(inputName);
        }

        public void TriggerMediaInputAction(String inputName, String mediaAction)
        {
            this._obsManager?.Actions.TriggerMediaInputAction(inputName, mediaAction);
        }

        public String[] GetMediaInputList()
        {
            return this._obsManager?.Actions.GetMediaInputList() ?? new String[0];
        }

        // Returns straight away: the lists are fetched in the background and only the latest
        // request's result reaches the callback - see SceneSourcesLoader.
        public void UpdateSourcesForScene(String sceneName, Action<String, String[], String[]> callback)
        {
            this._sceneSourcesLoader.Load(sceneName, callback);
        }

        private (String[] Sources, String[] AudioSources) FetchSceneSources(String sceneName)
        {
            String[] sources = this._obsManager?.Actions.GetSceneItemList(sceneName) ?? new String[0];

            String[] audioSourcesInScene = this._obsManager?.Actions.GetAudioSourcesInScene(sceneName) ?? new String[0];
            String[] audioInputsNotInAnyScene = this._obsManager?.Actions.GetAudioInputsNotInAnyScene() ?? new String[0];
            String[] allSceneAudioSources = audioSourcesInScene.Concat(audioInputsNotInAnyScene).ToArray();

            return (sources, allSceneAudioSources);
        }
    }
}
