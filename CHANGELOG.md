# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Virtual camera controls (toggle, start, stop)
- Source visibility toggle for sources in current scene
- Profiles dynamic folder showing all available profiles
- Connection status display showing real-time connection state
- Manual reconnect button for user-initiated retry
- Streaming controls (toggle, start, stop)
- Continuous reconnection with exponential backoff and jitter
- Comprehensive disposal pattern with thread safety

### Changed
- Display commands now use BitmapBuilder for anti-aliased text rendering
- Display commands show "Not Connected" when disconnected
- Virtual camera commands simplified to use base constructor pattern
- Reconnection now uses timer-based approach with auto-restart

### Fixed
- Virtual camera commands now appear in Logi Plugin Service app
- Display commands now properly initialize on connection
- Dynamic folders now clear when OBS disconnects

## [0.1.0] - Initial Release

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
