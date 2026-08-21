# Engineering Standards — C# / Loupedeck LogiActionSDK Projects

## 1. Code Style

### Language and Types
- Target framework: **.NET 8.0** (`net8.0`)
- Always use **BCL type names** — never C# keywords:
  ```csharp
  // CORRECT
  String name;
  Boolean isActive;
  Int32 count;
  Single volume;

  // WRONG
  string name;
  bool isActive;
  int count;
  float volume;
  ```
- Never use `var` — always declare explicit types:
  ```csharp
  // CORRECT
  String[] profiles = this._obs.GetProfileList();

  // WRONG
  var profiles = this._obs.GetProfileList();
  ```

### Qualification and Naming
- Every field, method, property, and event access **must** be qualified with `this.`
- Private fields use `_camelCase` with underscore prefix: `private String _currentScene;`
- Interfaces use `I` prefix: `IObsCommand`, `IPluginLog`
- Types, methods, properties: PascalCase
- `readonly` preferred for injected dependencies: `private readonly IOBSWebsocket _obs;`

### Braces and Layout
- **Allman brace style** — opening brace always on its own line
- **Braces always required** — even for single-line `if`/`foreach`/`while`
- `using` directives inside namespace declaration
- 4-space indentation, CRLF line endings

```csharp
namespace MyPlugin
{
    using System;

    public class MyClass
    {
        public void MyMethod()
        {
            if (condition)
            {
                DoSomething();
            }
        }
    }
}
```

---

## 2. Architecture

### Layered Structure
```
Loupedeck SDK (Actions layer)
        ↓
Plugin main class (orchestration)
        ↓
Facade (simplified public interface)
        ↓
Manager → Executor → IOBSWebsocket interface
                            ↓
                     Adapter → third-party library
```

### Key Principles
- **Single Responsibility**: each class has one reason to change
- **Dependency Inversion**: depend on interfaces, not concretions
- **Interface-based design**: all external dependencies injected via interfaces for testability
- **No God Classes**: if a class exceeds ~300 lines or has more than 3 responsibilities, split it

### Command Self-Registration Pattern
Every command sets its static `Instance` and registers with the plugin in its constructor:
```csharp
public class MyCommand : PluginDynamicCommand, IObsCommand
{
    public static MyCommand Instance { get; private set; }

    public MyCommand()
    {
        Instance = this;
        MyPlugin.Instance?.RegisterCommand(this);
        this.DisplayName = "My Command";
        this.GroupName = "1. Group";
        this.Description = "What this command does";
    }
}
```

### Notification Interface Pattern
Commands declare which events they care about by implementing interfaces:
```csharp
public interface IObsCommand
{
    void OnConnected();
    void OnDisconnected();
}

public interface ISceneAwareCommand : IObsCommand
{
    void OnSceneChanged(String sceneName);
}
```

A `CommandRegistry` dispatches events via interface type-filtering:
```csharp
foreach (var command in this._commands.OfType<ISceneAwareCommand>())
    command.OnSceneChanged(sceneName);
```

Adding a new notification type requires:
1. Add interface to `IObsCommand.cs`
2. Add `NotifyXxx()` to `CommandRegistry`
3. Add `NotifyXxx()` pass-through to `CommandCoordinator`
4. Subscribe to the OBS event in the WebSocket manager
5. Call `Plugin.Instance?.OnXxx()` from the event handler
6. Add `OnXxx()` to the plugin calling `_commandCoordinator.NotifyXxx()`
7. Implement the interface in relevant commands

---

## 3. Service Layer Patterns

### Connection Guard (Universal)
Every service method that calls OBS must guard on connection state first:
```csharp
public void ToggleRecording()
{
    Task.Run(() =>
    {
        if (!this._obs.IsConnected)
        {
            this._log.Warning("Cannot toggle recording - not connected");
            return;
        }
        try
        {
            this._obs.ToggleRecord();
        }
        catch (Exception ex)
        {
            this._log.Error($"Failed to toggle recording: {ex.Message}");
        }
    });
}
```

### Async Fire-and-Forget (All OBS Mutations)
All OBS write operations use `Task.Run` to avoid blocking the UI thread. Never `await` on the UI thread.

