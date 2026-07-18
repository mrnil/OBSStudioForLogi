# Project Structure

## Repository Layout

```
OBSStudioForLogiPlugin/
├── src/                          # Plugin source code
│   ├── Actions/                  # Loupedeck SDK command/folder classes (44 files)
│   ├── Helpers/                  # Utility classes (11 files)
│   ├── Models/                   # Data models (4 files)
│   ├── Services/                 # Business logic and OBS integration (14 files)
│   ├── Resources/icons/          # Embedded SVG/PNG icons (50 files)
│   ├── package/metadata/         # LoupedeckPackage.yaml + plugin icon
│   ├── OBSStudioForLogiPlugin.cs # Main plugin class (orchestration)
│   ├── OBSStudioForLogiApplication.cs
│   └── OBSStudioForLogiPlugin.csproj
├── tests/
│   └── OBSStudioForLogiPlugin.Tests/
│       ├── Actions/              # Action-layer integration tests (16 files)
│       └── *.cs                  # Services-layer unit tests (25 files)
├── tools/
│   └── InspectSdk/               # SDK inspection utility
├── .amazonq/rules/               # AI coding rules and memory bank
├── .github/workflows/            # CI: dependency-check.yml
├── bin/                          # Build output (Debug/Release)
├── ci/                           # CI-only PluginApi.dll stub
├── OBSStudioForLogiPlugin.sln
├── CHANGELOG.md
├── README.md
├── USER_MANUAL.md
└── TODO.md
```

## Source Layer Breakdown

### `src/Services/` — Business Logic (testable, no SDK dependency)

| File | Responsibility |
|------|---------------|
| `OBSWebSocketManager.cs` | WebSocket lifecycle, event subscription, reconnection timer |
| `OBSActionExecutor.cs` | All OBS operations with state tracking and error handling |
| `OBSWebsocketAdapter.cs` | Thin wrapper over obs-websocket-dotnet library |
| `IOBSWebsocket.cs` | Interface for testability (mocked in all tests) |
| `OBSFacade.cs` | Simplified public interface over OBSWebSocketManager |
| `ConnectionManager.cs` | Connection lifecycle: config discovery, connect/disconnect |
| `CommandCoordinator.cs` | Pass-through to CommandRegistry (thin delegation layer) |
| `CommandRegistry.cs` | Interface-based event dispatch to registered commands |
| `IObsCommand.cs` | 14 notification interfaces (IObsCommand + 13 specialised) |
| `OBSConfigReader.cs` | Reads OBS WebSocket config from disk |
| `OBSLifecycleManager.cs` | Port availability checking |
| `PluginConfigReader.cs` | Read/write plugin config JSON |
| `ReconnectionStrategy.cs` | Exponential backoff with jitter |
| `StatsService.cs` | Timer-based stats polling |

### `src/Actions/` — Loupedeck SDK Commands (SDK-dependent, exempt from strict TDD)

**Base classes:**
- `ToggleCommandBase` — shared logic for all toggle commands
- `StartStopCommandBase` — shared logic for start/stop command pairs
- `AudioInputDynamicFolderBase` — shared audio folder logic (mute, volume, selection, encoder)

**Dynamic Folders (PluginDynamicFolder):**
- `ScenesDynamicFolder`, `SourcesDynamicFolder`, `ProfilesDynamicFolder`
- `AudioMixerDynamicFolder`, `SceneAudioSourcesDynamicFolder`
- `AudioSelectDynamicFolder`, `AudioVolumeDynamicFolder`
- `MediaDynamicFolder`
- `StatsDynamicFolder`, `StreamStatsDynamicFolder`

**Toggle Commands (PluginDynamicCommand via ToggleCommandBase):**
- `StreamingToggleCommand`, `RecordingToggleCommand`, `VirtualCameraToggleCommand`
- `ReplayBufferToggleCommand`, `StudioModeToggleCommand`

**Start/Stop Commands (via StartStopCommandBase):**
- `StreamingStartCommand`, `StreamingStopCommand`
- `RecordingStartCommand`, `RecordingStopCommand`
- `VirtualCameraStartCommand`, `VirtualCameraStopCommand`

**User-Defined (ActionEditorCommand):**
- `SceneSwitchAdjustableCommand`, `SourceVisibilityAdjustableCommand`
- `AudioMuteAdjustableCommand`, `AudioMonitoringCycleAdjustableCommand`
- `AudioSelectAdjustableCommand`, `MediaActionCommand`
- `PluginSettingsCommand`

**Adjustments (PluginDynamicAdjustment):**
- `SelectedSourceVolumeAdjustment`, `AudioVolumeWheelTool`

**Display Commands:**
- `ConnectionStatusDisplay`, `CurrentSceneDisplay`, `CurrentProfileDisplay`
- `CurrentSceneCollectionDisplay`, `StatsDisplay`, `AudioStatusDisplayCommand`

### `src/Helpers/`

