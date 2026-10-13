# TODO

## High Priority

### Assessment: Streaming and Recording Buttons Never Redraw (#27) ✅ Done

- [x] ~~Add `IStreamingAwareCommand` and `IRecordingAwareCommand`, raise them from `OnStreamStateChanged`/`OnRecordStateChanged`, and have the seven `ToggleCommandBase`/`StartStopCommandBase` commands register and call `ActionImageChanged`~~ ✅ Done
- [x] ~~Check on a device after the fix: start/stop a stream and a recording from the device and from OBS's own window, pause/resume a recording, and confirm the buttons follow, including the starting/stopping states~~ ✅ Done 2026-10-13: the buttons follow changes made in OBS

### Assessment: Source Visibility and Media Status Query OBS on Redraw (#18) ✅ Done

- [x] ~~Cache source visibility per (scene, source), kept current from `SceneItemEnableStateChanged`; serve `SourcesDynamicFolder` and `SourceVisibilityAdjustableCommand` renders from it~~ ✅ Done
- [x] ~~Cache media status, kept current from the `MediaInputPlayback*` events; serve `MediaDynamicFolder` renders from it~~ ✅ Done
- [x] ~~Move `UpdateSourcesForScene` (about 10 requests per scene change) off the OBS event thread~~ ✅ Done
- [x] ~~Check on a device: media tiles follow play/pause/stop from both the device and OBS's own controls~~ ✅ Done 2026-09-29

### Assessment: Remote OBS Password Stored in Plaintext (#19) ✅ Done

- [x] ~~Keep `RemotePassword` out of `config.json`: store it through the SDK's encrypted plugin settings and migrate existing plaintext values~~ ✅ Done
- [x] ~~Stop the password staying in the device profile: an empty Password field keeps the saved password, the action tells users to clear it after saving, and a "Clear Saved Password" checkbox removes it~~ ✅ Done
- [x] ~~Check on a device: save a password, clear the field and save again (still connects), then use Clear Saved Password (connects without one)~~ ✅ Done 2026-09-29

### Assessment: Verify net10.0 Runtime Compatibility (#12) ✅ Done

- [x] ~~Verified net10.0 build runs under a real Logi Plugin Service install, connects to OBS, and responds to button presses~~ ✅ Done

## Medium Priority

### Assessment: Services and Plugin Class Reach Into Singletons (#20) ✅ Done

- [x] ~~Have `OBSWebSocketManager` raise events (or take a callback interface) instead of calling `OBSStudioForLogiPlugin.Instance`~~ ✅ Done — `IOBSEventListener`, implemented by the plugin class
- [x] ~~Route stats and media updates through `CommandCoordinator` instead of `StatsDisplay.Instance`, `MediaDynamicFolder.Instance` and similar~~ ✅ Done — `IStatsAwareCommand`, `IMediaStateAwareCommand`, `IConnectionStatusAwareCommand`
- [x] ~~Put the events `OBSWebSocketManager` needs on `IOBSWebsocket` (or an event-source interface) and route its direct library calls (`_obs.GetProfileList()`, `_obs.GetSceneList()`, ...) through it, then add tests for the event handlers~~ ✅ Done — `OBSWebSocketManagerEventHandlingTests`

### Assessment: Two Reconnect Loops (#29) — fixed, device check to do

- [x] ~~Make `ConnectionManager` the only owner of connection retries; `OBSWebSocketManager` just reports `ConnectionLost`~~ ✅ Done
- [x] ~~Re-read the local OBS config on each retry, so a password or port change in OBS doesn't need a manual reconnect~~ ✅ Done — every retry goes through `ConnectAsync`, which reads the settings afresh
- [x] ~~Make `_connectingInProgress`/`_shouldReconnect` thread-safe and drop the unused `attempt` parameter of `GetReconnectDelay`~~ ✅ Done — the flags are `volatile`, `ReconnectionStrategy` counts atomically, and `GetReconnectDelay` went with the manager's timer
- [ ] Check on a device: change the WebSocket password in OBS while connected (OBS drops the connection), and confirm the plugin reconnects with the new password without pressing Reconnect; then quit and restart OBS and confirm it reconnects
- [ ] Consider: every reconnect waits `OBSTimings.ConnectionDelay` (2s) before connecting, which was meant for OBS starting up; a reconnect to an OBS that is still running could skip it

### Assessment: No Scene Feedback in Studio Mode (#31)

