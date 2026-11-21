# v2.1.1 - Updated libraries

A maintenance release. The libraries the plugin uses are updated. Nothing changes in how the plugin works, and you don't need to change any settings.

## Changes

- `Microsoft.Extensions.Logging.Abstractions` 10.0.14 → 11.0.0
- `System.Drawing.Common` 10.0.14 → 11.0.0 (used to draw button images)
- Test project only: `Microsoft.NET.Test.Sdk` 18.11.0 → 18.12.0 and `coverlet.collector` 10.1.0 → 10.1.1

## Testing

- 823 unit tests, all passing locally and in CI

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 10.0 SDK (for development only)

## Installation

Download `OBSStudioForLogiPlugin-v2.1.1.lplug4` and install via Logi Options+ or Loupedeck software.
