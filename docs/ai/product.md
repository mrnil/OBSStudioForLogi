# Product Overview

## Project Identity

- **Name**: Streaming Assistant (package display name) / OBSStudioForLogiPlugin (assembly name)
- **Package ID**: OBSStudioForLogi
- **Current version**: see `version:` in `src/package/metadata/LoupedeckPackage.yaml` (2.0.1 at the time of writing)
- **Author**: Stephen Moretti
- **License**: MIT
- **Repository**: <https://github.com/mrnil/OBSStudioForLogi>

## Purpose

A Logitech/Loupedeck hardware plugin for controlling OBS Studio from physical devices (MX Creative Console, Loupedeck CT, Loupedeck Live). It connects the Logi Actions SDK to the OBS WebSocket 5.x protocol, so streaming, recording, scenes, audio and media can be controlled from the device instead of with the mouse and keyboard.

The plugin is **not** an OBS extension. It is a Logi Plugin Service plugin that talks to OBS's built-in WebSocket server.

## Target Users

- Streamers and content creators using a Logitech MX Creative Console or a Loupedeck device
- Users who want hardware buttons for OBS during live streams or recordings
- Power users who want configurable, per-button OBS actions (the "User defined" actions)

## Devices

- **MX Creative Console**: the main target. Dials and the big wheel for volume, adjustment tiles for audio
- **Loupedeck CT / Live**: fully supported, including encoder rotation and touch buttons

## Feature Groups

The numbered groups are the `GroupName` prefixes shown in the action list. `###` sub-groups appear nested under their group.

### 1. OBS

- Screenshot (saves to Pictures, Documents or Desktop)
- Reconnect to OBS, Connection Status (green connected, red disconnected, orange WebSocket server disabled)
- Toggle Studio Mode, Studio Mode Transition
- Stats Summary (FPS, CPU%, dropped frames, colour-coded) and Stats Folder (FPS, CPU, Memory, Render Missed, Encode Skipped, Total Dropped, Disk Space, Render Time)
- Plugin Settings (local or remote OBS, stats polling interval, audio meter refresh rate)

### 2. Streaming

- Toggle/Start/Stop Streaming. Start and Stop are greyed out when they can't act
- Stream Stats Folder (duration, bytes sent, congestion, skipped frames, total frames)

### 3. Recording

- Toggle/Start/Stop Recording and Recording Pause. Toggle Recording is a status key (red record dot while recording, green stop square while not)

### 4. Replay Buffer

- Toggle Replay Buffer, Save Replay (flashes green when saved)

### 5. Virtual Camera

- Toggle/Start/Stop Virtual Camera

### 6. Profiles

- Profiles folder, Current Profile display, and one select button per profile (`Available Profiles`)

### 7. Scenes

- Scenes folder, Scene Sources folder (toggle visibility), Scene Collections folder
- Current Scene and Current Collection displays
- One select button per scene (`Available Scenes`) and per scene collection (`Available Collections`)
- `User Defined`: Switch to Scene (optional profile and collection first), Toggle Source Visibility (comma-separated sources, optional scene)
- In studio mode, scene buttons set the preview scene; otherwise the program scene

### 8. Audio

- Live Audio Folder: real-time VU meters for the inputs OBS reports as live, tap to mute
- Mixer for All Audio and Mixer for Scene Audio: single tap selects the input for the dial, double tap mutes (the double-tap window, or off, is a Plugin Settings option), encoder adjusts volume and press cycles monitoring; volume in dB
- Audio Select folder (selection only), Audio Volume folder (adjustment tiles for the MX big wheel)
- Selected Source Volume adjustment for any dial or wheel
- `Meters`: one Audio Meter per input, live or not
- `Available Sources`: one mute button per input
- `User Defined`: Toggle Audio Mute, Cycle Audio Monitoring, Select Audio Source, Audio Source Status

### 9. Media

- Media Controls folder: single tap plays or pauses, double tap stops (or Media Action (User Defined) when double-tap is off); colour shows the state
- Media Action (User Defined): play, pause, stop, restart, next or previous on a named source

## Connection

- **Local OBS**: reads the WebSocket port and password from `%AppData%\obs-studio\plugin_config\obs-websocket\config.json`
- **Remote OBS**: IP address, port and password set through Plugin Settings; the password is kept in the SDK's encrypted plugin settings
- **Reconnection**: `ConnectionManager` retries with backoff (1s to 30s, ±15% jitter) and re-reads the settings on every attempt
- **Connection states**: Connected, Disconnected, WebSocket Disabled

## Non-Functional Characteristics

- OBS writes run on a background runner, so a slow OBS never holds up a button press
- Buttons render from cached state kept current by OBS events, and never wait on OBS
- Data that only one folder or button shows is loaded only while it is on screen
- Stats polling interval is configurable (2s, 5s or 10s)
- Windows is the main platform; the package also declares a macOS plugin folder
- Services are developed test first (see `test-coverage.md` for the current count and coverage)
