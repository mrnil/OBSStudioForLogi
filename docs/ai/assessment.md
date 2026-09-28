# Project Assessment — Findings, Tasks & Priorities

## Overview

Reassessed on 2026-09-28 against `main`: version 2.0.0, not yet tagged. The last release is v1.6.2. This pass also covers the uncommitted working-tree changes to `SourceVisibilityAdjustableCommand` and `SceneSwitchAdjustableCommand`. It covers code quality, architecture, security, usability, test reliability and doc accuracy. The first assessment was made against v1.5.1.

Item numbers stay the same between assessments because `TODO.md`, commit messages and `CHANGELOG.md` refer to them. Open items from earlier passes keep their original numbers. New findings start at #18. Fixed items are summarised under [Resolved](#resolved); for their full write-ups, see `git log -p -- docs/ai/assessment.md`.

**Open**: #3, #5, #8, #9, #15, #18–#24. Priority order is in the [Summary Table](#summary-table).

---

## High Priority

### 18. Source Visibility and Media Status Still Query OBS on the Render Path (Performance)

Carried over from #17 ("Still open"), with one more call site.

**Problem**: #16 moved audio rendering onto `AudioStateCache`, but three render methods still make blocking OBS requests on the SDK's render thread:

- `SourcesDynamicFolder.GetCommandImage` → `GetSourceVisibility` (`GetSceneItemId` + `GetSceneItemEnabled`, two requests per tile per redraw)
- `SourceVisibilityAdjustableCommand.GetCommandImage` → `GetSourceVisibility`. This is new in the uncommitted working-tree change and only runs while connected.
- `MediaDynamicFolder.GetCommandImage` → `GetMediaInputStatus`

A scene change also costs about 10 requests through `OBSFacade.UpdateSourcesForScene`. This is the same class of bug that caused the 2026-09-27 device lag. When OBS is slow, each tile waits up to `OBSTimings.RequestTimeout` (3s).

**Fix**: Do what `AudioStateCache` does: serve renders from a cache keyed by (scene, source), return a default and fetch in the background on a miss, and keep the cache current from `SceneItemEnableStateChanged` (already subscribed) and the `MediaInputPlayback*` events. Clear it on disconnect and on scene-collection change.

---

### 19. Remote OBS Password Stored in Plaintext (Security): Partly Fixed

**Fixed so far**: `config.json` no longer holds the password. `PluginConfig.RemotePassword` is `[JsonIgnore]`. `PluginConfigReader` stores the password through `ISecretStore`, which the plugin backs with the SDK's plugin settings (`SetPluginSetting`, `backupOnline: false`). The Logi Plugin Service stores these encrypted on Windows and macOS, so DPAPI and Keychain code isn't needed. On the first read, a plaintext password left by an older version is saved to the store and removed from the file. If the store rejects it, the file is left as it was and the plaintext copy is still used, so the password is never lost. The read that uses the store happens in `Load()`, because the SDK doesn't document whether plugin settings work in the plugin constructor.

**Still open**: the password is typed into the Plugin Settings action's `Password (sensitive)` text box, which is an Action Editor control. The Logi Plugin Service saves Action Editor values in the device profile (`%LocalAppData%\Logi\LogiPluginService\Applications\<device>\<app>\Profiles\<id>\ProfileInfo.json`), and the password is in plaintext there. This was confirmed on 2026-09-28 against a profile with the action assigned.

**Fix**: The SDK has no masked or non-persisted text box, so the password must not stay in the action's parameters. Options:

- Treat an empty Password field as "keep the stored password", tell users to clear the field after saving, and add a "Clear saved password" checkbox for removing it. The plaintext copy then lasts only until the user clears the field.
- Move password entry out of the Action Editor completely, for example into a plugin-level settings UI if the SDK offers one.

`docs/ai/secure-coding.md` records the rule against collecting secrets through Action Editor controls.

---

## Medium Priority

### 20. Services and Plugin Class Still Reach Into Singletons (Architecture)

**Problem**: #1 and #2 routed command notifications through `CommandCoordinator`, but the same bypass remains in other places:

- The service layer depends upward on the plugin class. `OBSWebSocketManager` calls `OBSStudioForLogiPlugin.Instance?.On…` about a dozen times (profile, scene collection, scene, virtual camera, replay buffer and media events). `OBSActionExecutor` calls it for source visibility changes, and `StatsService`'s default constructor calls it for its stats providers.
- `OBSStudioForLogiPlugin` calls specific commands directly: `MediaDynamicFolder.Instance`, `StatsDisplay.Instance`, `StatsDynamicFolder.Instance`, `StreamStatsDynamicFolder.Instance` and `ConnectionStatusDisplay.Instance`.

**Impact**: Services can't be tested without the static plugin instance. That is why `OBSWebSocketManager` event handling has so little coverage. Adding a new stats or media display also means editing the plugin class instead of implementing an interface, which is the same maintenance trap #1 described.

**Fix**: Have `OBSWebSocketManager` raise its own events, or take a callback interface in its constructor, and let the plugin class subscribe and forward them to `CommandCoordinator`. Add `IStatsAwareCommand` and `IMediaStateAwareCommand` (or similar) so the displays and the media folder receive updates through the registry.

