# Test Coverage & Architecture

## Overview

The project follows a TDD approach with 705 unit tests using xUnit + Moq (verified 2026-09-29, net10.0, all passing). Overall line coverage is ~37.4% (Cobertura, last measured pre-CommandCoordinator-refactor), branch coverage ~19.8%. The headline number is lower than expected because the Loupedeck SDK-dependent Action/Command classes (which are exempt from TDD) drag down the average — the testable services layer has much higher coverage. Coverage has drifted down slightly since v1.5.1 (was 39.5%/22.6%) as v1.6.0 added several new Actions-layer commands (`SceneSelectCommand`, `AudioSourceSelectCommand`, `SceneCollectionsDynamicFolder`) faster than services-layer surface area grew — expected under the TDD-exemption policy, not a regression.

## Test Count: 705

## Test Files

### Services Layer (High Coverage Target: 80%+)

| Test File | Covers | Tests |
|-----------|--------|-------|
| `OBSActionExecutorTests.cs` | Core executor: profiles, scenes, recording, streaming, mute, screenshots, error handling | ~50 |
| `OBSActionExecutorReplayBufferTests.cs` | Replay buffer: toggle, start, stop, save, state tracking | 17 |
| `OBSActionExecutorAudioTests.cs` | Audio: volume get/set, monitor type cycling, `TryGetInputAudioState`, `TryGetAudioInputSceneMembership` | 24 |
| `OBSActionExecutorSceneSwitchingTests.cs` | Scene switching with studio mode behavior | ~6 |
| `OBSActionExecutorStudioModeTests.cs` | Studio mode toggle, state management | ~8 |
| `OBSActionExecutorStudioModeTransitionTests.cs` | Studio mode transition command | ~6 |
| `OBSWebSocketManagerTests.cs` | Manager lifecycle, disposal | ~4 |
| `OBSWebSocketManagerStateTests.cs` | State properties delegation | ~6 |
| `OBSWebSocketManagerReconnectionTests.cs` | Reconnection with exponential backoff; request timeout set and kept across `ConnectAsync`; failed start clears the connecting flag | 7 |
| `OBSWebSocketManagerLoggingTests.cs` | Log output verification | ~3 |
| `OBSWebSocketManagerEventDispatchTests.cs` | State propagation through Actions executor | 18 |
| `ReconnectionStrategyTests.cs` | Backoff delays, attempt counting, error handling | 15 |
| `OBSConfigReaderTests.cs` | Config file parsing, validation, IsServerDisabled | 10 |
| `OBSConnectionSettingsTests.cs` | Connection settings model, localhost validation | ~5 |
| `OBSLifecycleManagerTests.cs` | Port checking, wait logic | ~3 |
| `OBSFacadeTests.cs` | Facade disconnected state, safe defaults, connection validation, cache-backed audio, source visibility and media state getters, scene source updates on the background runner and only while a scene source folder is open, scene audio list order | 68 |
| `SceneSourcesLoaderTests.cs` | Background scene source loads: nothing fetched on the calling thread, only the latest load delivered, results dropped after a disconnect, fetch and callback exceptions contained, nothing loaded without a viewer (#25) | 20 |
| `CachedValueTests.cs` | Fetch once until invalidated, failed fetches not cached, a fetch invalidated mid-flight not kept (#25) | 8 |
| `RenameHandlingTests.cs` | Scene and input renames (#21): `KeyedStateCache.RenameKeys`, `AudioStateCache.Rename`, `AudioMeterService.RenameInput` (newer state at the new name wins, in-flight fetches dropped, live order kept), `AudioSelectionState.RenameIfMatches`, and `OBSWebSocketManager.ApplyInputRename`/`ApplySceneRename` | 27 |
| `CommandRegistryTests.cs` | Registration, deduplication, generic `GetCommands<T>()` filtering | 6 |
| `CommandCoordinatorTests.cs` | Dispatch-by-interface for every notification type, per-command exception isolation | 24 |
| `AudioStateCacheTests.cs` | Non-blocking misses, single in-flight fetch, event-vs-fetch precedence, failure backoff, invalidation | 14 |
| `KeyedStateCacheTests.cs` | Same as `AudioStateCacheTests` for the single-value cache, plus `TryFetch` failures and `RemoveWhere` | 19 |
| `SessionGateTests.cs` | Once-per-connection gate, including concurrent opens; `Generation`/`IsCurrent` tell an old connection from a new one | 8 |
| `InitialStateLoaderTests.cs` | Initial state load retried while OBS answers "not ready" (207), given up after the last attempt, not retried for other errors, abandoned when the connection goes (#26) | 10 |
| `ConnectionManagerTests.cs` | `IsConnecting` during a port wait; `ReconnectAsync` ignores presses mid-attempt; retries while the port never comes up, stopped by `Disconnect`/`Dispose` | 8 |
| `StatsServiceTests.cs` | Poll stores stats, overlapping polls skipped, a throwing provider doesn't block later polls; polling only while connected with a viewer, an immediate poll for the first viewer, resuming after reconnect (#25) | 18 |
| `LoadingTilesTests.cs` | Loading tile parameters, tile index parsing, begin/end state, two tiles to fit beside the Back button, font size that fits the message (#25); rendering is SDK-dependent and checked on a device | 17 |
| `OBSWebsocketAdapterStatsTests.cs` | Null-tolerant `GetStats` parsing | 4 |
| `LogThrottleTests.cs` | Repeat suppression window, suppressed-count reporting, per-message independence, pruning | 6 |
| `VolumeConverterTests.cs` | Volume mul→dB conversion and formatting | 10 |
| `SourceVisibilityTests.cs` | Source visibility toggle and query, `TryGetSceneItemEnabled` cache fetch | 11 |
| `VirtualCameraCommandTests.cs` | Virtual camera state and toggle | ~5 |
| `ManualReconnectTests.cs` | Manual reconnect trigger | ~1 |
| `OBSActionExecutorStatsAndMediaTests.cs` | Stats, stream status, media input methods, `TryGetMediaInputStatus` cache fetch, `ToggleMediaInputPlayback` | 27 |
| `MediaInputStatesTests.cs` | Media action → resulting state mapping, single-tap action per state | 14 |
| `OBSStatsModelTests.cs` | OBSStats and OBSStreamStats derived properties | 12 |
| `PluginConfigReaderTests.cs` | Save/read config, round-trip, invalid JSON | 6 |

### Actions Layer (Integration Tests for Critical Paths)

| Test File | Covers | Tests |
|-----------|--------|-------|
| `Actions/ProfileSelectCommandTests.cs` | Constructor, singleton pattern | ~1 |
| `Actions/RecordingCommandTests.cs` | Start/Stop/Pause command construction | ~3 |
| `Actions/RecordingToggleCommandTests.cs` | Toggle command properties | ~1 |
| `Actions/SceneCollectionSelectCommandTests.cs` | Constructor, singleton | ~1 |
| `Actions/ScenesDynamicFolderTests.cs` | Constructor, instance property | ~2 |
| `Actions/SceneSwitchAdjustableCommandTests.cs` | Constructor, no-op interface methods (connect/disconnect only redraw, which needs the SDK) | 4 |
| `Actions/ScreenshotCommandTests.cs` | Constructor, properties | ~1 |
| `Actions/StatusDisplayCommandTests.cs` | Display command construction | ~3 |
| `Actions/SourceVisibilityAdjustableCommandTests.cs` | Constructor (its interface methods only redraw, which needs the SDK) | 1 |
| `Actions/AudioMuteAdjustableCommandTests.cs` | Constructor, interface methods | ~3 |
| `Actions/AudioMonitoringCycleAdjustableCommandTests.cs` | Constructor, interface methods | ~3 |
| `Actions/AudioSelectAdjustableCommandTests.cs` | Constructor, interface methods | ~3 |
| `Actions/AudioSelectDynamicFolderTests.cs` | Constructor, interface methods, deselect on disconnect | ~6 |
| `Actions/AudioVolumeDynamicFolderTests.cs` | Constructor, interface methods, event handlers | ~5 |
| `Actions/SelectedSourceVolumeAdjustmentTests.cs` | Constructor, interface methods | ~3 |
| `Actions/AudioSourceSelectCommandTests.cs` | Constructor, interface methods (added v1.6.0) | ~3 |
| `Actions/AudioStatusDisplayCommandTests.cs` | Constructor, interface methods | ~3 |
| `Actions/MediaDynamicFolderTests.cs` | Constructor, `IInputsListAwareCommand` behaviour | ~3 |
| `Actions/SceneCollectionsDynamicFolderTests.cs` | Constructor, instance property (added v1.6.0) | ~2 |
| `Actions/SceneSelectCommandTests.cs` | Constructor, interface methods (added v1.6.0) | ~3 |

## Coverage by Class (Key Classes)

| Class | Line Coverage | Branch Coverage | Notes |
|-------|-------------|-----------------|-------|
| **OBSActionExecutor** | 90% | 83% | Core business logic, excellent coverage |
| **OBSConfigReader** | 91% | 90% | Excellent coverage |
| **OBSLifecycleManager** | 79-100% | 75-100% | Good coverage |
| **OBSConnectionSettings** | 100% | 100% | Perfect |
| **OBSWebSocketManager** | 34% | 17% | Event handlers hard to unit test |
| **CommandRegistry** | 100% | 100% | Store + generic filter, fully covered |
| **OBSFacade** | 71% | 37% | Query/state methods covered, action delegation partially |
| **CommandCoordinator** | 100%* | 100%* | Now owns dispatch + exception isolation, directly tested (26 tests); *not yet re-measured with Cobertura since the refactor |

## Why Some Classes Show 0% Despite Tests

### Loupedeck SDK Dependency (Exempt per TDD rules)

All `Actions/` classes inherit from SDK base classes (`PluginDynamicCommand`, `PluginMultistateDynamicCommand`, `PluginDynamicFolder`, `ActionEditorCommand`). These:

- Require the Loupedeck runtime to instantiate properly
- Call `OBSStudioForLogiPlugin.Instance?.RegisterCommand(this)` in constructors
- Use `BitmapBuilder`, `EmbeddedResources`, `PluginImageSize` for rendering
- Throw `NullReferenceException` from `ActionImageChanged()`/`CommandImageChanged()` when created with `new` in a test, because the SDK never registered them — so don't write "does not throw" tests for handlers that redraw
- Cannot be meaningfully unit tested without mocking the entire framework

### Static Logging in OBSFacade

`OBSFacade` logs via the static `PluginLog.Warning()` helper rather than the injected `IPluginLog`. This means log-verification tests can't capture its output through the mock. Tests verify behavior (no-throw, safe defaults) instead.

### OBSWebsocketAdapter (Pass-Through)

`OBSWebsocketAdapter` is a thin wrapper delegating to `obs-websocket-dotnet`. It's tested indirectly through `OBSActionExecutor` tests which mock `IOBSWebsocket`.

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

    // The executor was built with an inline runner: new OBSActionExecutor(obs, log, action => action())
    this._executor.SetInputVolume("Microphone", 0.5f);

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

1. **OBSWebSocketManager event handlers (null-guard branches)** — `OnStreamStateChanged`, `OnRecordStateChanged`, etc. contain null-coalescing fallbacks (`e?.OutputState?.State ?? STOPPED`) that only fire if OBS sends malformed events. These are private methods triggered by the real `OBSWebsocket` library and can't be invoked directly without `InternalsVisibleTo`. The actual state-setting logic they delegate to is fully tested via `OBSActionExecutor` and `OBSWebSocketManagerEventDispatchTests`.
2. **Reconnection timer logic** — `OnReconnectTimer` complexity reduced to 4 after extracting `ReconnectionStrategy` (fully tested with 15 tests). Remaining untested branches are the guard clause (`_disposed || !_shouldReconnect`) and scheduling condition.
3. **`AudioInputMembership` clearing (#25)** — `OBSWebSocketManager` clears the cache from the same private event handlers as item 1 (scene item, input, scene list and collection events). `CachedValueTests` covers the cache itself; step 6 of `docs/device-checks/on-demand-loading.md` covers the wiring.

### Intentionally Not Tested

1. **All Actions/ command classes** — SDK-dependent, exempt per TDD rules
2. **ButtonImageHelper / ButtonTextRenderer** — Rendering code requiring SDK
3. **OBSWebsocketAdapter** — Pass-through wrapper
4. **OBSStudioForLogiPlugin main class** — Orchestration requiring full plugin runtime
5. **ConnectionManager** — Thin async delegation, hard to test without real WebSocket

## Test Timing Constants

Defined in `src/Helpers/OBSTimings.cs`:

- `TestAsyncDelay = 500ms` — wait for code driven by a real timer (`DoubleTapHelperTests`) only

`OBSActionExecutor` tests don't wait at all: they inject an inline background runner. The fixed 500ms sleeps they used to have raced the thread pool under full-suite load (assessment #15).
