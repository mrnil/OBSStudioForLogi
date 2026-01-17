# Project Structure

## Directory Organization

```
OBSStudioForLogiPlugin/
├── src/                          # Main plugin source code
│   ├── Actions/                  # Command implementations for hardware controls
│   ├── Services/                 # Core business logic and OBS integration
│   ├── Helpers/                  # Utility classes (logging, resources)
│   ├── Models/                   # Data models and configuration structures
│   ├── Icons/                    # Embedded SVG/PNG resources for UI
│   ├── package/metadata/         # Plugin metadata and packaging
│   └── *.cs                      # Root plugin and application classes
├── tests/                        # Unit and integration tests
│   └── OBSStudioForLogiPlugin.Tests/
└── bin/                          # Build output (Debug/Release)
```

## Core Components

### Plugin Entry Points
- **OBSStudioForLogiPlugin.cs**: Main plugin class, singleton instance, manages lifecycle and coordinates all subsystems
- **OBSStudioForLogiApplication.cs**: Defines OBS application detection (process name: obs64.exe, bundle: com.obsproject.obs-studio)

### Actions Layer (`src/Actions/`)
Command classes that handle user interactions from Loupedeck hardware:

**Display Commands** (read-only status indicators):
- `ConnectionStatusDisplay.cs`: Shows connection status (Connected/Disconnected)
- `CurrentProfileDisplay.cs`: Shows active OBS profile name
- `CurrentSceneCollectionDisplay.cs`: Shows active scene collection name
- `CurrentSceneDisplay.cs`: Shows current active scene

**Interactive Commands** (user-triggered actions):
- `ProfileSelectCommand.cs`: Multi-state command for switching OBS profiles
- `ProfilesDynamicFolder.cs`: Dynamic folder containing all available profiles
- `SceneCollectionSelectCommand.cs`: Multi-state command for switching scene collections
- `ScenesDynamicFolder.cs`: Dynamic folder containing all available scenes as buttons
- `SourcesDynamicFolder.cs`: Dynamic folder showing sources in current scene with visibility toggle
- `RecordingToggleCommand.cs`: Toggle recording on/off
- `RecordingStartCommand.cs`: Start recording
- `RecordingStopCommand.cs`: Stop recording
- `RecordingPauseToggleCommand.cs`: Pause/resume recording
- `StreamingToggleCommand.cs`: Toggle streaming on/off
- `StreamingStartCommand.cs`: Start streaming
- `StreamingStopCommand.cs`: Stop streaming
- `VirtualCameraToggleCommand.cs`: Toggle virtual camera on/off
- `VirtualCameraStartCommand.cs`: Start virtual camera
- `VirtualCameraStopCommand.cs`: Stop virtual camera
- `ReconnectCommand.cs`: Manually retry connection to OBS
- `ScreenshotCommand.cs`: Capture screenshot via OBS

### Services Layer (`src/Services/`)
Core business logic and OBS integration:

- **OBSWebSocketManager.cs**: Primary WebSocket connection manager, handles connect/disconnect/reconnect with exponential backoff and jitter, event routing, timer-based continuous reconnection
- **OBSActionExecutor.cs**: Executes OBS commands (scene switching, recording control, streaming control, virtual camera, profile management, source visibility)
- **IOBSWebsocket.cs**: Interface abstraction for OBS WebSocket operations (enables testing/mocking)
- **OBSWebsocketAdapter.cs**: Adapter wrapping obs-websocket-dotnet library
- **OBSConfigReader.cs**: Reads OBS configuration files to discover WebSocket settings (port, password)
- **OBSLifecycleManager.cs**: Manages connection lifecycle, port availability checking

### Helpers Layer (`src/Helpers/`)
- **PluginLog.cs**: Centralized logging wrapper (implements IPluginLog)
- **IPluginLog.cs**: Logging interface for dependency injection
- **PluginResources.cs**: Embedded resource loader for icons and images

### Models Layer (`src/Models/`)
- **OBSConnectionSettings.cs**: Data model for WebSocket connection configuration (URL, port, password)

## Architectural Patterns

### Singleton Pattern
Most command classes use singleton instances accessed via static `Instance` property:
```csharp
public static SceneCollectionSelectCommand Instance { get; private set; }
```
This allows the main plugin to notify commands of state changes without maintaining explicit references.

### Event-Driven Architecture
- Plugin subscribes to Loupedeck ClientApplication events (ApplicationStarted, ApplicationStopped)
- OBSWebSocketManager raises events for OBS state changes (scene changes, profile changes, recording state)
- Commands subscribe to relevant events and update their UI state accordingly

### Adapter Pattern
`OBSWebsocketAdapter` wraps the third-party `obs-websocket-dotnet` library, providing:
- Abstraction layer for easier testing
- Consistent error handling
- Event translation to plugin-specific formats

### Command Pattern
Each action inherits from Loupedeck base classes:
- `PluginMultistateDynamicCommand`: Commands with multiple states (selected/unselected)
- `PluginDynamicCommand`: Simple action commands
- Commands implement `RunCommand(String actionParameter)` for execution

### Dependency Injection (Partial)
- Services use interface abstractions (IOBSWebsocket) for testability
- PluginLog uses IPluginLog interface
- Main plugin instantiates concrete implementations

## Component Relationships

```
OBSStudioForLogiPlugin (main)
    ├── OBSWebSocketManager (connection management)
    │   ├── OBSWebsocketAdapter (library wrapper)
    │   └── OBSActionExecutor (command execution)
    ├── OBSConfigReader (configuration)
    ├── OBSLifecycleManager (connection lifecycle)
    └── Commands (Actions/)
        ├── Display Commands (ConnectionStatus, CurrentProfile, CurrentScene, CurrentSceneCollection)
        ├── Profile Commands (ProfileSelect, ProfilesDynamicFolder)
        ├── Scene Commands (SceneCollectionSelect, ScenesDynamicFolder, SourcesDynamicFolder)
        ├── Recording Commands (Toggle, Start, Stop, Pause)
        ├── Streaming Commands (Toggle, Start, Stop)
        ├── Virtual Camera Commands (Toggle, Start, Stop)
        └── Utility Commands (Screenshot, Reconnect)
```

### Data Flow
1. **Startup**: Plugin loads → reads OBS config → waits for OBS process/port → connects to WebSocket
2. **User Action**: Hardware button press → Command.RunCommand() → Plugin method → OBSActionExecutor → WebSocket request
3. **OBS Event**: WebSocket event → OBSWebSocketManager → Plugin callback → Command.OnStateChanged() → UI update

## Build Configuration

### Project Structure
- **Target Framework**: .NET 8.0
- **Root Namespace**: Loupedeck.OBSStudioForLogiPlugin
- **Output**: Custom paths to Logi Plugin Service directories
- **Platform**: Cross-platform (Windows/macOS with conditional compilation)

### Build Targets
- **CopyPackage**: Copies metadata and package files to output
- **PostBuild**: Creates .link file in plugin directory, triggers hot-reload via loupedeck:// protocol
- **PluginClean**: Removes link files and output directories

### Dependencies
- **PluginApi.dll**: Loupedeck SDK (referenced from system installation)
- **obs-websocket-dotnet** (v5.0.1): NuGet package for OBS WebSocket communication
