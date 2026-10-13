# SDK Quick Reference

## Command Creation Checklist

### Text-Only Display Command

```csharp
public class StatusDisplay : PluginDynamicCommand
{
    public StatusDisplay()
        : base("Status Display", "Shows current status", "Displays")
    {
        // Do NOT set IsWidget
    }

    // Override ONLY this method
    protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
    {
        return $"Status\r\n{GetStatus()}";
    }

    // Do NOT override GetCommandImage
}
```

### Icon-Only Command

```csharp
public class ToggleCommand : PluginDynamicCommand
{
    public ToggleCommand()
        : base("Toggle Action", "Toggles state", "Controls")
    {
        this.IsWidget = true; // REQUIRED for icon-only
    }

    // Override ONLY this method
    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        return ButtonImageHelper.Icon(isActive ? "On.svg" : "Off.svg", imageSize);
    }

    // Do NOT override GetCommandDisplayName
}
```

### Text + Icon Command (Rendered)

```csharp
public class DataDisplay : PluginDynamicCommand
{
    public DataDisplay()
        : base("Data Display", "Shows data with icon", "Displays")
    {
        this.IsWidget = true; // Icon-only mode
    }

    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        // Draw the text over an icon with ButtonTextRenderer (see image-rendering-simplified.md)
        String text = $"{label}\n\n{value}";
        return ButtonTextRenderer.RenderTextWithIcon(text, imageSize,
            isActive ? "ActiveIcon.svg" : "InactiveIcon.svg",
            isActive ? BitmapColor.Green : BitmapColor.Red);
    }
}
```

## Adjustment Creation Checklist

```csharp
public class VolumeAdjustment : PluginDynamicAdjustment
{
    private Int32 _volume = 50;

    public VolumeAdjustment()
        : base("Volume Control", "Adjust volume", "Audio", hasReset: true)
    {
    }

    protected override void ApplyAdjustment(String actionParameter, Int32 diff)
    {
        _volume = Math.Clamp(_volume + diff, 0, 100);
        this.AdjustmentValueChanged(); // Trigger UI update
    }

    protected override void RunCommand(String actionParameter)
    {
        // Reset on dial press (when hasReset: true)
        _volume = 50;
        this.AdjustmentValueChanged();
    }

    protected override String GetAdjustmentValue(String actionParameter)
    {
        return $"{_volume}%";
    }
}
```

## Dynamic Folder Checklist

```csharp
public class ItemsDynamicFolder : PluginDynamicFolder
{
    public ItemsDynamicFolder()
        : base("Items Folder", "Browse items", "Folders")
    {
    }

    protected override Boolean OnLoad()
    {
        this.IsEnabled = false;
        this.ResetParameters(false);
        return true;
    }

    private void ResetParameters(Boolean loadData)
    {
        this.RemoveAllParameters();

        if (loadData)
        {
            Item[] items = this.GetItems();
            foreach (Item item in items)
            {
                this.AddParameter(item.Id, item.Name, this.GroupName)
                    .Description = $"Select {item.Name}";
            }
        }

        this.ParametersChanged();
        this.ActionImageChanged();
    }

    public override void RunCommand(String actionParameter)
    {
        if (String.IsNullOrEmpty(actionParameter))
        {
            return;
        }

        this.ProcessItem(actionParameter);
    }

    public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        Boolean isSelected = actionParameter == GetCurrentItem();
        return ButtonImageHelper.Icon(isSelected ? "Selected.svg" : "Unselected.svg", imageSize);
    }
}
```

## Plugin Lifecycle Template

```csharp
public class MyPlugin : Plugin
{
    public override Boolean UsesApplicationApiOnly => true;
    public override Boolean HasNoApplication => true;

    public MyPlugin()
    {
        // Initialize helpers FIRST
        PluginLog.Init(this.Log);
        PluginResources.Init(this.Assembly);

        // Then initialize services
        InitializeServices();
    }

    public override void Load()
    {
        // Subscribe to events
        SubscribeToEvents();

        // Start services
        StartServices();
    }

    public override void Unload()
    {
        // Unsubscribe from events
        UnsubscribeFromEvents();

        // Stop services
        StopServices();

        // Dispose resources
        DisposeResources();
    }
}
```

## Common Patterns

### Update UI After State Change

```csharp
private void OnStateChanged()
{
    // Refresh specific parameter
    this.ActionImageChanged(parameter);

    // OR refresh all instances
    this.ActionImageChanged();
}
```

### Validate Parameters

```csharp
protected override void RunCommand(String actionParameter)
{
    if (String.IsNullOrEmpty(actionParameter))
    {
        PluginLog.Warning("No parameter provided");
        return;
    }

    ProcessCommand(actionParameter);
}
```

### Async Operations

`RunCommand` runs on the SDK's thread, so it must return quickly. In this plugin, OBS calls already run on the executor's background runner (see `guidelines.md`), so a command just calls the plugin:

```csharp
protected override void RunCommand(String actionParameter)
{
    OBSStudioForLogiPlugin.Instance?.ToggleRecording();
}
```

### Resource Loading

```csharp
// Always go through ButtonImageHelper: it keeps the multicolour marker that stops
// the device tinting the icon white (see image-rendering-simplified.md)
return ButtonImageHelper.Icon("icon.svg", imageSize);
```

## Common Mistakes to Avoid

### ❌ Mixing Display Modes

```csharp
// BAD - overrides both without IsWidget
protected override String GetCommandDisplayName(...) { }
protected override BitmapImage GetCommandImage(...) { }
```

### ❌ Forgetting IsWidget

```csharp
// BAD - icon-only without IsWidget flag
protected override BitmapImage GetCommandImage(...) { }
// Missing: this.IsWidget = true;
```

### ❌ Not Cleaning Up

```csharp
// BAD - subscribes but never unsubscribes
public override void Load()
{
    this.ClientApplication.ApplicationStarted += OnStarted;
    // Missing Unload() cleanup
}
```

### ❌ Blocking Operations

```csharp
// BAD - blocks UI thread
protected override void RunCommand(String actionParameter)
{
    Thread.Sleep(5000); // Blocks!
    PerformAction();
}

// GOOD - the executor runs the OBS call in the background
protected override void RunCommand(String actionParameter)
{
    OBSStudioForLogiPlugin.Instance?.ToggleRecording();
}
```

### ❌ Missing Validation

```csharp
// BAD - no validation
protected override void RunCommand(String actionParameter)
{
    ProcessParameter(actionParameter); // Could be null!
}

// GOOD - validates first
protected override void RunCommand(String actionParameter)
{
    if (String.IsNullOrEmpty(actionParameter))
    {
        return;
    }

    ProcessParameter(actionParameter);
}
```

## Testing Checklist

- [ ] Command executes with valid parameter
- [ ] Command handles null/empty parameter
- [ ] Command handles invalid parameter
- [ ] Display updates after state change
- [ ] Resources load correctly
- [ ] Cleanup happens in Unload()
- [ ] No memory leaks
- [ ] Thread-safe operations
- [ ] Error handling works
- [ ] Logging provides context

For building, packaging and releasing, see `tech.md` and `release-process.md`.
