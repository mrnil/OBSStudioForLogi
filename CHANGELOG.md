# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Audio volume display on audio mixer and scene audio buttons (0-100%)
- GetInputVolume() and SetInputVolume() API methods for volume control
- OnInputVolumeChanged() event handler for real-time volume updates
- Scene audio sources folder showing audio inputs in the current scene
- ActionImageStore pattern for efficient image caching (Phase 1 complete)
- TextImageFactory, StateImageFactory, SimpleIconImageFactory for cross-platform rendering
- TextImageData, StateImageData, SimpleIconImageData models
- Comprehensive debug logging for audio button state changes

### Changed
- Audio buttons now display input name and volume percentage with colored text
- Audio buttons use ButtonActionNamesChanged() for updates (matches ScenesDynamicFolder pattern)
- Audio buttons generate fresh images on every call for real-time state updates (no caching)
- Audio button display name hidden for cleaner appearance
- Audio button text spacing improved with extra line break between name and volume
- Migrated 16 commands to use ActionImageStore pattern for efficient caching
- Display commands now use TextImageFactory with ActionImageStore
- All image factories now use Loupedeck SDK's cross-platform BitmapBuilder/BitmapImage APIs
- Eliminated all Windows-only System.Drawing dependencies

### Removed
- BitmapHelper.cs (Windows-only System.Drawing code)
- IconWithTextImageFactory.cs and IconWithTextImageData.cs (unused)
- AudioInputImageFactory.cs and AudioInputImageData.cs (replaced with direct rendering)
- Icon rendering from audio buttons (BitmapBuilder cannot handle SVG format)

### Fixed
- All 78 CA1416 platform-specific warnings eliminated
- Plugin now fully compatible with macOS (no Windows-only dependencies)
- BitmapColor usage corrected in TextWithBackgroundImageFactory
- Audio buttons now update correctly when mute state or volume changes
- ButtonTextRenderer now uses PluginResources.ReadImage instead of non-existent EmbeddedResources
- Audio button image caching removed to allow real-time state reflection

## [0.8.3] - 2026-03-10

### Fixed
- macOS plugin load failure by enabling pluginFolderMac in package metadata
- Plugin now loads successfully on macOS devices

## [0.8.2] - 2026-01-17

### Fixed
- Plugin load failure with "channelName cannot be null" error
- Display commands now properly initialize with default empty parameter
- ActionImageChanged() calls now include required actionParameter

## [0.8.1] - 2026-01-17

### Fixed
- Eliminated jaggy text rendering on all buttons by preventing double text rendering
- Display commands now return null from GetCommandDisplayName() to avoid native text overlay
- Dynamic folders (Scenes, Sources) now render only anti-aliased text via BitmapBuilder

### Added
- ButtonTextRenderer helper class for consistent, reusable text rendering across all display commands
- RenderText(), RenderConnectionStatus(), RenderNotConnected() methods with proper font sizing

### Changed
- Simplified .gitignore to essential patterns, added *.lplug4 to ignored files

## [0.8.0] - 2026-01-17

### Added
- Virtual camera controls (toggle, start, stop)
- Source visibility toggle for sources in current scene
- Profiles dynamic folder showing all available profiles
- Connection status display showing real-time connection state
- Manual reconnect button for user-initiated retry
- Streaming controls (toggle, start, stop)
- Continuous reconnection with exponential backoff (1s to 30s) and jitter (0.85-1.15x)
- Comprehensive disposal pattern with thread safety
- 80 unit tests with full coverage of core functionality

### Changed
- Display commands now use BitmapBuilder for anti-aliased text rendering
- Display commands show "Not Connected" when disconnected
- Virtual camera commands simplified to use base constructor pattern
- Reconnection now uses timer-based approach with auto-restart

### Fixed
- Virtual camera commands now appear in Logi Plugin Service app
- Display commands now properly initialize on connection
- Dynamic folders now clear when OBS disconnects

## [0.1.0] - Initial Development

### Added
- Recording controls (toggle, start, stop, pause/resume)
- Scene management with dynamic folder
- Profile selection with multi-state buttons
- Scene collection selection with multi-state buttons
- Screenshot capture functionality
- Automatic OBS configuration discovery
- Connection resilience with exponential backoff
- Comprehensive logging
- 80 unit tests with full coverage of core functionality
- Display commands for current profile, scene, and scene collection
- Automatic connection on OBS startup
- Direct connection fallback when process detection fails
