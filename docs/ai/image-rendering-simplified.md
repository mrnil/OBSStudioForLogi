# Image Rendering System - Simplified

## Overview

The plugin uses a simple, consistent helper class for all button image rendering. No complex factory/store/data patterns needed.

## ButtonImageHelper API

All button images are created using the static `ButtonImageHelper` class:

### Methods

#### Icon(iconResourceName)

Returns a static SVG icon from embedded resources.

```csharp
return ButtonImageHelper.Icon("Reconnect.svg");
```

#### StateIcon(isActive, activeIcon, inactiveIcon)

Returns different icons based on boolean state.

```csharp
Boolean isRecording = OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;
return ButtonImageHelper.StateIcon(isRecording, "RecordingOn.svg", "RecordingOff.svg");
```

#### Text(text, imageSize, backgroundColor, textColor)

Renders text-only button with optional colors.

```csharp
return ButtonImageHelper.Text("Connected", imageSize, BitmapColor.Green, BitmapColor.White);
```

#### StateText(text, imageSize, isActive, activeColor, inactiveColor)

Renders text with color based on boolean state.

```csharp
Boolean isMuted = GetMuteState();
String text = $"{inputName}\n\n{volume}%";
return ButtonImageHelper.StateText(text, imageSize, !isMuted, BitmapColor.Green, BitmapColor.Red);
```

#### TextWithIcon(text, imageSize, iconResourceName, textColor)

Renders text with a background icon.

```csharp
String text = $"{inputName}\n\n{volume}%";
return ButtonImageHelper.TextWithIcon(text, imageSize, "AudioMixerUnmuted.svg", BitmapColor.Green);
```

#### StateTextWithIcon(text, imageSize, isActive, activeIcon, inactiveIcon, activeColor, inactiveColor)

Renders text with state-based background icon and color.

```csharp
Boolean isMuted = GetMuteState();
String text = $"{inputName}\n\n{volume}%";
return ButtonImageHelper.StateTextWithIcon(text, imageSize, !isMuted,
    "AudioMixerUnmuted.svg", "AudioMixerMuted.svg",
    BitmapColor.Green, BitmapColor.Red);
```

## Usage Examples

### Simple Icon Button

```csharp
public class ScreenshotCommand : PluginDynamicCommand
{
    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        return ButtonImageHelper.Icon("Screenshot.svg");
    }
}
```

### State-Based Icon Button

```csharp
public class RecordingToggleCommand : PluginDynamicCommand
{
    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        Boolean isRecording = OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;
        return ButtonImageHelper.StateIcon(isRecording, "RecordingOn.svg", "RecordingOff.svg");
    }
}
```

### Text Display Button

```csharp
public class CurrentSceneDisplay : PluginDynamicCommand
{
    private String _currentScene = "Not Connected";

    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        Boolean isConnected = OBSStudioForLogiPlugin.Instance?.IsConnected ?? false;
        String displayText = isConnected ? this._currentScene : "Not Connected";
        BitmapColor bgColor = isConnected ? new BitmapColor(57, 180, 120) : BitmapColor.Black;
        BitmapColor textColor = isConnected ? BitmapColor.White : new BitmapColor(128, 128, 128);

        return ButtonImageHelper.Text(displayText, imageSize, bgColor, textColor);
    }
}
```

### State-Based Text Button

```csharp
public class AudioInputDynamicFolderBase : PluginDynamicFolder
{
    public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        Boolean isMuted = OBSStudioForLogiPlugin.Instance?.GetInputMute(actionParameter) ?? false;
        Single volumeLevel = OBSStudioForLogiPlugin.Instance?.GetInputVolume(actionParameter) ?? 1.0f;

        Int32 volumePercent = (Int32)(volumeLevel * 100);
        String text = $"{actionParameter}\n\n{volumePercent}%";

        return ButtonImageHelper.StateTextWithIcon(text, imageSize, !isMuted,
            "AudioMixerUnmuted.svg", "AudioMixerMuted.svg",
            BitmapColor.Green, BitmapColor.Red);
    }
}
```

### Dynamic Folder with State Icons

```csharp
public class ScenesDynamicFolder : PluginDynamicFolder
{
    private String _currentScene = String.Empty;

    public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        Boolean isSelected = actionParameter == this._currentScene;
        return ButtonImageHelper.StateIcon(isSelected, "ScenesSelected.svg", "ScenesUnselected.svg");
    }
}
```

## Benefits

1. **Simple** - One helper class, six methods
2. **Consistent** - All buttons use same API
3. **Clear** - Method names describe what they do
4. **Minimal** - No boilerplate code needed
5. **Efficient** - Loupedeck framework handles caching
6. **Flexible** - Supports text-only, icon-only, and text-with-icon combinations

## Icon Resource Naming

Icons are embedded resources with automatic path resolution:

- Input: `"RecordingOn.svg"`
- Resolved to: `"Loupedeck.OBSStudioForLogiPlugin.Icons.RecordingOn.svg"`

No need to specify full resource path.

## When GetCommandImage is Called

The Loupedeck framework calls `GetCommandImage()`:

- When button first appears
- When you call `CommandImageChanged(actionParameter)`
- When you call `ActionImageChanged()`

The framework handles caching internally - you don't need to implement your own caching logic.

## Migration from Old System

**Before (complex):**

```csharp
private readonly ActionImageStore<StateImageData> imageStore;

public RecordingToggleCommand()
{
    this.imageStore = new ActionImageStore<StateImageData>(new StateImageFactory());
}

protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
{
    Boolean isRecording = OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;

    StateImageData imageData = new StateImageData
    {
        Id = "recording-toggle",
        IsActive = isRecording,
        ActiveIconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.RecordingOn.svg",
        InactiveIconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.RecordingOff.svg"
    };

    this.imageStore.UpdateImage(imageData.Id, imageData);

    if (this.imageStore.TryGetImage(imageData.Id, imageSize, out BitmapImage image))
    {
        return image;
    }

    return EmbeddedResources.ReadImage(imageData.InactiveIconPath);
}
```

**After (simple):**

```csharp
protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
{
    Boolean isRecording = OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;
    return ButtonImageHelper.StateIcon(isRecording, "RecordingOn.svg", "RecordingOff.svg");
}
```

**Result:** ~80% reduction in code, same functionality.
