# Release Notes - v1.4.1

## Audio Input Detection Fix

This patch release fixes a bug where OBS 28+ per-application audio capture sources (e.g., Spotify, Discord, game audio) were not appearing in the Audio Mixer or Scene Audio folders.

---

## Fixed

- **Missing audio sources** — Added `wasapi_process_output_capture` to the audio input kind filter. This is the input type used by OBS 28+ "Application Audio Capture" sources (per-app audio capture). Previously, these sources were silently excluded from all audio folders and controls.

---

## Impact

Users who have added per-application audio capture sources in OBS (Tools → Application Audio Capture) will now see them in:

- Audio Mixer Folder
- Scene Audio Folder
- Audio Select Folder
- Audio Volume Folder

All audio controls (mute/unmute, volume adjustment, monitoring) work with these sources.

---

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 8.0 runtime

---

## Installation

1. Download `OBSStudioForLogiPlugin-v1.4.1.lplug4`
2. Double-click to install, or import via Logi Options+
3. Launch OBS Studio — the plugin connects automatically

---

## Test Status

359 unit tests, all passing.
