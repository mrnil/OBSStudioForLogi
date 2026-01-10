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

        public Boolean IsRecording => this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED 
                                    || this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_PAUSED
                                    || this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_RESUMED;
        public Boolean IsRecordingPaused { get; private set; }
        public Boolean IsRecordingChanging => this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTING 
                                            || this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_STOPPING;

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

                this._log.Info($"Setting current profile to '{profileName}'");
                this._obs.SetCurrentProfile(profileName);
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
