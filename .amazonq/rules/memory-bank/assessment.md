# Project Assessment — Findings, Tasks & Priorities

## Overview

Assessment conducted against v1.5.1. Covers code quality, architecture, usability, and feature gaps.
Items are ordered by priority within each category.

---

## High Priority

### 1. CommandRegistry Bypass — `OBSWebSocketManager` (Architecture)

**Problem**: `OBSWebSocketManager.OnConnected` and `OnDisconnected` each contain ~10 hardcoded singleton calls that bypass the `CommandRegistry`/`CommandCoordinator` system entirely. `CommandCoordinator.NotifyConnected()` and `NotifyDisconnected()` exist but are never called from `OBSStudioForLogiPlugin.OnOBSConnected` / `OnOBSDisconnected`. `OnApplicationStopped` does call `NotifyDisconnected()` but `OnOBSConnected` never calls `NotifyConnected()`.

**Impact**: Any new command that correctly self-registers via `IObsCommand` will never receive `OnConnected()` through the registry. It only works if a hardcoded singleton call is also added to `OBSWebSocketManager`. This is a maintenance trap that contradicts the registry's design intent.

**Fix**:
1. Add `this._commandCoordinator.NotifyConnected()` to `OBSStudioForLogiPlugin.OnOBSConnected`
2. Add `this._commandCoordinator.NotifyDisconnected()` to `OBSStudioForLogiPlugin.OnOBSDisconnected`
3. Remove the hardcoded singleton calls from `OBSWebSocketManager.OnConnected` and `OnDisconnected`

**Risk**: Low — `CommandRegistry` is fully tested (100% coverage). Every command that self-registers already implements `IObsCommand` correctly.

---

### 2. Scene Change Bypasses Registry — `OBSStudioForLogiPlugin` (Architecture)

**Problem**: `OBSStudioForLogiPlugin.OnCurrentSceneChanged` calls `SourcesDynamicFolder.Instance` and `SceneAudioSourcesDynamicFolder.Instance` directly via singleton, bypassing the registry:

```csharp
this._obsFacade.UpdateSourcesForScene(sceneName,
    (scene, sources) => SourcesDynamicFolder.Instance?.UpdateSources(scene, sources),         // bypass
    (scene, audioSources) => SceneAudioSourcesDynamicFolder.Instance?.UpdateAudioSources(...)); // bypass
```

These folders implement `IObsCommand` and `ISceneAwareCommand` but their scene-change update path is hardwired. Adding a new scene-aware folder requires editing `OBSStudioForLogiPlugin` rather than just implementing the interface.

**Fix**: Introduce an `ISceneSourcesAwareCommand` interface (or extend `ISceneAwareCommand`) with an `OnSceneSourcesChanged(String sceneName, String[] sources)` method, and route through the registry.

---

### 3. Scene/Source/Profile Buttons Show No Text (Usability)

**Problem**: `ScenesDynamicFolder`, `SourcesDynamicFolder`, and `ProfilesDynamicFolder` display only a selected/unselected icon — no name text. On the MX Console's small tiles, users must memorise button positions to know which scene or source each button represents.

**Fix**: Use `ButtonTextRenderer.RenderTextWithBorder` or `ButtonImageHelper.StateTextWithIcon` to render the item name alongside the selection indicator. Example for scenes:

```csharp
public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
{
    Boolean isSelected = actionParameter == this._currentScene;
    String icon = isSelected ? "ScenesSelected.svg" : "ScenesUnselected.svg";
    return ButtonImageHelper.StateTextWithIcon(actionParameter, imageSize, isSelected,
        "ScenesSelected.svg", "ScenesUnselected.svg",
        BitmapColor.Green, BitmapColor.White);
}
```

---

## Medium Priority

### 4. `DoubleTapHelper` Race Condition and Memory Leak (Code Quality)

**Problem**: `_tapStates` dictionary is accessed from both the calling thread and `Task.Run` background threads without synchronisation — a race condition. Additionally, cancelled `CancellationTokenSource` objects are never disposed, leaking memory over time in a long-running plugin.

**Fix**:
- Add `lock (_tapStates)` around all dictionary access
- Call `cancellation.Dispose()` after cancellation in the `TaskCanceledException` catch block and after single-tap fires

---

### 5. Double-Tap Interaction Unreliable on MX Console (Usability)

**Problem**: The 500ms double-tap window is tight for the MX Creative Console's physical dial buttons. Double-tap is used in `AudioInputDynamicFolderBase` (double-tap to mute) and `MediaDynamicFolder` (double-tap to stop). The single-tap action also fires with a 500ms delay, making audio source selection feel broken.

**Recommendation**: Expose the double-tap threshold as a configurable value in `OBSTimings` or `PluginConfig`. Consider whether dedicated separate buttons (e.g. a standalone mute button) are a better UX than double-tap for the MX Console primary use case.

---

### 6. `CommandCoordinator` Is a Valueless Pass-Through (Code Quality)

