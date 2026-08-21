# Development Guidelines

## Code Style (EditorConfig Enforced)

### Type Names — BCL Keywords Forbidden

Always use BCL type names, never C# keywords:

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

### `this.` Qualification — Always Required

Every field, method, property, and event access must be qualified with `this.`:

```csharp
// CORRECT
this._obs.IsConnected
this._log.Warning("message")
this.CommandImageChanged(actionParameter)

// WRONG
_obs.IsConnected
_log.Warning("message")
CommandImageChanged(actionParameter)
```

### Private Fields — Underscore Prefix

```csharp
private readonly IOBSWebsocket _obs;
private readonly IPluginLog _log;
private String _currentScene = String.Empty;
private Boolean _disposed = false;
```

### No `var` — Explicit Types Always

```csharp
// CORRECT
String[] profiles = this._obs.GetProfileList();
Boolean isConnected = this._obs.IsConnected;

// WRONG
var profiles = this._obs.GetProfileList();
var isConnected = this._obs.IsConnected;
```

### Braces Always Required

```csharp
// CORRECT
if (String.IsNullOrEmpty(sceneName))
{
    return;
}

// WRONG
if (String.IsNullOrEmpty(sceneName))
    return;
```

### `using` Directives Inside Namespace

```csharp
namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Linq;
    // ...
}
```

### Allman Brace Style

Opening brace always on its own line:

```csharp
public void MyMethod()
{
    if (condition)
    {
        // ...
    }
}
```

---

## Service Layer Patterns

### Connection Guard Pattern (Universal)

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

### Async Fire-and-Forget Pattern (All OBS Mutations)

All OBS write operations use `Task.Run` to avoid blocking the UI thread:

```csharp
public void SetInputVolume(String inputName, Single volumeMul)
{
    Task.Run(() =>
    {
        if (!this._obs.IsConnected) { ... return; }
        try { this._obs.SetInputVolume(inputName, volumeMul); }
        catch (Exception ex) { this._log.Error($"...{ex.Message}"); }
    });
}
```

### Safe Default Return Pattern (All OBS Queries)

Query methods return safe defaults on disconnection or error — never throw:

```csharp
public Boolean GetInputMute(String inputName)
{
    if (!this._obs.IsConnected)
        return false;          // safe default

    try
    {
        return this._obs.GetInputMute(inputName);
    }
    catch (Exception ex)
    {
        this._log.Error($"Failed to get input mute state for '{inputName}': {ex.Message}");
        return false;          // safe default on error
    }
}
```

Safe defaults by type:

- `Boolean` → `false`
- `String` → `String.Empty` or domain-specific default (e.g. `"OBS_MONITORING_TYPE_NONE"`)
- `String[]` → `new String[0]`
- `Single` (volume) → `1.0f`
- Model objects → `null`

### Input Validation Before OBS Calls

Validate string parameters before proceeding:

```csharp
if (String.IsNullOrEmpty(sceneName) || String.IsNullOrEmpty(sourceName))
{
    this._log.Warning("Cannot toggle source visibility - scene or source name is empty");
    return;
}
```

### State Guard Pattern (Prevent Invalid Operations)

Check current state before executing operations that require a specific state:

```csharp
if (this.IsRecording)
{
    this._log.Warning("Cannot start recording - already recording");
    return;
}

if (this.IsRecordingChanging)
{
    this._log.Warning("Cannot toggle recording - state change in progress");
    return;
}
```

### Delayed Callback Pattern (Toggle Operations)

When toggling state, add a delay before notifying UI to avoid race conditions:

```csharp
this._obs.SetSceneItemEnabled(sceneName, sourceName, !currentState);
await Task.Delay(OBSTimings.StateUpdateDelay);
OBSStudioForLogiPlugin.Instance?.OnSourceVisibilityChanged(sceneName, sourceName);
```

---

## Command (Action) Layer Patterns

### Self-Registration in Constructor

Every command registers itself with the plugin and sets its static Instance:

```csharp
public class ScenesDynamicFolder : PluginDynamicFolder, IObsCommand, ISceneAwareCommand
{
    public static ScenesDynamicFolder Instance { get; private set; }

    public ScenesDynamicFolder()
    {
        Instance = this;
        OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
        this.DisplayName = "OBS Scenes";
        this.GroupName = "7. Scenes";
        this.Description = "Folder of scenes from the current collection";
    }
}
```

### Null Guard in RunCommand

Always guard against null/empty actionParameter:

```csharp
public override void RunCommand(String actionParameter)
{
    if (String.IsNullOrEmpty(actionParameter))
        return;

    OBSStudioForLogiPlugin.Instance?.SwitchScene(actionParameter);
}
```

### GetCommandImage — Query Current State

