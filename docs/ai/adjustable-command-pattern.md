# Action Editor Command Pattern

## Overview

Action Editor commands use the `ActionEditorCommand` base class to create actions the user configures per button, through textboxes, listboxes and checkboxes in Options+ or the Loupedeck software. In this plugin they are the "User defined" actions, plus Plugin Settings. The plugin has no `ActionEditorAdjustment` yet; the SDK section below shows it for completeness.

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
        this.DisplayName = "Switch to Scene (User Defined)";
        this.GroupName = "7. Scenes###User Defined";
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
        if (!actionParameters.TryGetString(SceneNameControlName, out String sceneName) || String.IsNullOrEmpty(sceneName))
        {
            return false;
        }

        actionParameters.TryGetString(ProfileNameControlName, out String profileName);
        actionParameters.TryGetString(CollectionNameControlName, out String collectionName);

        // A sequence of steps with waits needs its own background task. A single OBS call
        // doesn't: the executor already runs writes in the background. The fixed delays
        // here are assessment #37.
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
if (actionParameters.TryGetString("ControlName", out String value))
{
    // Use value
}
```

### Configuration Controls

Control types used in this plugin:

- **ActionEditorTextbox**: single-line text (names of scenes, sources, inputs)
- **ActionEditorListbox**: a list whose items the plugin supplies from `ActionEditor.ListboxItemsRequested` (`MediaActionCommand`'s actions, `PluginSettingsCommand`'s intervals)
- **ActionEditorCheckbox**: a Boolean (`PluginSettingsCommand`)

Check the SDK docs (`sdk-researcher`) before using any other control type.

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

### Source Visibility Toggle

- Configure source name(s) — comma-separated for multiple
- Optional scene name (defaults to current scene)
- Toggle visibility of overlays, cameras, alerts

### Audio Mute Toggle

- Configure audio source name
- Single tap mutes/unmutes the source
- No need to navigate audio folder

### Audio Monitoring Cycle

- Configure audio source name
- Cycles: None → Monitor Only → Monitor & Output
- Quick access to monitoring control

### Audio Source Selection

- Configure audio source name
- Toggle global selection for wheel/dial volume control
- Pairs with SelectedSourceVolumeAdjustment

## Implementation Checklist

When creating an ActionEditorCommand:

- [ ] Inherit from `ActionEditorCommand` or `ActionEditorAdjustment`
- [ ] Set `Name`, `DisplayName`, `GroupName`, `Description`
- [ ] Define control name constants
- [ ] Add controls with `ActionEditor.AddControlEx()`
- [ ] Override `OnLoad()` to return true
- [ ] Override `RunCommand()` or `ApplyAdjustment()`
- [ ] Extract parameters with `TryGetString()` (textboxes, listboxes) or `TryGetBoolean()` (checkboxes), using explicit types
- [ ] Validate required parameters
- [ ] Execute action with parameters
- [ ] Implement `IObsCommand` for connection state
- [ ] Register command with plugin in constructor

## Limitations

- **Typed names**: users type exact scene, source and input names. A typo only shows as a log warning when the button is pressed, and a rename in OBS breaks the button. Listboxes filled from OBS may fix this (assessment #40)
- **Plaintext values**: values typed into a control are saved in plaintext in the device profile, so a secret must be moved elsewhere on save (see `secure-coding.md`)

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
