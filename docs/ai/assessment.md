# Project Assessment — Findings, Tasks & Priorities

## Overview

Reassessed on 2026-10-13 against `main` after v2.0.1, including the unreleased changes listed in `CHANGELOG.md` (#20, #27, #29, #30, #32, greyed-out output buttons, action symbols, the plugin icon fix). It covers code quality, architecture, correctness, usability, build and doc accuracy.

Item numbers never change, because `TODO.md`, commit messages and `CHANGELOG.md` refer to them. This pass re-checked every open item against the code and adds #38–#42. Fixed items are listed under [Resolved](#resolved). Their full write-ups are in `git log -p -- docs/ai/assessment.md`.

At this pass the build has no warnings and all 801 tests pass in about 6s. Line coverage is 49.0% and branch coverage 36.5% overall; the Services layer is at 83.3% line and 61.1% branch (see `test-coverage.md`).

**Open**: #5, #9, #24, #28, #31, #33–#42. The [Summary Table](#summary-table) gives the order to work on them.

---

## High Priority

None open.

---

## Medium Priority

### 31. Scene Buttons Give No Feedback in Studio Mode (Usability)

**Problem**: With studio mode on, `OBSActionExecutor.SetCurrentScene` sets the preview scene. The scene folder, select command and Current Scene display only follow `CurrentProgramSceneChanged`. The plugin never subscribes to `CurrentPreviewSceneChanged` and never calls `GetCurrentPreviewScene`, and `IOBSWebsocket` has neither.

**Impact**: In studio mode, pressing a scene changes nothing on the device until the user transitions, so the press looks as if it didn't register.

**Fix**: Track the preview scene: call `GetCurrentPreviewScene` in the initial load and when studio mode turns on, then follow `CurrentPreviewSceneChanged`. Show it as a second state on the scene buttons, for example an outline for preview and a fill for program.

---

### 28. OBS Groups Aren't Handled (Correctness)

**Problem**: Nothing in `src/` handles OBS groups. `GetSceneItemList` returns a group as a single item without its children, which need `GetGroupSceneItemList`. As a result:

- The Scene Sources folder shows a group as one button, and the sources inside it can't be toggled.
- `OBSWebsocketAdapter.GetAudioInputsNotInAnyScene` doesn't look inside groups. An audio input that is only in a group therefore appears in the "not in any scene" part of Mixer for Scene Audio.

**Impact**: Users who organise scenes with groups can't reach those sources from the device, and the scene audio folder puts their audio inputs in the wrong section.

**Fix**: When building scene source lists and the scene membership set, expand groups (items with `isGroup`) through `GetGroupSceneItemList`. Decide whether the Scene Sources folder shows group children flat or as a sub-level. `SceneItemEnableStateChanged` for a group child reports the group's name as the scene, so check the visibility cache key too.

---

### 5. Double-Tap Delays Every Single Tap (Usability)

**Problem**: `DoubleTapHelper` (`src/Helpers/PressTimingHelper.cs`) still hard-codes a 500ms `DoubleTapThreshold`. Double-tap mutes in `AudioInputDynamicFolderBase` (Mixer for All Audio, Mixer for Scene Audio) and stops in `MediaDynamicFolder`. A single tap only fires after the whole window has passed, so selecting an audio source always lags by 500ms.

**Fix**: Move the threshold to `OBSTimings`, or make it a Plugin Settings option. The per-source **Audio Meter** action and **Toggle Audio Mute (User defined)** already give users single-press mute buttons, so another option is to make the folders single-tap only and drop double-tap.

---

## Low Priority

### 9. Recording Duration Display Missing (Feature Gap)

**Problem**: The Stream Stats folder shows how long the stream has been live, but nothing shows the recording duration. `GetRecordStatus` returns a timecode and the bytes written. The plugin doesn't call it.

**Fix**: Add the recording status to `StatsService`'s poll next to the stream status. Show it in the existing stats folders or in a new recording stats folder.

---

### 24. SessionGate Depends on an Unreleased Library Fix (Dependency)

**Problem**: obs-websocket-dotnet 5.7.0 raises `Connected` on every `ReIdentify`. `SessionGate` works around this in the plugin. The library fix is on the `mrnil/obs-websocket-dotnet` branch `fix/connected-raised-on-reidentify`, and the upstream PR and release are still pending.

**Fix**: Open the upstream PR, and upgrade once it is released. Keep `SessionGate` after upgrading: it is cheap and also covers any other source of duplicate `Connected` events.

---

### 33. Button Calls Pass Through Three Forwarding Layers (Architecture)

**Problem**: An action calls `OBSStudioForLogiPlugin.Instance`, which forwards to `OBSFacade`, which forwards to `OBSWebSocketManager.Actions` (the executor). The plugin class (667 lines) has about 60 one-line forwarders. Most `OBSFacade` members add nothing but a null check, and even that is inconsistent: some use `this._obsManager?.` and others `this._obsManager.IsConnected`.

**Impact**: Every new OBS operation needs the same method in three places, and the plugin class mixes SDK lifecycle with an API surface.

**Fix**: #20 removed the services' upward calls, so actions can now depend on the facade, or on small per-feature interfaces it implements, instead of the plugin class. Keep the plugin class for lifecycle, configuration and event wiring.

---

### 34. Profile, Scene Collection and Scene Select Classes Are Near Copies (Code Quality)

**Problem**: `ProfileSelectCommand` and `SceneCollectionSelectCommand` differ in about 38 lines once the names are swapped. `ProfilesDynamicFolder` and `SceneCollectionsDynamicFolder` differ in about 68. `SceneSelectCommand` and `ScenesDynamicFolder` follow the same pattern.

**Impact**: A fix to one of them, such as the list handling in #8, has to be copied by hand to the others.

**Fix**: Extract one generic base for the select commands and one for the folders, each handling "a list of names, one of them current". Each subclass supplies its icons, group name and switch call.

---

### 35. Repeated OBS String Literals and Hard-Coded Input Kind Lists (Code Quality)

**Problem**:

- `OBSActionExecutor`, `OBSWebsocketAdapter`, `AudioHelpers` and `AudioInputState` each write out the `OBS_MONITORING_TYPE_*` strings. `OBSWebsocketAdapter` also uses the literal `"OBS_MEDIA_STATE_NONE"`, although `MediaInputStates.None` exists.
- `OBSWebsocketAdapter.AudioInputKinds` and the local `mediaKinds` list decide which inputs appear in the audio and media folders. Both are allow-lists that fall behind as OBS adds input kinds: `monitor_capture` is already missing, and plugin-provided kinds never appear.

**Impact**: A typo in a literal fails silently, and inputs of an unlisted kind never appear in the audio and media folders.

**Fix**: Add a `MonitorTypes` constants class next to `MediaInputStates` and use the constants everywhere. For audio, consider replacing the allow-list with a capability check: an input whose `GetInputVolume` succeeds has audio, and the result can be cached per input kind. At the least, keep both lists in one place.

---

### 36. Code Style Rules Aren't Enforced by the Build (Build/DX)

**Problem**: `src/.editorconfig` sets the `var` preferences to `warning`, but neither the project nor `Directory.Build.props` sets `EnforceCodeStyleInBuild`. IDE style rules therefore only show in the editor, and the build reports nothing. At this pass `src/` uses `var` 125 times across 29 files (155 across 33 at the previous pass), against `guidelines.md`. The `.editorconfig` also sits in `src/`, so it doesn't apply to `tests/`.

**Impact**: The style rules `AGENTS.md` calls non-negotiable aren't actually checked, so the code drifts from them.

**Fix**: Set `EnforceCodeStyleInBuild` to `true` (for example in `src/Directory.Build.props`), then fix or suppress what it reports. Decide whether the tests follow the same rules, and if so move or copy the `.editorconfig` up a level. If the `var` rule isn't wanted, relax it in `guidelines.md` instead.

---

### 37. Small Robustness Items (Code Quality)

**Problem**:

- `SceneSwitchAdjustableCommand` waits fixed delays between its steps (`OBSTimings.ProfileSwitchDelay` and `CollectionSwitchDelay`, 1.5s each, then `SceneSwitchDelay`) instead of waiting for `CurrentProfileChanged` and `CurrentSceneCollectionChanged`. A slow collection load can still be running when the scene switch is sent, and a fast one wastes the rest of the delay.
- `OBSWebSocketManager.Dispose` unsubscribes every event except `InputVolumeMeters`, which `SubscribeToVolumeMeters` attaches while a meter is on screen.
- `AudioStatusDisplayCommand` wraps each of its five `ActionImageChanged` calls in an empty `catch { }`, which hides any failure.
- The user-defined actions spell their suffix two ways: "(User defined)" on five actions and "(User Defined)" on Switch to Scene and Media Action.
- `AudioMuteAdjustableCommand`, `AudioMonitoringCycleAdjustableCommand` and `SourceVisibilityAdjustableCommand` wrap their calls in `Task.Run`, although the executor already runs every OBS write on its background runner. That adds a second thread hop, and `guidelines.md` no longer shows the pattern.

**Fix**: In `SceneSwitchAdjustableCommand`, wait for the change events, with a timeout. In `Dispose`, detach the meter handler and clear the owner set. Log in the empty catches, or remove them, since `CommandCoordinator` already isolates exceptions. Pick one spelling (display names only, so existing button assignments are unaffected). Drop the redundant `Task.Run` wrappers.

---

### 38. Remote Host Input Mishandles IPv6 and Host Names (Correctness)

**Problem**: `OBSConnectionSettings.GetWebSocketUrl` builds `ws://{IpAddress}:{Port}` without brackets, so an IPv6 address gives an invalid URL such as `ws://::1:4455`. The `IpAddress` setter only accepts what `IPAddress.TryParse` parses. Anything else, including a host name like `obs-pc.local`, is replaced with `127.0.0.1` and only a log warning records it.

**Impact**: A user who enters a host name, or mistypes an IP address, in Plugin Settings is silently connected to the local OBS instead. Nothing on the device says the value was rejected.

**Fix**: Bracket IPv6 addresses in the URL. Either accept DNS host names (`Uri.CheckHostName`) or reject invalid input visibly, for example on the Connection Status button, instead of falling back to localhost.

---

### 39. Assembly Version Lags the Package Version (Build/DX)

**Problem**: `src/OBSStudioForLogiPlugin.csproj` still has `Version`, `AssemblyVersion` and `FileVersion` at 2.0.0, while `LoupedeckPackage.yaml` is at 2.0.1. Until this pass, step 1 of `release-process.md` only named the YAML file, although `tech.md` said both must change.

**Impact**: The DLL and log output report the wrong version, which makes bug reports harder to match to a release.

**Fix**: Bump the `.csproj` at the next release (`release-process.md` now lists both files), or define the version once, for example as an MSBuild property that the build also writes into the package metadata.

---

### 40. User-Defined Actions Need Names Typed Exactly (Usability)

**Problem**: Switch to Scene, Toggle Source Visibility, the audio actions and Media Action all take scene, source or input names in free-text `ActionEditorTextbox` fields. A typo only shows up as a warning in the log when the button is pressed. `ActionEditorListbox` with `ListboxItemsRequested` already works in this plugin: `MediaActionCommand` and `PluginSettingsCommand` use it for fixed lists. `USER_MANUAL.md` said the SDK can't fill lists at runtime, but nobody has checked that against the SDK docs.

**Impact**: Setting up a user-defined button is error-prone, and a renamed scene or input breaks it without warning (#21 logs a warning but can't rewrite the button).

**Fix**: Check with the SDK docs (`sdk-researcher`) whether `ListboxItemsRequested` runs each time the editor opens. If it does, fill the lists from the scene, input and profile lists the plugin already holds, and keep a textbox only where free text is needed, such as comma-separated sources.

---

### 41. Package Lists Only Loupedeck Devices (Packaging)

**Problem**: `LoupedeckPackage.yaml` lists `supportedDevices: LoupedeckCt, LoupedeckLive`, while the README and `product.md` call the MX Creative Console the primary device. The plugin works on an MX Creative Console today, so the field may only matter for Marketplace listing. That hasn't been checked.

**Fix**: Check the SDK's distribution docs for the device values a plugin should declare (`sdk-researcher`), and list the MX Creative Console if it needs one. Make the README and `INSTALL.md` device lists match.

---

### 42. Tests Don't Run in CI (Build/DX)

**Problem**: CI (`dependency-check.yml`) restores and builds but doesn't run the tests. They were kept out because fire-and-forget tests raced the thread pool. #15 fixed that race. The only tests that still wait on real timers are those for `DoubleTapHelper`, `ConnectionManager` retries and one real connect attempt, and they use bounded polls. `PluginIconTests` is Windows-only.

**Impact**: A pull request, including a Dependabot bump, can merge with failing tests unless someone runs them locally.

**Fix**: Add a `dotnet test` step on a Windows runner, with `-p:PluginDir=` pointing at a scratch folder so the build doesn't try to link the plugin (see `agents.md`). Watch the first few runs for timing failures, then update `AGENTS.md`, `tech.md` and `test-coverage.md`, which say tests run locally only.

---

## Resolved

| # | Area | Issue | Shipped in |
|---|------|-------|------------|
| 1 | Architecture | `NotifyConnected`/`NotifyDisconnected` bypassed `CommandRegistry` | v1.6.0 |
| 2 | Architecture | Scene changes bypassed the registry for the sources and scene audio folders | v1.6.0 |
| 3 | Usability | Scene/source/profile buttons showing no names: not a bug, the SDK labels them (device-checked) | No change needed |
| 4 | Code Quality | `DoubleTapHelper` race condition and `CancellationTokenSource` leak | v1.6.0 |
| 6 | Code Quality | `CommandCoordinator` dispatch with per-command exception isolation | v1.6.2 |
| 7 | Code Quality | `OBSStats.Empty` instead of null propagation | v1.6.0 |
| 8 | Feature | Profile and scene collection list changes followed, including create/delete of a collection | v2.0.0 |
| 10 | Feature | `MediaDynamicFolder` follows input list changes | v1.6.0 |
| 11 | Security | Password field labelled as sensitive | v1.6.0 |
| 12 | Risk | net10.0 build verified under a real Logi Plugin Service host | v1.6.2 |
| 13 | Build/DX | `obj/` location independent of how the build is invoked | v1.6.2 |
| 14 | Test Reliability | `DoubleTapHelperTests` fixed sleeps replaced with a bounded poll | v2.0.0 |
| 15 | Test Reliability | Injectable background runner; 77 fixed sleeps removed from executor tests | v2.0.0 |
| 16 | Performance | Reconnect storm on meter subscription changes; blocking audio requests on the render path | v2.0.0 |
| 17 | Performance | Log-review follow-ups: identified-only `IsConnected`, 3s request timeout, stats and meter fixes | v2.0.0 |
| 18 | Performance | Source visibility and media status served from `KeyedStateCache`; scene source loads off the event thread | v2.0.0 |
| 19 | Security | Remote password moved from `config.json` to the SDK's encrypted plugin settings | v2.0.0 |
| 20 | Architecture | `OBSWebSocketManager` reports through `IOBSEventListener`; no upward calls from the services | Unreleased |
| 21 | Correctness | Input and scene renames move cached state to the new name | v2.0.0 |
| 22 | Docs | AI docs rewritten against the real `ButtonImageHelper`/`ButtonTextRenderer` API | v2.0.0 |
| 23 | Code Quality | `async void` startup guarded; blocking wait after a collection switch removed; one disconnect notification per disconnect | v2.0.0 |
| 25 | Performance | OBS data loaded only while shown; scene audio membership cached | v2.0.0 |
| 26 | Correctness | Initial state load retried while OBS answers "not ready" (207) | v2.0.0 |
| 27 | Correctness | Streaming and recording buttons redraw on state change (device-checked 2026-10-13) | Unreleased |
| 29 | Architecture/Correctness | `ConnectionManager` is the only place that retries, and every attempt re-reads the settings. Device check of a password change still to do (`TODO.md`) | Unreleased |
| 30 | Performance | Source visibility toggle takes two requests instead of four | Unreleased |
| 32 | Code Quality | `OBSActionExecutor` calls go through `RunMutation`/`Query<T>`/`TryQuery<T>` (1,242 → 676 lines) | Unreleased |

---

## Summary Table

Open items, in the order to work on them.

| # | Priority | Area | Issue |
|---|----------|------|-------|
| 31 | Medium | Usability | Scene buttons give no feedback in studio mode (preview scene not tracked) |
| 5 | Medium | Usability | Hard-coded 500ms double-tap window delays every single tap |
| 28 | Medium | Correctness | OBS groups not handled in scene source lists or scene audio membership |
| 38 | Low | Correctness | IPv6 URL unbracketed; host names and typos silently replaced by localhost |
| 42 | Low | Build/DX | Tests don't run in CI |
| 39 | Low | Build/DX | `.csproj` version (2.0.0) lags the package version (2.0.1) |
| 40 | Low | Usability | User-defined actions need exact typed names; listboxes may be possible |
| 9 | Low | Feature | Recording duration display (parity with stream stats) |
| 37 | Low | Code Quality | Fixed delays in `SceneSwitchAdjustableCommand`, meter handler left attached on dispose, empty catches, "User defined" casing, redundant `Task.Run` |
| 35 | Low | Code Quality | Repeated OBS string literals; hard-coded input kind allow-lists |
| 36 | Low | Build/DX | `.editorconfig` style rules not enforced by the build (125 `var` uses in `src/`) |
| 34 | Low | Code Quality | Profile, scene collection and scene select classes are near copies |
| 33 | Low | Architecture | Button calls pass through plugin class, facade and executor forwarders |
| 41 | Low | Packaging | `supportedDevices` lists only Loupedeck devices; check what the MX Creative Console needs |
| 24 | Low | Dependency | `Connected`-on-`ReIdentify` library fix still unreleased upstream |
