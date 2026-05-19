namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading.Tasks;
    using OBSWebsocketDotNet.Types;

    public class OBSActionExecutor
    {
        private readonly IOBSWebsocket _obs;
        private readonly IPluginLog _log;
        private OutputState _recordingState = OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
        private OutputState _streamingState = OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
        private OutputState _virtualCameraState = OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
        private OutputState _replayBufferState = OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
        private String _currentProfile = String.Empty;
        private String _currentSceneCollection = String.Empty;
        private String _currentScene = String.Empty;
        private Boolean _studioModeEnabled = false;

        public Boolean IsRecording => this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED 
                                    || this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_PAUSED
                                    || this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_RESUMED;
        public Boolean IsRecordingPaused { get; private set; }
        public Boolean IsRecordingChanging => this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTING 
                                            || this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_STOPPING;
        public Boolean IsStreaming => this._streamingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED;
        public Boolean IsStreamingChanging => this._streamingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTING 
                                            || this._streamingState == OutputState.OBS_WEBSOCKET_OUTPUT_STOPPING;
        public Boolean IsVirtualCameraActive => this._virtualCameraState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED;
        public Boolean IsReplayBufferActive => this._replayBufferState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED;
        public Boolean IsStudioModeEnabled => this._studioModeEnabled;
        public String CurrentProfile => this._currentProfile;
        public String CurrentSceneCollection => this._currentSceneCollection;
        public String CurrentScene => this._currentScene;

        public OBSActionExecutor(IOBSWebsocket obs, IPluginLog log)
        {
            this._obs = obs;
            this._log = log;
        }

        public void SetCurrentScene(String sceneName)
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning($"Cannot set scene '{sceneName}' - not connected");
                    return;
                }

                try
                {
                    this._log.Info($"Setting current scene to '{sceneName}'");
                    this._obs.SetCurrentProgramScene(sceneName);
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to set scene '{sceneName}': {ex.Message}");
                }
            });
        }

        public void ToggleRecording()
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot toggle recording - not connected");
                    return;
                }

                if (this.IsRecordingChanging)
                {
                    this._log.Warning("Cannot toggle recording - state change in progress");
                    return;
                }

                try
                {
                    this._log.Info("Toggling recording");
                    this._obs.ToggleRecord();
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to toggle recording: {ex.Message}");
                }
            });
        }

        public void StartRecording()
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot start recording - not connected");
                    return;
                }

                if (this.IsRecording)
                {
                    this._log.Warning("Cannot start recording - already recording");
                    return;
                }

                try
                {
                    this._log.Info("Starting recording");
                    this._obs.StartRecord();
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to start recording: {ex.Message}");
                }
            });
        }

        public void StopRecording()
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot stop recording - not connected");
                    return;
                }

                if (!this.IsRecording)
                {
                    this._log.Warning("Cannot stop recording - not recording");
                    return;
                }

                try
                {
                    this._log.Info("Stopping recording");
                    this._obs.StopRecord();
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to stop recording: {ex.Message}");
                }
            });
        }

        public void ToggleRecordingPause()
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot toggle recording pause - not connected");
                    return;
                }

                if (!this.IsRecording)
                {
                    this._log.Warning("Cannot toggle recording pause - not recording");
                    return;
                }

                try
                {
                    this._log.Info("Toggling recording pause");
                    if (this.IsRecordingPaused)
                    {
                        this._obs.ResumeRecord();
                    }
                    else
                    {
                        this._obs.PauseRecord();
                    }
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to toggle recording pause: {ex.Message}");
                }
            });
        }

        public String[] GetProfileList()
        {
            if (!this._obs.IsConnected)
            {
                this._log.Warning("Cannot get profile list - not connected");
                return new String[0];
            }

            try
            {
                this._log.Info("Getting profile list");
                return this._obs.GetProfileList();
            }
            catch (Exception ex)
            {
                this._log.Error($"Failed to get profile list: {ex.Message}");
                return new String[0];
            }
        }

        public void SetCurrentProfile(String profileName)
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning($"Cannot set profile '{profileName}' - not connected");
                    return;
                }

                if (this._currentProfile == profileName)
                {
                    this._log.Info($"Profile '{profileName}' is already active");
                    return;
                }

                try
                {
                    this._log.Info($"Setting current profile to '{profileName}'");
                    this._obs.SetCurrentProfile(profileName);
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to set profile '{profileName}': {ex.Message}");
                }
            });
        }

        public void SetCurrentProfileState(String profileName)
        {
            this._currentProfile = profileName;
        }

        public String[] GetSceneCollectionList()
        {
            if (!this._obs.IsConnected)
            {
                this._log.Warning("Cannot get scene collection list - not connected");
                return new String[0];
            }

            try
            {
                this._log.Info("Getting scene collection list");
                return this._obs.GetSceneCollectionList();
            }
            catch (Exception ex)
            {
                this._log.Error($"Failed to get scene collection list: {ex.Message}");
                return new String[0];
            }
        }

        public void SetCurrentSceneCollection(String sceneCollectionName)
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning($"Cannot set scene collection '{sceneCollectionName}' - not connected");
                    return;
                }

                if (this._currentSceneCollection == sceneCollectionName)
                {
                    this._log.Info($"Scene collection '{sceneCollectionName}' is already active");
                    return;
                }

                try
                {
                    this._log.Info($"Setting current scene collection to '{sceneCollectionName}'");
                    this._obs.SetCurrentSceneCollection(sceneCollectionName);
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to set scene collection '{sceneCollectionName}': {ex.Message}");
                }
            });
        }

        public void SetCurrentSceneCollectionState(String sceneCollectionName)
        {
            this._currentSceneCollection = sceneCollectionName;
        }

        public void SetCurrentSceneState(String sceneName)
        {
            this._currentScene = sceneName;
        }

        public String[] GetSceneList()
        {
            if (!this._obs.IsConnected)
            {
                this._log.Warning("Cannot get scene list - not connected");
                return new String[0];
            }

            try
            {
                this._log.Info("Getting scene list");
                return this._obs.GetSceneList();
            }
            catch (Exception ex)
            {
                this._log.Error($"Failed to get scene list: {ex.Message}");
                return new String[0];
            }
        }

        public void SaveScreenshot(String screenshotPath)
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot save screenshot - not connected");
                    return;
                }

                if (String.IsNullOrEmpty(this._currentScene))
                {
                    this._log.Warning("Cannot save screenshot - no current scene");
                    return;
                }

                if (String.IsNullOrEmpty(screenshotPath))
                {
                    this._log.Warning("Cannot save screenshot - no valid path");
                    return;
                }

                try
                {
                    var filename = System.IO.Path.Combine(screenshotPath, $"Screenshot-{DateTime.Now:yyyyMMddHHmmssfff}.png");
                    this._log.Info($"Saving screenshot to {filename}");
                    this._obs.SaveSourceScreenshot(this._currentScene, "png", filename, -1, -1);
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to save screenshot: {ex.Message}");
                }
            });
        }

        public void SetRecordingState(OutputState state)
        {
            this._recordingState = state;
            
            if (state == OutputState.OBS_WEBSOCKET_OUTPUT_PAUSED)
            {
                this.IsRecordingPaused = true;
            }
            else if (state == OutputState.OBS_WEBSOCKET_OUTPUT_RESUMED || state == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED)
            {
                this.IsRecordingPaused = false;
            }
            else if (state == OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED)
            {
                this.IsRecordingPaused = false;
            }
        }

        public void ToggleStreaming()
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot toggle streaming - not connected");
                    return;
                }

                if (this.IsStreamingChanging)
                {
                    this._log.Warning("Cannot toggle streaming - state change in progress");
                    return;
                }

                try
                {
                    this._log.Info("Toggling streaming");
                    this._obs.ToggleStream();
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to toggle streaming: {ex.Message}");
                }
            });
        }

        public void StartStreaming()
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot start streaming - not connected");
                    return;
                }

                if (this.IsStreaming)
                {
                    this._log.Warning("Cannot start streaming - already streaming");
                    return;
                }

                try
                {
                    this._log.Info("Starting streaming");
                    this._obs.StartStream();
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to start streaming: {ex.Message}");
                }
            });
        }

        public void StopStreaming()
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot stop streaming - not connected");
                    return;
                }

                if (!this.IsStreaming)
                {
                    this._log.Warning("Cannot stop streaming - not streaming");
                    return;
                }

                try
                {
                    this._log.Info("Stopping streaming");
                    this._obs.StopStream();
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to stop streaming: {ex.Message}");
                }
            });
        }

        public void SetStreamingState(OutputState state)
        {
            this._streamingState = state;
        }

        public void ToggleVirtualCamera()
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot toggle virtual camera - not connected");
                    return;
                }

                try
                {
                    this._log.Info("Toggling virtual camera");
                    if (this.IsVirtualCameraActive)
                    {
                        this._obs.StopVirtualCam();
                    }
                    else
                    {
                        this._obs.StartVirtualCam();
                    }
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to toggle virtual camera: {ex.Message}");
                }
            });
        }

        public void StartVirtualCamera()
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot start virtual camera - not connected");
                    return;
                }

                if (this.IsVirtualCameraActive)
                {
                    this._log.Warning("Cannot start virtual camera - already active");
                    return;
                }

                try
                {
                    this._log.Info("Starting virtual camera");
                    this._obs.StartVirtualCam();
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to start virtual camera: {ex.Message}");
                }
            });
        }

        public void StopVirtualCamera()
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot stop virtual camera - not connected");
                    return;
                }

                if (!this.IsVirtualCameraActive)
                {
                    this._log.Warning("Cannot stop virtual camera - not active");
                    return;
                }

                try
                {
                    this._log.Info("Stopping virtual camera");
                    this._obs.StopVirtualCam();
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to stop virtual camera: {ex.Message}");
                }
            });
        }

        public void SetVirtualCameraState(OutputState state)
        {
            this._virtualCameraState = state;
        }

        public void ToggleReplayBuffer()
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot toggle replay buffer - not connected");
                    return;
                }

                try
                {
                    this._log.Info("Toggling replay buffer");
                    this._obs.ToggleReplayBuffer();
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to toggle replay buffer: {ex.Message}");
                }
            });
        }

        public void StartReplayBuffer()
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot start replay buffer - not connected");
                    return;
                }

                if (this.IsReplayBufferActive)
                {
                    this._log.Warning("Cannot start replay buffer - already active");
                    return;
                }

                try
                {
                    this._log.Info("Starting replay buffer");
                    this._obs.StartReplayBuffer();
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to start replay buffer: {ex.Message}");
                }
            });
        }

        public void StopReplayBuffer()
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot stop replay buffer - not connected");
                    return;
                }

                if (!this.IsReplayBufferActive)
                {
                    this._log.Warning("Cannot stop replay buffer - not active");
                    return;
                }

                try
                {
                    this._log.Info("Stopping replay buffer");
                    this._obs.StopReplayBuffer();
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to stop replay buffer: {ex.Message}");
                }
            });
        }

        public void SaveReplayBuffer()
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot save replay buffer - not connected");
                    return;
                }

                if (!this.IsReplayBufferActive)
                {
                    this._log.Warning("Cannot save replay buffer - not active");
                    return;
                }

                try
                {
                    this._log.Info("Saving replay buffer");
                    this._obs.SaveReplayBuffer();
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to save replay buffer: {ex.Message}");
                }
            });
        }

        public void SetReplayBufferState(OutputState state)
        {
            this._replayBufferState = state;
        }

        public String[] GetSceneItemList(String sceneName)
        {
            if (!this._obs.IsConnected)
            {
                this._log.Warning($"Cannot get scene item list for '{sceneName}' - not connected");
                return new String[0];
            }

            if (String.IsNullOrEmpty(sceneName))
            {
                this._log.Warning("Cannot get scene item list - scene name is empty");
                return new String[0];
            }

            try
            {
                this._log.Info($"Getting scene item list for '{sceneName}'");
                return this._obs.GetSceneItemList(sceneName);
            }
            catch (Exception ex)
            {
                this._log.Error($"Failed to get scene item list for '{sceneName}': {ex.Message}");
                return new String[0];
            }
        }

        public Boolean GetSceneItemEnabled(String sceneName, String sourceName)
        {
            if (!this._obs.IsConnected)
            {
                this._log.Warning($"Cannot get scene item enabled state for '{sourceName}' - not connected");
                return false;
            }

            try
            {
                return this._obs.GetSceneItemEnabled(sceneName, sourceName);
            }
            catch (Exception ex)
            {
                this._log.Error($"Failed to get scene item enabled state for '{sourceName}': {ex.Message}");
                return false;
            }
        }

        public void ToggleSourceVisibility(String sceneName, String sourceName)
        {
            Task.Run(async () =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning($"Cannot toggle source visibility for '{sourceName}' - not connected");
                    return;
                }

                if (String.IsNullOrEmpty(sceneName) || String.IsNullOrEmpty(sourceName))
                {
                    this._log.Warning("Cannot toggle source visibility - scene or source name is empty");
                    return;
                }

                try
                {
                    var currentState = this._obs.GetSceneItemEnabled(sceneName, sourceName);
                    this._log.Info($"Toggling source '{sourceName}' visibility from {currentState} to {!currentState}");
                    this._obs.SetSceneItemEnabled(sceneName, sourceName, !currentState);
                    
                    await Task.Delay(100);
                    OBSStudioForLogiPlugin.Instance?.OnSourceVisibilityChanged(sceneName, sourceName);
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to toggle source visibility for '{sourceName}': {ex.Message}");
                }
            });
        }

        public String[] GetInputList()
        {
            if (!this._obs.IsConnected)
            {
                this._log.Warning("Cannot get input list - not connected");
                return new String[0];
            }

            try
            {
                this._log.Info("Getting input list");
                return this._obs.GetInputList();
            }
            catch (Exception ex)
            {
                this._log.Error($"Failed to get input list: {ex.Message}");
                return new String[0];
            }
        }

        public String GetInputKind(String inputName)
        {
            if (!this._obs.IsConnected)
            {
                this._log.Warning($"Cannot get input kind for '{inputName}' - not connected");
                return String.Empty;
            }

            try
            {
                return this._obs.GetInputKind(inputName);
            }
            catch (Exception ex)
            {
                this._log.Error($"Failed to get input kind for '{inputName}': {ex.Message}");
                return String.Empty;
            }
        }

        public Boolean GetInputMute(String inputName)
        {
            if (!this._obs.IsConnected)
            {
                this._log.Warning($"Cannot get input mute state for '{inputName}' - not connected");
                return false;
            }

            try
            {
                return this._obs.GetInputMute(inputName);
            }
            catch (Exception ex)
            {
                this._log.Error($"Failed to get input mute state for '{inputName}': {ex.Message}");
                return false;
            }
        }

        public void ToggleInputMute(String inputName)
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning($"Cannot toggle input mute for '{inputName}' - not connected");
                    return;
                }

                if (String.IsNullOrEmpty(inputName))
                {
                    this._log.Warning("Cannot toggle input mute - input name is empty");
                    return;
                }

                try
                {
                    this._log.Info($"Toggling input mute for '{inputName}'");
                    this._obs.ToggleInputMute(inputName);
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to toggle input mute for '{inputName}': {ex.Message}");
                }
            });
        }

        public Single GetInputVolume(String inputName)
        {
            if (!this._obs.IsConnected)
            {
                this._log.Warning($"Cannot get input volume for '{inputName}' - not connected");
                return 1.0f;
            }

            try
            {
                return this._obs.GetInputVolume(inputName);
            }
            catch (Exception ex)
            {
                this._log.Error($"Failed to get input volume for '{inputName}': {ex.Message}");
                return 1.0f;
            }
        }

        public String[] GetAudioSourcesInScene(String sceneName)
        {
            if (!this._obs.IsConnected)
            {
                this._log.Warning($"Cannot get audio sources for scene '{sceneName}' - not connected");
                return new String[0];
            }

            if (String.IsNullOrEmpty(sceneName))
            {
                this._log.Warning("Cannot get audio sources - scene name is empty");
                return new String[0];
            }

            try
            {
                this._log.Info($"Getting audio sources for scene '{sceneName}'");
                return this._obs.GetAudioSourcesInScene(sceneName);
            }
            catch (Exception ex)
            {
                this._log.Error($"Failed to get audio sources for scene '{sceneName}': {ex.Message}");
                return new String[0];
            }
        }

        public String[] GetAudioInputsNotInAnyScene()
        {
            if (!this._obs.IsConnected)
            {
                this._log.Warning("Cannot get audio inputs not in any scene - not connected");
                return new String[0];
            }

            try
            {
                return this._obs.GetAudioInputsNotInAnyScene();
            }
            catch (Exception ex)
            {
                this._log.Error($"Failed to get audio inputs not in any scene: {ex.Message}");
                return new String[0];
            }
        }

        public String[] GetScenesForInput(String inputName)
        {
            if (!this._obs.IsConnected)
            {
                this._log.Warning($"Cannot get scenes for input '{inputName}' - not connected");
                return new String[0];
            }

            if (String.IsNullOrEmpty(inputName))
            {
                this._log.Warning("Cannot get scenes for input - input name is empty");
                return new String[0];
            }

            try
            {
                return this._obs.GetScenesForInput(inputName);
            }
            catch (Exception ex)
            {
                this._log.Error($"Failed to get scenes for input '{inputName}': {ex.Message}");
                return new String[0];
            }
        }

        public Boolean GetStudioModeEnabled()
        {
            if (!this._obs.IsConnected)
            {
                this._log.Warning("Cannot get studio mode state - not connected");
                return false;
            }

            try
            {
                return this._obs.GetStudioModeEnabled();
            }
            catch (Exception ex)
            {
                this._log.Error($"Failed to get studio mode state: {ex.Message}");
                return false;
            }
        }

        public void ToggleStudioMode()
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot toggle studio mode - not connected");
                    return;
                }

                try
                {
                    this._log.Info($"Toggling studio mode from {this._studioModeEnabled} to {!this._studioModeEnabled}");
                    this._obs.SetStudioModeEnabled(!this._studioModeEnabled);
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to toggle studio mode: {ex.Message}");
                }
            });
        }

        public void SetStudioModeState(Boolean enabled)
        {
            this._studioModeEnabled = enabled;
        }
    }
}
