# OBS Studio Plugin for Loupedeck v0.8.3

Patch release enabling macOS support.

## 🐛 Bug Fixes

### macOS Support
- **Fixed plugin load failure on macOS** with ArgumentNullException
- Enabled `pluginFolderMac` in package metadata
- Plugin now loads successfully on macOS devices (MacBook, iMac, Mac mini, etc.)

## 📦 Installation

### Quick Install
1. Download `OBSStudioForLogiPlugin-v0.8.3.lplug4` from releases
2. **Windows:** Run `LogiPluginTool.exe install OBSStudioForLogiPlugin-v0.8.3.lplug4`
3. **macOS:** Run `LogiPluginTool install OBSStudioForLogiPlugin-v0.8.3.lplug4`
4. Restart Logi Plugin Service
5. Launch OBS Studio - plugin connects automatically

See [INSTALL.md](INSTALL.md) for manual installation instructions.

## 📋 Requirements

- **OBS Studio** 28.0+ with obs-websocket 5.0+
- **Logi Plugin Service** (installed)
- **.NET 8.0 Runtime** (for development)

## 🔄 Upgrading from v0.8.2

Simply install v0.8.3 over v0.8.2 - no configuration changes needed. This release specifically fixes macOS compatibility.

## 🍎 macOS Users

This release fixes the critical load failure on macOS. Previous versions (v0.8.0 - v0.8.2) would not load on macOS due to missing `pluginFolderMac` configuration.

## 📝 Full Changelog

See [CHANGELOG.md](CHANGELOG.md) for complete version history.

---

**Note:** This is a patch release addressing macOS compatibility. All functionality from v0.8.2 remains unchanged.
