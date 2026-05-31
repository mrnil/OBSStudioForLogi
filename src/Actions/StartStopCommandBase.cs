namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public abstract class StartStopCommandBase : PluginDynamicCommand
    {
        private readonly Boolean _isStartCommand;

        protected StartStopCommandBase(String displayName, String description, String groupName, Boolean isStartCommand)
            : base(displayName, description, groupName)
        {
            this._isStartCommand = isStartCommand;
            this.IsWidget = true;
        }

        protected abstract void ExecuteStart();
        protected abstract void ExecuteStop();
        protected abstract Boolean GetState();
        protected abstract String GetEnabledIcon();
        protected abstract String GetDisabledIcon();

        protected override void RunCommand(String actionParameter)
        {
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
            Boolean isActive = this.GetState();
            Boolean shouldEnable = this._isStartCommand ? !isActive : isActive;
            return ButtonImageHelper.StateIcon(shouldEnable, this.GetEnabledIcon(), this.GetDisabledIcon());
        }
    }
}
