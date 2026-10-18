# Logitech/Loupedeck Plugin for interacting with OBS Studio

[![Dependency Check](https://github.com/mrnil/OBSStudioForLogi/actions/workflows/dependency-check.yml/badge.svg)](https://github.com/mrnil/OBSStudioForLogi/actions/workflows/dependency-check.yml)
[![Markdown Lint](https://github.com/mrnil/OBSStudioForLogi/actions/workflows/markdown-lint.yml/badge.svg)](https://github.com/mrnil/OBSStudioForLogi/actions/workflows/markdown-lint.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![OBS Studio](https://img.shields.io/badge/OBS%20Studio-28.0%2B-302E31?logo=obsstudio)](https://obsproject.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Release](https://img.shields.io/github/v/release/mrnil/OBSStudioForLogi)](https://github.com/mrnil/OBSStudioForLogi/releases)
[![Issues](https://img.shields.io/github/issues/mrnil/OBSStudioForLogi)](https://github.com/mrnil/OBSStudioForLogi/issues)

Control OBS Studio directly from your Logitech/Loupedeck hardware device with dedicated buttons and real-time status displays.

## NOTE

Please note that this is not a plugin or an extension for OBS. This project is not supported by or affiliated with the OBS Project and is not listed in their community.

This project interacts with the websocket server using [obs-websocket-dotnet](https://github.com/BarRaider/obs-websocket-dotnet) and is a plugin for Logitech Option+ and Loupedeck software that utilise the underlying LogiPluginService and [Logi Actions SDK](https://logitech.github.io/actions-sdk-docs/). See [the project structure](https://github.com/mrnil/OBSStudioForLogi/blob/main/docs/ai/structure.md) for further details.

An AI Coding Assistant has been used to support the development of this project and the understanding of associated technologies and libraries. The project uses strong engineering principles including [TDD](https://github.com/mrnil/OBSStudioForLogi/blob/main/docs/ai/guidelines.md), [secure coding principles](https://github.com/mrnil/OBSStudioForLogi/blob/main/docs/ai/secure-coding.md) and [conventional commits](https://github.com/mrnil/OBSStudioForLogi/blob/main/docs/ai/commit-conventions.md) to ensure every change is well understood, reliable and tested as appropriate. Please do report any issues you find with the tests or code base that I may have missed.

## Features

- **Automatic Connection**: Discovers OBS WebSocket settings automatically from OBS configuration
- **Streaming Controls**: Start, stop, toggle streaming with visual state indicators
- **Recording Controls**: Start, stop, toggle, pause/resume recording with visual state indicators
- **Virtual Camera**: Start, stop, toggle virtual camera output
- **Scene Management**: Switch scenes with a dynamic folder or per-scene buttons, with visual feedback
- **Source Visibility**: Toggle visibility of sources in current scene
- **Audio Mixers**: Mute/unmute audio inputs with visual feedback (red=muted, green=unmuted)
- **Audio Volume Control**: Adjust volume via MX big wheel (folder tiles) or standalone dial adjustment
- **Audio Display**: Volume shown in dB format with full OBS range support (up to +26 dB)
- **Audio Source Selection**: Global audio source selection for wheel/dial volume control
- **Audio Meters**: Real-time per-channel VU meters on a dB scale matching OBS's own mixer — a Live Audio Folder of everything currently live, plus a per-source meter action for any button
- **User Defined Actions**: Configurable actions for scene switching, source visibility, audio mute, monitoring, audio selection, audio status display and media actions
- **Profile Management**: Switch between OBS profiles with selection indicators and dynamic folder
- **Scene Collections**: Switch between scene collections with selection indicators
- **Screenshot Capture**: Take screenshots with automatic path detection
- **Status Displays**: Real-time display of current profile, scene collection, active scene, and connection status
- **Resilient Connection**: Automatic reconnection with exponential backoff and jitter, manual reconnect button
- **Remote OBS Support**: Connect to OBS on a remote device via configurable IP/port/password
- **Performance Monitoring**: Real-time stats display (FPS, CPU, memory, dropped frames) with colour-coded thresholds
- **Streaming Stats**: Live stream statistics (duration, bytes sent, congestion, skipped frames)
- **Media Controls**: Play, pause, stop media sources with dynamic folder and user-defined actions

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 10.0 SDK (for development)

## Installation

Download the latest `.lplug4` from the [Releases page](https://github.com/mrnil/OBSStudioForLogi/releases) and double-click it, or see [INSTALL.md](INSTALL.md). Launch OBS Studio and the plugin connects automatically.

To build from source instead, run `dotnet build src/OBSStudioForLogiPlugin.csproj -c Release`. The build links the plugin into Logi Plugin Service through a `.link` file.

## Available Controls

### OBS (Group 1)

- **Screenshot**: Capture screenshot to Pictures/Documents/Desktop folder
- **Reconnect to OBS**: Manually retry the connection to OBS; its colour shows the connection state
- **Toggle Studio Mode**: Enable/disable OBS studio mode for preview/program workflow
- **Studio Mode Transition**: Transition preview scene to program (only works when studio mode is enabled)
- **Connection Status Display**: Shows "Connected" (green), "Disconnected" (red), or "WebSocket Disabled" (orange)
- **Stats Summary**: Single button showing FPS, CPU%, and dropped frames (colour-coded)
- **Stats Folder**: Dynamic folder with individual tiles per stat (FPS, CPU, Memory, Render Missed, Encode Skipped, Total Dropped, Disk Space, Render Time)
- **Plugin Settings**: Configure the OBS connection (local/remote), stats polling interval, audio meter refresh rate and double-tap window

### Streaming (Group 2)

- **Toggle Streaming**: Toggle streaming on/off
- **Start Streaming** / **Stop Streaming**: Greyed out when they can't act (Start while streaming, Stop while not, either while OBS is starting or stopping the stream)
- **Stream Stats Folder**: Dynamic folder showing live stream statistics
  - Duration, Bytes Sent (MB/GB), Network Congestion, Skipped Frames, Total Frames
  - Colour-coded: green=healthy, yellow=warning, red=problem
  - Shows "Offline" when not streaming

### Recording (Group 3)

- **Toggle Recording**: Toggle recording on/off. A status key: red record dot while recording, green stop square while not
- **Start Recording** / **Stop Recording**: Greyed out when they can't act
- **Recording Pause**: Pause or resume the active recording; greyed out while nothing is recording

### Replay Buffer (Group 4)

- **Toggle Replay Buffer**: Start/stop replay buffer
- **Save Replay**: Save the current replay buffer to disk (green confirmation flash on success)

### Virtual Camera (Group 5)

- **Toggle Virtual Camera**: Toggle virtual camera on/off
- **Start Virtual Camera** / **Stop Virtual Camera**: Greyed out when they can't act

### Profiles (Group 6)

- **Available Profiles**: One button per profile, ticked when current
- **Profiles**: Dynamic folder with all available profiles
- **Current Profile Display**: Shows active profile name

### Scenes (Group 7)

- **Available Scenes** / **Available Collections**: One button per scene or scene collection, ticked when current
- **Scene Collections**: Dynamic folder with all available scene collections
- **Scenes**: Dynamic folder with all available scenes
  - When studio mode is disabled: Switches scene to program (live)
  - When studio mode is enabled: Switches scene to preview (not live yet). The buttons don't show the preview scene yet
- **Scene Sources**: Dynamic folder with the sources in the current scene (toggle visibility)
- **Current Scene** / **Current Collection**: Show the active scene and scene collection names

### Audio (Group 8)

- **Mixer for All Audio**: Dynamic folder with all audio inputs
  - Single tap selects the input for wheel/dial control; double tap toggles mute (the double-tap window is set in Plugin Settings)
  - Shows the input name, volume in dB and monitoring mode; green text = unmuted, red = muted, white border = selected
  - On a Loupedeck, the encoder adjusts the selected input's volume and pressing it cycles monitoring
  - Real-time updates when mute state, volume or monitoring changes in OBS
- **Mixer for Scene Audio**: The same, for the audio sources in the current scene plus audio inputs that aren't in any scene
- **Available Sources**: One mute button per audio input
- **Audio Select**: Dynamic folder for selecting the global audio source
  - Single tap selects/deselects an audio source
  - Displays mute state, volume, and monitoring mode for each source
  - Selection persists after folder is closed
  - Selected source is used by volume wheel/dial controls
- **Audio Volume**: Dynamic folder with adjustment tiles for MX big wheel
  - Tap a tile to arm the MX big wheel for that source's volume
  - Displays volume in dB and mute state (green/red)
  - Compatible with MX Creative Console Dialpad
- **Selected Source Volume**: Standalone adjustment for wheel/dial
  - Controls volume of the globally selected audio source
  - Drag onto MX big wheel, CT wheel, or any dial
  - Turn to adjust volume, press to reset to 100%
- **Live Audio Folder**: Dynamic folder with a live VU meter tile for each live audio input
  - Shows only inputs OBS reports as live (the live scene's audio sources plus global devices such as microphones); the folder updates automatically on scene switches and source changes
- **Audio Meter** (per source): one action per audio input in the Meters group, live or not, to put a meter for a specific source on any button
- Both meter types:
  - Per-channel bars on a dB scale (-60dB to 0dB), matching OBS's own meter
  - Colour zones: green below -20dB, yellow -20dB to -10dB, red at -10dB and above
  - A thin baseline marks a live-but-silent input
  - A red crossed-out speaker means the input is live but muted; a grey crossed-out speaker means it isn't live
  - Tap to toggle mute for that input
  - Refresh rate configurable in Plugin Settings (20/10/5 fps, default 10)

### User Defined Actions

These are in the `User Defined` sub-groups of Scenes and Audio; Media Action is under Media.

- **Switch to Scene (User Defined)**: Switch to a specific scene with optional profile and collection switching
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
- **Audio Source Status (User defined)**: Display-only widget showing mute state, volume, and monitoring mode
  - Configure audio source name via textbox
  - Shows real-time volume (dB), mute state (colour-coded), and monitoring type
  - Updates automatically when state changes in OBS

### Media (Group 9)

- **Media Controls**: Dynamic folder of media sources (ffmpeg, VLC, slideshow, text)
  - Single tap while stopped/paused: Play (uses Restart for stopped/ended sources)
  - Single tap while playing: Pause
  - Double tap: Stop (reset to beginning). With double-tap turned off in Plugin Settings, use Media Action (User Defined) to stop
  - Colour-coded: Green = Playing, Yellow = Paused, Grey = Stopped/Ended
  - Real-time updates when media starts/finishes naturally
- **Media Action (User Defined)**: Trigger media actions on a named source
  - Actions: Play, Pause, Stop, Restart, Next, Previous
  - Source name textbox + action listbox

## Configuration

The plugin automatically reads OBS WebSocket settings from:

- **Windows**: `%AppData%\obs-studio\plugin_config\obs-websocket\config.json`
- **macOS**: `~/Library/Application Support/obs-studio/plugin_config/obs-websocket/config.json`

For remote OBS connections or custom settings, use the **Plugin Settings** action to configure:

- **Use Local OBS** (checkbox): When checked, auto-discovers local OBS. When unchecked, uses manual settings.
- **IP Address**: Remote OBS machine IP
- **Port**: WebSocket port (default 4455)
- **Password**: WebSocket password. It is saved encrypted in the plugin's settings, not in the action. After pressing the action to save, clear this field: an empty field keeps the saved password, and a value left in the field is stored unencrypted in your device profile.
- **Clear Saved Password** (checkbox): Removes the saved password, for an OBS instance that doesn't use one.
- **Stats Polling Interval**: How often to refresh stats (2s, 5s, or 10s)
- **Audio Meter Refresh Rate**: How often the Live Audio Folder and Audio Meter buttons redraw (20, 10, or 5 fps)
- **Double-Tap Window**: How long the audio mixer and Media Controls folders wait for a second tap (200, 300 or 500ms, default 300ms), or Off so every tap acts at once

## Development

### Build

```bash
dotnet build src/OBSStudioForLogiPlugin.csproj
```

### Test

```bash
dotnet test tests/OBSStudioForLogiPlugin.Tests/OBSStudioForLogiPlugin.Tests.csproj
```

**Note:** Tests are run locally only. The GitHub Actions workflows check dependencies, validate the build and lint the Markdown, but don't run the tests yet.

### Clean

```bash
dotnet clean OBSStudioForLogiPlugin.sln
```

## Architecture

- **TDD Approach**: The services layer is developed test first; see [test coverage](docs/ai/test-coverage.md) for the current count and coverage
- **Dependency Injection**: Interface-based design for testability; OBS is mocked through `IOBSWebsocket`
- **Non-Blocking**: OBS writes run on a background runner, and buttons render from cached state, so a slow OBS never freezes the device
- **Event-Driven**: Real-time updates via OBS WebSocket events, dispatched to commands through `CommandCoordinator`
- **On-Demand Loading**: Data only one folder or button shows is loaded only while it is on screen
- **Resilient Reconnection**: `ConnectionManager` retries with exponential backoff (1s to 30s) and ±15% jitter, re-reading the connection settings each time
- **Command Base Classes**: `ToggleCommandBase` and `StartStopCommandBase` share the logic of the output buttons

See [docs/ai/structure.md](docs/ai/structure.md) for the full architecture.

## Troubleshooting

**Plugin doesn't connect:**

- Ensure OBS Studio is running
- Verify obs-websocket is enabled in OBS (Tools → WebSocket Server Settings)
- Check logs in Logi Plugin Service
- For remote OBS: verify IP address, port, and password in Plugin Settings

**Buttons greyed out or showing "Disconnected":**

- The plugin isn't connected to OBS yet
- Wait for the automatic connection, press Reconnect to OBS, or restart OBS Studio

## License

See LICENSE file for details.
