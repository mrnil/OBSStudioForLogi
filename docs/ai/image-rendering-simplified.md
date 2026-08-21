# Image Rendering System

## Overview

The plugin renders all button images through two static helper classes — no factory/store/data-cache pattern. The Loupedeck framework caches images itself, so neither helper does its own caching.

- **`ButtonImageHelper`** (`src/Helpers/ButtonImageHelper.cs`) — icon-only buttons.
- **`ButtonTextRenderer`** (`src/Helpers/ButtonTextRenderer.cs`) — anything showing text (name, value, status), with or without a border to indicate selection.

There is no combined "state icon" or "state text with icon" method on either class — state (on/off, selected/unselected) is expressed by branching inline at the call site to pick which icon, color, or border to use. This is deliberate: see "History" below for why an earlier design considered otherwise.

## ButtonImageHelper — Icons

### Icon(iconResourceName)

Returns a static SVG icon from embedded resources.

```csharp
return ButtonImageHelper.Icon("Screenshot.svg");
```

For a state-based icon, branch inline — this is the actual pattern used throughout `src/Actions/` (`ToggleCommandBase`, `StartStopCommandBase`, `SceneSelectCommand`, etc.):

```csharp
Boolean isRecording = OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;
return ButtonImageHelper.Icon(isRecording ? "RecordingOn.svg" : "RecordingOff.svg");
```

### IconWithBackground(iconResourceName, imageSize, backgroundColor)

Renders an icon over a solid background color. Used by `ReconnectCommand` to color-code connection state behind the icon.

```csharp
return ButtonImageHelper.IconWithBackground("Reconnect.svg", imageSize, backgroundColor);
```

## ButtonTextRenderer — Text

### RenderText(text, imageSize, backgroundColor?, textColor?)

Text-only button; font size is auto-scaled to fit the text length and image size. Used by `ConnectionStatusDisplay`, `CurrentSceneDisplay`, `CurrentProfileDisplay`, `CurrentSceneCollectionDisplay`, `StatsDisplay`, `StatsDynamicFolder`, `StreamStatsDynamicFolder`, `AudioVolumeDynamicFolder`, `MediaDynamicFolder`.

```csharp
return ButtonTextRenderer.RenderText("Connected", imageSize, BitmapColor.Black, BitmapColor.Green);
```

### RenderTextWithBorder(text, imageSize, textColor, showBorder)

Text with an optional 3px white border — the border is how "currently selected" is shown alongside a state color. Used by `AudioInputDynamicFolderBase`, `AudioHelpers`, and `ScenesDynamicFolder`/`SourcesDynamicFolder`/`ProfilesDynamicFolder` (name-on-button fix, Assessment #3).

```csharp
Boolean isSelected = actionParameter == this._currentScene;
return ButtonTextRenderer.RenderTextWithBorder(actionParameter, imageSize, isSelected ? BitmapColor.Green : BitmapColor.White, isSelected);
```

An overload takes explicit `imageWidth`/`imageHeight` instead of `PluginImageSize`, for non-standard sizes (used by `AudioHelpers`).

### RenderTextWithIcon(text, imageSize, iconResourceName, textColor?)

Draws an icon then overlays text on top. Exists on the helper but is not currently called anywhere in `src/` — `RenderTextWithBorder` has covered every case that's come up so far where text needs a state indicator.

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
public class RecordingToggleCommand : ToggleCommandBase, IObsCommand
{
    protected override String GetActiveIcon() => "RecordingOn.svg";
    protected override String GetInactiveIcon() => "RecordingOff.svg";
    // ToggleCommandBase.GetCommandImage branches on GetState() to pick between them
}
```

### Text Display Button

```csharp
public class ConnectionStatusDisplay : PluginDynamicCommand
{
    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        Boolean isConnected = OBSStudioForLogiPlugin.Instance?.IsConnected ?? false;
        String text = isConnected ? "Connected" : "Disconnected";
        BitmapColor bgColor = isConnected ? BitmapColor.Green : BitmapColor.Red;

        return ButtonTextRenderer.RenderText(text, imageSize, bgColor, BitmapColor.White);
    }
}
```

### Text With Selection Border

```csharp
public class ScenesDynamicFolder : PluginDynamicFolder
{
    private String _currentScene = String.Empty;

    public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        Boolean isSelected = actionParameter == this._currentScene;
        return ButtonTextRenderer.RenderTextWithBorder(actionParameter, imageSize, isSelected ? BitmapColor.Green : BitmapColor.White, isSelected);
    }
}
```

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

The framework handles caching internally — you don't need to implement your own caching logic.

## History

Earlier versions of this plugin (pre-v1.1.0) used a more complex `ActionImageStore<StateImageData>` factory/cache pattern (`StateImageFactory`, `TextImageFactory`, `SimpleIconImageFactory` plus matching `*Data` model classes) — removed in v1.1.0 in favor of static helpers (see `CHANGELOG.md`).

**Note on this file's accuracy**: an earlier revision of this document described the replacement as a single consolidated `ButtonImageHelper` class with six methods (`Icon`, `StateIcon`, `Text`, `StateText`, `TextWithIcon`, `StateTextWithIcon`). That consolidation was never actually implemented — none of the `State*`/`TextWithIcon`/`StateTextWithIcon` methods exist in the codebase. What was actually built is the two-class split documented above (`ButtonImageHelper` for icons, `ButtonTextRenderer` for text), discovered and corrected while implementing Assessment #3, which had inherited the same inaccurate method names into its proposed fix. If you're reading older context (chat history, a cached version of this file, etc.) that references `ButtonImageHelper.StateIcon` or similar — it's wrong; use the real API above.