Always query live state from the plugin; never cache state in the command:

```csharp
protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
{
    Boolean isRecording = OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;
    return ButtonImageHelper.Icon(isRecording ? "RecordingOn.svg" : "RecordingOff.svg");
}
```

### CommandImageChanged — Targeted Refresh

Refresh only the specific button that changed, not the entire folder:

```csharp
// CORRECT — targeted refresh
public void OnInputMuteChanged(String inputName)
{
    if (this.AudioInputs.Contains(inputName))
        this.CommandImageChanged(inputName);
}

// WRONG — rebuilds all buttons
public void OnInputMuteChanged(String inputName)
{
    this.ButtonActionNamesChanged(); // ❌ too expensive
}
```

### OnConnected / OnDisconnected Pattern

Clear state and rebuild button lists on disconnect; reload on connect:

```csharp
public void OnConnected()
{
    this._mediaInputs = OBSStudioForLogiPlugin.Instance?.GetMediaInputList() ?? new String[0];
    this.ButtonActionNamesChanged();
}

public void OnDisconnected()
{
    this._mediaInputs = new String[0];
    this.ButtonActionNamesChanged();
}
```

### ToggleCommandBase — For Toggle Commands

Extend `ToggleCommandBase` for any on/off toggle:

```csharp
public class RecordingToggleCommand : ToggleCommandBase, IObsCommand
{
    public RecordingToggleCommand()
        : base("Recording Toggle", "Toggle recording on/off", "3. Recording")
    {
        Instance = this;
        OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
    }

    protected override void ExecuteToggle() => OBSStudioForLogiPlugin.Instance?.ToggleRecording();
    protected override Boolean GetState() => OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;
    protected override String GetActiveIcon() => "RecordingOn.svg";
    protected override String GetInactiveIcon() => "RecordingOff.svg";

    public void OnConnected() => this.ActionImageChanged();
    public void OnDisconnected() => this.ActionImageChanged();
}
```

### StartStopCommandBase — For Start/Stop Pairs

Extend `StartStopCommandBase` for start and stop command pairs:

```csharp
public class RecordingStartCommand : StartStopCommandBase, IObsCommand
{
    public RecordingStartCommand()
        : base("Recording Start", "Start recording", "3. Recording", isStartCommand: true)
    {
        // ...
    }

    protected override void ExecuteStart() => OBSStudioForLogiPlugin.Instance?.StartRecording();
    protected override void ExecuteStop() => OBSStudioForLogiPlugin.Instance?.StopRecording();
    protected override Boolean GetState() => OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;
    protected override String GetEnabledIcon() => "RecordingStart.svg";
    protected override String GetDisabledIcon() => "RecordingStartDisabled.svg";
}
```

### ActionEditorCommand — For User-Defined Actions

Use constants for control names; validate required parameters:

```csharp
public class MyAdjustableCommand : ActionEditorCommand, IObsCommand
{
    private const String SourceNameControlName = "SourceName";

    public MyAdjustableCommand()
    {
        Instance = this;
        OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
        this.Name = "MyAction";
        this.DisplayName = "My Action";
        this.GroupName = "99. User Defined Actions";
        this.Description = "Description of what this does";
        this.ActionEditor.AddControlEx(new ActionEditorTextbox(SourceNameControlName, "Source Name (required)"));
    }

    protected override Boolean OnLoad() => true;

    protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
    {
        if (!actionParameters.TryGetString(SourceNameControlName, out var sourceName) || String.IsNullOrEmpty(sourceName))
            return false;

        Task.Run(() => OBSStudioForLogiPlugin.Instance?.DoSomething(sourceName));
        return true;
    }

    public void OnConnected() { }
    public void OnDisconnected() { }
}
```

---

## Image Rendering Patterns

### ButtonImageHelper (icons) and ButtonTextRenderer (text) — Preferred APIs

Two static helpers, not one — see `docs/ai/image-rendering-simplified.md` for the full reference. `ButtonImageHelper` is icon-only; `ButtonTextRenderer` is for anything showing text. Neither has a combined "state" method — branch inline at the call site to pick the icon/color/border for the current state:

```csharp
// Static icon
return ButtonImageHelper.Icon("Screenshot.svg");

// State-based icon — branch inline, there's no StateIcon() method
return ButtonImageHelper.Icon(isActive ? "RecordingOn.svg" : "RecordingOff.svg");

// Icon over a solid background color
return ButtonImageHelper.IconWithBackground("Reconnect.svg", imageSize, backgroundColor);

// Text only
return ButtonTextRenderer.RenderText("Connected", imageSize, BitmapColor.Black, BitmapColor.Green);

// Text with a border to indicate selection/state, alongside a state-based color
return ButtonTextRenderer.RenderTextWithBorder(text, imageSize, !isMuted ? BitmapColor.Green : BitmapColor.Red, isSelected);
```

