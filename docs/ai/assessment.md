# Project Assessment — Findings, Tasks & Priorities

## Overview

Assessment conducted against v1.5.1. Covers code quality, architecture, usability, and feature gaps.
Items are ordered by priority within each category.

Items #1, #2, #4, #7, #10, #11 shipped in v1.6.0; v1.6.1 was a maintenance release (audio input-kind filter fix, default profiles, action-picker grouping) with no assessment items addressed. #6, #12, and #13 were fixed post-v1.6.1, shipping in v1.6.2. #14 was fixed post-v1.6.2 (not yet released). As of now, the remaining open items are #3, #5, #8, #9, #15.

---

## High Priority

### 1. CommandRegistry Bypass — `OBSWebSocketManager` (Architecture)

**Problem**: `OBSWebSocketManager.OnConnected` and `OnDisconnected` each contain ~10 hardcoded singleton calls that bypass the `CommandRegistry`/`CommandCoordinator` system entirely. `CommandCoordinator.NotifyConnected()` and `NotifyDisconnected()` exist but are never called from `OBSStudioForLogiPlugin.OnOBSConnected` / `OnOBSDisconnected`. `OnApplicationStopped` does call `NotifyDisconnected()` but `OnOBSConnected` never calls `NotifyConnected()`.

**Impact**: Any new command that correctly self-registers via `IObsCommand` will never receive `OnConnected()` through the registry. It only works if a hardcoded singleton call is also added to `OBSWebSocketManager`. This is a maintenance trap that contradicts the registry's design intent.

**Fix**:

1. Add `this._commandCoordinator.NotifyConnected()` to `OBSStudioForLogiPlugin.OnOBSConnected`
2. Add `this._commandCoordinator.NotifyDisconnected()` to `OBSStudioForLogiPlugin.OnOBSDisconnected`
3. Remove the hardcoded singleton calls from `OBSWebSocketManager.OnConnected` and `OnDisconnected`

**Risk**: Low — `CommandRegistry` is fully tested (100% coverage). Every command that self-registers already implements `IObsCommand` correctly.

---

### 2. Scene Change Bypasses Registry — `OBSStudioForLogiPlugin` (Architecture)

**Problem**: `OBSStudioForLogiPlugin.OnCurrentSceneChanged` calls `SourcesDynamicFolder.Instance` and `SceneAudioSourcesDynamicFolder.Instance` directly via singleton, bypassing the registry:

```csharp
this._obsFacade.UpdateSourcesForScene(sceneName,
    (scene, sources) => SourcesDynamicFolder.Instance?.UpdateSources(scene, sources),         // bypass
    (scene, audioSources) => SceneAudioSourcesDynamicFolder.Instance?.UpdateAudioSources(...)); // bypass
```

These folders implement `IObsCommand` and `ISceneAwareCommand` but their scene-change update path is hardwired. Adding a new scene-aware folder requires editing `OBSStudioForLogiPlugin` rather than just implementing the interface.

**Fix**: Introduce an `ISceneSourcesAwareCommand` interface (or extend `ISceneAwareCommand`) with an `OnSceneSourcesChanged(String sceneName, String[] sources)` method, and route through the registry.

---

### 3. Scene/Source/Profile Buttons Show No Text (Usability)

**Problem**: `ScenesDynamicFolder`, `SourcesDynamicFolder`, and `ProfilesDynamicFolder` display only a selected/unselected icon — no name text. On the MX Console's small tiles, users must memorise button positions to know which scene or source each button represents.

**Fix**: Use `ButtonTextRenderer.RenderTextWithBorder` or `ButtonImageHelper.StateTextWithIcon` to render the item name alongside the selection indicator. Example for scenes:

```csharp
public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
{
    Boolean isSelected = actionParameter == this._currentScene;
    String icon = isSelected ? "SceneSelected.svg" : "SceneUnselected.svg";
    return ButtonImageHelper.StateTextWithIcon(actionParameter, imageSize, isSelected,
        "SceneSelected.svg", "SceneUnselected.svg",
        BitmapColor.Green, BitmapColor.White);
}
```

---

## Medium Priority

### 4. `DoubleTapHelper` Race Condition and Memory Leak (Code Quality)

**Problem**: `_tapStates` dictionary is accessed from both the calling thread and `Task.Run` background threads without synchronisation — a race condition. Additionally, cancelled `CancellationTokenSource` objects are never disposed, leaking memory over time in a long-running plugin.

**Fix**:

- Add `lock (_tapStates)` around all dictionary access
- Call `cancellation.Dispose()` after cancellation in the `TaskCanceledException` catch block and after single-tap fires

