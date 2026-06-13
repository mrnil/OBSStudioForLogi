# OBS WebSocket API - Complete Feature Reference

## Overview

This document catalogs all available features in the OBS WebSocket 5.x API (via obs-websocket-dotnet 5.0.1) and tracks which features are implemented in the OBSStudioForLogiPlugin.

**Legend:**

- ✅ Fully Implemented
- 🟡 Partially Implemented
- ❌ Not Implemented

---

## 1. General / Connection

| Feature | Status | Notes |
|---------|--------|-------|
| Connect to WebSocket | ✅ | Automatic connection with config discovery |
| Disconnect | ✅ | Manual disconnect supported |
| Reconnection with backoff | ✅ | Exponential backoff (1s-30s) with jitter |
| Authentication | ✅ | Password from OBS config |
| Get Version | ❌ | Not exposed to plugin |
| Get Stats | ❌ | Not implemented |

---

## 2. Outputs (Streaming/Recording/Virtual Camera)

### Streaming

| Feature | Status | Implementation |
|---------|--------|----------------|
| Start Streaming | ✅ | `StreamingStartCommand` |
| Stop Streaming | ✅ | `StreamingStopCommand` |
| Toggle Streaming | ✅ | `StreamingToggleCommand` |
| Get Streaming Status | ✅ | `IsStreaming` property |
| Stream State Events | ✅ | `StreamStateChanged` event |
| Get Stream Settings | ❌ | Not implemented |
| Set Stream Settings | ❌ | Not implemented |

### Recording

| Feature | Status | Implementation |
|---------|--------|----------------|
| Start Recording | ✅ | `RecordingStartCommand` |
| Stop Recording | ✅ | `RecordingStopCommand` |
| Toggle Recording | ✅ | `RecordingToggleCommand` |
| Pause Recording | ✅ | `RecordingPauseToggleCommand` |
| Resume Recording | ✅ | `RecordingPauseToggleCommand` |
| Get Recording Status | ✅ | `IsRecording`, `IsRecordingPaused` |
| Record State Events | ✅ | `RecordStateChanged` event |
| Get Recording Folder | ❌ | Not implemented |
| Set Recording Folder | ❌ | Not implemented |

### Virtual Camera

| Feature | Status | Implementation |
|---------|--------|----------------|
| Start Virtual Camera | ✅ | `VirtualCameraStartCommand` |
| Stop Virtual Camera | ✅ | `VirtualCameraStopCommand` |
| Toggle Virtual Camera | ✅ | `VirtualCameraToggleCommand` |
| Get Virtual Camera Status | ✅ | `IsVirtualCameraActive` property |
| Virtual Camera State Events | ✅ | `VirtualcamStateChanged` event |

### Replay Buffer

| Feature | Status | Implementation |
|---------|--------|----------------|
| Start Replay Buffer | ✅ | `ReplayBufferToggleCommand` |
| Stop Replay Buffer | ✅ | `ReplayBufferToggleCommand` |
| Toggle Replay Buffer | ✅ | `ReplayBufferToggleCommand` |
| Save Replay Buffer | ✅ | `ReplayBufferSaveCommand` |
| Get Replay Buffer Status | ✅ | `IsReplayBufferActive` property |
| Replay Buffer State Events | ✅ | `ReplayBufferStateChanged` event |
| Get Last Replay | ❌ | Not implemented |

---

## 3. Scenes

| Feature | Status | Implementation |
|---------|--------|----------------|
| Get Scene List | ✅ | `GetSceneList()` |
| Get Current Program Scene | ✅ | `CurrentScene` property |
| Set Current Program Scene | ✅ | `ScenesDynamicFolder` |
| Get Current Preview Scene | ❌ | Not implemented |
| Set Current Preview Scene | ✅ | Used in studio mode |
| Scene List Changed Event | ✅ | `SceneListChanged` event |
| Current Scene Changed Event | ✅ | `CurrentProgramSceneChanged` event |
| Create Scene | ❌ | Not implemented |
| Remove Scene | ❌ | Not implemented |
| Set Scene Name | ❌ | Not implemented |
| Get Scene Item List | ✅ | `GetSceneItemList()` |
| Get Scene Item Enabled | ✅ | `GetSceneItemEnabled()` |
| Set Scene Item Enabled | ✅ | `SourcesDynamicFolder` visibility toggle |
| Get Scene Item Transform | ❌ | Not implemented |
| Set Scene Item Transform | ❌ | Not implemented |
| Get Scene Item Index | ❌ | Not implemented |
| Set Scene Item Index | ❌ | Not implemented |

---

## 4. Scene Collections

