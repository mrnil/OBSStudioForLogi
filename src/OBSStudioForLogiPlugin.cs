namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading.Tasks;

    public class OBSStudioForLogiPlugin : Plugin
    {
        public static OBSStudioForLogiPlugin Instance { get; private set; }
        private OBSWebSocketManager _obsManager;
        private OBSConfigReader _configReader;
        private OBSLifecycleManager _lifecycleManager;

        public override Boolean UsesApplicationApiOnly => true;
        public override Boolean HasNoApplication => false;

        public OBSStudioForLogiPlugin()
        {
            Instance = this;
            PluginLog.Init(this.Log);
            PluginResources.Init(this.Assembly);
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
                    
                    await Task.Delay(500);
                    ProfileSelectCommand.Instance?.OnConnected();
                    SceneCollectionSelectCommand.Instance?.OnConnected();
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
            ProfileSelectCommand.Instance?.OnDisconnected();
            SceneCollectionSelectCommand.Instance?.OnDisconnected();
            this._obsManager?.Disconnect();
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
                
                await Task.Delay(500);
                ProfileSelectCommand.Instance?.OnConnected();
                SceneCollectionSelectCommand.Instance?.OnConnected();
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
            ProfileSelectCommand.Instance?.OnCurrentProfileChanged(oldProfile, newProfile);
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

        public void OnSceneCollectionChanged(String oldSceneCollection, String newSceneCollection)
        {
            SceneCollectionSelectCommand.Instance?.OnCurrentSceneCollectionChanged(oldSceneCollection, newSceneCollection);
        }

        public Boolean IsRecording => this._obsManager?.IsRecording ?? false;
        public Boolean IsRecordingPaused => this._obsManager?.Actions.IsRecordingPaused ?? false;
    }
}
