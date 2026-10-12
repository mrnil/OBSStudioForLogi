# Project Assessment — Findings, Tasks & Priorities

## Overview

Reassessed on 2026-10-12 against `main` at v2.0.1. It covers code quality, architecture, correctness, usability, performance and doc accuracy. The previous pass (2026-09-28, ahead of v2.0.0) shipped every item marked v2.0.0 below. The first assessment was made against v1.5.1.

Item numbers stay the same between assessments because `TODO.md`, commit messages and `CHANGELOG.md` refer to them. Open items from earlier passes keep their original numbers. The v2.0.0 pass added #18–#26 and this pass adds #27–#37. Fixed items are summarised under [Resolved](#resolved); for their full write-ups, see `git log -p -- docs/ai/assessment.md`.

At this pass the build is clean (no warnings) and all 723 tests pass in about 7s.

**Open**: #5, #9, #20, #24, #27–#31, #33–#37. Priority order is in the [Summary Table](#summary-table).

---

## High Priority

### 27. Streaming and Recording Buttons Never Redraw on State Change (Correctness)

**Problem**: Seven commands never ask the SDK to redraw: Toggle Streaming, Start Streaming, Stop Streaming, Toggle Recording, Start Recording, Stop Recording and Recording Pause. They derive from `ToggleCommandBase` or `StartStopCommandBase`, don't register with `CommandCoordinator`, and never call `ActionImageChanged`. `OBSWebSocketManager.OnStreamStateChanged` and `OnRecordStateChanged` update `OBSActionExecutor`'s state but notify nothing, and there is no `IStreamingAwareCommand` or `IRecordingAwareCommand`. Virtual camera, replay buffer and studio mode follow the right pattern (`IVirtualCameraAwareCommand` etc.), so this is an oversight. Git history shows it has been this way since the commands were added, so it is not a regression. Confirmed: the stale buttons have already been seen on a device (2026-10-12).

**Impact**: The buttons most likely to be on screen during a stream can show the wrong state. The state changes asynchronously after a press (starting → started), and changes made in OBS's own window never reach the buttons, so the icon only corrects itself when the SDK happens to redraw the page.

