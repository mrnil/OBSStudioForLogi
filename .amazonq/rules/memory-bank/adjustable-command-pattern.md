# Adjustable Command Pattern

## Overview
Adjustable commands are Loupedeck commands that respond to encoder rotation (dial turning) rather than button presses. They allow users to cycle through options by turning a physical encoder knob.

## Pattern Implementation

### Base Class
```csharp
public abstract class PluginDynamicCommand : PluginDynamicCommandBase
{
    protected override void ApplyAdjustment(String actionParameter, Int32 ticks)
    {
        // Called when encoder is rotated
        // ticks > 0: clockwise rotation
        // ticks < 0: counter-clockwise rotation
    }
}
```

### Example: Scene Switching with Encoder
```csharp
public class SceneSwitchAdjustableCommand : PluginDynamicCommand
{
    public static SceneSwitchAdjustableCommand Instance { get; private set; }

    public SceneSwitchAdjustableCommand()
    {
        Instance = this;
        this.DisplayName = "Scene Switch";
        this.Description = "Switch between scenes using encoder";
        this.GroupName = "7. Scenes";
        
        OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
    }

    protected override void ApplyAdjustment(String actionParameter, Int32 ticks)
    {
        if (ticks == 0)
            return;

        String[] scenes = OBSStudioForLogiPlugin.Instance?.GetSceneList() ?? new String[0];
        if (scenes.Length == 0)
            return;

        String currentScene = OBSStudioForLogiPlugin.Instance?.CurrentScene ?? String.Empty;
        Int32 currentIndex = Array.IndexOf(scenes, currentScene);
        
        if (currentIndex == -1)
            currentIndex = 0;

        // Calculate new index with wrapping
        Int32 newIndex = currentIndex + (ticks > 0 ? 1 : -1);
        
        if (newIndex < 0)
            newIndex = scenes.Length - 1;
        else if (newIndex >= scenes.Length)
            newIndex = 0;

        OBSStudioForLogiPlugin.Instance?.SwitchScene(scenes[newIndex]);
    }

    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        String currentScene = OBSStudioForLogiPlugin.Instance?.CurrentScene ?? "No Scene";
        return ButtonImageHelper.Text(currentScene, imageSize);
    }

    public void OnConnected()
    {
        this.IsEnabled = true;
        this.ActionImageChanged();
    }

    public void OnDisconnected()
    {
        this.IsEnabled = false;
        this.ActionImageChanged();
    }

    public void OnSceneChanged(String sceneName)
    {
        this.ActionImageChanged();
    }
}
```

## Key Concepts

### Ticks Parameter
- **Positive ticks**: Clockwise rotation (next item)
- **Negative ticks**: Counter-clockwise rotation (previous item)
- **Zero ticks**: No rotation (should be ignored)
- **Magnitude**: Number of detents/clicks rotated (usually 1 or -1)

### Wrapping Behavior
When cycling through a list:
- At end of list + clockwise → wrap to beginning
- At beginning of list + counter-clockwise → wrap to end

```csharp
// Wrap forward
if (newIndex >= items.Length)
    newIndex = 0;

// Wrap backward
if (newIndex < 0)
    newIndex = items.Length - 1;
```

### State Display
Update the button display to show current selection:
```csharp
protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
{
    String currentItem = GetCurrentItem();
    return ButtonImageHelper.Text(currentItem, imageSize);
}
```

Call `ActionImageChanged()` when state changes to refresh display.

## Use Cases

### Scene Switching
- Rotate encoder to cycle through scenes
- Display shows current scene name
- Wraps from last scene to first scene

### Profile Switching
- Rotate encoder to cycle through profiles
- Display shows current profile name
- Wraps from last profile to first profile

### Volume Control
- Rotate encoder to adjust volume
- Display shows current volume percentage
- Clamps at 0% and 100% (no wrapping)

### Audio Source Selection
- Rotate encoder to cycle through audio inputs
- Display shows current input name
- Wraps from last input to first input

## Implementation Checklist

When creating an adjustable command:

- [ ] Inherit from `PluginDynamicCommand`
- [ ] Override `ApplyAdjustment(String actionParameter, Int32 ticks)`
- [ ] Check for `ticks == 0` and return early
- [ ] Get current list of items
- [ ] Find current item index
- [ ] Calculate new index based on ticks direction
- [ ] Implement wrapping or clamping logic
- [ ] Call action method with new item
- [ ] Override `GetCommandImage()` to show current state
- [ ] Call `ActionImageChanged()` when state changes
- [ ] Implement `IObsCommand` for connection state
- [ ] Register command with plugin in constructor

## Benefits

- **Intuitive**: Physical rotation matches mental model of cycling through options
- **Fast**: Quickly navigate through many items without multiple button presses
- **Compact**: Single encoder can replace many buttons
- **Contextual**: Display shows current selection at all times

## Limitations

- **Hardware Dependent**: Requires device with encoder knobs (Loupedeck CT, Live, Live S)
- **No Direct Selection**: Can't jump directly to specific item (must cycle through)
- **Wrapping Can Be Confusing**: Users may not expect wrap-around behavior

## Best Practices

1. **Always show current state** on the button display
2. **Implement wrapping** for finite lists (scenes, profiles)
3. **Implement clamping** for ranges (volume, 0-100%)
4. **Handle empty lists** gracefully (return early)
5. **Update display immediately** after adjustment
6. **Disable when disconnected** from OBS
7. **Log adjustments** for debugging (optional)

## Testing

```csharp
[Fact]
public void ApplyAdjustment_ClockwiseRotation_SelectsNextScene()
{
    // Arrange
    var scenes = new[] { "Scene1", "Scene2", "Scene3" };
    mockPlugin.Setup(x => x.GetSceneList()).Returns(scenes);
    mockPlugin.Setup(x => x.CurrentScene).Returns("Scene1");
    
    // Act
    command.ApplyAdjustment(String.Empty, 1); // Clockwise
    
    // Assert
    mockPlugin.Verify(x => x.SwitchScene("Scene2"), Times.Once);
}

[Fact]
public void ApplyAdjustment_CounterClockwiseAtStart_WrapsToEnd()
{
    // Arrange
    var scenes = new[] { "Scene1", "Scene2", "Scene3" };
    mockPlugin.Setup(x => x.GetSceneList()).Returns(scenes);
    mockPlugin.Setup(x => x.CurrentScene).Returns("Scene1");
    
    // Act
    command.ApplyAdjustment(String.Empty, -1); // Counter-clockwise
    
    // Assert
    mockPlugin.Verify(x => x.SwitchScene("Scene3"), Times.Once);
}
```

## Related Patterns

- **Dynamic Folder**: Shows all items as individual buttons
- **Multi-State Command**: Shows all items as states on one button
- **Adjustable Command**: Cycles through items with encoder

Choose based on:
- **Few items (2-5)**: Multi-State Command
- **Many items (6+)**: Dynamic Folder or Adjustable Command
- **Frequent switching**: Adjustable Command (fastest)
- **Direct selection**: Dynamic Folder (most intuitive)
