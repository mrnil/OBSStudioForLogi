# TODO

## High Priority

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

### Assessment: Services and Plugin Class Reach Into Singletons (#20)

- [ ] Have `OBSWebSocketManager` raise events (or take a callback interface) instead of calling `OBSStudioForLogiPlugin.Instance`
- [ ] Route stats and media updates through `CommandCoordinator` instead of `StatsDisplay.Instance`, `MediaDynamicFolder.Instance` and similar

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
- [ ] Consider: `MediaDynamicFolder` still requests the full input list on every input list change, and the initial state load requests the scene list twice

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

- [ ] Transition selection (choose type and duration)

## Low Priority

### Assessment: ProfileListChanged / SceneCollectionListChanged Not Subscribed (#8) ✅ Done

- [x] ~~Subscribe to `ProfileListChanged` event in `OBSWebSocketManager` and push the new list via `NotifyProfileList()`~~ ✅ Done
- [x] ~~Subscribe to `SceneCollectionListChanged` event in `OBSWebSocketManager` and push the new list via `OnSceneCollectionsChanged()`~~ ✅ Done — both push the list from the event without asking OBS again, and re-read the current profile or collection when it is missing from the list (it was renamed)
- [x] ~~Check on a device: create, rename and delete a profile and a scene collection in OBS, including renaming the current one, and confirm the folders and ticks follow~~ ✅ Done 2026-09-29 — everything followed except creating and deleting a scene collection: OBS sends no `SceneCollectionListChanged` for those, only `CurrentSceneCollectionChanged`
- [x] ~~Re-read the scene collection list on `CurrentSceneCollectionChanged`, since creating or deleting a collection always switches it~~ ✅ Done
- [ ] Check on a device: create and delete a scene collection and confirm the folder follows

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

- [ ] Stats polls during a scene collection switch log `ERROR Failed to get stats: 207` (OBS is "not ready" while the collection loads). Treat 207 as expected there and log it at Debug, or skip polls while a switch is in progress
- [ ] Recording duration display (parity with streaming stats — `GetRecordStatus` returns timecode and bytes)
- [ ] Audio sync offset controls (set-and-forget, rarely adjusted mid-stream)
- [ ] Audio track assignment (multi-track recording)
- [ ] Scene item transforms (position, scale, rotation)
- [ ] Scene/source creation from hardware
- [ ] Filter settings adjustment (not just toggle)
- [ ] Get current preview scene (studio mode)
- [ ] Trigger OBS hotkeys from hardware (fallback for third-party plugin actions)

## Events Not Yet Subscribed

- [ ] `CurrentPreviewSceneChanged` — studio mode preview tracking
- [x] ~~`InputNameChanged` — input list sync when renamed in OBS (assessment #21)~~ ✅ Done
- [ ] `InputAudioBalanceChanged` — audio balance display
- [ ] `InputAudioSyncOffsetChanged` — audio sync display
- [ ] `InputAudioTracksChanged` — track assignment display
- [ ] `SourceFilterCreated` / `SourceFilterRemoved` — filter list updates
- [ ] `SourceFilterEnableStateChanged` — filter state display
- [ ] `CurrentSceneTransitionChanged` — transition display
- [ ] `SceneTransitionStarted` / `SceneTransitionEnded` — transition progress

## Architecture (Deferred)

- [ ] Multi-instance OBS support (see `docs/ai/multi-instance-obs-design.md`)
- [x] ~~Dependency injection for StatsService (inject `Func<OBSStats>` instead of static singleton)~~ ✅ Done (assessment #17)
- [ ] Split the 1,165-line `OBSActionExecutor` by feature area (outputs, scenes, audio, media) the next time a large change touches it (left over from assessment #23)

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
