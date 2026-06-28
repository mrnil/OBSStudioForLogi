# OBS WebSocket Audio API Analysis

## Current Implementation (Updated v1.0.1)

### What We Have Now

#### Audio Input Discovery

- **GetInputList()** - ✅ Returns all audio inputs filtered by kind
  - Filters for audio input types: wasapi, coreaudio, pulse, alsa, jack, ffmpeg, dshow, window_capture, audio_capture
  - Returns input names only
  - Implemented in `OBSWebsocketAdapter.GetInputList()`
  
- **GetInputKind(inputName)** - ✅ Returns the kind/type of an input
  - Used to identify audio input types
  - Implemented in `OBSWebsocketAdapter.GetInputKind()`

- **GetAudioSourcesInScene(sceneName)** - ✅ Returns audio inputs present in a specific scene
  - Filters scene items to only include audio inputs
  - Used by Scene Audio folder
  - Implemented in `OBSWebsocketAdapter.GetAudioSourcesInScene()`

- **GetScenesForInput(inputName)** - ✅ Returns which scenes contain a specific input
  - Used for debugging/logging in Audio Mixer
  - Implemented in `OBSWebsocketAdapter.GetScenesForInput()`

#### Audio Control (Currently Implemented)

- **GetInputMute(inputName)** - ✅ Returns mute state (Boolean)
  - Implemented in `IOBSWebsocket` interface
  - Used by `AudioMixerDynamicFolder` and `SceneAudioSourcesDynamicFolder`
  
- **ToggleInputMute(inputName)** - ✅ Toggles mute on/off
  - Implemented in `IOBSWebsocket` interface
  - Used by audio folder buttons
  
- **GetInputVolume(inputName)** - ✅ Returns volume level (0.0-1.0)
  - Implemented in `IOBSWebsocket` interface
  - Displays volume percentage (0-100%) on audio buttons
  
- **SetInputVolume(inputName, volumeMul)** - ✅ Sets volume level
  - API implemented in `IOBSWebsocket` interface
  - ⚠️ No UI controls yet (faders/buttons needed)
  
- **Visual Feedback**:
  - Red text = muted
  - Green text = unmuted
  - Volume percentage displayed (0-100%)

### Current UI Components

1. **Audio Mixer Dynamic Folder** - ✅ Shows all audio inputs with mute/unmute and volume display
2. **Scene Audio Dynamic Folder** - ✅ Shows audio inputs in current scene with mute/unmute and volume display

### Events Subscribed

- **InputMuteStateChanged** - ✅ Updates button colors when mute state changes
- **InputVolumeChanged** - ✅ Updates volume percentage display in real-time

## OBS WebSocket 5.x Audio API Capabilities

Based on the obs-websocket-dotnet library and OBS WebSocket 5.x protocol, here are the available audio-related APIs:

### Input Audio APIs

#### Volume Control

- **GetInputVolume(inputName)**
  - Returns: volumeMul (float 0.0-20.0), volumeDb (float -100.0 to 26.0)
  - volumeMul: Linear volume multiplier (1.0 = 100%)
  - volumeDb: Decibel volume level
  
- **SetInputVolume(inputName, volumeMul, volumeDb)**
  - Set volume using either multiplier or dB
  - Can specify one or both parameters

#### Mute Control (Already Implemented)

- **GetInputMute(inputName)** - Returns Boolean
- **SetInputMute(inputName, muted)** - Set mute state directly
- **ToggleInputMute(inputName)** - Toggle mute state

#### Audio Monitoring

- **GetInputAudioMonitorType(inputName)**
  - Returns: "OBS_MONITORING_TYPE_NONE", "OBS_MONITORING_TYPE_MONITOR_ONLY", "OBS_MONITORING_TYPE_MONITOR_AND_OUTPUT"
  - Controls whether you hear the audio source in your headphones
  
- **SetInputAudioMonitorType(inputName, monitorType)**
  - Set monitoring type

#### Audio Sync/Offset

- **GetInputAudioSyncOffset(inputName)**
  - Returns: offset in milliseconds (Int64)
  - Used to sync audio with video
  
- **SetInputAudioSyncOffset(inputName, offset)**
  - Set audio sync offset in milliseconds

#### Audio Balance (Stereo)

- **GetInputAudioBalance(inputName)**
  - Returns: balance (float 0.0-1.0, where 0.5 = center)
  
- **SetInputAudioBalance(inputName, balance)**
  - Set stereo balance

#### Audio Tracks (Recording/Streaming)

- **GetInputAudioTracks(inputName)**
  - Returns: Object with track1-track6 Boolean values
  - Controls which recording/streaming tracks the audio goes to
  
- **SetInputAudioTracks(inputName, tracks)**
  - Set which tracks the audio is sent to