### Icon Resource Names — Short Form

Pass only the filename; the helper resolves the full embedded resource path:

```csharp
// CORRECT
ButtonImageHelper.Icon("RecordingOn.svg")

// WRONG — don't specify full path
ButtonImageHelper.Icon("Loupedeck.OBSStudioForLogiPlugin.Icons.RecordingOn.svg")
```

### Volume Display — Always dB Format

Use `VolumeConverter.FormatDb()` for all volume displays:

```csharp
String volumeText = VolumeConverter.FormatDb(volumeMul); // e.g. "+6.0 dB", "0.0 dB", "-∞ dB"
```

### Colour Conventions

- Green (`BitmapColor.Green`) = active, unmuted, connected, healthy
- Red (`BitmapColor.Red`) = inactive, muted, error
- Yellow (`new BitmapColor(255, 200, 0)`) = warning, paused
- Orange = WebSocket disabled state
- Grey (`new BitmapColor(128, 128, 128)`) = stopped, idle, disconnected

---

## Notification Interface Pattern

### Implementing Notification Interfaces

Commands declare which events they care about by implementing the appropriate interfaces:

```csharp
public class AudioMixerDynamicFolder : PluginDynamicFolder,
    IObsCommand,
    IInputMuteAwareCommand,
    IInputVolumeAwareCommand,
    IInputMonitorAwareCommand,
    IInputsListAwareCommand
{
    public void OnConnected() { /* load inputs */ }
    public void OnDisconnected() { /* clear inputs */ }
    public void OnInputMuteChanged(String inputName) { this.CommandImageChanged(inputName); }
    public void OnInputVolumeChanged(String inputName) { this.CommandImageChanged(inputName); }
    public void OnInputMonitorTypeChanged(String inputName) { this.CommandImageChanged(inputName); }
    public void OnInputsChanged(String[] inputs) { /* rebuild list */ }
}
```

### Adding a New Notification Type

1. Add interface to `src/Services/IObsCommand.cs`
2. Add `NotifyXxx()` method to `CommandCoordinator` calling `this.NotifyEach<TInterface>(nameof(TInterface.OnXxx), c => c.OnXxx(...))` — `CommandRegistry` needs no changes, its generic `GetCommands<T>()` filters for any interface automatically
3. Add `OnXxx()` call to `OBSStudioForLogiPlugin`
4. Implement the interface in relevant commands

---

## Testing Patterns

### Test Class Structure

```csharp
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
}
```

### Async Fire-and-Forget Testing

Use `Thread.Sleep(OBSTimings.TestAsyncDelay)` (500ms) after triggering async operations:

```csharp
[Fact]
public void SetCurrentProfile_WhenConnected_CallsObs()
{
    this._mockObs.Setup(x => x.IsConnected).Returns(true);

    this._executor.SetCurrentProfile("test");

    System.Threading.Thread.Sleep(OBSTimings.TestAsyncDelay);
    this._mockObs.Verify(x => x.SetCurrentProfile("test"), Times.Once);
}
```

### Test Naming Convention

`MethodName_Condition_ExpectedBehaviour`:

```csharp
GetProfileList_WhenConnected_ReturnsProfiles()
GetProfileList_WhenNotConnected_ReturnsEmpty()
GetProfileList_WhenOBSThrows_LogsErrorAndReturnsEmpty()
ToggleRecording_WhenOBSThrows_LogsError()
```

### Error Path Testing — Always Test Three Paths

For every service method, test:

1. Happy path (connected, succeeds)
2. Disconnected path (returns safe default, no OBS call)
3. Exception path (logs error with context, returns safe default)

```csharp
[Fact]
public void GetInputMute_WhenOBSThrows_LogsErrorAndReturnsFalse()
{
    this._mockObs.Setup(x => x.IsConnected).Returns(true);
    this._mockObs.Setup(x => x.GetInputMute(It.IsAny<String>())).Throws(new Exception("OBS error"));

    var result = this._executor.GetInputMute("Microphone");

    Assert.False(result);
    this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("Microphone") && s.Contains("OBS error"))), Times.Once);
}
```

### Log Message Verification

Error log messages must contain both the entity name and the exception message:

```csharp
this._mockLog.Verify(x => x.Error(
    It.Is<String>(s => s.Contains("Microphone") && s.Contains("OBS error"))),
    Times.Once);
```

### TDD Scope

Tests should be written before or alongside implementation where practical; all new business logic and core functionality must have accompanying tests.

