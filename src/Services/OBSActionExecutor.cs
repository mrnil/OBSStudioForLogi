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

        public void SetRecordingState(OutputState state)
        {
            this._recordingState = state;
        }
    }
}