### Audio Mixer APIs

#### Global Audio

- **GetInputList(inputKind)** - Already implemented
  - Can filter by specific input kind
  
#### Audio Meters (Real-time)

- **Event: InputVolumeMeters**
  - Real-time audio level data for all inputs
  - Returns array of inputs with volume levels
  - Can be used for VU meters/level indicators

### Filter APIs (Audio Effects)

#### Audio Filters

- **GetSourceFilterList(sourceName)**
  - Returns list of filters on an audio source
  - Includes: Compressor, Expander, Gain, Limiter, Noise Gate, Noise Suppression, VST plugins
  
- **GetSourceFilter(sourceName, filterName)**
  - Get specific filter settings
  
- **SetSourceFilterEnabled(sourceName, filterName, enabled)**
  - Enable/disable audio filters
  
- **SetSourceFilterSettings(sourceName, filterName, settings)**
  - Modify filter parameters

## Potential Plugin Features

### High Priority (Commonly Used)

#### 1. Volume Control

- **Volume Slider/Fader** for each audio input
  - Display current volume level (0-100%)
  - Adjust volume with encoder/slider
  - Visual feedback showing volume level
  - Could use encoder knobs on Loupedeck devices

#### 2. Audio Monitoring Toggle

- **Monitor Button** for each input
  - Toggle between: None / Monitor Only / Monitor & Output
  - Visual indicator showing monitoring state
  - Useful for hearing audio sources in headphones

#### 3. Audio Level Meters

- **VU Meter Display** for each input
  - Real-time audio level visualization
  - Color-coded: Green (normal), Yellow (loud), Red (clipping)
  - Subscribe to InputVolumeMeters event
  - Update display in real-time

### Medium Priority

#### 4. Audio Sync Offset

- **Sync Adjustment** for each input
  - Display current offset in milliseconds
  - Adjust offset with +/- buttons or encoder
  - Useful for fixing audio/video sync issues

#### 5. Audio Track Assignment

- **Track Toggle Buttons** for each input
  - 6 buttons per input (Track 1-6)
  - Visual indicator showing which tracks are active
  - Useful for multi-track recording

#### 6. Stereo Balance

- **Balance Control** for stereo inputs
  - Adjust left/right balance
  - Visual indicator showing balance position
  - Center button to reset to 50/50

### Low Priority (Advanced)

#### 7. Audio Filter Controls

- **Filter Enable/Disable** buttons
  - List filters for each audio input
  - Toggle filters on/off
  - Examples: Noise Gate, Compressor, Noise Suppression
  
#### 8. Quick Presets

- **Volume Presets** for common scenarios
  - "Mute All" button
  - "Reset All Volumes" button
  - "Game Audio Down" / "Mic Audio Up" macros

## Recommended Phase 2 Implementation

### Core Features (Must Have)

1. ✅ **Mute/Unmute** - Fully implemented with visual feedback
2. ✅ **Volume Display** - Shows current volume percentage (0-100%)
3. 🟡 **Volume Control** - API exists, needs UI (faders/encoders or +/- buttons)

### Enhanced Features (Should Have)

1. ❌ **Audio Monitoring Toggle** - Monitor in headphones (None/Monitor Only/Monitor & Output)
2. ❌ **Audio Level Meters** - Real-time VU meters (if device supports dynamic displays)

### Advanced Features (Nice to Have)

1. ❌ **Audio Sync Offset** - Fix audio/video sync
2. ❌ **Track Assignment** - Multi-track recording control
3. ❌ **Filter Toggle** - Enable/disable audio filters

## Technical Considerations

### Volume Control Implementation

- Use encoder knobs if available on device
- Fallback to +/- buttons for devices without encoders
- Store volume as multiplier (0.0-1.0 for 0-100%)
- Convert to dB for display if needed

### Audio Meters Implementation

- Subscribe to InputVolumeMeters event
- Update display at reasonable rate (10-30 Hz)
- Use color coding for visual feedback
- May require custom image rendering for meter bars

### UI Layout Suggestions

- **Audio Mixer Folder**: Show all inputs with volume + mute
- **Scene Audio Folder**: Show scene inputs with volume + mute
- **Audio Input Detail View**: Full controls for single input (volume, monitoring, sync, tracks, filters)

## Events to Subscribe To

### Currently Subscribed

- ✅ **InputMuteStateChanged** - Updates UI when mute state changes in OBS
- ✅ **InputVolumeChanged** - Updates volume display when changed in OBS

### Recommended Additions

