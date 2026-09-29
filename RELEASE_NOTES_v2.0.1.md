# Release Notes — v2.0.1

A small tidy-up after v2.0.0.

## Changes

### Consistent Action Names

v2.0.0 dropped the "OBS" prefix from the Profiles and Scene Collections folders. Three actions were missed, and are now renamed to match:

| Before | After |
|---|---|
| OBS Scenes | Scenes |
| OBS Stats Folder | Stats Folder |
| OBS Stats Summary | Stats Summary |

Only the names change. Buttons already on your device keep working.

## Removed

### OBS Volume Wheel Tool

The "OBS Volume" wheel tool was built for the Loupedeck CT's centre wheel and never appeared in the action list on any device. To change the selected audio source's volume from a wheel, dial or roller, use **Selected Source Volume** in `8. Audio`.

## Testing

- 723 unit tests, all passing

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 10.0 SDK (for development only)

## Installation

Download `OBSStudioForLogiPlugin-v2.0.1.lplug4` and install via Logi Options+ or Loupedeck software.
