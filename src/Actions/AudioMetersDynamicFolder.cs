namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Timers;
    using Loupedeck.OBSStudioForLogiPlugin.Models;
    using Loupedeck.OBSStudioForLogiPlugin.Services;

    public class AudioMetersDynamicFolder : PluginDynamicFolder, IObsCommand, IInputsListAwareCommand
    {
        public static AudioMetersDynamicFolder Instance { get; private set; }

        private String[] _audioInputs = new String[0];
        private readonly Timer _refreshTimer = new Timer();

        public AudioMetersDynamicFolder()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.DisplayName = "Audio Meters";
            this.GroupName = "8. Audio###Meters";
            this.Description = "Real-time volume meters for audio inputs";

            this._refreshTimer.Elapsed += this.OnRefreshTimer;
            this._refreshTimer.AutoReset = true;
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
            return this._audioInputs.Select(input => this.CreateCommandName(input));
        }

        // InputVolumeMeters is a high-volume event - only subscribe while this folder is actually
        // visible. Activate/Deactivate fire on first-instance-open/last-instance-close respectively.
        public override Boolean Activate()
        {
            PluginLog.Info("AudioMetersDynamicFolder activated - subscribing to volume meters");
            OBSStudioForLogiPlugin.Instance?.SubscribeToVolumeMeters();

            Int32 refreshMs = new PluginConfigReader().ReadConfig()?.AudioMeterRefreshInterval ?? 100;
            this._refreshTimer.Interval = refreshMs;
            this._refreshTimer.Start();
            return true;
        }

        public override Boolean Deactivate()
        {
            PluginLog.Info("AudioMetersDynamicFolder deactivated - unsubscribing from volume meters");
            this._refreshTimer.Stop();
            OBSStudioForLogiPlugin.Instance?.UnsubscribeFromVolumeMeters();
            return true;
        }

        private void OnRefreshTimer(Object sender, ElapsedEventArgs e)
        {
            foreach (var input in this._audioInputs)
            {
                this.CommandImageChanged(input);
            }
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var levels = OBSStudioForLogiPlugin.Instance?.GetAudioMeterLevels(actionParameter) ?? AudioMeterLevels.Empty;
            return VuMeterRenderer.Render(levels.ChannelPeaks, imageSize);
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.ToggleInputMute(actionParameter);
        }

        public void OnConnected()
        {
            this._audioInputs = OBSStudioForLogiPlugin.Instance?.GetInputList() ?? new String[0];
            this.ButtonActionNamesChanged();
        }

        public void OnDisconnected()
        {
            this._audioInputs = new String[0];
            this.ButtonActionNamesChanged();
        }

        public void OnInputsChanged(String[] inputs)
        {
            this._audioInputs = inputs ?? new String[0];
            this.ButtonActionNamesChanged();
        }
    }
}
