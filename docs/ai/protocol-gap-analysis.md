# OBS WebSocket Protocol Notes

## Overview

Notes from comparing the OBS WebSocket 5.x protocol specification (<https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md>) with the plugin and with obs-websocket-dotnet. Which calls the plugin uses is in `obs-websocket-api-complete.md`, and the planned features are in `TODO.md`. This file keeps what those two don't: how the library behaves, protocol details that shaped the code, and design notes for features not built yet.

## Library: obs-websocket-dotnet 5.7.0

None of the planned features is blocked by the library: every request and event they need exists in 5.7.0. The plugin moved from 5.0.1 to 5.7.0 for the VU meters, and the upgrade changed some behaviour:

- **Request timeouts are enforced.** 5.0.1 effectively waited forever for an unanswered request. 5.7.0 waits `WSTimeout` and throws `ErrorResponseException("Request timed out", 1)`. The plugin sets the timeout to `OBSTimings.RequestTimeout` (3s), and `OBSActionExecutor`'s helpers log the exception.
- **`MediaInputPlaybackStarted` carries the input name.** 5.0.1 read the wrong field, so the name was always `null`.
- **`Identify` sends `eventSubscriptions`.** High-volume events such as `InputVolumeMeters` subscribe when the first handler is attached and unsubscribe when the last is removed, each through a `ReIdentify`. See `vu-meters-learnings.md`.
- **`Connected` is raised on every `ReIdentify`.** This is a library bug, worked around by `SessionGate`. The fix is on the `mrnil/obs-websocket-dotnet` branch `fix/connected-raised-on-reidentify` (#24).
- `IsIdentified` is a better "ready for requests" check than `IsConnected`, and the adapter's `IsConnected` requires it.
- 5.7.0 adds requests and events the plugin could use: `SetRecordDirectory`, `SplitRecordFile`, `CreateRecordChapter`, `RecordFileChanged`, `ScreenshotSaved`, `GetSourceFilterKindList`, `SourceFilterSettingsChanged`, `InputSettingsChanged`, the generic output requests (`GetOutputList`, `ToggleOutput`, ...), `GetSceneItemSource` and the canvas requests.
- Still **not** supported: `RequestBatch` (see below).

The library's local clone is at `B:\development\obs-websocket-dotnet`. To test a library change in the plugin, replace the `PackageReference` in `src/OBSStudioForLogiPlugin.csproj` with a `ProjectReference` to `..\..\obs-websocket-dotnet\obs-websocket-dotnet\obs-websocket-dotnet.csproj`, and switch back before committing.

## Protocol Details

### Scene Item ID vs Source Name

The protocol addresses scene items by `sceneItemId`. The adapter's `GetSceneItems` reads `GetSceneItemList`'s raw response, which gives each item's ID, source name and `sceneItemEnabled` in one request; the library's `SceneItemDetails` drops the visibility. A toggle is that list plus `SetSceneItemEnabled` by ID (#30). A source that is in a scene more than once is handled by its first item, which is the one the buttons show. `OBSWebSocketManager` ignores visibility events for the later duplicates for the same reason.

### Volume Range

`inputVolumeMul` runs from 0.0 to about 20.0, and `inputVolumeDb` from -inf to +26dB. The plugin supports the full range and shows it in dB (`VolumeConverter`).

### Media Input Status

`GetMediaInputStatus` returns `mediaState`, `mediaDuration` and `mediaCursor` (both in ms). The plugin reads only `mediaState`. The other two could show elapsed and total time on media buttons.

### Audio Input Detection

`OBSWebsocketAdapter.AudioInputKinds` is an allow-list. `monitor_capture`, which captures desktop audio on some platforms, is missing, and inputs from OBS plugins never match (#35). A sturdier test is whether `GetInputVolume` succeeds: it fails for inputs without audio.

### Capability Negotiation

`GetVersion` returns `obsVersion`, `obsWebSocketVersion`, `rpcVersion` and `availableRequests`. The plugin could use it to disable features the connected OBS doesn't support.

### Batch Requests

The protocol's `RequestBatch` runs several requests in order. `SceneSwitchAdjustableCommand` (profile, then collection, then scene) could use it, but obs-websocket-dotnet has no batch API, so it would need a library contribution first.

### Unencrypted Connection

The handshake uses SHA256 challenge-response, so the password itself isn't sent. The connection is `ws://`, not `wss://`, so everything after the handshake is unencrypted. This only matters for remote connections, and `USER_MANUAL.md` advises a trusted network.

## Design Notes for Future Features

### Transitions

```text
GetSceneTransitionList → [{transitionName, transitionKind, transitionFixed}]
GetCurrentSceneTransition → {transitionName, transitionKind, transitionFixed, transitionDuration, transitionConfigurable}
SetCurrentSceneTransition(transitionName)
SetCurrentSceneTransitionDuration(transitionDuration)
GetCurrentSceneTransitionCursor → {transitionCursor} (0.0-1.0)
SetTBarPosition(position, release) — position 0.0-1.0
```

- **Transition folder**: a dynamic folder listing the transitions; tap one to select it
- **T-bar**: a `PluginDynamicAdjustment` on a dial that calls `SetTBarPosition`, releasing at the end
- **Duration**: an adjustment, or an `ActionEditorCommand` with a duration field

### Source Filters

```text
GetSourceFilterList(sourceName) → [{filterEnabled, filterIndex, filterKind, filterName, filterSettings}]
SetSourceFilterEnabled(sourceName, filterName, filterEnabled)
GetSourceFilterDefaultSettings(filterKind) → {defaultFilterSettings}
SetSourceFilterSettings(sourceName, filterName, filterSettings, overlay)
```

- **Filter folder per source**: an `ActionEditorCommand` naming the source opens a folder of its filters
- **Filter toggle**: each button toggles `filterEnabled`, kept current by `SourceFilterEnableStateChanged`
- **State**: blue when enabled, grey when disabled, following `icon-style.md`