- [ ] Track the preview scene: `GetCurrentPreviewScene` in the initial load and when studio mode turns on, `CurrentPreviewSceneChanged` after that
- [ ] Show preview and program as separate states on the scene folder, select command and Current Scene display
- [ ] Check on a device: in studio mode, press a scene (preview state shows), then transition (program state moves)

### Assessment: OBS Groups Not Handled (#28)

- [ ] Expand groups through `GetGroupSceneItemList` when building scene source lists, and decide between a flat list and a sub-level in the Scene Sources folder
- [ ] Include group children in `GetAudioInputsNotInAnyScene`'s membership set
- [ ] Check that visibility caching works for group children (`SceneItemEnableStateChanged` reports the group as the scene)

### Assessment: Renamed Inputs Go Stale (#21) ✅ Done

- [x] ~~Subscribe to `InputNameChanged`, re-key `AudioStateCache` and meter entries, push the refreshed input list~~ ✅ Done
- [x] ~~Check whether `SceneListChanged` fires on scene rename; handle `SceneNameChanged` if not~~ ✅ Done — `SceneNameChanged` is handled directly, so it no longer matters whether OBS also sends `SceneListChanged`
- [x] ~~Check on a device: rename an audio input (selected on the dial), a media source and the current scene in OBS, and confirm the folders, selection and scene tick follow~~ ✅ Done 2026-09-29

### Assessment: AI Docs Describe Non-Existent `ButtonImageHelper` Methods (#22) ✅ Done

- [x] ~~Fix `StateIcon`/`StateText`/`TextWithIcon`/`StateTextWithIcon` examples in `guidelines.md`, `image-rendering-simplified.md` and `sdk-quick-reference.md`~~ ✅ Done

### Assessment: OBS Data Loaded Whether or Not Anything Shows It (#25) ✅ Done

- [x] ~~Load scene source lists only while the Scene Sources or Mixer for Scene Audio folder is open, with loading tiles until they arrive~~ ✅ Done
- [x] ~~Cache the "audio inputs not in any scene" list and clear it on the events that change it~~ ✅ Done
- [x] ~~Poll stats only while a stats folder is open or the summary button is visible, polling straight away for the first viewer~~ ✅ Done
- [x] ~~Check on a device: follow `docs/device-checks/on-demand-loading.md`~~ ✅ Done 2026-09-29 on a Loupedeck device: loads skipped with no folder open, cache hits after the first load, cache cleared on source add/remove, folder reloads after an OBS restart, stats poll only while shown
- [x] ~~Check how the loading tiles look~~ ✅ Done 2026-09-29: the message sits right of the Back button and reads correctly across both tiles
- [ ] Consider: `MediaDynamicFolder` still requests the full input list on every input list change (the initial state load now reads the current scene with `GetCurrentProgramScene`, so it requests the scene list once)

### Assessment: Initial State Not Loaded When OBS Is Still Starting (#26) ✅ Done

- [x] ~~Retry the initial state load while OBS answers "not ready" (207), for as long as the connection is unchanged~~ ✅ Done
- [x] ~~Start stats polling after the initial state load instead of at connect~~ ✅ Done
- [x] ~~Check on a device: restart OBS with the plugin running~~ ✅ Done 2026-09-29: OBS answered "not ready" twice, the load succeeded on attempt 3 about 1s after connecting, inputs, profiles and studio mode loaded, and stats started afterwards with no errors

### Assessment: Scene/Source/Profile Buttons Show No Text (#3) ✅ Done

- [x] ~~Find out whether the SDK draws display names over button images~~ ✅ Done 2026-09-29 — it does: the Audio Volume folder showed its text twice (fixed), which most likely explains the revert
- [x] ~~Check on a device that the Scenes, Scene Sources and Profiles folder buttons are labelled with their names, then close #3~~ ✅ Done 2026-09-29 — they are, so #3 is closed with no change

### Assessment: Double-Tap Window (#5)

- [ ] Move the 500ms `DoubleTapThreshold` into `OBSTimings` or Plugin Settings, or drop double-tap in favour of dedicated mute buttons

### Assessment: CommandCoordinator Has No Error Isolation (#6) ✅ Done

- [x] ~~Add per-command exception isolation in each `Notify*` method so one failing command does not break others~~ ✅ Done

### Assessment: OBSStats Null Propagation (#7) ✅ Done

- [x] ~~Return `OBSStats.Empty` / `OBSStreamStats.Empty` instead of `null` and remove the null guards from the stats displays~~ ✅ Done