- **Business logic / core functionality**: 90%+ coverage
- **Services layer** (`OBSActionExecutor`, `CommandRegistry`, `OBSFacade`, etc.): 80%+ coverage required
- **Actions layer** (`src/Actions/`): Integration tests for constructor/singleton only; SDK-dependent rendering exempt
- **Helpers** (`VolumeConverter`, `OBSConfigReader`, etc.): Full coverage

---

## Logging Conventions

### Log Levels

- `Trace` — render/UI calls (`GetCommandImage`, `GetEncoderNames`) — very high frequency
- `Debug` — operational detail (folder updates, selection state, intermediate steps)
- `Info` — significant state changes (scene changed, profile changed, connected, disconnected)
- `Warning` — guard clause violations (not connected, invalid state, empty parameter)
- `Error` — caught exceptions with context

### Log Message Format

Include the entity name and action in every message:

```csharp
this._log.Info($"Setting current profile to '{profileName}'");
this._log.Warning($"Cannot set profile '{profileName}' - not connected");
this._log.Error($"Failed to set profile '{profileName}': {ex.Message}");
```

Never log passwords or sensitive data. Sanitize file paths in logs.

---

## Timing Constants

All timing values are centralised in `OBSTimings`:

```csharp
OBSTimings.StateUpdateDelay      // 100ms — wait after OBS API call before refreshing UI
OBSTimings.ProfileSwitchDelay    // delay between profile switch and next operation
OBSTimings.CollectionSwitchDelay // delay between collection switch and next operation
OBSTimings.TestAsyncDelay        // 500ms — standard wait in tests for Task.Run completion
OBSTimings.TestAsyncDelayExtended // 750ms — extended wait for slower operations
```

Never hardcode timing values — always use `OBSTimings` constants.

---

## Adding a New Feature — Checklist

### New OBS API Method

1. Add method signature to `IOBSWebsocket`
2. Implement in `OBSWebsocketAdapter` (thin pass-through)
3. Add business logic to `OBSActionExecutor` (connection guard + error handling)
4. Expose via `OBSFacade`
5. Expose via `OBSStudioForLogiPlugin` public method
6. Write tests for all three paths (connected, disconnected, exception)

### New Command

1. Create class in `src/Actions/`
2. Inherit from appropriate base (`ToggleCommandBase`, `StartStopCommandBase`, `PluginDynamicFolder`, `ActionEditorCommand`)
3. Implement `IObsCommand` and any relevant notification interfaces
4. Set `Instance = this` and call `RegisterCommand(this)` in constructor
5. Set `DisplayName`, `GroupName`, `Description`
6. Override `RunCommand` with null guard
7. Override `GetCommandImage` querying live state
8. Implement `OnConnected` / `OnDisconnected`
9. Add icon SVG to `src/Resources/icons/` and register in `.csproj` as `EmbeddedResource`

### New Notification Event

1. Add interface to `IObsCommand.cs`
2. Add `NotifyXxx()` to `CommandCoordinator` via `this.NotifyEach<TInterface>(...)` — `CommandRegistry` needs no changes
3. Subscribe to OBS event in `OBSWebSocketManager`
4. Call `OBSStudioForLogiPlugin.Instance?.OnXxx()` from event handler
5. Add `OnXxx()` to `OBSStudioForLogiPlugin` calling `_commandCoordinator.NotifyXxx()`
6. Implement interface in relevant commands

---

## Known Issues to Be Aware Of

### Fixed in v1.6.0 (kept here for history — do not reintroduce)

- **CommandRegistry Bypass** — `OBSWebSocketManager.OnConnected`/`OnDisconnected` used to contain hardcoded singleton calls bypassing the `CommandRegistry`. Now `OBSStudioForLogiPlugin.OnOBSConnected`/`OnOBSDisconnected` call `CommandCoordinator.NotifyConnected()`/`NotifyDisconnected()`, which dispatch through the registry. New self-registering commands correctly receive `OnConnected`/`OnDisconnected` with no extra wiring — do not add hardcoded singleton calls back into `OBSWebSocketManager`.
- **Scene Change Bypass** — `OnCurrentSceneChanged` used to call `SourcesDynamicFolder.Instance`/`SceneAudioSourcesDynamicFolder.Instance` directly. Now routes through `ISceneSourcesAwareCommand` via the registry.
- **DoubleTapHelper Thread Safety** — `_tapStates` access is now wrapped in `lock(_tapStates)`; `CancellationTokenSource` objects are disposed on every path (single-tap `finally`, double-tap, and `Reset()`).

See `assessment.md` items #1, #2, #4 for the original write-ups and `test-coverage.md` for the tests added alongside each fix.

### Still Open

See `assessment.md` for current priority list — as of v1.6.1 the top remaining item is #3 (scene/source/profile buttons show no text).
