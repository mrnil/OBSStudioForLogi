# Release Notes — v2.0.0

**Draft — merged to `main` but not yet released or tagged.** More work is planned before the next release; the version number may change.

## New Features

### Real-Time Audio VU Meters

Two new ways to watch audio levels, both in `8. Audio › Meters`, updating up to 10 times a second by default:

- The **Live Audio Folder** shows live, per-channel volume bars for the audio inputs that are currently live in OBS — one tile per input. It follows OBS's own idea of what's live (the current scene's audio sources plus global devices like your microphone) and updates itself as you switch scenes or show, hide, add, remove or rename sources.
- The **Audio Meter** action lists every audio input, live or not, so you can put a meter for a specific source on any button.

Both share these behaviours:

- **dB scale, matching OBS's own meter**: green below -20dB, yellow -20dB to -10dB, red at -10dB and above, over the same -60dB-to-0dB range OBS uses. Normal speech now reads as a substantial, expressive bar rather than a barely-visible sliver.
- **Tap a meter to mute** that input, same as elsewhere in the plugin. A live but muted input shows a red crossed-out speaker; an input that isn't live shows a grey crossed-out speaker.
- **Silent vs. no data**: a thin grey baseline under the bars means the input is live but silent, so a quiet mic no longer looks broken.
- **Configurable refresh rate** — 20/10/5 fps — in Plugin Settings, alongside the existing stats polling interval. Defaults to 10fps.
- Meters only draw data while they're actually on screen; the underlying high-volume event subscription starts and stops automatically as you navigate in and out.

**Note on what you'll see**: OBS only reports levels for inputs it considers "active" — in practice this tracks your current scene, plus device-capture inputs like a microphone. Inputs outside the live scene don't appear in the folder. An active input with no audio flowing yet (e.g. a browser source that isn't playing anything) appears with just the silent baseline. When you open the folder, tiles appear a moment later, once OBS sends its first levels.

## Dependency Update

`obs-websocket-dotnet` is upgraded from 5.0.1 to **5.7.0**, the first published release with the high-volume `InputVolumeMeters` event subscription support the meters feature needs (contributed upstream as BarRaider/obs-websocket-dotnet PR #150). Earlier preview builds of this branch depended on a local, unpublished fork of the library; the source now builds anywhere from the public NuGet package.

## Testing

- 466 unit tests
- Verified live against real OBS audio on a physical device — mic levels confirmed moving and color-coded correctly

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 10.0 SDK (for development only)

## Installation

Install `OBSStudioForLogiPlugin-v2.0.0.lplug4` via Logi Options+ or Loupedeck software. This is a preview build for testing the audio meters feature.
