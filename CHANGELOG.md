# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.0] - 2026-06-13

### Added

- Remote OBS connection support: configure IP/port/password to control OBS on a remote device
- Plugin Settings command (ActionEditorCommand): configure connection mode and stats polling interval
- OBS Stats Summary display: single button showing FPS, CPU%, and dropped frames (colour-coded)
- OBS Stats Folder: dynamic folder with individual tiles per stat (FPS, CPU, Memory, Render Missed, Encode Skipped, Total Dropped)
- Stream Stats Folder: live streaming statistics (Duration, Bytes Sent, Congestion, Skipped Frames, Total Frames)
- Media Controls Folder (Group 9): dynamic folder of media sources with play/pause/stop
  - Single tap stopped/paused: Play; single tap playing: Pause; double tap: Stop
  - Colour-coded: green=playing, yellow=paused, grey=stopped/ended
- Media Action (User Defined): ActionEditorCommand with source name and action listbox (Play, Pause, Stop, Restart, Next, Previous)
- StatsService: timer-based polling with configurable interval (2s, 5s, 10s)
- Subscribe to InputAudioMonitorTypeChanged event: audio folders refresh when monitor type changes in OBS
- Subscribe to SceneItemCreated/SceneItemRemoved events: Sources folder auto-refreshes
- Subscribe to InputCreated/InputRemoved events: Audio Mixer auto-refreshes
- Subscribe to MediaInputPlaybackStarted/MediaInputPlaybackEnded events: real-time media state updates
- IInputMonitorAwareCommand interface for audio monitoring events
- OBSStats and OBSStreamStats models with derived properties
- PluginConfigReader.SaveConfig() for persistent configuration
- 37 new unit tests (348 total)

### Changed

- ConnectionConfigureCommand renamed to PluginSettingsCommand (broader scope)
- OBSConnectionSettings now accepts any valid IP address (previously localhost-only)
- ConnectionManager branches between local auto-discovery and remote direct connection
- StatsService starts on connect, stops on disconnect

### Fixed

- Reconnection race condition: added _connectingInProgress flag to prevent duplicate connection attempts when timer fires before initial connection completes

## [1.2.0] - 2026-06-13

### Added

- User Defined Actions group (Group 99) for user-configurable commands
- SourceVisibilityAdjustableCommand: toggle source visibility with configurable source name(s) and optional scene name
- AudioMuteAdjustableCommand: toggle mute for a named audio source
- AudioMonitoringCycleAdjustableCommand: cycle monitoring type for a named audio source
- AudioSelectAdjustableCommand: toggle global audio source selection for a named source
- AudioSelectDynamicFolder: selection-only folder for setting global audio source
- AudioVolumeDynamicFolder: MX-compatible folder with adjustment tiles for big wheel volume control
- SelectedSourceVolumeAdjustment: standalone PluginDynamicAdjustment for wheel/dial volume control of selected source

### Changed

- SceneSwitchAdjustableCommand moved to User Defined Actions group
- Audio source selection now accessible via dedicated folder (AudioSelectDynamicFolder) in addition to double-tap in Audio Mixer

## [1.1.0] - 2026-06-08

### Added

- SceneSwitchAdjustableCommand for encoder-based scene switching with configurable profile, collection, and scene
- Enhanced scene collection change handling with automatic current scene update
- Validation logging for profile/collection/scene switching

### Changed

- Scene collection switching now waits 100ms before querying current scene for reliability

## [1.0.1] - 2026-05-xx

### Added

- Audio volume display on audio mixer and scene audio buttons (0-100%)
- GetInputVolume() and SetInputVolume() API methods for volume control
- OnInputVolumeChanged() event handler for real-time volume updates
- Scene audio sources folder showing audio inputs in the current scene
- AudioVolumeWheelTool for encoder-based volume adjustment
- AudioSelectionState for dial control source selection
- DoubleTapHelper for distinguishing single/double tap on buttons
- Audio monitoring type cycling (None → Monitor Only → Monitor & Output)
- ButtonImageHelper static class for simplified image rendering

### Changed

- All toggle commands (Recording, Streaming, Virtual Camera, Replay Buffer, Studio Mode) now use ToggleCommandBase
- All start/stop commands now use StartStopCommandBase
- Simplified image rendering system with ButtonImageHelper (6 simple methods)
- All commands migrated to use ButtonImageHelper API
- Audio buttons now display input name and volume percentage with colored text
- Reduced image rendering code by ~80% while maintaining same functionality

### Removed

- ActionImageStore, IActionImageFactory, IActionImageData (replaced with ButtonImageHelper)
- StateImageFactory, TextImageFactory, SimpleIconImageFactory
- StateImageData, TextImageData, SimpleIconImageData models
- BitmapHelper.cs (Windows-only System.Drawing code)

### Fixed

- All 78 CA1416 platform-specific warnings eliminated
- Plugin now fully compatible with macOS (no Windows-only dependencies)
- Audio buttons now update correctly when mute state or volume changes

## [1.0.0] - 2026-04-xx

### Added

- Command Registry pattern with interface-based self-registration
- CommandCoordinator and CommandRegistry for centralized event distribution
- OBSFacade for simplified OBS interface access
- ConnectionManager for encapsulated connection lifecycle
- Replay buffer controls (toggle, start, stop, save)
- Studio mode toggle and transition commands
- Audio mixer folder with mute/unmute controls
- Configurable log levels via JSON configuration file
- OBSTimings centralized timing constants
- PluginConfigReader for plugin configuration

### Changed

- Refactored main plugin class from ~400 lines to 289 lines (4 focused classes)
- All commands self-register via IObsCommand interfaces
- Eliminated 15+ manual notification calls from main plugin

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

### Added

- ButtonTextRenderer helper class for consistent text rendering

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
- Display commands for current profile, scene, and scene collection
- Automatic connection on OBS startup
