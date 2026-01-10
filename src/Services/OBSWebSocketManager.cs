namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading.Tasks;
    using System.Timers;
    using OBSWebsocketDotNet;
    using OBSWebsocketDotNet.Communication;
    using OBSWebsocketDotNet.Types;
    using OBSWebsocketDotNet.Types.Events;

    public class OBSWebSocketManager : IDisposable
    {
        private readonly OBSWebsocket _obs;
        private readonly Timer _reconnectTimer;
        private readonly IPluginLog _log;
        private readonly Int32[] _backoffDelays = { 1000, 2000, 4000, 8000, 15000, 30000 };
        private Int32 _reconnectAttempts = 0;
        private String _lastUrl;
        private String _lastPassword;
        private Boolean _shouldReconnect = false;
        private Boolean _disposed = false;
        private OutputState _streamingState = OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
        private OutputState _recordingState = OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;

        public Boolean IsConnected => this._obs?.IsConnected ?? false;
        public Boolean ShouldReconnect => this._shouldReconnect;
        public Boolean IsStreaming => this._streamingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED;
        public Boolean IsRecording => this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED;
        public Boolean IsStreamingChanging => this._streamingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTING 
                                            || this._streamingState == OutputState.OBS_WEBSOCKET_OUTPUT_STOPPING;
        public Boolean IsRecordingChanging => this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTING 
                                            || this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_STOPPING;

        public OBSWebSocketManager() : this(new PluginLogAdapter())
        {
        }

        public OBSWebSocketManager(IPluginLog log)
        {
            this._log = log;
            this._obs = new OBSWebsocket();
            this._reconnectTimer = new Timer();
            this._reconnectTimer.Elapsed += this.OnReconnectTimer;
            this._reconnectTimer.AutoReset = false;

            this._obs.Disconnected += this.OnDisconnected;
            this._obs.Connected += this.OnConnected;
            this._obs.StreamStateChanged += this.OnStreamStateChanged;
            this._obs.RecordStateChanged += this.OnRecordStateChanged;
            
            this._log.Info("OBSWebSocketManager initialized");
        }

        public async Task ConnectAsync(String url, String password)
        {
            this._lastUrl = url;
            this._lastPassword = password;
            this._shouldReconnect = true;

            this._log.Info($"Connecting to OBS WebSocket at {url}");
            
            await Task.Run(() =>
            {
                this._obs.ConnectAsync(url, password);
            });
        }

        public void SetCurrentScene(String sceneName)
        {
            Task.Run(() =>
            {
                if (!this.IsConnected)
                {
                    this._log.Warning($"Cannot set scene '{sceneName}' - not connected");
                    return;
                }
                
                this._log.Info($"Setting current scene to '{sceneName}'");
                this._obs?.SetCurrentProgramScene(sceneName);
            });
        }

        public void ToggleRecording()
        {
            Task.Run(() =>
            {
                if (!this.IsConnected)
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
                this._obs?.ToggleRecord();
            });
        }

        public void Disconnect()
        {
            this._log.Info("Disconnecting from OBS WebSocket");
            this._shouldReconnect = false;
            this._reconnectTimer?.Stop();
            this._obs?.Disconnect();
        }

        public Int32 GetReconnectDelay(Int32 attempt)
        {
            var index = Math.Min(attempt, this._backoffDelays.Length - 1);
            return this._backoffDelays[index];
        }

        private void OnConnected(Object sender, EventArgs e)
        {
            this._log.Info("WebSocket connection established");
            this._reconnectAttempts = 0;
            this._reconnectTimer?.Stop();
        }

        private void OnDisconnected(Object sender, ObsDisconnectionInfo e)
        {
            this._log.Warning($"WebSocket disconnected: {e.DisconnectReason}");
            
            this._streamingState = OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
            this._recordingState = OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
            
            if (this._shouldReconnect && !this._disposed)
            {
                var delay = this.GetReconnectDelay(this._reconnectAttempts);
                this._log.Info($"Scheduling reconnection attempt in {delay}ms");
                this._reconnectTimer.Interval = delay;
                this._reconnectTimer.Start();
            }
        }

        private void OnStreamStateChanged(Object sender, StreamStateChangedEventArgs e)
        {
            this._streamingState = e?.OutputState?.State ?? OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
            this._log.Info($"Streaming state changed to {this._streamingState}");
        }

        private void OnRecordStateChanged(Object sender, RecordStateChangedEventArgs e)
        {
            this._recordingState = e?.OutputState?.State ?? OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
            this._log.Info($"Recording state changed to {this._recordingState}");
        }

        private void OnReconnectTimer(Object sender, ElapsedEventArgs e)
        {
            if (this._disposed || !this._shouldReconnect)
                return;

            this._reconnectAttempts++;
            this._log.Info($"Reconnection attempt {this._reconnectAttempts} to {this._lastUrl}");
            this._obs.ConnectAsync(this._lastUrl, this._lastPassword);
        }

        public void Dispose()
        {
            if (this._disposed)
                return;

            this._log.Info("Disposing OBSWebSocketManager");
            this._disposed = true;
            this._shouldReconnect = false;
            this._reconnectTimer?.Stop();
            this._reconnectTimer?.Dispose();
            this._obs?.Disconnect();
        }
    }
}
