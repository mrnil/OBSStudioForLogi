# OBS WebSocket Protocol Gap Analysis

## Overview

Comparison of the OBS WebSocket 5.x protocol specification (<https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md>) against the current plugin implementation. Analysis performed against obs-websocket-dotnet v5.0.1.

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
| 7 | Subscribe to `ProfileListChanged` + `SceneCollectionListChanged` | Low | Medium | Avoid stale lists when profiles/collections created/removed |
| 8 | Recording status/duration display | Low | Medium | `GetRecordStatus` returns timecode and bytes |
| 9 | Broader audio input detection | Medium | Medium | Current filter list misses `browser_source`, `game_capture`, `monitor_capture` |
| 10 | `GetRecordDirectory` / `SetRecordDirectory` | Low | Low | Display/change recording save path |

### Missing Events Worth Subscribing To

| Event | Value | Use Case |
|-------|-------|----------|
| `CurrentPreviewSceneChanged` | High (studio mode users) | Show preview scene, update preview folder |
| ~~`ReplayBufferSaved`~~ | ~~Medium~~ | ✅ Done — shows green save confirmation icon |
| `SceneCollectionListChanged` | Medium | Keep collection list current without reconnect |
| `ProfileListChanged` | Medium | Keep profile list current without reconnect |
| `SceneTransitionStarted` / `SceneTransitionEnded` | Medium | Visual feedback during transitions |
| `InputNameChanged` | Medium | Keep audio/source lists in sync without full rebuild |
| `SourceFilterEnableStateChanged` | Medium (if filters implemented) | Update filter button state |
| `MediaInputActionTriggered` | Low | Confirm media actions executed |

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

Current `AudioInputKinds` filter may miss sources that have audio:

- `browser_source` — has audio output
- `game_capture` — captures game audio
- `monitor_capture` — can capture desktop audio on some platforms
- `wasapi_process_output_capture` — newer per-app audio capture (OBS 28+)

A more robust approach: attempt `GetInputVolume` on each input; non-audio sources throw an error.

### 5. GetVersion / Capability Negotiation

Protocol `GetVersion` returns:

- `obsVersion` — OBS Studio version
- `obsWebSocketVersion` — WebSocket plugin version
- `rpcVersion` — protocol version
- `availableRequests` — array of supported request names

Could use this to gracefully disable features not supported by the connected OBS version.

### 6. Batch Requests

Protocol supports `RequestBatch` for atomic multi-request operations. The `SceneSwitchAdjustableCommand` (profile→collection→scene with delays) could benefit, though obs-websocket-dotnet library support would need verification.

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
