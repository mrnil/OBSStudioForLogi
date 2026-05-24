namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading.Tasks;

    public class OBSStudioForLogiPlugin : Plugin
    {
        public static OBSStudioForLogiPlugin Instance { get; private set; }
        private readonly CommandRegistry _commandRegistry = new CommandRegistry();
        private OBSWebSocketManager _obsManager;
        private OBSConfigReader _configReader;
        private OBSLifecycleManager _lifecycleManager;
        public static String ScreenshotPath { get; private set; }
        
        public override Boolean UsesApplicationApiOnly => true;
        public override Boolean HasNoApplication => true;

        public OBSStudioForLogiPlugin()
        {
            Instance = this;
            PluginLog.Init(this.Log);
            PluginResources.Init(this.Assembly);
            DiscoverScreenshotPath();
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
            
            this._configReader = new OBSConfigReader();
            this._obsManager = new OBSWebSocketManager();
            this._lifecycleManager = new OBSLifecycleManager();

            this.ClientApplication.ApplicationStarted += this.OnApplicationStarted;
            this.ClientApplication.ApplicationStopped += this.OnApplicationStopped;
            
            if (this.ClientApplication.IsRunning())
            {
                PluginLog.Info("OBS detected via ClientApplication");
                this.OnApplicationStarted(this, EventArgs.Empty);
            }
            else
            {
                PluginLog.Info("OBS not detected, attempting direct connection");
                Task.Run(() => this.TryDirectConnection());
            }
            
            PluginLog.Info("Plugin loaded");
        }

        public override void Unload()
        {
            PluginLog.Info("Plugin unloading...");
            
            this.ClientApplication.ApplicationStarted -= this.OnApplicationStarted;
            this.ClientApplication.ApplicationStopped -= this.OnApplicationStopped;
            
            this._obsManager?.Dispose();
            PluginLog.Info("Plugin unloaded");
        }

        private async void OnApplicationStarted(Object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                PluginLog.Info("OBS application started");
                
                var settings = this._configReader.ReadConfig();
                if (settings == null)
                {
                    PluginLog.Warning("No valid OBS configuration found");
                    return;
                }

                PluginLog.Info($"Waiting for OBS WebSocket port {settings.Port} to be ready");
                var portReady = await this._lifecycleManager.WaitForPortAsync("127.0.0.1", settings.Port);
                
                if (portReady)
                {
                    await Task.Delay(2000);
                    PluginLog.Info("Initiating connection to OBS");
                    await this._obsManager.ConnectAsync(settings.GetWebSocketUrl(), settings.Password);
                }
                else
                {
                    PluginLog.Error("OBS WebSocket port did not become available");
                }
            });
        }

        private void OnApplicationStopped(Object sender, EventArgs e)
        {
            PluginLog.Info("OBS application stopped");
            this._commandRegistry.NotifyDisconnected();
            this._obsManager?.Disconnect();
        }

        public void RegisterCommand(IObsCommand command)
        {
            this._commandRegistry.Register(command);
        }

        private async void TryDirectConnection()
        {
            PluginLog.Info("Attempting direct connection to OBS");
            
            var settings = this._configReader.ReadConfig();
            if (settings == null)
            {
                PluginLog.Warning("No valid OBS configuration found for direct connection");
                return;
            }

            PluginLog.Info($"Waiting for OBS WebSocket port {settings.Port} to be ready");
            var portReady = await this._lifecycleManager.WaitForPortAsync("127.0.0.1", settings.Port);
            
            if (portReady)
            {
                await Task.Delay(2000);
                PluginLog.Info("Initiating direct connection to OBS");
                await this._obsManager.ConnectAsync(settings.GetWebSocketUrl(), settings.Password);
            }
            else
            {
                PluginLog.Error("OBS WebSocket port did not become available for direct connection");
            }
        }

        public void SwitchScene(String sceneName)
        {
            if (!this._obsManager.IsConnected)
            {
                PluginLog.Warning($"Cannot switch to scene '{sceneName}' - not connected to OBS");
                return;
            }

            this._obsManager.Actions.SetCurrentScene(sceneName);
        }

        public void SaveScreenshot()
        {
            this._obsManager?.Actions.SaveScreenshot(ScreenshotPath);
        }

        public void ToggleRecording()
        {
            this._obsManager?.Actions.ToggleRecording();
        }

        public void StartRecording()
        {
            this._obsManager?.Actions.StartRecording();
        }

        public void StopRecording()
        {
            this._obsManager?.Actions.StopRecording();
        }

        public void ToggleRecordingPause()
        {
            this._obsManager?.Actions.ToggleRecordingPause();
        }

        public void ToggleStreaming()
        {
            this._obsManager?.Actions.ToggleStreaming();
        }

        public void StartStreaming()
        {
            this._obsManager?.Actions.StartStreaming();
        }

        public void StopStreaming()
        {
            this._obsManager?.Actions.StopStreaming();
        }

        public String[] GetProfileList()
        {
            return this._obsManager?.Actions.GetProfileList() ?? new String[0];
        }

        public void SwitchProfile(String profileName)
        {
            if (!this._obsManager.IsConnected)
            {
                PluginLog.Warning($"Cannot switch to profile '{profileName}' - not connected to OBS");
                return;
            }

            this._obsManager.Actions.SetCurrentProfile(profileName);
        }

        public String CurrentProfile => this._obsManager?.Actions.CurrentProfile ?? String.Empty;

        public void OnProfileChanged(String oldProfile, String newProfile)
        {
            PluginLog.Info($"Plugin notified of profile change: '{oldProfile}' -> '{newProfile}'");
            this._commandRegistry.NotifyProfileChanged(oldProfile, newProfile);
        }

        public String[] GetSceneCollectionList()
        {
            return this._obsManager?.Actions.GetSceneCollectionList() ?? new String[0];
        }

        public void SwitchSceneCollection(String sceneCollectionName)
        {
            if (!this._obsManager.IsConnected)
            {
                PluginLog.Warning($"Cannot switch to scene collection '{sceneCollectionName}' - not connected to OBS");
                return;
            }

            this._obsManager.Actions.SetCurrentSceneCollection(sceneCollectionName);
        }

        public String CurrentSceneCollection => this._obsManager?.Actions.CurrentSceneCollection ?? String.Empty;

        public String[] GetSceneList()
        {
            return this._obsManager?.Actions.GetSceneList() ?? new String[0];
        }

        public String GetCurrentScene()
        {
            return this._obsManager?.Actions.CurrentScene ?? String.Empty;
        }

        public void OnSceneCollectionChanged(String oldSceneCollection, String newSceneCollection)
        {
            PluginLog.Info($"Plugin notified of scene collection change: '{oldSceneCollection}' -> '{newSceneCollection}'");
            this._commandRegistry.NotifySceneCollectionChanged(oldSceneCollection, newSceneCollection);
        }

        public void OnScenesChanged(String[] scenes)
        {
            this._commandRegistry.NotifyScenesChanged(scenes);
        }

        public void OnProfilesChanged(String[] profiles, String currentProfile)
        {
            this._commandRegistry.NotifyProfilesChanged(profiles, currentProfile);
        }

        public void OnCurrentSceneChanged(String sceneName)
        {
            PluginLog.Info($"Plugin notified of scene change: '{sceneName}'");
            this._commandRegistry.NotifySceneChanged(sceneName);
            this.UpdateSourcesForScene(sceneName);
        }

        private void UpdateSourcesForScene(String sceneName)
        {
            if (String.IsNullOrEmpty(sceneName))
            {
                PluginLog.Warning("Cannot update sources - scene name is empty");
                return;
            }

            var sources = this._obsManager?.Actions.GetSceneItemList(sceneName) ?? new String[0];
            SourcesDynamicFolder.Instance?.UpdateSources(sceneName, sources);
            
            var audioSourcesInScene = this._obsManager?.Actions.GetAudioSourcesInScene(sceneName) ?? new String[0];
            var audioInputsNotInAnyScene = this._obsManager?.Actions.GetAudioInputsNotInAnyScene() ?? new String[0];
            var allSceneAudioSources = audioSourcesInScene.Concat(audioInputsNotInAnyScene).ToArray();
            
            SceneAudioSourcesDynamicFolder.Instance?.UpdateAudioSources(sceneName, allSceneAudioSources);
        }

        public Boolean GetSourceVisibility(String sceneName, String sourceName)
        {
            return this._obsManager?.Actions.GetSceneItemEnabled(sceneName, sourceName) ?? false;
        }

        public void ToggleSourceVisibility(String sceneName, String sourceName)
        {
            this._obsManager?.Actions.ToggleSourceVisibility(sceneName, sourceName);
        }

        public void ManualReconnect()
        {
            PluginLog.Info("Manual reconnect requested");
            Task.Run(() => this.TryDirectConnection());
        }

        public Boolean IsRecording => this._obsManager?.IsRecording ?? false;
        public Boolean IsRecordingPaused => this._obsManager?.Actions.IsRecordingPaused ?? false;
        public Boolean IsStreaming => this._obsManager?.IsStreaming ?? false;
        public Boolean IsVirtualCameraActive => this._obsManager?.Actions.IsVirtualCameraActive ?? false;
        public Boolean IsReplayBufferActive => this._obsManager?.Actions.IsReplayBufferActive ?? false;
        public Boolean IsStudioModeEnabled => this._obsManager?.Actions.IsStudioModeEnabled ?? false;
        public Boolean IsConnected => this._obsManager?.IsConnected ?? false;

        public void ToggleVirtualCamera()
        {
            this._obsManager?.Actions.ToggleVirtualCamera();
        }

        public void StartVirtualCamera()
        {
            this._obsManager?.Actions.StartVirtualCamera();
        }

        public void StopVirtualCamera()
        {
            this._obsManager?.Actions.StopVirtualCamera();
        }

        public void OnVirtualCameraStateChanged()
        {
            this._commandRegistry.NotifyVirtualCameraStateChanged();
        }

        public void ToggleReplayBuffer()
        {
            this._obsManager?.Actions.ToggleReplayBuffer();
        }

        public void StartReplayBuffer()
        {
            this._obsManager?.Actions.StartReplayBuffer();
        }

        public void StopReplayBuffer()
        {
            this._obsManager?.Actions.StopReplayBuffer();
        }

        public void SaveReplayBuffer()
        {
            this._obsManager?.Actions.SaveReplayBuffer();
        }

        public void OnReplayBufferStateChanged()
        {
            this._commandRegistry.NotifyReplayBufferStateChanged();
        }

        public String[] GetInputList()
        {
            return this._obsManager?.Actions.GetInputList() ?? new String[0];
        }

        public String GetInputKind(String inputName)
        {
            return this._obsManager?.Actions.GetInputKind(inputName) ?? String.Empty;
        }

        public String[] GetScenesForInput(String inputName)
        {
            return this._obsManager?.Actions.GetScenesForInput(inputName) ?? new String[0];
        }

        public Boolean GetInputMute(String inputName)
        {
            return this._obsManager?.Actions.GetInputMute(inputName) ?? false;
        }

        public void ToggleInputMute(String inputName)
        {
            this._obsManager?.Actions.ToggleInputMute(inputName);
        }

        public Single GetInputVolume(String inputName)
        {
            return this._obsManager?.Actions.GetInputVolume(inputName) ?? 1.0f;
        }

        public void OnInputsChanged(String[] inputs)
        {
            this._commandRegistry.NotifyInputsChanged(inputs);
        }

        public void OnInputMuteChanged(String inputName)
        {
            this._commandRegistry.NotifyInputMuteChanged(inputName);
        }

        public void OnInputVolumeChanged(String inputName)
        {
            this._commandRegistry.NotifyInputVolumeChanged(inputName);
        }

        public void OnSourceVisibilityChanged(String sceneName, String sourceName)
        {
            this._commandRegistry.NotifySourceVisibilityChanged(sceneName, sourceName);
        }

        public void ToggleStudioMode()
        {
            this._obsManager?.Actions.ToggleStudioMode();
        }

        public void OnStudioModeStateChanged()
        {
            this._commandRegistry.NotifyStudioModeStateChanged();
        }

        public void TriggerStudioModeTransition()
        {
            this._obsManager?.Actions.TriggerStudioModeTransition();
        }
    }
}
