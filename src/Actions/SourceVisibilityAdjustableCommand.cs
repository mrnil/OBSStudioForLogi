namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    public class SourceVisibilityAdjustableCommand : ActionEditorCommand, IObsCommand, ISourceVisibilityAwareCommand, ISceneAwareCommand
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

                    var sources = ParseSourceNames(sourceNames);

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

        // Multiple sources toggle independently and can disagree, so the icon
        // follows the first one; this also keeps it to one OBS query per redraw.
        protected override BitmapImage GetCommandImage(ActionEditorActionParameters actionParameters, Int32 imageWidth, Int32 imageHeight)
        {
            Boolean isVisible = false;
            OBSStudioForLogiPlugin plugin = OBSStudioForLogiPlugin.Instance;

            if (plugin != null && plugin.IsConnected
                && actionParameters.TryGetString(SourceNameControlName, out String sourceNames))
            {
                String firstSource = ParseSourceNames(sourceNames).FirstOrDefault();
                actionParameters.TryGetString(SceneNameControlName, out String sceneName);
                String targetScene = !String.IsNullOrEmpty(sceneName) ? sceneName : plugin.GetCurrentScene();

                if (!String.IsNullOrEmpty(firstSource) && !String.IsNullOrEmpty(targetScene))
                {
                    isVisible = plugin.GetSourceVisibility(targetScene, firstSource);
                }
            }

            return ButtonImageHelper.Icon(isVisible ? "SourceVisibilityOn.svg" : "SourceVisibilityOff.svg");
        }

        private static String[] ParseSourceNames(String sourceNames)
        {
            return (sourceNames ?? String.Empty).Split(',')
                .Select(s => s.Trim())
                .Where(s => !String.IsNullOrEmpty(s))
                .ToArray();
        }

        public void OnConnected()
        {
            this.ActionImageChanged();
        }

        public void OnDisconnected()
        {
            this.ActionImageChanged();
        }

        public void OnSourceVisibilityChanged(String sceneName, String sourceName)
        {
            this.ActionImageChanged();
        }

        // Covers buttons with no scene set, which follow the current scene.
        public void OnSceneChanged(String sceneName)
        {
            this.ActionImageChanged();
        }
    }
}