---

### 21. Renamed Inputs Go Stale (Feature Gap / Correctness)

**Problem**: `InputNameChanged` is not subscribed. The input lists are only refreshed on `InputCreated` and `InputRemoved`. If an input is renamed in OBS:

- the audio, media and meter folders keep showing the old name, and pressing one of those buttons targets an input that no longer exists
- `AudioStateCache` and the meter levels are keyed by input name, so the old entry lingers and the new name starts as a cache miss
- user-defined buttons (`AudioMuteAdjustableCommand`, `SourceVisibilityAdjustableCommand` and others) store the name and stop working without any error

**Fix**: Subscribe to `InputNameChanged`, re-key the cache entries, and push the refreshed input list through `NotifyInputsChanged`. User-defined buttons can't be updated automatically, so log a clear warning when a configured name isn't found. Check whether `SceneListChanged` fires on scene rename. If it doesn't, handle `SceneNameChanged` the same way.

---

### 22. AI Docs Describe `ButtonImageHelper` Methods That Don't Exist (Docs)

**Problem**: `docs/ai/guidelines.md` (the image examples near lines 262 and 406–418), `docs/ai/image-rendering-simplified.md` and `docs/ai/sdk-quick-reference.md` document `ButtonImageHelper.StateIcon`, `StateText`, `TextWithIcon` and `StateTextWithIcon`. None of them exist. The real API is `ButtonImageHelper.Icon(...)` for icons and `ButtonTextRenderer` for text. Commit `5eab775` corrected these docs, but its revert (`4e0b0d3`) restored the wrong versions along with the code.

**Impact**: `AGENTS.md` tells assistants to read `guidelines.md` before writing code, so they will produce calls that don't compile.

**Fix**: Re-apply only the doc parts of `5eab775`, or rewrite the examples against what is in `src/Helpers/`. This doesn't depend on the #3 decision.

---

### 3. Scene/Source/Profile Buttons Show No Text (Usability): Needs Decision

**Status**: The fix, `5eab775`, rendered item names with `ButtonTextRenderer.RenderTextWithBorder`. It was reverted in `4e0b0d3` and no reason was recorded. Since then the icon restyle added a tick for the selected scene, profile and scene collection, so the selected/unselected state is clearer. It is still unknown whether names are readable on the device.

**Next step**: Check on a device whether the SDK draws each item's display name under the icon in `ScenesDynamicFolder`, `SourcesDynamicFolder` and `ProfilesDynamicFolder`. The changelog notes that display names are drawn as a native overlay in some cases. If names already show, close this item and record why. If they don't, find out what caused the revert (duplicated text? clashes with the icon style guide?) before trying again. `SceneSelectCommand`, `ProfileSelectCommand` and `SceneCollectionSelectCommand` would need the same answer.

---

### 5. Double-Tap Interaction Unreliable on MX Console (Usability)

**Problem**: `DoubleTapHelper` (`src/Helpers/PressTimingHelper.cs`) still uses a hard-coded 500ms `DoubleTapThreshold` constant. Double-tap drives mute in `AudioInputDynamicFolderBase` and stop in `MediaDynamicFolder`. A single tap has to wait out the whole window before it fires, so selecting an audio source always lags by 500ms.

**Recommendation**: Move the threshold to `OBSTimings`, or make it a Plugin Settings option. Since then, the per-source **Audio Meter** action and the user-defined `AudioMuteAdjustableCommand` have given users single-press mute buttons. Consider making the folders single-tap only and dropping double-tap.

---

### 15. `Thread.Sleep`-After-`Task.Run` Flakiness in Tests (Test Reliability)

**Problem**: Unchanged in kind, and larger now: 88 `Thread.Sleep` calls in 12 test files, out of 531 tests. The largest are `OBSActionExecutorTests` (26), `OBSActionExecutorReplayBufferTests` (15) and `OBSActionExecutorAudioTests` (11). Most fire a `Task.Run` operation, sleep for `OBSTimings.TestAsyncDelay` and then assert on a mock, so they fail at random under full-suite thread-pool load. Some remaining sleeps in `DoubleTapHelperTests` and `ConnectionManagerTests` are intentional "nothing should happen" waits, so check each one before converting it.

**Fix**: Replace them with the bounded `WaitFor` poll used in #14, one file at a time, starting with the largest files. This is still the blocker for running tests in CI (see `AGENTS.md`).

---

## Low Priority

### 8. `ProfileListChanged` / `SceneCollectionListChanged` Not Subscribed (Feature Gap)

**Problem**: If a profile or scene collection is created, renamed or deleted in OBS while connected, the folders and select commands go stale until the current profile or collection changes or the plugin reconnects.

**Fix**: This is easier after #16. The lists already reach commands through `IProfilesListAwareCommand` and `ISceneCollectionsListAwareCommand`, so subscribe to both events in `OBSWebSocketManager`, re-read the list, and push it through the existing `NotifyProfileList` path and the scene-collection path.

---

### 9. Recording Duration Display Missing (Feature Gap)

