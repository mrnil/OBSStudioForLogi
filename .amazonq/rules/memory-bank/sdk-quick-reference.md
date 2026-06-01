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
        return ButtonImageHelper.StateIcon(isActive, "On.svg", "Off.svg");
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
        // Render text on icon using BitmapBuilder or ButtonImageHelper
        String text = $"{label}\n\n{value}";
        return ButtonImageHelper.StateTextWithIcon(text, imageSize, isActive,
            "ActiveIcon.svg", "InactiveIcon.svg",
            BitmapColor.Green, BitmapColor.Red);
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
            var items = GetItems();
            foreach (var item in items)
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
            return;
            
        ProcessItem(actionParameter);
    }
    
    public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        Boolean isSelected = actionParameter == GetCurrentItem();
        return ButtonImageHelper.StateIcon(isSelected, "Selected.svg", "Unselected.svg");
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

```csharp
protected override void RunCommand(String actionParameter)
{
    Task.Run(async () =>
    {
        try
        {
            await PerformActionAsync(actionParameter);
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Action failed");
        }
    });
}
```

### Resource Loading

```csharp
// Short name (preferred)
var icon = EmbeddedResources.ReadImage(EmbeddedResources.FindFile("icon.svg"));

// Via ButtonImageHelper (best)
return ButtonImageHelper.Icon("icon.svg");
```

## Package Metadata Template

```yaml
pluginName: MyPlugin
displayName: "My Plugin"
version: 1.0.0
author: "Your Name"
supportedDevices:
    - LoupedeckCtFamily
minimumLoupedeckVersion: 6.0
license: MIT
homepageUrl: https://example.com
category: "Productivity"
keywords: ["automation", "control", "workflow"]
```

## Build Commands

```bash
# Development build with hot-reload
dotnet build

# Release build for packaging
dotnet build --configuration Release

# Create package
LogiPluginTool pack ./bin/Release ./MyPlugin.lplug4

# Verify package
LogiPluginTool verify ./MyPlugin.lplug4

# Install package
LogiPluginTool install ./MyPlugin.lplug4

# View metadata
LogiPluginTool metadata ./MyPlugin.lplug4
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

// GOOD - async operation
protected override void RunCommand(String actionParameter)
{
    Task.Run(() => PerformAction());
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
        return;
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

## Deployment Checklist

- [ ] Build in Release configuration
- [ ] Update version in LoupedeckPackage.yaml
- [ ] Add/update category and keywords
- [ ] Test on target devices
- [ ] Verify package with LogiPluginTool
- [ ] Test installation on clean machine
- [ ] Update documentation
- [ ] Create release notes