**Problem**: Every method in `CommandCoordinator` is a one-liner delegating identically to `CommandRegistry`. It adds a layer of indirection with no behaviour, no validation, no error isolation, and 0% test coverage.

**Options**:
- Give it a real responsibility (e.g. per-command exception isolation so one failing command doesn't break others)
- Or collapse it — have `OBSStudioForLogiPlugin` hold `CommandRegistry` directly

**Recommended fix** — add per-command exception isolation in `CommandCoordinator`:
```csharp
public void NotifySceneChanged(String sceneName)
{
    foreach (var command in this._registry.GetSceneAwareCommands())
    {
        try { command.OnSceneChanged(sceneName); }
        catch (Exception ex) { PluginLog.Error($"Command {command.GetType().Name} threw on OnSceneChanged: {ex.Message}"); }
    }
}
```

---

### 7. `OBSStats` Null Propagation (Code Quality)

**Problem**: `OBSActionExecutor.GetStats()` and `OBSFacade.GetStats()` return `null` when disconnected. Display commands (`StatsDisplay`, `StatsDynamicFolder`, `StreamStatsDynamicFolder`) must null-check defensively on every call.

**Fix**: Introduce a null-object pattern:
```csharp
public static OBSStats Empty => new OBSStats(); // all zero values
```
Return `OBSStats.Empty` instead of `null` from disconnected/error paths. Display commands can then render "0.0 FPS" etc. without null guards.

---

## Low Priority

### 8. `ProfileListChanged` / `SceneCollectionListChanged` Events Not Subscribed (Feature Gap)

**Problem**: If a user creates or deletes a profile or scene collection while the plugin is connected, the dynamic folders go stale until reconnect. The OBS WebSocket protocol fires `ProfileListChanged` and `SceneCollectionListChanged` events for exactly this case.

**Fix**: Subscribe to both events in `OBSWebSocketManager` and call `UpdateProfileList()` / `UpdateSceneList()` respectively. Low effort, meaningful reliability improvement.

---

### 9. Recording Duration Display Missing (Feature Gap)

**Problem**: The Stream Stats folder shows live stream duration, but there is no equivalent for recording. `GetRecordStatus` in the OBS WebSocket API returns a timecode and bytes written. Parity between streaming and recording stats displays is a usability gap for users monitoring recording length.

**Fix**: Add a `RecordingStatsDynamicFolder` or a `RecordingStatusDisplay` command using the same polling pattern as `StatsService`.

---

### 10. `MediaDynamicFolder` Doesn't Respond to Input List Changes (Feature Gap)

**Problem**: `MediaDynamicFolder.OnConnected()` loads the media list once. If a user adds or removes a media source in OBS while connected, the folder doesn't update. Audio inputs handle this via `IInputsListAwareCommand` and `OnInputListChanged`, but `MediaDynamicFolder` only implements `IObsCommand`.

**Fix**: Implement `IInputsListAwareCommand` in `MediaDynamicFolder` and filter the incoming inputs list to media kinds:
```csharp
public void OnInputsChanged(String[] inputs)
{
    this._mediaInputs = OBSStudioForLogiPlugin.Instance?.GetMediaInputList() ?? new String[0];
    this.ButtonActionNamesChanged();
}
```

---

### 11. Password Field Has No Sensitivity Indication (Security)

**Problem**: The password field in `PluginSettingsCommand` is a plain `ActionEditorTextbox` with no masking or indication that the value is sensitive. Per `SecureCoding.md`, sensitive fields should be clearly marked.

**Fix**: Add "(sensitive)" to the label text as a minimum. If the SDK supports a password input type, use it.

---

## Summary Table

| # | Priority | Area | Issue |
|---|----------|------|-------|
| 1 | ~~High~~ | ~~Architecture~~ | ~~`CommandRegistry` bypass — `NotifyConnected`/`NotifyDisconnected` never called through registry~~ ✅ Fixed |
| 2 | ~~High~~ | ~~Architecture~~ | ~~`OnCurrentSceneChanged` bypasses registry for `SourcesDynamicFolder` and `SceneAudioSourcesDynamicFolder`~~ ✅ Fixed |
| 3 | High | Usability | Scene/source/profile buttons show no text — unusable without memorisation |
| 4 | ~~Medium~~ | ~~Code Quality~~ | ~~`DoubleTapHelper` race condition and `CancellationTokenSource` leak~~ ✅ Fixed |
| 5 | Medium | Usability | Double-tap unreliable on MX Console; 500ms delay on audio selection |
| 6 | Medium | Code Quality | `CommandCoordinator` is a valueless pass-through — no error isolation |
| 7 | Medium | Code Quality | `OBSStats` null propagation — null-object pattern would clean up display commands |
| 8 | Low | Feature | `ProfileListChanged`/`SceneCollectionListChanged` events not subscribed |
| 9 | Low | Feature | Recording duration display (parity with streaming stats) |
| 10 | Low | Feature | `MediaDynamicFolder` doesn't respond to input list changes |
| 11 | Low | Security | Password field has no masking or sensitivity indication |
