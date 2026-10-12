namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading.Tasks;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;
    using OBSWebsocketDotNet.Types;

    public class OBSActionExecutor
    {
        private readonly IOBSWebsocket _obs;
        private readonly IPluginLog _log;
        private readonly Action<Action> _runInBackground;
        private OutputState _recordingState = OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
        private OutputState _streamingState = OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
        private OutputState _virtualCameraState = OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
        private OutputState _replayBufferState = OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
        private String _currentProfile = String.Empty;
        private String _currentSceneCollection = String.Empty;
        private String _currentScene = String.Empty;
        private Boolean _studioModeEnabled = false;

        public Boolean IsRecording => this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED
                                    || this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_PAUSED
                                    || this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_RESUMED;
        public Boolean IsRecordingPaused { get; private set; }
        public Boolean IsRecordingChanging => this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTING
                                            || this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_STOPPING;
        public Boolean IsStreaming => this._streamingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED;
        public Boolean IsStreamingChanging => this._streamingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTING
                                            || this._streamingState == OutputState.OBS_WEBSOCKET_OUTPUT_STOPPING;
        public Boolean IsVirtualCameraActive => this._virtualCameraState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED;
        public Boolean IsReplayBufferActive => this._replayBufferState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED;
        public Boolean IsStudioModeEnabled => this._studioModeEnabled;
        public String CurrentProfile => this._currentProfile;
        public String CurrentSceneCollection => this._currentSceneCollection;
        public String CurrentScene => this._currentScene;

        public OBSActionExecutor(IOBSWebsocket obs, IPluginLog log)
            : this(obs, log, action => Task.Run(action))
        {
        }

        // OBS mutations run off the caller's thread so a slow OBS never stalls a button press.
        // runInBackground lets tests run that work inline instead of sleeping until Task.Run finishes.
        public OBSActionExecutor(IOBSWebsocket obs, IPluginLog log, Action<Action> runInBackground)
        {
            this._obs = obs;
            this._log = log;
            this._runInBackground = runInBackground ?? (action => Task.Run(action));
        }

        public void SetCurrentScene(String sceneName)
        {
            this.RunMutation($"set scene '{sceneName}'", () =>
            {
                if (this._studioModeEnabled)
                {
                    this._log.Info($"Setting current preview scene to '{sceneName}' (studio mode enabled)");
                    this._obs.SetCurrentPreviewScene(sceneName);
                }
                else
                {
                    this._log.Info($"Setting current program scene to '{sceneName}'");
                    this._obs.SetCurrentProgramScene(sceneName);
                }
            });
        }

        public void ToggleRecording()
        {
            this.RunMutation("toggle recording", () =>
            {
                this._log.Info("Toggling recording");
                this._obs.ToggleRecord();
            },
            () => this.IsRecordingChanging ? "state change in progress" : null);
        }

        public void StartRecording()
        {
            this.RunMutation("start recording", () =>
            {
                this._log.Info("Starting recording");
                this._obs.StartRecord();
            },
            () => this.IsRecording ? "already recording" : null);
        }

        public void StopRecording()
        {
            this.RunMutation("stop recording", () =>
            {
                this._log.Info("Stopping recording");
                this._obs.StopRecord();
            },
            () => this.IsRecording ? null : "not recording");
        }

        public void ToggleRecordingPause()
        {
            this.RunMutation("toggle recording pause", () =>
            {
                this._log.Info("Toggling recording pause");
                if (this.IsRecordingPaused)
                {
                    this._obs.ResumeRecord();
                }
                else
                {
                    this._obs.PauseRecord();
                }
            },
            () => this.IsRecording ? null : "not recording");
        }

        public String[] GetProfileList()
        {
            return this.Query("profile list", new String[0], () =>
            {
                this._log.Debug("Getting profile list");
                return this._obs.GetProfileList();
            });
        }

        public void SetCurrentProfile(String profileName)
        {
            this.RunMutation($"set profile '{profileName}'", () =>
            {
                if (this._currentProfile == profileName)
                {
                    this._log.Info($"Profile '{profileName}' is already active");
                    return;
                }

                this._log.Info($"Setting current profile to '{profileName}'");
                this._obs.SetCurrentProfile(profileName);
            });
        }

        public void SetCurrentProfileState(String profileName)
        {
            this._currentProfile = profileName;
        }

        public String[] GetSceneCollectionList()
        {
            return this.Query("scene collection list", new String[0], () =>
            {
                this._log.Debug("Getting scene collection list");
                return this._obs.GetSceneCollectionList();
            });
        }

        public void SetCurrentSceneCollection(String sceneCollectionName)
        {
            this.RunMutation($"set scene collection '{sceneCollectionName}'", () =>
            {
                if (this._currentSceneCollection == sceneCollectionName)
                {
                    this._log.Info($"Scene collection '{sceneCollectionName}' is already active");
                    return;
                }

                this._log.Info($"Setting current scene collection to '{sceneCollectionName}'");
                this._obs.SetCurrentSceneCollection(sceneCollectionName);
            });
        }

        public void SetCurrentSceneCollectionState(String sceneCollectionName)
        {
            this._currentSceneCollection = sceneCollectionName;
        }

        public void SetCurrentSceneState(String sceneName)
        {
            this._currentScene = sceneName;
        }

        public String[] GetSceneList()
        {
            return this.Query("scene list", new String[0], () =>
            {
                this._log.Debug("Getting scene list");
                return this._obs.GetSceneList();
            });
        }

        public void SaveScreenshot(String screenshotPath)
        {
            this.RunMutation("save screenshot", () =>
            {
                String filename = System.IO.Path.Combine(screenshotPath, $"Screenshot-{DateTime.Now:yyyyMMddHHmmssfff}.png");
                this._log.Info($"Saving screenshot to {filename}");
                this._obs.SaveSourceScreenshot(this._currentScene, "png", filename, -1, -1);
            },
            () =>
            {
                if (String.IsNullOrEmpty(this._currentScene))
                {
                    return "no current scene";
                }

                return String.IsNullOrEmpty(screenshotPath) ? "no valid path" : null;
            });
        }

        public void SetRecordingState(OutputState state)
        {
            this._recordingState = state;

            if (state == OutputState.OBS_WEBSOCKET_OUTPUT_PAUSED)
            {
                this.IsRecordingPaused = true;
            }
            else if (state == OutputState.OBS_WEBSOCKET_OUTPUT_RESUMED || state == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED)
            {
                this.IsRecordingPaused = false;
            }
            else if (state == OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED)
            {
                this.IsRecordingPaused = false;
            }
        }

        public void ToggleStreaming()
        {
            this.RunMutation("toggle streaming", () =>
            {
                this._log.Info("Toggling streaming");
                this._obs.ToggleStream();
            },
            () => this.IsStreamingChanging ? "state change in progress" : null);
        }

        public void StartStreaming()
        {
            this.RunMutation("start streaming", () =>
            {
                this._log.Info("Starting streaming");
                this._obs.StartStream();
            },
            () => this.IsStreaming ? "already streaming" : null);
        }

        public void StopStreaming()
        {
            this.RunMutation("stop streaming", () =>
            {
                this._log.Info("Stopping streaming");
                this._obs.StopStream();
            },
            () => this.IsStreaming ? null : "not streaming");
        }

        public void SetStreamingState(OutputState state)
        {
            this._streamingState = state;
        }

        public void ToggleVirtualCamera()
        {
            this.RunMutation("toggle virtual camera", () =>
            {
                this._log.Info("Toggling virtual camera");
                if (this.IsVirtualCameraActive)
                {
                    this._obs.StopVirtualCam();
                }
                else
                {
                    this._obs.StartVirtualCam();
                }
            });
        }

        public void StartVirtualCamera()
        {
            this.RunMutation("start virtual camera", () =>
            {
                this._log.Info("Starting virtual camera");
                this._obs.StartVirtualCam();
            },
            () => this.IsVirtualCameraActive ? "already active" : null);
        }

        public void StopVirtualCamera()
        {
            this.RunMutation("stop virtual camera", () =>
            {
                this._log.Info("Stopping virtual camera");
                this._obs.StopVirtualCam();
            },
            () => this.IsVirtualCameraActive ? null : "not active");
        }

        public void SetVirtualCameraState(OutputState state)
        {
            this._virtualCameraState = state;
        }

        public void ToggleReplayBuffer()
        {
            this.RunMutation("toggle replay buffer", () =>
            {
                this._log.Info("Toggling replay buffer");
                this._obs.ToggleReplayBuffer();
            });
        }

        public void StartReplayBuffer()
        {
            this.RunMutation("start replay buffer", () =>
            {
                this._log.Info("Starting replay buffer");
                this._obs.StartReplayBuffer();
            },
            () => this.IsReplayBufferActive ? "already active" : null);
        }

        public void StopReplayBuffer()
        {
            this.RunMutation("stop replay buffer", () =>
            {
                this._log.Info("Stopping replay buffer");
                this._obs.StopReplayBuffer();
            },
            () => this.IsReplayBufferActive ? null : "not active");
        }

        public void SaveReplayBuffer()
        {
            this.RunMutation("save replay buffer", () =>
            {
                this._log.Info("Saving replay buffer");
                this._obs.SaveReplayBuffer();
            },
            () => this.IsReplayBufferActive ? null : "not active");
        }

        public void SetReplayBufferState(OutputState state)
        {
            this._replayBufferState = state;
        }

        public String[] GetSceneItemList(String sceneName)
        {
            if (String.IsNullOrEmpty(sceneName))
            {
                this._log.Warning("Cannot get scene item list - scene name is empty");
                return new String[0];
            }

            return this.Query($"scene item list for '{sceneName}'", new String[0], () =>
            {
                this._log.Debug($"Getting scene item list for '{sceneName}'");
                return this._obs.GetSceneItemList(sceneName);
            });
        }

        // Fetch for the source visibility cache: reports failure instead of returning false, so a
        // timeout is retried later rather than cached as "hidden".
        public Boolean TryGetSceneItemEnabled(String sceneName, String sourceName, out Boolean enabled)
        {
            enabled = false;
            if (String.IsNullOrEmpty(sceneName) || String.IsNullOrEmpty(sourceName))
            {
                return false;
            }

            return this.TryQuery($"visibility of '{sourceName}' in scene '{sceneName}'",
                () => this._obs.GetSceneItemEnabled(sceneName, sourceName), out enabled);
        }

        public void ToggleSourceVisibility(String sceneName, String sourceName)
        {
            this.RunMutation($"toggle source visibility for '{sourceName}'", () =>
            {
                Boolean currentState = this._obs.GetSceneItemEnabled(sceneName, sourceName);
                this._log.Info($"Toggling source '{sourceName}' visibility from {currentState} to {!currentState}");
                this._obs.SetSceneItemEnabled(sceneName, sourceName, !currentState);
                _ = this.NotifySourceVisibilityChangedAsync(sceneName, sourceName);
            },
            () => String.IsNullOrEmpty(sceneName) || String.IsNullOrEmpty(sourceName) ? "scene or source name is empty" : null);
        }

        // Kept out of the background action so that action stays synchronous (Action<Action>):
        // the redraw waits for OBS to apply the change, but nothing should wait for the redraw.
        private async Task NotifySourceVisibilityChangedAsync(String sceneName, String sourceName)
        {
            try
            {
                await Task.Delay(OBSTimings.StateUpdateDelay);
                OBSStudioForLogiPlugin.Instance?.OnSourceVisibilityChanged(sceneName, sourceName);
            }
            catch (Exception ex)
            {
                this._log.Error($"Failed to refresh source visibility for '{sourceName}': {ex.Message}");
            }
        }

        public String[] GetInputList()
        {
            return this.Query("input list", new String[0], () =>
            {
                this._log.Debug("Getting input list");
                return this._obs.GetInputList();
            });
        }

        public Boolean GetInputMute(String inputName)
        {
            return this.Query($"input mute state for '{inputName}'", false, () => this._obs.GetInputMute(inputName));
        }

        public void ToggleInputMute(String inputName)
        {
            this.RunMutation($"toggle input mute for '{inputName}'", () =>
            {
                this._log.Info($"Toggling input mute for '{inputName}'");
                this._obs.ToggleInputMute(inputName);
            },
            () => String.IsNullOrEmpty(inputName) ? "input name is empty" : null);
        }

        public void SetInputVolume(String inputName, Single volumeMul)
        {
            this.RunMutation($"set input volume for '{inputName}'", () =>
            {
                Int32 volumePercent = (Int32)(volumeMul * 100);
                this._log.Debug($"Setting input volume for '{inputName}' to {volumePercent}% ({volumeMul:F2})");
                this._obs.SetInputVolume(inputName, volumeMul);
                this._log.Debug($"Successfully set input volume for '{inputName}'");
            },
            () => String.IsNullOrEmpty(inputName) ? "input name is empty" : null);
        }

        // Reads everything the audio buttons render for one input, for AudioStateCache. Returns null
        // instead of a safe default on failure, so a timeout is retried later rather than cached as
        // if OBS had reported "unmuted, 0 dB".
        public Models.AudioInputState TryGetInputAudioState(String inputName)
        {
            if (String.IsNullOrEmpty(inputName))
            {
                return null;
            }

            Boolean fetched = this.TryQuery($"audio state for '{inputName}'", () => new Models.AudioInputState
            {
                IsMuted = this._obs.GetInputMute(inputName),
                VolumeMul = this._obs.GetInputVolume(inputName),
                MonitorType = this._obs.GetInputAudioMonitorType(inputName)
            }, out Models.AudioInputState state);

            return fetched ? state : null;
        }

        public void CycleInputAudioMonitorType(String inputName)
        {
            this.RunMutation($"cycle audio monitor type for '{inputName}'", () =>
            {
                String current = this._obs.GetInputAudioMonitorType(inputName);
                String next;

                switch (current)
                {
                    case "OBS_MONITORING_TYPE_NONE":
                        next = "OBS_MONITORING_TYPE_MONITOR_ONLY";
                        break;
                    case "OBS_MONITORING_TYPE_MONITOR_ONLY":
                        next = "OBS_MONITORING_TYPE_MONITOR_AND_OUTPUT";
                        break;
                    default:
                        next = "OBS_MONITORING_TYPE_NONE";
                        break;
                }

                this._log.Info($"Cycling audio monitor type for '{inputName}' from {current} to {next}");
                this._obs.SetInputAudioMonitorType(inputName, next);
                // Replicate OBS UI behaviour: mute output when in Monitor Only mode
                this._obs.SetInputMute(inputName, next == "OBS_MONITORING_TYPE_MONITOR_ONLY");
            },
            () => String.IsNullOrEmpty(inputName) ? "input name is empty" : null);
        }

        // For OBSWebSocketManager.AudioInputMembership. Returns false rather than empty lists on
        // failure, so an OBS error isn't cached as "no audio inputs".
        public Boolean TryGetAudioInputSceneMembership(out Models.AudioInputSceneMembership membership)
        {
            return this.TryQuery("audio input scene membership",
                () => new Models.AudioInputSceneMembership(this._obs.GetInputList(), this._obs.GetAudioInputsNotInAnyScene()),
                out membership);
        }

        public Boolean GetStudioModeEnabled()
        {
            return this.Query("studio mode state", false, () => this._obs.GetStudioModeEnabled());
        }

        public void ToggleStudioMode()
        {
            this.RunMutation("toggle studio mode", () =>
            {
                this._log.Info($"Toggling studio mode from {this._studioModeEnabled} to {!this._studioModeEnabled}");
                this._obs.SetStudioModeEnabled(!this._studioModeEnabled);
            });
        }

        public void SetStudioModeState(Boolean enabled)
        {
            this._studioModeEnabled = enabled;
        }

        public void TriggerStudioModeTransition()
        {
            this.RunMutation("trigger studio mode transition", () =>
            {
                this._log.Info("Triggering studio mode transition");
                this._obs.TriggerStudioModeTransition();
            },
            () => this._studioModeEnabled ? null : "studio mode not enabled");
        }

        // Polled on a timer, so a disconnect isn't worth a warning on every tick.
        public Models.OBSStats GetStats()
        {
            return this.Query("stats", Models.OBSStats.Empty, () => this._obs.GetStats(), warnIfDisconnected: false);
        }

        public Models.OBSStreamStats GetStreamStatus()
        {
            return this.Query("stream status", Models.OBSStreamStats.Empty, () => this._obs.GetStreamStatus(), warnIfDisconnected: false);
        }

        // Fetch for the media state cache: reports failure instead of returning "none", so a timeout
        // is retried later rather than cached as idle.
        public Boolean TryGetMediaInputStatus(String inputName, out String state)
        {
            state = MediaInputStates.None;
            if (String.IsNullOrEmpty(inputName)
                || !this.TryQuery($"media status for '{inputName}'", () => this._obs.GetMediaInputStatus(inputName), out String fetched))
            {
                return false;
            }

            state = fetched ?? MediaInputStates.None;
            return true;
        }

        // Single tap on a media button. Reads the live state in the background rather than the
        // render cache, so a press right after connecting doesn't act on the cache's default.
        public void ToggleMediaInputPlayback(String inputName)
        {
            this.RunMutation($"toggle media playback for '{inputName}'", () =>
            {
                String state = this._obs.GetMediaInputStatus(inputName);
                String mediaAction = MediaInputStates.ToggleActionFor(state);
                this._log.Info($"Toggling media playback for '{inputName}': {state} -> '{mediaAction}'");
                this._obs.TriggerMediaInputAction(inputName, mediaAction);
            },
            () => String.IsNullOrEmpty(inputName) ? "input name is empty" : null);
        }

        public void TriggerMediaInputAction(String inputName, String mediaAction)
        {
            this.RunMutation($"trigger media action '{mediaAction}' on '{inputName}'", () =>
            {
                this._log.Info($"Triggering media action '{mediaAction}' on '{inputName}'");
                this._obs.TriggerMediaInputAction(inputName, mediaAction);
            },
            () => String.IsNullOrEmpty(inputName) || String.IsNullOrEmpty(mediaAction) ? "input name or action is empty" : null);
        }

        public String[] GetMediaInputList()
        {
            return this.Query("media input list", new String[0], () => this._obs.GetMediaInputList(), warnIfDisconnected: false);
        }

        // Every OBS mutation goes through here: it runs on the background runner, is skipped with a
        // warning while OBS isn't connected or when refusal gives a reason, and logs an OBS error
        // instead of throwing. Messages read "Cannot <action> - <reason>" and "Failed to <action>: ...".
        private void RunMutation(String action, Action call, Func<String> refusal = null)
        {
            this._runInBackground(() =>
            {
                if (!this._obs.IsConnected)
                {
                    this._log.Warning($"Cannot {action} - not connected");
                    return;
                }

                String reason = refusal?.Invoke();
                if (reason != null)
                {
                    this._log.Warning($"Cannot {action} - {reason}");
                    return;
                }

                try
                {
                    call();
                }
                catch (Exception ex)
                {
                    this._log.Error($"Failed to {action}: {ex.Message}");
                }
            });
        }

        // Reads a value on the caller's thread, returning fallback while OBS isn't connected or when
        // the request fails, so callers never see an exception.
        private T Query<T>(String what, T fallback, Func<T> call, Boolean warnIfDisconnected = true)
        {
            if (!this._obs.IsConnected)
            {
                if (warnIfDisconnected)
                {
                    this._log.Warning($"Cannot get {what} - not connected");
                }

                return fallback;
            }

            try
            {
                return call();
            }
            catch (Exception ex)
            {
                this._log.Error($"Failed to get {what}: {ex.Message}");
                return fallback;
            }
        }

        // For the render caches: reports whether the read succeeded instead of substituting a
        // default, so the caches retry a failure later rather than store it. Silent while
        // disconnected - the caches are cleared on disconnect anyway.
        private Boolean TryQuery<T>(String what, Func<T> call, out T value)
        {
            value = default;
            if (!this._obs.IsConnected)
            {
                return false;
            }

            try
            {
                value = call();
                return true;
            }
            catch (Exception ex)
            {
                this._log.Error($"Failed to get {what}: {ex.Message}");
                return false;
            }
        }
    }
}
