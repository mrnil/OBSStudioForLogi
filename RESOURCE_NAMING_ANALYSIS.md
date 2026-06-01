# Resource Naming Analysis Report

## Overview

Analysis of icon resource naming patterns across the OBSStudioForLogiPlugin to identify consistency, clarity, and areas for improvement.

---

## Resource Location

**Current**: `src/Icons/` (44 files)
**SDK Recommended**: `src/Resources/icons/`

**Note**: This is an organizational difference only. Functionality is not affected.

---

## Naming Pattern Analysis

### ✅ Clear and Consistent Patterns

#### 1. Recording Resources (8 files)
```
RecordingOn.svg              ← Toggle active state
RecordingOff.svg             ← Toggle inactive state
RecordingStart.svg           ← Start command enabled
RecordingStartDisabled.svg   ← Start command disabled
RecordingStop.svg            ← Stop command enabled
RecordingStopDisabled.svg    ← Stop command disabled
RecordingPause.svg           ← Pause active state
RecordingResume.svg          ← Resume/unpause state
```

**Pattern**: `Recording{Action}.svg` or `Recording{Action}Disabled.svg`
**Clarity**: ✅ Excellent - Clear action names, consistent disabled suffix
**Usage**: All used correctly in commands

#### 2. Virtual Camera Resources (6 files)
```
VirtualCameraOn.svg              ← Toggle active state
VirtualCameraOff.svg             ← Toggle inactive state
VirtualCameraStart.svg           ← Start command enabled
VirtualCameraStartDisabled.svg   ← Start command disabled
VirtualCameraStop.svg            ← Stop command enabled
VirtualCameraStopDisabled.svg    ← Stop command disabled
```

**Pattern**: `VirtualCamera{Action}.svg` or `VirtualCamera{Action}Disabled.svg`
**Clarity**: ✅ Excellent - Matches recording pattern perfectly
**Usage**: All used correctly in commands

#### 3. Profile Resources (2 files)
```
ProfileSelected.svg      ← Selected state
ProfileUnselected.svg    ← Unselected state
```

**Pattern**: `Profile{State}.svg`
**Clarity**: ✅ Excellent - Clear selected/unselected states
**Usage**: Used in ProfilesDynamicFolder and ProfileSelectCommand

#### 4. Scene Resources (2 files)
```
ScenesSelected.svg       ← Selected state
ScenesUnselected.svg     ← Unselected state
```

**Pattern**: `Scenes{State}.svg`
**Clarity**: ✅ Excellent - Matches profile pattern
**Usage**: Used in ScenesDynamicFolder

#### 5. Scene Collection Resources (2 files)
```
ScenesCollectionsSelected.svg      ← Selected state
ScenesCollectionsUnselected.svg    ← Unselected state
```

**Pattern**: `ScenesCollections{State}.svg`
**Clarity**: ✅ Good - Clear but note plural "Collections"
**Usage**: Used in SceneCollectionSelectCommand

#### 6. Source Visibility Resources (2 files)
```
SourceVisibilityOn.svg     ← Visible state
SourceVisibilityOff.svg    ← Hidden state
```

**Pattern**: `SourceVisibility{State}.svg`
**Clarity**: ✅ Excellent - Clear on/off states
**Usage**: Used in SourcesDynamicFolder

#### 7. Audio Resources (4 files)
```
AudioMixerMuted.svg      ← Muted state
AudioMixerUnmuted.svg    ← Unmuted state
AudioMediaFolder.svg     ← Folder icon
AudioFilterDisabled.svg  ← Filter disabled (unused?)
```

**Pattern**: `Audio{Component}{State}.svg`
**Clarity**: ✅ Good - Clear component and state
**Note**: AudioFilterDisabled.svg may be unused

#### 8. Utility Resources (4 files)
```
Screenshot.svg              ← Screenshot action
Reconnect.svg              ← Reconnect action
StudioModeTransition.svg   ← Studio mode transition
ReplayBufferSave.svg       ← Save replay buffer
```

**Pattern**: `{Action}.svg`
**Clarity**: ✅ Excellent - Simple, descriptive names
**Usage**: All used correctly

---

### ⚠️ Inconsistent Patterns

#### 1. Streaming Resources (2 files) - INVERTED LOGIC
```
StreamingToggleOn.svg      ← Used for INACTIVE state ❌
StreamingToggleOff.svg     ← Used for ACTIVE state ❌
```

**Issue**: Icon names are inverted from their actual usage

