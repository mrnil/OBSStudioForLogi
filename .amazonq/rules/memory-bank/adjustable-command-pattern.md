# Action Editor Command Pattern

## Overview
Action Editor commands use the ActionEditorCommand base class to create configurable actions with user-defined parameters. These commands allow users to configure actions through textboxes, dropdowns, and other controls in the Loupedeck software.

## Pattern Implementation

### Base Classes

#### ActionEditorCommand
For button press actions with configurable parameters:
```csharp
public class MyCommand : ActionEditorCommand
{
    protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
    {
        // Execute action with user-configured parameters
        return true;
    }
}
```

#### ActionEditorAdjustment
For encoder rotation actions with configurable parameters:
```csharp
public class MyAdjustment : ActionEditorAdjustment
{
    public MyAdjustment() : base(hasReset: false)
    {
    }

    protected override Boolean ApplyAdjustment(ActionEditorActionParameters actionParameters, Int32 diff)
    {
        // diff > 0: clockwise rotation
        // diff < 0: counter-clockwise rotation
        return true;
    }
}
```

### Example: Scene Switching with Configuration
```csharp
public class SceneSwitchAdjustableCommand : ActionEditorCommand, IObsCommand
{
    private const String ProfileNameControlName = "ProfileName";
    private const String CollectionNameControlName = "CollectionName";
    private const String SceneNameControlName = "SceneName";

    public static SceneSwitchAdjustableCommand Instance { get; private set; }

    public SceneSwitchAdjustableCommand()
    {
        Instance = this;
        OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
        
        this.Name = "SceneSwitchAdjustable";
        this.DisplayName = "Switch to Scene (Adjustable)";
        this.GroupName = "7. Scenes";
        this.Description = "Switch to a specific scene with optional profile and collection switching";

        // Add configuration controls
        this.ActionEditor.AddControlEx(new ActionEditorTextbox(ProfileNameControlName, "Profile Name (optional)"));
        this.ActionEditor.AddControlEx(new ActionEditorTextbox(CollectionNameControlName, "Collection Name (optional)"));
        this.ActionEditor.AddControlEx(new ActionEditorTextbox(SceneNameControlName, "Scene Name (required)"));
    }

    protected override Boolean OnLoad() => true;

    protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
    {
        // Get user-configured values
        if (!actionParameters.TryGetString(SceneNameControlName, out var sceneName) || String.IsNullOrEmpty(sceneName))
            return false;

        actionParameters.TryGetString(ProfileNameControlName, out var profileName);
        actionParameters.TryGetString(CollectionNameControlName, out var collectionName);

        // Execute action
        Task.Run(async () =>
        {
            if (!String.IsNullOrEmpty(profileName))
            {
                OBSStudioForLogiPlugin.Instance?.SwitchProfile(profileName);
                await Task.Delay(OBSTimings.ProfileSwitchDelay);
            }

            if (!String.IsNullOrEmpty(collectionName))
            {
                OBSStudioForLogiPlugin.Instance?.SwitchSceneCollection(collectionName);
                await Task.Delay(OBSTimings.CollectionSwitchDelay);
            }

            OBSStudioForLogiPlugin.Instance?.SwitchScene(sceneName);
        });

        return true;
    }

    public void OnConnected() { }
    public void OnDisconnected() { }
}
```

## Key Concepts

### ActionEditorActionParameters
Parameters configured by the user in the Loupedeck software:
```csharp
// Get string parameter
if (actionParameters.TryGetString("ControlName", out var value))
{
    // Use value
}

// Get integer parameter
if (actionParameters.TryGetInt("ControlName", out var intValue))
{
    // Use intValue
}
```

### Configuration Controls
Available control types:
- **ActionEditorTextbox**: Single-line text input
- **ActionEditorDropdown**: Dropdown selection
- **ActionEditorCheckbox**: Boolean checkbox
- **ActionEditorSlider**: Numeric slider

### Control Names
Use constants for control names to avoid typos:
```csharp
private const String ProfileNameControlName = "ProfileName";
private const String SceneNameControlName = "SceneName";

this.ActionEditor.AddControlEx(new ActionEditorTextbox(ProfileNameControlName, "Profile Name"));
```

## Use Cases

### Scene Switching with Context
- Configure profile, collection, and scene name
- Single button switches entire OBS context
- Useful for different show formats

### Audio Volume Adjustment
- Configure audio input name
- Encoder adjusts volume for that specific input
- No need for dynamic folders

### Parameterized Actions
- Any action that needs user configuration
- Reusable across different contexts
- Configuration stored per button instance

## Implementation Checklist

When creating an ActionEditorCommand:

- [ ] Inherit from `ActionEditorCommand` or `ActionEditorAdjustment`
- [ ] Set `Name`, `DisplayName`, `GroupName`, `Description`
- [ ] Define control name constants
- [ ] Add controls with `ActionEditor.AddControlEx()`
- [ ] Override `OnLoad()` to return true
- [ ] Override `RunCommand()` or `ApplyAdjustment()`
- [ ] Extract parameters with `TryGetString()`, `TryGetInt()`, etc.
- [ ] Validate required parameters
- [ ] Execute action with parameters
- [ ] Implement `IObsCommand` for connection state
- [ ] Register command with plugin in constructor

## Benefits

- **Configurable**: Users customize behavior per button
- **Reusable**: Same command, different configurations
- **Flexible**: Supports optional and required parameters
- **Type-Safe**: Parameter extraction with type checking
- **User-Friendly**: Configuration UI in Loupedeck software

## Limitations

- **Static Configuration**: Parameters set at design time, not runtime
- **No Dynamic Lists**: Can't populate dropdowns from OBS
- **Manual Entry**: Users must type exact names (profiles, scenes, etc.)
- **No Validation**: Invalid names fail silently at runtime

## Best Practices

1. **Use constants** for control names
2. **Validate required parameters** before execution
3. **Provide clear labels** for controls
4. **Mark optional parameters** in labels
5. **Log parameter values** for debugging
6. **Handle missing/invalid values** gracefully
7. **Use async operations** for OBS actions
8. **Add delays** between sequential OBS operations

## Comparison with Other Patterns

| Pattern | Use Case | Configuration | Selection |
|---------|----------|---------------|----------|
| **ActionEditorCommand** | Parameterized actions | User-configured | Manual entry |
| **Dynamic Folder** | List of items | Auto-populated | Click button |
| **Multi-State Command** | Few fixed options | Predefined states | Cycle through |

Choose based on:
- **User-configured actions**: ActionEditorCommand
- **Dynamic lists from OBS**: Dynamic Folder
- **Fixed set of options**: Multi-State Command
