# Release Notes - v1.0.0

## 🎉 First Stable Release

This is the first stable release of OBS Studio Plugin for Loupedeck/Logitech Devices, featuring comprehensive OBS control directly from your hardware device.

## ✨ New Features in v1.0.0

### Studio Mode Support
- **Studio Mode Toggle**: Enable/disable OBS studio mode for preview/program workflow
- **Studio Mode Transition**: Transition preview scene to program with configured transition effect
- **Smart Scene Switching**: Automatically switches to preview when studio mode is enabled, program when disabled
- Provides professional broadcast workflow with preview before going live

### Replay Buffer Controls
- **Replay Buffer Toggle**: Start/stop replay buffer
- **Save Replay Buffer**: Save the current replay buffer to disk
- Perfect for capturing highlights during streams

### Reorganized Groups
- Virtual Camera moved to dedicated Group 5 for better organization
- Clear separation of functionality across 8 groups

## 📋 Complete Feature Set

### Group 1: OBS
- Screenshot capture to Pictures/Documents/Desktop folder
- Manual reconnect to OBS
- Studio mode toggle and transition
- Connection status display (green=connected, red=disconnected)

### Group 2: Streaming
- Toggle, start, and stop streaming
- Visual state indicators

### Group 3: Recording
- Toggle, start, and stop recording
- Pause/resume recording
- Visual state indicators

### Group 4: Replay Buffer
- Toggle replay buffer on/off
- Save replay buffer to disk

### Group 5: Virtual Camera
- Toggle, start, and stop virtual camera
- Visual state indicators

### Group 6: Profiles
- Switch between OBS profiles (multi-state buttons)
- Dynamic folder with all available profiles
- Current profile display

### Group 7: Scenes
- Switch between scene collections (multi-state buttons)
- Dynamic folder with all available scenes
- Toggle source visibility in current scene
- Current scene and scene collection displays
- Studio mode aware: switches to preview when enabled, program when disabled

### Group 8: Audio
- Audio Mixer folder with all audio inputs
- Scene Audio folder with audio sources in current scene
- Mute/unmute toggle with visual feedback (green=unmuted, red=muted)
- Real-time volume display (0-100%)

## 🔧 Technical Highlights

- **134 Unit Tests**: Comprehensive test coverage ensuring reliability
- **Automatic Connection**: Discovers OBS WebSocket settings from OBS configuration
- **Resilient Reconnection**: Exponential backoff (1s to 30s) with jitter (0.85-1.15x)
- **Event-Driven Updates**: Real-time state synchronization with OBS
- **Async Operations**: All OBS operations wrapped in Task.Run to prevent UI freezing
- **TDD Approach**: Test-driven development with interface-based design

## 📦 Installation

1. Download `OBSStudioForLogiPlugin-v1.0.0.lplug4` from the release assets
2. Double-click the `.lplug4` file to install via Logi Plugin Service
3. Launch OBS Studio - the plugin connects automatically

## 📋 Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- Windows 10/11 or macOS 10.15+

## 🐛 Bug Fixes

- Fixed replay buffer icon loading (icons were not embedded in project file)
- Fixed studio mode scene switching to use preview when enabled
- Improved icon update patterns for dynamic folders

## 🙏 Acknowledgments

Built with:
- [obs-websocket-dotnet](https://github.com/BarRaider/obs-websocket-dotnet) v5.0.1
- Loupedeck Plugin SDK
- .NET 8.0

## 📝 License

MIT License - See LICENSE file for details

## 🔗 Links

- [GitHub Repository](https://github.com/mrnil/OBSStudioForLogi)
- [Report Issues](https://github.com/mrnil/OBSStudioForLogi/issues)
- [Documentation](https://github.com/mrnil/OBSStudioForLogi/blob/main/README.md)
