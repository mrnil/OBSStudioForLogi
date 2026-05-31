# TODO

## High Priority

- [ ] Implement volume adjustment controls (faders/encoders or +/- buttons)
- [ ] Add audio monitoring toggle (None/Monitor Only/Monitor & Output)
- [ ] Add filter enable/disable controls for sources

## Medium Priority

- [ ] Add audio level meters (real-time VU meters with color coding)
- [ ] Add audio sync offset controls (display and adjust)
- [ ] Add audio track assignment controls (toggle tracks 1-6)
- [ ] Add stereo balance controls for audio inputs
- [ ] Add audio filter controls (list and toggle filters)
- [ ] Add transition selection and duration controls
- [ ] Add hotkey trigger commands
- [ ] Implement media source controls (play/pause/restart)

## Low Priority

- [ ] Add audio quick presets (Mute All, Reset All Volumes, custom macros)
- [ ] Add statistics display (FPS, CPU usage, dropped frames)
- [ ] Implement custom image overlays for scene buttons
- [ ] Add scene item transform controls (position, scale, rotation)
- [ ] Create preset configurations for common streaming setups
- [ ] Add multi-language support for UI text

## Technical Improvements

- [ ] Add configuration UI for manual WebSocket settings override
- [ ] Add retry limit configuration for reconnection attempts
- [ ] Create diagnostic logging toggle command
- [ ] Add WebSocket protocol version detection and compatibility warnings
- [ ] Implement graceful degradation for unsupported OBS versions
- [ ] Add error recovery for transient WebSocket failures

## Testing

- [ ] Add integration tests with actual OBS instance
- [ ] Create performance benchmarks for connection handling
- [ ] Add UI automation tests for command state updates
- [ ] Implement stress tests for rapid command execution

## Documentation

- [ ] Create video tutorial for setup and usage
- [ ] Expand troubleshooting guide with more common issues
- [ ] Document all available OBS WebSocket events (reference guide)

## Completed

### Features

- [x] Add streaming controls (start/stop/toggle streaming)
- [x] Add source visibility toggle commands
- [x] Add virtual camera controls (start/stop/toggle)
- [x] Add replay buffer controls (toggle/start/stop/save)
- [x] Add studio mode toggle and transition controls
- [x] Add manual reconnect command
- [x] Add audio mixer controls (mute/unmute audio inputs)
- [x] Add audio volume display (0-100% on buttons)
- [x] Add scene audio sources folder (audio inputs in current scene)
- [x] Implement recording controls (start/stop/toggle/pause)
- [x] Implement scene management with dynamic folder
- [x] Implement profile and scene collection selection
- [x] Add profiles dynamic folder
- [x] Add screenshot capture functionality
- [x] Add connection status display
- [x] Add current profile display
- [x] Add current scene display
- [x] Add current scene collection display

### Technical

- [x] Implement ToggleCommandBase and StartStopCommandBase to eliminate duplication
- [x] Migrate all toggle and start/stop commands to use base classes
- [x] Add SceneSwitchAdjustableCommand for encoder-based scene switching
- [x] Implement comprehensive disposal pattern with thread safety
- [x] Add exponential backoff with jitter for reconnection (0.85-1.15x)
- [x] Implement continuous reconnection with auto-restart
- [x] Implement automatic OBS configuration discovery
- [x] Implement comprehensive logging
- [x] Add test coverage for all core functionality (140 tests passing)
- [x] Simplify image rendering with ButtonImageHelper (replaced Factory + Store + Data pattern)
- [x] Eliminate Windows-only System.Drawing dependencies for macOS compatibility
- [x] Achieve full cross-platform compatibility (Windows + macOS)

### Documentation

- [x] Create comprehensive memory bank documentation
- [x] Document icon update patterns and best practices
- [x] Document simplified image rendering system with ButtonImageHelper
- [x] Create architecture documentation with diagrams
- [x] Create developer guide for extending the plugin (CONTRIBUTING.md)
- [x] Add troubleshooting guide (README.md)
- [x] Document release process
- [x] Create installation guide (INSTALL.md)
- [x] Maintain changelog with version history

### UI/UX

- [x] Simplify display commands to use BitmapBuilder for text rendering
- [x] Ensure display buttons get initial state on connection
- [x] Add description to scenes dynamic folder
- [x] Fix display commands to show "Not Connected" when disconnected
- [x] Implement efficient image caching for all state-based and display commands
- [x] Fix source visibility toggle to work bidirectionally
- [x] Fix source visibility icon updates with delayed callback pattern
- [x] Fix scene icon updates to use CommandImageChanged (not ButtonActionNamesChanged)
- [x] Fix profile icon updates to use CommandImageChanged (not ButtonActionNamesChanged)
- [x] Fix audio button color updates to use CommandImageChanged for individual buttons
- [x] Correct audio button font sizes (16pt Width90, 14pt Width60)

## Future Considerations

- [ ] Support for multiple OBS instances
- [ ] Cloud sync for plugin configurations
- [ ] Custom scripting support for advanced automation
- [ ] Integration with other streaming tools (Streamlabs, StreamElements)
- [ ] Mobile companion app for remote control
