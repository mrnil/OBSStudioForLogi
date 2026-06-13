# Logitech/Loupedeck Plugin for interacting with OBS Studio

[![Dependency Check](https://github.com/mrnil/OBSStudioForLogi/actions/workflows/dependency-check.yml/badge.svg)](https://github.com/mrnil/OBSStudioForLogi/actions/workflows/dependency-check.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![OBS Studio](https://img.shields.io/badge/OBS%20Studio-28.0%2B-302E31?logo=obsstudio)](https://obsproject.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Release](https://img.shields.io/github/v/release/mrnil/OBSStudioForLogi)](https://github.com/mrnil/OBSStudioForLogi/releases)
[![Issues](https://img.shields.io/github/issues/mrnil/OBSStudioForLogi)](https://github.com/mrnil/OBSStudioForLogi/issues)

Control OBS Studio directly from your Logitech/Loupedeck hardware device with dedicated buttons and real-time status displays.

## NOTE

Please note that this is not a plugin or an extension for OBS. This project is not supported by or affliated with OBS Project and is not listed in their community.

This project interacts with the websocket server using [obs-websocket-dotnet](https://github.com/BarRaider/obs-websocket-dotnet) and is a plugin for Logitech Option+ and Loupedeck software that utilise the underlying LogiPluginService and [Logi Actions SDK](https://logitech.github.io/actions-sdk-docs/). See [the project structure](https://github.com/mrnil/OBSStudioForLogi/blob/main/.amazonq/rules/memory-bank/structure.md) for further details.

An AI Coding Assistant has been used to support the development of this project and the understanding of associated technologies and libraries. The project uses strong engineering principles including [TDD](https://github.com/mrnil/OBSStudioForLogi/blob/main/.amazonq/rules/TestDrivenDevelopment.md), [secure coding principles](https://github.com/mrnil/OBSStudioForLogi/blob/main/.amazonq/rules/SecureCoding.md) and [conventional commits](https://github.com/mrnil/OBSStudioForLogi/blob/main/.amazonq/rules/CodeCommits.md) to ensure every change is well understood, reliable and tested as appropriate. Please do report any issues you find with the tests or code base that I may have missed.

## Features

- **Automatic Connection**: Discovers OBS WebSocket settings automatically from OBS configuration
- **Streaming Controls**: Start, stop, toggle streaming with visual state indicators
- **Recording Controls**: Start, stop, toggle, pause/resume recording with visual state indicators
- **Virtual Camera**: Start, stop, toggle virtual camera output
- **Scene Management**: Switch scenes with dynamic folder and visual feedback
- **Source Visibility**: Toggle visibility of sources in current scene
- **Audio Mixer**: Mute/unmute audio inputs with visual feedback (red=muted, green=unmuted)
- **Audio Volume Control**: Adjust volume via MX big wheel (folder tiles) or standalone dial adjustment
- **Audio Source Selection**: Global audio source selection for wheel/dial volume control
- **User Defined Actions**: Configurable actions for source visibility, audio mute, monitoring, and audio selection
- **Profile Management**: Switch between OBS profiles with selection indicators and dynamic folder
- **Scene Collections**: Switch between scene collections with selection indicators
- **Screenshot Capture**: Take screenshots with automatic path detection
- **Status Displays**: Real-time display of current profile, scene collection, active scene, and connection status
- **Resilient Connection**: Automatic reconnection with exponential backoff and jitter, manual reconnect button

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 8.0 SDK (for development)

## Installation

1. Build the project:

   ```bash
   dotnet build src/OBSStudioForLogiPlugin.csproj -c Release
   ```

2. The plugin automatically installs to Logi Plugin Service via the `.link` file

3. Launch OBS Studio and the plugin connects automatically

## Available Controls

### OBS (Group 1)

- **Screenshot**: Capture screenshot to Pictures/Documents/Desktop folder
- **Reconnect**: Manually retry connection to OBS
- **Studio Mode Toggle**: Enable/disable OBS studio mode for preview/program workflow
- **Studio Mode Transition**: Transition preview scene to program (only works when studio mode is enabled)
- **Connection Status Display**: Shows "Connected" (green) or "Disconnected" (red)

### Streaming (Group 2)

- **Streaming Toggle**: Toggle streaming on/off
- **Streaming Start**: Start streaming
- **Streaming Stop**: Stop streaming

### Recording (Group 3)

- **Recording Toggle**: Toggle recording on/off
- **Recording Start**: Start recording
- **Recording Stop**: Stop recording
- **Recording Pause/Resume**: Pause or resume active recording

### Replay Buffer (Group 4)

- **Replay Buffer Toggle**: Start/stop replay buffer
- **Save Replay Buffer**: Save the current replay buffer to disk

### Virtual Camera (Group 5)

- **Virtual Camera Toggle**: Toggle virtual camera on/off
- **Virtual Camera Start**: Start virtual camera
- **Virtual Camera Stop**: Stop virtual camera

### Profiles (Group 6)

- **Profile Select**: Switch between OBS profiles (multi-state buttons)
- **Profiles Folder**: Dynamic folder with all available profiles
- **Current Profile Display**: Shows active profile name

### Scenes (Group 7)

- **Scene Collection Select**: Switch between scene collections (multi-state buttons)
- **Scenes Folder**: Dynamic folder with all available scenes
  - When studio mode is disabled: Switches scene to program (live)
  - When studio mode is enabled: Switches scene to preview (not live yet)
- **Sources Folder**: Dynamic folder with sources in current scene (toggle visibility)
- **Current Scene Display**: Shows active scene name
- **Current Scene Collection Display**: Shows active scene collection name

### Audio (Group 8)

- **Audio Mixer Folder**: Dynamic folder with all audio inputs (mute/unmute toggle, volume display)
  - Visual feedback: Green text = unmuted, Red text = muted
  - Displays input name and current volume percentage (0-100%)
  - Real-time updates when mute state or volume changes in OBS
  - Toggle mute by pressing the button
- **Scene Audio Folder**: Dynamic folder with audio sources in the current scene
  - Shows only audio inputs present in the active scene
  - Same mute/unmute controls, volume display, and visual feedback as Audio Mixer
  - Updates automatically when switching scenes or changing volume
- **Audio Select Folder**: Dynamic folder for selecting the global audio source
  - Single tap selects/deselects an audio source
  - Displays mute state, volume, and monitoring mode for each source
  - Selection persists after folder is closed
  - Selected source is used by volume wheel/dial controls
- **Audio Volume Folder**: Dynamic folder with adjustment tiles for MX big wheel
  - Tap a tile to arm the MX big wheel for that source's volume
  - Displays volume percentage and mute state (green/red)
  - Compatible with MX Creative Console Dialpad
- **Selected Source Volume**: Standalone adjustment for wheel/dial
  - Controls volume of the globally selected audio source
  - Drag onto MX big wheel, CT wheel, or any dial
  - Turn to adjust volume, press to reset to 100%

### User Defined Actions (Group 99)

- **Switch to Scene (Adjustable)**: Switch to a specific scene with optional profile and collection switching
  - Configure profile name, collection name, and scene name via textboxes
- **Toggle Source Visibility (User defined)**: Toggle visibility of one or more sources
  - Comma-separate multiple source names
  - Optional scene name (defaults to current active scene)
- **Toggle Audio Mute (User defined)**: Toggle mute/unmute for a named audio source
- **Cycle Audio Monitoring (User defined)**: Cycle monitoring type for a named audio source
  - Cycles: None → Monitor Only → Monitor & Output
- **Select Audio Source (User defined)**: Toggle global audio source selection
  - Selects/deselects the named source as the globally selected audio source
  - Used by Selected Source Volume adjustment and other audio controls

## Configuration

No manual configuration required. The plugin automatically reads OBS WebSocket settings from:

- **Windows**: `%AppData%\obs-studio\plugin_config\obs-websocket\config.json`
- **macOS**: `~/Library/Application Support/obs-studio/plugin_config/obs-websocket/config.json`

## Development

### Build

```bash
dotnet build src/OBSStudioForLogiPlugin.csproj
```

### Test

```bash
dotnet test tests/OBSStudioForLogiPlugin.Tests/OBSStudioForLogiPlugin.Tests.csproj
```

**Note:** Tests are run locally only. The GitHub Actions workflow performs dependency checking and build validation but does not run tests due to timing issues with fire-and-forget Task.Run patterns in CI environments.

### Clean

```bash
dotnet clean OBSStudioForLogiPlugin.sln
```

## Architecture

- **TDD Approach**: Comprehensive test coverage with 300 unit tests
- **Dependency Injection**: Interface-based design for testability
- **Async Operations**: All OBS operations wrapped in Task.Run to prevent UI freezing
- **Event-Driven**: Real-time updates via OBS WebSocket events
- **Singleton Pattern**: Command instances accessible via static Instance properties
- **Resilient Reconnection**: Timer-based reconnection with exponential backoff (1s to 30s) and jitter (0.85-1.15x)
- **Command Base Classes**: ToggleCommandBase and StartStopCommandBase eliminate duplication across similar commands

## Troubleshooting

**Plugin doesn't connect:**

- Ensure OBS Studio is running
- Verify obs-websocket is enabled in OBS (Tools → WebSocket Server Settings)
- Check logs in Logi Plugin Service

**Commands disabled:**

- Plugin only enables when connected to OBS
- Wait for automatic connection or restart OBS Studio

## License

See LICENSE file for details.
