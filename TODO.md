# TODO

## In Progress

- [ ] Validate remote OBS connection config is being picked up correctly (diagnostic logging added)

## High Priority

No items.

## Medium Priority

### Audio

- [ ] Audio level meters (real-time VU meters)
- [ ] Audio filter enable/disable toggle
- [ ] Stereo balance controls
- [ ] Audio quick presets ("Mute All", "Reset All Volumes")

### Media

- [ ] Media source controls (play/pause/stop/restart)

### Transitions

- [ ] Transition selection (choose type and duration)

## Low Priority

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
- [ ] `MediaInputPlaybackStarted` / `MediaInputPlaybackEnded` — media state
- [ ] `CurrentSceneTransitionChanged` — transition display
- [ ] `SceneTransitionStarted` / `SceneTransitionEnded` — transition progress

## Architecture (Deferred)

- [ ] Multi-instance OBS support (see `.amazonq/rules/memory-bank/multi-instance-obs-design.md`)

## Recently Completed

- [x] +/- button volume alternatives (provided by MX Creative Console Dialpad)
- [x] OBS Stats display — summary button + dynamic folder with colour-coded thresholds
- [x] Stats polling service with configurable interval (2s/5s/10s via Plugin Settings)
- [x] Plugin Settings command (renamed from ConnectionConfigureCommand)
- [x] Subscribe to `InputAudioMonitorTypeChanged` — audio folders refresh when monitor type changes
- [x] Subscribe to `SceneItemCreated` / `SceneItemRemoved` — Sources folder auto-refreshes
- [x] Subscribe to `InputCreated` / `InputRemoved` — Audio Mixer auto-refreshes
- [x] Remote OBS connection support (configurable IP/port/password via ActionEditorCommand)
- [x] Reconnection race condition fix (`_connectingInProgress` flag)
- [x] `OBSConnectionSettings` accepts any valid IP address
- [x] Plugin config persistence (`PluginConfigReader.SaveConfig`)
