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
   - Choose a **Stats Polling Interval** (2s, 5s, or 10s), an **Audio Meter Refresh Rate** (20, 10 or 5 fps) and a **Double-Tap Window** (see [Double-Tap Window](#double-tap-window))
3. Press the button to save and reconnect
4. Clear the **Password** field and save the action again. The password is kept, encrypted, in the plugin's settings. An empty field keeps it, but a value left in the field is stored unencrypted in your device profile. To remove a saved password, tick **Clear Saved Password** and press the button.

Settings persist between sessions. To switch back to local OBS, check "Use Local OBS" and press the button again.

Enter an IP address, not a computer name: a value the plugin can't read as an IP address is replaced with the local address (127.0.0.1). The connection to OBS isn't encrypted. Your password isn't sent over it, but everything else is, so only connect to a remote OBS over a network you trust.

---

## Quick Setup: Streaming

Here's a suggested button layout for a basic streaming setup:

| Button | Action | What it does |
|--------|--------|-------------|
| 1 | Toggle Streaming | Start/stop your stream with one tap |
| 2 | Toggle Recording | Start/stop recording alongside your stream |
| 3 | Scenes | Open to switch between scenes (e.g., Starting Soon, Live, BRB, Ending) |
| 4 | Current Scene | Always shows which scene is live |
| 5 | Mixer for All Audio | Quick access to every audio source (double-tap to mute/unmute) |
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
| 1 | Toggle Recording | Start/stop recording |
| 2 | Recording Pause | Pause/resume without creating a new file |
| 3 | Scenes | Switch between camera angles or screen shares |
| 4 | Scene Sources | Toggle source visibility (show/hide overlays) |
| 5 | Toggle Audio Mute (User defined) | One button to mute/unmute your microphone |
| 6 | Current Scene | See which scene is active |

---

## All Available Actions

### 1. OBS

| Action | Type | Description |
|--------|------|-------------|
| Screenshot | Button | Captures a screenshot of the current scene. Saves to your Pictures, Documents, or Desktop folder. |
| Reconnect to OBS | Button | Manually retry the connection to OBS. Its colour shows the connection state. |
| Toggle Studio Mode | Button | Enable/disable OBS studio mode. When enabled, you can preview scenes before sending them live. |
| Studio Mode Transition | Button | Send the preview scene to program (live). Only works when studio mode is on. |
| Connection Status | Display | Shows "Connected" (green), "Disconnected" (red), or "WebSocket Disabled" (orange). Read-only. |
| Stats Summary | Display | Shows FPS, CPU%, and dropped frames. Green = healthy, red = problems. |
| Stats Folder | Folder | Individual tiles: FPS, CPU, Memory, Render Missed, Encode Skipped, Total Dropped, Disk Space, Render Time. Colour-coded thresholds. |
| Plugin Settings | Button | Configure the OBS connection (local/remote), IP, port, password, stats polling interval, audio meter refresh rate and double-tap window. Press to save. |

### 2. Streaming

| Action | Type | Description |
|--------|------|-------------|
| Toggle Streaming | Button | Start streaming if stopped, stop if streaming. Single button for both. |
| Start Streaming | Button | Start streaming only. Greyed out, and does nothing when pressed, while streaming or while OBS is starting or stopping the stream. |
| Stop Streaming | Button | Stop streaming only. Greyed out, and does nothing when pressed, while not streaming or while OBS is starting or stopping the stream. |
| Stream Stats Folder | Folder | Live stream statistics: Duration, Bytes Sent, Network Congestion, Skipped Frames, Total Frames. Shows "Offline" when not streaming. |

### 3. Recording

| Action | Type | Description |
|--------|------|-------------|
| Toggle Recording | Button | Start recording if stopped, stop if recording. The icon is red while recording and green while not. |
| Start Recording | Button | Start recording only. Greyed out, and does nothing when pressed, while recording or while OBS is starting or stopping the recording. |
| Stop Recording | Button | Stop recording only. Greyed out, and does nothing when pressed, while not recording or while OBS is starting or stopping the recording. |
| Recording Pause | Button | Pause active recording. Tap again to resume. Useful for breaks without creating a new file. Greyed out, and does nothing when pressed, while nothing is recording. |

### 4. Replay Buffer

| Action | Type | Description |
|--------|------|-------------|
| Toggle Replay Buffer | Button | Start/stop the replay buffer. |
| Save Replay | Button | Save the last N seconds to disk (configured in OBS). Button flashes green on successful save. Great for capturing highlights. |

### 5. Virtual Camera

| Action | Type | Description |
|--------|------|-------------|
| Toggle Virtual Camera | Button | Start/stop the virtual camera output. Useful for video calls. |
| Start Virtual Camera | Button | Start virtual camera only. Greyed out while the virtual camera is on or starting. |
| Stop Virtual Camera | Button | Stop virtual camera only. Greyed out while the virtual camera is off or stopping. |

### 6. Profiles

| Action | Type | Description |
|--------|------|-------------|
| Available Profiles | Button (one per profile) | Switch to that profile. Ticked while it is active. |
| Profiles | Folder | Opens a folder showing all available profiles. Tap one to switch. |
| Current Profile | Display | Shows the name of the active profile. Read-only. |

### 7. Scenes

| Action | Type | Description |
|--------|------|-------------|
| Available Scenes | Button (one per scene) | Switch to that scene. Ticked while it is live. In studio mode, this sets the preview. |
| Available Collections | Button (one per collection) | Switch to that scene collection. Ticked while it is active. |
| Scenes | Folder | Opens a folder showing all scenes. Tap to switch. In studio mode, this sets the preview (not live). |
| Scene Collections | Folder | Opens a folder showing all scene collections. Tap to switch. |
| Scene Sources | Folder | Opens a folder showing all sources in the current scene. Tap to toggle visibility on/off. |
| Current Scene | Display | Shows the name of the active scene. Read-only. |
| Current Collection | Display | Shows the name of the active scene collection. Read-only. |

### 8. Audio

| Action | Type | Description |
|--------|------|-------------|
| Mixer for All Audio | Folder | All audio inputs. Single-tap to select for wheel control, double-tap to mute/unmute. Shows volume in dB and monitoring mode. |
| Mixer for Scene Audio | Folder | Same as Mixer for All Audio, for the audio sources in the current scene plus audio inputs that aren't in any scene. |
| Available Sources | Button (one per input) | Mute/unmute that audio input. |
| Audio Select | Folder | Selection-only folder. Single tap to select/deselect a source for global wheel/dial control. No mute action. |
| Audio Volume | Folder | MX big wheel compatible. Tap a tile to arm the wheel for that source's volume, then turn the wheel to adjust. |
| Selected Source Volume | Adjustment | Standalone volume control. Drag onto any wheel or dial. Controls volume of the globally selected audio source. Press to reset to 100%. |
| Live Audio Folder | Folder | Live VU meters for audio sources that are currently live in OBS. Tap a tile to mute/unmute. See [Audio Meters](#audio-meters). |
| Audio Meter | Button (one per source) | A VU meter for one specific audio source, live or not. Tap to mute/unmute. See [Audio Meters](#audio-meters). |

### User Defined Actions

These actions are configurable: you type in the names of the OBS items you want to control. They are in the **User Defined** sub-groups of Scenes and Audio; Media Action is under Media.

| Action | Configuration | Description |
|--------|--------------|-------------|
| Switch to Scene | Profile (optional), Collection (optional), Scene (required) | Switch to a specific scene. Optionally switch profile and collection first. |
| Toggle Source Visibility | Scene (optional), Source name(s) (required) | Toggle visibility of named sources. Comma-separate for multiple. Defaults to current scene. |
| Toggle Audio Mute | Audio source name (required) | Mute/unmute a specific audio source. |
| Cycle Audio Monitoring | Audio source name (required) | Cycle: None → Monitor Only → Monitor & Output. |
| Select Audio Source | Audio source name (required) | Toggle this source as the globally selected audio source for wheel/dial control. |
| Audio Source Status | Audio source name (required) | Display-only. Shows mute state, volume in dB, monitoring mode, and selection border. Updates in real-time. |
| Media Action | Source name (required), Action (required) | Trigger a media action on a named source. Actions: Play, Pause, Stop, Restart, Next, Previous. |

---

## Media Controls

The **Media Controls** folder (`9. Media`) shows all media sources in OBS (video clips, VLC sources, slideshows, text sources).

### Tap Behaviour

| Current State | Single Tap | Double Tap |
|---------------|------------|------------|
| Stopped/Ended | Play | Stop (no change) |
| Paused | Play (resume) | Stop (reset to start) |
| Playing | Pause | Stop (reset to start) |

With double-tap turned off, use **Media Action (User Defined)** with the Stop action to stop a source.

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

- **Mixer for All Audio** / **Mixer for Scene Audio**: double-tap any source to toggle mute
- **Available Sources**, **Audio Meter** and **Live Audio Folder**: a single tap toggles mute
- **Toggle Audio Mute (User defined)**: a dedicated button that mutes a specific source

### Double-Tap Window

Mixer for All Audio, Mixer for Scene Audio and Media Controls tell a single tap from a double tap by waiting briefly for a second tap, so a single tap acts only after that wait. Set the wait with **Double-Tap Window** in Plugin Settings:

- **200ms**: single taps act soonest, but double taps have to be quick
- **300ms** (default)
- **500ms**: the most time for a double tap, and the longest wait for a single tap
- **Off**: every tap is a single tap and acts at once. Mute with **Audio Meter** or **Toggle Audio Mute (User defined)**, and stop media with **Media Action (User Defined)**

### Volume Control

Volume requires two steps: **select** a source, then **adjust** with a wheel/dial.

**Selecting a source:**

- Single-tap in Mixer for All Audio or Mixer for Scene Audio
- Tap in the Audio Select folder
- Use "Select Audio Source" user-defined action

**Adjusting volume:**

- **MX Creative Console**: use the Audio Volume folder (tap tile, turn big wheel)
- **Loupedeck CT**: use the encoder wheel while in one of the mixer folders
- **Any device** — drag "Selected Source Volume" onto any wheel or dial

### Visual Feedback

All audio buttons use colour coding:

- **Green text** = unmuted
- **Red text** = muted
- **White border** = currently selected for wheel/dial control
- Volume shown in dB format (e.g., "0.0 dB", "+6.0 dB", "-∞ dB")

### Audio Monitoring

Monitoring controls whether you hear a source in your headphones:

- **None** — source goes to stream/recording only
- **Monitor Only** — source plays in your headphones only (not stream)
- **Monitor & Output** — source plays in headphones AND goes to stream

Cycle through these with the "Cycle Audio Monitoring" user-defined action, or press the encoder in the mixer folders.

### Audio Meters

There are two ways to see audio levels, both in the **Audio › Meters** group:

- **Live Audio Folder** shows a meter for each audio source OBS is currently sending to your program output — for example your microphone and desktop audio, plus any audio sources in the live scene. The folder updates itself as you switch scenes or show, hide, add or remove sources, so you only ever see sources that are actually live.
- **Audio Meter** lists every audio source, live or not. Drag the one you want onto a button to keep a meter for that source in a fixed place.

What a meter shows:

- **Bars** — one per channel, on the same -60dB to 0dB scale as OBS's own mixer. Green below -20dB, yellow from -20dB to -10dB, red at -10dB and above.
- **Thin grey line under the bars** — the source is live but silent.
- **Red crossed-out speaker** — the source is live but muted. Tap to unmute it.
- **Grey crossed-out speaker** — the source isn't live right now (for example, it's only in a scene that isn't on air).

Tapping any meter toggles mute for that source.

Change how often the meters redraw (20, 10 or 5 fps) in Plugin Settings.

---

## Studio Mode

Studio mode lets you preview a scene before sending it live:

1. Enable studio mode with the **Toggle Studio Mode** button
2. When you tap a scene in the Scenes folder, it goes to **preview** (not live). The scene buttons don't show the preview scene yet, so the tick stays on the live scene until you transition
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

1. Drag "Switch to Scene (User Defined)" onto a button
2. Configure:
   - Profile Name: `Streaming` (optional)
   - Collection Name: `Main Show` (optional)
   - Scene Name: `Gaming` (required)
3. One tap switches your entire OBS context

---

## Device-Specific Notes

### MX Creative Console

- **Big wheel volume**: Use the **Audio Volume** folder. Tap a tile on the Keypad to arm the wheel on the Dialpad, then turn to adjust.
- **Standalone volume**: Drag **Selected Source Volume** onto the big wheel. Select a source using the Audio Select folder or user-defined action.
- If only the Keypad is connected (no Dialpad), tapping an adjustment tile shows on-screen +/- controls.

### Loupedeck

- **Encoder wheel**: Works inside the Mixer for All Audio and Mixer for Scene Audio folders. Select a source with single-tap, then turn the encoder to adjust volume.
- **Encoder press**: Cycles monitoring type for the selected source.
- **Dials**: Drag **Selected Source Volume** onto any dial for dedicated volume control.

---

## Troubleshooting

### Plugin doesn't connect

- **Is OBS running?** The plugin needs OBS to be open
- **Is WebSocket enabled?** In OBS: *Tools → WebSocket Server Settings* — ensure "Enable WebSocket server" is checked. If it's disabled, the Connection Status button will show "WebSocket Disabled" (orange).
- **Firewall blocking?** A local OBS is reached on 127.0.0.1, which firewalls don't usually block. For a remote OBS, allow the WebSocket port (default 4455) on the remote machine
- **Try Reconnect button** — tap the Reconnect action to force a retry
- **Restart Logi Plugin Service** — sometimes a fresh restart resolves connection issues

### Buttons show "Disconnected" or appear greyed out

- Actions are greyed out until the plugin connects to OBS
- Wait a few seconds for the automatic connection, or tap Reconnect to OBS
- If OBS crashed, restart OBS and the plugin reconnects automatically (it retries after 1s, then backs off to every 30s)
- If you changed the WebSocket password or port in OBS, the plugin picks up the new settings on its next attempt

### Audio source not responding

- Ensure you typed the **exact** source name (case-sensitive)
- Check the Audio Mixer in OBS to verify the source exists
- Renaming a source in OBS breaks user-defined buttons that use the old name: update the name in the button's settings
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

- **Typed names**: User Defined Actions require you to type exact names; there are no lists to pick from yet.
- **Volume only via selection**: You must select a source before adjusting volume. There is no per-source dedicated volume knob.
- **Studio mode preview**: Scene buttons show the live scene, not the preview scene.
- **OBS groups**: Sources inside an OBS group can't be toggled from the Scene Sources folder; the group shows as one button.
- **No filter controls**: Toggling audio/video filters is not yet available.
- **No transition selection**: Cannot choose or configure scene transitions from the device.

---

## Tips & Best Practices

1. **Start simple** — begin with Toggle Streaming, Toggle Recording, and the Scenes folder. Add more as you get comfortable.
2. **Use folders for discovery** — the Mixer for All Audio and Scenes folders show everything available, which helps you learn your source names.
3. **Use dedicated buttons for frequent operations** — if you mute your mic often, a Toggle Audio Mute or Available Sources button is faster than opening a mixer folder.
4. **Selected Source Volume is the most flexible** — drag it onto any wheel/dial and pair with Audio Select for quick access.
5. **Screenshot during streams** — the Screenshot button captures exactly what your viewers see, great for social media clips.
6. **Studio Mode for safety** — if you're worried about accidentally showing the wrong scene, enable Studio Mode and use the transition button.

---

## Getting Help

- **Report bugs**: [GitHub Issues](https://github.com/mrnil/OBSStudioForLogi/issues)
- **Source code**: [GitHub Repository](https://github.com/mrnil/OBSStudioForLogi)
- **License**: MIT — free to use, modify, and distribute