### Assessment: obj/ Location Depended on Invocation Method (#13) ✅ Done

- [x] ~~Fixed `src/Directory.Build.props` `BaseIntermediateOutputPath` to use `$(MSBuildThisFileDirectory)` instead of `$(SolutionDir)` so `obj/` always resolves to `src/obj/` regardless of how the build is invoked~~ ✅ Done

### Audio

- [x] ~~Audio level meters (real-time VU meters)~~ ✅ Done — `AudioMetersDynamicFolder`, built against obs-websocket-dotnet 5.7.0; see `docs/ai/vu-meters-learnings.md`. Follow-ups done: stale levels expire after 500ms, folder shows only live inputs (renamed "Live Audio Folder"), live-but-silent baseline, per-source Audio Meter action, crossed-out speaker icons for muted/inactive inputs
- [ ] Audio filter enable/disable toggle
- [ ] Stereo balance controls
- [ ] Audio quick presets ("Mute All", "Reset All Volumes")

### Transitions

- [ ] Transition selection (choose type and duration): `GetSceneTransitionList`, `SetCurrentSceneTransition`, `SetCurrentSceneTransitionDuration`
- [ ] T-bar on a dial: `SetTBarPosition` through a `PluginDynamicAdjustment` (design notes in `protocol-gap-analysis.md`)
- [ ] Transition duration on a dial

### OBS Requests Worth Adding (from the 2026-10-12 assessment)

All of these are available in obs-websocket-dotnet 5.7.0.

- [ ] Split recording file and add recording chapter buttons (`SplitRecordFile`, `CreateRecordChapter`; chapters need OBS 30.2+ with Hybrid MP4), with `RecordFileChanged` confirmation
- [ ] Media elapsed/total time on media buttons (`GetMediaInputStatus` `mediaCursor`/`mediaDuration`) and scrubbing on a dial (`SetMediaInputCursor`, `OffsetMediaInputCursor`)
- [ ] Screenshot saved confirmation flash from `ScreenshotSaved`, like the replay buffer save
- [ ] Read `GetVersion` on connect and disable features the connected OBS doesn't support
- [ ] Open source/scene projector button (`OpenSourceProjector`)

### Greyed-Out Start, Stop and Pause Buttons

- [x] ~~Grey out Start while running and Stop while stopped (streaming, recording, virtual camera), and Recording Pause while nothing is recording; ignore presses while greyed~~ ✅ Done — `OutputButtonAvailability`; both are also greyed while OBS is starting or stopping the output, and while disconnected
- [x] ~~Icons tinted white on the device: returning them as bitmaps~~ Tried, didn't help — still white on the device
- [x] ~~Icons tinted white on the device~~ ✅ Fixed — every icon SVG carries a tiny `#FF00FF` corner mark (`generate_icons.py` `MARKER`) so it isn't monochrome; device-checked 2026-10-13
- [ ] Consider: try a near-invisible marker colour (e.g. `#010101`) and check the icons keep their colours
- [ ] Check on a device: with nothing running, Start Streaming/Recording are blue and Stop/Pause grey, and pressing a grey one does nothing; start each and watch the buttons swap, including the starting state; pause and resume a recording; disconnect from OBS and confirm all of them grey out

### Toggle Recording Status Key

- [x] ~~Make the Toggle Recording button red while recording and green while not~~ ✅ Done — white `RecordingOnWhite`/`RecordingOffWhite` glyphs on `ButtonImageHelper.StatusGreen`/`StatusRed`
- [ ] Check on a device: the key is green with a white stop square while idle, turns red with a white record dot once recording starts (from the device and from OBS), and stays red while paused

## Low Priority

### Assessment: Source Visibility Toggle Costs Four Requests (#30) ✅ Done

- [x] ~~Resolve scene item IDs with `GetSceneItemId`, or keep the IDs from the scene sources load, instead of calling `GetSceneItemList` before every get and set~~ ✅ Done — `GetSceneItemList`'s raw response carries each item's ID and visibility, so a toggle is one list and one set by ID, and a cache fetch one list
- [x] ~~Use the cached visibility in `ToggleSourceVisibility` instead of reading it again~~ Not done on purpose — the list the toggle needs for the ID already carries the live state, which is safer than a cache that may still hold its default
- [ ] Consider: the Scene Sources folder's list load already reads the scene's items, so it could seed the visibility cache and save one fetch per tile when the folder opens. Needs a cache write that doesn't overwrite a newer event (`KeyedStateCache.Set` is for events)

