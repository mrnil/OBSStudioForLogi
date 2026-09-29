# Project Assessment — Findings, Tasks & Priorities

## Overview

Reassessed on 2026-09-28 against `main`: version 2.0.0, not yet tagged. The last release is v1.6.2. It covers code quality, architecture, security, usability, test reliability and doc accuracy. The first assessment was made against v1.5.1.

Item numbers stay the same between assessments because `TODO.md`, commit messages and `CHANGELOG.md` refer to them. Open items from earlier passes keep their original numbers. New findings start at #18. Fixed items are summarised under [Resolved](#resolved); for their full write-ups, see `git log -p -- docs/ai/assessment.md`.

**Open**: #3, #5, #9, #20, #24. Priority order is in the [Summary Table](#summary-table).

---

## High Priority

None open.

---

## Medium Priority

### 20. Services and Plugin Class Still Reach Into Singletons (Architecture)

**Problem**: #1 and #2 routed command notifications through `CommandCoordinator`, but the same bypass remains in other places:

- The service layer depends upward on the plugin class. `OBSWebSocketManager` calls `OBSStudioForLogiPlugin.Instance?.On…` about a dozen times (profile, scene collection, scene, virtual camera, replay buffer and media events). `OBSActionExecutor` calls it for source visibility changes, and `StatsService`'s default constructor calls it for its stats providers.
- `OBSStudioForLogiPlugin` calls specific commands directly: `MediaDynamicFolder.Instance`, `StatsDisplay.Instance`, `StatsDynamicFolder.Instance`, `StreamStatsDynamicFolder.Instance` and `ConnectionStatusDisplay.Instance`.

**Impact**: Services can't be tested without the static plugin instance. That is why `OBSWebSocketManager` event handling has so little coverage. Adding a new stats or media display also means editing the plugin class instead of implementing an interface, which is the same maintenance trap #1 described.

**Fix**: Have `OBSWebSocketManager` raise its own events, or take a callback interface in its constructor, and let the plugin class subscribe and forward them to `CommandCoordinator`. Add `IStatsAwareCommand` and `IMediaStateAwareCommand` (or similar) so the displays and the media folder receive updates through the registry.

---

### 3. Scene/Source/Profile Buttons Show No Text (Usability): Needs Decision

**Status**: The fix, `5eab775`, rendered item names with `ButtonTextRenderer.RenderTextWithBorder`. It was reverted in `4e0b0d3` and no reason was recorded. Since then the icon restyle added a tick for the selected scene, profile and scene collection, so the selected/unselected state is clearer. It is still unknown whether names are readable on the device.

**Next step**: Check on a device whether the SDK draws each item's display name under the icon in `ScenesDynamicFolder`, `SourcesDynamicFolder` and `ProfilesDynamicFolder`. The changelog notes that display names are drawn as a native overlay in some cases. If names already show, close this item and record why. If they don't, find out what caused the revert (duplicated text? clashes with the icon style guide?) before trying again. `SceneSelectCommand`, `ProfileSelectCommand` and `SceneCollectionSelectCommand` would need the same answer.

---

### 5. Double-Tap Interaction Unreliable on MX Console (Usability)

**Problem**: `DoubleTapHelper` (`src/Helpers/PressTimingHelper.cs`) still uses a hard-coded 500ms `DoubleTapThreshold` constant. Double-tap drives mute in `AudioInputDynamicFolderBase` and stop in `MediaDynamicFolder`. A single tap has to wait out the whole window before it fires, so selecting an audio source always lags by 500ms.

**Recommendation**: Move the threshold to `OBSTimings`, or make it a Plugin Settings option. Since then, the per-source **Audio Meter** action and the user-defined `AudioMuteAdjustableCommand` have given users single-press mute buttons. Consider making the folders single-tap only and dropping double-tap.

---

## Low Priority

### 9. Recording Duration Display Missing (Feature Gap)

**Problem**: The Stream Stats folder shows how long the stream has been live, but nothing shows the recording duration. `GetRecordStatus` returns a timecode and the bytes written.

