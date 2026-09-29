namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading.Tasks;
    using Loupedeck.OBSStudioForLogiPlugin.Models;
    using Loupedeck.OBSStudioForLogiPlugin.Services;

    public class OBSStudioForLogiPlugin : Plugin
    {
        public static OBSStudioForLogiPlugin Instance { get; private set; }
        private readonly ConnectionManager _connectionManager;
        private readonly CommandCoordinator _commandCoordinator;
        private readonly OBSFacade _obsFacade;
        private readonly OBSConfigReader _obsConfigReader;
        private readonly StatsService _statsService;
        private readonly PluginConfigReader _pluginConfigReader;
        public static String ScreenshotPath { get; private set; }
        
        public override Boolean UsesApplicationApiOnly => true;
        public override Boolean HasNoApplication => true;

        public OBSStudioForLogiPlugin()
        {
            Instance = this;
            PluginLog.Init(this.Log);
            PluginResources.Init(this.Assembly);
            this._pluginConfigReader = new PluginConfigReader(new PluginSettingsSecretStore(this));
            var pluginConfig = LoadPluginConfiguration();
            DiscoverScreenshotPath();
            
            var obsManager = new OBSWebSocketManager();
            this._obsConfigReader = new OBSConfigReader();
            this._connectionManager = new ConnectionManager(obsManager, this._obsConfigReader, new OBSLifecycleManager());
            this._connectionManager.SetPluginConfig(pluginConfig);
            this._commandCoordinator = new CommandCoordinator();
            this._obsFacade = new OBSFacade(obsManager);
            this._statsService = new StatsService(pluginConfig?.StatsPollingInterval ?? 5000);
            this._statsService.StatsUpdated += this.OnStatsUpdated;
        }

        private static PluginConfig LoadPluginConfiguration()
        {
            var configReader = new PluginConfigReader();
            var config = configReader.ReadConfig();
            
            if (config != null)
            {
                PluginLog.CurrentLevel = config.LogLevel;
                PluginLog.Info($"Loaded configuration from file. Log level set to: {PluginLog.CurrentLevel}");
                PluginLog.Info($"Connection mode: {(config.UseLocalObs ? "Local" : "Remote")} OBS");
            }
            else
            {
                PluginLog.Info($"No configuration file found. Using default log level: {PluginLog.CurrentLevel}");
                PluginLog.Info($"To customize settings, create: {configReader.ConfigPath}");
            }

            return config;
        }

        public OBSConnectionSettings GetLocalOBSSettings()
        {
            return this._obsConfigReader?.ReadConfig();
        }

        public void SaveAndApplyConnectionConfig(PluginConfig config, Boolean clearStoredPassword)
        {
            if (!this._pluginConfigReader.SaveEditedConfig(config, clearStoredPassword))
            {
                PluginLog.Error("Failed to save plugin config - not applying it");
                return;
            }

            this.ApplyConnectionConfig(config);
        }

        private void ApplyConnectionConfig(PluginConfig config)
        {
            PluginLog.Info($"Applying connection config: UseLocal={config.UseLocalObs}, IP={config.RemoteIpAddress}, Port={config.RemotePort}, Polling={config.StatsPollingInterval}ms");
            this._connectionManager.SetPluginConfig(config);
            this._statsService.SetInterval(config.StatsPollingInterval);
            
            // Disconnect and reconnect with new settings
            this._connectionManager.Disconnect();
            Task.Run(() => this._connectionManager.ConnectAsync());
        }

        private static void DiscoverScreenshotPath()
        {
            var folders = new[] { Environment.SpecialFolder.MyPictures, Environment.SpecialFolder.MyDocuments, Environment.SpecialFolder.Desktop };
            foreach (var folder in folders)
            {
                var path = Environment.GetFolderPath(folder);
                if (System.IO.Directory.Exists(path))
                {
                    ScreenshotPath = path;
                    PluginLog.Info($"Screenshot path set to: {path}");
                    return;
                }
            }
            PluginLog.Warning("No valid screenshot path found");
        }

        public override void Load()
        {
            PluginLog.Info("Plugin loading...");

            try
            {
                // The constructor's read skips the secret store, because the SDK doesn't document
                // whether plugin settings are available that early. Re-read with it here, before
                // the first connection attempt, to pick up the remote password.
                this._connectionManager.SetPluginConfig(this._pluginConfigReader.ReadConfig());

                this.Info.Icon256x256 = EmbeddedResources.ReadImage("Loupedeck.OBSStudioForLogiPlugin.metadata.Icon256x256.png");

                // Subscribe to connection events for status reporting
                this._connectionManager.Connected += this.OnOBSConnected;
                this._connectionManager.Disconnected += this.OnOBSDisconnected;
                this._connectionManager.WebSocketServerDisabled += this.OnWebSocketServerDisabled;

                this.ClientApplication.ApplicationStarted += this.OnApplicationStarted;
                this.ClientApplication.ApplicationStopped += this.OnApplicationStopped;

                // Plugin status is never reported synchronously from here: the service redraws devices
                // inside OnPluginStatusChanged, and doing that while it is still building pages for
                // this plugin threw a concurrent-collection exception out of Load, failing the whole
                // load. Normal is the default and OnOBSConnected re-reports it once connected.
                if (this.ClientApplication.IsRunning())
                {
                    PluginLog.Info("OBS detected via ClientApplication");
                    this.OnApplicationStarted(this, EventArgs.Empty);
                }
                else
                {
                    PluginLog.Info("OBS not detected, attempting direct connection");
                    Task.Run(() => this.ReportPluginStatus(Loupedeck.PluginStatus.Warning, "OBS is offline. Please launch OBS"));
                    Task.Run(() => this._connectionManager.ConnectAsync());
                }
            }
            catch (Exception ex)
            {
                // The service never calls Unload after a failed Load, so without this the connection
                // manager's reconnect loop keeps running as an orphan for the life of the service.
                PluginLog.Error(ex, "Plugin load failed, releasing resources");
                this.Unload();
                throw;
            }

            PluginLog.Info("Plugin loaded");
        }

        public override void Unload()
        {
            PluginLog.Info("Plugin unloading...");
            
            // Unsubscribe from connection events
            this._connectionManager.Connected -= this.OnOBSConnected;
            this._connectionManager.Disconnected -= this.OnOBSDisconnected;
            this._connectionManager.WebSocketServerDisabled -= this.OnWebSocketServerDisabled;
            
            this.ClientApplication.ApplicationStarted -= this.OnApplicationStarted;
            this.ClientApplication.ApplicationStopped -= this.OnApplicationStopped;
            
            this._connectionManager?.Dispose();
            this._statsService?.Dispose();
            PluginLog.Info("Plugin unloaded");
        }

        private async void OnApplicationStarted(Object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                PluginLog.Info("OBS application started");
                await this._connectionManager.ConnectAsync();
            });
        }

        private void OnApplicationStopped(Object sender, EventArgs e)
        {
            PluginLog.Info("OBS application stopped");
            this._commandCoordinator.NotifyDisconnected();
            this._connectionManager.Disconnect();
        }

        private void OnOBSConnected(Object sender, EventArgs e)
        {
            PluginLog.Info("OBS WebSocket connected");
            this.ReportPluginStatus(Loupedeck.PluginStatus.Normal, null);
            this._statsService.Start();
            this._commandCoordinator.NotifyConnected();
            ConnectionStatusDisplay.Instance?.UpdateStatus();
        }

        private void OnOBSDisconnected(Object sender, EventArgs e)
        {
            PluginLog.Info("OBS WebSocket disconnected");
            this.ReportPluginStatus(Loupedeck.PluginStatus.Warning, "OBS is offline. Please launch OBS");
            this._statsService.Stop();
            this._commandCoordinator.NotifyDisconnected();
            ConnectionStatusDisplay.Instance?.UpdateStatus();
        }

        private void OnWebSocketServerDisabled(Object sender, EventArgs e)
        {
            PluginLog.Warning("OBS WebSocket server is disabled");
            this.ReportPluginStatus(Loupedeck.PluginStatus.Warning, "OBS WebSocket server is disabled. Enable it in OBS Tools menu.");
            ConnectionStatusDisplay.Instance?.UpdateStatus();
        }

        private void ReportPluginStatus(Loupedeck.PluginStatus status, String message)
        {
            // OnPluginStatusChanged redraws devices synchronously on the caller's thread, and that
            // redraw can race the service's own page updates; a failure there must not take down
            // the connection handler that reported the status.
            try
            {
                this.OnPluginStatusChanged(status, message);
            }
            catch (Exception ex)
            {
                PluginLog.Warning(ex, $"Failed to report plugin status {status}");
            }
        }

        public void RegisterCommand(IObsCommand command)
        {
            this._commandCoordinator.RegisterCommand(command);
        }



        public void SwitchScene(String sceneName)
        {
            this._obsFacade.SwitchScene(sceneName);
        }

        public void SaveScreenshot()
        {
            this._obsFacade.SaveScreenshot(ScreenshotPath);
        }

        public void ToggleRecording()
        {
            this._obsFacade.ToggleRecording();
        }

        public void StartRecording()
        {
            this._obsFacade.StartRecording();
        }

        public void StopRecording()
        {
            this._obsFacade.StopRecording();
        }

        public void ToggleRecordingPause()
        {
            this._obsFacade.ToggleRecordingPause();
        }

        public void ToggleStreaming()
        {
            this._obsFacade.ToggleStreaming();
        }

        public void StartStreaming()
        {
            this._obsFacade.StartStreaming();
        }

        public void StopStreaming()
        {
            this._obsFacade.StopStreaming();
        }

        public String[] GetProfileList()
        {
            return this._obsFacade.GetProfileList();
        }

        public void SwitchProfile(String profileName)
        {
            this._obsFacade.SwitchProfile(profileName);
        }

        public String CurrentProfile => this._obsFacade.CurrentProfile;

        public void OnProfileChanged(String oldProfile, String newProfile)
        {
            PluginLog.Debug($"Plugin notified of profile change: '{oldProfile}' -> '{newProfile}'");
            this._commandCoordinator.NotifyProfileChanged(oldProfile, newProfile);
        }

        public String[] GetSceneCollectionList()
        {
            return this._obsFacade.GetSceneCollectionList();
        }

        public void SwitchSceneCollection(String sceneCollectionName)
        {
            this._obsFacade.SwitchSceneCollection(sceneCollectionName);
        }

        public String CurrentSceneCollection => this._obsFacade.CurrentSceneCollection;

        public String[] GetSceneList()
        {
            return this._obsFacade.GetSceneList();
        }

        public String GetCurrentScene()
        {
            return this._obsFacade.CurrentScene;
        }

        public void OnSceneCollectionChanged(String oldSceneCollection, String newSceneCollection)
        {
            PluginLog.Debug($"Plugin notified of scene collection change: '{oldSceneCollection}' -> '{newSceneCollection}'");
            this._commandCoordinator.NotifySceneCollectionChanged(oldSceneCollection, newSceneCollection);
        }

        public void OnScenesChanged(String[] scenes)
        {
            this._commandCoordinator.NotifyScenesChanged(scenes);
        }

        public void OnProfilesChanged(String[] profiles, String currentProfile)
        {
            this._commandCoordinator.NotifyProfilesChanged(profiles, currentProfile);
        }

        public void OnSceneCollectionsChanged(String[] sceneCollections, String currentSceneCollection)
        {
            this._commandCoordinator.NotifySceneCollectionsChanged(sceneCollections, currentSceneCollection);
        }

        public void OnCurrentSceneChanged(String sceneName)
        {
            PluginLog.Debug($"Plugin notified of scene change: '{sceneName}'");
            this._commandCoordinator.NotifySceneChanged(sceneName);
            this._obsFacade.UpdateSourcesForScene(sceneName,
                (scene, sources, audioSources) => this.OnSceneSourcesChanged(scene, sources, audioSources));
        }

        public void OnSceneSourcesChanged(String sceneName, String[] sources, String[] audioSources)
        {
            this._commandCoordinator.NotifySceneSourcesChanged(sceneName, sources, audioSources);
        }

        // Scene source lists are only fetched while a folder that shows them is open. Returns true
        // if the current scene's lists started loading, so the folder can show its loading tiles.
        public Boolean OpenSceneSources(String owner)
        {
            return this._obsFacade.AddSceneSourcesViewer(owner,
                (scene, sources, audioSources) => this.OnSceneSourcesChanged(scene, sources, audioSources));
        }

        public void CloseSceneSources(String owner)
        {
            this._obsFacade.RemoveSceneSourcesViewer(owner);
        }



        public Boolean GetSourceVisibility(String sceneName, String sourceName)
        {
            return this._obsFacade.GetSourceVisibility(sceneName, sourceName);
        }

        public void ToggleSourceVisibility(String sceneName, String sourceName)
        {
            this._obsFacade.ToggleSourceVisibility(sceneName, sourceName);
        }

        public void ManualReconnect()
        {
            PluginLog.Info("Manual reconnect requested");
            Task.Run(() => this._connectionManager.ReconnectAsync());
        }

        public Models.OBSStats GetStats()
        {
            return this._obsFacade.GetStats();
        }

        public Models.OBSStreamStats GetStreamStatus()
        {
            return this._obsFacade.GetStreamStatus();
        }

        public String GetMediaInputStatus(String inputName)
        {
            return this._obsFacade.GetMediaInputStatus(inputName);
        }

        public void TriggerMediaInputAction(String inputName, String mediaAction)
        {
            this._obsFacade.TriggerMediaInputAction(inputName, mediaAction);
        }

        public void ToggleMediaInputPlayback(String inputName)
        {
            this._obsFacade.ToggleMediaInputPlayback(inputName);
        }

        public String[] GetMediaInputList()
        {
            return this._obsFacade.GetMediaInputList();
        }

        public void OnMediaPlaybackStateChanged(String inputName)
        {
            PluginLog.Debug($"Media playback state changed for '{inputName}'");
            MediaDynamicFolder.Instance?.OnMediaStateChanged(inputName);
        }

        public Models.OBSStats GetCurrentStats()
        {
            return this._statsService.CurrentStats;
        }

        public Models.OBSStreamStats GetCurrentStreamStats()
        {
            return this._statsService.CurrentStreamStats;
        }

        // Stats are only polled while something that shows them is visible - see StatsService.
        public void AddStatsViewer(String owner)
        {
            this._statsService.AddViewer(owner);
        }

        public void RemoveStatsViewer(String owner)
        {
            this._statsService.RemoveViewer(owner);
        }

        private void OnStatsUpdated(Object sender, EventArgs e)
        {
            StatsDisplay.Instance?.UpdateDisplay();
            StatsDynamicFolder.Instance?.UpdateDisplay();
            StreamStatsDynamicFolder.Instance?.UpdateDisplay();
        }

        public Boolean IsRecording => this._obsFacade.IsRecording;
        public Boolean IsRecordingPaused => this._obsFacade.IsRecordingPaused;
        public Boolean IsStreaming => this._obsFacade.IsStreaming;
        public Boolean IsVirtualCameraActive => this._obsFacade.IsVirtualCameraActive;
        public Boolean IsReplayBufferActive => this._obsFacade.IsReplayBufferActive;
        public Boolean IsStudioModeEnabled => this._obsFacade.IsStudioModeEnabled;
        public Boolean IsConnected => this._obsFacade.IsConnected;
        public Boolean IsWebSocketServerDisabled => this._connectionManager?.IsWebSocketServerDisabled ?? false;

        public void ToggleVirtualCamera()
        {
            this._obsFacade.ToggleVirtualCamera();
        }

        public void StartVirtualCamera()
        {
            this._obsFacade.StartVirtualCamera();
        }

        public void StopVirtualCamera()
        {
            this._obsFacade.StopVirtualCamera();
        }

        public void OnVirtualCameraStateChanged()
        {
            this._commandCoordinator.NotifyVirtualCameraStateChanged();
        }

        public void ToggleReplayBuffer()
        {
            this._obsFacade.ToggleReplayBuffer();
        }

        public void StartReplayBuffer()
        {
            this._obsFacade.StartReplayBuffer();
        }

        public void StopReplayBuffer()
        {
            this._obsFacade.StopReplayBuffer();
        }

        public void SaveReplayBuffer()
        {
            this._obsFacade.SaveReplayBuffer();
        }

        public void OnReplayBufferStateChanged()
        {
            this._commandCoordinator.NotifyReplayBufferStateChanged();
        }

        public void OnReplayBufferSaved(String savedReplayPath)
        {
            this._commandCoordinator.NotifyReplayBufferSaved(savedReplayPath);
        }

        public String[] GetInputList()
        {
            return this._obsFacade.GetInputList();
        }

        public Boolean GetInputMute(String inputName)
        {
            return this._obsFacade.GetInputMute(inputName);
        }

        public void ToggleInputMute(String inputName)
        {
            this._obsFacade.ToggleInputMute(inputName);
        }

        public Single GetInputVolume(String inputName)
        {
            return this._obsFacade.GetInputVolume(inputName);
        }

        public void SetInputVolume(String inputName, Single volumeMul)
        {
            this._obsFacade.SetInputVolume(inputName, volumeMul);
        }

        public String GetInputAudioMonitorType(String inputName)
        {
            return this._obsFacade.GetInputAudioMonitorType(inputName);
        }

        public void CycleInputAudioMonitorType(String inputName)
        {
            this._obsFacade.CycleInputAudioMonitorType(inputName);
        }

        public void SubscribeToVolumeMeters(String owner)
        {
            this._obsFacade.SubscribeToVolumeMeters(owner);
        }

        public void UnsubscribeFromVolumeMeters(String owner)
        {
            this._obsFacade.UnsubscribeFromVolumeMeters(owner);
        }

        public AudioMeterLevels GetAudioMeterLevels(String inputName)
        {
            return this._obsFacade.GetAudioMeterLevels(inputName);
        }

        public String[] GetLiveAudioMeterInputs()
        {
            return this._obsFacade.GetLiveAudioMeterInputs();
        }

        public void OnInputsChanged(String[] inputs)
        {
            this._commandCoordinator.NotifyInputsChanged(inputs);
        }

        public void OnInputMuteChanged(String inputName)
        {
            this._commandCoordinator.NotifyInputMuteChanged(inputName);
        }

        public void OnInputVolumeChanged(String inputName)
        {
            this._commandCoordinator.NotifyInputVolumeChanged(inputName);
        }

        public void OnSourceVisibilityChanged(String sceneName, String sourceName)
        {
            this._commandCoordinator.NotifySourceVisibilityChanged(sceneName, sourceName);
        }

        public void OnInputMonitorTypeChanged(String inputName)
        {
            PluginLog.Debug($"Plugin notified of monitor type change for '{inputName}'");
            this._commandCoordinator.NotifyInputMonitorTypeChanged(inputName);
        }

        public void OnSceneItemsChanged(String sceneName)
        {
            var currentScene = this._obsFacade.CurrentScene;
            if (sceneName != currentScene)
            {
                PluginLog.Debug($"Plugin: scene items changed in '{sceneName}' but current scene is '{currentScene}', ignoring");
                return;
            }

            PluginLog.Debug($"Plugin notified of scene items change in '{sceneName}'");
            this._obsFacade.UpdateSourcesForScene(sceneName,
                (scene, sources, audioSources) => this.OnSceneSourcesChanged(scene, sources, audioSources));
        }

        public void ToggleStudioMode()
        {
            this._obsFacade.ToggleStudioMode();
        }

        public void OnStudioModeStateChanged()
        {
            this._commandCoordinator.NotifyStudioModeStateChanged();
        }

        public void TriggerStudioModeTransition()
        {
            this._obsFacade.TriggerStudioModeTransition();
        }

        // The SDK's plugin-settings methods are protected, so this nested class exposes them as an
        // ISecretStore. The Logi Plugin Service stores these values encrypted. They are not backed
        // up online: the only secret is a password for a machine on the user's own network.
        private sealed class PluginSettingsSecretStore : ISecretStore
        {
            private readonly OBSStudioForLogiPlugin _plugin;

            public PluginSettingsSecretStore(OBSStudioForLogiPlugin plugin)
            {
                this._plugin = plugin;
            }

            public Boolean TryGet(String name, out String value) => this._plugin.TryGetPluginSetting(name, out value);

            public void Set(String name, String value) => this._plugin.SetPluginSetting(name, value, false);

            public void Delete(String name) => this._plugin.DeletePluginSetting(name);
        }
    }
}
