# TODO

## In Progress

- [ ] Validate remote OBS connection config is being picked up correctly (diagnostic logging added)

## High Priority

- [ ] +/- button volume alternatives for devices without encoders/wheels

## Medium Priority

### Audio
- [ ] Audio level meters (real-time VU meters)
- [ ] Audio filter enable/disable toggle
- [ ] Audio sync offset controls
- [ ] Audio track assignment (multi-track recording)
- [ ] Stereo balance controls
- [ ] Audio quick presets ("Mute All", "Reset All Volumes")

### Media
- [ ] Media source controls (play/pause/stop/restart)

### Transitions
- [ ] Transition selection (choose type and duration)

### Hotkeys
- [ ] Trigger OBS hotkeys from hardware

## Low Priority

- [ ] Statistics display (FPS, CPU, dropped frames)
- [ ] Scene item transforms (position, scale, rotation)
- [ ] Scene/source creation from hardware
- [ ] Filter settings adjustment (not just toggle)
- [ ] Get current preview scene (studio mode)

## Events Not Yet Subscribed

- [ ] `CurrentPreviewSceneChanged` — studio mode preview tracking
- [ ] `SceneItemCreated` / `SceneItemRemoved` — dynamic source list updates
- [ ] `InputCreated` / `InputRemoved` — dynamic input list updates
- [ ] `InputNameChanged` — input list sync
- [ ] `InputAudioBalanceChanged` — audio balance display
- [ ] `InputAudioSyncOffsetChanged` — audio sync display
- [ ] `InputAudioTracksChanged` — track assignment display
- [ ] `InputAudioMonitorTypeChanged` — monitor state display
- [ ] `SourceFilterCreated` / `SourceFilterRemoved` — filter list updates
- [ ] `SourceFilterEnableStateChanged` — filter state display
- [ ] `MediaInputPlaybackStarted` / `MediaInputPlaybackEnded` — media state
- [ ] `CurrentSceneTransitionChanged` — transition display
- [ ] `SceneTransitionStarted` / `SceneTransitionEnded` — transition progress

## Architecture (Deferred)

- [ ] Multi-instance OBS support (see `.amazonq/rules/memory-bank/multi-instance-obs-design.md`)

## Recently Completed

- [x] Remote OBS connection support (configurable IP/port/password via ActionEditorCommand)
- [x] Reconnection race condition fix (`_connectingInProgress` flag)
- [x] `OBSConnectionSettings` accepts any valid IP address
- [x] Plugin config persistence (`PluginConfigReader.SaveConfig`)
- [x] `ConnectionConfigureCommand` with checkbox and textboxes
