namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    /// <summary>
    /// Base class for toggle commands that switch between two states (on/off, active/inactive).
    /// </summary>
    public abstract class ToggleCommandBase : PluginDynamicCommand
    {
        protected ToggleCommandBase(String displayName, String description, String groupName)
            : base(displayName, description, groupName)
        {
            this.IsWidget = false;
        }

        /// <summary>
        /// Executes the toggle action.
        /// </summary>
        protected abstract void ExecuteToggle();

        /// <summary>
        /// Gets the current state (true = active/on, false = inactive/off).
        /// </summary>
        protected abstract Boolean GetState();

        /// <summary>
        /// Gets the icon resource name for the active state.
        /// </summary>
        protected abstract String GetActiveIcon();

        /// <summary>
        /// Gets the icon resource name for the inactive state.
        /// </summary>
        protected abstract String GetInactiveIcon();

        protected override void RunCommand(String actionParameter)
        {
            this.ExecuteToggle();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isActive = this.GetState();
            return ButtonImageHelper.Icon(isActive ? this.GetActiveIcon() : this.GetInactiveIcon());
        }
    }
}
