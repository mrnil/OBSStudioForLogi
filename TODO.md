# TODO

Open work only. Finished work is in `CHANGELOG.md` and the git history, and fixed assessment items are listed in the Resolved table of `docs/ai/assessment.md`. Numbers in brackets refer to that file.

## Device Checks Outstanding

- [ ] Reconnect after an OBS settings change (#29): change the WebSocket password in OBS while connected (OBS drops the connection), and confirm the plugin reconnects with the new password without pressing Reconnect. Then quit and restart OBS and confirm it reconnects.

## Medium Priority

### No Scene Feedback in Studio Mode (#31)

- [ ] Track the preview scene: `GetCurrentPreviewScene` in the initial load and when studio mode turns on, `CurrentPreviewSceneChanged` after that
- [ ] Show preview and program as separate states on the scene folder, select command and Current Scene display
- [ ] Check on a device: in studio mode, press a scene (the preview state shows), then transition (the program state moves)

### Double-Tap Window (#5)

- [ ] Move the 500ms `DoubleTapThreshold` into `OBSTimings` or Plugin Settings, or drop double-tap in favour of dedicated mute buttons

### OBS Groups Not Handled (#28)

- [ ] Expand groups through `GetGroupSceneItemList` when building scene source lists, and decide between a flat list and a sub-level in the Scene Sources folder
- [ ] Include group children in `GetAudioInputsNotInAnyScene`'s membership set
- [ ] Check that visibility caching works for group children (`SceneItemEnableStateChanged` reports the group as the scene)

## Low Priority

### Assessment Items

- [ ] Remote host input (#38): bracket IPv6 in the WebSocket URL; accept host names or reject bad input visibly instead of falling back to localhost
- [ ] Run tests in CI (#42) on a Windows runner with `-p:PluginDir=` set to a scratch folder
- [ ] Version (#39): bump the `.csproj` to the package version at the next release, or single-source it
- [ ] User-defined actions (#40): check whether `ListboxItemsRequested` can offer scene, source and input names, and use listboxes if so
- [ ] Recording duration display (#9): `GetRecordStatus` timecode and bytes, alongside the stream stats
- [ ] Small robustness items (#37): event-driven waits in `SceneSwitchAdjustableCommand`, detach the `InputVolumeMeters` handler in `Dispose`, log or remove the empty catches in `AudioStatusDisplayCommand`, one spelling of "(User defined)", drop the redundant `Task.Run` wrappers in the adjustable commands
- [ ] String literals and input kinds (#35): a `MonitorTypes` constants class, `MediaInputStates.None` instead of the literal, and one place (or a capability check) for `AudioInputKinds` and `mediaKinds`
- [ ] Style enforcement (#36): set `EnforceCodeStyleInBuild`, fix or suppress what it reports, and decide whether `tests/` follows the same `.editorconfig`
- [ ] Near-copy select classes (#34): one generic base for the profile/scene collection/scene select commands, one for their folders
- [ ] Forwarding layers (#33): have actions depend on the facade (or small per-feature interfaces) instead of the plugin class's forwarders
- [ ] Supported devices (#41): check which `supportedDevices` values the MX Creative Console needs
- [ ] Library fix (#24): open the upstream PR from `fix/connected-raised-on-reidentify` and upgrade once released (keep `SessionGate`)

### Worth Considering

- [ ] Every reconnect waits `OBSTimings.ConnectionDelay` (2s), which was meant for OBS starting up; a reconnect to an OBS that is still running could skip it
- [ ] `MediaDynamicFolder` requests the full input list on every input list change
- [ ] The Scene Sources folder's list load already reads each item's visibility, so it could seed the visibility cache and save one fetch per tile. It needs a cache write that doesn't overwrite a newer event (`KeyedStateCache.Set` is for events)
- [ ] Try a near-invisible icon marker colour (for example `#010101`) instead of `#FF00FF`, and check on a device that the icons keep their colours
- [ ] Split the 676-line `OBSActionExecutor` by feature area (outputs, scenes, audio, media) the next time a large change touches it

## Features

All of these use requests and events that obs-websocket-dotnet 5.7.0 already has. `docs/ai/protocol-gap-analysis.md` has design notes for transitions and filters.

### Transitions

- [ ] Transition selection (type and duration): `GetSceneTransitionList`, `SetCurrentSceneTransition`, `SetCurrentSceneTransitionDuration`, `CurrentSceneTransitionChanged`
- [ ] T-bar on a dial: `SetTBarPosition` through a `PluginDynamicAdjustment`
- [ ] Transition duration on a dial
- [ ] Transition progress feedback: `SceneTransitionStarted` / `SceneTransitionEnded`

### Recording and Outputs

- [ ] Split recording file and add recording chapter buttons (`SplitRecordFile`, `CreateRecordChapter`; chapters need OBS 30.2+ with Hybrid MP4), confirmed by `RecordFileChanged`
- [ ] Screenshot saved confirmation flash from `ScreenshotSaved`, like the replay buffer save

### Media

- [ ] Elapsed/total time on media buttons (`GetMediaInputStatus` `mediaCursor`/`mediaDuration`) and scrubbing on a dial (`SetMediaInputCursor`, `OffsetMediaInputCursor`)

### Audio

- [ ] Audio filter enable/disable toggle (`GetSourceFilterList`, `SetSourceFilterEnabled`, `SourceFilterEnableStateChanged`, `SourceFilterCreated`/`SourceFilterRemoved`)
- [ ] Stereo balance controls (`InputAudioBalanceChanged`)
- [ ] Quick presets ("Mute All", "Reset All Volumes")
- [ ] Audio sync offset controls (`InputAudioSyncOffsetChanged`; set-and-forget, rarely adjusted mid-stream)
- [ ] Audio track assignment for multi-track recording (`InputAudioTracksChanged`)

### Other

- [ ] Read `GetVersion` on connect and disable features the connected OBS doesn't support
- [ ] Open a source or scene projector (`OpenSourceProjector`)
- [ ] Trigger OBS hotkeys from hardware (`TriggerHotkeyByName`; a fallback for third-party plugin actions)
- [ ] Filter settings adjustment (not just the toggle)
- [ ] Scene item transforms (position, scale, rotation)
- [ ] Scene and source creation from hardware
- [ ] Multi-instance OBS support (design in `docs/ai/multi-instance-obs-design.md`)
