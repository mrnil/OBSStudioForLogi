# Image Rendering System - Simplified

## Overview

The plugin builds every button image with two small static helpers in `src/Helpers/`. There is no factory, store or image-data layer.

- `ButtonImageHelper` draws embedded SVG icons.
- `ButtonTextRenderer` draws text, sized to fit the button.

These are the only rendering helpers. Older versions of this document described `StateIcon`, `Text`, `StateText`, `TextWithIcon` and `StateTextWithIcon`; none of them exist. Choose a state-dependent icon or colour with a conditional at the call site instead.

## ButtonImageHelper API

### Icon(iconResourceName)

Returns an SVG icon from the embedded resources.

```csharp
return ButtonImageHelper.Icon("Reconnect.svg");

// State-dependent icon
Boolean isRecording = OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;
return ButtonImageHelper.Icon(isRecording ? "RecordingOn.svg" : "RecordingOff.svg");
```

### IconWithBackground(iconResourceName, imageSize, backgroundColor)

Fills the button with a colour and draws the icon over it. Icons have no background shape of their own (see `icon-style.md`), so use this when the colour carries the state, as the Reconnect button does for connection status.

```csharp
return ButtonImageHelper.IconWithBackground("Reconnect.svg", imageSize, backgroundColor);
```

## ButtonTextRenderer API

All methods pick the font size from the text length, the number of lines and the button size.

### RenderText(text, imageSize, backgroundColor = null, textColor = null)

Text only. The background defaults to black and the text to white.

```csharp
return ButtonTextRenderer.RenderText("Connected", imageSize, BitmapColor.Black, BitmapColor.Green);

// State-dependent colour
return ButtonTextRenderer.RenderText(text, imageSize, BitmapColor.Black, !isMuted ? BitmapColor.Green : BitmapColor.Red);
```

### RenderTextWithBorder(text, imageSize, textColor, showBorder)

Text on black, with a 3px white border when `showBorder` is true. Used to mark the selected item, e.g. the selected audio source. An overload takes `imageWidth` and `imageHeight` in pixels instead of a `PluginImageSize`.

```csharp
return ButtonTextRenderer.RenderTextWithBorder(text, imageSize, !isMuted ? BitmapColor.Green : BitmapColor.Red, isSelected);
```

### RenderTextWithIcon(text, imageSize, iconResourceName, textColor = null)

Draws the icon on black, then the text over it. If the icon fails to load, it logs a warning and draws the text alone.

```csharp
return ButtonTextRenderer.RenderTextWithIcon(text, imageSize, "SourceVisibilityOn.svg", BitmapColor.Green);
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

Toggle and start/stop commands extend `ToggleCommandBase` or `StartStopCommandBase`, which call `ButtonImageHelper.Icon` for you. The subclass only names the icons:

```csharp
public class RecordingToggleCommand : ToggleCommandBase, IObsCommand
{
    protected override Boolean GetState() => OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;
    protected override String GetActiveIcon() => "RecordingOn.svg";
    protected override String GetInactiveIcon() => "RecordingOff.svg";
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
        BitmapColor backgroundColor = isConnected ? new BitmapColor(57, 180, 120) : BitmapColor.Black;
        BitmapColor textColor = isConnected ? BitmapColor.White : new BitmapColor(128, 128, 128);

        return ButtonTextRenderer.RenderText(displayText, imageSize, backgroundColor, textColor);
    }
}
```

### State-Based Text Button

```csharp
public class AudioVolumeDynamicFolder : PluginDynamicFolder
{
    public override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize)
    {
        Boolean isMuted = OBSStudioForLogiPlugin.Instance?.GetInputMute(actionParameter) ?? false;
        Single volumeLevel = OBSStudioForLogiPlugin.Instance?.GetInputVolume(actionParameter) ?? 1.0f;
        String text = $"{actionParameter}\n\n{VolumeConverter.FormatDb(volumeLevel)}";

        return ButtonTextRenderer.RenderText(text, imageSize, BitmapColor.Black, !isMuted ? BitmapColor.Green : BitmapColor.Red);
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
        return ButtonImageHelper.Icon(isSelected ? "SceneSelected.svg" : "SceneUnselected.svg");
    }
}
```

## Rendering Must Not Block

`GetCommandImage` and `GetAdjustmentImage` run on the SDK's render threads. Draw only from state the plugin already holds: the plugin getters used above (`IsRecording`, `GetInputMute`, `GetInputVolume`, `GetSourceVisibility` and so on) read cached state and never wait on OBS. See "Rendering Must Not Block on OBS" in `guidelines.md`.

## Display Names Are Drawn Over the Image

The SDK draws a command's display name (`GetCommandDisplayName` / `GetAdjustmentDisplayName`) as a white title over the image. Pick one of these per button:

- **The image carries the text** (`ButtonTextRenderer`): return `String.Empty` from the display-name override, or the text shows twice. The audio and stats folders do this. `AudioVolumeDynamicFolder` didn't, and showed each name and volume twice until 2026-09-29.
- **The image is an icon or a meter**: leave the display name to the SDK, which labels the button with it. `VuMeterRenderer` draws no name for this reason.

Single-command actions (`ActionEditorCommand`) return `null` from `GetCommandDisplayName` for the same reason.

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
    return ButtonImageHelper.Icon(isRecording ? "RecordingOn.svg" : "RecordingOff.svg");
}
```

**Result:** ~80% reduction in code, same functionality.
