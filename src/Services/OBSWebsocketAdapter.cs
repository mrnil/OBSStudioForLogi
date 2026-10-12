namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using OBSWebsocketDotNet;
    using OBSWebsocketDotNet.Communication;
    using OBSWebsocketDotNet.Types;
    using OBSWebsocketDotNet.Types.Events;

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
            "audio_capture",
            "game_capture",
            "browser_source"
        };

        public OBSWebsocketAdapter(OBSWebsocket obs)
        {
            this._obs = obs;
        }

        // The socket being open (the library's IsConnected) isn't enough - requests are only valid
        // once OBS has confirmed identification, and IsConnected is already true during that handshake.
        public Boolean IsConnected => this._obs != null && this._obs.IsConnected && this._obs.IsIdentified;

        // In obs-websocket-dotnet 5.7.0 the WSTimeout setter also sets the live client's no-message
        // ReconnectTimeout, so set this only while disconnected - see OBSWebSocketManager.
        public TimeSpan RequestTimeout
        {
            get => this._obs.WSTimeout;
            set => this._obs.WSTimeout = value;
        }

        // The library's ConnectAsync starts the connection and returns straight away; the outcome
        // arrives as Connected or Disconnected.
        public void Connect(String url, String password)
        {
            this._obs.ConnectAsync(url, password);
        }

        public void Disconnect()
        {
            this._obs.Disconnect();
        }

        public (String CurrentProfile, String[] Profiles) GetCurrentProfileAndList()
        {
            GetProfileListInfo profiles = this._obs.GetProfileList();
            return (profiles?.CurrentProfileName, profiles?.Profiles?.ToArray() ?? new String[0]);
        }

        public String GetCurrentSceneCollection()
        {
            return this._obs.GetCurrentSceneCollection();
        }

        public String GetCurrentProgramScene()
        {
            return this._obs.GetCurrentProgramScene();
        }

        // Reads the raw response rather than the library's GetSceneItemList: OBS sends each item's
        // sceneItemEnabled, which the library's SceneItemDetails drops, and having it here saves a
        // GetSceneItemEnabled request for every visibility read and toggle (assessment #30).
        public (Int32 ItemId, String SourceName, Boolean Enabled)[] GetSceneItems(String sceneName)
        {
            return ParseSceneItems(this._obs.SendRequest("GetSceneItemList", new JObject { { "sceneName", sceneName } }));
        }

        internal static (Int32 ItemId, String SourceName, Boolean Enabled)[] ParseSceneItems(JObject response)
        {
            if (!(response?["sceneItems"] is JArray items))
            {
                return new (Int32, String, Boolean)[0];
            }

            return items.OfType<JObject>()
                .Select(item => ((Int32?)item["sceneItemId"] ?? 0, (String)item["sourceName"], (Boolean?)item["sceneItemEnabled"] ?? false))
                .ToArray();
        }

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

        // One request: the item list carries each item's visibility (see GetSceneItems).
        public Boolean GetSceneItemEnabled(String sceneName, String sourceName)
        {
            foreach ((Int32 _, String itemSource, Boolean enabled) in this.GetSceneItems(sceneName))
            {
                if (itemSource == sourceName)
                {
                    return enabled;
                }
            }

            return false;
        }

        public void SetSceneItemEnabled(String sceneName, Int32 sceneItemId, Boolean enabled)
        {
            this._obs.SetSceneItemEnabled(sceneName, sceneItemId, enabled);
        }

        public String[] GetInputList()
        {
            var inputs = this._obs?.GetInputList(null);
            if (inputs == null)
                return new String[0];

            var audioInputs = inputs.Where(input => AudioInputKinds.Contains(input.InputKind)).ToArray();
            return audioInputs.Select(input => input.InputName).ToArray();
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

        // Reads the raw response rather than the library's typed GetStats: its ObsStats uses
        // non-nullable doubles, and OBS can report a field as null (seen for cpuUsage under load),
        // which made the whole call throw on every poll.
        public Models.OBSStats GetStats()
        {
            return this._obs == null ? null : ParseStats(this._obs.SendRequest("GetStats"));
        }

        internal static Models.OBSStats ParseStats(JObject response)
        {
            if (response == null)
            {
                return null;
            }

            return new Models.OBSStats
            {
                Fps = (Double?)response["activeFps"] ?? 0,
                CpuUsage = (Double?)response["cpuUsage"] ?? 0,
                MemoryUsage = (Double?)response["memoryUsage"] ?? 0,
                AverageFrameTime = (Double?)response["averageFrameRenderTime"] ?? 0,
                FreeDiskSpace = (Double?)response["availableDiskSpace"] ?? 0,
                RenderTotalFrames = (Int64?)response["renderTotalFrames"] ?? 0,
                RenderMissedFrames = (Int64?)response["renderSkippedFrames"] ?? 0,
                OutputTotalFrames = (Int64?)response["outputTotalFrames"] ?? 0,
                OutputSkippedFrames = (Int64?)response["outputSkippedFrames"] ?? 0
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

        // Events forward straight to the library's own, so subscribing and unsubscribing behave
        // exactly as on the library - including the InputVolumeMeters opt-in.
        public event EventHandler Connected
        {
            add => this._obs.Connected += value;
            remove => this._obs.Connected -= value;
        }

        public event EventHandler<ObsDisconnectionInfo> Disconnected
        {
            add => this._obs.Disconnected += value;
            remove => this._obs.Disconnected -= value;
        }

        public event EventHandler<StreamStateChangedEventArgs> StreamStateChanged
        {
            add => this._obs.StreamStateChanged += value;
            remove => this._obs.StreamStateChanged -= value;
        }

        public event EventHandler<RecordStateChangedEventArgs> RecordStateChanged
        {
            add => this._obs.RecordStateChanged += value;
            remove => this._obs.RecordStateChanged -= value;
        }

        public event EventHandler<VirtualcamStateChangedEventArgs> VirtualcamStateChanged
        {
            add => this._obs.VirtualcamStateChanged += value;
            remove => this._obs.VirtualcamStateChanged -= value;
        }

        public event EventHandler<ReplayBufferStateChangedEventArgs> ReplayBufferStateChanged
        {
            add => this._obs.ReplayBufferStateChanged += value;
            remove => this._obs.ReplayBufferStateChanged -= value;
        }

        public event EventHandler<ReplayBufferSavedEventArgs> ReplayBufferSaved
        {
            add => this._obs.ReplayBufferSaved += value;
            remove => this._obs.ReplayBufferSaved -= value;
        }

        public event EventHandler<CurrentProfileChangedEventArgs> CurrentProfileChanged
        {
            add => this._obs.CurrentProfileChanged += value;
            remove => this._obs.CurrentProfileChanged -= value;
        }

        public event EventHandler<ProfileListChangedEventArgs> ProfileListChanged
        {
            add => this._obs.ProfileListChanged += value;
            remove => this._obs.ProfileListChanged -= value;
        }

        public event EventHandler<CurrentSceneCollectionChangingEventArgs> CurrentSceneCollectionChanging
        {
            add => this._obs.CurrentSceneCollectionChanging += value;
            remove => this._obs.CurrentSceneCollectionChanging -= value;
        }

        public event EventHandler<CurrentSceneCollectionChangedEventArgs> CurrentSceneCollectionChanged
        {
            add => this._obs.CurrentSceneCollectionChanged += value;
            remove => this._obs.CurrentSceneCollectionChanged -= value;
        }

        public event EventHandler<SceneCollectionListChangedEventArgs> SceneCollectionListChanged
        {
            add => this._obs.SceneCollectionListChanged += value;
            remove => this._obs.SceneCollectionListChanged -= value;
        }

        public event EventHandler<SceneListChangedEventArgs> SceneListChanged
        {
            add => this._obs.SceneListChanged += value;
            remove => this._obs.SceneListChanged -= value;
        }

        public event EventHandler<ProgramSceneChangedEventArgs> CurrentProgramSceneChanged
        {
            add => this._obs.CurrentProgramSceneChanged += value;
            remove => this._obs.CurrentProgramSceneChanged -= value;
        }

        public event EventHandler<SceneNameChangedEventArgs> SceneNameChanged
        {
            add => this._obs.SceneNameChanged += value;
            remove => this._obs.SceneNameChanged -= value;
        }

        public event EventHandler<StudioModeStateChangedEventArgs> StudioModeStateChanged
        {
            add => this._obs.StudioModeStateChanged += value;
            remove => this._obs.StudioModeStateChanged -= value;
        }

        public event EventHandler<SceneItemEnableStateChangedEventArgs> SceneItemEnableStateChanged
        {
            add => this._obs.SceneItemEnableStateChanged += value;
            remove => this._obs.SceneItemEnableStateChanged -= value;
        }

        public event EventHandler<SceneItemCreatedEventArgs> SceneItemCreated
        {
            add => this._obs.SceneItemCreated += value;
            remove => this._obs.SceneItemCreated -= value;
        }

        public event EventHandler<SceneItemRemovedEventArgs> SceneItemRemoved
        {
            add => this._obs.SceneItemRemoved += value;
            remove => this._obs.SceneItemRemoved -= value;
        }

        public event EventHandler<InputCreatedEventArgs> InputCreated
        {
            add => this._obs.InputCreated += value;
            remove => this._obs.InputCreated -= value;
        }

        public event EventHandler<InputRemovedEventArgs> InputRemoved
        {
            add => this._obs.InputRemoved += value;
            remove => this._obs.InputRemoved -= value;
        }

        public event EventHandler<InputNameChangedEventArgs> InputNameChanged
        {
            add => this._obs.InputNameChanged += value;
            remove => this._obs.InputNameChanged -= value;
        }

        public event EventHandler<InputMuteStateChangedEventArgs> InputMuteStateChanged
        {
            add => this._obs.InputMuteStateChanged += value;
            remove => this._obs.InputMuteStateChanged -= value;
        }

        public event EventHandler<InputVolumeChangedEventArgs> InputVolumeChanged
        {
            add => this._obs.InputVolumeChanged += value;
            remove => this._obs.InputVolumeChanged -= value;
        }

        public event EventHandler<InputAudioMonitorTypeChangedEventArgs> InputAudioMonitorTypeChanged
        {
            add => this._obs.InputAudioMonitorTypeChanged += value;
            remove => this._obs.InputAudioMonitorTypeChanged -= value;
        }

        public event EventHandler<MediaInputPlaybackStartedEventArgs> MediaInputPlaybackStarted
        {
            add => this._obs.MediaInputPlaybackStarted += value;
            remove => this._obs.MediaInputPlaybackStarted -= value;
        }

        public event EventHandler<MediaInputPlaybackEndedEventArgs> MediaInputPlaybackEnded
        {
            add => this._obs.MediaInputPlaybackEnded += value;
            remove => this._obs.MediaInputPlaybackEnded -= value;
        }

        public event EventHandler<MediaInputActionTriggeredEventArgs> MediaInputActionTriggered
        {
            add => this._obs.MediaInputActionTriggered += value;
            remove => this._obs.MediaInputActionTriggered -= value;
        }

        public event EventHandler<InputVolumeMetersEventArgs> InputVolumeMeters
        {
            add => this._obs.InputVolumeMeters += value;
            remove => this._obs.InputVolumeMeters -= value;
        }
    }
}
