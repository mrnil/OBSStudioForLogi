# TODO

## High Priority

### Assessment: Scene/Source/Profile Buttons Show No Text (#3)

- [ ] `ScenesDynamicFolder.GetCommandImage` — render scene name alongside selected/unselected icon using `ButtonImageHelper.StateTextWithIcon`
- [ ] `SourcesDynamicFolder.GetCommandImage` — render source name alongside visibility icon
- [ ] `ProfilesDynamicFolder.GetCommandImage` — render profile name alongside selected/unselected icon

### Assessment: Verify net10.0 Runtime Compatibility (#12) ✅ Done

- [x] ~~Verified net10.0 build runs under a real Logi Plugin Service install, connects to OBS, and responds to button presses~~ ✅ Done

## Medium Priority

### Assessment: CommandCoordinator Has No Error Isolation (#6) ✅ Done

- [x] ~~Add per-command exception isolation in each `Notify*` method so one failing command does not break others~~ ✅ Done

### Assessment: OBSStats Null Propagation (#7) ✅ Done

### Assessment: obj/ Location Depended on Invocation Method (#13) ✅ Done

- [x] ~~Fixed `src/Directory.Build.props` `BaseIntermediateOutputPath` to use `$(MSBuildThisFileDirectory)` instead of `$(SolutionDir)` so `obj/` always resolves to `src/obj/` regardless of how the build is invoked~~ ✅ Done

### Audio

- [ ] Audio level meters (real-time VU meters) — **deferred** until obs-websocket-dotnet supports InputVolumeMeters high-volume event subscription
- [ ] Audio filter enable/disable toggle
- [ ] Stereo balance controls
- [ ] Audio quick presets ("Mute All", "Reset All Volumes")

### Transitions

- [ ] Transition selection (choose type and duration)

## Low Priority

### Assessment: ProfileListChanged / SceneCollectionListChanged Not Subscribed (#8)

- [ ] Subscribe to `ProfileListChanged` event in `OBSWebSocketManager` and call `UpdateProfileList()`
- [ ] Subscribe to `SceneCollectionListChanged` event in `OBSWebSocketManager` and call `UpdateSceneCollectionList()`

### Assessment: MediaDynamicFolder Doesn't Respond to Input List Changes (#10)

- [x] ~~Implement `IInputsListAwareCommand` in `MediaDynamicFolder`~~ ✅ Done
- [x] ~~Filter incoming inputs list to media kinds in `OnInputsChanged`~~ ✅ Done

### Assessment: Password Field Has No Sensitivity Indication (#11)

- [x] ~~Add `(sensitive)` to the password field label in `PluginSettingsCommand`~~ ✅ Done

### Assessment: DoubleTapHelperTests Flaky Under Load (#14)

- [ ] Increase `OBSTimings.TestAsyncDelayExtended`, or replace the fixed `Thread.Sleep` in the flaky test with a bounded poll on expected state

### Other

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
- [ ] `InputNameChanged` — input list sync when renamed in OBS
- [ ] `InputAudioBalanceChanged` — audio balance display
- [ ] `InputAudioSyncOffsetChanged` — audio sync display
- [ ] `InputAudioTracksChanged` — track assignment display
- [ ] `SourceFilterCreated` / `SourceFilterRemoved` — filter list updates
- [ ] `SourceFilterEnableStateChanged` — filter state display
- [ ] `CurrentSceneTransitionChanged` — transition display
- [ ] `SceneTransitionStarted` / `SceneTransitionEnded` — transition progress

## Architecture (Deferred)

- [ ] Multi-instance OBS support (see `docs/ai/multi-instance-obs-design.md`)
- [ ] Dependency injection for StatsService (inject `Func<OBSStats>` instead of static singleton)

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
