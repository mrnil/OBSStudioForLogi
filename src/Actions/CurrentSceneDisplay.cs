namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class CurrentSceneDisplay : PluginDynamicCommand
    {
        public static CurrentSceneDisplay Instance { get; private set; }

        private String _currentScene = "Not Connected";

        public CurrentSceneDisplay()
            : base(displayName: "Current Scene", description: "Shows current OBS scene", groupName: "1. OBS")
        {
            Instance = this;
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            if (!OBSStudioForLogiPlugin.Instance?.IsConnected ?? true)
            {
                return "Not Connected";
            }
            return this._currentScene;
        }

        public void UpdateScene(String sceneName)
        {
            if (String.IsNullOrEmpty(sceneName))
            {
                PluginLog.Warning("Cannot update scene display - scene name is empty");
                return;
            }

            PluginLog.Info($"Updating scene display to '{sceneName}'");
            this._currentScene = sceneName;
            this.ActionImageChanged();
        }

        public void UpdateDisplay()
        {
            this.ActionImageChanged();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            return null;
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only, no action
        }
    }
}
