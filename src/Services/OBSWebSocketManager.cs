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
        private readonly Object _disposeLock = new Object();
        private Int32 _reconnectAttempts = 0;
        private String _lastUrl;
        private String _lastPassword;
        private Boolean _shouldReconnect = false;
        private Boolean _disposed = false;
        private OutputState _streamingState = OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;

        public Boolean IsConnected => this._obs?.IsConnected ?? false;
        public Boolean ShouldReconnect => this._shouldReconnect;
        public Boolean IsStreaming => this._streamingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED;
        public Boolean IsRecording => this.Actions.IsRecording;
        public Boolean IsStreamingChanging => this._streamingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTING 
                                            || this._streamingState == OutputState.OBS_WEBSOCKET_OUTPUT_STOPPING;
        public Boolean IsRecordingChanging => this.Actions.IsRecordingChanging;
        public OBSActionExecutor Actions { get; }

        public OBSWebSocketManager() : this(new PluginLogAdapter())
        {
        }

        public OBSWebSocketManager(IPluginLog log)
        {
            this._log = log;
            this._obs = new OBSWebsocket();
            this.Actions = new OBSActionExecutor(new OBSWebsocketAdapter(this._obs), log);
            this._reconnectTimer = new Timer();
            this._reconnectTimer.Elapsed += this.OnReconnectTimer;
            this._reconnectTimer.AutoReset = false;

            this._obs.Disconnected += this.OnDisconnected;
            this._obs.Connected += this.OnConnected;
            this._obs.StreamStateChanged += this.OnStreamStateChanged;
            this._obs.RecordStateChanged += this.OnRecordStateChanged;
            this._obs.VirtualcamStateChanged += this.OnVirtualCameraStateChanged;
            this._obs.CurrentProfileChanged += this.OnCurrentProfileChanged;
            this._obs.CurrentSceneCollectionChanged += this.OnCurrentSceneCollectionChanged;
            this._obs.SceneListChanged += this.OnSceneListChanged;
            this._obs.CurrentProgramSceneChanged += this.OnCurrentSceneChanged;
            
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
            var baseDelay = this._backoffDelays[index];
            var jitter = Random.Shared.NextDouble() * 0.3 + 0.85; // 0.85-1.15x
            return (Int32)(baseDelay * jitter);
        }

        private void OnConnected(Object sender, EventArgs e)
        {
            this._log.Info("WebSocket connection established");
            this._reconnectAttempts = 0;
            this._reconnectTimer?.Stop();
            
            // Get initial state
            Task.Run(() =>
            {
                try
                {
                    var profiles = this._obs.GetProfileList();
                    if (profiles?.CurrentProfileName != null)
                    {
                        this.Actions.SetCurrentProfileState(profiles.CurrentProfileName);
                        this._log.Info($"Initial profile: '{profiles.CurrentProfileName}'");
                        OBSStudioForLogiPlugin.Instance?.OnProfileChanged(String.Empty, profiles.CurrentProfileName);
                    }

                    var currentCollection = this._obs.GetCurrentSceneCollection();
                    if (!String.IsNullOrEmpty(currentCollection))
                    {
                        this.Actions.SetCurrentSceneCollectionState(currentCollection);
                        this._log.Info($"Initial scene collection: '{currentCollection}'");
                        OBSStudioForLogiPlugin.Instance?.OnSceneCollectionChanged(String.Empty, currentCollection);
                    }

                    var sceneList = this._obs.GetSceneList();
                    if (sceneList?.CurrentProgramSceneName != null)
                    {
                        this.Actions.SetCurrentSceneState(sceneList.CurrentProgramSceneName);
                        this._log.Info($"Initial scene: '{sceneList.CurrentProgramSceneName}'");
                        OBSStudioForLogiPlugin.Instance?.OnCurrentSceneChanged(sceneList.CurrentProgramSceneName);
                    }

                    // Load initial scene list and notify commands
                    this.UpdateSceneList();
                    ProfileSelectCommand.Instance?.OnConnected();
                    SceneCollectionSelectCommand.Instance?.OnConnected();
                    VirtualCameraToggleCommand.Instance?.OnConnected();
                    VirtualCameraStartCommand.Instance?.OnConnected();
                    VirtualCameraStopCommand.Instance?.OnConnected();
                }
                catch (Exception ex)
                {
                    this._log.Warning($"Failed to get initial state: {ex.Message}");
                }
            });
        }

        private void OnDisconnected(Object sender, ObsDisconnectionInfo e)
        {
            this._log.Warning($"WebSocket disconnected: {e.DisconnectReason}");
            
            this._streamingState = OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
            this.Actions.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);
            this.Actions.SetVirtualCameraState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);
            
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
            this.Actions.SetStreamingState(this._streamingState);
            this._log.Info($"Streaming state changed to {this._streamingState}");
        }

        private void OnRecordStateChanged(Object sender, RecordStateChangedEventArgs e)
        {
            var state = e?.OutputState?.State ?? OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
            this.Actions.SetRecordingState(state);
            this._log.Info($"Recording state changed to {state}");
        }

        private void OnVirtualCameraStateChanged(Object sender, VirtualcamStateChangedEventArgs e)
        {
            var state = e?.OutputState?.State ?? OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
            this.Actions.SetVirtualCameraState(state);
            this._log.Info($"Virtual camera state changed to {state}");
            OBSStudioForLogiPlugin.Instance?.OnVirtualCameraStateChanged();
        }

        private void OnCurrentProfileChanged(Object sender, EventArgs e)
        {
            // Event doesn't provide profile name, query it
            Task.Run(() =>
            {
                try
                {
                    var profiles = this._obs.GetProfileList();
                    if (profiles?.CurrentProfileName != null)
                    {
                        var oldProfile = this.Actions.CurrentProfile;
                        this.Actions.SetCurrentProfileState(profiles.CurrentProfileName);
                        this._log.Info($"Current profile changed to '{profiles.CurrentProfileName}'");
                        
                        // Notify ProfileSelectCommand
                        OBSStudioForLogiPlugin.Instance?.OnProfileChanged(oldProfile, profiles.CurrentProfileName);
                    }
                }
                catch (Exception ex)
                {
                    this._log.Warning($"Failed to get current profile: {ex.Message}");
                }
            });
        }

        private void OnCurrentSceneCollectionChanged(Object sender, CurrentSceneCollectionChangedEventArgs e)
        {
            if (e?.SceneCollectionName == null)
                return;

            var oldSceneCollection = this.Actions.CurrentSceneCollection;
            this.Actions.SetCurrentSceneCollectionState(e.SceneCollectionName);
            this._log.Info($"Current scene collection changed to '{e.SceneCollectionName}'");
            
            // Notify SceneCollectionSelectCommand
            OBSStudioForLogiPlugin.Instance?.OnSceneCollectionChanged(oldSceneCollection, e.SceneCollectionName);
            
            // Update scenes in dynamic folder
            this.UpdateSceneList();
        }

        private void OnSceneListChanged(Object sender, EventArgs e)
        {
            this._log.Info("Scene list changed");
            this.UpdateSceneList();
        }

        private void UpdateSceneList()
        {
            Task.Run(() =>
            {
                try
                {
                    var scenes = this.Actions.GetSceneList();
                    this._log.Info($"Loaded {scenes.Length} scenes");
                    OBSStudioForLogiPlugin.Instance?.OnScenesChanged(scenes);
                }
                catch (Exception ex)
                {
                    this._log.Warning($"Failed to get scene list: {ex.Message}");
                }
            });
        }

        private void OnCurrentSceneChanged(Object sender, ProgramSceneChangedEventArgs e)
        {
            if (e?.SceneName == null)
                return;

            this.Actions.SetCurrentSceneState(e.SceneName);
            this._log.Info($"Current scene changed to '{e.SceneName}'");
            OBSStudioForLogiPlugin.Instance?.OnCurrentSceneChanged(e.SceneName);
        }

        private void OnReconnectTimer(Object sender, ElapsedEventArgs e)
        {
            if (this._disposed || !this._shouldReconnect)
                return;

            this._reconnectAttempts++;
            this._log.Info($"Reconnection attempt {this._reconnectAttempts} to {this._lastUrl}");
            
            try
            {
                this._obs.ConnectAsync(this._lastUrl, this._lastPassword);
            }
            catch (Exception ex)
            {
                this._log.Warning($"Connection attempt failed: {ex.Message}");
            }
            
            // Schedule next attempt if still not connected
            if (!this.IsConnected && this._shouldReconnect && !this._disposed)
            {
                var delay = this.GetReconnectDelay(this._reconnectAttempts);
                this._log.Info($"Scheduling next reconnection attempt in {delay}ms");
                this._reconnectTimer.Interval = delay;
                this._reconnectTimer.Start();
            }
        }

        public void Dispose()
        {
            lock (this._disposeLock)
            {
                if (this._disposed)
                    return;

                this._log.Info("Disposing OBSWebSocketManager");
                this._disposed = true;
                this._shouldReconnect = false;

                // Stop and dispose timer
                if (this._reconnectTimer != null)
                {
                    this._reconnectTimer.Stop();
                    this._reconnectTimer.Elapsed -= this.OnReconnectTimer;
                    this._reconnectTimer.Dispose();
                }

                // Unsubscribe from all events
                if (this._obs != null)
                {
                    this._obs.Disconnected -= this.OnDisconnected;
                    this._obs.Connected -= this.OnConnected;
                    this._obs.StreamStateChanged -= this.OnStreamStateChanged;
                    this._obs.RecordStateChanged -= this.OnRecordStateChanged;
                    this._obs.VirtualcamStateChanged -= this.OnVirtualCameraStateChanged;
                    this._obs.CurrentProfileChanged -= this.OnCurrentProfileChanged;
                    this._obs.CurrentSceneCollectionChanged -= this.OnCurrentSceneCollectionChanged;
                    this._obs.SceneListChanged -= this.OnSceneListChanged;
                    this._obs.CurrentProgramSceneChanged -= this.OnCurrentSceneChanged;
                    
                    this._obs.Disconnect();
                    
                    // Dispose if OBSWebsocket implements IDisposable
                    if (this._obs is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                this._log.Info("OBSWebSocketManager disposed");
            }
        }
    }
}