| Feature | Status | Implementation |
|---------|--------|----------------|
| Get Scene Collection List | ✅ | `GetSceneCollectionList()` |
| Get Current Scene Collection | ✅ | `CurrentSceneCollection` property |
| Set Current Scene Collection | ✅ | `SceneCollectionSelectCommand` |
| Scene Collection Changed Event | ✅ | `CurrentSceneCollectionChanged` event |
| Create Scene Collection | ❌ | Not implemented |
| Remove Scene Collection | ❌ | Not implemented |

---

## 5. Profiles

| Feature | Status | Implementation |
|---------|--------|----------------|
| Get Profile List | ✅ | `GetProfileList()` |
| Get Current Profile | ✅ | `CurrentProfile` property |
| Set Current Profile | ✅ | `ProfileSelectCommand`, `ProfilesDynamicFolder` |
| Profile Changed Event | ✅ | `CurrentProfileChanged` event |
| Create Profile | ❌ | Not implemented |
| Remove Profile | ❌ | Not implemented |
| Get Profile Parameter | ❌ | Not implemented |
| Set Profile Parameter | ❌ | Not implemented |

---

## 6. Inputs (Sources)

### Input Discovery

| Feature | Status | Implementation |
|---------|--------|----------------|
| Get Input List | ✅ | `GetInputList()` - filtered for audio |
| Get Input Kind | ✅ | `GetInputKind()` |
| Get Input Settings | ❌ | Not implemented |
| Set Input Settings | ❌ | Not implemented |
| Get Input Default Settings | ❌ | Not implemented |
| Create Input | ❌ | Not implemented |
| Remove Input | ❌ | Not implemented |
| Set Input Name | ❌ | Not implemented |

### Audio Control

| Feature | Status | Implementation |
|---------|--------|----------------|
| Get Input Mute | ✅ | `GetInputMute()` |
| Set Input Mute | ❌ | Not exposed (uses toggle) |
| Toggle Input Mute | ✅ | `AudioMixerDynamicFolder`, `SceneAudioSourcesDynamicFolder` |
| Input Mute Changed Event | ✅ | `InputMuteStateChanged` event |
| Get Input Volume | ✅ | `GetInputVolume()` - displays on buttons |
| Set Input Volume | ✅ | `SetInputVolume()` - wheel tool + API |
| Input Volume Changed Event | ✅ | `InputVolumeChanged` event |
| Get Input Audio Balance | ❌ | Not implemented |
| Set Input Audio Balance | ❌ | Not implemented |
| Get Input Audio Sync Offset | ❌ | Not implemented |
| Set Input Audio Sync Offset | ❌ | Not implemented |
| Get Input Audio Monitor Type | ✅ | `GetInputAudioMonitorType()` |
| Set Input Audio Monitor Type | ✅ | `CycleInputAudioMonitorType()` |
| Get Input Audio Tracks | ❌ | Not implemented |
| Set Input Audio Tracks | ❌ | Not implemented |

### Audio Monitoring

| Feature | Status | Implementation |
|---------|--------|----------------|
| Monitor Type: None | ✅ | `CycleInputAudioMonitorType()` |
| Monitor Type: Monitor Only | ✅ | Cycles through states |
| Monitor Type: Monitor & Output | ✅ | Cycles through states |

---

## 7. Filters

| Feature | Status | Implementation |
|---------|--------|----------------|
| Get Source Filter List | ❌ | Not implemented |
| Get Source Filter | ❌ | Not implemented |
| Get Source Filter Default Settings | ❌ | Not implemented |
| Create Source Filter | ❌ | Not implemented |
| Remove Source Filter | ❌ | Not implemented |
| Set Source Filter Name | ❌ | Not implemented |
| Get Source Filter Enabled | ❌ | Not implemented |
| Set Source Filter Enabled | ❌ | Not implemented |
| Get Source Filter Settings | ❌ | Not implemented |
| Set Source Filter Settings | ❌ | Not implemented |
| Get Source Filter Index | ❌ | Not implemented |
| Set Source Filter Index | ❌ | Not implemented |

---

## 8. Transitions

| Feature | Status | Implementation |
|---------|--------|----------------|
| Get Transition List | ❌ | Not implemented |
| Get Current Scene Transition | ❌ | Not implemented |
| Set Current Scene Transition | ❌ | Not implemented |
| Get Transition Duration | ❌ | Not implemented |
| Set Transition Duration | ❌ | Not implemented |
| Get Transition Settings | ❌ | Not implemented |
| Set Transition Settings | ❌ | Not implemented |
| Trigger Studio Mode Transition | ✅ | `StudioModeTransitionCommand` |
| Set T-Bar Position | ❌ | Not implemented |

---

## 9. Studio Mode

