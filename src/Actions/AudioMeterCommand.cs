namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Linq;
    using System.Timers;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;
    using Loupedeck.OBSStudioForLogiPlugin.Models;
    using Loupedeck.OBSStudioForLogiPlugin.Services;

    // One meter action per audio input, whether or not it is currently live, so a user can put a
    // meter for a specific source on any button. Unlike the live audio folder, a plain command gets
    // no Activate/Deactivate from the SDK, so visibility is inferred instead: the SDK only asks for
    // images of buttons it is showing, so each image request renews a lease that keeps the
    // InputVolumeMeters subscription alive, and the refresh timer drops the subscription once
    // requests stop. While idle, images are still invalidated about once a second so a button that
    // comes into view asks to be redrawn and renews the lease.
    public class AudioMeterCommand : PluginDynamicCommand, IObsCommand, IInputsListAwareCommand, IInputMuteAwareCommand
    {
        private const String SubscriptionOwner = nameof(AudioMeterCommand);

        public static AudioMeterCommand Instance { get; private set; }

        private String[] _audioInputs = new String[0];
        private readonly Timer _refreshTimer = new Timer();
        private readonly ActivityLease _renderLease = new ActivityLease(TimeSpan.FromMilliseconds(OBSTimings.AudioMeterRenderLease));
        private Int32 _idleTicksPerProbe = 1;
        private Int32 _idleTicks = 0;

        public AudioMeterCommand()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.DisplayName = "Audio Meter";
            this.Description = "Real-time volume meter for a single audio input";
            this.GroupName = "8. Audio###Meters";

            this._refreshTimer.Elapsed += this.OnRefreshTimer;
            this._refreshTimer.AutoReset = true;
        }

        protected override Boolean OnLoad()
        {
            this.IsEnabled = false;
            this.ResetParameters();
            return true;
        }

        protected override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.ToggleInputMute(actionParameter);
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            if (this._renderLease.Touch())
            {
                PluginLog.Debug("Audio meter button visible - subscribing to volume meters");
                OBSStudioForLogiPlugin.Instance?.SubscribeToVolumeMeters(SubscriptionOwner);
            }

            AudioMeterLevels levels = OBSStudioForLogiPlugin.Instance?.GetAudioMeterLevels(actionParameter) ?? AudioMeterLevels.Empty;
            return VuMeterRenderer.Render(levels, imageSize);
        }

        private void OnRefreshTimer(Object sender, ElapsedEventArgs e)
        {
            if (this._renderLease.ExpireIfIdle())
            {
                PluginLog.Debug("No audio meter buttons visible - unsubscribing from volume meters");
                OBSStudioForLogiPlugin.Instance?.UnsubscribeFromVolumeMeters(SubscriptionOwner);
            }

            if (!this._renderLease.IsActive && ++this._idleTicks < this._idleTicksPerProbe)
            {
                return;
            }

            this._idleTicks = 0;
            this.RefreshAllImages();
        }

        private void RefreshAllImages()
        {
            foreach (String input in this._audioInputs)
            {
                this.ActionImageChanged(input);
            }
        }

        private void ResetParameters()
        {
            this.RemoveAllParameters();

            foreach (String input in this._audioInputs)
            {
                this.AddParameter(input, input, this.GroupName).Description = $"Live volume meter for \"{input}\" - tap to toggle mute";
            }

            this.ParametersChanged();
            this.RefreshAllImages();
        }

        public void OnInputsChanged(String[] inputs)
        {
            this._audioInputs = inputs ?? new String[0];
            this.ResetParameters();
        }

        public void OnInputMuteChanged(String inputName)
        {
            if (this._audioInputs.Contains(inputName))
            {
                this.ActionImageChanged(inputName);
            }
        }

        public void OnConnected()
        {
            // Inputs arrive via OnInputsChanged from OBSWebSocketManager's per-connection state load.
            this.IsEnabled = true;

            Int32 refreshMs = new PluginConfigReader().ReadConfig()?.AudioMeterRefreshInterval ?? 100;
            this._refreshTimer.Interval = refreshMs;
            this._idleTicksPerProbe = Math.Max(1, OBSTimings.AudioMeterIdleProbeInterval / refreshMs);
            this._refreshTimer.Start();
        }

        public void OnDisconnected()
        {
            this._refreshTimer.Stop();

            if (this._renderLease.Release())
            {
                OBSStudioForLogiPlugin.Instance?.UnsubscribeFromVolumeMeters(SubscriptionOwner);
            }

            this.IsEnabled = false;
            this._audioInputs = new String[0];
            this.ResetParameters();
        }
    }
}