### Safe Default Return (All OBS Queries)
Query methods return safe defaults on disconnection or error — never throw:
```csharp
public Boolean GetInputMute(String inputName)
{
    if (!this._obs.IsConnected)
        return false;
    try
    {
        return this._obs.GetInputMute(inputName);
    }
    catch (Exception ex)
    {
        this._log.Error($"Failed to get mute state for '{inputName}': {ex.Message}");
        return false;
    }
}
```

Safe defaults by type:
- `Boolean` → `false`
- `String` → `String.Empty`
- `String[]` → `new String[0]`
- `Single` (volume) → `1.0f`
- Model objects → `null` (or a null-object if the type supports it)

### Input Validation
Validate string parameters before proceeding:
```csharp
if (String.IsNullOrEmpty(sourceName))
{
    this._log.Warning("Cannot toggle source - source name is empty");
    return;
}
```

### Timing Constants
All timing values must be centralised in a static `Timings` class — never hardcode millisecond values:
```csharp
public static class OBSTimings
{
    public const Int32 StateUpdateDelay = 100;
    public const Int32 TestAsyncDelay = 500;
}
```

---

## 4. Loupedeck LogiActionSDK — Command Patterns

### PluginDynamicCommand — Icon Only
```csharp
public class MyToggleCommand : PluginDynamicCommand
{
    public MyToggleCommand()
        : base("My Toggle", "Toggles something", "1. Group")
    {
        this.IsWidget = true; // REQUIRED for icon-only display
    }

    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        Boolean isActive = MyPlugin.Instance?.IsActive ?? false;
        return ButtonImageHelper.StateIcon(isActive, "ActiveIcon.svg", "InactiveIcon.svg");
    }

    protected override void RunCommand(String actionParameter)
    {
        Task.Run(() => MyPlugin.Instance?.Toggle());
    }
}
```

### PluginDynamicCommand — Text Display
```csharp
public class MyDisplay : PluginDynamicCommand
{
    public MyDisplay()
        : base("My Display", "Shows current state", "1. Group")
    {
        // Do NOT set IsWidget for text-only display
    }

    protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
    {
        return $"Label\r\n{GetCurrentValue()}";
    }

    // Do NOT override GetCommandImage for text-only display
}
```

### PluginDynamicCommand — Text + Icon (Rendered)
```csharp
public MyCommand()
{
    this.IsWidget = true; // Required when overriding GetCommandImage
}

protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
{
    Boolean isActive = GetState();
    String text = $"Label\n\nValue";
    return ButtonImageHelper.StateTextWithIcon(text, imageSize, isActive,
        "ActiveIcon.svg", "InactiveIcon.svg",
        BitmapColor.Green, BitmapColor.Red);
}
```

### PluginDynamicFolder
```csharp
public class MyDynamicFolder : PluginDynamicFolder, IObsCommand
{
    private String[] _items = new String[0];

    public MyDynamicFolder()
    {
        Instance = this;
        MyPlugin.Instance?.RegisterCommand(this);
        this.DisplayName = "My Folder";
        this.GroupName = "1. Group";
        this.Description = "Browse items";
    }

    public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType)
    {
        return this._items;
    }

    public override void RunCommand(String actionParameter)
    {
        if (String.IsNullOrEmpty(actionParameter))
            return;
        Task.Run(() => MyPlugin.Instance?.DoAction(actionParameter));
    }

    public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        Boolean isSelected = actionParameter == MyPlugin.Instance?.CurrentItem;
        return ButtonImageHelper.StateIcon(isSelected, "Selected.svg", "Unselected.svg");
    }

    public void OnConnected()
    {
        this._items = MyPlugin.Instance?.GetItems() ?? new String[0];
        this.ButtonActionNamesChanged();
    }

    public void OnDisconnected()
    {
        this._items = new String[0];
        this.ButtonActionNamesChanged();
    }
}
```

