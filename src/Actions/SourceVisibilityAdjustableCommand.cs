namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    public class SourceVisibilityAdjustableCommand : ActionEditorCommand, IObsCommand
    {
        private const String SceneNameControlName = "SceneName";
        private const String SourceNameControlName = "SourceName";

        public static SourceVisibilityAdjustableCommand Instance { get; private set; }

        public SourceVisibilityAdjustableCommand()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.Name = "SourceVisibilityAdjustable";
            this.DisplayName = "Toggle Source Visibility (User defined)";
            this.GroupName = "7. Scenes###User Defined";
            this.Description = "Toggle visibility of one or more sources. Comma-separate multiple source names.";

            this.ActionEditor.AddControlEx(new ActionEditorTextbox(SceneNameControlName, "Scene Name (optional, defaults to current scene)"));
            this.ActionEditor.AddControlEx(new ActionEditorTextbox(SourceNameControlName, "Source Name(s) (required, comma-separated)"));
        }

        protected override Boolean OnLoad() => true;

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            if (!actionParameters.TryGetString(SourceNameControlName, out var sourceNames) || String.IsNullOrEmpty(sourceNames))
            {
                PluginLog.Warning("SourceVisibilityAdjustableCommand: Source name is required but not provided");
                return false;
            }

            actionParameters.TryGetString(SceneNameControlName, out var sceneName);

            Task.Run(() =>
            {
                try
                {
                    var targetScene = !String.IsNullOrEmpty(sceneName)
                        ? sceneName
                        : OBSStudioForLogiPlugin.Instance?.GetCurrentScene() ?? String.Empty;

                    if (String.IsNullOrEmpty(targetScene))
                    {
                        PluginLog.Warning("SourceVisibilityAdjustableCommand: No scene available");
                        return;
                    }

                    var sources = sourceNames.Split(',')
                        .Select(s => s.Trim())
                        .Where(s => !String.IsNullOrEmpty(s))
                        .ToArray();

                    foreach (var source in sources)
                    {
                        PluginLog.Info($"SourceVisibilityAdjustableCommand: Toggling '{source}' in scene '{targetScene}'");
                        OBSStudioForLogiPlugin.Instance?.ToggleSourceVisibility(targetScene, source);
                    }
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"SourceVisibilityAdjustableCommand: Failed to toggle source visibility: {ex.Message}");
                }
            });

            return true;
        }

        public void OnConnected()
        {
        }

        public void OnDisconnected()
        {
        }
    }
}
