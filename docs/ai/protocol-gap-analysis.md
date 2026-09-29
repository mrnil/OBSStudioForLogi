# OBS WebSocket Protocol Gap Analysis

## Overview

Comparison of the OBS WebSocket 5.x protocol specification (<https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md>) against the current plugin implementation. Originally analysed against obs-websocket-dotnet v5.0.1; updated 2026-09-27 for the upgrade to v5.7.0 (see "Library Upgrade: 5.0.1 → 5.7.0" below).

## Library Upgrade: 5.0.1 → 5.7.0

None of the gaps prioritised below were blocked by the library. Every request and event they need already existed in 5.0.1 and still exists in 5.7.0, so the remaining work is all on the plugin side. The upgrade does change existing behaviour and add new API surface, as follows.

### Behaviour Changes Affecting the Plugin

- **Request timeouts are now enforced.** 5.0.1 called `tcs.Task.Wait(wsTimeout.Milliseconds)`, which is the milliseconds *component* of the default 10s `WSTimeout` (i.e. `0`), then blocked on `tcs.Task.Result` with no timeout at all, so a request OBS never answered would hang forever. 5.7.0 waits `WSTimeout.TotalMilliseconds` and throws `ErrorResponseException("Request timed out", 1)`. `OBSActionExecutor` catches `Exception` around its OBS calls, so this surfaces as a logged error instead of a hung thread.
- **`MediaInputPlaybackStarted` now carries the input name.** 5.0.1 read `sourceName` instead of `inputName` from the event body, so `InputName` was always `null` and `OBSWebSocketManager.OnMediaInputPlaybackStarted` returned early every time. 5.7.0 reads the correct field, so media buttons now refresh when playback starts (previously they relied on `MediaInputPlaybackEnded` and polling).
- **`Identify` sends `eventSubscriptions`.** The subscription bitmask is now explicit, and high-volume events (`InputVolumeMeters` etc.) subscribe automatically when a handler is attached. This is what the VU meters depend on; see `vu-meters-learnings.md`.
- Other fixes that don't affect current plugin code: `GetGroupSceneItemList`, `GetInputAudioTracks` (previously returned `false` for every track), string conversion in several event args, and `SHA256Managed` replaced with `SHA256.Create()`.

### New API Surface Worth Considering

