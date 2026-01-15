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
        private String _currentProfile = String.Empty;
        private String _currentSceneCollection = String.Empty;
        private String _currentScene = String.Empty;

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
                
                this._log.Info($"Setting current scene to '{sceneName}'");
                this._obs.SetCurrentProgramScene(sceneName);
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

                this._log.Info("Toggling recording");
                this._obs.ToggleRecord();
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

                this._log.Info("Starting recording");
                this._obs.StartRecord();
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

                this._log.Info("Stopping recording");
                this._obs.StopRecord();
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

                this._log.Info("Toggling recording pause");
                if (this.IsRecordingPaused)
                {
                    this._obs.ResumeRecord();
                }
                else
                {
                    this._obs.PauseRecord();
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

            this._log.Info("Getting profile list");
            return this._obs.GetProfileList();
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

                this._log.Info($"Setting current profile to '{profileName}'");
                this._obs.SetCurrentProfile(profileName);
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

            this._log.Info("Getting scene collection list");
            return this._obs.GetSceneCollectionList();
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

                this._log.Info($"Setting current scene collection to '{sceneCollectionName}'");
                this._obs.SetCurrentSceneCollection(sceneCollectionName);
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

            this._log.Info("Getting scene list");
            return this._obs.GetSceneList();
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

                var filename = System.IO.Path.Combine(screenshotPath, $"Screenshot-{DateTime.Now:yyyyMMddHHmmssfff}.png");
                this._log.Info($"Saving screenshot to {filename}");
                this._obs.SaveSourceScreenshot(this._currentScene, "png", filename, -1, -1);
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

                this._log.Info("Toggling streaming");
                this._obs.ToggleStream();
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

                this._log.Info("Starting streaming");
                this._obs.StartStream();
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

                this._log.Info("Stopping streaming");
                this._obs.StopStream();
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

                this._log.Info("Toggling virtual camera");
                if (this.IsVirtualCameraActive)
                {
                    this._obs.StopVirtualCam();
                }
                else
                {
                    this._obs.StartVirtualCam();
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

                this._log.Info("Starting virtual camera");
                this._obs.StartVirtualCam();
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

                this._log.Info("Stopping virtual camera");
                this._obs.StopVirtualCam();
            });
        }

        public void SetVirtualCameraState(OutputState state)
        {
            this._virtualCameraState = state;
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

            this._log.Info($"Getting scene item list for '{sceneName}'");
            return this._obs.GetSceneItemList(sceneName);
        }

        public Boolean GetSceneItemEnabled(String sceneName, String sourceName)
        {
            if (!this._obs.IsConnected)
            {
                this._log.Warning($"Cannot get scene item enabled state for '{sourceName}' - not connected");
                return false;
            }

            return this._obs.GetSceneItemEnabled(sceneName, sourceName);
        }

        public void ToggleSourceVisibility(String sceneName, String sourceName)
        {
            Task.Run(() =>
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

                var currentState = this._obs.GetSceneItemEnabled(sceneName, sourceName);
                this._log.Info($"Toggling source '{sourceName}' visibility from {currentState} to {!currentState}");
                this._obs.SetSceneItemEnabled(sceneName, sourceName, !currentState);
            });
        }
    }
}
