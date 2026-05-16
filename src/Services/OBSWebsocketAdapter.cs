namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Linq;
    using OBSWebsocketDotNet;

    public class OBSWebsocketAdapter : IOBSWebsocket
    {
        private readonly OBSWebsocket _obs;

        private static readonly String[] AudioInputKinds = new[]
        {
            "wasapi_input_capture",
            "wasapi_output_capture",
            "coreaudio_input_capture",
            "coreaudio_output_capture",
            "pulse_input_capture",
            "pulse_output_capture",
            "alsa_input_capture",
            "jack_output_capture",
            "ffmpeg_source",
            "dshow_input",
            "window_capture",
            "audio_capture"
        };

        public OBSWebsocketAdapter(OBSWebsocket obs)
        {
            this._obs = obs;
        }

        public Boolean IsConnected => this._obs?.IsConnected ?? false;

        public void SetCurrentProgramScene(String sceneName)
        {
            this._obs?.SetCurrentProgramScene(sceneName);
        }

        public void ToggleRecord()
        {
            this._obs?.ToggleRecord();
        }

        public void StartRecord()
        {
            this._obs?.StartRecord();
        }

        public void StopRecord()
        {
            this._obs?.StopRecord();
        }

        public void PauseRecord()
        {
            this._obs?.PauseRecord();
        }

        public void ResumeRecord()
        {
            this._obs?.ResumeRecord();
        }

        public void ToggleStream()
        {
            this._obs?.ToggleStream();
        }

        public void StartStream()
        {
            this._obs?.StartStream();
        }

        public void StopStream()
        {
            this._obs?.StopStream();
        }

        public void ToggleVirtualCam()
        {
            this._obs?.ToggleVirtualCam();
        }

        public void StartVirtualCam()
        {
            this._obs?.StartVirtualCam();
        }

        public void StopVirtualCam()
        {
            this._obs?.StopVirtualCam();
        }

        public String[] GetProfileList()
        {
            var profiles = this._obs?.GetProfileList()?.Profiles;
            return profiles?.ToArray() ?? new String[0];
        }

        public void SetCurrentProfile(String profileName)
        {
            this._obs?.SetCurrentProfile(profileName);
        }

        public String[] GetSceneCollectionList()
        {
            var collections = this._obs?.GetSceneCollectionList();
            return collections?.ToArray() ?? new String[0];
        }

        public void SetCurrentSceneCollection(String sceneCollectionName)
        {
            this._obs?.SetCurrentSceneCollection(sceneCollectionName);
        }

        public String[] GetSceneList()
        {
            var scenes = this._obs?.GetSceneList()?.Scenes;
            return scenes?.Select(s => s.Name).ToArray() ?? new String[0];
        }

        public void SaveSourceScreenshot(String sourceName, String imageFormat, String imageFilePath, Int32 imageWidth, Int32 imageHeight)
        {
            this._obs?.SaveSourceScreenshot(sourceName, imageFormat, imageFilePath, imageWidth, imageHeight);
        }

        public String[] GetSceneItemList(String sceneName)
        {
            var sceneItems = this._obs?.GetSceneItemList(sceneName);
            return sceneItems?.Select(item => item.SourceName).ToArray() ?? new String[0];
        }

        public Boolean GetSceneItemEnabled(String sceneName, String sourceName)
        {
            var sceneItems = this._obs?.GetSceneItemList(sceneName);
            var item = sceneItems?.FirstOrDefault(i => i.SourceName == sourceName);
            return item != null;
        }

        public void SetSceneItemEnabled(String sceneName, String sourceName, Boolean enabled)
        {
            var sceneItems = this._obs?.GetSceneItemList(sceneName);
            var item = sceneItems?.FirstOrDefault(i => i.SourceName == sourceName);
            if (item != null)
            {
                this._obs?.SetSceneItemEnabled(sceneName, item.ItemId, enabled);
            }
        }

        public String[] GetInputList()
        {
            var inputs = this._obs?.GetInputList(null);
            if (inputs == null)
                return new String[0];

            // Filter for audio inputs only
            var audioInputs = inputs.Where(input => AudioInputKinds.Contains(input.InputKind)).ToArray();

            // Debug logging to show all audio inputs with their kinds
            PluginLog.Info("=== Audio Mixer Inputs ===");
            foreach (var input in audioInputs)
            {
                PluginLog.Info($"Input: '{input.InputName}', Kind: '{input.InputKind}'");
            }
            PluginLog.Info($"Total audio inputs: {audioInputs.Length}");

            return audioInputs.Select(input => input.InputName).ToArray();
        }

        public String GetInputKind(String inputName)
        {
            var inputs = this._obs?.GetInputList(null);
            var input = inputs?.FirstOrDefault(i => i.InputName == inputName);
            return input?.InputKind ?? String.Empty;
        }

        public Boolean GetInputMute(String inputName)
        {
            return this._obs?.GetInputMute(inputName) ?? false;
        }

        public void ToggleInputMute(String inputName)
        {
            this._obs?.ToggleInputMute(inputName);
        }

        public String[] GetAudioSourcesInScene(String sceneName)
        {
            PluginLog.Info($"=== GetAudioSourcesInScene for '{sceneName}' ===");
            
            var sceneItems = this._obs?.GetSceneItemList(sceneName);
            if (sceneItems == null)
            {
                PluginLog.Info("Scene items is null");
                return new String[0];
            }

            PluginLog.Info($"Scene has {sceneItems.Count} items:");
            foreach (var item in sceneItems)
            {
                PluginLog.Info($"  - '{item.SourceName}' (Kind: '{item.SourceKind}', Type: {item.SourceType})");
            }

            // Get all inputs from OBS
            var allInputs = this._obs?.GetInputList(null);
            if (allInputs == null)
            {
                PluginLog.Info("All inputs is null");
                return new String[0];
            }

            PluginLog.Info($"Total inputs in OBS: {allInputs.Count}");
            foreach (var input in allInputs)
            {
                PluginLog.Info($"  - '{input.InputName}' (Kind: '{input.InputKind}')");
            }

            // Filter for audio input kinds using shared constant
            var audioInputNames = allInputs
                .Where(input => AudioInputKinds.Contains(input.InputKind))
                .Select(input => input.InputName)
                .ToHashSet();

            PluginLog.Info($"Filtered audio inputs: {audioInputNames.Count}");
            foreach (var name in audioInputNames)
            {
                PluginLog.Info($"  - '{name}'");
            }

            // Return scene items that are audio inputs
            var result = sceneItems
                .Where(item => audioInputNames.Contains(item.SourceName))
                .Select(item => item.SourceName)
                .ToArray();

            PluginLog.Info($"Matched audio sources in scene: {result.Length}");
            foreach (var name in result)
            {
                PluginLog.Info($"  - '{name}'");
            }

            return result;
        }

        public String[] GetAudioInputsNotInAnyScene()
        {
            var allAudioInputs = this.GetInputList();
            var scenes = this._obs?.GetSceneList()?.Scenes;
            
            if (scenes == null)
                return allAudioInputs;

            var inputsInScenes = new HashSet<String>();
            
            foreach (var scene in scenes)
            {
                var sceneItems = this._obs?.GetSceneItemList(scene.Name);
                if (sceneItems != null)
                {
                    foreach (var item in sceneItems)
                    {
                        inputsInScenes.Add(item.SourceName);
                    }
                }
            }

            return allAudioInputs.Where(input => !inputsInScenes.Contains(input)).ToArray();
        }

        public String[] GetScenesForInput(String inputName)
        {
            var scenes = this._obs?.GetSceneList()?.Scenes;
            if (scenes == null)
                return new String[0];

            var scenesWithInput = scenes
                .Where(scene => 
                {
                    var items = this._obs?.GetSceneItemList(scene.Name);
                    return items?.Any(item => item.SourceName == inputName) ?? false;
                })
                .Select(scene => scene.Name)
                .ToArray();

            return scenesWithInput;
        }
    }
}
