# Release Notes - v1.1.0

## New Features

### Adjustable Scene Switch Command
- **New Command**: "Switch to Scene (Adjustable)" in the Scenes group
- **Manual Configuration**: Enter profile name, collection name, and scene name via text inputs in the Action Editor
- **Smart Switching**: Automatically switches profile → collection → scene with appropriate delays
- **Validation**: Validates that profile/collection/scene exist before attempting to switch
- **Flexible Usage**:
  - Enter only scene name to switch scenes in current profile/collection
  - Enter collection + scene to switch collection then scene
  - Enter all three to switch profile, collection, and scene
- **Comprehensive Logging**: Detailed logs for debugging and validation

## Improvements

### Scene Collection Change Handling
- **Enhanced State Management**: Current scene is now automatically updated when scene collection changes
- **Better Synchronization**: Ensures UI displays correct active scene after collection switches
- **Improved Reliability**: 100ms delay ensures OBS has processed the collection change before querying current scene

### Logging Enhancements
- **Validation Logging**: Logs whether profile/collection/scene exists in available options
- **State Tracking**: Logs current state before and after each switch operation
- **Error Details**: Lists available options when requested item is not found
- **Final State**: Logs complete final state after all switches complete

## Technical Details

### Architecture
- **Base Class**: `ActionEditorCommand` with `ActionEditorTextbox` controls
- **Event Handling**: Wired into existing connection/profile/collection/scene change events
- **Async Operations**: Uses `Task.Run` with delays for sequential switching
- **Singleton Pattern**: Accessible via `SceneSwitchAdjustableCommand.Instance`

### Testing
- **Unit Tests**: 6 new tests for command initialization and event handlers
- **Total Tests**: 134 tests passing (100% pass rate)
- **Coverage**: Constructor, singleton instance, and all public event handlers

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 8.0 SDK (for development)

## Installation

1. Download `OBSStudioForLogiPlugin-v1.1.0.lplug4` from the release assets
2. Double-click the `.lplug4` file to install via Logi Plugin Service
3. Restart Logi Plugin Service if needed
4. Launch OBS Studio and the plugin connects automatically

## Usage

### Configuring the Adjustable Scene Switch Command

1. Drag "Switch to Scene (Adjustable)" from the Actions panel to a button
2. Click the button to open the Action Editor
3. Enter the desired values:
   - **Profile Name** (optional): Leave empty to use current profile
   - **Collection Name** (optional): Leave empty to use current collection
   - **Scene Name** (required): The scene to switch to
4. Click "Save" to apply the configuration
5. Press the button to execute the scene switch

### Examples

**Switch scene only:**
- Profile Name: (empty)
- Collection Name: (empty)
- Scene Name: `Gaming Scene`

**Switch collection and scene:**
- Profile Name: (empty)
- Collection Name: `Streaming`
- Scene Name: `Starting Soon`

**Switch everything:**
- Profile Name: `Twitch`
- Collection Name: `Live Show`
- Scene Name: `Main Camera`

## Known Issues

None reported.

## Acknowledgments

Thanks to the Loupedeck/Logitech community for feedback and testing.

## Links

- **GitHub Repository**: https://github.com/mrnil/OBSStudioForLogi
- **Issue Tracker**: https://github.com/mrnil/OBSStudioForLogi/issues
- **Documentation**: See README.md in the repository
