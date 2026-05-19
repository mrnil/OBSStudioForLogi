# Release Notes - v0.9.2

## Display Order Fix

This patch release fixes the display order of scenes and sources to match OBS Studio's top-to-bottom order.

## Bug Fixes

### Scene and Source Display Order
- **Fixed**: Scenes and sources now appear in the same top-to-bottom order as displayed in OBS Studio
- **Root Cause**: OBS WebSocket API returns items in reverse order (bottom-to-top)
- **Solution**: Arrays are now reversed after retrieval to match OBS display order
- **Impact**: First scene/source in OBS now appears as first button in plugin folders

## Technical Details

### Modified Components
- `OBSWebsocketAdapter.GetSceneList()`: Added `Array.Reverse()` after retrieving scenes
- `OBSWebsocketAdapter.GetSceneItemList()`: Added `Array.Reverse()` after retrieving scene items

### Files Changed
- `src/Services/OBSWebsocketAdapter.cs`

## Installation

### For Users
1. Download `OBSStudioForLogiPlugin-v0.9.2.lplug4`
2. Double-click to install via Logi Plugin Service
3. Plugin will automatically reload

### For Developers
```bash
dotnet build src/OBSStudioForLogiPlugin.csproj -c Release
```

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 8.0 SDK (for development)

## Upgrade Notes

This is a patch release with no breaking changes. Simply install over v0.9.1 or earlier versions.

## Acknowledgments

Thanks to users who reported the display order inconsistency between OBS Studio and the plugin.

---

**Full Changelog**: https://github.com/yourusername/OBSStudioForLogiPlugin/compare/v0.9.1...v0.9.2
