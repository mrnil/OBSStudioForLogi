namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    public abstract class StartStopCommandBase : PluginDynamicCommand
    {
        private readonly Boolean _isStartCommand;

        protected StartStopCommandBase(String displayName, String description, String groupName, Boolean isStartCommand)
            : base(displayName, description, groupName)
        {
            this._isStartCommand = isStartCommand;
            this.IsWidget = false;
        }

        protected abstract void ExecuteStart();
        protected abstract void ExecuteStop();
        protected abstract Boolean GetState();
        protected abstract String GetEnabledIcon();
        protected abstract String GetDisabledIcon();

        // True while OBS is starting or stopping the output. Outputs without those states (the
        // virtual camera) leave it false.
        protected virtual Boolean IsChanging() => false;

        // A Start that can't start (already running) or a Stop that can't stop (not running) is
        // drawn greyed and ignores presses - see OutputButtonAvailability.
        private Boolean IsAvailable()
        {
            Boolean isConnected = OBSStudioForLogiPlugin.Instance?.IsConnected ?? false;
            return OutputButtonAvailability.StartStop(this._isStartCommand, isConnected, this.GetState(), this.IsChanging());
        }

        protected override void RunCommand(String actionParameter)
        {
            if (!this.IsAvailable())
            {
                PluginLog.Debug($"{this.DisplayName} pressed while unavailable - ignored");
                return;
            }

            if (this._isStartCommand)
            {
                this.ExecuteStart();
            }
            else
            {
                this.ExecuteStop();
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return ButtonImageHelper.Icon(this.IsAvailable() ? this.GetEnabledIcon() : this.GetDisabledIcon());
        }
    }
}
