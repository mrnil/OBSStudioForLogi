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
            "wasapi_process_output_capture",
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

        public void SetCurrentPreviewScene(String sceneName)
        {
            this._obs?.SetCurrentPreviewScene(sceneName);
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

        public void ToggleReplayBuffer()
        {
            this._obs?.ToggleReplayBuffer();
        }

        public void StartReplayBuffer()
        {
            this._obs?.StartReplayBuffer();
        }

        public void StopReplayBuffer()
        {
            this._obs?.StopReplayBuffer();
        }

        public void SaveReplayBuffer()
        {
            this._obs?.SaveReplayBuffer();
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
            var sceneNames = scenes?.Select(s => s.Name).ToArray() ?? new String[0];
            Array.Reverse(sceneNames);
            return sceneNames;
        }

        public void SaveSourceScreenshot(String sourceName, String imageFormat, String imageFilePath, Int32 imageWidth, Int32 imageHeight)
        {
            this._obs?.SaveSourceScreenshot(sourceName, imageFormat, imageFilePath, imageWidth, imageHeight);
        }

        public String[] GetSceneItemList(String sceneName)
        {
            var sceneItems = this._obs?.GetSceneItemList(sceneName);
            var sourceNames = sceneItems?.Select(item => item.SourceName).ToArray() ?? new String[0];
            Array.Reverse(sourceNames);
            return sourceNames;
        }

        public Boolean GetSceneItemEnabled(String sceneName, String sourceName)
        {
            var sceneItems = this._obs?.GetSceneItemList(sceneName);
            var item = sceneItems?.FirstOrDefault(i => i.SourceName == sourceName);
            if (item == null)
                return false;
            
            var sceneItemEnabled = this._obs?.GetSceneItemEnabled(sceneName, item.ItemId);
            return sceneItemEnabled ?? false;
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

            var audioInputs = inputs.Where(input => AudioInputKinds.Contains(input.InputKind)).ToArray();
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

        public void SetInputMute(String inputName, Boolean muted)
        {
            this._obs?.SetInputMute(inputName, muted);
        }

        public Single GetInputVolume(String inputName)
        {
            var volume = this._obs?.GetInputVolume(inputName);
            return volume?.VolumeMul ?? 1.0f;
        }

        public void SetInputVolume(String inputName, Single volumeMul)
        {
            this._obs?.SetInputVolume(inputName, volumeMul);
        }

        public String GetInputAudioMonitorType(String inputName)
        {
            return this._obs?.GetInputAudioMonitorType(inputName) ?? "OBS_MONITORING_TYPE_NONE";
        }

        public void SetInputAudioMonitorType(String inputName, String monitorType)
        {
            this._obs?.SetInputAudioMonitorType(inputName, monitorType);
        }

        public String[] GetAudioSourcesInScene(String sceneName)
        {
            var sceneItems = this._obs?.GetSceneItemList(sceneName);
            if (sceneItems == null)
                return new String[0];

            var allInputs = this._obs?.GetInputList(null);
            if (allInputs == null)
                return new String[0];

            var audioInputNames = allInputs
                .Where(input => AudioInputKinds.Contains(input.InputKind))
                .Select(input => input.InputName)
                .ToHashSet();

            var result = sceneItems
                .Where(item => audioInputNames.Contains(item.SourceName))
                .Select(item => item.SourceName)
                .ToArray();

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

        public Boolean GetStudioModeEnabled()
        {
            return this._obs?.GetStudioModeEnabled() ?? false;
        }

        public void SetStudioModeEnabled(Boolean enabled)
        {
            this._obs?.SetStudioModeEnabled(enabled);
        }

        public void TriggerStudioModeTransition()
        {
            this._obs?.TriggerStudioModeTransition();
        }

        public Models.OBSStats GetStats()
        {
            var stats = this._obs?.GetStats();
            if (stats == null)
                return null;

            return new Models.OBSStats
            {
                Fps = stats.FPS,
                CpuUsage = stats.CpuUsage,
                MemoryUsage = stats.MemoryUsage,
                AverageFrameTime = stats.AverageFrameTime,
                FreeDiskSpace = stats.FreeDiskSpace,
                RenderTotalFrames = stats.RenderTotalFrames,
                RenderMissedFrames = stats.RenderMissedFrames,
                OutputTotalFrames = stats.OutputTotalFrames,
                OutputSkippedFrames = stats.OutputSkippedFrames
            };
        }

        public Models.OBSStreamStats GetStreamStatus()
        {
            var status = this._obs?.GetStreamStatus();
            if (status == null)
                return null;

            return new Models.OBSStreamStats
            {
                IsActive = status.IsActive,
                BytesSent = status.BytesSent,
                Duration = status.Duration,
                Congestion = status.Congestion,
                SkippedFrames = status.SkippedFrames,
                TotalFrames = status.TotalFrames
            };
        }

        public String GetMediaInputStatus(String inputName)
        {
            var status = this._obs?.GetMediaInputStatus(inputName);
            if (status?.State == null)
                return "OBS_MEDIA_STATE_NONE";

            return status.State.ToString();
        }

        public void TriggerMediaInputAction(String inputName, String mediaAction)
        {
            this._obs?.TriggerMediaInputAction(inputName, mediaAction);
        }

        public String[] GetMediaInputList()
        {
            var inputs = this._obs?.GetInputList(null);
            if (inputs == null)
                return new String[0];

            var mediaKinds = new[]
            {
                "ffmpeg_source",
                "vlc_source",
                "slideshow",
                "text_gdiplus",
                "text_gdiplus_v2",
                "text_ft2_source",
                "text_ft2_source_v2"
            };

            return inputs.Where(input => mediaKinds.Contains(input.InputKind))
                         .Select(input => input.InputName)
                         .ToArray();
        }
    }
}
