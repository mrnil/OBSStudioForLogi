# Icon Update Patterns

## Overview
This document describes the patterns used for updating button icons in dynamic folders when state changes occur in OBS.

## Pattern: CommandImageChanged for Individual Button Updates

### When to Use
Use `CommandImageChanged(actionParameter)` when you need to update a specific button's icon without rebuilding the entire button list.

### Benefits
- **Efficient**: Only refreshes the specific button that changed
- **Fast**: No need to rebuild all buttons in the folder
- **Immediate**: Provides instant visual feedback to the user

### Pattern Implementation

#### For State-Based Icons (Selected/Unselected)
Used in: **ScenesDynamicFolder**, **ProfilesDynamicFolder**

```csharp
public void OnCurrentSceneChanged(String sceneName)
{
    var oldScene = this._currentScene;
    this._currentScene = sceneName ?? String.Empty;
    
    // Update old scene icon to unselected
    if (!String.IsNullOrEmpty(oldScene) && oldScene != this._currentScene)
    {
        this.CommandImageChanged(oldScene);
    }
    
    // Update new scene icon to selected
    if (!String.IsNullOrEmpty(this._currentScene))
    {
        this.CommandImageChanged(this._currentScene);
    }
}
```

**Key Points:**
- Track the old state before updating
- Refresh both old and new items (if different)
- Check for null/empty strings
- Check that old != new to avoid double refresh

#### For Toggle-Based Icons (On/Off, Visible/Hidden)
Used in: **SourcesDynamicFolder**

```csharp
// In RunCommand - initiate toggle
public override void RunCommand(String actionParameter)
{
    if (String.IsNullOrEmpty(actionParameter))
        return;

    OBSStudioForLogiPlugin.Instance?.ToggleSourceVisibility(this._currentScene, actionParameter);
    // Note: Do NOT call CommandImageChanged here - timing issue!
}

// In callback - refresh after OBS processes change
public void OnSourceVisibilityChanged(String sceneName, String sourceName)
{
    if (sceneName != this._currentScene)
        return;

    this.CommandImageChanged(sourceName);
}
```

**Key Points:**
- Do NOT refresh immediately in RunCommand (race condition)
- Use async callback with delay to allow OBS to process
- Refresh only after OBS has updated the state
- Verify the change is for the current scene/context

#### For Real-Time State Icons (Mute/Volume)
Used in: **AudioInputDynamicFolderBase**, **AudioMixerDynamicFolder**, **SceneAudioSourcesDynamicFolder**

```csharp
public void OnInputMuteChanged(String inputName)
{
    this.CommandImageChanged(inputName);
}

public void OnInputVolumeChanged(String inputName)
{
    this.CommandImageChanged(inputName);
}
```

**Key Points:**
- Simple direct refresh when OBS event fires
- No delay needed (event fires after OBS has updated)
- GetCommandImage queries current state from OBS

## Pattern: Delayed Callback for Toggle Operations

### Problem
When toggling state in OBS (e.g., source visibility), there's a timing issue:
1. Plugin sends toggle command to OBS
2. Plugin immediately refreshes icon
3. GetCommandImage queries OBS for current state
4. OBS hasn't processed the toggle yet → wrong state returned
5. Icon shows incorrect state

### Solution
Add delay and callback after OBS API call:

```csharp
// In OBSActionExecutor
public void ToggleSourceVisibility(String sceneName, String sourceName)
{
    Task.Run(async () =>
    {
        if (!this._obs.IsConnected)
        {
            this._log.Warning($"Cannot toggle source visibility for '{sourceName}' - not connected");
            return;
        }

        try
        {
            var currentState = this._obs.GetSceneItemEnabled(sceneName, sourceName);
            this._log.Info($"Toggling source '{sourceName}' visibility from {currentState} to {!currentState}");
            this._obs.SetSceneItemEnabled(sceneName, sourceName, !currentState);
            
            // Wait for OBS to process the change
            await Task.Delay(100);
            
            // Notify plugin to refresh icon
            OBSStudioForLogiPlugin.Instance?.OnSourceVisibilityChanged(sceneName, sourceName);
        }
        catch (Exception ex)
        {
            this._log.Error($"Failed to toggle source visibility for '{sourceName}': {ex.Message}");
        }
    });
}
```

