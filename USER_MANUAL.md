# Streaming Assistant - User Manual

## What is Streaming Assistant?

Streaming Assistant is a plugin for Logitech Option+ and Loupedeck software that lets you control OBS Studio directly from your Logitech hardware device. Instead of clicking around in OBS during a live stream or recording, you get dedicated physical buttons with real-time visual feedback showing exactly what's happening.

This plugin is **not** an OBS extension — it communicates with OBS through its built-in WebSocket server, which means OBS doesn't need any additional plugins installed.

---

## Getting Started

### Requirements

- **OBS Studio 28.0 or later** (includes obs-websocket 5.0+)
- **Logi Options+** or **Loupedeck** software with a compatible device
- A supported device: MX Creative Console, Loupedeck CT, Loupedeck Live, or similar

### Installation

1. Download the latest `.lplug4` file from the [Releases page](https://github.com/mrnil/OBSStudioForLogi/releases)
2. Double-click the file to install, or open Logi Options+ and import the plugin
3. Restart the Logi Plugin Service if prompted

### First Connection

The plugin connects to OBS automatically — no configuration needed for local OBS.

1. **Open OBS Studio** — make sure OBS is running
2. **Verify WebSocket is enabled** — in OBS, go to *Tools → WebSocket Server Settings* and ensure the server is enabled
3. The plugin reads your OBS WebSocket settings automatically and connects within a few seconds
4. You'll see "Connected" (green) on the Connection Status display when ready

If OBS is already running when you install the plugin, it will connect immediately. If not, it will connect as soon as OBS starts.

> **Tip:** The plugin reads your OBS password automatically from OBS's config file. You never need to enter it manually.

### Connecting to Remote OBS

To control OBS running on a different computer on your network:

1. Drag **Plugin Settings** onto a button
2. Configure:
   - Uncheck **Use Local OBS**
   - Enter the **IP Address** of the remote machine
   - Enter the **Port** (default 4455)
   - Enter the **Password** (set in OBS WebSocket Server Settings on the remote machine)
   - Choose a **Stats Polling Interval** (2s, 5s, or 10s)
3. Press the button to save and reconnect

Settings persist between sessions. To switch back to local OBS, check "Use Local OBS" and press the button again.

---

## Quick Setup: Streaming

Here's a suggested button layout for a basic streaming setup:

| Button | Action | What it does |
|--------|--------|-------------|
| 1 | Streaming Toggle | Start/stop your stream with one tap |
| 2 | Recording Toggle | Start/stop recording alongside your stream |
| 3 | Scenes Folder | Open to switch between scenes (e.g., Starting Soon, Live, BRB, Ending) |
| 4 | Current Scene Display | Always shows which scene is live |
| 5 | Audio Mixer Folder | Quick access to mute/unmute any audio source |
| 6 | Screenshot | Capture a moment from your stream |

### How to set up

1. Open Logi Options+ (or Loupedeck software)
2. Select your device
3. Find **Streaming Assistant** in the actions list
4. Drag actions onto your device buttons
5. Launch OBS and start streaming!

---

## Quick Setup: Recording

For recording-focused workflows:

| Button | Action | What it does |
|--------|--------|-------------|
| 1 | Recording Toggle | Start/stop recording |
| 2 | Recording Pause | Pause/resume without creating a new file |
| 3 | Scenes Folder | Switch between camera angles or screen shares |
| 4 | Sources Folder | Toggle source visibility (show/hide overlays) |
| 5 | Audio Mixer Folder | Mute/unmute microphone or desktop audio |
| 6 | Current Scene Display | See which scene is active |

---

## All Available Actions

### OBS (Group 1)

| Action | Type | Description |
|--------|------|-------------|
| Screenshot | Button | Captures a screenshot of the current scene. Saves to your Pictures, Documents, or Desktop folder. |
| Reconnect | Button | Manually retry connection to OBS if it disconnects. |
| Studio Mode Toggle | Button | Enable/disable OBS studio mode. When enabled, you can preview scenes before sending them live. |
| Studio Mode Transition | Button | Send the preview scene to program (live). Only works when studio mode is on. |
| Connection Status | Display | Shows "Connected" (green) or "Disconnected" (red). Read-only. |
| OBS Stats Summary | Display | Shows FPS, CPU%, and dropped frames. Green = healthy, red = problems. |
| OBS Stats Folder | Folder | Individual tiles: FPS, CPU, Memory, Render Missed, Encode Skipped, Total Dropped, Disk Space, Render Time. Colour-coded thresholds. |
| Plugin Settings | Button | Configure OBS connection (local/remote), IP, port, password, and stats polling interval. Press to save. |

### Streaming (Group 2)

| Action | Type | Description |
|--------|------|-------------|
| Streaming Toggle | Button | Start streaming if stopped, stop if streaming. Single button for both. |
| Streaming Start | Button | Start streaming only. Does nothing if already streaming. |
| Streaming Stop | Button | Stop streaming only. Does nothing if not streaming. |
| Stream Stats Folder | Folder | Live stream statistics: Duration, Bytes Sent, Network Congestion, Skipped Frames, Total Frames. Shows "Offline" when not streaming. |

### Recording (Group 3)

| Action | Type | Description |
|--------|------|-------------|
| Recording Toggle | Button | Start recording if stopped, stop if recording. |
| Recording Start | Button | Start recording only. |
| Recording Stop | Button | Stop recording only. |
| Recording Pause | Button | Pause active recording. Tap again to resume. Useful for breaks without creating a new file. |

### Replay Buffer (Group 4)

| Action | Type | Description |
|--------|------|-------------|
| Replay Buffer Toggle | Button | Start/stop the replay buffer. |
| Save Replay Buffer | Button | Save the last N seconds to disk (configured in OBS). Great for capturing highlights. |

### Virtual Camera (Group 5)

| Action | Type | Description |
|--------|------|-------------|
| Virtual Camera Toggle | Button | Start/stop the virtual camera output. Useful for video calls. |
| Virtual Camera Start | Button | Start virtual camera only. |
| Virtual Camera Stop | Button | Stop virtual camera only. |

### Profiles (Group 6)

| Action | Type | Description |
|--------|------|-------------|
| Profile Select | Multi-state | Switch between OBS profiles. Shows which profile is active. |
| Profiles Folder | Folder | Opens a folder showing all available profiles. Tap one to switch. |
| Current Profile | Display | Shows the name of the active profile. Read-only. |

### Scenes (Group 7)

| Action | Type | Description |
|--------|------|-------------|
| Scene Collection Select | Multi-state | Switch between scene collections. Shows which is active. |
| Scenes Folder | Folder | Opens a folder showing all scenes. Tap to switch. In studio mode, this sets the preview (not live). |
| Sources Folder | Folder | Opens a folder showing all sources in the current scene. Tap to toggle visibility on/off. |
| Current Scene | Display | Shows the name of the active scene. Read-only. |
| Current Collection | Display | Shows the name of the active scene collection. Read-only. |

### Audio (Group 8)

| Action | Type | Description |
|--------|------|-------------|
| Audio Mixer Folder | Folder | All audio inputs. Single-tap to select for wheel control, double-tap to mute/unmute. Shows volume % and monitoring mode. |
| Scene Audio Folder | Folder | Same as Audio Mixer but filtered to audio sources in the current scene. |
| Audio Select Folder | Folder | Selection-only folder. Single tap to select/deselect a source for global wheel/dial control. No mute action. |
| Audio Volume Folder | Folder | MX big wheel compatible. Tap a tile to arm the wheel for that source's volume, then turn the wheel to adjust. |
| Selected Source Volume | Adjustment | Standalone volume control. Drag onto any wheel or dial. Controls volume of the globally selected audio source. Press to reset to 100%. |

### User Defined Actions (Group 99)

These actions are configurable — you type in the names of the OBS items you want to control:

| Action | Configuration | Description |
|--------|--------------|-------------|
| Switch to Scene | Profile (optional), Collection (optional), Scene (required) | Switch to a specific scene. Optionally switch profile and collection first. |
| Toggle Source Visibility | Scene (optional), Source name(s) (required) | Toggle visibility of named sources. Comma-separate for multiple. Defaults to current scene. |
| Toggle Audio Mute | Audio source name (required) | Mute/unmute a specific audio source. |
| Cycle Audio Monitoring | Audio source name (required) | Cycle: None → Monitor Only → Monitor & Output. |
| Select Audio Source | Audio source name (required) | Toggle this source as the globally selected audio source for wheel/dial control. |
| Audio Source Status | Audio source name (required) | Display-only. Shows mute state, volume %, monitoring mode, and selection border. Updates in real-time. |
| Media Action | Source name (required), Action (required) | Trigger a media action on a named source. Actions: Play, Pause, Stop, Restart, Next, Previous. |

---

## Media Controls

The **Media Controls Folder** (Group 9) shows all media sources in OBS (video clips, VLC sources, slideshows, text sources).

### Tap Behaviour

| Current State | Single Tap | Double Tap |
|---------------|------------|------------|
| Stopped/Ended | Play | Stop (no change) |
| Paused | Play (resume) | Stop (reset to start) |
| Playing | Pause | Stop (reset to start) |

### Colour Coding

- **Green** = Playing
- **Yellow** = Paused
- **Grey** = Stopped or Ended

Buttons update in real-time when media finishes playing naturally.

### User Defined Media Action

For more granular control, use the **Media Action (User Defined)** command:
1. Drag onto a button
2. Type the exact media source name
3. Select an action from the dropdown: Play, Pause, Stop, Restart, Next, Previous
4. Press to trigger

**Next/Previous** are for VLC playlist sources that contain multiple items.

---

## Understanding Audio Controls

The plugin offers several ways to control audio, each suited to different workflows:

### Muting/Unmuting

- **Audio Mixer Folder** — double-tap any source to toggle mute
- **Toggle Audio Mute (User defined)** — assign a dedicated button to mute a specific source

### Volume Control

Volume requires two steps: **select** a source, then **adjust** with a wheel/dial.

**Selecting a source:**
- Single-tap in Audio Mixer or Scene Audio folders
- Tap in Audio Select folder
- Use "Select Audio Source" user-defined action

**Adjusting volume:**
- **MX Creative Console** — use Audio Volume folder (tap tile, turn big wheel)
- **Loupedeck CT** — use the encoder wheel while in Audio Mixer/Scene Audio folders
- **Any device** — drag "Selected Source Volume" onto any wheel or dial

### Visual Feedback

All audio buttons use colour coding:
- **Green text** = unmuted
- **Red text** = muted
- **White border** = currently selected for wheel/dial control
- Volume shown as percentage (0-100%, can exceed 100% with boost)

### Audio Monitoring

Monitoring controls whether you hear a source in your headphones:
- **None** — source goes to stream/recording only
- **Monitor Only** — source plays in your headphones only (not stream)
- **Monitor & Output** — source plays in headphones AND goes to stream

Cycle through these with the "Cycle Audio Monitoring" user-defined action, or press the encoder in Audio Mixer/Scene Audio folders.

---

## Studio Mode

Studio mode lets you preview a scene before sending it live:

1. Enable studio mode with the **Studio Mode Toggle** button
2. When you tap a scene in the Scenes Folder, it goes to **preview** (not live)
3. When you're ready, tap **Studio Mode Transition** to send preview to program (live)

This is essential for professional broadcasts where you need to verify a scene before the audience sees it.

---

## User Defined Actions — Detailed Guide

User Defined Actions let you create custom buttons tailored to your specific OBS setup. You configure them by typing exact names from your OBS configuration.

### Finding Source Names

To find the exact name of a source in OBS:
1. Open OBS Studio
2. Look in the **Sources** panel at the bottom
3. The name shown there is exactly what you type into the configuration

### Finding Audio Source Names

1. In OBS, go to the **Audio Mixer** panel
2. The name of each audio source shown there is what you type in

### Example: Quick Mute Button for Microphone

1. Drag "Toggle Audio Mute (User defined)" onto a button
2. In the configuration panel, type your mic name exactly (e.g., `Microphone`)
3. Tap the button to mute/unmute

### Example: Toggle Multiple Overlays

1. Drag "Toggle Source Visibility (User defined)" onto a button
2. Type source names separated by commas: `Chat Overlay, Alerts, Webcam Border`
3. Leave scene name empty (defaults to current scene)
4. Tap to toggle all three at once

### Example: One-Button Scene Switch with Context

1. Drag "Switch to Scene (Adjustable)" onto a button
2. Configure:
   - Profile Name: `Streaming` (optional)
   - Collection Name: `Main Show` (optional)
   - Scene Name: `Gaming` (required)
3. One tap switches your entire OBS context

---

## Device-Specific Notes

### MX Creative Console

- **Big wheel volume**: Use the **Audio Volume Folder**. Tap a tile on the Keypad to arm the wheel on the Dialpad, then turn to adjust.
- **Standalone volume**: Drag **Selected Source Volume** onto the big wheel. Select a source using the Audio Select folder or user-defined action.
- If only the Keypad is connected (no Dialpad), tapping an adjustment tile shows on-screen +/- controls.

### Loupedeck

- **Encoder wheel**: Works inside Audio Mixer and Scene Audio folders. Select a source with single-tap, then turn the encoder to adjust volume.
- **Encoder press**: Cycles monitoring type for the selected source.
- **Dials**: Drag **Selected Source Volume** onto any dial for dedicated volume control.

---

## Troubleshooting

### Plugin doesn't connect

- **Is OBS running?** The plugin needs OBS to be open
- **Is WebSocket enabled?** In OBS: *Tools → WebSocket Server Settings* — ensure "Enable WebSocket server" is checked
- **Firewall blocking?** The plugin connects to localhost (127.0.0.1) only — it should not be blocked
- **Try Reconnect button** — tap the Reconnect action to force a retry
- **Restart Logi Plugin Service** — sometimes a fresh restart resolves connection issues

### Buttons show "Disconnected" or appear disabled

- All actions are disabled until the plugin connects to OBS
- Wait a few seconds for automatic connection, or tap Reconnect
- If OBS crashed, restart OBS and the plugin will reconnect automatically (within 1-30 seconds)

### Audio source not responding

- Ensure you typed the **exact** source name (case-sensitive)
- Check the Audio Mixer in OBS to verify the source exists
- Some sources may not be audio sources (e.g., video captures without audio)

### Scene switch not working

- Verify the scene name is typed exactly as shown in OBS
- If using Studio Mode, the scene goes to preview first — use Studio Mode Transition to send it live
- If switching profile/collection first, allow a moment for OBS to load before the scene switches

### Volume wheel not responding

- Ensure a source is **selected** (shown with white border in audio folders)
- Use Audio Select folder or the user-defined Select Audio Source action to select
- The wheel only adjusts the globally selected source

---

## Current Limitations

- **ActionEditorCommand grouping** — User Defined Actions do not appear in their named group in the Logi software (SDK limitation, reported as bug). They appear ungrouped.
- **No dynamic dropdowns** — User Defined Actions require you to type exact names. The SDK does not support populating dropdowns from OBS at runtime.
- **Volume only via selection** — You must select a source before adjusting volume. There is no per-source dedicated volume knob.
- **No audio level meters** — Real-time VU meters are not yet implemented.
- **No filter controls** — Toggling audio/video filters is not yet available.
- **No transition selection** — Cannot choose or configure scene transitions from the device.
- **Monitoring refresh** — After cycling audio monitoring mode via the user-defined action, the monitoring state updates on the status display but may not update in all dynamic folders until you re-open them.

---

## Tips & Best Practices

1. **Start simple** — begin with Streaming Toggle, Recording Toggle, and Scenes Folder. Add more as you get comfortable.
2. **Use folders for discovery** — the Audio Mixer and Scenes folders show everything available, which helps you learn your source names.
3. **Use User Defined Actions for frequent operations** — if you mute your mic often, a dedicated mute button is faster than opening the Audio Mixer folder.
4. **Selected Source Volume is the most flexible** — drag it onto any wheel/dial and pair with Audio Select for quick access.
5. **Screenshot during streams** — the Screenshot button captures exactly what your viewers see, great for social media clips.
6. **Studio Mode for safety** — if you're worried about accidentally showing the wrong scene, enable Studio Mode and use the transition button.

---

## Getting Help

- **Report bugs**: [GitHub Issues](https://github.com/mrnil/OBSStudioForLogi/issues)
- **Source code**: [GitHub Repository](https://github.com/mrnil/OBSStudioForLogi)
- **License**: MIT — free to use, modify, and distribute