**Problem**: The Stream Stats folder shows how long the stream has been live, but nothing shows the recording duration. `GetRecordStatus` returns a timecode and the bytes written.

**Fix**: Add the recording duration to `StatsService`'s poll, next to the stream status, and show it in a `RecordingStatsDynamicFolder` or add it to the existing stats folder.

---

### 23. Small Robustness Items (Code Quality)

- `OBSStudioForLogiPlugin.OnApplicationStarted` (line 138) is `async void` and has no try/catch. An exception from `ConnectAsync` would be unhandled and could take down the plugin host. Wrap the body and log the exception.
- `OBSWebSocketManager.OnCurrentSceneCollectionChanged` blocks a thread-pool thread with `Task.Delay(100).Wait()` and then queries the scene list, hoping OBS has caught up. OBS sends `CurrentProgramSceneChanged` after a collection switch, so the delay-and-query step can probably be removed. If it can't, `await` the delay instead of blocking.
- After a disconnect, `CommandCoordinator.NotifyDisconnected` can run twice (`OnApplicationStopped` then `OnOBSDisconnected`). This is harmless today, but any command whose `OnDisconnected` isn't idempotent will break.
- `OBSActionExecutor` is 1,165 lines and handles every OBS feature area. Split it by area (outputs, scenes, audio, media) the next time a large change touches it.

---

### 24. SessionGate Depends on an Unreleased Library Fix (Dependency)

**Problem**: obs-websocket-dotnet 5.7.0 raises `Connected` on every `ReIdentify`. `SessionGate` works around this inside the plugin. The library fix is on `mrnil/obs-websocket-dotnet` branch `fix/connected-raised-on-reidentify`, and the upstream PR and release are still pending.

**Fix**: Open the upstream PR and upgrade once it is released. Keep `SessionGate` after upgrading: it also guards against any other source of duplicate `Connected` events, and it is cheap.

---

## Resolved

| # | Area | Issue | Shipped in |
|---|------|-------|------------|
| 1 | Architecture | `CommandRegistry` bypass: `NotifyConnected`/`NotifyDisconnected` weren't routed through the registry | v1.6.0 |
| 2 | Architecture | Scene change bypassed the registry for the sources and scene-audio folders | v1.6.0 |
| 4 | Code Quality | `DoubleTapHelper` race condition and `CancellationTokenSource` leak | v1.6.0 |
| 6 | Code Quality | `CommandCoordinator` given real dispatch with per-command exception isolation | v1.6.2 |
| 7 | Code Quality | `OBSStats` null propagation replaced with `OBSStats.Empty` | v1.6.0 |
| 10 | Feature | `MediaDynamicFolder` now follows input list changes | v1.6.0 |
| 11 | Security | Password field labelled `Password (sensitive)` | v1.6.0 |
| 12 | Risk | net10.0 build verified under a real Logi Plugin Service host | v1.6.2 |
| 13 | Build/DX | `obj/` location no longer depends on how the build is invoked | v1.6.2 |
| 14 | Test Reliability | `DoubleTapHelperTests` fixed-sleep race replaced with a bounded poll | Unreleased (2.0.0) |
| 16 | Performance | Reconnect storm on meter subscription changes; blocking audio requests on the render path | Unreleased (2.0.0) |
| 17 | Performance | Log-review follow-ups: identified-only `IsConnected`, 3s request timeout, non-overlapping stats polls, null-tolerant stats, startup retry, 15s meter lease, log throttling. The remaining part became #18. | Unreleased (2.0.0) |

---

## Summary Table

Open items, in the order to work on them.

| # | Priority | Area | Issue |
|---|----------|------|-------|
| 18 | High | Performance | Source visibility and media status still make blocking OBS requests on redraw (three render paths) |
| 19 | High | Security | Remote OBS password: out of `config.json` now, but still saved in plaintext in the device profile by the Action Editor text box |
| 20 | Medium | Architecture | Services call `OBSStudioForLogiPlugin.Instance`, and the plugin calls command singletons directly |
| 21 | Medium | Correctness | `InputNameChanged` not handled: renamed inputs leave stale folders, cache entries and user-defined buttons |
| 22 | Medium | Docs | AI docs describe `ButtonImageHelper` methods that don't exist (drift brought back by the #3 revert) |
| 3 | Medium | Usability | Name text on scene/source/profile buttons: the fix was reverted without a recorded reason, so check on a device and decide |
| 5 | Medium | Usability | Hard-coded 500ms double-tap window delays every single tap |
| 15 | Medium | Test Reliability | 88 fixed `Thread.Sleep` waits across 12 test files; blocks running tests in CI |
| 8 | Low | Feature | `ProfileListChanged`/`SceneCollectionListChanged` not subscribed |
| 9 | Low | Feature | Recording duration display (parity with stream stats) |
| 23 | Low | Code Quality | `async void` without a catch, blocking `Task.Delay().Wait()`, double `NotifyDisconnected`, 1,165-line `OBSActionExecutor` |
| 24 | Low | Dependency | `Connected`-on-`ReIdentify` library fix still unreleased upstream |
