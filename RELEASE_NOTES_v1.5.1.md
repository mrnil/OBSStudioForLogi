# Release v1.5.1 - Dynamic Folder & Media Playback Fixes

## Bug Fixes

### Dynamic Folders Empty on Connect

- **Audio Select Folder**: Was showing empty despite audio sources being available in the Audio Mixer
- **Audio Volume Folder**: Was showing empty despite audio sources being available in the Audio Mixer
- **Media Controls Folder**: Was showing empty despite media sources existing in the scene

**Root Cause**: These folders were never explicitly notified on connection. They relied on registry-based notifications that weren't reaching them during initial state loading. The Audio Mixer folder worked because it was explicitly called.

**Fix**: Added explicit `OnConnected()` and `OnDisconnected()` calls for all three folders in `OBSWebSocketManager`, matching the existing pattern used for `AudioMixerDynamicFolder`.

### Media Play Action Not Working in OBS

- Tapping a media source button in the Media Controls folder would update the button state visually but not actually trigger playback in OBS

**Root Cause**: The play action used `OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PLAY` for all non-playing states. However, `PLAY` only works to resume from a `PAUSED` state. For `STOPPED`, `ENDED`, or `NONE` states, OBS requires `OBS_WEBSOCKET_MEDIA_INPUT_ACTION_RESTART`.

**Fix**: Updated single-tap logic:

- **Playing** → Pause
- **Paused** → Play (resume)
- **Stopped/Ended/None** → Restart (start from beginning)

## Installation

1. Download `OBSStudioForLogiPlugin-v1.5.1.lplug4`
2. Double-click to install, or use: `LogiPluginTool install OBSStudioForLogiPlugin-v1.5.1.lplug4`

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 8.0 Runtime
