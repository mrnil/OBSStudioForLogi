namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Loupedeck.OBSStudioForLogiPlugin.Services;

    public class AudioMetersDynamicFolder : PluginDynamicFolder, IObsCommand, IInputsListAwareCommand, ISceneAwareCommand
    {
        public static AudioMetersDynamicFolder Instance { get; private set; }

        private String[] _audioInputs = new String[0];
        private Boolean _folderOpen = false;
        private DateTime _lastOpenTime = DateTime.MinValue;

        public AudioMetersDynamicFolder()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.DisplayName = "Audio Meters";
            this.GroupName = "8. Audio";
            this.Description = "Real-time VU meters for all audio inputs";
        }

        public override PluginDynamicFolderNavigation GetNavigationArea(DeviceType _)
        {
            return PluginDynamicFolderNavigation.ButtonArea;
        }

        public override BitmapImage GetButtonImage(PluginImageSize imageSize)
        {
            return ButtonImageHelper.Icon("AudioMediaFolder.svg");
        }

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType)
        {
            // Called when folder opens
            this._folderOpen = true;
            this._lastOpenTime = DateTime.UtcNow;
            OBSStudioForLogiPlugin.Instance?.StartMetering();
            return this._audioInputs.Select(this.CreateCommandName);
        }

        public override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return String.Empty;
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var meterService = OBSStudioForLogiPlugin.Instance?.GetMeterService();
            if (meterService == null)
            {
                return VuMeterRenderer.Render(0f, 0f, actionParameter, imageSize);
            }

            var (peakL, peakR) = meterService.GetLevels(actionParameter);
            PluginLog.Trace($"[Meters] {actionParameter}: L={peakL:F3} R={peakR:F3}");
            return VuMeterRenderer.Render(peakL, peakR, actionParameter, imageSize);
        }

        public override void RunCommand(String actionParameter)
        {
            // Display only — no action on tap
        }

        public void OnConnected()
        {
            this._audioInputs = OBSStudioForLogiPlugin.Instance?.GetInputList() ?? new String[0];
            this.ButtonActionNamesChanged();
        }

        public void OnDisconnected()
        {
            this.StopMetering();
            this._audioInputs = new String[0];
            this.ButtonActionNamesChanged();
        }

        public void OnInputsChanged(String[] inputs)
        {
            this._audioInputs = inputs ?? new String[0];
            this.ButtonActionNamesChanged();
        }

        public void OnSceneChanged(String sceneName)
        {
            // Refresh input list in case scene change brings new audio sources
            this._audioInputs = OBSStudioForLogiPlugin.Instance?.GetInputList() ?? new String[0];
            this.ButtonActionNamesChanged();
        }

        public void RefreshMeters()
        {
            if (!this._folderOpen)
                return;

            // Auto-stop after 60 seconds as safety (folder should call StopMetering explicitly)
            if ((DateTime.UtcNow - this._lastOpenTime).TotalSeconds > 60)
            {
                this.StopMetering();
                return;
            }

            foreach (var input in this._audioInputs)
            {
                this.CommandImageChanged(input);
            }
        }

        public void StopMetering()
        {
            this._folderOpen = false;
            OBSStudioForLogiPlugin.Instance?.StopMetering();
        }
    }
}