**Fix**: Add the recording duration to `StatsService`'s poll, next to the stream status, and show it in a `RecordingStatsDynamicFolder` or add it to the existing stats folder.

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
| 8 | Feature | `ProfileListChanged` and `SceneCollectionListChanged` handled: the list from the event goes straight to the folders and select commands, and if the current profile or collection is missing from it (it was renamed) the current one is read again first. Needs a device check | Unreleased (2.0.0) |
| 10 | Feature | `MediaDynamicFolder` now follows input list changes | v1.6.0 |
| 11 | Security | Password field labelled `Password (sensitive)` | v1.6.0 |
| 12 | Risk | net10.0 build verified under a real Logi Plugin Service host | v1.6.2 |
| 13 | Build/DX | `obj/` location no longer depends on how the build is invoked | v1.6.2 |
| 14 | Test Reliability | `DoubleTapHelperTests` fixed-sleep race replaced with a bounded poll | Unreleased (2.0.0) |
| 15 | Test Reliability | `OBSActionExecutor` takes an injectable background runner, so its tests run mutations inline; 77 fixed sleeps removed from 9 test files. The 11 left wait on real timers (`DoubleTapHelper`, `ConnectionManager` retries, one real connect attempt) | Unreleased (2.0.0) |
| 16 | Performance | Reconnect storm on meter subscription changes; blocking audio requests on the render path | Unreleased (2.0.0) |
| 17 | Performance | Log-review follow-ups: identified-only `IsConnected`, 3s request timeout, non-overlapping stats polls, null-tolerant stats, startup retry, 15s meter lease, log throttling. The remaining part became #18. | Unreleased (2.0.0) |
| 18 | Performance | Source visibility and media status served from `KeyedStateCache` instead of blocking OBS requests on redraw; scene source lists load off the OBS event thread through `SceneSourcesLoader`, latest load wins. Device-checked 2026-09-29 | Unreleased (2.0.0) |
| 19 | Security | Remote OBS password moved out of `config.json` into the SDK's encrypted plugin settings, with migration. The Action Editor field is persisted in plaintext in the device profile, so an empty field now keeps the saved password, users are told to clear it after saving, and a "Clear Saved Password" checkbox removes it. A value left in the field still sits in the profile. Device-checked 2026-09-29 | Unreleased (2.0.0) |
| 21 | Correctness | `InputNameChanged` and `SceneNameChanged` handled: cached audio, meter, media and visibility state moves to the new name, the dial selection follows the input, the current scene is updated, and the input and scene lists refresh in the background. User-defined buttons can't be rewritten, so a rename logs a warning naming the new name. Input create/remove/rename now refresh the list off the OBS event thread. Device-checked 2026-09-29 | Unreleased (2.0.0) |
| 22 | Docs | `guidelines.md`, `image-rendering-simplified.md`, `sdk-quick-reference.md` and `structure.md` rewritten against the real `ButtonImageHelper`/`ButtonTextRenderer` API; the non-existent `StateIcon`/`StateText`/`TextWithIcon`/`StateTextWithIcon` examples are gone | Unreleased (2.0.0) |
| 25 | Performance | OBS data loaded on demand. Scene source lists load only while the Scene Sources or Mixer for Scene Audio folder is open, which shows loading tiles until they arrive. The "audio inputs not in any scene" list (one request per scene) is cached in `OBSWebSocketManager.AudioInputMembership` and cleared by the events that change it, so a scene change costs one request instead of about N+5. Stats poll only while a stats folder is open or the summary button is visible, with an immediate first poll. Device-checked 2026-09-29 (`docs/device-checks/on-demand-loading.md`), including the loading tiles | Unreleased (2.0.0) |
| 23 | Code Quality | `OnApplicationStarted` (`async void`) catches and logs exceptions. The blocking `Task.Delay(100).Wait()` after a scene collection switch is gone: obs-websocket sends `CurrentSceneCollectionChanged` only after the collection has loaded, so the scene list is read straight away. `CommandCoordinator` notifies commands of a disconnect once until the next connect. Splitting `OBSActionExecutor` moved to `TODO.md` (Architecture, Deferred) | Unreleased (2.0.0) |
| 26 | Correctness | Found in the #25 device check: OBS accepts connections while starting and answers "not ready" (207) until its scene collection loads, so the one-shot initial state load failed and the input list, profiles and studio mode stayed unloaded until something changed in OBS. `InitialStateLoader` now retries every 500ms (up to 30s) while OBS isn't ready and the connection is unchanged, and stats polling starts after the load instead of at connect, so it no longer logs errors while OBS starts. Device-checked 2026-09-29: loaded on the third attempt, about 1s after connecting | Unreleased (2.0.0) |

---

## Summary Table

Open items, in the order to work on them.

| # | Priority | Area | Issue |
|---|----------|------|-------|
| 20 | Medium | Architecture | Services call `OBSStudioForLogiPlugin.Instance`, and the plugin calls command singletons directly |
| 3 | Medium | Usability | Name text on scene/source/profile buttons: the fix was reverted without a recorded reason, so check on a device and decide |
| 5 | Medium | Usability | Hard-coded 500ms double-tap window delays every single tap |
| 9 | Low | Feature | Recording duration display (parity with stream stats) |
| 24 | Low | Dependency | `Connected`-on-`ReIdentify` library fix still unreleased upstream |
