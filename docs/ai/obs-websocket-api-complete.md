# OBS WebSocket API — Plugin Coverage

## Overview

Which OBS WebSocket 5.x requests and events the plugin uses, through obs-websocket-dotnet 5.7.0. Everything the plugin uses goes through `IOBSWebsocket` (`src/Services/IOBSWebsocket.cs`), so that file is the authoritative list; this one adds which feature uses each call. Planned features are in `TODO.md`, and library notes and design ideas are in `protocol-gap-analysis.md`.

**Legend:** ✅ used, 🟡 partly used, ❌ not used.

---

## 1. General / Connection

| Feature | Status | Notes |
|---------|--------|-------|
| Connect / disconnect | ✅ | `ConnectionManager` with config discovery; the only place that retries |
| Reconnection with backoff | ✅ | `ReconnectionStrategy`: 1s, 2s, 4s, 8s, 15s, 30s with ±15% jitter |
| Authentication | ✅ | Local password from OBS's config; remote password from the SDK's encrypted plugin settings |
| `GetVersion` | ❌ | Could disable features the connected OBS doesn't support |
| `GetStats` | ✅ | `StatsService` polling, shown by Stats Summary and Stats Folder |

---

## 2. Outputs

| Feature | Status | Implementation |
|---------|--------|----------------|
| Start/stop/toggle stream | ✅ | `StreamingStartCommand`, `StreamingStopCommand`, `StreamingToggleCommand` |
| `GetStreamStatus` | ✅ | Stream Stats Folder (polled) |
| Get/set stream settings | ❌ | |
| Start/stop/toggle/pause/resume record | ✅ | `RecordingStartCommand`, `RecordingStopCommand`, `RecordingToggleCommand`, `RecordingPauseToggleCommand` |
| `GetRecordStatus` | ❌ | Recording duration display (#9) |
| `GetRecordDirectory` / `SetRecordDirectory` | ❌ | `SetRecordDirectory` is new in library 5.7.0 |
| `SplitRecordFile`, `CreateRecordChapter` | ❌ | New in library 5.7.0; chapters need OBS 30.2+ with Hybrid MP4 |
| Start/stop/toggle virtual camera | ✅ | `VirtualCameraStartCommand`, `VirtualCameraStopCommand`, `VirtualCameraToggleCommand` |
| Start/stop/toggle/save replay buffer | ✅ | `ReplayBufferToggleCommand`, `ReplayBufferSaveCommand` |
| `GetLastReplayBufferReplay` | ❌ | |
| `GetOutputList`, `ToggleOutput` and the other generic output requests | ❌ | Would reach third-party outputs such as multi-RTMP plugins |

---

## 3. Scenes and Scene Items

| Feature | Status | Implementation |
|---------|--------|----------------|
| `GetSceneList`, `GetCurrentProgramScene` | ✅ | Initial state load; Scenes folder, select command, Current Scene display |
| `SetCurrentProgramScene` | ✅ | Scene buttons while studio mode is off |
| `SetCurrentPreviewScene` | ✅ | Scene buttons while studio mode is on |
| `GetCurrentPreviewScene` | ❌ | Needed for studio mode feedback (#31) |
| Create/remove/rename scene | ❌ | |
| `GetSceneItemList` | ✅ | Read raw, so each item's ID and visibility come in one request (#30) |
| `GetSceneItemEnabled` | ✅ | Visibility cache fetch |
| `SetSceneItemEnabled` | ✅ | Scene Sources folder, Toggle Source Visibility (User defined); by item ID |
| `GetGroupSceneItemList` | ❌ | Groups aren't expanded (#28) |
| Scene item transform, index, `GetSceneItemSource` | ❌ | |

---

## 4. Scene Collections and Profiles

| Feature | Status | Implementation |
|---------|--------|----------------|
| `GetSceneCollectionList`, `SetCurrentSceneCollection` | ✅ | Scene Collections folder, select command, display |
| `GetProfileList`, `SetCurrentProfile` | ✅ | Profiles folder, select command, display |
| Create/remove collection or profile, profile parameters | ❌ | |

---

## 5. Inputs and Audio

| Feature | Status | Implementation |
|---------|--------|----------------|
| `GetInputList` | 🟡 | Filtered to the `AudioInputKinds` and media allow-lists in `OBSWebsocketAdapter` (#35) |
| Input settings, create/remove/rename input | ❌ | |
| `GetInputMute`, `ToggleInputMute` | ✅ | Audio folders, Audio Meter, Toggle Audio Mute (User defined) |
| `SetInputMute` | ✅ | Only to mute an input switched to Monitor Only, as OBS's own mixer does |
| `GetInputVolume`, `SetInputVolume` | ✅ | Audio Volume folder, Selected Source Volume; full 0.0–20.0 range shown in dB |
| `GetInputAudioMonitorType`, `SetInputAudioMonitorType` | ✅ | Cycle Audio Monitoring (User defined), encoder press in the audio folders |
| Audio balance, sync offset, tracks | ❌ | |
| `InputVolumeMeters` (high-volume event) | ✅ | Live Audio Folder and Audio Meter, subscribed only while a meter is on screen |

---

## 6. Filters

| Feature | Status | Notes |
|---------|--------|-------|
| All source filter requests | ❌ | Filter toggle folder planned (`TODO.md`); design notes in `protocol-gap-analysis.md` |

---

## 7. Transitions and Studio Mode

| Feature | Status | Implementation |
|---------|--------|----------------|
| `GetStudioModeEnabled`, `SetStudioModeEnabled` | ✅ | `StudioModeToggleCommand` |
| `TriggerStudioModeTransition` | ✅ | `StudioModeTransitionCommand` |
| Transition list, current transition, duration, settings | ❌ | Planned (`TODO.md`) |
| `SetTBarPosition` | ❌ | Planned for a dial |

---

## 8. Media Inputs

| Feature | Status | Implementation |
|---------|--------|----------------|
| `GetMediaInputStatus` | 🟡 | `mediaState` drives the Media Controls folder; `mediaCursor` and `mediaDuration` aren't read |
| `TriggerMediaInputAction` | ✅ | Media Controls folder (play, pause, stop, restart), Media Action (User Defined) (also next, previous) |
| `SetMediaInputCursor`, `OffsetMediaInputCursor` | ❌ | |

---

## 9. Screenshots, Hotkeys and UI

| Feature | Status | Implementation |
|---------|--------|----------------|
| `SaveSourceScreenshot` | ✅ | `ScreenshotCommand` |
| `GetSourceScreenshot` | ❌ | |
| Hotkey requests | ❌ | `TriggerHotkeyByName` planned |
| Projectors, properties/filters/interact dialogs, monitor list | ❌ | `OpenSourceProjector` planned |

---

## 10. Events

### Subscribed

Every handler is in `OBSWebSocketManager` and is unsubscribed in `Dispose`, except `InputVolumeMeters` (#37).

| Event | Used for |
|-------|----------|
| `Connected`, `Disconnected` | Connection state, initial state load (`SessionGate`, `InitialStateLoader`) |
| `StreamStateChanged`, `RecordStateChanged`, `VirtualcamStateChanged`, `ReplayBufferStateChanged` | Output buttons, including starting/stopping states |
| `ReplayBufferSaved` | Save confirmation flash |
| `CurrentProfileChanged`, `ProfileListChanged` | Profile folder, select command, display |
| `CurrentSceneCollectionChanging`, `CurrentSceneCollectionChanged`, `SceneCollectionListChanged` | Collection buttons; pausing stats polling during a switch |
| `SceneListChanged`, `CurrentProgramSceneChanged`, `SceneNameChanged` | Scene buttons; renames move cached state |
| `StudioModeStateChanged` | Studio mode toggle |
| `SceneItemEnableStateChanged`, `SceneItemCreated`, `SceneItemRemoved` | Visibility cache, scene source lists, audio membership cache |
| `InputCreated`, `InputRemoved`, `InputNameChanged` | Input lists; renames move cached state |
| `InputMuteStateChanged`, `InputVolumeChanged`, `InputAudioMonitorTypeChanged` | `AudioStateCache` and audio buttons |
| `MediaInputPlaybackStarted`, `MediaInputPlaybackEnded`, `MediaInputActionTriggered` | Media state cache |
| `InputVolumeMeters` | Audio meters, only while one is on screen |

### Not Subscribed

| Event | Would be used for |
|-------|-------------------|
| `CurrentPreviewSceneChanged` | Studio mode preview feedback (#31) |
| `CurrentSceneTransitionChanged`, `CurrentSceneTransitionDurationChanged` | Transition display |
| `SceneTransitionStarted`, `SceneTransitionEnded`, `SceneTransitionVideoEnded` | Transition progress |
| `SourceFilterCreated`, `SourceFilterRemoved`, `SourceFilterEnableStateChanged`, `SourceFilterListReindexed` | Filter folder |
| `InputAudioBalanceChanged`, `InputAudioSyncOffsetChanged`, `InputAudioTracksChanged` | Advanced audio controls |
| `InputSettingsChanged` | Refreshing input-dependent buttons |
| `SceneItemTransformChanged` | Transform controls |
| `RecordFileChanged` | Confirming a recording split |
| `ScreenshotSaved` | Confirming a screenshot |

---

## Version Information

- **OBS WebSocket protocol**: 5.x
- **obs-websocket-dotnet**: 5.7.0
- **OBS Studio**: 28.0+