| File | Purpose |
|------|---------|
| `ButtonImageHelper.cs` | Static factory: Icon, StateIcon, Text, StateText, TextWithIcon, StateTextWithIcon |
| `ButtonTextRenderer.cs` | BitmapBuilder-based text rendering with border support |
| `AudioHelpers.cs` | Shared audio button image rendering |
| `AudioSelectionState.cs` | Static singleton: global selected audio source for wheel/dial |
| `VolumeConverter.cs` | volumeMul ↔ dB conversion and formatting |
| `PressTimingHelper.cs` | DoubleTapHelper: 500ms window single/double tap detection |
| `OBSTimings.cs` | Centralised timing constants (delays, test timeouts) |
| `PluginLog.cs` | Static logging facade with configurable level |
| `IPluginLog.cs` | Interface for injectable logging in services |
| `PluginResources.cs` | Embedded resource access helper |
| `LogLevel.cs` | Log level enum |

### `src/Models/`

| File | Contents |
|------|---------|
| `OBSConnectionSettings.cs` | IP, port, password — with localhost validation |
| `OBSStats.cs` | Stats model with derived properties (FPS, CPU%, render lag %) |
| `OBSStreamStats.cs` | Stream stats model (duration, bytes, congestion, frames) |
| `PluginConfig.cs` | Persisted plugin config (UseLocalObs, RemoteIP, Port, Password, StatsPollingInterval, LogLevel) |

## Architectural Patterns

### 1. Layered Architecture

```
Loupedeck SDK (Actions layer)
        ↓ calls
OBSStudioForLogiPlugin (orchestration)
        ↓ delegates to
OBSFacade → OBSWebSocketManager → OBSActionExecutor → IOBSWebsocket
                                                              ↓
                                                   OBSWebsocketAdapter → obs-websocket-dotnet
```

### 2. Command Registry / Self-Registration Pattern

Commands register themselves in their constructor:
```csharp
public ScenesDynamicFolder()
{
    Instance = this;
    OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
}
```
`CommandRegistry` dispatches events via interface type-filtering:
```csharp
foreach (var command in this._commands.OfType<ISceneAwareCommand>())
    command.OnSceneChanged(sceneName);
```

### 3. Notification Interface Hierarchy

```
IObsCommand (OnConnected, OnDisconnected)
    ├── ISceneAwareCommand          (OnSceneChanged)
    ├── IScenesListAwareCommand     (OnScenesChanged)
    ├── IProfileAwareCommand        (OnProfileChanged)
    ├── IProfilesListAwareCommand   (OnProfilesChanged)
    ├── ISceneCollectionAwareCommand(OnSceneCollectionChanged)
    ├── ISourceVisibilityAwareCommand(OnSourceVisibilityChanged)
    ├── IInputMuteAwareCommand      (OnInputMuteChanged)
    ├── IInputVolumeAwareCommand    (OnInputVolumeChanged)
    ├── IInputsListAwareCommand     (OnInputsChanged)
    ├── IVirtualCameraAwareCommand  (OnVirtualCameraStateChanged)
    ├── IReplayBufferAwareCommand   (OnReplayBufferStateChanged)
    ├── IReplayBufferSavedAwareCommand(OnReplayBufferSaved)
    ├── IStudioModeAwareCommand     (OnStudioModeStateChanged)
    └── IInputMonitorAwareCommand   (OnInputMonitorTypeChanged)
```

### 4. Singleton Instance Pattern

Every command exposes a static `Instance` property set in its constructor. This enables direct access from `OBSWebSocketManager` and `OBSStudioForLogiPlugin` for cases not yet routed through the registry.

### 5. Facade Pattern

`OBSFacade` provides a single, null-safe access point to all OBS state and actions, hiding the `OBSWebSocketManager`/`OBSActionExecutor` chain from the main plugin class.

### 6. Event Flow

```
OBS fires event
    → obs-websocket-dotnet raises C# event
    → OBSWebSocketManager handler
    → OBSStudioForLogiPlugin.OnXxx() callback
    → CommandCoordinator.NotifyXxx()
    → CommandRegistry dispatches to registered commands via interface
    → Command calls CommandImageChanged(parameter)
    → Loupedeck framework calls GetCommandImage()
    → Button icon updates
```

## Known Architectural Inconsistency

`OBSWebSocketManager.OnConnected` and `OnDisconnected` contain ~10 hardcoded singleton calls each that bypass the `CommandRegistry`. The `CommandCoordinator.NotifyConnected()` / `NotifyDisconnected()` methods exist but are never called from `OBSStudioForLogiPlugin.OnOBSConnected` / `OnOBSDisconnected`. This means new commands that self-register will not receive `OnConnected`/`OnDisconnected` unless also added as hardcoded calls in `OBSWebSocketManager`.

## Build Output

```
bin/
├── Debug/bin/     ← DLL + all dependencies (hot-reload via .link file)
├── Debug/metadata/
├── Release/bin/
└── Release/metadata/
```

Post-build: writes a `.link` file to `%LocalAppData%\Logi\LogiPluginService\Plugins\` and triggers `loupedeck:plugin/OBSStudioForLogi/reload`.

## Package Format

`.lplug4` — created with `LogiPluginTool pack`. Verified with `LogiPluginTool verify`. Metadata defined in `src/package/metadata/LoupedeckPackage.yaml`.
