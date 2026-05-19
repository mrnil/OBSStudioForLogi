namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class StudioModeToggleCommand : PluginDynamicCommand
    {
        public static StudioModeToggleCommand Instance { get; private set; }

        public StudioModeToggleCommand()
            : base(displayName: "Toggle Studio Mode", description: "Enable/disable OBS studio mode", groupName: "1. OBS")
        {
            Instance = this;
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.ToggleStudioMode();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isEnabled = OBSStudioForLogiPlugin.Instance?.IsStudioModeEnabled ?? false;
            return ButtonImageHelper.StateIcon(isEnabled, "StudioModeToggleOn.svg", "StudioModeToggleOff.svg");
        }

        public void OnStudioModeStateChanged()
        {
            this.ActionImageChanged();
        }
    }
}