| Addition | Kind | Possible use |
|----------|------|--------------|
| `SetRecordDirectory` | Request | Completes gap #10 (only `GetRecordDirectory` existed in 5.0.1) |
| `SplitRecordFile`, `CreateRecordChapter` | Request | One-tap buttons for recording workflows (chapters need OBS 30.2+ and Hybrid MP4 recording) |
| `RecordFileChanged` | Event | Confirmation feedback when a recording splits |
| `ScreenshotSaved` | Event | Confirmation feedback for a screenshot button |
| `GetSourceFilterKindList`, `SourceFilterSettingsChanged` | Request / event | Supports the source filter folder (gap #3) |
| `InputSettingsChanged` | Event | Refresh input-dependent buttons without polling |
| `GetOutputList`, `GetOutputStatus`, `ToggleOutput`, `StartOutput`, `StopOutput`, `Get/SetOutputSettings` | Request | Control of arbitrary outputs (e.g. third-party output plugins such as multi-RTMP) |
| `GetSceneItemSource` | Request | Resolve scene item ID → source name |
| `Get/SetInputDeinterlaceMode`, `Get/SetInputDeinterlaceFieldOrder` | Request | Low priority, niche |
| `GetCanvasList`, `CanvasCreated`/`CanvasRemoved`/`CanvasNameChanged` | Request / event | Multi-canvas support (obs-websocket protocol 5.7.0); not needed yet |
| `CustomEvent` | Event | Receives `BroadcastCustomEvent` payloads from other clients |
| `IsIdentified`, `EventSubscriptions` | Property | `IsIdentified` is a more accurate "ready for requests" check than `IsConnected` |

Still **not** supported in 5.7.0: `RequestBatch` (see Protocol Details §6).

## Bugs Fixed

### Duplicate Mute Logic in CycleInputAudioMonitorType (Fixed)

The `SetInputMute` call appeared twice in `OBSActionExecutor.CycleInputAudioMonitorType` due to copy-paste. Collapsed to a single call. The auto-mute behaviour is intentional — it replicates the OBS UI which mutes output when switching to Monitor Only mode.

## Known Implementation Gaps

### Missing Protocol Features (Prioritised)

| Priority | Feature | Effort | Impact | Notes |
|----------|---------|--------|--------|-------|
| ~~1~~ | ~~Volume range >100% support~~ | ~~Low~~ | ~~Medium~~ | ✅ Done — displays dB, allows 0.0-20.0 |
| 2 | Transition selection + T-bar encoder | Medium | High | `GetSceneTransitionList`, `SetCurrentSceneTransition`, `SetTBarPosition` — uniquely suited to hardware dials |
| 3 | Source filter toggle folder | Medium | High | `GetSourceFilterList`, `SetSourceFilterEnabled` — high demand from streamers |
| 4 | Hotkey trigger (ActionEditorCommand) | Low | Medium | `TriggerHotkeyByName` — power user feature |
| 5 | Media duration/cursor display | Low | Medium | `GetMediaInputStatus` returns `mediaDuration` and `mediaCursor` — not extracted |
| ~~6~~ | ~~Subscribe to `ReplayBufferSaved` event~~ | ~~Trivial~~ | ~~Low~~ | ✅ Done — green icon flash for 2s on save |
| ~~7~~ | ~~Subscribe to `ProfileListChanged` + `SceneCollectionListChanged`~~ | ~~Low~~ | ~~Medium~~ | ✅ Done — lists pushed from the event (assessment #8) |
| 8 | Recording status/duration display | Low | Medium | `GetRecordStatus` returns timecode and bytes |
| 9 | Broader audio input detection | Low | Low | `browser_source`, `game_capture` and `wasapi_process_output_capture` are now in `AudioInputKinds`; `monitor_capture` still missing |
| 10 | `GetRecordDirectory` / `SetRecordDirectory` | Low | Low | Display/change recording save path; `SetRecordDirectory` added in library 5.7.0 |
| 11 | Split recording / add chapter | Low | Medium | `SplitRecordFile`, `CreateRecordChapter` (library 5.7.0; chapters need OBS 30.2+ with Hybrid MP4) |

### Missing Events Worth Subscribing To

| Event | Value | Use Case |
|-------|-------|----------|
| `CurrentPreviewSceneChanged` | High (studio mode users) | Show preview scene, update preview folder |
| ~~`ReplayBufferSaved`~~ | ~~Medium~~ | ✅ Done — shows green save confirmation icon |
| ~~`SceneCollectionListChanged`~~ | ~~Medium~~ | ✅ Done — pushes the new list to the folders and select commands (assessment #8) |
| ~~`ProfileListChanged`~~ | ~~Medium~~ | ✅ Done — pushes the new list to the folders and select commands (assessment #8) |
| `SceneTransitionStarted` / `SceneTransitionEnded` | Medium | Visual feedback during transitions |
| ~~`InputNameChanged`~~ | ~~Medium~~ | ✅ Done — moves cached state to the new name and refreshes the input lists (assessment #21); `SceneNameChanged` is handled the same way |
| `SourceFilterEnableStateChanged` | Medium (if filters implemented) | Update filter button state |
| ~~`MediaInputActionTriggered`~~ | ~~Low~~ | ✅ Done — keeps the media state cache current on pause/resume/stop |
| `RecordFileChanged` | Low (new in library 5.7.0) | Confirm a recording split |
| `ScreenshotSaved` | Low (new in library 5.7.0) | Confirm a screenshot was saved |
| ~~`InputVolumeMeters`~~ | ~~High~~ | ✅ Done — drives the audio VU meters (library 5.7.0 required) |

## Protocol Details to Address

### 1. Scene Item ID vs Source Name

The protocol uses `sceneItemId` (Int32) for all scene item operations. The adapter resolves name→ID internally via `GetSceneItemList`. However, if a source appears multiple times in a scene (duplicates), `FirstOrDefault` returns the first match only. This is an edge case but could cause incorrect toggling.

### 2. Volume Range

- Protocol: `inputVolumeMul` range is 0.0 to ~20.0 (0% to ~2000%)
- Protocol: `inputVolumeDb` range is -inf to +26.0 dB
- ✅ **Resolved**: Plugin now supports full 0.0-20.0 range, displays in dB format

### 3. Media Input Status

Protocol `GetMediaInputStatus` returns:

- `mediaState` (string) ✅ extracted — used by `MediaDynamicFolder` for play/pause/stop logic
- `mediaDuration` (int, ms) ❌ not extracted
- `mediaCursor` (int, ms) ❌ not extracted

Duration/cursor could display elapsed/total time on media buttons.

### 4. Audio Input Detection

`AudioInputKinds` (in `OBSWebsocketAdapter`) now includes `browser_source`, `game_capture` and `wasapi_process_output_capture`. Still missing:

- `monitor_capture` — can capture desktop audio on some platforms

A more robust approach: attempt `GetInputVolume` on each input; non-audio sources throw an error.

### 5. GetVersion / Capability Negotiation

Protocol `GetVersion` returns:

- `obsVersion` — OBS Studio version
- `obsWebSocketVersion` — WebSocket plugin version
- `rpcVersion` — protocol version
- `availableRequests` — array of supported request names

Could use this to gracefully disable features not supported by the connected OBS version.

### 6. Batch Requests

Protocol supports `RequestBatch` for atomic multi-request operations. The `SceneSwitchAdjustableCommand` (profile→collection→scene with delays) could benefit, but obs-websocket-dotnet still has no batch request API as of 5.7.0; this would need a library contribution first.

## Security Note

OBS WebSocket uses SHA256 challenge-response authentication (handled by library) but the connection itself is unencrypted (ws:// not wss://). For remote connections, the password traverses the network in plaintext during the initial handshake. Consider noting this in the Plugin Settings UI for remote configurations.

## Transition Control (Design Notes for Future)

The protocol provides full transition control:

```
GetSceneTransitionList → [{transitionName, transitionKind, transitionFixed}]
GetCurrentSceneTransition → {transitionName, transitionKind, transitionFixed, transitionDuration, transitionConfigurable}
SetCurrentSceneTransition(transitionName)
SetCurrentSceneTransitionDuration(transitionDuration)
GetCurrentSceneTransitionCursor → {transitionCursor} (0.0-1.0)
SetTBarPosition(position, release) — position 0.0-1.0
```

Implementation approach:

- **Transition folder**: Dynamic folder listing available transitions, tap to select
- **T-bar adjustment**: `PluginDynamicAdjustment` mapped to encoder, calls `SetTBarPosition`
- **Duration adjustment**: ActionEditorCommand with duration textbox or encoder

## Source Filters (Design Notes for Future)

```
GetSourceFilterList(sourceName) → [{filterEnabled, filterIndex, filterKind, filterName, filterSettings}]
SetSourceFilterEnabled(sourceName, filterName, filterEnabled)
GetSourceFilterDefaultSettings(filterKind) → {defaultFilterSettings}
SetSourceFilterSettings(sourceName, filterName, filterSettings, overlay)
```

Implementation approach:

- **Filter folder per source**: ActionEditorCommand with source name textbox, opens folder of filters
- **Filter toggle**: Each button in folder toggles filterEnabled
- **Visual feedback**: Green = enabled, grey = disabled