| Feature | Status | Implementation |
|---------|--------|----------------|
| Get Studio Mode Enabled | ✅ | `GetStudioModeEnabled()` |
| Set Studio Mode Enabled | ✅ | `StudioModeToggleCommand` |
| Studio Mode State Changed Event | ✅ | `StudioModeStateChanged` event |
| Trigger Studio Mode Transition | ✅ | `StudioModeTransitionCommand` |
| Get Current Preview Scene | ❌ | Not exposed to plugin |
| Set Current Preview Scene | ✅ | Used internally when studio mode enabled |

---

## 10. Media Inputs

| Feature | Status | Implementation |
|---------|--------|----------------|
| Get Media Input Status | ❌ | Not implemented |
| Set Media Input Cursor | ❌ | Not implemented |
| Offset Media Input Cursor | ❌ | Not implemented |
| Trigger Media Input Action | ❌ | Not implemented |
| - Play | ❌ | Not implemented |
| - Pause | ❌ | Not implemented |
| - Stop | ❌ | Not implemented |
| - Restart | ❌ | Not implemented |
| - Next | ❌ | Not implemented |
| - Previous | ❌ | Not implemented |

---

## 11. Screenshots

| Feature | Status | Implementation |
|---------|--------|----------------|
| Save Source Screenshot | ✅ | `ScreenshotCommand` |
| Get Screenshot | ❌ | Not implemented (returns base64) |

---

## 12. Hotkeys

| Feature | Status | Implementation |
|---------|--------|----------------|
| Get Hotkey List | ❌ | Not implemented |
| Trigger Hotkey by Name | ❌ | Not implemented |
| Trigger Hotkey by Key Sequence | ❌ | Not implemented |

---

## 13. UI

| Feature | Status | Implementation |
|---------|--------|----------------|
| Get Studio Mode Enabled | ✅ | Implemented |
| Open Source Properties Dialog | ❌ | Not implemented |
| Open Source Filters Dialog | ❌ | Not implemented |
| Open Source Interact Dialog | ❌ | Not implemented |
| Get Monitor List | ❌ | Not implemented |
| Open Video Mix Projector | ❌ | Not implemented |
| Open Source Projector | ❌ | Not implemented |

---

## 14. Statistics

| Feature | Status | Implementation |
|---------|--------|----------------|
| Get Stats | ❌ | Not implemented |
| - CPU Usage | ❌ | Not implemented |
| - Memory Usage | ❌ | Not implemented |
| - FPS | ❌ | Not implemented |
| - Render Lag | ❌ | Not implemented |
| - Dropped Frames | ❌ | Not implemented |
| - Output Skipped Frames | ❌ | Not implemented |

---

## 15. Events (Subscribed)

### Currently Subscribed Events

| Event | Status | Handler |
|-------|--------|---------|
| Connected | ✅ | `OnConnected()` |
| Disconnected | ✅ | `OnDisconnected()` |
| StreamStateChanged | ✅ | `OnStreamStateChanged()` |
| RecordStateChanged | ✅ | `OnRecordStateChanged()` |
| VirtualcamStateChanged | ✅ | `OnVirtualCameraStateChanged()` |
| ReplayBufferStateChanged | ✅ | `OnReplayBufferStateChanged()` |
| CurrentProfileChanged | ✅ | `OnCurrentProfileChanged()` |
| CurrentSceneCollectionChanged | ✅ | `OnCurrentSceneCollectionChanged()` |
| SceneListChanged | ✅ | `OnSceneListChanged()` |
| CurrentProgramSceneChanged | ✅ | `OnCurrentSceneChanged()` |
| InputMuteStateChanged | ✅ | `OnInputMuteStateChanged()` |
| InputVolumeChanged | ✅ | `OnInputVolumeChanged()` |
| StudioModeStateChanged | ✅ | `OnStudioModeStateChanged()` |

### Available But Not Subscribed