---

### 5. Double-Tap Interaction Unreliable on MX Console (Usability)

**Problem**: The 500ms double-tap window is tight for the MX Creative Console's physical dial buttons. Double-tap is used in `AudioInputDynamicFolderBase` (double-tap to mute) and `MediaDynamicFolder` (double-tap to stop). The single-tap action also fires with a 500ms delay, making audio source selection feel broken.

**Recommendation**: Expose the double-tap threshold as a configurable value in `OBSTimings` or `PluginConfig`. Consider whether dedicated separate buttons (e.g. a standalone mute button) are a better UX than double-tap for the MX Console primary use case.

---

### 6. `CommandCoordinator` Is a Valueless Pass-Through (Code Quality) ✅ Fixed

**Problem**: Every method in `CommandCoordinator` was a one-liner delegating identically to `CommandRegistry`. It added a layer of indirection with no behaviour, no validation, no error isolation, and 0% test coverage.

**Fix applied**: Responsibility moved so each class does one thing — `CommandRegistry` is now purely a store (`Register()` + a generic `GetCommands<T>()` filter), and `CommandCoordinator` owns dispatch through a private generic `NotifyEach<T>(eventName, action)` helper that every `NotifyXxx()` method calls. Each command invocation inside `NotifyEach` is wrapped in try/catch — a throwing command is logged via `PluginLog.Error` and does not prevent the remaining registered commands from being notified for that same event. `CommandCoordinatorTests.cs` (26 tests) covers dispatch-by-interface for every notification type plus explicit exception-isolation tests (`NotifyConnected_OneCommandThrows_StillNotifiesRemainingCommands`, `NotifyConnected_CommandThrows_DoesNotPropagateException`, `NotifySceneChanged_OneCommandThrows_StillNotifiesRemainingCommands`). `CommandRegistryTests.cs` was trimmed to cover only registration/dedup and `GetCommands<T>()` filtering, since dispatch no longer lives there.

---

### 7. `OBSStats` Null Propagation (Code Quality)

**Problem**: `OBSActionExecutor.GetStats()` and `OBSFacade.GetStats()` return `null` when disconnected. Display commands (`StatsDisplay`, `StatsDynamicFolder`, `StreamStatsDynamicFolder`) must null-check defensively on every call.

**Fix**: Introduce a null-object pattern:

```csharp
public static OBSStats Empty => new OBSStats(); // all zero values
```

Return `OBSStats.Empty` instead of `null` from disconnected/error paths. Display commands can then render "0.0 FPS" etc. without null guards.

---

## Low Priority

### 8. `ProfileListChanged` / `SceneCollectionListChanged` Events Not Subscribed (Feature Gap)

**Problem**: If a user creates or deletes a profile or scene collection while the plugin is connected, the dynamic folders go stale until reconnect. The OBS WebSocket protocol fires `ProfileListChanged` and `SceneCollectionListChanged` events for exactly this case.

**Fix**: Subscribe to both events in `OBSWebSocketManager`, re-read the lists, and push them through `OnProfilesChanged` / `OnSceneCollectionsChanged` respectively. Low effort, meaningful reliability improvement.

---

### 9. Recording Duration Display Missing (Feature Gap)

**Problem**: The Stream Stats folder shows live stream duration, but there is no equivalent for recording. `GetRecordStatus` in the OBS WebSocket API returns a timecode and bytes written. Parity between streaming and recording stats displays is a usability gap for users monitoring recording length.

**Fix**: Add a `RecordingStatsDynamicFolder` or a `RecordingStatusDisplay` command using the same polling pattern as `StatsService`.

---

### 10. `MediaDynamicFolder` Doesn't Respond to Input List Changes (Feature Gap)

**Problem**: `MediaDynamicFolder.OnConnected()` loads the media list once. If a user adds or removes a media source in OBS while connected, the folder doesn't update. Audio inputs handle this via `IInputsListAwareCommand` and `OnInputListChanged`, but `MediaDynamicFolder` only implements `IObsCommand`.

**Fix**: Implement `IInputsListAwareCommand` in `MediaDynamicFolder` and filter the incoming inputs list to media kinds:

```csharp
public void OnInputsChanged(String[] inputs)
{
    this._mediaInputs = OBSStudioForLogiPlugin.Instance?.GetMediaInputList() ?? new String[0];
    this.ButtonActionNamesChanged();
}
```

---

### 11. Password Field Has No Sensitivity Indication (Security)

