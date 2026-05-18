# Release Notes - OBS Studio Plugin for Loupedeck/Logitech Devices

## Version 0.9.0 - Audio Volume Display & Cross-Platform Improvements

### 🎉 New Features

#### Audio Volume Display
- **Real-time Volume Monitoring**: Audio mixer and scene audio buttons now display current volume levels (0-100%)
- **Visual Feedback**: 
  - Green text when unmuted
  - Red text when muted
  - Input name and volume percentage clearly displayed
- **Live Updates**: Volume display updates automatically when changed in OBS or via hardware controls

#### Enhanced Audio Controls
- **Mute/Unmute Toggle**: Press any audio button to toggle mute state
- **Scene-Specific Audio**: Scene Audio folder shows only audio inputs present in the current scene
- **Clean Display**: Display name overlay hidden for better readability

### 🔧 Improvements

#### Cross-Platform Compatibility
- **Full macOS Support**: Eliminated all Windows-only dependencies
- **Zero Platform Warnings**: All 78 CA1416 platform-specific warnings resolved
- **Native SDK Usage**: Uses Loupedeck SDK's cross-platform BitmapBuilder/BitmapImage APIs

#### Performance Optimizations
- **Efficient Image Caching**: 16 commands now use ActionImageStore pattern for reduced rendering overhead
- **Real-time State Updates**: Audio buttons generate fresh images to reflect current state immediately
- **Smart Update Strategy**: Uses ButtonActionNamesChanged() for reliable folder button updates

#### User Experience
- **Better Text Spacing**: Extra line break between input name and volume percentage
- **Consistent Font Sizing**: 18pt (large buttons) / 16pt (small buttons)
- **Cleaner Interface**: Removed display name overlays from audio buttons

### 🐛 Bug Fixes

- Fixed audio buttons not updating when mute state changes
- Fixed audio buttons not updating when volume changes in OBS
- Fixed incorrect resource loading (EmbeddedResources → PluginResources)
- Fixed BitmapBuilder SVG compatibility issues
- Fixed ActionImageStore caching preventing real-time updates
- Fixed BitmapColor property access errors in TextWithBackgroundImageFactory

### 🗑️ Removed

- Removed Windows-only System.Drawing dependencies (BitmapHelper.cs)
- Removed unused image factories (IconWithTextImageFactory, IconWithTextImageData)
- Removed icon rendering from audio buttons (SVG compatibility issues)

### 📋 Technical Details

#### API Additions
- `GetInputVolume(inputName)` - Returns volume level (0.0-1.0)
- `SetInputVolume(inputName, volumeMul)` - Sets volume level
- `OnInputVolumeChanged(inputName)` - Event handler for volume changes

#### Architecture Changes
- Audio buttons no longer use ActionImageStore caching (real-time state requires fresh rendering)
- ButtonActionNamesChanged() used instead of CommandImageChanged() for dynamic folder updates
- Direct ButtonTextRenderer calls for audio buttons to ensure current state display

### 🔮 Future Enhancements

Planned for future releases:
- Volume adjustment controls (faders/encoders)
- Audio monitoring toggle (hear sources in headphones)
- Audio sync offset adjustment
- Audio track assignment controls
- Audio filter enable/disable
- Replay buffer controls
- Studio mode toggle

### 📦 Installation

1. Download the latest release
2. Build the project:
   ```bash
   dotnet build src/OBSStudioForLogiPlugin.csproj -c Release
   ```
3. Plugin automatically installs to Logi Plugin Service
4. Launch OBS Studio - plugin connects automatically

### 📖 Documentation

- Full feature documentation: [README.md](README.md)
- Development guidelines: [.amazonq/rules/memory-bank/guidelines.md](.amazonq/rules/memory-bank/guidelines.md)
- Architecture overview: [.amazonq/rules/memory-bank/structure.md](.amazonq/rules/memory-bank/structure.md)

### 🙏 Acknowledgments

This release includes significant improvements to cross-platform compatibility and real-time audio monitoring capabilities, making the plugin more reliable and feature-rich for content creators on both Windows and macOS.

---

**Requirements:**
- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 8.0 SDK (for development)

**Compatibility:**
- Windows 10/11
- macOS 10.15+
- Loupedeck Live, Loupedeck Live S, Loupedeck CT, Logitech MX Creative Console