- ❌ **InputAudioMonitorTypeChanged** - Update monitoring state display
- ❌ **InputVolumeMeters** - Real-time audio level data for VU meters
- ❌ **SourceFilterEnableStateChanged** - Update filter state display
- ❌ **InputAudioBalanceChanged** - Update balance display
- ❌ **InputAudioSyncOffsetChanged** - Update sync offset display
- ❌ **InputAudioTracksChanged** - Update track assignment display

## API Methods to Add to IOBSWebsocket

```csharp
// Volume Control - PARTIALLY IMPLEMENTED
Single GetInputVolume(String inputName); // ✅ Already implemented
void SetInputVolume(String inputName, Single volumeMul); // ✅ Already implemented

// Audio Monitoring - NOT IMPLEMENTED
String GetInputAudioMonitorType(String inputName); // ❌ Needs implementation
void SetInputAudioMonitorType(String inputName, String monitorType); // ❌ Needs implementation

// Audio Sync - NOT IMPLEMENTED
Int64 GetInputAudioSyncOffset(String inputName); // ❌ Needs implementation
void SetInputAudioSyncOffset(String inputName, Int64 offset); // ❌ Needs implementation

// Audio Balance - NOT IMPLEMENTED
Single GetInputAudioBalance(String inputName); // ❌ Needs implementation
void SetInputAudioBalance(String inputName, Single balance); // ❌ Needs implementation

// Audio Tracks - NOT IMPLEMENTED
Dictionary<String, Boolean> GetInputAudioTracks(String inputName); // ❌ Needs implementation
void SetInputAudioTracks(String inputName, Dictionary<String, Boolean> tracks); // ❌ Needs implementation

// Audio Filters - NOT IMPLEMENTED
String[] GetSourceFilterList(String sourceName); // ❌ Needs implementation
Boolean GetSourceFilterEnabled(String sourceName, String filterName); // ❌ Needs implementation
void SetSourceFilterEnabled(String sourceName, String filterName, Boolean enabled); // ❌ Needs implementation
```

## Implementation Progress Summary

### ✅ Completed (v1.2.0)

- Audio input discovery and filtering
- Mute/unmute controls with visual feedback (red/green text)
- Volume display in dB format (e.g., "+6.0 dB", "0.0 dB", "-∞ dB") on all audio buttons
- Full OBS volume range support (0.0-20.0 multiplier, ~+26 dB max)
- Real-time updates via events (mute and volume changes)
- Audio Mixer folder (all audio inputs)
- Scene Audio folder (audio inputs in current scene)
- Audio inputs not in any scene included in Scene Audio folder
- Volume adjustment via encoder/wheel tool (`AudioVolumeWheelTool`)
- Audio selection state for dial control (`AudioSelectionState`)
- Double-tap to select/deselect audio source for wheel control
- Audio monitoring type cycling (None → Monitor Only → Monitor & Output)
- Audio Select folder (dedicated selection-only folder)
- Audio Volume folder (MX big wheel-compatible adjustment tiles)
- Standalone Selected Source Volume adjustment (PluginDynamicAdjustment for any wheel/dial)
- User-defined audio mute toggle (ActionEditorCommand)
- User-defined audio monitoring cycle (ActionEditorCommand)
- User-defined audio source selection (ActionEditorCommand)

### 🟡 Partially Completed

- Volume adjustment — multiple approaches available:
  - `AudioVolumeWheelTool` ✅ CT encoder-based volume adjustment
  - `AudioVolumeDynamicFolder` ✅ MX big wheel via adjustment tiles
  - `SelectedSourceVolumeAdjustment` ✅ standalone adjustment for any wheel/dial
  - `AudioSelectionState` ✅ tracks selected input for dial
  - `VolumeConverter` ✅ dB display format with full OBS range (0-20x)
  - Needs: +/- button alternatives for devices without encoders/wheels

### ❌ Not Yet Implemented

- Audio level meters (VU meters) — **deferred** until obs-websocket-dotnet supports high-volume event subscription (see vu-meters-learnings.md)
- Audio monitoring controls
- Audio sync offset controls
- Audio track assignment
- Stereo balance controls
- Audio filter controls
- Audio quick presets

### Next Steps

1. **Medium Priority**: Add audio level meters (deferred — requires obs-websocket-dotnet library modifications, see vu-meters-learnings.md)
2. **Medium Priority**: Implement filter enable/disable controls
3. **Low Priority**: Add +/- button volume alternatives for devices without wheels

## Conclusion

The OBS WebSocket API provides comprehensive audio control capabilities. The most valuable features for streamers are:

1. **Volume Control** - Essential for mixing audio levels
2. **Audio Monitoring** - Important for hearing sources in headphones
3. **Audio Level Meters** - Visual feedback for audio levels
4. **Mute Control** - Already implemented ✅

These features would significantly enhance the plugin's audio capabilities and provide professional-level audio mixing directly from the Loupedeck device.
