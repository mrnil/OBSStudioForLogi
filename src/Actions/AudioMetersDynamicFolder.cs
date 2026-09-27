namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Timers;
    using Loupedeck.OBSStudioForLogiPlugin.Models;
    using Loupedeck.OBSStudioForLogiPlugin.Services;

    // Shows only "live" inputs - the ones OBS is currently reporting audio levels for. OBS decides
    // that itself (inputs active on program output, including global audio devices, nested scenes
    // and groups), so scene switches, source visibility toggles, renames and input add/remove are
    // all picked up from the meter data on the next refresh tick without tracking scene items here.
    public class AudioMetersDynamicFolder : PluginDynamicFolder, IObsCommand
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
            this.Description = "Real-time volume meters for live audio inputs";

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
            this.SetInputs(new String[0]);
            return true;
        }

        private void OnRefreshTimer(Object sender, ElapsedEventArgs e)
        {
            String[] liveInputs = OBSStudioForLogiPlugin.Instance?.GetLiveAudioMeterInputs() ?? new String[0];
            this.SetInputs(liveInputs);

            foreach (String input in this._audioInputs)
            {
                this.CommandImageChanged(input);
            }
        }

        // The live list is kept in stable first-seen order by AudioMeterService, so a plain
        // sequence comparison only triggers a (comparatively expensive) button rebuild when an
        // input actually appears or drops out.
        private void SetInputs(String[] inputs)
        {
            if (inputs.SequenceEqual(this._audioInputs))
            {
                return;
            }

            PluginLog.Debug($"AudioMetersDynamicFolder live inputs changed: [{String.Join(", ", inputs)}]");
            this._audioInputs = inputs;
            this.ButtonActionNamesChanged();
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            AudioMeterLevels levels = OBSStudioForLogiPlugin.Instance?.GetAudioMeterLevels(actionParameter) ?? AudioMeterLevels.Empty;
            return VuMeterRenderer.Render(levels, imageSize);
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.ToggleInputMute(actionParameter);
        }

        public void OnConnected()
        {
            // Nothing to load - the input list fills from meter data once the folder is open.
        }

        public void OnDisconnected()
        {
            this.SetInputs(new String[0]);
        }
    }
}
