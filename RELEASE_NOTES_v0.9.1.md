# Release v0.9.1 - Bug Fixes and Icon Updates

## 🐛 Bug Fixes

### Source Visibility
- **Fixed source visibility toggle** - Now works bidirectionally (visible ↔ hidden)
  - Previously only worked in one direction due to incorrect state checking
  - Added proper `GetSceneItemEnabled` implementation to query actual visibility state
- **Fixed source visibility icon updates** - Icons now update immediately after toggling
  - Implemented delayed callback pattern (100ms) to allow OBS to process state changes
  - Icons correctly show SourceVisibilityOn/Off based on actual state

### Scene Management
- **Fixed scene icon updates** - Scene icons now update in real-time when switching scenes
  - Changed from `ButtonActionNamesChanged()` to `CommandImageChanged()` for efficiency
  - Only refreshes the two affected scene buttons (old unselected, new selected)
  - Provides immediate visual feedback

### Profile Management
- **Fixed profile icon updates** - Profile icons now update in real-time when switching profiles
  - Changed from `ButtonActionNamesChanged()` to `CommandImageChanged()` for efficiency
  - Only refreshes the two affected profile buttons (old unselected, new selected)
  - Provides immediate visual feedback

### Audio Controls
- **Fixed audio button color updates** - Mute state colors now update immediately
  - Changed from `ButtonActionNamesChanged()` to `CommandImageChanged()` for individual buttons
  - Red text for muted, green text for unmuted
- **Fixed audio volume display updates** - Volume percentage now updates in real-time
  - Subscribed to `InputVolumeChanged` event for real-time updates
  - Volume changes in OBS immediately reflected on buttons

## 🎨 UI/UX Improvements

- **Improved audio button readability** - Reduced font size from 18pt/16pt to 16pt/14pt (Width90/Width60)
- **Enhanced display buttons** - Dynamic font sizing based on text length for better fit
- **Cleaner button appearance** - Hidden display name overlays on all display commands
- **Better reconnect icon** - Added electrical plug icon for Reconnect button

## 🧹 Code Quality

- **Removed debug logging** - Cleaned up verbose debug logging from audio functionality
- **Improved logging clarity** - Simplified audio button logging with clear tags
- **Better color handling** - Use explicit RGB values for mute state colors

## 📚 Documentation

- **Created icon update patterns guide** - Comprehensive documentation of best practices
  - Documented CommandImageChanged pattern for individual button updates
  - Documented delayed callback pattern for toggle operations
  - Added anti-patterns to avoid
  - Included event flow examples
- **Updated memory bank** - Corrected font sizes and added icon update documentation
- **Updated TODO list** - Reflects current status and prioritizes audio features

## 🔧 Technical Details

### Icon Update Pattern
All dynamic folders now use the efficient `CommandImageChanged(actionParameter)` pattern:
- **Scenes**: Updates only old and new scene icons when switching
- **Profiles**: Updates only old and new profile icons when switching
- **Sources**: Updates individual source icon after toggle with 100ms delay
- **Audio**: Updates individual audio button when mute/volume changes

### Delayed Callback Pattern
Source visibility toggles now use a delayed callback to ensure consistency:
1. Send toggle command to OBS
2. Wait 100ms for OBS to process
3. Trigger icon refresh via callback
4. Query updated state from OBS
5. Display correct icon

## 📦 Installation

1. Download the release
2. Extract to your Logi Plugin Service plugins directory
3. Restart Logi Plugin Service or use hot-reload
4. Launch OBS Studio - plugin connects automatically

## 🔗 Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 8.0 Runtime

## 🙏 Acknowledgments

Thanks to all users who reported the icon update issues and provided feedback on the audio controls!

---

**Full Changelog**: v0.9.0...v0.9.1
