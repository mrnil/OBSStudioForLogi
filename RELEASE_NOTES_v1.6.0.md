# Release Notes — v1.6.0

## New Features

### Scene Select Command
A new `Scene Select` multi-state button is available in **7. Scenes › Available Scenes**. Each button represents a scene in the current collection. The active scene is highlighted with a selected indicator. Pressing a button switches to that scene using the studio-mode-aware switch (preview when studio mode is on, program when off). This mirrors the existing Profile Select and Scene Collection Select commands.

### Audio Source Select Command
A new `Audio Source Select` multi-state button is available in **8. Audio › Available Sources**. Each button represents an audio input. Buttons display the source name, volume in dB, and mute state (green = unmuted, red = muted) — identical rendering to the Audio Mixer folder. Pressing a button toggles mute for that source. The button list updates automatically when audio inputs are added or removed in OBS.

### Scene Collections Dynamic Folder
A new `OBS Scene Collections` dynamic folder is available in **7. Scenes › Available Collections**, providing a folder-based alternative to the Scene Collection Select multi-state buttons.

## UI Reorganisation

Actions are now grouped into functional sub-groups within their top-level group, making the Loupedeck software action picker easier to navigate.

### Scenes (7. Scenes)
| Sub-group | Actions |
|---|---|
| Available Scenes | Scene Select (new multi-state buttons) |
| Available Collections | Scene Collection Select, OBS Scene Collections folder |
| User Defined | Switch to Scene, Toggle Source Visibility |

### Profiles (6. Profiles)
| Sub-group | Actions |
|---|---|
| Available Profiles | Profile Select, OBS Profiles folder |

### Audio (8. Audio)
| Sub-group | Actions |
|---|---|
| Available Sources | Audio Source Select (new multi-state buttons) |
| User Defined | Toggle Audio Mute, Cycle Audio Monitoring, Select Audio Source, Audio Source Status |

Previously all user-defined actions were grouped under `99. User Defined Actions`. That group is now empty and has been retired — all configurable actions live alongside their related controls.

## Bug Fixes

### DoubleTapHelper Race Condition and Memory Leak (Assessment #4)
- Fixed a race condition where `_tapStates` was accessed from both the calling thread and background `Task.Run` threads without synchronisation. All dictionary access is now protected with `lock(_tapStates)`.
- Fixed a `CancellationTokenSource` leak — cancelled token sources are now disposed in all code paths (single-tap `finally`, double-tap path, and `Reset()`).

### OBSStats Null-Object Pattern (Assessment #7)
- `OBSStats.Empty` and `OBSStreamStats.Empty` static properties added. `OBSActionExecutor` and `OBSFacade` now return `Empty` instead of `null` when disconnected or on error.
- Null guards removed from `StatsDisplay`, `StatsDynamicFolder`, and `StreamStatsDynamicFolder` — these now always receive a valid object.

### MediaDynamicFolder Input List Updates (Assessment #10)
- `MediaDynamicFolder` now implements `IInputsListAwareCommand`. When media sources are added or removed in OBS while connected, the folder updates automatically without requiring a reconnect.

### CommandRegistry Bypass Fixed (Assessment #1)
- `OBSStudioForLogiPlugin.OnOBSConnected` and `OnOBSDisconnected` now route through `CommandCoordinator.NotifyConnected()` / `NotifyDisconnected()`. Hardcoded singleton calls in `OBSWebSocketManager` removed. New commands that self-register via `IObsCommand` will correctly receive connection lifecycle events.

### Scene Sources Registry Bypass Fixed (Assessment #2)
- `OnCurrentSceneChanged` now routes scene source updates through the `CommandRegistry` via `ISceneSourcesAwareCommand` rather than calling `SourcesDynamicFolder.Instance` and `SceneAudioSourcesDynamicFolder.Instance` directly.

## Security

### Password Field Sensitivity Indication (Assessment #11)
- The password field in Plugin Settings is now labelled `Password (sensitive)` to clearly indicate the field contains sensitive data.

## Architecture

- 389 unit tests (up from 362 at v1.5.1)
- All new commands follow the established TDD pattern — tests written before implementation
- `99. User Defined Actions` group retired; all configurable actions moved to functional groups

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 8.0 SDK (for development only)

## Installation

Download `OBSStudioForLogiPlugin-v1.6.0.lplug4` and install via Logi Options+ or Loupedeck software.