### Assessment: OBSActionExecutor Repetition (#32) ✅ Done

- [x] ~~Add `RunMutation`/`Query<T>` helpers and express the connected/guard/try/log methods through them~~ ✅ Done — plus `TryQuery<T>` for the render-cache fetches; 1,242 → 657 lines
- [x] ~~Collapse `Get*`/`TryGet*` pairs whose non-`Try` version has no remaining callers~~ ✅ Done — `GetSceneItemEnabled`, `GetMediaInputStatus`, `GetInputVolume` and `GetInputAudioMonitorType` removed

### Assessment: Forwarding Layers (#33)

- [ ] After #20, have actions depend on the facade (or small per-feature interfaces) instead of the plugin class's ~60 forwarders

### Assessment: Near-Copy Select Classes (#34)

- [ ] Extract a generic base for the profile/scene collection/scene select commands, and one for their folders

### Assessment: String Literals and Input Kind Lists (#35)

- [ ] Add a `MonitorTypes` constants class and use it and `MediaInputStates.None` instead of the literals
- [ ] Replace or centralise `AudioInputKinds` and `mediaKinds`; consider detecting audio inputs by whether `GetInputVolume` succeeds, cached per kind

### Assessment: Style Rules Not Enforced (#36)

- [ ] Set `EnforceCodeStyleInBuild`, fix or suppress what it reports (155 `var` uses in `src/`), and decide whether `tests/` follows the same `.editorconfig`

### Assessment: Small Robustness and Doc Items (#37)

- [ ] `SceneSwitchAdjustableCommand`: wait for `CurrentProfileChanged`/`CurrentSceneCollectionChanged` (with a timeout) instead of fixed delays
- [ ] `OBSWebSocketManager.Dispose`: detach the `InputVolumeMeters` handler
- [ ] `AudioStatusDisplayCommand`: log in, or remove, the empty `catch { }` blocks
- [ ] Update the version in `docs/ai/product.md` (says 1.6.1, package is 2.0.1)

### Assessment: ProfileListChanged / SceneCollectionListChanged Not Subscribed (#8) ✅ Done

- [x] ~~Subscribe to `ProfileListChanged` event in `OBSWebSocketManager` and push the new list via `NotifyProfileList()`~~ ✅ Done
- [x] ~~Subscribe to `SceneCollectionListChanged` event in `OBSWebSocketManager` and push the new list via `OnSceneCollectionsChanged()`~~ ✅ Done — both push the list from the event without asking OBS again, and re-read the current profile or collection when it is missing from the list (it was renamed)
- [x] ~~Check on a device: create, rename and delete a profile and a scene collection in OBS, including renaming the current one, and confirm the folders and ticks follow~~ ✅ Done 2026-09-29 — everything followed except creating and deleting a scene collection: OBS sends no `SceneCollectionListChanged` for those, only `CurrentSceneCollectionChanged`
- [x] ~~Re-read the scene collection list on `CurrentSceneCollectionChanged`, since creating or deleting a collection always switches it~~ ✅ Done
- [x] ~~Check on a device: create and delete a scene collection and confirm the folder follows~~ ✅ Done 2026-09-29: the folder went from 3 to 4 collections on create and back to 3 on delete

### Assessment: MediaDynamicFolder Doesn't Respond to Input List Changes (#10) ✅ Done

- [x] ~~Implement `IInputsListAwareCommand` in `MediaDynamicFolder`~~ ✅ Done
- [x] ~~Filter incoming inputs list to media kinds in `OnInputsChanged`~~ ✅ Done

### Assessment: Password Field Has No Sensitivity Indication (#11) ✅ Done

- [x] ~~Add `(sensitive)` to the password field label in `PluginSettingsCommand`~~ ✅ Done

### Assessment: DoubleTapHelperTests Flaky Under Load (#14) ✅ Done

- [x] ~~Replaced fixed `Thread.Sleep` with a bounded poll (`WaitFor`) in the two flaky tests~~ ✅ Done — verified with 5 consecutive full-suite runs, zero failures from this class

### Assessment: General Thread.Sleep-After-Task.Run Flakiness in OBSActionExecutor* Tests (#15) ✅ Done

- [x] ~~Same fixed-sleep race as #14, across 88 `Thread.Sleep` calls in 12 test files~~ ✅ Done — `OBSActionExecutor` takes an injectable background runner and its tests run mutations inline; 77 sleeps removed, verified with 5 consecutive full-suite runs, zero failures
- [ ] Decide whether to run tests in CI now that the executor timing race is gone (`AGENTS.md` still says not to)

