# Development Guidelines

## Code Quality Standards

### Naming Conventions
- **Classes**: PascalCase (e.g., `OBSActionExecutor`, `PluginResources`, `SceneCollectionSelectCommand`)
- **Interfaces**: PascalCase with `I` prefix (e.g., `IOBSWebsocket`, `IPluginLog`)
- **Private Fields**: camelCase with underscore prefix (e.g., `_obsManager`, `_configReader`, `_mockObs`)
- **Public Properties**: PascalCase (e.g., `IsConnected`, `CurrentProfile`, `ScreenshotPath`)
- **Methods**: PascalCase (e.g., `SetCurrentScene`, `ToggleRecording`, `OnApplicationStarted`)
- **Parameters**: camelCase (e.g., `sceneName`, `profileName`, `actionParameter`)
- **Constants**: SCREAMING_SNAKE_CASE (e.g., `SCENE_COLLECTION_UNSELECTED`, `SCENE_COLLECTION_SELECTED`)

### Type Usage
- **Always use BCL types**: Use `String`, `Boolean`, `Int32`, `Int16`, `Byte[]` instead of `string`, `bool`, `int`, `short`, `byte[]`
- **Explicit types**: Prefer explicit type declarations over `var` for clarity
- **Nullable handling**: Project has nullable reference types disabled (`<Nullable>disable</Nullable>`)