**Fix**: Add `IStreamingAwareCommand` and `IRecordingAwareCommand`, raise them from the two state handlers (through the plugin class, or through #20's events if that lands first), and have the two base classes or the seven commands register and call `ActionImageChanged`. Check on a device afterwards: start and stop a stream and a recording from both the device and OBS's own window, pause and resume a recording, and confirm the buttons follow, including the starting and stopping states.

---

## Medium Priority

### 29. Two Reconnect Loops, One Using Stale Credentials (Architecture/Correctness)

**Problem**: Connections are retried in two places. `ConnectionManager` retries the initial connection every `ConnectRetryDelay` while the local port is closed. `OBSWebSocketManager` has its own `_reconnectTimer` with `ReconnectionStrategy` backoff for dropped connections. Within the second loop:

- `OnReconnectTimer` calls the library's fire-and-forget `ConnectAsync` and then checks `IsConnected` straight away, which is almost always still false, so it schedules another attempt every time. `OnConnected` stopping the timer is what actually ends the loop.
- `_connectingInProgress` and `_shouldReconnect` are plain `Boolean` fields written and read from the timer, socket and caller threads.
- `GetReconnectDelay(Int32 attempt)` ignores its argument.
- The timer retries with the stored `_lastUrl` and `_lastPassword`. It never re-reads OBS's `config.json`, so if the user changes the WebSocket password or port in OBS while the plugin is running, the plugin retries the old values until Reconnect is pressed (not yet verified on a device).

**Impact**: Reconnect behaviour is hard to reason about and test, and a password or port change in OBS needs a manual reconnect.

**Fix**: Make `ConnectionManager` the only owner of retries. It already knows how to read the local config, so each attempt picks up current settings. Have `OBSWebSocketManager` only report `ConnectionLost`, and make the remaining connection-state fields `volatile` or `Interlocked`. Drop the unused `attempt` parameter.

---

### 31. Scene Buttons Give No Feedback in Studio Mode (Usability)

**Problem**: With studio mode on, `OBSActionExecutor.SetCurrentScene` sets the preview scene, but the scene folder, select command and Current Scene display follow only `CurrentProgramSceneChanged`. The plugin doesn't subscribe to `CurrentPreviewSceneChanged` or call `GetCurrentPreviewScene`.

**Impact**: Pressing a scene in studio mode changes nothing on the device until the user transitions, so it looks as if the press didn't register.

**Fix**: Track the preview scene (`GetCurrentPreviewScene` in the initial load and on studio mode enable, `CurrentPreviewSceneChanged` after that) and show it as a second state on the scene buttons, for example an outline for preview and a fill for program. This is the same work as the "Get current preview scene" and `CurrentPreviewSceneChanged` items in `TODO.md`.

---

### 28. OBS Groups Aren't Handled (Correctness)

**Problem**: Nothing in `src/` handles OBS groups. `GetSceneItemList` returns a group as a single item and doesn't list its children, which need `GetGroupSceneItemList`. As a result:

- The Scene Sources folder shows a group as one button, and sources inside it can't be toggled.
- `OBSWebsocketAdapter.GetAudioInputsNotInAnyScene` doesn't look inside groups, so an audio input that is only in a group is listed in the "not in any scene" part of Mixer for Scene Audio.

**Impact**: Users who organise scenes with groups can't reach those sources from the device, and the scene audio folder misplaces their audio inputs.

**Fix**: Expand groups (items with `isGroup`) through `GetGroupSceneItemList` when building scene source lists and the scene membership set. Decide whether the Scene Sources folder shows group children flat or as a sub-level. Note that `SceneItemEnableStateChanged` for a group child reports the group's name as the scene, so the visibility cache key needs checking too.

---

### 20. Services and Plugin Class Still Reach Into Singletons (Architecture)

**Problem**: #1 and #2 routed command notifications through `CommandCoordinator`, but the same bypass remains in other places:

- The service layer depends upward on the plugin class. `OBSWebSocketManager` calls `OBSStudioForLogiPlugin.Instance?.On…` about a dozen times (profile, scene collection, scene, virtual camera, replay buffer and media events). `OBSActionExecutor` calls it for source visibility changes, and `StatsService`'s default constructor calls it for its stats providers.
- `OBSStudioForLogiPlugin` calls specific commands directly: `MediaDynamicFolder.Instance`, `StatsDisplay.Instance`, `StatsDynamicFolder.Instance`, `StreamStatsDynamicFolder.Instance` and `ConnectionStatusDisplay.Instance`.
- `OBSWebSocketManager` holds the concrete library `OBSWebsocket`, not `IOBSWebsocket`. It subscribes to the library's events directly, and its initial load and refreshes call the library directly too (`_obs.GetProfileList()`, `_obs.GetSceneList()`, `_obs.GetSceneItemList()` and others), so those paths can't be driven by the mocked interface the rest of the tests use. At the 2026-10-12 pass, `src/Services` referenced `OBSStudioForLogiPlugin.Instance` 35 times.

**Impact**: Services can't be tested without the static plugin instance. That is why `OBSWebSocketManager` event handling has so little coverage. Adding a new stats or media display also means editing the plugin class instead of implementing an interface, which is the same maintenance trap #1 described.

**Fix**: Have `OBSWebSocketManager` raise its own events, or take a callback interface in its constructor, and let the plugin class subscribe and forward them to `CommandCoordinator`. Add `IStatsAwareCommand` and `IMediaStateAwareCommand` (or similar) so the displays and the media folder receive updates through the registry. Put the events the manager needs on `IOBSWebsocket` (or a separate event-source interface) and route the manager's direct library calls through it, so the event handlers can be tested with the existing mocks. Doing this together with #27 and #32 avoids touching the same code three times.

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

### 30. Source Visibility Toggle Costs Four OBS Requests (Performance)

**Problem**: `OBSWebsocketAdapter.GetSceneItemEnabled` and `SetSceneItemEnabled` each call `GetSceneItemList` to turn the source name into a scene item ID, then make the real request. `OBSActionExecutor.ToggleSourceVisibility` reads and then sets, so one press is four round trips. Cache fetches through `TryGetSceneItemEnabled` cost two.

**Impact**: Toggles are slower than they need to be, especially on a remote OBS or a scene with many items.

**Fix**: Resolve the ID with `GetSceneItemId` (in obs-websocket-dotnet 5.7.0), or keep the IDs from the `GetSceneItemList` call the scene sources load already makes. The toggle can also use the cached state from `KeyedStateCache` instead of reading it again.

---

### 33. Button Calls Pass Through Three Forwarding Layers (Architecture)

**Problem**: An action calls `OBSStudioForLogiPlugin.Instance`, which forwards to `OBSFacade`, which forwards to `OBSWebSocketManager.Actions` (the executor). The plugin class has about 60 one-line forwarders, and most `OBSFacade` members only add a null check.

**Impact**: Every new OBS operation needs the same method added in three places, and the plugin class mixes SDK lifecycle with an API surface.

**Fix**: Once #20 lands, let actions depend on the facade, or on small per-feature interfaces it implements, instead of the plugin class. Keep the plugin class for lifecycle, configuration and event wiring.

---

### 34. Profile, Scene Collection and Scene Select Classes Are Near Copies (Code Quality)

**Problem**: `ProfileSelectCommand` and `SceneCollectionSelectCommand` differ in about 38 lines once the names are swapped, and `ProfilesDynamicFolder` and `SceneCollectionsDynamicFolder` in about 68. `SceneSelectCommand` and `ScenesDynamicFolder` follow the same shape.

**Impact**: A fix to one (for example the list handling in #8) has to be repeated by hand in the others.

**Fix**: Extract a generic base for "a list of names with one current" for the select commands and one for the folders, with the icons, group name and switch call supplied by each subclass.

---

### 35. Repeated OBS String Literals and Hard-Coded Input Kind Lists (Code Quality)

**Problem**:

- The `OBS_MONITORING_TYPE_*` strings are written out in `OBSActionExecutor`, `OBSWebsocketAdapter`, `AudioHelpers` and `AudioInputState`. `OBSActionExecutor` and `OBSWebsocketAdapter` also use the literal `"OBS_MEDIA_STATE_NONE"` although `MediaInputStates.None` exists.
- `OBSWebsocketAdapter.AudioInputKinds` and the local `mediaKinds` list decide which inputs appear in the audio and media folders. They are allow-lists that drift as OBS adds input kinds: `monitor_capture` is already missing (see `protocol-gap-analysis.md` §4), and plugin-provided kinds never appear.

**Impact**: A typo in a literal fails silently, and inputs of an unlisted kind are invisible to the audio and media folders.

**Fix**: Add a `MonitorTypes` constants class next to `MediaInputStates` and use the constants everywhere. For audio, consider replacing the allow-list with a capability check (an input whose `GetInputVolume` succeeds has audio), cached per input kind, or at least move both lists into one place.

---

### 36. Code Style Rules Aren't Enforced by the Build (Build/DX)

**Problem**: `src/.editorconfig` sets the `var` preferences to `warning`, but `EnforceCodeStyleInBuild` is not set in the project or `Directory.Build.props`, so IDE-style rules only show in the editor and the build reports nothing. At the 2026-10-12 pass, `src/` used `var` 155 times across 33 files, against the guideline in `guidelines.md`. The `.editorconfig` also sits in `src/`, so it doesn't apply to `tests/`.

**Impact**: The "non-negotiable" style rules in `AGENTS.md` aren't actually checked, so they drift.

**Fix**: Set `EnforceCodeStyleInBuild` to `true` (for example in `src/Directory.Build.props`), fix or suppress what it reports, and decide whether the tests follow the same rules (move or copy the `.editorconfig` up a level). Alternatively, relax the `var` rule in `guidelines.md` if it isn't wanted.

---

### 37. Small Robustness and Doc Items (Code Quality/Docs)

**Problem**:

- `SceneSwitchAdjustableCommand` waits fixed delays between its steps (`OBSTimings.ProfileSwitchDelay` and `CollectionSwitchDelay`, 1.5s each, then `SceneSwitchDelay`) instead of waiting for `CurrentProfileChanged` and `CurrentSceneCollectionChanged`. A slow collection load can still be in progress when the scene switch is sent, and a fast one wastes the rest of the delay.
- `OBSWebSocketManager.Dispose` unsubscribes every event except `InputVolumeMeters`, which `SubscribeToVolumeMeters` attaches while a meter is shown.
- `AudioStatusDisplayCommand` wraps each `ActionImageChanged` call in an empty `catch { }`, which hides any failure.
- `product.md` still gives the current version as 1.6.1; the package is at 2.0.1.

**Fix**: Wait for the change events (with a timeout) in `SceneSwitchAdjustableCommand`. Detach the meter handler and clear the owner set in `Dispose`. Log in the empty catches, or remove them, since `CommandCoordinator` already isolates exceptions. Update `product.md`.

---

## Resolved

| # | Area | Issue | Shipped in |
|---|------|-------|------------|
| 1 | Architecture | `CommandRegistry` bypass: `NotifyConnected`/`NotifyDisconnected` weren't routed through the registry | v1.6.0 |
| 2 | Architecture | Scene change bypassed the registry for the sources and scene-audio folders | v1.6.0 |
| 3 | Usability | Scene/source/profile buttons showing no names: not a bug. The SDK labels each button with its display name, which these folders leave to the SDK. The reverted fix (`5eab775`, reverted in `4e0b0d3`) also drew names into the images, most likely showing them twice, as the Audio Volume folder did until it was fixed. Device-checked 2026-09-29: the Scenes, Scene Sources and Profiles folders show each name | No change needed |
| 4 | Code Quality | `DoubleTapHelper` race condition and `CancellationTokenSource` leak | v1.6.0 |
| 6 | Code Quality | `CommandCoordinator` given real dispatch with per-command exception isolation | v1.6.2 |
| 7 | Code Quality | `OBSStats` null propagation replaced with `OBSStats.Empty` | v1.6.0 |
| 8 | Feature | `ProfileListChanged` and `SceneCollectionListChanged` handled: the list from the event goes straight to the folders and select commands, and if the current profile or collection is missing from it (it was renamed) the current one is read again first. The device check found OBS sends no `SceneCollectionListChanged` when a collection is created or deleted, only `CurrentSceneCollectionChanged`, so the collection list is now also re-read on every collection switch (keeping the last list if the read fails). Device-checked 2026-09-29, including collection create and delete after the fix | v2.0.0 |
| 10 | Feature | `MediaDynamicFolder` now follows input list changes | v1.6.0 |
| 11 | Security | Password field labelled `Password (sensitive)` | v1.6.0 |
| 12 | Risk | net10.0 build verified under a real Logi Plugin Service host | v1.6.2 |
| 13 | Build/DX | `obj/` location no longer depends on how the build is invoked | v1.6.2 |
| 14 | Test Reliability | `DoubleTapHelperTests` fixed-sleep race replaced with a bounded poll | v2.0.0 |
| 15 | Test Reliability | `OBSActionExecutor` takes an injectable background runner, so its tests run mutations inline; 77 fixed sleeps removed from 9 test files. The 11 left wait on real timers (`DoubleTapHelper`, `ConnectionManager` retries, one real connect attempt) | v2.0.0 |
| 16 | Performance | Reconnect storm on meter subscription changes; blocking audio requests on the render path | v2.0.0 |
| 17 | Performance | Log-review follow-ups: identified-only `IsConnected`, 3s request timeout, non-overlapping stats polls, null-tolerant stats, startup retry, 15s meter lease, log throttling. The remaining part became #18. | v2.0.0 |
| 18 | Performance | Source visibility and media status served from `KeyedStateCache` instead of blocking OBS requests on redraw; scene source lists load off the OBS event thread through `SceneSourcesLoader`, latest load wins. Device-checked 2026-09-29 | v2.0.0 |
| 19 | Security | Remote OBS password moved out of `config.json` into the SDK's encrypted plugin settings, with migration. The Action Editor field is persisted in plaintext in the device profile, so an empty field now keeps the saved password, users are told to clear it after saving, and a "Clear Saved Password" checkbox removes it. A value left in the field still sits in the profile. Device-checked 2026-09-29 | v2.0.0 |
| 21 | Correctness | `InputNameChanged` and `SceneNameChanged` handled: cached audio, meter, media and visibility state moves to the new name, the dial selection follows the input, the current scene is updated, and the input and scene lists refresh in the background. User-defined buttons can't be rewritten, so a rename logs a warning naming the new name. Input create/remove/rename now refresh the list off the OBS event thread. Device-checked 2026-09-29 | v2.0.0 |
| 22 | Docs | `guidelines.md`, `image-rendering-simplified.md`, `sdk-quick-reference.md` and `structure.md` rewritten against the real `ButtonImageHelper`/`ButtonTextRenderer` API; the non-existent `StateIcon`/`StateText`/`TextWithIcon`/`StateTextWithIcon` examples are gone | v2.0.0 |
| 25 | Performance | OBS data loaded on demand. Scene source lists load only while the Scene Sources or Mixer for Scene Audio folder is open, which shows loading tiles until they arrive. The "audio inputs not in any scene" list (one request per scene) is cached in `OBSWebSocketManager.AudioInputMembership` and cleared by the events that change it, so a scene change costs one request instead of about N+5. Stats poll only while a stats folder is open or the summary button is visible, with an immediate first poll. Device-checked 2026-09-29 (`docs/device-checks/on-demand-loading.md`), including the loading tiles | v2.0.0 |
| 23 | Code Quality | `OnApplicationStarted` (`async void`) catches and logs exceptions. The blocking `Task.Delay(100).Wait()` after a scene collection switch is gone: obs-websocket sends `CurrentSceneCollectionChanged` only after the collection has loaded, so the scene list is read straight away. `CommandCoordinator` notifies commands of a disconnect once until the next connect. Splitting `OBSActionExecutor` moved to `TODO.md` (Architecture, Deferred) | v2.0.0 |
| 26 | Correctness | Found in the #25 device check: OBS accepts connections while starting and answers "not ready" (207) until its scene collection loads, so the one-shot initial state load failed and the input list, profiles and studio mode stayed unloaded until something changed in OBS. `InitialStateLoader` now retries every 500ms (up to 30s) while OBS isn't ready and the connection is unchanged, and stats polling starts after the load instead of at connect, so it no longer logs errors while OBS starts. Device-checked 2026-09-29: loaded on the third attempt, about 1s after connecting | v2.0.0 |
| 32 | Code Quality | `OBSActionExecutor` repeated the connected/guard/try/log steps in about 25 methods. They now go through three private helpers: `RunMutation` (background runner, "not connected" and refusal warnings, error logging), `Query<T>` (safe fallback) and `TryQuery<T>` (reports failure, for the render caches). The getters with no production callers left (`GetSceneItemEnabled`, `GetMediaInputStatus`, `GetInputVolume`, `GetInputAudioMonitorType`) are gone, since rendering reads the caches; their `TryGet*` versions stay. 1,242 → 657 lines | Unreleased |

---

## Summary Table

Open items, in the order to work on them.

| # | Priority | Area | Issue |
|---|----------|------|-------|
| 27 | High | Correctness | Streaming and recording buttons never redraw on state change (seen on a device) |
| 20 | Medium | Architecture | Services call `OBSStudioForLogiPlugin.Instance`, the plugin calls command singletons directly, and `OBSWebSocketManager` bypasses `IOBSWebsocket` |
| 29 | Medium | Architecture | Two reconnect loops; the timer retries stale credentials and has unsynchronised state |
| 31 | Medium | Usability | Scene buttons give no feedback in studio mode (preview scene not tracked) |
| 5 | Medium | Usability | Hard-coded 500ms double-tap window delays every single tap |
| 28 | Medium | Correctness | OBS groups not handled in scene source lists or scene audio membership |
| 30 | Low | Performance | A source visibility toggle costs four OBS requests |
| 9 | Low | Feature | Recording duration display (parity with stream stats) |
| 33 | Low | Architecture | Button calls pass through plugin class, facade and executor forwarders |
| 34 | Low | Code Quality | Profile, scene collection and scene select classes are near copies |
| 35 | Low | Code Quality | Repeated OBS string literals; hard-coded input kind allow-lists |
| 36 | Low | Build/DX | `.editorconfig` style rules not enforced by the build (155 `var` uses in `src/`) |
| 37 | Low | Code Quality/Docs | Fixed delays in `SceneSwitchAdjustableCommand`, meter handler left attached on dispose, empty catches, stale `product.md` version |
| 24 | Low | Dependency | `Connected`-on-`ReIdentify` library fix still unreleased upstream |
