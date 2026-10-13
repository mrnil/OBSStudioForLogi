namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

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

        /// <summary>
        /// Whether the toggle can do anything right now. An unavailable toggle is drawn with
        /// <see cref="GetUnavailableIcon"/> and ignores presses. Most toggles are always available.
        /// </summary>
        protected virtual Boolean IsAvailable() => true;

        /// <summary>
        /// Gets the icon resource name shown while <see cref="IsAvailable"/> is false: the grey
        /// variant of the glyph the toggle normally shows.
        /// </summary>
        protected virtual String GetUnavailableIcon() => this.GetInactiveIcon();

        protected override void RunCommand(String actionParameter)
        {
            if (!this.IsAvailable())
            {
                PluginLog.Debug($"{this.DisplayName} pressed while unavailable - ignored");
                return;
            }

            this.ExecuteToggle();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            if (!this.IsAvailable())
            {
                return ButtonImageHelper.Icon(this.GetUnavailableIcon());
            }

            Boolean isActive = this.GetState();
            return ButtonImageHelper.Icon(isActive ? this.GetActiveIcon() : this.GetInactiveIcon());
        }
    }
}