**Key Points:**
- Use `async` Task.Run for the operation
- Call OBS API to toggle state
- Add `await Task.Delay(100)` after API call
- Call callback method to trigger icon refresh
- 100ms is sufficient for OBS to process most changes

## Anti-Patterns to Avoid

### ❌ ButtonActionNamesChanged for Icon Updates
**Problem**: Rebuilds entire button list, inefficient and slow

```csharp
// BAD - rebuilds all buttons
public void OnCurrentSceneChanged(String sceneName)
{
    this._currentScene = sceneName;
    this.ButtonActionNamesChanged(); // ❌ Rebuilds everything
}
```

**Solution**: Use CommandImageChanged for specific buttons

```csharp
// GOOD - updates only affected buttons
public void OnCurrentSceneChanged(String sceneName)
{
    var oldScene = this._currentScene;
    this._currentScene = sceneName;
    
    if (!String.IsNullOrEmpty(oldScene))
        this.CommandImageChanged(oldScene); // ✅ Update old
    
    if (!String.IsNullOrEmpty(this._currentScene))
        this.CommandImageChanged(this._currentScene); // ✅ Update new
}
```

### ❌ Immediate Refresh After Toggle
**Problem**: Race condition - OBS hasn't processed change yet

```csharp
// BAD - refreshes before OBS processes toggle
public override void RunCommand(String actionParameter)
{
    OBSStudioForLogiPlugin.Instance?.ToggleSourceVisibility(this._currentScene, actionParameter);
    this.CommandImageChanged(actionParameter); // ❌ Too early!
}
```

**Solution**: Use delayed callback

```csharp
// GOOD - refreshes after OBS processes toggle
public override void RunCommand(String actionParameter)
{
    OBSStudioForLogiPlugin.Instance?.ToggleSourceVisibility(this._currentScene, actionParameter);
    // Callback will trigger refresh after delay
}

public void OnSourceVisibilityChanged(String sceneName, String sourceName)
{
    if (sceneName != this._currentScene)
        return;
    
    this.CommandImageChanged(sourceName); // ✅ Refreshes after delay
}
```

## Summary

| Scenario | Pattern | Example |
|----------|---------|---------|
| State change (selected/unselected) | Update old + new | Scenes, Profiles |
| Toggle with delay needed | Delayed callback | Source Visibility |
| Real-time event from OBS | Direct refresh | Audio Mute, Volume |
| Multiple buttons changed | Individual updates | NOT ButtonActionNamesChanged |

## Event Flow Examples

### Scene Change
1. User clicks scene button OR changes scene in OBS
2. OBS fires `CurrentProgramSceneChanged` event
3. `OBSWebSocketManager.OnCurrentSceneChanged()` receives event
4. `OBSStudioForLogiPlugin.OnCurrentSceneChanged()` called
5. `ScenesDynamicFolder.OnCurrentSceneChanged()` called
6. `CommandImageChanged(oldScene)` - unselect old scene icon
7. `CommandImageChanged(newScene)` - select new scene icon
8. Framework calls `GetCommandImage()` for both scenes
9. Icons update to show correct selected/unselected state

### Source Visibility Toggle
1. User clicks source button
2. `SourcesDynamicFolder.RunCommand()` called
3. `OBSStudioForLogiPlugin.ToggleSourceVisibility()` called
4. `OBSActionExecutor.ToggleSourceVisibility()` called
5. OBS API call: `SetSceneItemEnabled()`
6. `await Task.Delay(100)` - wait for OBS
7. `OBSStudioForLogiPlugin.OnSourceVisibilityChanged()` called
8. `SourcesDynamicFolder.OnSourceVisibilityChanged()` called
9. `CommandImageChanged(sourceName)` - refresh icon
10. Framework calls `GetCommandImage()`
11. Queries OBS for current state (now updated)
12. Icon updates to show correct visibility state

### Audio Mute Change
1. User clicks audio button OR changes mute in OBS
2. OBS fires `InputMuteStateChanged` event
3. `OBSWebSocketManager.OnInputMuteStateChanged()` receives event
4. `OBSStudioForLogiPlugin.OnInputMuteChanged()` called
5. `AudioMixerDynamicFolder.OnInputMuteChanged()` called
6. `CommandImageChanged(inputName)` - refresh icon
7. Framework calls `GetCommandImage()`
8. Queries OBS for current mute state
9. Icon updates with correct color (red=muted, green=unmuted)
