# Release Notes - v1.3.0

## Remote OBS, Performance Stats, and Media Controls

This release adds significant new capabilities: control OBS on a remote machine over the network, monitor real-time performance statistics, and play/pause/stop media sources directly from your hardware.

---

## New Features

### Remote OBS Connection

You can now connect to OBS Studio running on a different computer on your network. Configure via the new **Plugin Settings** action:

- **Use Local OBS** checkbox (default: checked) — uncheck to use manual settings
- **IP Address** — the remote machine's IP (detected local IP shown in label for reference)
- **Port** — WebSocket port (default 4455)
- **Password** — WebSocket authentication password
- Settings persist between sessions automatically

### Performance Monitoring (Group 1. OBS)

- **OBS Stats Summary** — single button showing FPS, CPU%, and dropped frames at a glance
- **OBS Stats Folder** — dynamic folder with individual tiles for each metric:
  - FPS (derived from frame render time)
  - CPU usage %
  - Memory usage (MB)
  - Render Missed Frames (GPU lag)
  - Encode Skipped Frames (CPU lag)
  - Total Dropped Frames
- **Colour-coded thresholds**: green = healthy, yellow = warning, red = problem
- Configurable polling interval: 2 seconds, 5 seconds, or 10 seconds

### Streaming Statistics (Group 2. Streaming)

- **Stream Stats Folder** — live streaming statistics while broadcasting:
  - Duration (formatted as h:mm:ss)
  - Bytes Sent (MB/GB)
  - Network Congestion (% with colour coding)
  - Skipped Frames (with percentage)
  - Total Frames
- Shows "Offline" in grey when not streaming

### Media Controls (Group 9. Media)

- **Media Controls Folder** — dynamic folder of all media sources (ffmpeg, VLC, slideshow, text sources)
  - Single tap while stopped/paused → Play
  - Single tap while playing → Pause
  - Double tap → Stop (reset to beginning)
  - Colour-coded: Green = Playing, Yellow = Paused, Grey = Stopped/Ended
  - Real-time updates when media starts or finishes naturally
- **Media Action (User Defined)** — ActionEditorCommand for granular control
  - Configure source name + action (Play, Pause, Stop, Restart, Next, Previous)
  - Next/Previous useful for VLC playlist sources

### New Event Subscriptions

The plugin now reacts in real-time to more OBS changes:

- **InputAudioMonitorTypeChanged** — audio folder buttons refresh when monitoring is changed from OBS
- **SceneItemCreated / SceneItemRemoved** — Sources folder auto-refreshes when sources are added/removed
- **InputCreated / InputRemoved** — Audio Mixer auto-refreshes when audio inputs are added/removed
- **MediaInputPlaybackStarted / MediaInputPlaybackEnded** — media folder updates when clips finish

---

## Bug Fixes

- **Reconnection race condition** — Fixed duplicate connection attempts when the reconnect timer fired before the initial connection completed. Added `_connectingInProgress` flag to prevent overlapping connections that caused disconnect/reconnect loops.

---

## Technical Details

- **37 new unit tests** (348 total)
- New models: `OBSStats`, `OBSStreamStats` with derived properties
- New service: `StatsService` (timer-based polling, starts on connect, stops on disconnect)
- New interface: `IInputMonitorAwareCommand`
- `OBSConnectionSettings` now accepts any valid IP address (previously localhost-only)
- `PluginConfigReader` gains `SaveConfig()` for persistent configuration
- `ConnectionManager` branches between local auto-discovery and remote direct connection
- `ConnectionConfigureCommand` renamed to `PluginSettingsCommand`

---

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 8.0 runtime

---

## Installation

1. Download `OBSStudioForLogiPlugin-v1.3.0.lplug4`
2. Double-click to install, or import via Logi Options+
3. Launch OBS Studio — the plugin connects automatically
4. For remote OBS: use the **Plugin Settings** action to configure connection details

---

## Acknowledgments

Developed with support from an AI coding assistant using TDD, secure coding principles, and conventional commits.
