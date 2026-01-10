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
                PluginLog.Info("OBS is already running");
                this.OnApplicationStarted(this, EventArgs.Empty);
            }
            else
            {
                PluginLog.Info("OBS is not running, waiting for start");
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
            this._obsManager?.Disconnect();
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

        public void PauseRecording()
        {
            this._obsManager?.Actions.PauseRecording();
        }

        public void ResumeRecording()
        {
            this._obsManager?.Actions.ResumeRecording();
        }

        public Boolean IsRecording => this._obsManager?.IsRecording ?? false;
    }
}