### ActionEditorCommand — User-Defined Configurable Actions
```csharp
public class MyAdjustableCommand : ActionEditorCommand, IObsCommand
{
    private const String SourceNameControlName = "SourceName";

    public static MyAdjustableCommand Instance { get; private set; }

    public MyAdjustableCommand()
    {
        Instance = this;
        MyPlugin.Instance?.RegisterCommand(this);
        this.Name = "MyAction";
        this.DisplayName = "My Action";
        this.GroupName = "1. Group###User Defined";
        this.Description = "Description of what this does";
        this.ActionEditor.AddControlEx(new ActionEditorTextbox(SourceNameControlName, "Source Name (required)"));
    }

    protected override Boolean OnLoad() => true;

    protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
    {
        if (!actionParameters.TryGetString(SourceNameControlName, out var sourceName) || String.IsNullOrEmpty(sourceName))
            return false;

        Task.Run(() => MyPlugin.Instance?.DoAction(sourceName));
        return true;
    }

    public void OnConnected() { }
    public void OnDisconnected() { }
}
```

Available control types: `ActionEditorTextbox`, `ActionEditorDropdown`, `ActionEditorCheckbox`, `ActionEditorSlider`.
Always use **constants** for control names to avoid typos.

### ActionEditorAdjustment — Encoder/Dial
```csharp
public class MyAdjustment : ActionEditorAdjustment
{
    public MyAdjustment() : base(hasReset: true) { }

    protected override Boolean ApplyAdjustment(ActionEditorActionParameters actionParameters, Int32 diff)
    {
        // diff > 0: clockwise, diff < 0: counter-clockwise
        return true;
    }
}
```

### PluginDynamicAdjustment — Standalone Dial/Wheel
```csharp
public class MyAdjustment : PluginDynamicAdjustment
{
    public MyAdjustment()
        : base("My Adjustment", "Adjusts value", "1. Group", hasReset: true)
    {
    }

    protected override void ApplyAdjustment(String actionParameter, Int32 diff)
    {
        Task.Run(() => MyPlugin.Instance?.AdjustValue(diff));
        this.AdjustmentValueChanged();
    }

    protected override void RunCommand(String actionParameter)
    {
        // Called on dial press when hasReset: true — reset to default
        Task.Run(() => MyPlugin.Instance?.ResetValue());
        this.AdjustmentValueChanged();
    }

    protected override String GetAdjustmentValue(String actionParameter)
    {
        return $"{MyPlugin.Instance?.CurrentValue ?? 0}%";
    }
}
```

### Sub-groups
Use `###` separator to create sub-groups in the Loupedeck UI:
```csharp
this.GroupName = "7. Scenes###Available Scenes";   // sub-group
this.GroupName = "8. Audio###User Defined";         // user-defined sub-group
```

### Key SDK Rules
- **Never mix** `GetCommandDisplayName` and `GetCommandImage` overrides in the same command
- `IsWidget = true` is **required** when overriding `GetCommandImage`
- **Never call** `ButtonActionNamesChanged()` just to refresh icons — use `CommandImageChanged(actionParameter)` for targeted refresh
- **Never refresh immediately** after a toggle in `RunCommand` — use a delayed callback after OBS processes the change
- `GetCommandImage` must **always query live state** — never cache state inside the command class
- All OBS operations in `RunCommand` must use `Task.Run` — never block

---

## 5. Image Rendering

### ButtonImageHelper — Always Use This
```csharp
// Static icon
ButtonImageHelper.Icon("MyIcon.svg");

// State-based icon
ButtonImageHelper.StateIcon(isActive, "On.svg", "Off.svg");

// Text only
ButtonImageHelper.Text("Connected", imageSize, BitmapColor.Green, BitmapColor.White);

// State-based text
ButtonImageHelper.StateText(text, imageSize, isActive, BitmapColor.Green, BitmapColor.Red);

// Text with background icon
ButtonImageHelper.TextWithIcon(text, imageSize, "Icon.svg", BitmapColor.Green);

// State-based text with icon
ButtonImageHelper.StateTextWithIcon(text, imageSize, isActive,
    "On.svg", "Off.svg", BitmapColor.Green, BitmapColor.Red);
```

### Icon Resource Names
Pass only the filename — the helper resolves the full embedded resource path:
```csharp
ButtonImageHelper.Icon("MyIcon.svg")  // CORRECT
ButtonImageHelper.Icon("Loupedeck.MyPlugin.Icons.MyIcon.svg")  // WRONG
```