### Assessment: Small Robustness Items (#23) ✅ Done

- [x] ~~Wrap `OnApplicationStarted` (`async void`) in try/catch~~ ✅ Done
- [x] ~~Remove or `await` the `Task.Delay(100).Wait()` in `OnCurrentSceneCollectionChanged`~~ ✅ Done — removed: OBS sends `CurrentSceneCollectionChanged` only after the collection has loaded, so the scene list is read straight away
- [x] ~~Make sure `NotifyDisconnected` runs once per disconnect~~ ✅ Done — `CommandCoordinator` skips a repeat until the next connect

### Assessment: Library Fix for `Connected` on `ReIdentify` (#24)

- [ ] Open the upstream PR from `fix/connected-raised-on-reidentify` and upgrade once released (keep `SessionGate`)

### Other

- [x] ~~Stats polls during a scene collection switch log `ERROR Failed to get stats: 207` (OBS is "not ready" while the collection loads)~~ ✅ Done — `StatsService` pauses between `CurrentSceneCollectionChanging` and `CurrentSceneCollectionChanged`, then polls straight away
- [x] ~~Check on a device: switch scene collection with the OBS Stats Summary button on screen; the log shows `StatsService paused` and `resumed` and no 207 errors~~ ✅ Done 2026-09-29: paused and resumed around both switches, no 207 errors
- [ ] Recording duration display (parity with streaming stats — `GetRecordStatus` returns timecode and bytes)
- [ ] Audio sync offset controls (set-and-forget, rarely adjusted mid-stream)
- [ ] Audio track assignment (multi-track recording)
- [ ] Scene item transforms (position, scale, rotation)
- [ ] Scene/source creation from hardware
- [ ] Filter settings adjustment (not just toggle)
- [ ] Get current preview scene (studio mode) — see assessment #31
- [ ] Trigger OBS hotkeys from hardware (fallback for third-party plugin actions)

## Events Not Yet Subscribed

