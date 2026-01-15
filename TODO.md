# TODO

## High Priority

- [x] Add streaming controls (start/stop/toggle streaming)
- [x] Add source visibility toggle commands
- [x] Add virtual camera controls (start/stop)
- [ ] Add audio mixer controls (mute/unmute sources)
- [ ] Implement replay buffer controls (start/stop/save)

## Medium Priority

- [ ] Add transition selection and duration controls
- [ ] Implement studio mode toggle
- [ ] Add filter enable/disable controls
- [ ] Create custom scene transition triggers
- [ ] Add hotkey trigger commands
- [ ] Implement media source controls (play/pause/restart)

## Low Priority

- [ ] Add statistics display (FPS, CPU usage, dropped frames)
- [ ] Implement custom image overlays for scene buttons
- [ ] Add scene item transform controls (position, scale, rotation)
- [ ] Create preset configurations for common streaming setups
- [ ] Add multi-language support for UI text

## Technical Improvements

- [x] Implement comprehensive disposal pattern with thread safety
- [x] Add exponential backoff with jitter for reconnection
- [x] Implement continuous reconnection with auto-restart
- [ ] Add configuration UI for manual WebSocket settings override
- [ ] Implement connection status indicator command
- [ ] Add retry limit configuration for reconnection attempts
- [ ] Create diagnostic logging toggle command
- [ ] Add WebSocket protocol version detection and compatibility warnings
- [ ] Implement graceful degradation for unsupported OBS versions

## Testing

- [ ] Add integration tests with actual OBS instance
- [ ] Create performance benchmarks for connection handling
- [ ] Add UI automation tests for command state updates
- [ ] Implement stress tests for rapid command execution

## Documentation

- [ ] Create video tutorial for setup and usage
- [ ] Add troubleshooting guide with common issues
- [ ] Document all available OBS WebSocket events
- [ ] Create developer guide for extending the plugin
- [ ] Add architecture diagrams

## Completed

- [x] Add streaming controls (start/stop/toggle streaming)
- [x] Add source visibility toggle commands
- [x] Add virtual camera controls (start/stop/toggle)
- [x] Implement comprehensive disposal pattern with thread safety
- [x] Add exponential backoff with jitter for reconnection
- [x] Implement continuous reconnection with auto-restart
- [x] Add manual reconnect command
- [x] Simplify display commands to use text-based buttons
- [x] Ensure display buttons get initial state on connection
- [x] Add description to scenes dynamic folder
- [x] Implement recording controls (start/stop/toggle/pause)
- [x] Implement scene management with dynamic folder
- [x] Implement profile and scene collection selection
- [x] Add screenshot capture functionality
- [x] Implement automatic OBS configuration discovery
- [x] Add connection resilience with exponential backoff
- [x] Implement comprehensive logging
- [x] Add test coverage for all core functionality (80 tests)

## Future Considerations

- [ ] Support for multiple OBS instances
- [ ] Cloud sync for plugin configurations
- [ ] Custom scripting support for advanced automation
- [ ] Integration with other streaming tools (Streamlabs, StreamElements)
- [ ] Mobile companion app for remote control