**Code Usage**:
```csharp
// StreamingToggleCommand.cs
protected override String GetActiveIcon() => "StreamingToggleOff.svg";   // ❌ Confusing
protected override String GetInactiveIcon() => "StreamingToggleOn.svg";  // ❌ Confusing
```

**Expected Pattern**: Should match Recording pattern
```
StreamingOn.svg      ← Active state
StreamingOff.svg     ← Inactive state
```

**Impact**: High confusion risk for maintenance
**Recommendation**: Rename files to match actual usage or fix code logic

#### 2. Studio Mode Resources (2 files) - INVERTED LOGIC
```
StudioModeToggleOn.svg     ← Used for ACTIVE state ✅
StudioModeToggleOff.svg    ← Used for INACTIVE state ✅
```

**Code Usage**:
```csharp
// StudioModeToggleCommand.cs
protected override String GetActiveIcon() => "StudioModeToggleOn.svg";    // ✅ Correct
protected override String GetInactiveIcon() => "StudioModeToggleOff.svg"; // ✅ Correct
```

**Pattern**: Correct usage but inconsistent naming with other toggles
**Note**: Includes "Toggle" in name unlike Recording/VirtualCamera
**Clarity**: ⚠️ Acceptable but could be simplified to `StudioModeOn.svg` / `StudioModeOff.svg`

#### 3. Replay Buffer Resources (2 files) - INVERTED LOGIC
```
ReplayBufferToggleStart.svg    ← Used for INACTIVE state ❌
ReplayBufferToggleStop.svg     ← Used for ACTIVE state ❌
```

**Issue**: Icon names suggest action, but used for state

**Code Usage**:
```csharp
// ReplayBufferToggleCommand.cs
protected override String GetActiveIcon() => "ReplayBufferToggleStop.svg";    // ❌ Confusing
protected override String GetInactiveIcon() => "ReplayBufferToggleStart.svg"; // ❌ Confusing
```

**Expected Pattern**: Should match Recording pattern
```
ReplayBufferOn.svg      ← Active state
ReplayBufferOff.svg     ← Inactive state
```

**Impact**: High confusion risk for maintenance
**Recommendation**: Rename to match state, not action

---

### 🔍 Unused or Unclear Resources

#### Potentially Unused Files
```
AudioDisabled.png           ← PNG format, may be legacy
AudioFiterEnabled.svg       ← Typo: "Fiter" instead of "Filter"
FilterDisabled.png          ← PNG format, generic name
SceneDisabled.png           ← PNG format, may be legacy
SourceDisabled.png          ← PNG format, may be legacy
```

**Issues**:
1. **PNG files** - All other resources are SVG (scalable)
2. **Typo** - "AudioFiterEnabled.svg" has spelling error
3. **Generic names** - "FilterDisabled.png" lacks context
4. **Unclear usage** - May be legacy or unused

**Recommendation**: Audit usage and remove if unused, or convert PNG to SVG

---

## Naming Convention Summary

### Consistent Patterns (Good Examples)

| Pattern | Example | Usage |
|---------|---------|-------|
| `{Feature}On/Off` | `RecordingOn.svg` | Toggle active/inactive |
| `{Feature}Start/Stop` | `RecordingStart.svg` | Start/stop commands |
| `{Feature}StartDisabled` | `RecordingStartDisabled.svg` | Disabled state |
| `{Feature}Selected/Unselected` | `ProfileSelected.svg` | Selection state |
| `{Feature}Visibility{On/Off}` | `SourceVisibilityOn.svg` | Visibility toggle |
| `{Action}` | `Screenshot.svg` | Simple actions |

### Inconsistent Patterns (Need Attention)

| Current Name | Used For | Should Be | Issue |
|--------------|----------|-----------|-------|
| `StreamingToggleOn.svg` | Inactive state | `StreamingOff.svg` | Inverted logic |
| `StreamingToggleOff.svg` | Active state | `StreamingOn.svg` | Inverted logic |
| `ReplayBufferToggleStart.svg` | Inactive state | `ReplayBufferOff.svg` | Action vs state |
| `ReplayBufferToggleStop.svg` | Active state | `ReplayBufferOn.svg` | Action vs state |
| `StudioModeToggleOn.svg` | Active state | `StudioModeOn.svg` | Unnecessary "Toggle" |
| `StudioModeToggleOff.svg` | Inactive state | `StudioModeOff.svg` | Unnecessary "Toggle" |

---

## Resource Usage Patterns

### ButtonImageHelper Usage

All resources are loaded through `ButtonImageHelper` which automatically prepends the namespace:

