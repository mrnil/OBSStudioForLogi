# Test Coverage & Architecture

## Overview

The project follows a TDD approach with 241 unit tests using xUnit + Moq. Overall line coverage is ~27% (Cobertura), but this is misleading due to the architecture — the testable business logic layer has much higher coverage while Loupedeck SDK-dependent code is exempt.

## Test Count: 241

## Test Files

### Services Layer (High Coverage Target: 80%+)

| Test File | Covers | Tests |
|-----------|--------|-------|
| `OBSActionExecutorTests.cs` | Core executor: profiles, scenes, recording, streaming, mute, screenshots, error handling | ~50 |
| `OBSActionExecutorReplayBufferTests.cs` | Replay buffer: toggle, start, stop, save, state tracking | 17 |
| `OBSActionExecutorAudioTests.cs` | Audio: volume get/set, monitor type cycling, input kind, scenes for input | 28 |
| `OBSActionExecutorSceneSwitchingTests.cs` | Scene switching with studio mode behavior | ~6 |
| `OBSActionExecutorStudioModeTests.cs` | Studio mode toggle, state management | ~8 |
| `OBSActionExecutorStudioModeTransitionTests.cs` | Studio mode transition command | ~6 |
| `OBSWebSocketManagerTests.cs` | Manager lifecycle, disposal | ~4 |
| `OBSWebSocketManagerStateTests.cs` | State properties delegation | ~6 |
| `OBSWebSocketManagerReconnectionTests.cs` | Reconnection with exponential backoff | ~4 |
| `OBSWebSocketManagerLoggingTests.cs` | Log output verification | ~3 |
| `OBSConfigReaderTests.cs` | Config file parsing, validation | ~7 |
| `OBSConnectionSettingsTests.cs` | Connection settings model, localhost validation | ~5 |
| `OBSLifecycleManagerTests.cs` | Port checking, wait logic | ~3 |
| `OBSFacadeTests.cs` | Facade disconnected state, safe defaults, connection validation | 36 |
| `CommandRegistryTests.cs` | Registration, interface-based dispatch, deduplication | 19 |
| `SourceVisibilityTests.cs` | Source visibility toggle and query | ~5 |
| `VirtualCameraCommandTests.cs` | Virtual camera state and toggle | ~5 |
| `ManualReconnectTests.cs` | Manual reconnect trigger | ~1 |

### Actions Layer (Integration Tests for Critical Paths)

| Test File | Covers | Tests |
|-----------|--------|-------|
| `Actions/ProfileSelectCommandTests.cs` | Constructor, singleton pattern | ~1 |
| `Actions/RecordingCommandTests.cs` | Start/Stop/Pause command construction | ~3 |
| `Actions/RecordingToggleCommandTests.cs` | Toggle command properties | ~1 |
| `Actions/SceneCollectionSelectCommandTests.cs` | Constructor, singleton | ~1 |
| `Actions/ScenesDynamicFolderTests.cs` | Constructor, instance property | ~2 |
| `Actions/SceneSwitchAdjustableCommandTests.cs` | Constructor, interface methods | ~6 |
| `Actions/ScreenshotCommandTests.cs` | Constructor, properties | ~1 |
| `Actions/StatusDisplayCommandTests.cs` | Display command construction | ~3 |

## Coverage by Class (Key Classes)

| Class | Line Coverage | Branch Coverage | Notes |
|-------|-------------|-----------------|-------|
| **OBSActionExecutor** | 63% | 54% | Core business logic, well-tested |
| **OBSConfigReader** | 91% | 90% | Excellent coverage |
| **OBSLifecycleManager** | 79-89% | 75% | Good coverage |
| **OBSConnectionSettings** | 100% | 100% | Perfect |
| **OBSWebSocketManager** | 34% | 17% | Event handlers hard to unit test |
| **CommandRegistry** | 0%→covered | - | Tests exist but coverage tool shows 0 due to execution path |
| **OBSFacade** | 0%→covered | - | Tests verify behavior when disconnected |

## Why Some Classes Show 0% Despite Tests

### Loupedeck SDK Dependency (Exempt per TDD rules)