**Problem**: The password field in `PluginSettingsCommand` is a plain `ActionEditorTextbox` with no masking or indication that the value is sensitive. Per `SecureCoding.md`, sensitive fields should be clearly marked.

**Fix**: Add "(sensitive)" to the label text as a minimum. If the SDK supports a password input type, use it.

---

## Newly Identified (2026-08-21)

Found while migrating to .NET 10.0 (`5d04506`) and refreshing this memory bank. Not yet actioned.

### 12. Verify net10.0 Plugin Actually Runs Under Logi Plugin Service (Risk) ✅ Fixed

**Problem**: The .NET 10 migration built cleanly and all unit tests passed, but it had not been verified to run inside the real Logi Plugin Service host process. `PluginApi.dll` is loaded at runtime from `C:\Program Files\Logi\LogiPluginService\PluginApi.dll` (or the macOS equivalent) — compiling against the `ci/PluginApi.dll` stub proves nothing about whether that host process can load a net10.0 plugin assembly.

**Resolution**: Verified locally — the net10.0 build runs under a real Logi Plugin Service install, connects to OBS, and responds to button presses. Confirmed 2026-08-21, ahead of the v1.6.2 release.

---

### 13. `obj/` Location Depended on How You Invoked the Build (Build/DX) ✅ Fixed

**Problem**: This was not just migration leftovers — it was self-reproducing. `src/Directory.Build.props` set `<BaseIntermediateOutputPath>$(SolutionDir)obj\</BaseIntermediateOutputPath>`. `$(SolutionDir)` is only defined by MSBuild when building through the `.sln` (resolving to the repo root, e.g. `obj/` at the top level); building the bare `.csproj` directly (or via a `ProjectReference`) leaves `$(SolutionDir)` undefined, so it falls back to a path relative to the project file (`src/obj/`). Whichever invocation ran last left stale generated `AssemblyAttributes.cs`/`AssemblyInfo.cs` behind that the *other* invocation's default item-exclude glob no longer matched, so `**/*.cs` picked the stale file up as a real source file alongside the freshly generated one in the new location — hence the `CS0579` duplicate-attribute errors. Alternating between `dotnet build OBSStudioForLogiPlugin.sln`, `dotnet build src/OBSStudioForLogiPlugin.csproj`, and `dotnet test` (which builds the `ProjectReference`) reproduced it repeatedly.