### Colour Conventions
- Green (`BitmapColor.Green`) — active, unmuted, connected, healthy
- Red (`BitmapColor.Red`) — inactive, muted, error
- Yellow (`new BitmapColor(255, 200, 0)`) — warning, paused
- Orange — disabled/degraded state
- Grey (`new BitmapColor(128, 128, 128)`) — stopped, idle, disconnected

---

## 6. Test Driven Development

### Scope
| Layer | Coverage Target | Notes |
|-------|----------------|-------|
| Services (`OBSActionExecutor`, `CommandRegistry`, etc.) | **80%+ line coverage** | Full TDD — write tests first |
| Business logic / helpers | **90%+ line coverage** | Full TDD |
| Actions/Commands — non-rendering | **All paths tested** | Constructor, singleton, `OnConnected`, `OnDisconnected`, state methods |
| Actions/Commands — rendering | Exempt | `GetCommandImage`, `GetCommandDisplayName` require SDK runtime |
| Third-party adapters (pass-through) | Exempt | Tested indirectly via executor tests |

### Test Naming Convention
`MethodName_Condition_ExpectedBehaviour`:
```csharp
GetInputMute_WhenConnected_ReturnsMuteState()
GetInputMute_WhenNotConnected_ReturnsFalse()
GetInputMute_WhenOBSThrows_LogsErrorAndReturnsFalse()
```

### Always Test Three Paths
For every service method:
1. **Happy path** — connected, succeeds
2. **Disconnected path** — returns safe default, no OBS call made
3. **Exception path** — logs error with context, returns safe default

```csharp
[Fact]
public void GetInputMute_WhenOBSThrows_LogsErrorAndReturnsFalse()
{
    this._mockObs.Setup(x => x.IsConnected).Returns(true);
    this._mockObs.Setup(x => x.GetInputMute(It.IsAny<String>())).Throws(new Exception("OBS error"));

    Boolean result = this._executor.GetInputMute("Microphone");

    Assert.False(result);
    this._mockLog.Verify(x => x.Error(
        It.Is<String>(s => s.Contains("Microphone") && s.Contains("OBS error"))),
        Times.Once);
}
```

### Async Fire-and-Forget Testing
Use `Thread.Sleep(OBSTimings.TestAsyncDelay)` after triggering async operations:
```csharp
[Fact]
public void SetInputVolume_WhenConnected_CallsObs()
{
    this._mockObs.Setup(x => x.IsConnected).Returns(true);

    this._executor.SetInputVolume("Microphone", 0.5f);

    System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
    this._mockObs.Verify(x => x.SetInputVolume("Microphone", 0.5f), Times.Once);
}
```

### Test Class Structure
```csharp
public class MyExecutorTests
{
    private readonly Mock<IOBSWebsocket> _mockObs;
    private readonly Mock<IPluginLog> _mockLog;
    private readonly MyExecutor _executor;

    public MyExecutorTests()
    {
        this._mockObs = new Mock<IOBSWebsocket>();
        this._mockLog = new Mock<IPluginLog>();
        this._executor = new MyExecutor(this._mockObs.Object, this._mockLog.Object);
    }
}
```

### Log Message Verification
Error messages must contain both the entity name and the exception message:
```csharp
this._mockLog.Verify(x => x.Error(
    It.Is<String>(s => s.Contains("Microphone") && s.Contains("OBS error"))),
    Times.Once);
```

---

## 7. Logging

### Injection
Services receive `IPluginLog` via constructor injection. Never use static logging in testable service classes.

### Log Levels
| Level | When to Use |
|-------|-------------|
| `Trace` | Render/UI calls (`GetCommandImage`) — very high frequency |
| `Debug` | Operational detail (folder updates, selection state, intermediate steps) |
| `Info` | Significant state changes (connected, disconnected, scene changed) |
| `Warning` | Guard clause violations (not connected, invalid state, empty parameter) |
| `Error` | Caught exceptions with context |