- [ ] `CurrentPreviewSceneChanged` — studio mode preview tracking (assessment #31)
- [x] ~~`InputNameChanged` — input list sync when renamed in OBS (assessment #21)~~ ✅ Done
- [ ] `InputAudioBalanceChanged` — audio balance display
- [ ] `InputAudioSyncOffsetChanged` — audio sync display
- [ ] `InputAudioTracksChanged` — track assignment display
- [ ] `SourceFilterCreated` / `SourceFilterRemoved` — filter list updates
- [ ] `SourceFilterEnableStateChanged` — filter state display
- [ ] `CurrentSceneTransitionChanged` — transition display
- [ ] `SceneTransitionStarted` / `SceneTransitionEnded` — transition progress
- [ ] `RecordFileChanged` — confirm a recording split
- [ ] `ScreenshotSaved` — confirm a screenshot was saved
- [x] ~~`StreamStateChanged` / `RecordStateChanged` are subscribed but don't notify commands (assessment #27)~~ ✅ Done

## Architecture (Deferred)

- [ ] Multi-instance OBS support (see `docs/ai/multi-instance-obs-design.md`)
- [x] ~~Dependency injection for StatsService (inject `Func<OBSStats>` instead of static singleton)~~ ✅ Done (assessment #17)
- [ ] Split the 657-line `OBSActionExecutor` by feature area (outputs, scenes, audio, media) the next time a large change touches it (left over from assessment #23; now a mechanical move, since #32 put the shared steps in helpers)

## Recently Completed (v2.0.0)

- [x] Real-time audio VU meters (Live Audio Folder, per-source Audio Meter action) on obs-websocket-dotnet 5.7.0
- [x] Restyled icon set
- [x] Assessments #8, #14–#19, #21–#23, #25, #26 — see `docs/ai/assessment.md` Resolved table

## Recently Completed (v1.6.2)

- [x] Assessment #6 — `CommandCoordinator` given real responsibility: per-command exception isolation via a private generic `NotifyEach<T>()` dispatcher; `CommandRegistry` simplified to a store + generic `GetCommands<T>()` filter; `CommandCoordinatorTests.cs` added (26 tests) covering dispatch and exception isolation, `CommandRegistryTests.cs` trimmed to registration/filtering
- [x] Assessment #12 — verified the net10.0 build runs under a real Logi Plugin Service install, connects to OBS, and responds to button presses
- [x] Assessment #13 — root-caused and fixed the recurring `obj/` location bug: `src/Directory.Build.props` now uses `$(MSBuildThisFileDirectory)` instead of `$(SolutionDir)` for `BaseIntermediateOutputPath`, so `obj/` no longer depends on whether the build is invoked via the `.sln`, the bare `.csproj`, or `dotnet test`
- [x] Resolved nullable-reference build warnings (CS8600/CS8602) in `DoubleTapHelperTests.cs` and `PluginConfigReaderTests.cs`
- [x] Migrated to .NET 10.0 (plugin, tests, CI)
- [x] Moved AI coding-assistant docs from `.amazonq/rules/` to `docs/ai/`; added `AGENTS.md`/`CLAUDE.md`
- [x] Added CI markdown linting (`rumdl`) and fixed existing violations repo-wide
- [x] Removed `tools/InspectSdk` reflection-based SDK reverse-engineering utility

## Recently Completed (v1.5.x)

- [x] Assessment #10 — `MediaDynamicFolder` now implements `IInputsListAwareCommand`; `OnInputsChanged` reloads media list via `GetMediaInputList()` and calls `ButtonActionNamesChanged()`
- [x] Assessment #11 — Password field label updated to `"Password (sensitive)"` in `PluginSettingsCommand`
- [x] Assessment #7 — `OBSStats.Empty` and `OBSStreamStats.Empty` null-object pattern added; `OBSActionExecutor` and `OBSFacade` return `Empty` instead of `null`; null guards removed from `StatsDisplay`, `StatsDynamicFolder`, `StreamStatsDynamicFolder`
- [x] Assessment #4 — `DoubleTapHelper` race condition and `CancellationTokenSource` leak fixed: `lock(_tapStates)` added around all dictionary access; `CancellationTokenSource.Dispose()` called in `finally` on single-tap path, immediately on double-tap path, and in `Reset()`; 7 unit tests added
- [x] Assessment #1 — CommandRegistry bypass fixed: `NotifyConnected`/`NotifyDisconnected` now called through `CommandCoordinator` in `OnOBSConnected`/`OnOBSDisconnected`
- [x] Assessment #2 — Scene sources registry bypass fixed: `ISceneSourcesAwareCommand` interface added; `OnCurrentSceneChanged` and `OnSceneItemsChanged` route through `CommandCoordinator` → `CommandRegistry` instead of direct singleton calls
- [x] `SceneCollectionsDynamicFolder` — dynamic folder for scene collections (consistent with `ProfilesDynamicFolder` and `ScenesDynamicFolder`)
- [x] Build error fix — reverted to `net8.0`, pointed `PluginApiDir` at `ci\PluginApi.dll`

## Recently Completed (v1.4.0)

- [x] FPS fix — use direct `stats.FPS` property instead of deriving from AverageFrameTime
- [x] Disk Space tile in OBS Stats Folder
- [x] Render Time tile in OBS Stats Folder
- [x] Logging refactoring — Trace/Debug/Info levels properly separated
- [x] IPluginLog.Debug added to interface
- [x] Duplicate cross-layer logging removed
- [x] ConnectionManager event relay (removed _obsManager from main plugin)
- [x] PluginSettingsCommand renamed from ConnectionConfigureCommand
- [x] Flaky test fix (extended delay for error-path tests)

## Previously Completed (v1.3.x)

- [x] Media source controls — dynamic folder + ActionEditorCommand
- [x] Subscribe to `MediaInputPlaybackStarted` / `MediaInputPlaybackEnded`
- [x] Streaming stats folder (duration, bytes sent, congestion, skipped frames)
- [x] OBS Stats display — summary button + dynamic folder with colour-coded thresholds
- [x] Stats polling service with configurable interval (2s/5s/10s)
- [x] Plugin Settings command
- [x] Subscribe to `InputAudioMonitorTypeChanged`
- [x] Subscribe to `SceneItemCreated` / `SceneItemRemoved`
- [x] Subscribe to `InputCreated` / `InputRemoved`
- [x] Remote OBS connection support (configurable IP/port/password)
- [x] Reconnection race condition fix (`_connectingInProgress` flag)
- [x] `OBSConnectionSettings` accepts any valid IP address
- [x] Plugin config persistence (`PluginConfigReader.SaveConfig`)
- [x] +/- button volume alternatives (MX Creative Console Dialpad)