All `Actions/` classes inherit from SDK base classes (`PluginDynamicCommand`, `PluginMultistateDynamicCommand`, `PluginDynamicFolder`, `ActionEditorCommand`). These:

- Require the Loupedeck runtime to instantiate properly
- Call `OBSStudioForLogiPlugin.Instance?.RegisterCommand(this)` in constructors
- Use `BitmapBuilder`, `EmbeddedResources`, `PluginImageSize` for rendering
- Cannot be meaningfully unit tested without mocking the entire framework

### Static Logging in OBSFacade

`OBSFacade` logs via the static `PluginLog.Warning()` helper rather than the injected `IPluginLog`. This means log-verification tests can't capture its output through the mock. Tests verify behavior (no-throw, safe defaults) instead.

### OBSWebsocketAdapter (Pass-Through)

`OBSWebsocketAdapter` is a thin wrapper delegating to `obs-websocket-dotnet`. It's tested indirectly through `OBSActionExecutor` tests which mock `IOBSWebsocket`.

### Coverage Tool Quirks

`CommandRegistry` shows 0% in Cobertura despite having 19 tests because the coverage was measured before those tests were added. Re-running coverage will show the actual numbers.

## Running Tests

```bash
# Run all tests
dotnet test tests/OBSStudioForLogiPlugin.Tests/OBSStudioForLogiPlugin.Tests.csproj

# Run with coverage
dotnet test tests/OBSStudioForLogiPlugin.Tests/OBSStudioForLogiPlugin.Tests.csproj --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~OBSActionExecutorAudioTests"
```

## Test Patterns Used

### Arrange-Act-Assert with Moq

```csharp
[Fact]
public void GetInputVolume_WhenConnected_ReturnsVolume()
{
    // Arrange
    this._mockObs.Setup(x => x.IsConnected).Returns(true);
    this._mockObs.Setup(x => x.GetInputVolume("Microphone")).Returns(0.75f);

    // Act
    var result = this._executor.GetInputVolume("Microphone");

    // Assert
    Assert.Equal(0.75f, result);
}
```

### Async Fire-and-Forget Testing

```csharp
[Fact]
public void SetInputVolume_WhenConnected_CallsObs()
{
    this._mockObs.Setup(x => x.IsConnected).Returns(true);

    this._executor.SetInputVolume("Microphone", 0.5f);

    System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay); // 500ms
    this._mockObs.Verify(x => x.SetInputVolume("Microphone", 0.5f), Times.Once);
}
```

### Error Path Verification

```csharp
[Fact]
public void GetInputVolume_WhenOBSThrows_LogsErrorAndReturnsDefault()
{
    this._mockObs.Setup(x => x.IsConnected).Returns(true);
    this._mockObs.Setup(x => x.GetInputVolume(It.IsAny<String>())).Throws(new Exception("OBS error"));

    var result = this._executor.GetInputVolume("Microphone");

    Assert.Equal(1.0f, result);
    this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("Microphone"))), Times.Once);
}
```

## Known Gaps & Future Work

### Should Add Tests For

1. **OBSWebSocketManager event handlers** — `OnStreamStateChanged`, `OnRecordStateChanged`, `OnVirtualCameraStateChanged`, `OnReplayBufferStateChanged`, `OnCurrentSceneChanged`, `OnInputMuteStateChanged`, `OnInputVolumeChanged` contain real dispatch logic
2. **Reconnection timer logic** — `OnReconnectTimer` has complex branching (partially covered at 75%)

### Intentionally Not Tested

1. **All Actions/ command classes** — SDK-dependent, exempt per TDD rules
2. **ButtonImageHelper / ButtonTextRenderer** — Rendering code requiring SDK
3. **OBSWebsocketAdapter** — Pass-through wrapper
4. **OBSStudioForLogiPlugin main class** — Orchestration requiring full plugin runtime
5. **ConnectionManager** — Thin async delegation, hard to test without real WebSocket

## Test Timing Constants

Defined in `src/Helpers/OBSTimings.cs`:

- `TestAsyncDelay = 500ms` — Standard wait for Task.Run fire-and-forget
- `TestAsyncDelayExtended = 750ms` — Extended wait for slower operations

These are set conservatively for CI environments. Local execution could use shorter delays.
