# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **Live Audio Folder** dynamic folder (`8. Audio`): real-time per-channel VU meters for live audio inputs on a dB scale (-60dB to 0dB) matching OBS's own mixer, with green/yellow/red zones at -20dB and -10dB; tap a tile to toggle mute
- **Audio Meter Refresh Rate** setting in Plugin Settings (20/10/5 fps, default 10fps)
- **Audio Meter** action (`8. Audio › Meters`): one per audio input, live or not, so a meter for a specific source can go on any button
- The high-volume `InputVolumeMeters` event subscription is only active while a meter is on screen
- The Live Audio Folder shows only live inputs (those OBS is reporting levels for) and updates automatically as scenes and sources change
- Audio meters show a baseline for live-but-silent inputs, a red crossed-out speaker for muted inputs, and a grey crossed-out speaker for inputs that aren't live

### Changed

- Upgraded `obs-websocket-dotnet` from 5.0.1 to 5.7.0 (required for `InputVolumeMeters` subscription support, contributed upstream)
- Upgraded `Microsoft.Extensions.Logging.Abstractions` and `System.Drawing.Common` to 10.0.12
- Button icons restyled to one consistent set: 2px outline glyphs on a 32px grid, a fixed five-colour palette, a slash for off states, grey for unavailable actions, and a tick for the selected scene, scene collection and profile
- Virtual Camera Start/Stop now show a camera instead of the recording dot and square
- Streaming Start/Stop grey out when unavailable, like Recording and Virtual Camera, instead of showing the opposite state in full colour
- Toggle Source Visibility (User defined) shows the source's visibility, following the first source when several are listed
- Switch to Scene (User Defined) shows a tick when its scene is the current scene
- The Profiles, Scene Collections and Media Controls folder buttons show an icon instead of their name
- Renamed the `OBS Profiles` and `OBS Scene Collections` folders to `Profiles` and `Scene Collections`
- The Reconnect button's icon is redrawn in white so it reads on the green, amber and red status backgrounds
- The OBS Scenes folder has its own folder icon
- The plugin only asks OBS for what the device is showing. The Scene Sources and Mixer for Scene Audio folders load their lists when opened, and show "Loading..." across the top row, beside the Back button, until they arrive. While neither is open, scene changes no longer request any source lists. With one open, a scene change takes one request instead of one per scene
- OBS stats are only polled while a stats folder is open or the OBS Stats Summary button is on screen, and they are polled as soon as one appears instead of after the next interval

### Security

- The remote OBS password is no longer written to `config.json`. It is kept in the plugin settings, which the Logi Plugin Service stores encrypted, and a password saved by an earlier version is moved there on first load
- An empty Password field in Plugin Settings now keeps the saved password, so the field can be cleared after saving. The Logi Plugin Service stores Action Editor values unencrypted in the device profile. A new **Clear Saved Password** checkbox removes the saved password

### Fixed

- Media buttons now refresh when playback starts — `MediaInputPlaybackStarted` always arrived with no input name in library 5.0.1, so the event was ignored
- OBS requests that never receive a response now time out after 10s and are logged, instead of blocking indefinitely (library 5.0.1 did not enforce its request timeout)
- Scene Sources and Toggle Source Visibility buttons no longer wait on OBS each time they are drawn, so a slow OBS no longer stalls the device while they are on screen. Their state is read once and then kept up to date from OBS's visibility events
- Media Controls buttons no longer wait on OBS each time they are drawn, and now show Paused and Stopped as soon as media is paused or stopped, including from OBS's own controls. Previously they only caught up on their next redraw
- Switching scenes no longer holds up other OBS updates while the new scene's source lists load. The lists now load in the background, and if scenes are switched quickly only the latest scene's sources are shown
- Renaming an input or scene in OBS no longer leaves the plugin showing the old name. The Mixer, Live Audio, Media and Scene Sources folders and the scene list update straight away, the renamed input stays selected on the dial, and its mute, volume, visibility and playback state carry over. User-defined buttons store the name you typed, so the plugin log now warns which name to change them to
- Adding or removing an input no longer holds up other OBS updates while the input list reloads

## [1.6.2] - 2026-08-21

No functional changes for end users. Internal engineering release.

### Fixed

- `CommandCoordinator` now isolates per-command exceptions during event dispatch — a throwing command no longer prevents other registered commands from receiving the same notification
- Fixed a recurring build bug where `obj/` intermediate output location depended on invocation method (`.sln` vs bare `.csproj` vs `dotnet test`), causing spurious `CS0579` duplicate-attribute errors
- Resolved nullable-reference build warnings (CS8600/CS8602) in the test project

### Changed

- Migrated target framework from .NET 8.0 to .NET 10.0 (plugin project, test project, CI workflow, README); verified against a real Logi Plugin Service install

### Documentation

- Moved AI coding-assistant docs from `.amazonq/rules/` to `docs/ai/`; added `AGENTS.md`/`CLAUDE.md` entry points
- Added CI markdown linting (`rumdl`) and fixed existing violations repo-wide
- `AGENTS.md` now points assistants at the Logi Actions SDK's official AI-friendly doc index instead of inferring SDK behavior
- Removed `tools/InspectSdk`, a reflection-based SDK reverse-engineering utility, now unnecessary
- Backfilled missing CHANGELOG entries for v1.5.1, v1.6.0, and v1.6.1

### Dependencies

- `System.Drawing.Common` 10.0.10 → 10.0.11
- `Microsoft.Extensions.Logging.Abstractions` 10.0.10 → 10.0.11

## [1.6.1] - 2026-08-21

### Fixed

- Sources using `game_capture` and `browser_source` input kinds were not appearing in the Audio Mixer or Scene Audio folders — both kinds now included in the audio input filter

### Changed

