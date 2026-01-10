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

        public Boolean IsRecording => this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED;
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

        public void PauseRecording()
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot pause recording - not connected");
                    return;
                }

                if (!this.IsRecording)
                {
                    this._log.Warning("Cannot pause recording - not recording");
                    return;
                }

                this._log.Info("Pausing recording");
                this._obs.PauseRecord();
            });
        }

        public void ResumeRecording()
        {
            Task.Run(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning("Cannot resume recording - not connected");
                    return;
                }

                if (!this.IsRecording)
                {
                    this._log.Warning("Cannot resume recording - not recording");
                    return;
                }

                this._log.Info("Resuming recording");
                this._obs.ResumeRecord();
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
