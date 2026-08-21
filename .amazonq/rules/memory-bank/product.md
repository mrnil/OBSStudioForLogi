# Product Overview

## Project Identity

- **Name**: Streaming Assistant (package display name) / OBSStudioForLogiPlugin (assembly name)
- **Package ID**: OBSStudioForLogi
- **Current Version**: 1.6.1
- **Author**: Stephen Moretti
- **License**: MIT
- **Repository**: https://github.com/mrnil/OBSStudioForLogi

## Purpose and Value Proposition

A Logitech/Loupedeck hardware plugin that enables direct, real-time control of OBS Studio from physical devices (MX Creative Console, Loupedeck CT, Loupedeck Live). It bridges the Logi Actions SDK with the OBS WebSocket 5.x protocol, providing tactile hardware control over streaming, recording, scenes, audio, and media — without touching the mouse or keyboard.

The plugin is **not** an OBS extension or plugin. It is a Logi Plugin Service plugin that communicates with OBS via its WebSocket server.

## Target Users

- Streamers and content creators using Logitech MX Creative Console or Loupedeck devices
- Users who want hardware-button control over OBS during live streams or recordings
- Power users who need configurable, per-button OBS actions (user-defined action group)

## Primary Device

- **MX Creative Console** (primary) — uses dial/wheel for volume, adjustment tiles for audio
- **Loupedeck CT / Loupedeck Live** — first-class citizens; encoder rotation and touch buttons supported

## Feature Groups

### Group 1 — OBS
- Screenshot capture (auto-detects Pictures/Documents/Desktop path)
- Manual reconnect button
- Studio mode toggle and transition
- Connection status display (green=connected, red=disconnected, orange=WebSocket disabled)
- OBS Stats Summary (FPS, CPU%, dropped frames — colour-coded)
- OBS Stats Folder (FPS, CPU, Memory, Render Missed, Encode Skipped, Total Dropped, Disk Space, Render Time)
- Plugin Settings (configure local/remote OBS, stats polling interval)

### Group 2 — Streaming
- Toggle, start, stop streaming
- Stream Stats Folder (duration, bytes sent, congestion, skipped frames, total frames — colour-coded)

### Group 3 — Recording
- Toggle, start, stop, pause/resume recording

### Group 4 — Replay Buffer
- Toggle replay buffer, save replay buffer (green flash confirmation on save)

### Group 5 — Virtual Camera
- Toggle, start, stop virtual camera

### Group 6 — Profiles
- **Available Profiles** sub-group: Profile Select (multi-state), OBS Profiles Dynamic Folder
- Current Profile Display

### Group 7 — Scenes
- **Available Scenes** sub-group: Scene Select (multi-state — new in v1.6.0)
- **Available Collections** sub-group: Scene Collection Select, OBS Scene Collections Dynamic Folder (new in v1.6.0)
- **User Defined** sub-group: Switch to Scene, Toggle Source Visibility
- Sources Folder (visibility toggle), Current Scene Display, Current Scene Collection Display
- Studio mode aware: switches to preview when studio mode enabled, program when disabled

### Group 8 — Audio
- Audio Mixer Folder (all inputs, mute/unmute, volume in dB, real-time updates)
- Scene Audio Folder (inputs in current scene + inputs not in any scene)
- Audio Select Folder (dedicated source selection for wheel/dial control)
- Audio Volume Folder (MX big wheel-compatible adjustment tiles)
- Selected Source Volume Adjustment (standalone dial/wheel adjustment)

### Group 9 — Media
- Media Controls Folder (play/pause/stop with single/double tap, colour-coded state)

### Group 99 — User Defined Actions
- Switch to Scene (configurable profile, collection, scene)
- Toggle Source Visibility (comma-separated sources, optional scene)
- Toggle Audio Mute (named source)
- Cycle Audio Monitoring (None → Monitor Only → Monitor & Output)
- Select Audio Source (toggle global selection for wheel/dial)
- Audio Source Status Display (real-time volume dB, mute state, monitoring type)
- Media Action (named source + action listbox: Play, Pause, Stop, Restart, Next, Previous)

## Connection Architecture

- **Local OBS**: Auto-discovers WebSocket settings from `%AppData%\obs-studio\plugin_config\obs-websocket\config.json`
- **Remote OBS**: Configurable IP/port/password via Plugin Settings action
- **Resilient reconnection**: Exponential backoff (1s–30s) with ±15% jitter
- **Three connection states**: Connected, Disconnected, WebSocket Disabled

## Key Non-Functional Characteristics

- All OBS operations are async (Task.Run) to prevent UI thread blocking
- Real-time event-driven updates via OBS WebSocket events (no polling for state)
- Stats polling is configurable (2s, 5s, 10s intervals)
- Cross-platform build (Windows primary, macOS supported via pluginFolderMac)
- 362 unit tests; TDD approach for services layer
