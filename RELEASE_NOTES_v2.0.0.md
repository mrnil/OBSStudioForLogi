# Release Notes — v2.0.0

**Preview build on the `feat/audio-vu-meters` branch — not merged to `main`, not a tagged release.** See "Known Issue" below before building this yourself.

## New Features

### Real-Time Audio VU Meters

A new **Audio Meters** dynamic folder (`8. Audio › Meters`) shows live, per-channel volume bars for your audio inputs — one tile per input, updating up to 10 times a second by default.

- **dB scale, matching OBS's own meter**: green below -20dB, yellow -20dB to -10dB, red at -10dB and above, over the same -60dB-to-0dB range OBS uses. Normal speech now reads as a substantial, expressive bar rather than a barely-visible sliver.
- **Tap a tile to mute** that input, same as elsewhere in the plugin.
- **Configurable refresh rate** — 20/10/5 fps — in Plugin Settings, alongside the existing stats polling interval. Defaults to 10fps.
- Meters only draw data while the folder is actually open on your device; the underlying high-volume event subscription starts and stops automatically as you navigate in and out.

**Note on what you'll see**: OBS only reports levels for inputs it considers "active" — in practice this tracks your current scene, plus device-capture inputs like a microphone. An input that isn't in the live scene, or one that's active but has no audio flowing (e.g. a browser source without "Control audio via OBS" enabled), will show its title but no bars. That's expected OBS behavior, not a bug.

## Known Issue

This build depends on a **local, unpublished fork** of `obs-websocket-dotnet` (branch `feat/high-volume-event-subscription`) that adds the high-volume `InputVolumeMeters` event subscription support the meters feature needs. `src/OBSStudioForLogiPlugin.csproj` and the test project both reference it via an absolute local `ProjectReference` path rather than the published NuGet package.

**Practical effect**: this `.lplug4` package runs fine once installed, but the source on this branch will not build on another machine, and will fail CI, until either the fork is published somewhere reachable or its pull request is merged upstream and the reference reverts to the NuGet package. Track progress in `docs/ai/vu-meters-learnings.md`.

## Testing

- 435 unit tests, all passing
- Verified live against real OBS audio on a physical device — mic levels confirmed moving and color-coded correctly

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 10.0 SDK (for development only)

## Installation

Install `OBSStudioForLogiPlugin-v2.0.0.lplug4` via Logi Options+ or Loupedeck software. This is a preview build for testing the audio meters feature — not intended for wider distribution until the fork dependency above is resolved.