**Fix applied**: Changed `BaseIntermediateOutputPath` to `$(MSBuildThisFileDirectory)obj\`, which always resolves to `src/obj/` (the directory containing `Directory.Build.props` itself) regardless of invocation method. Verified by alternating all three invocation methods from a clean state — `obj/` now lands only in `src/obj/` every time. `<BaseOutputPath>$(SolutionDir)..\bin\</BaseOutputPath>` in the same file is still `$(SolutionDir)`-dependent but is dead code in practice — `OBSStudioForLogiPlugin.csproj` sets its own `<BaseOutputPath>` later using `$(MSBuildThisFileDirectory)`, which wins. Left as-is to keep this fix minimal; worth cleaning up if it's ever touched again.

---

### 14. `DoubleTapHelperTests` Flaky Under Full-Suite / Coverage-Collector Load (Test Reliability) ✅ Fixed

**Problem**: `OnTap_TwoDistinctParameters_FireIndependently` and `OnTap_SingleTap_FiresSingleAfterThreshold` failed intermittently under full-suite/coverage-collector load, but passed cleanly every time they ran in isolation. They relied on `Thread.Sleep(OBSTimings.TestAsyncDelayExtended)` (750ms) after `Task.Run` fire-and-forget calls racing a 500ms internal threshold — only 250ms of margin, not always enough under thread-pool contention.

**Fix applied**: Replaced the fixed sleep in both tests with a bounded poll (`WaitFor`, 20ms interval, 3000ms ceiling) that returns as soon as the expected state is observed instead of waiting a fixed window and hoping it was long enough. Verified with 5 consecutive full-suite runs — zero failures from `DoubleTapHelperTests` across all 5 (previously flaked roughly 1 in 3 runs).

**Follow-up finding**: those same 5 runs surfaced that the identical race pattern (fixed `Thread.Sleep` after a `Task.Run` fire-and-forget, asserting a mock call landed) exists much more broadly across the `OBSActionExecutor*` test classes, and is actually the dominant source of full-suite flakiness now that `DoubleTapHelperTests` is fixed. That's a separate, larger piece of work — see #15.

---

### 15. General `Thread.Sleep`-After-`Task.Run` Flakiness Across `OBSActionExecutor*` Tests (Test Reliability)

**Problem**: Many tests across `OBSActionExecutorStudioModeTests`, `OBSActionExecutorAudioTests`, `OBSActionExecutorSceneSwitchingTests`, `OBSActionExecutorStudioModeTransitionTests`, `VirtualCameraCommandTests`, and others follow the same pattern as the now-fixed #14: fire a `Task.Run`-based operation, `Thread.Sleep(OBSTimings.TestAsyncDelay)`, then verify a mock call landed. Under full-suite thread-pool contention (393 tests, many spinning up background tasks concurrently) this fixed window isn't always enough — observed 3-6 different tests failing per run across 5 consecutive full-suite runs, never the same set twice, all passing in isolation.

**Fix**: Same approach as #14 — replace the fixed `Thread.Sleep` + assert pattern with a bounded poll on the mock's invocation count (e.g. `WaitFor(() => mock.Invocations.Any(i => i.Method.Name == "MethodName"))`, or expose a poll helper on the mock verification itself). Given the number of affected call sites (dozens, across many test files), this is a larger, mechanical refactor rather than a quick fix — worth doing as its own pass rather than folding into other work. This is the real blocker for ever enabling tests in CI (`AGENTS.md` already calls this out), more so than #14 was.

---

## Newly Identified (2026-09-27)

Found by reviewing a Logi Plugin Service log (`%LocalAppData%\Logi\LogiPluginService\Logs\plugin_logs\OBSStudioForLogi.log`) from a session where the device lagged: ~3,250 lines in 7 minutes, 359 errors (326 of them `Request timed out`), and 47 "OBS WebSocket connected" entries from only 2 real connections.

### 16. Reconnect Storm and Blocking OBS Requests on the Render Path (Performance) ✅ Fixed

**Problem**: Three compounding issues.

1. obs-websocket-dotnet 5.7.0 raises `Connected` for every `Identified` message, and OBS sends one to confirm each `ReIdentify`. The library sends a `ReIdentify` on every `InputVolumeMeters` subscribe/unsubscribe, which the audio meter buttons toggle as they appear and disappear. Each page flip therefore re-ran the full initial-state load: every command's `OnConnected`, a stats restart, and profile/scene/input reloads.
2. Every audio button redraw made up to three blocking OBS requests (mute, volume, monitor type) on the SDK's render thread. Requests block for up to the library's 10s timeout; under the reload storm they backed up, timed out together, and the device stalled while buttons waited.
3. Each reload fanned out into dozens of redundant requests: five commands fetched the input list themselves, `SceneSelectCommand.OnScenesChanged` discarded the list it was given and fetched it again, and `AudioMixerDynamicFolder` made roughly inputs × (2 + scenes) requests (~88 for 11 inputs and 6 scenes) only to write a debug log line.

**Fix applied**:

- `SessionGate` makes `OBSWebSocketManager.OnConnected` run once per connection; repeats are ignored until a disconnect, `Disconnect` or new `ConnectAsync`. The matching library fix (`Connected` raised only on the first `Identified`) is on `mrnil/obs-websocket-dotnet` branch `fix/connected-raised-on-reidentify`, pending an upstream PR and release.
- `ConnectionManager.ReconnectAsync` ignores the Reconnect button while already connected or while an attempt is still waiting on the port. A second press used to tear down a working connection.
- `AudioStateCache` serves mute/volume/monitor type to rendering without blocking: a miss returns defaults and fetches that input once in the background, then redraws it; change events keep it current, event values win over an in-flight fetch, and failed fetches back off for `OBSTimings.AudioStateRetryDelay`. `OBSFacade`'s audio getters read it, so every existing render path is covered. `SetInputVolume` records the target immediately so fast dial turns don't lose steps.
- The per-connection state load is the single source for the input, scene, profile and scene collection lists; commands receive them through `IInputsListAwareCommand`, `IScenesListAwareCommand`, `IProfilesListAwareCommand` and the new `ISceneCollectionsListAwareCommand` instead of querying OBS in `OnConnected`. The mixer folder's debug loop and the now-unused `GetInputKind`/`GetScenesForInput` chain were removed.

Tests: `SessionGateTests`, `ConnectionManagerTests`, `AudioStateCacheTests`, `TryGetInputAudioState_*` in `OBSActionExecutorAudioTests`, cache-backed getters in `OBSFacadeTests`, and `NotifySceneCollectionsChanged_*` in `CommandCoordinatorTests`.

---

### 17. Remaining Findings From the 2026-09-27 Log Review (Performance/Reliability)

**Problem**: Follow-ups from the same review, not yet actioned.

- **Requests are still synchronous with a 10s timeout.** Set a shorter `WSTimeout` *before* `ConnectAsync` — in 5.7.0 the setter also sets Websocket.Client's no-message `ReconnectTimeout`, so setting it after connecting arms an idle-disconnect watchdog. Guard on `IsIdentified` (new in 5.7.0) rather than `IsConnected`, which is true before the handshake completes.
- **`StatsService` polls can overlap** — a 5s timer with requests that can block for 10s. Skip a tick while the previous one is in flight. `GetStats` also fails outright when OBS reports `cpuUsage: null` (non-nullable in the library's `ObsStats`).
- **Other render paths still query OBS**: `SourcesDynamicFolder` (`GetSceneItemEnabled`, two requests per redraw) and `MediaDynamicFolder` (`GetMediaInputStatus`). A scene change also costs ~10 requests via `OBSFacade.UpdateSourcesForScene`.
- **No retry after the startup port wait gives up.** If OBS isn't running when the plugin loads, the plugin stops trying after 20 attempts and only reconnects on a manual Reconnect (a 2-hour gap in the reviewed log). `ClientApplication.ApplicationStarted` never fired — check the SDK docs on whether `HasNoApplication = true` prevents it.
- **Logging volume**: routine calls log at Info (`Getting input list`, `Getting scene list`), and a stalled request logs one error per redraw rather than one per incident.
- **Meter lease churn**: the 3s `AudioMeterRenderLease` still drops and re-takes the `InputVolumeMeters` subscription on page flips. Each toggle is now a single cheap `ReIdentify`, but a longer lease would avoid it.
- `INSTALL.md` gives the log path as `Logs\OBSStudioForLogiPlugin.log`; the file is actually `Logs\plugin_logs\OBSStudioForLogi.log`.

---

## Summary Table

| # | Priority | Area | Issue |
|---|----------|------|-------|
| 1 | ~~High~~ | ~~Architecture~~ | ~~`CommandRegistry` bypass — `NotifyConnected`/`NotifyDisconnected` never called through registry~~ ✅ Fixed |
| 2 | ~~High~~ | ~~Architecture~~ | ~~`OnCurrentSceneChanged` bypasses registry for `SourcesDynamicFolder` and `SceneAudioSourcesDynamicFolder`~~ ✅ Fixed |
| 3 | High | Usability | Scene/source/profile buttons show no text — unusable without memorisation |
| 4 | ~~Medium~~ | ~~Code Quality~~ | ~~`DoubleTapHelper` race condition and `CancellationTokenSource` leak~~ ✅ Fixed |
| 5 | Medium | Usability | Double-tap unreliable on MX Console; 500ms delay on audio selection |
| 6 | ~~Medium~~ | ~~Code Quality~~ | ~~`CommandCoordinator` is a valueless pass-through — no error isolation~~ ✅ Fixed |
| 7 | ~~Medium~~ | ~~Code Quality~~ | ~~`OBSStats` null propagation — null-object pattern would clean up display commands~~ ✅ Fixed |
| 8 | Low | Feature | `ProfileListChanged`/`SceneCollectionListChanged` events not subscribed |
| 9 | Low | Feature | Recording duration display (parity with streaming stats) |
| 10 | ~~Low~~ | ~~Feature~~ | ~~`MediaDynamicFolder` doesn't respond to input list changes~~ ✅ Fixed |
| 11 | ~~Low~~ | ~~Security~~ | ~~Password field has no masking or sensitivity indication~~ ✅ Fixed |
| 12 | ~~High~~ | ~~Risk~~ | ~~net10.0 migration unverified against real Logi Plugin Service host~~ ✅ Fixed |
| 13 | ~~Medium~~ | ~~Build/DX~~ | ~~`obj/` location depended on invocation method, causing spurious `CS0579` errors~~ ✅ Fixed |
| 14 | ~~Low-Medium~~ | ~~Test Reliability~~ | ~~`DoubleTapHelperTests` flaky under full-suite/coverage load~~ ✅ Fixed |
| 15 | Low-Medium | Test Reliability | Same fixed-sleep race broadly across `OBSActionExecutor*` tests — dozens of call sites, dominant flakiness source now that #14 is fixed |
| 16 | ~~High~~ | ~~Performance~~ | ~~Reconnect storm on every meter subscription change; blocking OBS requests on the render path~~ ✅ Fixed |
| 17 | Medium | Performance/Reliability | Remaining log-review findings: request timeout, stats overlap, other render-path queries, startup retry, log volume |
