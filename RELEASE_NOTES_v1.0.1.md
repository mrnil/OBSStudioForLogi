# Release Notes - v1.0.1

**Release Date**: May 22, 2026  
**Release Type**: Patch Release (Code Quality Improvements)

## Overview

This patch release focuses on code quality improvements and standardization. All display commands have been refactored to use the standardized `ButtonImageHelper` API, resulting in cleaner, more maintainable code.

## Changes

### Code Quality Improvements

#### Refactored Display Commands
- **ConnectionStatusDisplay**: Migrated to `ButtonImageHelper.Text()` API
- **CurrentProfileDisplay**: Migrated to `ButtonImageHelper.Text()` API
- **CurrentSceneDisplay**: Migrated to `ButtonImageHelper.Text()` API
- **CurrentSceneCollectionDisplay**: Migrated to `ButtonImageHelper.Text()` API

#### Code Style Improvements
- Converted `GetCommandDisplayName` to expression body syntax in ConnectionStatusDisplay
- Removed manual `BitmapBuilder` usage across all display commands
- Reduced code complexity by ~50% per file

#### UI/UX Improvements
- Updated display names for better clarity:
  - ConnectionStatusDisplay: "" → "Connection Status"
  - CurrentProfileDisplay: "" → "Current Profile"
  - CurrentSceneDisplay: "" → "Current Scene"
  - CurrentSceneCollectionDisplay: "" → "Current Collection"
- Updated package displayName: "OBS Studio" → "OBS Studio Assistant"

## Technical Details

### Benefits
- **Simplified Code**: Replaced verbose `BitmapBuilder` usage with concise `ButtonImageHelper` calls
- **Consistency**: All display commands now follow the same standardized pattern
- **Maintainability**: Easier to understand, modify, and extend
- **Project Guidelines**: Fully compliant with established coding standards

### Files Modified
- `src/Actions/ConnectionStatusDisplay.cs`
- `src/Actions/CurrentProfileDisplay.cs`
- `src/Actions/CurrentSceneDisplay.cs`
- `src/Actions/CurrentSceneCollectionDisplay.cs`
- `src/package/metadata/LoupedeckPackage.yaml`

## Installation

### New Installation
1. Download `OBSStudioForLogiPlugin-v1.0.1.lplug4`
2. Double-click the file to install via Logi Plugin Service
3. Launch OBS Studio - the plugin connects automatically

### Upgrade from v1.0.0
1. Download `OBSStudioForLogiPlugin-v1.0.1.lplug4`
2. Double-click to install (will replace v1.0.0)
3. Restart Logi Plugin Service if needed

## Requirements

- **OBS Studio**: 28.0+ with obs-websocket 5.0+
- **Logi Plugin Service**: Installed and running
- **Supported Devices**: Loupedeck CT, Live, Live S, Razer Stream Controller, Razer Stream Controller X
- **.NET Runtime**: 8.0 (included with Logi Plugin Service)

## Known Issues

None reported for this release.

## Compatibility

This release is fully backward compatible with v1.0.0. No configuration changes or user action required after upgrade.

## What's Next

See [TODO.md](TODO.md) for planned features in future releases.

## Acknowledgments

Thank you to the OBS Studio and Loupedeck communities for their continued support.

## Support

- **Issues**: https://github.com/mrnil/OBSStudioForLogi/issues
- **Homepage**: https://github.com/mrnil/OBSStudioForLogi

## License

MIT License - See [LICENSE](LICENSE) file for details.
