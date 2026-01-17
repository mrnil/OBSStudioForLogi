# OBS Studio Plugin for Loupedeck v0.8.0

Control OBS Studio directly from your Loupedeck/Logitech hardware device with dedicated buttons and real-time status displays.

## 🎉 What's New in v0.8.0

### Core Features
- ✅ **Streaming Controls** - Start, stop, toggle streaming with visual indicators
- ✅ **Recording Controls** - Start, stop, toggle, pause/resume recording
- ✅ **Virtual Camera** - Control virtual camera output for video conferencing
- ✅ **Scene Management** - Dynamic folder with all scenes, instant switching
- ✅ **Source Visibility** - Toggle visibility of sources in current scene
- ✅ **Profile Management** - Multi-state buttons and dynamic folder for profiles
- ✅ **Scene Collections** - Switch between scene collections
- ✅ **Connection Status** - Real-time display showing connection state
- ✅ **Manual Reconnect** - Button to retry connection on demand
- ✅ **Screenshot Capture** - Take screenshots with automatic path detection

### Technical Highlights
- 🔄 **Resilient Connection** - Exponential backoff (1s-30s) with jitter (0.85-1.15x)
- 🧵 **Thread-Safe Disposal** - Comprehensive cleanup with lock protection
- 🔍 **Auto-Discovery** - Reads OBS WebSocket settings automatically
- ✅ **80 Unit Tests** - Full coverage of core functionality
- 📊 **Anti-Aliased Text** - BitmapBuilder rendering for crisp displays

## 📋 Requirements

- **OBS Studio** 28.0+ with obs-websocket 5.0+
- **Logi Plugin Service** (installed)
- **.NET 8.0 Runtime** (for development)

## 🚀 Installation

### Quick Install
1. Download `OBSStudioForLogiPlugin-v0.8.0.lplug4` from releases
2. Run: `LogiPluginTool.exe install OBSStudioForLogiPlugin-v0.8.0.lplug4`
3. Restart Logi Plugin Service
4. Launch OBS Studio - plugin connects automatically

See [INSTALL.md](INSTALL.md) for manual installation instructions.

## 🎮 Available Controls

### OBS (Group 1)
- Screenshot, Reconnect, Virtual Camera (Toggle/Start/Stop), Connection Status

### Streaming (Group 2)
- Streaming Toggle, Start, Stop

### Recording (Group 3)
- Recording Toggle, Start, Stop, Pause/Resume

### Profiles (Group 4)
- Profile Select (multi-state), Profiles Folder, Current Profile Display

### Scenes (Group 5)
- Scene Collection Select, Scenes Folder, Sources Folder, Current Scene/Collection Display

## 🔮 Coming in v1.0

- Audio mixer controls (mute/unmute)
- Replay buffer controls
- Studio mode toggle
- Filter enable/disable

## 🐛 Known Issues

None reported for v0.8.0

## 📝 Full Changelog

See [CHANGELOG.md](CHANGELOG.md) for complete version history.

## 🤝 Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development guidelines.

## 📄 License

See [LICENSE](LICENSE) for details.

---

**Note:** This is a beta release (v0.8.0). Core streaming features are complete and tested, but some professional features are planned for v1.0.
