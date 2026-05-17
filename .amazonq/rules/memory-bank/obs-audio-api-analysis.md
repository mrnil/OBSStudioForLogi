# OBS WebSocket Audio API Analysis

## Current Implementation

### What We Have Now

#### Audio Input Discovery
- **GetInputList()** - Returns all audio inputs filtered by kind
  - Filters for audio input types: wasapi, coreaudio, pulse, alsa, jack, ffmpeg, dshow, window_capture, audio_capture
  - Returns input names only
  
- **GetInputKind(inputName)** - Returns the kind/type of an input
  - Used to identify audio input types

- **GetAudioSourcesInScene(sceneName)** - Returns audio inputs present in a specific scene
  - Filters scene items to only include audio inputs
  - Used by Scene Audio folder

- **GetScenesForInput(inputName)** - Returns which scenes contain a specific input
  - Used for debugging/logging in Audio Mixer

#### Audio Control (Currently Implemented)
- **GetInputMute(inputName)** - Returns mute state (Boolean)
- **ToggleInputMute(inputName)** - Toggles mute on/off
- **Visual Feedback**: Red icon = muted, Green icon = unmuted

### Current UI Components
1. **Audio Mixer Dynamic Folder** - Shows all audio inputs with mute/unmute
2. **Scene Audio Dynamic Folder** - Shows audio inputs in current scene with mute/unmute

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
1. ✅ **Mute/Unmute** - Already implemented
2. 🆕 **Volume Control** - Add volume fader/slider
3. 🆕 **Volume Display** - Show current volume percentage

### Enhanced Features (Should Have)
4. 🆕 **Audio Monitoring Toggle** - Monitor in headphones
5. 🆕 **Audio Level Meters** - Real-time VU meters (if device supports dynamic displays)

### Advanced Features (Nice to Have)
6. 🆕 **Audio Sync Offset** - Fix audio/video sync
7. 🆕 **Track Assignment** - Multi-track recording control
8. 🆕 **Filter Toggle** - Enable/disable audio filters

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

### Currently Used
- ✅ **InputMuteStateChanged** - Already implemented

### Recommended Additions
- 🆕 **InputVolumeChanged** - Update volume display when changed in OBS
- 🆕 **InputAudioMonitorTypeChanged** - Update monitoring state display
- 🆕 **InputVolumeMeters** - Real-time audio level data
- 🆕 **SourceFilterEnableStateChanged** - Update filter state display

## API Methods to Add to IOBSWebsocket

```csharp
// Volume Control
Single GetInputVolume(String inputName); // Returns volumeMul (0.0-1.0)
void SetInputVolume(String inputName, Single volumeMul);

// Audio Monitoring
String GetInputAudioMonitorType(String inputName);
void SetInputAudioMonitorType(String inputName, String monitorType);

// Audio Sync
Int64 GetInputAudioSyncOffset(String inputName);
void SetInputAudioSyncOffset(String inputName, Int64 offset);

// Audio Balance
Single GetInputAudioBalance(String inputName);
void SetInputAudioBalance(String inputName, Single balance);

// Audio Tracks
Dictionary<String, Boolean> GetInputAudioTracks(String inputName);
void SetInputAudioTracks(String inputName, Dictionary<String, Boolean> tracks);

// Audio Filters
String[] GetSourceFilterList(String sourceName);
Boolean GetSourceFilterEnabled(String sourceName, String filterName);
void SetSourceFilterEnabled(String sourceName, String filterName, Boolean enabled);
```

## Conclusion

The OBS WebSocket API provides comprehensive audio control capabilities. The most valuable features for streamers are:

1. **Volume Control** - Essential for mixing audio levels
2. **Audio Monitoring** - Important for hearing sources in headphones
3. **Audio Level Meters** - Visual feedback for audio levels
4. **Mute Control** - Already implemented ✅

These features would significantly enhance the plugin's audio capabilities and provide professional-level audio mixing directly from the Loupedeck device.