### Message Format
Always include the entity name and action:
```csharp
this._log.Info($"Setting current profile to '{profileName}'");
this._log.Warning($"Cannot set profile '{profileName}' - not connected");
this._log.Error($"Failed to set profile '{profileName}': {ex.Message}");
```

### Rules
- **Never log passwords**, tokens, or credentials
- **Sanitize file paths** — avoid exposing usernames in log output
- Every `catch` block must log with sufficient context to diagnose the failure

---

## 8. Secure Coding

### Input Validation
- Use `String.IsNullOrEmpty()` for all string inputs from external sources
- Validate port numbers are in range 1–65535
- Validate file paths before reading/writing — prevent path traversal
- Handle JSON parsing exceptions gracefully
- Validate IP addresses before connecting

### Credentials and Sensitive Data
- Never store passwords or tokens in source code
- Never log passwords, tokens, or authentication data
- Mark sensitive configuration fields clearly (e.g. label text "(sensitive)")
- Clear sensitive data from memory when no longer needed

### Communication
- WebSocket connections must only connect to validated addresses
- Use the library's built-in authentication mechanism
- Handle connection failures gracefully with retry/backoff logic
- Note: `ws://` connections are unencrypted — document this for remote connection scenarios

### File Operations
- Only read from known configuration directories
- Only write to user-approved directories (Pictures, Documents, Desktop)
- Use `Path.Combine()` for cross-platform path construction
- Always handle file I/O exceptions

### Memory Management
- Properly dispose all `IDisposable` resources
- Unsubscribe from all events in `Unload`/`Dispose`
- Use `using` statements for file operations
- Avoid memory leaks in long-running async operations — dispose `CancellationTokenSource` after use

### General
- Use managed code (.NET) — avoid unsafe blocks
- Never execute OS commands from user input
- Validate connection state before all OBS operations
- Use guard clauses for early validation

---

## 9. Conventional Commits

All commits must follow the conventional commits format:

```
<type>: <short summary>

<body — what changed and why>
```

### Types
| Type | When to Use |
|------|-------------|
| `feat` | New feature or command |
| `fix` | Bug fix |
| `refactor` | Code change that neither fixes a bug nor adds a feature |
| `test` | Adding or updating tests |
| `chore` | Dependency updates, build config, tooling |
| `docs` | Documentation only |
| `perf` | Performance improvement |

### Rules
- Summary line: imperative mood, lowercase, no full stop, max 72 characters
- Body: explain **what** changed and **why** — not how
- Reference issue numbers where applicable

---

## 10. Adding a New Feature — Checklist

### New OBS API Method
- [ ] Add method signature to `IOBSWebsocket`
- [ ] Implement in adapter (thin pass-through only)
- [ ] Add business logic to executor (connection guard + error handling)
- [ ] Expose via facade
- [ ] Expose via plugin public method
- [ ] Write tests for all three paths (connected, disconnected, exception)

### New Command
- [ ] Create class in `Actions/`
- [ ] Inherit from appropriate base (`ToggleCommandBase`, `PluginDynamicFolder`, `ActionEditorCommand`, etc.)
- [ ] Implement `IObsCommand` and any relevant notification interfaces
- [ ] Set `Instance = this` and call `RegisterCommand(this)` in constructor
- [ ] Set `DisplayName`, `GroupName`, `Description`
- [ ] Override `RunCommand` with null guard and `Task.Run`
- [ ] Override `GetCommandImage` querying live state only
- [ ] Implement `OnConnected` / `OnDisconnected`
- [ ] Add icon SVG to `Resources/icons/` and register as `EmbeddedResource` in `.csproj`
- [ ] Write tests for constructor, singleton, `OnConnected`, `OnDisconnected`

### New Notification Event
- [ ] Add interface to `IObsCommand.cs`
- [ ] Add `NotifyXxx()` to `CommandRegistry`
- [ ] Add `NotifyXxx()` pass-through to `CommandCoordinator`
- [ ] Subscribe to event in WebSocket manager
- [ ] Call `Plugin.Instance?.OnXxx()` from event handler
- [ ] Add `OnXxx()` to plugin calling `_commandCoordinator.NotifyXxx()`
- [ ] Implement interface in relevant commands
- [ ] Write tests for registry dispatch
