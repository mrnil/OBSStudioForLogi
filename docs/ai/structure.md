# Project Structure

## Repository Layout

```
OBSStudioForLogiPlugin/
├── src/                          # Plugin source code
│   ├── Actions/                  # Loupedeck SDK command/folder classes (51 files)
│   ├── Helpers/                  # Utility classes (17 files)
│   ├── Models/                   # Data models (7 files)
│   ├── Services/                 # Business logic and OBS integration (21 files)
│   ├── Resources/icons/          # SVG button icons (40 files; rules in docs/ai/icon-style.md)
│   ├── package/metadata/         # LoupedeckPackage.yaml + plugin icon
│   ├── OBSStudioForLogiPlugin.cs # Main plugin class (orchestration)
│   ├── OBSStudioForLogiApplication.cs
│   └── OBSStudioForLogiPlugin.csproj
├── tests/
│   └── OBSStudioForLogiPlugin.Tests/
│       ├── Actions/              # Action-layer integration tests (22 files)
│       └── *.cs                  # Services-layer unit tests (43 files)
├── docs/ai/                      # AI coding rules and architecture docs (Claude Code, etc.)
├── .github/workflows/            # CI: dependency-check.yml
├── bin/                          # Build output (Debug/Release)
├── ci/                           # CI-only PluginApi.dll stub
├── tools/icons/                  # Icon generator, csproj sync and reference sheet (Python)
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
| `OBSWebsocketAdapter.cs` | Thin wrapper over obs-websocket-dotnet library; `IsConnected` requires `IsIdentified`; parses `GetStats` null-tolerantly |
| `IOBSWebsocket.cs` | Interface for testability (mocked in all tests) |
| `OBSFacade.cs` | Simplified public interface over OBSWebSocketManager |
| `ConnectionManager.cs` | Connection lifecycle: config discovery, connect/disconnect, retrying every `ConnectRetryDelay` while OBS isn't running; `ReconnectAsync` ignores manual reconnects while connected or mid-attempt |
| `CommandCoordinator.cs` | Owns event dispatch: per-command exception isolation via generic `NotifyEach<T>()` |
| `CommandRegistry.cs` | Command store: registration/dedup + generic `GetCommands<T>()` interface filter |
| `IObsCommand.cs` | 15 notification interfaces (IObsCommand + 14 specialised) |
| `OBSConfigReader.cs` | Reads OBS WebSocket config from disk |
| `OBSLifecycleManager.cs` | Port availability checking |
| `PluginConfigReader.cs` | Read/write plugin config JSON; keeps the remote password out of the file, in an `ISecretStore`, and moves plaintext passwords left by older versions into it |
| `ISecretStore.cs` | Encrypted value storage; the plugin backs it with the SDK's plugin settings (`PluginSettingsSecretStore`, nested in the plugin class) |
| `ReconnectionStrategy.cs` | Exponential backoff with jitter |
| `StatsService.cs` | Timer-based stats polling; skips a tick while the previous poll is still running. Polls only while connected and while at least one viewer (a stats folder, the summary button) is registered, with an immediate poll for the first viewer |
| `AudioMeterService.cs` | Latest per-input audio meter levels fed by `InputVolumeMeters` (expire after `OBSTimings.AudioMeterStaleThreshold`), the live-input list, and a cached per-input mute state |
| `AudioStateCache.cs` | Non-blocking per-input mute/volume/monitor type for button rendering: fetches a miss once in the background, kept current by OBS change events |
| `KeyedStateCache.cs` | The same non-blocking pattern for one value per key; `OBSWebSocketManager.SourceVisibility` uses it keyed by (scene, source), `OBSWebSocketManager.MediaState` keyed by input name |
| `SceneSourcesLoader.cs` | Loads a scene's source and audio source lists in the background for `OBSFacade.UpdateSourcesForScene`, so a scene change doesn't hold up the OBS event thread; only the latest load is delivered, and none after a disconnect. Loads nothing unless a scene source folder has registered as a viewer |
| `InitialStateLoader.cs` | Runs the once-per-connection initial state load, retrying every `OBSTimings.InitialStateRetryDelay` while OBS answers "not ready" (207) during its startup, until the connection changes. `OBSWebSocketManager` raises `InitialStateLoadFinished` afterwards, which starts stats polling |
| `CachedValue.cs` | One OBS-derived value fetched on first use and kept until `Invalidate`; failed fetches and fetches overtaken by an invalidation aren't kept. `OBSWebSocketManager.AudioInputMembership` uses it for the audio input lists behind the scene audio folder |

### `src/Actions/` — Loupedeck SDK Commands (SDK-dependent, exempt from strict TDD)

**Base classes:**

- `ToggleCommandBase` — shared logic for all toggle commands
- `StartStopCommandBase` — shared logic for start/stop command pairs
- `AudioInputDynamicFolderBase` — shared audio folder logic (mute, volume, selection, encoder)

**Dynamic Folders (PluginDynamicFolder):**

- `ScenesDynamicFolder`, `SourcesDynamicFolder`, `ProfilesDynamicFolder`, `SceneCollectionsDynamicFolder` (added v1.6.0). `SourcesDynamicFolder` and `SceneAudioSourcesDynamicFolder` register as scene source viewers in `Activate()`/`Deactivate()`, so their lists load only while open, and show `LoadingTiles` until the first list arrives
- `AudioMixerDynamicFolder`, `SceneAudioSourcesDynamicFolder`
- `AudioSelectDynamicFolder`, `AudioVolumeDynamicFolder`
- `AudioMetersDynamicFolder` ("Live Audio Folder") — real-time VU meters; subscribes to `InputVolumeMeters` in `Activate()` and unsubscribes in `Deactivate()`; its button list is the live-input list from `AudioMeterService`, re-checked on every refresh tick
- `MediaDynamicFolder`
- `StatsDynamicFolder`, `StreamStatsDynamicFolder` — register as stats viewers in `Activate()`/`Deactivate()`

**Toggle Commands (PluginDynamicCommand via ToggleCommandBase):**

- `StreamingToggleCommand`, `RecordingToggleCommand`, `VirtualCameraToggleCommand`
- `ReplayBufferToggleCommand`, `StudioModeToggleCommand`

**Start/Stop Commands (via StartStopCommandBase):**

- `StreamingStartCommand`, `StreamingStopCommand`
- `RecordingStartCommand`, `RecordingStopCommand`, `RecordingPauseToggleCommand`
- `VirtualCameraStartCommand`, `VirtualCameraStopCommand`

**Multi-State Select Commands:**

- `ProfileSelectCommand`, `SceneSelectCommand` (added v1.6.0), `SceneCollectionSelectCommand`, `AudioSourceSelectCommand` (added v1.6.0)

**Parameterised Display Commands:**

- `AudioMeterCommand` — one VU meter parameter per audio input; subscribes to `InputVolumeMeters` through an `ActivityLease` renewed by image requests, since plain commands get no `Activate()`/`Deactivate()`

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
- `ReconnectCommand`, `ReplayBufferSaveCommand`, `StudioModeTransitionCommand`, `ScreenshotCommand`

`StatsDisplay` registers as a stats viewer through an `ActivityLease` renewed by image requests, like `AudioMeterCommand`.

Note: as of v1.6.0 the `99. User Defined Actions` group has been retired — all configurable (`ActionEditorCommand`) actions now live in sub-groups alongside their related controls (e.g. `8. Audio › User Defined`, `7. Scenes › User Defined`).

### `src/Helpers/`

| File | Purpose |
|------|---------|
| `ButtonImageHelper.cs` | Icon rendering: `Icon` (embedded SVG) and `IconWithBackground` (SVG on a solid colour) |
| `ButtonTextRenderer.cs` | `BitmapBuilder`-based text rendering: `RenderText`, `RenderTextWithBorder`, `RenderTextWithIcon`, with font size fitted to the text |
| `AudioHelpers.cs` | Shared audio button image rendering |
| `AudioSelectionState.cs` | Static singleton: global selected audio source for wheel/dial |
| `VolumeConverter.cs` | volumeMul ↔ dB conversion and formatting |
| `VuMeterRenderer.cs` | VU meter tile state (inactive/muted/meter), bar rendering, dB scaling and colour zones |
| `ActivityLease.cs` | Touch-renewed lease that lapses when idle; infers button visibility for `AudioMeterCommand` and `StatsDisplay` |
| `LoadingTiles.cs` | "Loading …" placeholder a folder shows while its list loads, drawn as one message across the two buttons right of the folder's Back button |
| `SessionGate.cs` | Opens once per OBS connection so the initial state load ignores repeated `Connected` events (ReIdentify confirmations); its `Generation` lets work started for one connection tell when that connection has gone |
| `PressTimingHelper.cs` | DoubleTapHelper: 500ms window single/double tap detection |
| `OBSTimings.cs` | Centralised timing constants (delays, test timeouts) |
| `MediaInputStates.cs` | OBS media state/action names, the state each action leaves an input in, and the media button's single-tap action |
| `PluginLog.cs` | Static logging facade with configurable level; throttles repeated warnings/errors |
| `LogThrottle.cs` | Writes an identical message at most once per window and counts the suppressed repeats |
| `IPluginLog.cs` | Interface for injectable logging in services |
| `PluginResources.cs` | Embedded resource access helper |
| `LogLevel.cs` | Log level enum |

### `src/Models/`

| File | Contents |
|------|---------|
| `AudioMeterLevels.cs` | Plugin-owned per-channel meter levels, decoupled from the library's `InputVolumeMeter` type |
| `AudioInputSceneMembership.cs` | The audio input names and the audio inputs not in any scene, cached together for the scene audio folder |
| `OBSConnectionSettings.cs` | IP, port, password — with localhost validation |
| `OBSStats.cs` | Stats model with derived properties (FPS, CPU%, render lag %) |
| `OBSStreamStats.cs` | Stream stats model (duration, bytes, congestion, frames) |
| `PluginConfig.cs` | Persisted plugin config (UseLocalObs, RemoteIP, Port, Password, StatsPollingInterval, AudioMeterRefreshInterval, LogLevel) |

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

`CommandCoordinator` dispatches events via interface type-filtering, with per-command exception isolation so one throwing command doesn't block the rest:

```csharp
this.NotifyEach<ISceneAwareCommand>(nameof(ISceneAwareCommand.OnSceneChanged), c => c.OnSceneChanged(sceneName));
// NotifyEach iterates this._registry.GetCommands<T>(), try/catching each call individually
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
    → CommandCoordinator.NotifyXxx() — filters via CommandRegistry.GetCommands<T>(), dispatches per-command with exception isolation
    → Command calls CommandImageChanged(parameter)
    → Loupedeck framework calls GetCommandImage()
    → Button icon updates
```

## Architectural Inconsistency (Fixed in v1.6.0)

Previously, `OBSWebSocketManager.OnConnected`/`OnDisconnected` bypassed the `CommandRegistry` via hardcoded singleton calls, and `OnCurrentSceneChanged` bypassed it for `SourcesDynamicFolder`/`SceneAudioSourcesDynamicFolder`. Both are fixed: `OBSStudioForLogiPlugin.OnOBSConnected`/`OnOBSDisconnected` now route through `CommandCoordinator.NotifyConnected()`/`NotifyDisconnected()`, and scene-source updates route through the registry via `ISceneSourcesAwareCommand`. New self-registering commands now correctly receive all lifecycle notifications with no extra wiring. See `assessment.md` items #1 and #2.

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