```csharp
// ButtonImageHelper.cs
public static BitmapImage Icon(String iconResourceName)
{
    return PluginResources.ReadImage($"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconResourceName}");
}
```

**Usage in Commands**:
```csharp
// Short name (good)
protected override String GetActiveIcon() => "RecordingOn.svg";

// Full namespace NOT needed (helper adds it)
// "Loupedeck.OBSStudioForLogiPlugin.Icons.RecordingOn.svg" ← Don't do this
```

**Clarity**: ✅ Excellent - Consistent short name usage across all commands

---

## Recommendations by Priority

### High Priority (Fixes Confusion)

1. **Fix Streaming Icon Names**
   ```
   Rename: StreamingToggleOn.svg  → StreamingOff.svg
   Rename: StreamingToggleOff.svg → StreamingOn.svg
   Update: StreamingToggleCommand.cs code
   ```
   **Impact**: Eliminates inverted logic confusion

2. **Fix Replay Buffer Icon Names**
   ```
   Rename: ReplayBufferToggleStart.svg → ReplayBufferOff.svg
   Rename: ReplayBufferToggleStop.svg  → ReplayBufferOn.svg
   Update: ReplayBufferToggleCommand.cs code
   ```
   **Impact**: Aligns with state-based naming pattern

### Medium Priority (Improves Consistency)

3. **Simplify Studio Mode Icon Names**
   ```
   Rename: StudioModeToggleOn.svg  → StudioModeOn.svg
   Rename: StudioModeToggleOff.svg → StudioModeOff.svg
   Update: StudioModeToggleCommand.cs code
   ```
   **Impact**: Matches other toggle patterns

4. **Fix Typo in Audio Filter**
   ```
   Rename: AudioFiterEnabled.svg → AudioFilterEnabled.svg
   ```
   **Impact**: Corrects spelling error

### Low Priority (Cleanup)

5. **Audit and Remove Unused Resources**
   ```
   Review: AudioDisabled.png
   Review: FilterDisabled.png
   Review: SceneDisabled.png
   Review: SourceDisabled.png
   Review: AudioFilterDisabled.svg
   Review: AudioFilterEnabled.svg (after typo fix)
   ```
   **Impact**: Reduces clutter, clarifies what's actually used

6. **Convert PNG to SVG**
   - If PNG files are still needed, convert to SVG for scalability
   - SVG is the standard format for all other icons

7. **Rename Icons Folder**
   ```
   Rename: src/Icons/ → src/Resources/icons/
   Update: Embedded resource paths
   ```
   **Impact**: Aligns with SDK recommended structure

---

## Naming Convention Guidelines

### For Future Resources

**Toggle Commands** (On/Off states):
```
{Feature}On.svg      ← Active state
{Feature}Off.svg     ← Inactive state
```

**Start/Stop Commands**:
```
{Feature}Start.svg           ← Enabled state
{Feature}StartDisabled.svg   ← Disabled state
{Feature}Stop.svg            ← Enabled state
{Feature}StopDisabled.svg    ← Disabled state
```

**Selection States**:
```
{Feature}Selected.svg      ← Selected
{Feature}Unselected.svg    ← Unselected
```

**Visibility States**:
```
{Feature}VisibilityOn.svg    ← Visible
{Feature}VisibilityOff.svg   ← Hidden
```

**Simple Actions**:
```
{Action}.svg    ← Single-purpose action
```

---

## Summary

### Strengths ✅
- **Consistent patterns** for Recording, VirtualCamera, Profile, Scene resources
- **Clear naming** for most resources
- **Short name usage** via ButtonImageHelper
- **SVG format** for most icons (scalable)

### Issues ⚠️
- **Inverted logic** in Streaming and ReplayBuffer resources (high priority)
- **Inconsistent naming** with "Toggle" prefix in some resources
- **Typo** in AudioFiterEnabled.svg
- **Unused resources** (PNG files, unclear usage)
- **Folder location** (Icons vs Resources/icons)

### Impact
- **Maintenance confusion**: Inverted names make code harder to understand
- **Consistency**: Mixed patterns reduce predictability
- **Clarity**: Some resources have unclear purpose

### Recommendation
Address high-priority naming inversions first (Streaming, ReplayBuffer) to eliminate confusion. Medium and low priority items can be addressed incrementally.

---

## References

- Icon Files: `src/Icons/` (44 files)
- Helper: `src/Helpers/ButtonImageHelper.cs`
- SDK Structure: `.amazonq/rules/memory-bank/sdk-alignment.md`
