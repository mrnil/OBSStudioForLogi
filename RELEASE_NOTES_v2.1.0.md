# Release Notes — v2.1.0

Faster taps, clearer buttons, and a plugin that copes with OBS settings changes on its own.

## New

### Double-Tap Window Setting

Mixer for All Audio, Mixer for Scene Audio and Media Controls tell a single tap from a double tap by waiting briefly for a second tap. That wait is now 300ms instead of 500ms, so selecting an audio source or pausing media responds sooner.

You can change the wait with the new **Double-Tap Window** option in **Plugin Settings**:

| Setting | Effect |
|---|---|
| 200ms | Single taps act soonest; double taps have to be quick |
| 300ms (default) | A balance of the two |
| 500ms | The old behaviour: the most time for a double tap |
| Off | Every tap acts at once, and double tap does nothing |

With double-tap off, mute a source with **Audio Meter** or **Toggle Audio Mute (User defined)**, and stop media with **Media Action (User Defined)** set to Stop.

### Action Symbols

Every action now has its own symbol in the Logi Options+ action picker, instead of a generic one.

## Changes

- **Start and Stop buttons are greyed out when they can't do anything.** This covers streaming, recording and the virtual camera: Start while the output is running, Stop while it's stopped, either while OBS is starting or stopping the output, and both while disconnected. Greyed buttons ignore presses. Recording Pause is greyed out while nothing is recording.
- **Toggle Recording is now a status key.** Its record dot is red while recording, and its stop square is green while not.
- **Source visibility toggles respond faster**, especially with OBS on another computer: a toggle now takes two requests to OBS instead of four.

## Fixes

- **Streaming and Recording buttons update straight away** when OBS changes state, including the starting and stopping states and changes made in OBS's own window. Before, they could show the old state until the page was redrawn.
- **No manual Reconnect after changing OBS's WebSocket settings.** If you change the WebSocket password or port in OBS while the plugin is connected, the plugin now picks up the new settings on its next automatic attempt. Before, it kept retrying the old ones.
- **Button icons show their own colours** on the device: blue when available, grey when unavailable, red when muted. The device software was tinting them all white, so greyed-out buttons looked the same as available ones.
- **The plugin icon** now meets the Logi Options+ and Marketplace requirements.
- **The plugin reports the right version** in its log. 2.0.1 still reported 2.0.0.

## Upgrading

Nothing to do. Your buttons and settings carry over, and the double-tap wait starts at the new 300ms default. To keep the old 500ms behaviour, choose 500ms under Double-Tap Window in Plugin Settings and press the button to save.

## Testing

- 823 unit tests, all passing

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 10.0 SDK (for development only)

## Installation

Download `OBSStudioForLogiPlugin-v2.1.0.lplug4` and install via Logi Options+ or Loupedeck software.
