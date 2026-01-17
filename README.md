# OBS Studio Plugin for Loupedeck/Logitech Devices

Control OBS Studio directly from your Loupedeck/Logitech hardware device with dedicated buttons and real-time status displays.

## Features

- **Automatic Connection**: Discovers OBS WebSocket settings automatically from OBS configuration
- **Streaming Controls**: Start, stop, toggle streaming with visual state indicators
- **Recording Controls**: Start, stop, toggle, pause/resume recording with visual state indicators
- **Virtual Camera**: Start, stop, toggle virtual camera output
- **Scene Management**: Switch scenes with dynamic folder and visual feedback
- **Source Visibility**: Toggle visibility of sources in current scene
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
- **Virtual Camera Toggle**: Toggle virtual camera on/off
- **Virtual Camera Start**: Start virtual camera
- **Virtual Camera Stop**: Stop virtual camera
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

### Profiles (Group 4)
- **Profile Select**: Switch between OBS profiles (multi-state buttons)
- **Profiles Folder**: Dynamic folder with all available profiles
- **Current Profile Display**: Shows active profile name

### Scenes (Group 5)
- **Scene Collection Select**: Switch between scene collections (multi-state buttons)
- **Scenes Folder**: Dynamic folder with all available scenes
- **Sources Folder**: Dynamic folder with sources in current scene (toggle visibility)
- **Current Scene Display**: Shows active scene name
- **Current Scene Collection Display**: Shows active scene collection name

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

### Clean
```bash
dotnet clean OBSStudioForLogiPlugin.sln
```

## Architecture

- **TDD Approach**: Comprehensive test coverage with 80 unit tests
- **Dependency Injection**: Interface-based design for testability
- **Async Operations**: All OBS operations wrapped in Task.Run to prevent UI freezing
- **Event-Driven**: Real-time updates via OBS WebSocket events
- **Singleton Pattern**: Command instances accessible via static Instance properties
- **Resilient Reconnection**: Timer-based reconnection with exponential backoff (1s to 30s) and jitter (0.85-1.15x)

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
