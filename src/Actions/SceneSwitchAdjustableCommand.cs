namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading.Tasks;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    public class SceneSwitchAdjustableCommand : ActionEditorCommand, IObsCommand, IProfileAwareCommand, ISceneCollectionAwareCommand, IScenesListAwareCommand
    {
        private const String ProfileNameControlName = "ProfileName";
        private const String CollectionNameControlName = "CollectionName";
        private const String SceneNameControlName = "SceneName";

        public static SceneSwitchAdjustableCommand Instance { get; private set; }

        public SceneSwitchAdjustableCommand()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.Name = "SceneSwitchAdjustable";
            this.DisplayName = "Switch to Scene (User Defined)";
            this.GroupName = "7. Scenes###User Defined";
            this.Description = "Switch to a specific scene with optional profile and collection switching";

            PluginLog.Debug("SceneSwitchAdjustableCommand: Constructor called");

            this.ActionEditor.AddControlEx(new ActionEditorTextbox(ProfileNameControlName, "Profile Name (optional)"));
            this.ActionEditor.AddControlEx(new ActionEditorTextbox(CollectionNameControlName, "Collection Name (optional)"));
            this.ActionEditor.AddControlEx(new ActionEditorTextbox(SceneNameControlName, "Scene Name (required)"));

            PluginLog.Debug("SceneSwitchAdjustableCommand: ActionEditor controls added");
        }

        protected override Boolean OnLoad()
        {
            PluginLog.Debug("SceneSwitchAdjustableCommand: OnLoad called");
            return true;
        }

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            PluginLog.Debug("SceneSwitchAdjustableCommand: RunCommand called");

            if (!actionParameters.TryGetString(SceneNameControlName, out var sceneName) || String.IsNullOrEmpty(sceneName))
            {
                PluginLog.Warning("SceneSwitchAdjustableCommand: Scene name is required but not provided");
                return false;
            }

            actionParameters.TryGetString(ProfileNameControlName, out var profileName);
            actionParameters.TryGetString(CollectionNameControlName, out var collectionName);

            PluginLog.Debug($"SceneSwitchAdjustableCommand: Requested - Profile='{profileName}', Collection='{collectionName}', Scene='{sceneName}'");

            Task.Run(async () =>
            {
                try
                {
                    var currentProfile = OBSStudioForLogiPlugin.Instance?.CurrentProfile ?? String.Empty;
                    var currentCollection = OBSStudioForLogiPlugin.Instance?.CurrentSceneCollection ?? String.Empty;
                    var currentScene = OBSStudioForLogiPlugin.Instance?.GetCurrentScene() ?? String.Empty;

                    PluginLog.Debug($"SceneSwitchAdjustableCommand: Current state - Profile='{currentProfile}', Collection='{currentCollection}', Scene='{currentScene}'");

                    // Handle profile switching
                    if (!String.IsNullOrEmpty(profileName))
                    {
                        if (profileName != currentProfile)
                        {
                            var availableProfiles = OBSStudioForLogiPlugin.Instance?.GetProfileList() ?? new String[0];
                            if (availableProfiles.Contains(profileName))
                            {
                                PluginLog.Debug($"SceneSwitchAdjustableCommand: Profile '{profileName}' found, switching from '{currentProfile}'");
                                OBSStudioForLogiPlugin.Instance?.SwitchProfile(profileName);
                                await Task.Delay(OBSTimings.ProfileSwitchDelay);
                                
                                // Update current state after switch
                                currentProfile = OBSStudioForLogiPlugin.Instance?.CurrentProfile ?? String.Empty;
                                currentCollection = OBSStudioForLogiPlugin.Instance?.CurrentSceneCollection ?? String.Empty;
                                PluginLog.Debug($"SceneSwitchAdjustableCommand: After profile switch - Profile='{currentProfile}', Collection='{currentCollection}'");
                            }
                            else
                            {
                                PluginLog.Warning($"SceneSwitchAdjustableCommand: Profile '{profileName}' not found in available profiles: [{String.Join(", ", availableProfiles)}]");
                                return;
                            }
                        }
                        else
                        {
                            PluginLog.Debug($"SceneSwitchAdjustableCommand: Profile '{profileName}' is already active");
                        }
                    }
                    else
                    {
                        PluginLog.Debug("SceneSwitchAdjustableCommand: No profile specified, using current profile");
                    }

                    // Handle collection switching
                    if (!String.IsNullOrEmpty(collectionName))
                    {
                        if (collectionName != currentCollection)
                        {
                            var availableCollections = OBSStudioForLogiPlugin.Instance?.GetSceneCollectionList() ?? new String[0];
                            if (availableCollections.Contains(collectionName))
                            {
                                PluginLog.Debug($"SceneSwitchAdjustableCommand: Collection '{collectionName}' found, switching from '{currentCollection}'");
                                OBSStudioForLogiPlugin.Instance?.SwitchSceneCollection(collectionName);
                                await Task.Delay(OBSTimings.CollectionSwitchDelay);
                                
                                // Update current state after switch
                                currentCollection = OBSStudioForLogiPlugin.Instance?.CurrentSceneCollection ?? String.Empty;
                                currentScene = OBSStudioForLogiPlugin.Instance?.GetCurrentScene() ?? String.Empty;
                                PluginLog.Debug($"SceneSwitchAdjustableCommand: After collection switch - Collection='{currentCollection}', Scene='{currentScene}'");
                            }
                            else
                            {
                                PluginLog.Warning($"SceneSwitchAdjustableCommand: Collection '{collectionName}' not found in available collections: [{String.Join(", ", availableCollections)}]");
                                return;
                            }
                        }
                        else
                        {
                            PluginLog.Debug($"SceneSwitchAdjustableCommand: Collection '{collectionName}' is already active");
                        }
                    }
                    else
                    {
                        PluginLog.Debug("SceneSwitchAdjustableCommand: No collection specified, using current collection");
                    }

                    // Handle scene switching
                    var availableScenes = OBSStudioForLogiPlugin.Instance?.GetSceneList() ?? new String[0];
                    if (availableScenes.Contains(sceneName))
                    {
                        if (sceneName != currentScene)
                        {
                            PluginLog.Debug($"SceneSwitchAdjustableCommand: Scene '{sceneName}' found, switching from '{currentScene}'");
                            OBSStudioForLogiPlugin.Instance?.SwitchScene(sceneName);
                            
                            // Update current state after switch
                            await Task.Delay(OBSTimings.SceneSwitchDelay);
                            currentScene = OBSStudioForLogiPlugin.Instance?.GetCurrentScene() ?? String.Empty;
                            PluginLog.Debug($"SceneSwitchAdjustableCommand: After scene switch - Scene='{currentScene}'");
                        }
                        else
                        {
                            PluginLog.Debug($"SceneSwitchAdjustableCommand: Scene '{sceneName}' is already active");
                        }
                    }
                    else
                    {
                        PluginLog.Warning($"SceneSwitchAdjustableCommand: Scene '{sceneName}' not found in available scenes: [{String.Join(", ", availableScenes)}]");
                        return;
                    }

                    PluginLog.Debug($"SceneSwitchAdjustableCommand: Switch completed successfully - Final state: Profile='{currentProfile}', Collection='{currentCollection}', Scene='{currentScene}'");
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"SceneSwitchAdjustableCommand: Failed to switch scene: {ex.Message}");
                }
            });

            return true;
        }

        public void OnConnected()
        {
            PluginLog.Debug("SceneSwitchAdjustableCommand: OnConnected called");
        }

        public void OnDisconnected()
        {
            PluginLog.Debug("SceneSwitchAdjustableCommand: OnDisconnected called");
        }

        public void OnProfileChanged(String oldProfile, String newProfile)
        {
        }

        public void OnSceneCollectionChanged(String oldSceneCollection, String newSceneCollection)
        {
        }

        public void OnScenesChanged(String[] scenes)
        {
        }

        private void OnProfileChanged()
        {
            this.ActionImageChanged();
        }

        private void OnSceneCollectionChanged()
        {
            this.ActionImageChanged();
        }

        private void OnScenesChanged()
        {
            this.ActionImageChanged();
        }
    }
}
