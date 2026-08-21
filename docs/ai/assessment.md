# Project Assessment — Findings, Tasks & Priorities

## Overview

Assessment conducted against v1.5.1. Covers code quality, architecture, usability, and feature gaps.
Items are ordered by priority within each category.

Items #1, #2, #4, #7, #10, #11 shipped in v1.6.0; v1.6.1 was a maintenance release (audio input-kind filter fix, default profiles, action-picker grouping) with no assessment items addressed. #6, #12, and #13 were fixed post-v1.6.1, shipping in v1.6.2. #3 and #14 were fixed post-v1.6.2 (not yet released). As of now, the remaining open items are #5, #8, #9, #15, #16.

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

### 3. Scene/Source/Profile Buttons Show No Text (Usability) ✅ Fixed

**Problem**: `ScenesDynamicFolder`, `SourcesDynamicFolder`, and `ProfilesDynamicFolder` displayed only a selected/unselected icon — no name text. On the MX Console's small tiles, users had to memorise button positions to know which scene or source each button represents.

**Fix applied**: Switched `GetCommandImage` in all three to `ButtonTextRenderer.RenderTextWithBorder`, rendering the item name as text with color + border indicating state, replacing the icon entirely:

```csharp
public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
{
    Boolean isSelected = actionParameter == this._currentScene;
    return ButtonTextRenderer.RenderTextWithBorder(actionParameter, imageSize, isSelected ? BitmapColor.Green : BitmapColor.White, isSelected);
}
```

`SourcesDynamicFolder` uses Green/Red for visible/hidden instead (no border — visibility isn't a "selection" concept). The originally-proposed `ButtonImageHelper.StateTextWithIcon` doesn't exist anywhere in the codebase — that was inherited documentation drift, corrected here and in `docs/ai/image-rendering-simplified.md`/`guidelines.md`/`sdk-quick-reference.md`/`structure.md` while implementing this fix. `SourceVisibilityOn.svg`/`SourceVisibilityOff.svg` were removed (no longer used by anything); `ScenesSelected/Unselected.svg` and `ProfileSelected/Unselected.svg` are kept — still used by `SceneSelectCommand`/`ProfileSelectCommand`, which have the identical no-text problem but are out of this item's original scope (see #16).

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

**Fix**: Subscribe to both events in `OBSWebSocketManager` and call `UpdateProfileList()` / `UpdateSceneList()` respectively. Low effort, meaningful reliability improvement.

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

### 16. `SceneSelectCommand`/`ProfileSelectCommand`/`SceneCollectionSelectCommand`/`AudioSourceSelectCommand` Have the Same No-Text Problem as #3 (Usability)

**Problem**: Fixing #3 only covers the three `*DynamicFolder` classes. The four newer multi-state `*SelectCommand` classes (added in v1.6.0, see `docs/ai/structure.md`'s "Multi-State Select Commands") have the identical problem — `GetCommandImage` returns a plain `ButtonImageHelper.Icon(...)` selected/unselected icon with no name text, e.g. `SceneSelectCommand.GetCommandImage`. `AudioSourceSelectCommand` is the exception — it already renders name/volume/mute state as text, so it's not affected.

**Fix**: Same pattern as #3 — switch `GetCommandImage` to `ButtonTextRenderer.RenderTextWithBorder(actionParameter, imageSize, isSelected ? BitmapColor.Green : BitmapColor.White, isSelected)` in `SceneSelectCommand`, `ProfileSelectCommand`, and `SceneCollectionSelectCommand`. `PluginMultistateDynamicCommand`'s `GetCommandImage` signature takes an extra `stateIndex` parameter instead of reading instance state directly — check the disconnected-state early return in each (e.g. `SceneSelectCommand` returns a bare icon when `!isConnected`) still makes sense as text or reverts to an icon-only fallback.

---

## Summary Table

| # | Priority | Area | Issue |
|---|----------|------|-------|
| 1 | ~~High~~ | ~~Architecture~~ | ~~`CommandRegistry` bypass — `NotifyConnected`/`NotifyDisconnected` never called through registry~~ ✅ Fixed |
| 2 | ~~High~~ | ~~Architecture~~ | ~~`OnCurrentSceneChanged` bypasses registry for `SourcesDynamicFolder` and `SceneAudioSourcesDynamicFolder`~~ ✅ Fixed |
| 3 | ~~High~~ | ~~Usability~~ | ~~Scene/source/profile buttons show no text — unusable without memorisation~~ ✅ Fixed |
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
| 16 | Low | Usability | `SceneSelectCommand`/`ProfileSelectCommand`/`SceneCollectionSelectCommand` have the same no-text problem #3 fixed, out of its original scope |
