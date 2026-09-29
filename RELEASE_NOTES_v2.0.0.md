# Release Notes — v2.0.0

A large release: real-time audio meters, a restyled icon set, a more secure home for the remote OBS password, and a plugin that asks OBS for much less and keeps up with changes made in OBS itself.

## New Features

### Real-Time Audio VU Meters

Two new ways to watch audio levels, both in `8. Audio › Meters`:

- The **Live Audio Folder** shows per-channel volume bars for every audio input that is live in OBS, one tile per input. It follows OBS's own idea of what's live (the current scene's audio sources plus global devices such as your microphone) and updates itself as you switch scenes or show, hide, add, remove or rename sources.
- The **Audio Meter** action lists every audio input, live or not, so you can put a meter for a specific source on any button.

Both use the same dB scale as OBS's own mixer (-60dB to 0dB, green below -20dB, yellow to -10dB, red above). Tap a meter to mute its input. A thin grey baseline means the input is live but silent, a red crossed-out speaker means it is muted, and a grey crossed-out speaker means it isn't live. The refresh rate (20, 10 or 5 fps, default 10) is in Plugin Settings. The high-volume meter data is only requested from OBS while a meter is on screen.

### Restyled Icons

Every button icon has been redrawn as one consistent set: outline glyphs, a fixed five-colour palette, a slash for off states, grey for actions that aren't available, and a tick on the current scene, scene collection and profile.

- Virtual Camera Start/Stop show a camera instead of the recording dot and square
- Streaming Start/Stop grey out when unavailable, like Recording and Virtual Camera
- Toggle Source Visibility (User Defined) shows whether its source is visible
- Switch to Scene (User Defined) shows a tick when its scene is live
- The Profiles, Scene Collections, Media Controls and OBS Scenes folders have their own icons
- The `OBS Profiles` and `OBS Scene Collections` folders are renamed `Profiles` and `Scene Collections`

## Security

### Remote OBS Password No Longer Stored in Plain Text

- The remote OBS password is no longer written to `config.json`. It is kept in the plugin settings, which the Logi Plugin Service stores encrypted. A password saved by an earlier version is moved there automatically the first time the plugin loads.
- The Logi Plugin Service stores Action Editor fields unencrypted in your device profile. An empty Password field now keeps the saved password, so **after saving, clear the Password field and save the action again**. A new **Clear Saved Password** checkbox removes a saved password.

## Performance

The plugin now asks OBS only for what the device is showing, and button redraws no longer wait on OBS:

- The Scene Sources and Mixer for Scene Audio folders load their lists when opened, showing "Loading..." until they arrive. With neither open, a scene change requests no source lists; with one open, it takes one request instead of one per scene.
- OBS stats are polled only while a stats folder or the OBS Stats Summary button is on screen, and straight away when one appears.
- Audio, source visibility and media buttons are drawn from cached state kept current by OBS events, so a slow OBS no longer stalls the device.
- Scene source lists and input lists load in the background instead of holding up other OBS updates. When you switch scenes quickly, only the latest scene's sources are shown.
- OBS requests that never get a response now time out and are logged instead of blocking.

## Bug Fixes

- **Renames in OBS are followed.** Renaming an input or scene updates the Mixer, Live Audio, Media and Scene Sources folders and the scene list straight away. The renamed input stays selected on the dial, and its mute, volume, visibility and playback state carry over. User-defined buttons keep the name you typed, so the plugin log warns which name to change them to.
- **Profile and scene collection changes are followed.** Creating, renaming or removing a profile or scene collection in OBS updates the Profiles and Scene Collections folders straight away.
- **Connecting while OBS is starting works.** Previously, if OBS was restarted with the plugin running, the audio folders, profiles and studio mode could stay empty. The plugin now retries for up to 30 seconds while OBS finishes loading.
- **No stats errors while switching scene collection.** Stats polling pauses during the switch and refreshes as soon as it finishes.
- **Media buttons** show Paused and Stopped as soon as media changes, including from OBS's own controls, and refresh when playback starts.
- **The Audio Volume folder** no longer shows each input's name and volume twice.
- An error while connecting after OBS starts is now logged instead of going unhandled.

## Dependencies

- `obs-websocket-dotnet` upgraded from 5.0.1 to 5.7.0 (needed for the audio meters, contributed upstream)
- `Microsoft.Extensions.Logging.Abstractions` and `System.Drawing.Common` upgraded to 10.0.12

## Testing

- 723 unit tests, all passing (up from 393 at v1.6.2)
- Test timing flakiness removed: background work in the action executor tests now runs inline instead of waiting on fixed sleeps
- All fixes in this release checked on a physical device against a real OBS

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 10.0 SDK (for development only)

## Installation

Download `OBSStudioForLogiPlugin-v2.0.0.lplug4` and install via Logi Options+ or Loupedeck software.

If you use a remote OBS connection, open the Plugin Settings action after upgrading, clear the Password field and save it again.
