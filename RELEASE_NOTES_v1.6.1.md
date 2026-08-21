# Release Notes — v1.6.1

No functional changes. This is a maintenance release.

## Bug Fixes

### Audio Inputs Missing from Audio Mixer and Scene Audio Folders

Sources using `game_capture` and `browser_source` input kinds were not appearing in the Audio Mixer or Scene Audio folders. These source types can carry audio and are now included in the audio input filter.

## Improvements

### Profiles and Scene Collections Folders Moved to Parent Groups

The **OBS Profiles** and **OBS Scene Collections** dynamic folders have been moved up to their parent groups in the action picker, making them easier to find alongside the related multi-state buttons:

- **OBS Profiles** folder: now in **6. Profiles › Available Profiles** (was a sub-group below)
- **OBS Scene Collections** folder: now in **7. Scenes › Available Collections** (was a sub-group below)

### Default Profiles for All Supported Devices

Default button layouts are now shipped with the plugin for all six supported device types. Logi Plugin Service applies these automatically on first install or when creating a new profile.

| File | Device |
|---|---|
| `DefaultProfile20.lp5` | Loupedeck CT |
| `DefaultProfile30.lp5` | Loupedeck Live |
| `DefaultProfile50.lp5` | Loupedeck Live S |
| `DefaultProfile70.lp5` | MX Creative Keypad |
| `DefaultProfile71.lp5` | MX Creative Dialpad |
| `DefaultProfile72.lp5` | Logitech Actions Ring |

## Dependencies

| Package | From | To |
|---|---|---|
| `System.Drawing.Common` | 9.0.0 | 10.0.10 |
| `Microsoft.Extensions.Logging.Abstractions` | 9.0.4 | 10.0.10 |
| `Microsoft.NET.Test.Sdk` | 18.7.0 | 18.8.1 |
| `actions/setup-dotnet` (CI) | 5 | 6 |

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 8.0 SDK (for development only)

## Installation

Download `OBSStudioForLogiPlugin-v1.6.1.lplug4` and install via Logi Options+ or Loupedeck software.