- OBS Profiles folder moved to `6. Profiles › Available Profiles` (was a separate sub-group)
- OBS Scene Collections folder moved to `7. Scenes › Available Collections` (was a separate sub-group)
- Default button layouts now shipped for all six supported device types (`DefaultProfile20/30/50/70/71/72.lp5` — Loupedeck CT, Loupedeck Live, Loupedeck Live S, MX Creative Keypad, MX Creative Dialpad, Logitech Actions Ring); Logi Plugin Service applies these automatically on first install or new profile creation

### Dependencies

- `System.Drawing.Common` 9.0.0 → 10.0.10
- `Microsoft.Extensions.Logging.Abstractions` 9.0.4 → 10.0.10
- `Microsoft.NET.Test.Sdk` 18.7.0 → 18.8.1
- `actions/setup-dotnet` (CI) 5 → 6

## [1.6.0] - 2026-07-18

### Added

- Scene Select multi-state command (`7. Scenes › Available Scenes`) — mirrors Profile Select / Scene Collection Select, studio-mode-aware scene switching
- Audio Source Select multi-state command (`8. Audio › Available Sources`) — per-input name, volume in dB, and mute state; updates automatically as inputs are added/removed
- Scene Collections Dynamic Folder (`7. Scenes › Available Collections`) — folder-based alternative to Scene Collection Select

### Changed

- Actions reorganised into functional sub-groups within their top-level group (Available Scenes / Available Collections / User Defined, etc.) for easier navigation in the action picker
- `99. User Defined Actions` group retired — all configurable actions now live alongside their related controls

### Fixed

- `DoubleTapHelper` race condition on `_tapStates` and `CancellationTokenSource` leak — dictionary access now synchronised, token sources disposed on all paths (Assessment #4)
- `OBSStats`/`OBSStreamStats` null-object pattern added (`.Empty`) — `OBSActionExecutor`/`OBSFacade` return `Empty` instead of `null`; null guards removed from stats display commands (Assessment #7)
- `MediaDynamicFolder` now implements `IInputsListAwareCommand` and updates automatically when media sources are added/removed in OBS (Assessment #10)
- `CommandRegistry` bypass fixed — `OnOBSConnected`/`OnOBSDisconnected` now route through `CommandCoordinator.NotifyConnected()`/`NotifyDisconnected()` instead of hardcoded singleton calls (Assessment #1)
- Scene sources registry bypass fixed — `OnCurrentSceneChanged` routes through the registry via `ISceneSourcesAwareCommand` instead of calling `SourcesDynamicFolder`/`SceneAudioSourcesDynamicFolder` directly (Assessment #2)

### Security

- Password field in Plugin Settings labelled `Password (sensitive)` (Assessment #11)

### Testing

- 389 unit tests (up from 362 at v1.5.1)

## [1.5.1] - 2026-07-08

### Fixed

- Audio Select Folder, Audio Volume Folder, and Media Controls Folder showing empty on connect — added explicit `OnConnected()`/`OnDisconnected()` calls in `OBSWebSocketManager`, matching the existing `AudioMixerDynamicFolder` pattern
- Media play action not triggering playback in OBS — single tap now uses Pause (playing), Play/resume (paused), or Restart (stopped/ended/none) instead of always using `PLAY`, which only resumes from `PAUSED`

## [1.5.0] - 2026-07-05

### Added

- WebSocket Disabled state on connection status button (orange background) when OBS config has `server_enabled=false`
- Three distinct connection states: Connected (green), Disconnected (red), WebSocket Disabled (orange)
- Plugin status message for disabled state: "OBS WebSocket server is disabled. Enable it in OBS Tools menu."
- `IsServerDisabled` property on OBSConfigReader
- `WebSocketServerDisabled` event on ConnectionManager
- 3 new unit tests for IsServerDisabled property (362 total)

### Changed

- Connection status display now refreshes on connect, disconnect, and server-disabled events

### Documentation

- Updated release-process.md with correct LogiPluginTool location (dotnet global tool)

## [1.4.1] - 2026-06-27

### Fixed

- Missing per-application audio capture sources (Spotify, Discord, game audio) in Audio Mixer and Scene Audio folders — added `wasapi_process_output_capture` to audio input kind filter (OBS 28+ Application Audio Capture)

## [1.4.0] - 2026-06-26

### Added

- Disk Space tile in OBS Stats Folder (colour-coded: red <1GB, yellow <10GB)
- Render Time tile in OBS Stats Folder (colour-coded: red >10ms, yellow >5ms)
- FreeDiskSpace property to OBSStats model
- Debug method to IPluginLog interface for services layer logging

### Fixed

- FPS display showing incorrect value (3200 instead of 60) — now uses direct `stats.FPS` property from OBS instead of deriving from AverageFrameTime
- Flaky test timing for StartReplayBuffer error-path test (extended delay)

### Changed

- Logging levels refined across entire codebase:
  - Render/UI calls (GetCommandImage, GetEncoderNames) downgraded to Trace
  - Operational detail (folder updates, selection state, intermediate steps) downgraded to Debug
  - Duplicate cross-layer logging removed (volume/monitoring logged once in executor)
- ConnectionManager now owns and relays Connected/Disconnected events (main plugin no longer holds direct OBSWebSocketManager reference)
- PluginSettingsCommand renamed from ConnectionConfigureCommand (file + class)

### Documentation

- Updated INSTALL.md, CONTRIBUTING.md, CONFIGURATION.md, config.sample.json
- Updated USER_MANUAL.md, README.md with all current features
- Updated memory bank (test-coverage.md, tech.md, structure.md, product.md, obs-websocket-api-complete.md)

## [1.3.1] - 2026-06-23

### Fixed

- Excluded PluginApi.dll from plugin package output (provided by Logi Plugin Service at runtime)

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
