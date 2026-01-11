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
        private String _currentProfile = String.Empty;
        private String _currentSceneCollection = String.Empty;
        private String _currentScene = String.Empty;

        public Boolean IsRecording => this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED 
                                    || this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_PAUSED
                                    || this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_RESUMED;
        public Boolean IsRecordingPaused { get; private set; }
        public Boolean IsRecordingChanging => this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTING 
                                            || this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_STOPPING;
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

        public void SaveScreenshot()
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot save screenshot - not connected");
                    return;
                }

                this._log.Info("Saving screenshot");
                this._obs.SaveSourceScreenshot();
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
    }
}
