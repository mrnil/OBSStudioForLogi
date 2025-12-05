namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class SceneSwitchCommand : PluginDynamicCommand
    {
        public SceneSwitchCommand()
            : base(displayName: "Switch Scene", description: "Switch OBS scene", groupName: "OBS")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.SwitchScene(actionParameter);
        }
    }
}