### Code Formatting
- **Braces**: Opening braces on same line for methods and control structures
- **Indentation**: Consistent 4-space indentation
- **Line breaks**: Single line break between methods
- **Namespace**: File-scoped namespace declarations (C# 10+ style)
  ```csharp
  namespace Loupedeck.OBSStudioForLogiPlugin
  {
      using System;
      // class definitions
  }
  ```

### Using Directives
- **Placement**: Inside namespace block, after namespace declaration
- **Order**: System namespaces first, then third-party, then project namespaces
- **Example**:
  ```csharp
  namespace Loupedeck.OBSStudioForLogiPlugin
  {
      using System;
      using System.Threading.Tasks;
      using OBSWebsocketDotNet.Types;
  }
  ```

## Structural Conventions

### Singleton Pattern Implementation
Commands and major components use static `Instance` property:
```csharp
public static SceneCollectionSelectCommand Instance { get; private set; }

public SceneCollectionSelectCommand()
{
    Instance = this;
    // initialization
}
```
- Set `Instance` in constructor
- Use `private set` to prevent external modification
- Access via `ClassName.Instance?.Method()` with null-conditional operator

### Class Organization Order
1. Constants
2. Static properties
3. Private fields
4. Public properties
5. Constructor(s)
6. Public methods
7. Protected/override methods
8. Private methods

Example from `OBSActionExecutor`:
```csharp
public class OBSActionExecutor
{
    // 1. Private fields
    private readonly IOBSWebsocket _obs;
    private readonly IPluginLog _log;
    private OutputState _recordingState = OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED;
    
    // 2. Public properties
    public Boolean IsRecording => /* expression */;
    public String CurrentProfile => this._currentProfile;
    
    // 3. Constructor
    public OBSActionExecutor(IOBSWebsocket obs, IPluginLog log) { }
    
    // 4. Public methods
    public void SetCurrentScene(String sceneName) { }
    
    // 5. Private methods (if any)
}
```

### Dependency Injection
- Constructor injection for dependencies
- Use interfaces for testability (`IOBSWebsocket`, `IPluginLog`)
- Store dependencies in readonly private fields
```csharp
private readonly IOBSWebsocket _obs;
private readonly IPluginLog _log;

public OBSActionExecutor(IOBSWebsocket obs, IPluginLog log)
{
    this._obs = obs;
    this._log = log;
}
```

## Semantic Patterns

### Asynchronous Operations with Task.Run
Wrap long-running or I/O operations in `Task.Run` to avoid blocking:
```csharp
public void SetCurrentScene(String sceneName)
{
    Task.Run(() =>
    {
        if (!this._obs.IsConnected)
        {
            this._log.Warning($"Cannot set scene '{sceneName}' - not connected");
            return;
        }
        
        this._log.Info($"Setting current scene to '{sceneName}'");
        this._obs.SetCurrentProgramScene(sceneName);
    });
}
```
- Use `Task.Run` for fire-and-forget operations
- Always check connection state before OBS operations
- Log warnings for failed preconditions
- Log info for successful operations

### Async/Await Pattern
For operations requiring sequential async steps:
```csharp
private async void OnApplicationStarted(Object sender, EventArgs e)
{
    await Task.Run(async () =>
    {
        PluginLog.Info("OBS application started");
        
        var settings = this._configReader.ReadConfig();
        if (settings == null)
        {
            PluginLog.Warning("No valid OBS configuration found");
            return;
        }

        var portReady = await this._lifecycleManager.WaitForPortAsync("127.0.0.1", settings.Port);
        
        if (portReady)
        {
            await Task.Delay(2000);
            await this._obsManager.ConnectAsync(settings.GetWebSocketUrl(), settings.Password);
        }
    });
}
```
- Use `async void` for event handlers
- Wrap in `Task.Run(async () => ...)` for background execution
- Use `await` for sequential async operations
- Add delays (`Task.Delay`) for timing-sensitive operations

### Null-Conditional Operator Pattern
Extensively use `?.` for safe navigation:
```csharp
// Safe method invocation
ProfileSelectCommand.Instance?.OnConnected();
this._obsManager?.Dispose();

// Safe property access with null coalescing
public String CurrentProfile => this._obsManager?.Actions.CurrentProfile ?? String.Empty;
public String[] GetProfileList() => this._obsManager?.Actions.GetProfileList() ?? new String[0];
```
- Always use `?.` when accessing singleton instances
- Combine with `??` operator for default values
- Return empty arrays (`new String[0]`) instead of null for collections

### Guard Clauses
Always validate preconditions at method start:
```csharp
public void SetCurrentProfile(String profileName)
{
    Task.Run(() =>
    {
        // Connection check
        if (!this._obs.IsConnected)
        {
            this._log.Warning($"Cannot set profile '{profileName}' - not connected");
            return;
        }

        // Redundant operation check
        if (this._currentProfile == profileName)
        {
            this._log.Info($"Profile '{profileName}' is already active");
            return;
        }

        // Actual operation
        this._log.Info($"Setting current profile to '{profileName}'");
        this._obs.SetCurrentProfile(profileName);
    });
}
```
- Check connection state first
- Check for redundant operations
- Log appropriate messages for each guard
- Return early to avoid nesting

### String Validation Pattern
Use `String.IsNullOrEmpty()` for string validation:
```csharp
protected override void RunCommand(String actionParameter)
{
    if (String.IsNullOrEmpty(actionParameter))
        return;

    OBSStudioForLogiPlugin.Instance?.SwitchSceneCollection(actionParameter);
}
```

### Logging Pattern
Consistent logging throughout the codebase:
```csharp
// Info: Normal operations
PluginLog.Info("Plugin loading...");
PluginLog.Info($"Setting current scene to '{sceneName}'");

// Warning: Expected failures or missing preconditions
PluginLog.Warning("No valid OBS configuration found");
PluginLog.Warning($"Cannot set scene '{sceneName}' - not connected");

// Error: Unexpected failures
PluginLog.Error("OBS WebSocket port did not become available");
```
- Use string interpolation for dynamic messages
- Include relevant context (names, states) in messages
- Log before and after significant operations

## Internal API Usage

### Loupedeck Plugin API

#### Plugin Base Class
```csharp
public class OBSStudioForLogiPlugin : Plugin
{
    public override Boolean UsesApplicationApiOnly => true;
    public override Boolean HasNoApplication => false;

    public override void Load() { }
    public override void Unload() { }
}
```
- Override `Load()` for initialization
- Override `Unload()` for cleanup
- Set `UsesApplicationApiOnly` and `HasNoApplication` appropriately

#### Command Base Classes
```csharp
// Multi-state commands (selected/unselected)
public class SceneCollectionSelectCommand : PluginMultistateDynamicCommand
{
    public SceneCollectionSelectCommand()
    {
        this.Description = "Switches to a specific scene collection in OBS Studio";
        this.GroupName = "5. Scenes";
        this.AddState("", "Scene collection unselected");
        this.AddState("", "Scene collection selected");
    }

    protected override Boolean OnLoad()
    {
        this.IsEnabled = false;
        this.ResetParameters(false);
        return true;
    }

    protected override void RunCommand(String actionParameter) { }
    
    protected override BitmapImage GetCommandImage(String actionParameter, Int32 stateIndex, PluginImageSize imageSize)
    {
        return null; // Return null for default icon
    }
}
```

#### Dynamic Parameters
```csharp
private void ResetParameters(Boolean readContent)
{
    this.RemoveAllParameters();

    if (readContent)
    {
        var sceneCollections = OBSStudioForLogiPlugin.Instance?.GetSceneCollectionList() ?? new String[0];
        
        foreach (var sceneCollection in sceneCollections)
        {
            this.AddParameter(sceneCollection, $"{sceneCollection} Collection", this.GroupName)
                .Description = $"Switch to scene collection \"{sceneCollection}\"";
            this.SetCurrentState(sceneCollection, sceneCollection == currentSceneCollection ? SELECTED : UNSELECTED);
        }
    }

    this.ParametersChanged();
    this.ActionImageChanged();
}
```
- Call `RemoveAllParameters()` before rebuilding
- Use `AddParameter(value, displayName, groupName)` to add options
- Set state with `SetCurrentState(parameter, stateIndex)`
- Call `ParametersChanged()` and `ActionImageChanged()` after updates

#### ClientApplication Integration
```csharp
public class OBSStudioForLogiApplication : ClientApplication
{
    protected override String GetProcessName() => "obs64.exe";
    protected override String GetBundleName() => "com.obsproject.obs-studio";
    
    public override ClientApplicationStatus GetApplicationStatus()
    {
        return ClientApplicationStatus.Unknown;
    }
}
```

#### Event Subscription
```csharp
this.ClientApplication.ApplicationStarted += this.OnApplicationStarted;
this.ClientApplication.ApplicationStopped += this.OnApplicationStopped;

// Cleanup in Unload
this.ClientApplication.ApplicationStarted -= this.OnApplicationStarted;
this.ClientApplication.ApplicationStopped -= this.OnApplicationStopped;
```

### Resource Management
```csharp
// Initialize in plugin constructor
PluginResources.Init(this.Assembly);

// Load embedded resources
this.Info.Icon256x256 = EmbeddedResources.ReadImage("Loupedeck.OBSStudioForLogiPlugin.metadata.Icon256x256.png");

// Use PluginResources helper
var image = PluginResources.ReadImage("ResourceName.png");
var text = PluginResources.ReadTextFile("ResourceName.txt");
```

## Testing Patterns

### Unit Test Structure (xUnit)
```csharp
namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Moq;
using OBSWebsocketDotNet.Types;

public class OBSActionExecutorTests
{
    private readonly Mock<IOBSWebsocket> _mockObs;
    private readonly Mock<IPluginLog> _mockLog;
    private readonly OBSActionExecutor _executor;

    public OBSActionExecutorTests()
    {
        this._mockObs = new Mock<IOBSWebsocket>();
        this._mockLog = new Mock<IPluginLog>();
        this._executor = new OBSActionExecutor(this._mockObs.Object, this._mockLog.Object);
    }

    [Fact]
    public void GetProfileList_WhenConnected_ReturnsProfiles()
    {
        // Arrange
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetProfileList()).Returns(new[] { "profile1", "profile2" });

        // Act
        var result = this._executor.GetProfileList();

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Contains("profile1", result);
    }
}
```

### Mocking with Moq
- Use `Mock<T>` for interface dependencies
- Setup return values: `mock.Setup(x => x.Method()).Returns(value)`
- Verify calls: `mock.Verify(x => x.Method(param), Times.Once)`
- Access mock object: `mock.Object`

### Test Naming Convention
Format: `MethodName_Condition_ExpectedBehavior`
- `GetProfileList_WhenConnected_ReturnsProfiles`
- `GetProfileList_WhenNotConnected_ReturnsEmpty`
- `SetCurrentProfile_WhenConnected_CallsObs`

### Async Test Handling
```csharp
[Fact]
public void SetCurrentProfile_WhenConnected_CallsObs()
{
    this._mockObs.Setup(x => x.IsConnected).Returns(true);

    this._executor.SetCurrentProfile("test");

    // Wait for Task.Run to complete
    System.Threading.Thread.Sleep(100);
    this._mockObs.Verify(x => x.SetCurrentProfile("test"), Times.Once);
}
```
- Add `Thread.Sleep()` when testing `Task.Run` fire-and-forget methods
- Keep delays minimal (100ms typically sufficient)

## Common Idioms

### State Management Pattern
```csharp
public void SetRecordingState(OutputState state)
{
    this._recordingState = state;
    
    if (state == OutputState.OBS_WEBSOCKET_OUTPUT_PAUSED)
    {
        this.IsRecordingPaused = true;
    }
    else if (state == OutputState.OBS_WEBSOCKET_OUTPUT_RESUMED || state == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED)
    {
        this.IsRecordingPaused = false;
    }
    else if (state == OutputState.OBS_WEBSOCKET_OUTPUT_STOPPED)
    {
        this.IsRecordingPaused = false;
    }
}
```
- Update internal state first
- Use if-else chains for state transitions
- Update derived properties based on state

### Expression-Bodied Properties
```csharp
public Boolean IsRecording => this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED 
                            || this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_PAUSED
                            || this._recordingState == OutputState.OBS_WEBSOCKET_OUTPUT_RESUMED;

public String CurrentProfile => this._obsManager?.Actions.CurrentProfile ?? String.Empty;
```
- Use for computed properties
- Multi-line expressions aligned with `||` or `&&` operators
- Combine with null-conditional and null-coalescing operators

### Connection State Callbacks
```csharp
public void OnConnected()
{
    this.IsEnabled = true;
    this.ResetParameters(true);
}

public void OnDisconnected()
{
    this.IsEnabled = false;
    this.ResetParameters(false);
}
```
- Enable/disable commands based on connection
- Refresh parameters with appropriate data loading flag

### Event Notification Pattern
```csharp
public void OnCurrentSceneCollectionChanged(String oldSceneCollection, String newSceneCollection)
{
    if (!String.IsNullOrEmpty(oldSceneCollection))
    {
        this.SetCurrentState(oldSceneCollection, SCENE_COLLECTION_UNSELECTED);
    }

    if (!String.IsNullOrEmpty(newSceneCollection))
    {
        this.SetCurrentState(newSceneCollection, SCENE_COLLECTION_SELECTED);
    }

    this.ActionImageChanged();
}
```
- Accept both old and new values
- Validate strings before processing
- Update UI state for both old and new values
- Trigger UI refresh at end

## Best Practices Summary

1. **Always use BCL type names** (`String`, `Boolean`, `Int32`) instead of C# keywords
2. **Prefix private fields** with underscore (`_fieldName`)
3. **Use readonly** for injected dependencies
4. **Implement singleton pattern** via static `Instance` property
5. **Wrap async operations** in `Task.Run` for fire-and-forget
6. **Use guard clauses** for early returns with logging
7. **Apply null-conditional operators** (`?.`) extensively
8. **Return empty collections** instead of null (`new String[0]`)
9. **Log all significant operations** with appropriate level
10. **Test with Moq** for interface dependencies
11. **Follow Arrange-Act-Assert** pattern in tests
12. **Initialize singletons** in constructor
13. **Clean up event subscriptions** in Unload/Dispose
14. **Use string interpolation** for log messages
15. **Check connection state** before OBS operations
