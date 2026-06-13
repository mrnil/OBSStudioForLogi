# Release v1.2.0 - User Defined Actions & Audio Enhancements

## Summary

This release introduces User Defined Actions — configurable commands that allow users to set up custom buttons for frequently used operations without navigating dynamic folders. It also adds MX Creative Console support for audio volume control via the big wheel, a dedicated audio source selection folder, and a standalone volume adjustment that works on any wheel or dial.

## New Features

### User Defined Actions (Group 99)

Five new configurable actions using ActionEditorCommand textboxes:

- **Switch to Scene (Adjustable)** — Switch to a specific scene with optional profile and collection switching. Configure all three via textboxes.
- **Toggle Source Visibility (User defined)** — Toggle visibility of one or more sources (comma-separated). Optional scene name defaults to current active scene.
- **Toggle Audio Mute (User defined)** — Toggle mute/unmute for a named audio source with a single tap.
- **Cycle Audio Monitoring (User defined)** — Cycle monitoring type (None → Monitor Only → Monitor & Output) for a named audio source.
- **Select Audio Source (User defined)** — Toggle global audio source selection. The selected source is used by volume wheel/dial controls.

### Audio Source Status Display

- **Audio Source Status (User defined)** — Display-only button showing mute state (green/red), volume %, monitoring mode, and selection border for a specific audio source. Updates in real-time.

### Audio Select Folder

- New dynamic folder showing all audio inputs with single-tap selection
- Displays mute state, volume %, and monitoring mode for each source
- Selection persists after folder is closed (in-memory)
- Selected source is used by volume wheel/dial controls

### Audio Volume Folder (MX Creative Console)

- New dynamic folder with adjustment tiles for the MX big wheel
- Tap a tile to arm the big wheel for that source's volume
- Turn the big wheel to adjust volume
- Displays volume percentage and mute state (green/red)

### Standalone Volume Adjustment

- **Selected Source Volume** — PluginDynamicAdjustment for any wheel or dial
- Controls volume of the globally selected audio source
- Drag onto MX big wheel, CT wheel, or any dial
- Turn to adjust volume, press to reset to 100%

## Improvements

### Event-Based Audio Selection

- `AudioSelectionState.SelectionChanged` event ensures all UI components update automatically when selection changes from any source
- Eliminates manual notification coupling between commands

### Code Quality

- Extracted `AudioHelpers` for shared audio state rendering (eliminated 3x code duplication)
- Refactored `ButtonImageHelper` — removed 6 pass-through methods, keeping only `Icon()` and `IconWithBackground()`
- `AudioSelectDynamicFolder` now inherits from `AudioInputDynamicFolderBase`
- Added `ButtonTextRenderer.RenderTextWithBorder` raw-dimension overload for ActionEditorCommand support

## Technical Details

- 305 unit tests passing
- 0 build warnings
- TDD approach for all new features
- Compatible with MX Creative Console (Keypad + Dialpad) and Loupedeck CT

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service 6.0+
- .NET 8.0 Runtime

## Installation

1. Download `OBSStudioForLogiPlugin-v1.2.0.lplug4`
2. Double-click to install, or use: `LogiPluginTool.exe install OBSStudioForLogiPlugin-v1.2.0.lplug4`
3. Restart Logi Plugin Service if prompted
4. Launch OBS Studio — the plugin connects automatically

## Known Issues

- ActionEditorCommand `GroupName` property is not respected by Loupedeck software UI — user-defined actions appear ungrouped. Reported as SDK bug.
- ActionEditorCommand `displayName` property is not suppressed using `isWidget` for either Option+ or Loupedeck software. Reported as SDK bug.
- ActionEditorCommand `getCommandImage` is ignored in Option+ affecting all user defined actions. Note - you can still set user defined titles and edit the underlying icon, colour etc. Reported as SDK bug.
