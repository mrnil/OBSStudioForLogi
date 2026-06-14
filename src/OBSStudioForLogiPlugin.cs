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
        private OBSWebSocketManager _obsManager;
        public static String ScreenshotPath { get; private set; }
        
        public override Boolean UsesApplicationApiOnly => true;
        public override Boolean HasNoApplication => true;

        public OBSStudioForLogiPlugin()
        {
            Instance = this;
            PluginLog.Init(this.Log);
            PluginResources.Init(this.Assembly);
            var pluginConfig = LoadPluginConfiguration();
            DiscoverScreenshotPath();
            
            this._obsManager = new OBSWebSocketManager();
            this._obsConfigReader = new OBSConfigReader();
            this._connectionManager = new ConnectionManager(this._obsManager, this._obsConfigReader, new OBSLifecycleManager());
            this._connectionManager.SetPluginConfig(pluginConfig);
            this._commandCoordinator = new CommandCoordinator();
            this._obsFacade = new OBSFacade(this._obsManager);
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

        public void ApplyConnectionConfig(PluginConfig config)
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
            
            this.Info.Icon256x256 = EmbeddedResources.ReadImage("Loupedeck.OBSStudioForLogiPlugin.metadata.Icon256x256.png");

            // Subscribe to connection events for status reporting
            this._obsManager.ConnectionEstablished += this.OnOBSConnected;
            this._obsManager.ConnectionLost += this.OnOBSDisconnected;

            this.ClientApplication.ApplicationStarted += this.OnApplicationStarted;
            this.ClientApplication.ApplicationStopped += this.OnApplicationStopped;
            
            if (this.ClientApplication.IsRunning())
            {
                PluginLog.Info("OBS detected via ClientApplication");
                this.OnPluginStatusChanged(Loupedeck.PluginStatus.Normal, null);                
                this.OnApplicationStarted(this, EventArgs.Empty);
            }
            else
            {
                PluginLog.Info("OBS not detected, attempting direct connection");
                this.OnPluginStatusChanged(Loupedeck.PluginStatus.Warning,"OBS is offline. Please launch OBS");
                Task.Run(() => this._connectionManager.ConnectAsync());
            }
            
            PluginLog.Info("Plugin loaded");
        }

        public override void Unload()
        {
            PluginLog.Info("Plugin unloading...");
            
            // Unsubscribe from connection events
            this._obsManager.ConnectionEstablished -= this.OnOBSConnected;
            this._obsManager.ConnectionLost -= this.OnOBSDisconnected;
            
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
            this.OnPluginStatusChanged(Loupedeck.PluginStatus.Normal, null);
            this._statsService.Start();
        }

        private void OnOBSDisconnected(Object sender, EventArgs e)
        {
            PluginLog.Info("OBS WebSocket disconnected");
            this.OnPluginStatusChanged(Loupedeck.PluginStatus.Warning,"OBS is offline. Please launch OBS");
            this._statsService.Stop();
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
            PluginLog.Info($"Plugin notified of profile change: '{oldProfile}' -> '{newProfile}'");
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
            PluginLog.Info($"Plugin notified of scene collection change: '{oldSceneCollection}' -> '{newSceneCollection}'");
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

        public void OnCurrentSceneChanged(String sceneName)
        {
            PluginLog.Info($"Plugin notified of scene change: '{sceneName}'");
            this._commandCoordinator.NotifySceneChanged(sceneName);
            this._obsFacade.UpdateSourcesForScene(sceneName, 
                (scene, sources) => SourcesDynamicFolder.Instance?.UpdateSources(scene, sources),
                (scene, audioSources) => SceneAudioSourcesDynamicFolder.Instance?.UpdateAudioSources(scene, audioSources));
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
            Task.Run(() => this._connectionManager.ConnectAsync());
        }

        public Models.OBSStats GetStats()
        {
            return this._obsFacade.GetStats();
        }

        public Models.OBSStats GetCurrentStats()
        {
            return this._statsService.CurrentStats;
        }

        private void OnStatsUpdated(Object sender, EventArgs e)
        {
            StatsDisplay.Instance?.UpdateDisplay();
            StatsDynamicFolder.Instance?.UpdateDisplay();
        }

        public Boolean IsRecording => this._obsFacade.IsRecording;
        public Boolean IsRecordingPaused => this._obsFacade.IsRecordingPaused;
        public Boolean IsStreaming => this._obsFacade.IsStreaming;
        public Boolean IsVirtualCameraActive => this._obsFacade.IsVirtualCameraActive;
        public Boolean IsReplayBufferActive => this._obsFacade.IsReplayBufferActive;
        public Boolean IsStudioModeEnabled => this._obsFacade.IsStudioModeEnabled;
        public Boolean IsConnected => this._obsFacade.IsConnected;

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

        public String[] GetInputList()
        {
            return this._obsFacade.GetInputList();
        }

        public String GetInputKind(String inputName)
        {
            return this._obsFacade.GetInputKind(inputName);
        }

        public String[] GetScenesForInput(String inputName)
        {
            return this._obsFacade.GetScenesForInput(inputName);
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
            PluginLog.Info($"Plugin: Setting input volume for '{inputName}' to {(Int32)(volumeMul * 100)}%");
            this._obsFacade.SetInputVolume(inputName, volumeMul);
        }

        public String GetInputAudioMonitorType(String inputName)
        {
            return this._obsFacade.GetInputAudioMonitorType(inputName);
        }

        public void CycleInputAudioMonitorType(String inputName)
        {
            PluginLog.Info($"Plugin: Cycling audio monitoring for '{inputName}'");
            this._obsFacade.CycleInputAudioMonitorType(inputName);
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
            PluginLog.Info($"Plugin notified of monitor type change for '{inputName}'");
            this._commandCoordinator.NotifyInputMonitorTypeChanged(inputName);
        }

        public void OnSceneItemsChanged(String sceneName)
        {
            var currentScene = this._obsFacade.CurrentScene;
            if (sceneName != currentScene)
            {
                PluginLog.Info($"Plugin: scene items changed in '{sceneName}' but current scene is '{currentScene}', ignoring");
                return;
            }

            PluginLog.Info($"Plugin notified of scene items change in '{sceneName}'");
            this._obsFacade.UpdateSourcesForScene(sceneName,
                (scene, sources) => SourcesDynamicFolder.Instance?.UpdateSources(scene, sources),
                (scene, audioSources) => SceneAudioSourcesDynamicFolder.Instance?.UpdateAudioSources(scene, audioSources));
        }

        public void OnInputListChanged()
        {
            PluginLog.Info("Plugin notified of input list change");
            var inputs = this._obsFacade.GetInputList();
            this._commandCoordinator.NotifyInputsChanged(inputs);
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
    }
}
