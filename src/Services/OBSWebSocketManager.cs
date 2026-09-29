namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Timers;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;
    using OBSWebsocketDotNet;
    using OBSWebsocketDotNet.Communication;
    using OBSWebsocketDotNet.Types;
    using OBSWebsocketDotNet.Types.Events;

    public class OBSWebSocketManager : IDisposable
    {
        private readonly OBSWebsocket _obs;
        private readonly Timer _reconnectTimer;
        private readonly IPluginLog _log;
        private readonly ReconnectionStrategy _reconnectionStrategy;
        private readonly Object _disposeLock = new Object();
        private readonly HashSet<String> _volumeMeterOwners = new HashSet<String>();
        private readonly SessionGate _session = new SessionGate();
        private String _lastUrl;
        private String _lastPassword;
        private Boolean _shouldReconnect = false;
        private Boolean _disposed = false;
        private Boolean _connectingInProgress = false;

        // Connected means identified: the socket is open during the handshake too, before OBS will
        // accept requests.
        public Boolean IsConnected => this._obs != null && this._obs.IsConnected && this._obs.IsIdentified;
        public Boolean IsConnecting => this._connectingInProgress;
        internal TimeSpan RequestTimeout => this._obs.WSTimeout;
        public Boolean ShouldReconnect => this._shouldReconnect;
        public Boolean IsStreaming => this.Actions.IsStreaming;
        public Boolean IsRecording => this.Actions.IsRecording;
        public Boolean IsStreamingChanging => this.Actions.IsStreamingChanging;
        public Boolean IsRecordingChanging => this.Actions.IsRecordingChanging;
        public OBSActionExecutor Actions { get; }
        public AudioMeterService AudioMeters { get; }
        public AudioStateCache AudioState { get; }
        public KeyedStateCache<(String Scene, String Source), Boolean> SourceVisibility { get; }
        public KeyedStateCache<String, String> MediaState { get; }
        public CachedValue<Models.AudioInputSceneMembership> AudioInputMembership { get; }

        public event EventHandler ConnectionEstablished;
        public event EventHandler ConnectionLost;

        public OBSWebSocketManager() : this(new PluginLogAdapter())
        {
        }

        public OBSWebSocketManager(IPluginLog log)
        {
            this._log = log;
            this._obs = new OBSWebsocket();

            // Set before any connection exists: in obs-websocket-dotnet 5.7.0 the WSTimeout setter also
            // sets the live Websocket.Client's no-message ReconnectTimeout, so setting it while connected
            // would drop the connection whenever OBS went quiet for that long. ConnectAsync creates a
            // fresh client with that watchdog off but keeps this request timeout.
            this._obs.WSTimeout = TimeSpan.FromMilliseconds(OBSTimings.RequestTimeout);
            this._reconnectionStrategy = new ReconnectionStrategy(log);
            this.Actions = new OBSActionExecutor(new OBSWebsocketAdapter(this._obs), log);
            this.AudioMeters = new AudioMeterService();
            this.AudioState = new AudioStateCache(this.Actions.TryGetInputAudioState, this.OnAudioStateFetched);
            this.SourceVisibility = new KeyedStateCache<(String Scene, String Source), Boolean>(
                this.TryFetchSourceVisibility, this.OnSourceVisibilityFetched, false, "source visibility");
            this.MediaState = new KeyedStateCache<String, String>(
                this.Actions.TryGetMediaInputStatus, this.OnMediaStateFetched, MediaInputStates.None, "media state");
            this.AudioInputMembership = new CachedValue<Models.AudioInputSceneMembership>(
                this.Actions.TryGetAudioInputSceneMembership, "audio input scene membership");
            this._reconnectTimer = new Timer();
            this._reconnectTimer.Elapsed += this.OnReconnectTimer;
            this._reconnectTimer.AutoReset = false;

            this._obs.Disconnected += this.OnDisconnected;
            this._obs.Connected += this.OnConnected;
            this._obs.StreamStateChanged += this.OnStreamStateChanged;
            this._obs.RecordStateChanged += this.OnRecordStateChanged;
            this._obs.VirtualcamStateChanged += this.OnVirtualCameraStateChanged;
            this._obs.ReplayBufferStateChanged += this.OnReplayBufferStateChanged;
            this._obs.CurrentProfileChanged += this.OnCurrentProfileChanged;
            this._obs.CurrentSceneCollectionChanged += this.OnCurrentSceneCollectionChanged;
            this._obs.SceneListChanged += this.OnSceneListChanged;
            this._obs.CurrentProgramSceneChanged += this.OnCurrentSceneChanged;
            this._obs.InputMuteStateChanged += this.OnInputMuteStateChanged;
            this._obs.InputVolumeChanged += this.OnInputVolumeChanged;
            this._obs.StudioModeStateChanged += this.OnStudioModeStateChanged;
            this._obs.SceneItemEnableStateChanged += this.OnSceneItemEnableStateChanged;
            this._obs.InputAudioMonitorTypeChanged += this.OnInputAudioMonitorTypeChanged;
            this._obs.SceneItemCreated += this.OnSceneItemCreated;
            this._obs.SceneItemRemoved += this.OnSceneItemRemoved;
            this._obs.InputCreated += this.OnInputCreated;
            this._obs.InputRemoved += this.OnInputRemoved;
            this._obs.InputNameChanged += this.OnInputNameChanged;
            this._obs.SceneNameChanged += this.OnSceneNameChanged;
            this._obs.MediaInputPlaybackStarted += this.OnMediaInputPlaybackStarted;
            this._obs.MediaInputPlaybackEnded += this.OnMediaInputPlaybackEnded;
            this._obs.MediaInputActionTriggered += this.OnMediaInputActionTriggered;
            this._obs.ReplayBufferSaved += this.OnReplayBufferSaved;
            
            this._log.Info("OBSWebSocketManager initialized");
        }

        public async Task ConnectAsync(String url, String password)
        {
            this._lastUrl = url;
            this._lastPassword = password;
            this._shouldReconnect = true;
            this._connectingInProgress = true;

            // A fresh connection starts a fresh session, even if the previous one's Disconnected
            // event has not been processed yet.
            this._session.Close();

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
            this._session.Close();
            this._obs?.Disconnect();
        }

        public Int32 GetReconnectDelay(Int32 attempt)
        {
            return this._reconnectionStrategy.GetNextDelay();
        }

        // InputVolumeMeters is a high-volume event (fires ~20x/sec) - the library's event accessor
        // sends the ReIdentify to opt in on the first += and opt back out once the last -= removes
        // the final handler. Several meter UIs (the live audio folder, per-source meter buttons) can
        // need it at once, so each registers as a named owner and the event handler is attached
        // only while at least one owner remains.
        public void SubscribeToVolumeMeters(String owner)
        {
            if (String.IsNullOrEmpty(owner))
            {
                return;
            }

            lock (this._volumeMeterOwners)
            {
                if (!this._volumeMeterOwners.Add(owner) || this._volumeMeterOwners.Count > 1)
                {
                    return;
                }

                this._obs.InputVolumeMeters += this.OnInputVolumeMeters;
                this._log.Debug($"Subscribed to InputVolumeMeters (requested by '{owner}')");
            }
        }

        public void UnsubscribeFromVolumeMeters(String owner)
        {
            if (String.IsNullOrEmpty(owner))
            {
                return;
            }

            lock (this._volumeMeterOwners)
            {
                if (!this._volumeMeterOwners.Remove(owner) || this._volumeMeterOwners.Count > 0)
                {
                    return;
                }

                this._obs.InputVolumeMeters -= this.OnInputVolumeMeters;
                this.AudioMeters.Clear();
                this._log.Debug($"Unsubscribed from InputVolumeMeters (last released by '{owner}')");
            }
        }

        private void OnInputVolumeMeters(Object sender, InputVolumeMetersEventArgs e)
        {
            if (e?.inputs == null)
                return;

            foreach (var input in e.inputs)
            {
                if (String.IsNullOrEmpty(input?.InputName))
                    continue;

                var peaks = input.InputLevels?.Select(channel => channel.PeakWithVolume).ToArray() ?? new Single[0];
                this.AudioMeters.UpdateLevels(input.InputName, new Models.AudioMeterLevels { ChannelPeaks = peaks });

                // Seed the mute state once per input so meter tiles can render it without querying
                // OBS on every repaint; InputMuteStateChanged keeps it current from then on.
                if (this.AudioMeters.TryBeginMuteLookup(input.InputName))
                {
                    String inputName = input.InputName;
                    Task.Run(() => this.AudioMeters.CompleteMuteLookup(inputName, this.Actions.GetInputMute(inputName)));
                }
            }
        }

        private void OnConnected(Object sender, EventArgs e)
        {
            if (!this._session.TryOpen())
            {
                this._log.Debug("Ignoring repeated Connected for the current session (ReIdentify confirmation)");
                return;
            }

            this._log.Info("WebSocket connection established");
            this._connectingInProgress = false;
            this.AudioState.Clear();
            this.SourceVisibility.Clear();
            this.MediaState.Clear();
            this.AudioInputMembership.Invalidate("new connection");
            this._reconnectionStrategy.Reset();
            this._reconnectTimer?.Stop();
            
            // Raise plugin-level event
            this.ConnectionEstablished?.Invoke(this, EventArgs.Empty);
            
            // Load the initial state once and push it to the commands - they no longer query OBS
            // for these lists themselves in OnConnected.
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
                        this.NotifyProfileList(profiles.Profiles, profiles.CurrentProfileName);
                    }

                    var currentCollection = this._obs.GetCurrentSceneCollection();
                    if (!String.IsNullOrEmpty(currentCollection))
                    {
                        this.Actions.SetCurrentSceneCollectionState(currentCollection);
                        this._log.Info($"Initial scene collection: '{currentCollection}'");
                        OBSStudioForLogiPlugin.Instance?.OnSceneCollectionChanged(String.Empty, currentCollection);
                    }

                    String[] sceneCollections = this.Actions.GetSceneCollectionList();
                    OBSStudioForLogiPlugin.Instance?.OnSceneCollectionsChanged(sceneCollections, currentCollection ?? String.Empty);

                    var sceneList = this._obs.GetSceneList();
                    if (sceneList?.CurrentProgramSceneName != null)
                    {
                        this.Actions.SetCurrentSceneState(sceneList.CurrentProgramSceneName);
                        this._log.Info($"Initial scene: '{sceneList.CurrentProgramSceneName}'");
                        OBSStudioForLogiPlugin.Instance?.OnCurrentSceneChanged(sceneList.CurrentProgramSceneName);
                    }

                    // Load initial scene list and notify commands
                    this.UpdateSceneList();
                    this.UpdateInputList();
                    this.UpdateStudioModeState();
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
            this._connectingInProgress = false;
            this._session.Close();
            
            // Raise plugin-level event
            this.ConnectionLost?.Invoke(this, EventArgs.Empty);
            
            this.Actions.SetStreamingState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);
            this.Actions.SetRecordingState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);
            this.Actions.SetVirtualCameraState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);
            this.Actions.SetReplayBufferState(OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED);
            this.Actions.SetStudioModeState(false);

            // State may change while disconnected - drop the caches so they're re-queried.
            this.AudioMeters.Clear();
            this.AudioState.Clear();
            this.SourceVisibility.Clear();
            this.MediaState.Clear();
            this.AudioInputMembership.Invalidate("disconnected");

            // NotifyDisconnected is called via OBSStudioForLogiPlugin.OnOBSDisconnected → CommandCoordinator
            
            if (this._shouldReconnect && !this._disposed)
            {
                this.ScheduleNextReconnect();
            }
        }

        private void OnStreamStateChanged(Object sender, StreamStateChangedEventArgs e)
        {
            var state = e?.OutputState?.State ?? OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
            this.Actions.SetStreamingState(state);
            this._log.Info($"Streaming state changed to {state}");
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

        private void OnReplayBufferStateChanged(Object sender, ReplayBufferStateChangedEventArgs e)
        {
            var state = e?.OutputState?.State ?? OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
            this.Actions.SetReplayBufferState(state);
            this._log.Info($"Replay buffer state changed to {state}");
            OBSStudioForLogiPlugin.Instance?.OnReplayBufferStateChanged();
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
                        this.NotifyProfileList(profiles.Profiles, profiles.CurrentProfileName);
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

            // A different collection can reuse scene, source and input names with different state.
            this.SourceVisibility.Clear();
            this.MediaState.Clear();
            this.AudioInputMembership.Invalidate("scene collection changed");
            this._log.Info($"Current scene collection changed to '{e.SceneCollectionName}'");
            
            // Notify SceneCollectionSelectCommand
            OBSStudioForLogiPlugin.Instance?.OnSceneCollectionChanged(oldSceneCollection, e.SceneCollectionName);
            
            // Update scenes in dynamic folder and get new current scene
            this.UpdateSceneList();
            
            // Query and update current scene after collection change
            Task.Run(() =>
            {
                try
                {
                    // Small delay to ensure OBS has updated
                    Task.Delay(100).Wait();
                    
                    var sceneList = this._obs.GetSceneList();
                    if (sceneList?.CurrentProgramSceneName != null)
                    {
                        this.Actions.SetCurrentSceneState(sceneList.CurrentProgramSceneName);
                        this._log.Info($"Current scene updated to '{sceneList.CurrentProgramSceneName}' after collection change");
                        OBSStudioForLogiPlugin.Instance?.OnCurrentSceneChanged(sceneList.CurrentProgramSceneName);
                    }
                }
                catch (Exception ex)
                {
                    this._log.Warning($"Failed to update current scene after collection change: {ex.Message}");
                }
            });
        }

        private void OnSceneListChanged(Object sender, EventArgs e)
        {
            this._log.Info("Scene list changed");
            this.AudioInputMembership.Invalidate("scene list changed");
            this.UpdateSceneList();
        }

        private void UpdateSceneList()
        {
            Task.Run(() =>
            {
                try
                {
                    var scenes = this.Actions.GetSceneList();
                    this._log.Debug($"Loaded {scenes.Length} scenes");
                    OBSStudioForLogiPlugin.Instance?.OnScenesChanged(scenes);
                }
                catch (Exception ex)
                {
                    this._log.Warning($"Failed to get scene list: {ex.Message}");
                }
            });
        }

        // GetProfileList already returns the full list alongside the current profile, so pass it on
        // rather than requesting it from OBS a second time.
        private void NotifyProfileList(IEnumerable<String> profiles, String currentProfile)
        {
            String[] profileNames = profiles?.ToArray() ?? new String[0];
            this._log.Debug($"Loaded {profileNames.Length} profiles");
            OBSStudioForLogiPlugin.Instance?.OnProfilesChanged(profileNames, currentProfile);
        }

        private void OnCurrentSceneChanged(Object sender, ProgramSceneChangedEventArgs e)
        {
            if (e?.SceneName == null)
                return;

            this.Actions.SetCurrentSceneState(e.SceneName);
            this._log.Info($"Current scene changed to '{e.SceneName}'");
            OBSStudioForLogiPlugin.Instance?.OnCurrentSceneChanged(e.SceneName);
        }

        private void UpdateInputList()
        {
            Task.Run(() =>
            {
                try
                {
                    var inputs = this.Actions.GetInputList();
                    this._log.Debug($"Loaded {inputs.Length} inputs");
                    OBSStudioForLogiPlugin.Instance?.OnInputsChanged(inputs);
                }
                catch (Exception ex)
                {
                    this._log.Warning($"Failed to get input list: {ex.Message}");
                }
            });
        }

        // The cache fetched an input the buttons were showing defaults for - redraw them.
        private void OnAudioStateFetched(String inputName)
        {
            OBSStudioForLogiPlugin.Instance?.OnInputMuteChanged(inputName);
            OBSStudioForLogiPlugin.Instance?.OnInputVolumeChanged(inputName);
            OBSStudioForLogiPlugin.Instance?.OnInputMonitorTypeChanged(inputName);
        }

        private Boolean TryFetchSourceVisibility((String Scene, String Source) key, out Boolean enabled)
        {
            return this.Actions.TryGetSceneItemEnabled(key.Scene, key.Source, out enabled);
        }

        // The cache fetched a source the buttons were showing "hidden" for - redraw them.
        private void OnSourceVisibilityFetched((String Scene, String Source) key)
        {
            OBSStudioForLogiPlugin.Instance?.OnSourceVisibilityChanged(key.Scene, key.Source);
        }

        // The cache fetched an input the media buttons were showing "idle" for - redraw them.
        private void OnMediaStateFetched(String inputName)
        {
            OBSStudioForLogiPlugin.Instance?.OnMediaPlaybackStateChanged(inputName);
        }

        private void OnInputMuteStateChanged(Object sender, OBSWebsocketDotNet.Types.Events.InputMuteStateChangedEventArgs e)
        {
            if (e?.InputName == null)
                return;

            this.AudioMeters.SetMuted(e.InputName, e.InputMuted);
            this.AudioState.SetMuted(e.InputName, e.InputMuted);
            OBSStudioForLogiPlugin.Instance?.OnInputMuteChanged(e.InputName);
        }

        private void OnInputVolumeChanged(Object sender, OBSWebsocketDotNet.Types.Events.InputVolumeChangedEventArgs e)
        {
            if (e?.Volume?.InputName == null)
                return;

            PluginLog.Trace($"Input '{e.Volume.InputName}' volume changed to {e.Volume.InputVolumeMul}");
            this.AudioState.SetVolume(e.Volume.InputName, e.Volume.InputVolumeMul);
            OBSStudioForLogiPlugin.Instance?.OnInputVolumeChanged(e.Volume.InputName);
        }

        private void UpdateStudioModeState()
        {
            Task.Run(() =>
            {
                try
                {
                    var enabled = this.Actions.GetStudioModeEnabled();
                    this.Actions.SetStudioModeState(enabled);
                    this._log.Info($"Initial studio mode state: {enabled}");
                    OBSStudioForLogiPlugin.Instance?.OnStudioModeStateChanged();
                }
                catch (Exception ex)
                {
                    this._log.Warning($"Failed to get studio mode state: {ex.Message}");
                }
            });
        }

        private void OnStudioModeStateChanged(Object sender, StudioModeStateChangedEventArgs e)
        {
            this.Actions.SetStudioModeState(e.StudioModeEnabled);
            this._log.Info($"Studio mode state changed to {e.StudioModeEnabled}");
            OBSStudioForLogiPlugin.Instance?.OnStudioModeStateChanged();
        }

        private void OnSceneItemEnableStateChanged(Object sender, SceneItemEnableStateChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(e?.SceneName))
                return;

            // Event provides SceneItemId, need to get source name from scene items
            Task.Run(() =>
            {
                try
                {
                    var sceneItems = this._obs?.GetSceneItemList(e.SceneName);
                    var item = sceneItems?.FirstOrDefault(i => i.ItemId == e.SceneItemId);
                    if (item != null && !String.IsNullOrEmpty(item.SourceName))
                    {
                        this._log.Info($"Scene item '{item.SourceName}' visibility changed to {e.SceneItemEnabled} in scene '{e.SceneName}'");

                        // Buttons show the source's first item in the scene (GetSceneItemEnabled
                        // looks the source up by name), so a duplicate item must not overwrite it.
                        SceneItemDetails firstItem = sceneItems.First(i => i.SourceName == item.SourceName);
                        if (firstItem.ItemId == item.ItemId)
                        {
                            this.SourceVisibility.Set((e.SceneName, item.SourceName), e.SceneItemEnabled);
                        }

                        OBSStudioForLogiPlugin.Instance?.OnSourceVisibilityChanged(e.SceneName, item.SourceName);
                    }
                }
                catch (Exception ex)
                {
                    // Without the source name the cached entry can't be found - drop the scene's
                    // entries so its buttons re-read OBS rather than keep showing the old state.
                    this.SourceVisibility.RemoveWhere(key => key.Scene == e.SceneName);
                    this._log.Warning($"Failed to process scene item visibility change: {ex.Message}");
                }
            });
        }

        private void OnInputAudioMonitorTypeChanged(Object sender, InputAudioMonitorTypeChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(e?.InputName))
                return;

            this._log.Info($"Input '{e.InputName}' audio monitor type changed");
            this.AudioState.SetMonitorType(e.InputName, e.MonitorType);
            OBSStudioForLogiPlugin.Instance?.OnInputMonitorTypeChanged(e.InputName);
        }

        private void OnSceneItemCreated(Object sender, SceneItemCreatedEventArgs e)
        {
            if (String.IsNullOrEmpty(e?.SceneName))
                return;

            this._log.Info($"Scene item created in scene '{e.SceneName}'");
            this.SourceVisibility.RemoveWhere(key => key.Scene == e.SceneName);
            this.AudioInputMembership.Invalidate($"scene item created in '{e.SceneName}'");
            OBSStudioForLogiPlugin.Instance?.OnSceneItemsChanged(e.SceneName);
        }

        private void OnSceneItemRemoved(Object sender, SceneItemRemovedEventArgs e)
        {
            if (String.IsNullOrEmpty(e?.SceneName))
                return;

            this._log.Info($"Scene item removed from scene '{e.SceneName}'");
            this.SourceVisibility.RemoveWhere(key => key.Scene == e.SceneName);
            this.AudioInputMembership.Invalidate($"scene item removed from '{e.SceneName}'");
            OBSStudioForLogiPlugin.Instance?.OnSceneItemsChanged(e.SceneName);
        }

        private void OnInputCreated(Object sender, InputCreatedEventArgs e)
        {
            this._log.Info($"Input created: '{e?.InputName}'");
            this.AudioInputMembership.Invalidate("input created");
            this.UpdateInputList();
        }

        private void OnInputRemoved(Object sender, InputRemovedEventArgs e)
        {
            this._log.Info($"Input removed: '{e?.InputName}'");
            this.AudioState.Remove(e?.InputName);
            this.MediaState.Remove(e?.InputName);
            this.AudioInputMembership.Invalidate("input removed");
            this.UpdateInputList();
        }

        private void OnInputNameChanged(Object sender, InputNameChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(e?.OldInputName) || String.IsNullOrEmpty(e.InputName))
                return;

            this._log.Info($"Input renamed from '{e.OldInputName}' to '{e.InputName}'");
            this.ApplyInputRename(e.OldInputName, e.InputName);
            this.WarnUserDefinedButtonsRenamed("input", e.OldInputName, e.InputName);

            // The folders list inputs by name, and the current scene's source folders may list it too.
            this.UpdateInputList();
            String currentScene = this.Actions.CurrentScene;
            if (!String.IsNullOrEmpty(currentScene))
            {
                OBSStudioForLogiPlugin.Instance?.OnSceneItemsChanged(currentScene);
            }
        }

        private void OnSceneNameChanged(Object sender, SceneNameChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(e?.OldSceneName) || String.IsNullOrEmpty(e.SceneName))
                return;

            this._log.Info($"Scene renamed from '{e.OldSceneName}' to '{e.SceneName}'");
            Boolean wasCurrentScene = this.ApplySceneRename(e.OldSceneName, e.SceneName);
            this.WarnUserDefinedButtonsRenamed("scene", e.OldSceneName, e.SceneName);

            // Refresh the list here rather than relying on OBS also sending SceneListChanged for a
            // rename. If it does, the second refresh is one cheap request.
            this.UpdateSceneList();
            if (wasCurrentScene)
            {
                OBSStudioForLogiPlugin.Instance?.OnCurrentSceneChanged(e.SceneName);
            }
        }

        // Moves everything keyed by the input's old name to its new name. Must run before the
        // refreshed input list reaches the folders, or they drop the audio selection.
        internal void ApplyInputRename(String oldInputName, String newInputName)
        {
            this.AudioState.Rename(oldInputName, newInputName);
            this.AudioMeters.RenameInput(oldInputName, newInputName);
            this.MediaState.RenameKeys(key => key == oldInputName ? newInputName : key);
            this.SourceVisibility.RenameKeys(key => key.Source == oldInputName ? (key.Scene, newInputName) : key);
            this.AudioInputMembership.Invalidate("input renamed");
            AudioSelectionState.RenameIfMatches(oldInputName, newInputName);
        }

        // Moves everything keyed by the scene's old name to its new name, and returns whether it
        // was the current scene. A scene can also be a source in another scene, so both halves of
        // the source visibility key are renamed.
        internal Boolean ApplySceneRename(String oldSceneName, String newSceneName)
        {
            this.SourceVisibility.RenameKeys(key => (
                key.Scene == oldSceneName ? newSceneName : key.Scene,
                key.Source == oldSceneName ? newSceneName : key.Source));

            if (this.Actions.CurrentScene != oldSceneName)
            {
                return false;
            }

            this.Actions.SetCurrentSceneState(newSceneName);
            return true;
        }

        // User-defined buttons store the name typed into the Action Editor, and the plugin can't
        // rewrite a saved button. Say so at the moment it happens, since afterwards those buttons
        // just stop working.
        private void WarnUserDefinedButtonsRenamed(String kind, String oldName, String newName)
        {
            this._log.Warning($"OBS {kind} '{oldName}' was renamed to '{newName}'. User-defined buttons that name '{oldName}' won't find it any more - change them to '{newName}' in the Action Editor");
        }

        private void OnMediaInputPlaybackStarted(Object sender, MediaInputPlaybackStartedEventArgs e)
        {
            if (String.IsNullOrEmpty(e?.InputName))
                return;

            this._log.Info($"Media playback started: '{e.InputName}'");
            this.MediaState.Set(e.InputName, MediaInputStates.Playing);
            OBSStudioForLogiPlugin.Instance?.OnMediaPlaybackStateChanged(e.InputName);
        }

        private void OnMediaInputPlaybackEnded(Object sender, MediaInputPlaybackEndedEventArgs e)
        {
            if (String.IsNullOrEmpty(e?.InputName))
                return;

            this._log.Info($"Media playback ended: '{e.InputName}'");
            this.MediaState.Set(e.InputName, MediaInputStates.Ended);
            OBSStudioForLogiPlugin.Instance?.OnMediaPlaybackStateChanged(e.InputName);
        }

        // Pause, resume and stop only arrive as this event - there is no playback-paused event.
        // OBS raises it for actions from its own UI as well as from websocket requests.
        private void OnMediaInputActionTriggered(Object sender, MediaInputActionTriggeredEventArgs e)
        {
            if (String.IsNullOrEmpty(e?.InputName))
                return;

            this._log.Info($"Media action '{e.MediaAction}' triggered on '{e.InputName}'");
            String state = MediaInputStates.StateAfterAction(e.MediaAction);
            if (state != null)
            {
                this.MediaState.Set(e.InputName, state);
            }
            else
            {
                // Next/previous playlist item: the resulting state isn't implied, so re-read it.
                this.MediaState.Remove(e.InputName);
            }

            OBSStudioForLogiPlugin.Instance?.OnMediaPlaybackStateChanged(e.InputName);
        }

        private void OnReplayBufferSaved(Object sender, ReplayBufferSavedEventArgs e)
        {
            var path = e?.SavedReplayPath ?? String.Empty;
            this._log.Info($"Replay buffer saved: '{path}'");
            OBSStudioForLogiPlugin.Instance?.OnReplayBufferSaved(path);
        }

        private void OnReconnectTimer(Object sender, ElapsedEventArgs e)
        {
            if (this._disposed || !this._shouldReconnect)
                return;

            if (this.IsConnected || this._connectingInProgress)
            {
                this._log.Debug("Already connected or connection in progress, skipping reconnection attempt");
                return;
            }

            this._connectingInProgress = true;
            this._reconnectionStrategy.TryReconnect(() =>
                this._obs.ConnectAsync(this._lastUrl, this._lastPassword));

            if (!this.IsConnected && this._shouldReconnect && !this._disposed)
            {
                this.ScheduleNextReconnect();
            }
        }

        private void ScheduleNextReconnect()
        {
            var delay = this._reconnectionStrategy.GetNextDelay();
            this._log.Info($"Scheduling next reconnection attempt in {delay}ms");
            this._reconnectTimer.Interval = delay;
            this._reconnectTimer.Start();
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
                    this._obs.ReplayBufferStateChanged -= this.OnReplayBufferStateChanged;
                    this._obs.CurrentProfileChanged -= this.OnCurrentProfileChanged;
                    this._obs.CurrentSceneCollectionChanged -= this.OnCurrentSceneCollectionChanged;
                    this._obs.SceneListChanged -= this.OnSceneListChanged;
                    this._obs.CurrentProgramSceneChanged -= this.OnCurrentSceneChanged;
                    this._obs.InputMuteStateChanged -= this.OnInputMuteStateChanged;
                    this._obs.InputVolumeChanged -= this.OnInputVolumeChanged;
                    this._obs.StudioModeStateChanged -= this.OnStudioModeStateChanged;
                    this._obs.SceneItemEnableStateChanged -= this.OnSceneItemEnableStateChanged;
                    this._obs.InputAudioMonitorTypeChanged -= this.OnInputAudioMonitorTypeChanged;
                    this._obs.SceneItemCreated -= this.OnSceneItemCreated;
                    this._obs.SceneItemRemoved -= this.OnSceneItemRemoved;
                    this._obs.InputCreated -= this.OnInputCreated;
                    this._obs.InputRemoved -= this.OnInputRemoved;
                    this._obs.InputNameChanged -= this.OnInputNameChanged;
                    this._obs.SceneNameChanged -= this.OnSceneNameChanged;
                    this._obs.MediaInputPlaybackStarted -= this.OnMediaInputPlaybackStarted;
                    this._obs.MediaInputPlaybackEnded -= this.OnMediaInputPlaybackEnded;
                    this._obs.MediaInputActionTriggered -= this.OnMediaInputActionTriggered;
                    this._obs.ReplayBufferSaved -= this.OnReplayBufferSaved;
                    
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
