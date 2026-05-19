namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class StudioModeTransitionCommand : PluginDynamicCommand
    {
        public static StudioModeTransitionCommand Instance { get; private set; }

        public StudioModeTransitionCommand()
            : base(displayName: "Studio Mode Transition", description: "Transition preview to program in studio mode", groupName: "1. OBS")
        {
            Instance = this;
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.TriggerStudioModeTransition();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return ButtonImageHelper.Icon("StudioModeTransition.svg");
        }
    }
}
