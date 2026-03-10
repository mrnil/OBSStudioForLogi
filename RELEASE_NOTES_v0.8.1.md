# OBS Studio Plugin for Loupedeck v0.8.1

Patch release fixing text rendering quality issues.

## 🐛 Bug Fixes

### Text Rendering Quality
- **Fixed jaggy text** on all display buttons and dynamic folders
- Eliminated double text rendering (native overlay + BitmapBuilder)
- All text now renders with anti-aliasing for crisp, clear appearance

### Technical Changes
- Created `ButtonTextRenderer` helper class for consistent text rendering
- Display commands return `null` from `GetCommandDisplayName()` to prevent native text overlay
- Dynamic folders (Scenes, Sources, Profiles) use only BitmapBuilder rendering

## 📦 Installation

### Quick Install
1. Download `OBSStudioForLogiPlugin-v0.8.1.lplug4` from releases
2. Run: `LogiPluginTool.exe install OBSStudioForLogiPlugin-v0.8.1.lplug4`
3. Restart Logi Plugin Service
4. Launch OBS Studio - plugin connects automatically

See [INSTALL.md](INSTALL.md) for manual installation instructions.

## 📋 Requirements

- **OBS Studio** 28.0+ with obs-websocket 5.0+
- **Logi Plugin Service** (installed)
- **.NET 8.0 Runtime** (for development)

## 🔄 Upgrading from v0.8.0

Simply install v0.8.1 over v0.8.0 - no configuration changes needed. All features from v0.8.0 are included with improved text rendering quality.

## 📝 Full Changelog

See [CHANGELOG.md](CHANGELOG.md) for complete version history.

---

**Note:** This is a patch release addressing visual quality issues. All functionality from v0.8.0 remains unchanged.