| Event | Status | Potential Use |
|-------|--------|---------------|
| CurrentPreviewSceneChanged | ❌ | Studio mode preview tracking |
| SceneItemCreated | ❌ | Dynamic source list updates |
| SceneItemRemoved | ❌ | Dynamic source list updates |
| SceneItemEnableStateChanged | ❌ | Source visibility sync |
| SceneItemTransformChanged | ❌ | Source position/scale tracking |
| InputCreated | ❌ | Dynamic input list updates |
| InputRemoved | ❌ | Dynamic input list updates |
| InputNameChanged | ❌ | Input list sync |
| InputAudioBalanceChanged | ❌ | Audio balance display |
| InputAudioSyncOffsetChanged | ❌ | Audio sync display |
| InputAudioTracksChanged | ❌ | Track assignment display |
| InputAudioMonitorTypeChanged | ❌ | Monitor state display |
| SourceFilterCreated | ❌ | Filter list updates |
| SourceFilterRemoved | ❌ | Filter list updates |
| SourceFilterEnableStateChanged | ❌ | Filter state display |
| SourceFilterListReindexed | ❌ | Filter order updates |
| MediaInputPlaybackStarted | ❌ | Media playback state |
| MediaInputPlaybackEnded | ❌ | Media playback state |
| MediaInputActionTriggered | ❌ | Media control feedback |
| CurrentSceneTransitionChanged | ❌ | Transition display |
| CurrentSceneTransitionDurationChanged | ❌ | Transition duration display |
| SceneTransitionStarted | ❌ | Transition progress |
| SceneTransitionEnded | ❌ | Transition completion |
| SceneTransitionVideoEnded | ❌ | Transition video completion |

---

## Implementation Summary

### ✅ Fully Implemented Categories

1. **Connection Management** - Auto-connect, reconnect, authentication
2. **Streaming Controls** - Start, stop, toggle, status
3. **Recording Controls** - Start, stop, toggle, pause/resume, status
4. **Virtual Camera** - Start, stop, toggle, status
5. **Replay Buffer** - Start, stop, toggle, save, status
6. **Scene Management** - List, switch, current scene tracking
7. **Scene Collections** - List, switch, current collection tracking
8. **Profiles** - List, switch, current profile tracking
9. **Source Visibility** - Toggle visibility in scenes
10. **Audio Mute** - Mute/unmute, status display
11. **Audio Volume Display** - Show volume percentage
12. **Studio Mode** - Toggle, transition, status
13. **Screenshots** - Capture to file

### 🟡 Partially Implemented Categories

1. **Inputs** - Discovery ✅, Settings ❌, Creation ❌

### ❌ Not Implemented Categories

1. **Media Source Controls** - Play, pause, stop, seek
2. **Filters** - List, enable/disable, settings
3. **Transitions** - List, select, duration, settings
4. **Audio Advanced** - Balance, sync offset, monitoring, track assignment
5. **Scene Item Transforms** - Position, scale, rotation, crop
6. **Hotkeys** - Trigger hotkeys
7. **Statistics** - FPS, CPU, dropped frames
8. **UI Dialogs** - Open properties, filters, projectors
9. **Scene/Source Creation** - Create/remove scenes, sources, filters

---

## Priority Recommendations

### High Priority (User-Requested Features)

1. **Volume Adjustment Controls** - ✅ Multiple approaches (WheelTool, AudioVolumeDynamicFolder, SelectedSourceVolumeAdjustment)
2. **Audio Monitoring Toggle** - ✅ CycleInputAudioMonitorType implemented
3. **Filter Enable/Disable** - Toggle audio/video filters

### Medium Priority (Professional Features)

1. **Media Source Controls** - Play/pause/stop pre-recorded content
2. **Audio Level Meters** - Real-time VU meters
3. **Audio Sync Offset** - Fix audio/video sync
4. **Audio Track Assignment** - Multi-track recording
5. **Transition Selection** - Choose transition and duration
6. **Hotkey Triggers** - Execute OBS hotkeys from hardware

### Low Priority (Advanced Features)

1. **Statistics Display** - FPS, CPU, dropped frames
2. **Scene Item Transforms** - Position, scale, rotation
3. **Audio Balance** - Stereo balance control
4. **Scene/Source Creation** - Create new scenes/sources
5. **Filter Settings** - Adjust filter parameters

---

## Technical Notes

### API Wrapper Architecture

- **IOBSWebsocket** - Interface for testability
- **OBSWebsocketAdapter** - Wraps obs-websocket-dotnet library
- **OBSActionExecutor** - Business logic layer
- **OBSWebSocketManager** - Connection and event management

### Event Handling Pattern

1. OBS fires event → obs-websocket-dotnet
2. Event → OBSWebSocketManager handler
3. Handler → OBSStudioForLogiPlugin callback
4. Plugin → Command.OnStateChanged()
5. Command → UI update (CommandImageChanged)

### Adding New Features Checklist

1. Add method to `IOBSWebsocket` interface
2. Implement in `OBSWebsocketAdapter`
3. Add business logic to `OBSActionExecutor`
4. Wire up in `OBSStudioForLogiPlugin`
5. Create command class in `Actions/`
6. Add tests for all layers
7. Update documentation

---

## Version Information

- **OBS WebSocket Protocol**: 5.x
- **obs-websocket-dotnet**: 5.0.1
- **OBS Studio**: 28.0+ required
- **Plugin Version**: 1.0.1
