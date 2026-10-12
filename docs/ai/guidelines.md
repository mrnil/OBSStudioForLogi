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

### OBS Calls Go Through the Executor's Helpers

`OBSActionExecutor` never repeats the connection guard, background runner and error handling by hand (assessment #32). Every OBS call goes through one of three private helpers:

| Helper | Use for | Disconnected | OBS error |
|--------|---------|--------------|-----------|
| `RunMutation(action, call, refusal)` | Every OBS write | Logs `Cannot <action> - not connected`, skips the call | Logs `Failed to <action>: <message>` |
| `Query<T>(what, fallback, call)` | Reads whose callers want a safe value | Logs `Cannot get <what> - not connected` (pass `warnIfDisconnected: false` for polled reads), returns `fallback` | Logs `Failed to get <what>: <message>`, returns `fallback` |
| `TryQuery<T>(what, call, out value)` | Fetches for the render caches (`AudioStateCache`, `KeyedStateCache`, `CachedValue`) | Returns `false` silently | Logs the error, returns `false` |

Name the entity in `action` or `what` (`$"set profile '{profileName}'"`) so both messages say what failed.

### Mutations — `RunMutation`

`RunMutation` runs the call on the injected `_runInBackground` runner (`Task.Run` in production), never `Task.Run` directly, so a slow OBS never stalls a button press and tests can run the work inline. Input validation and state guards go in the optional `refusal`, which returns the reason the call can't run, or `null` to go ahead. It runs after the connection check and is logged as `Cannot <action> - <reason>`:

```csharp
public void StartRecording()
{
    this.RunMutation("start recording", () =>
    {
        this._log.Info("Starting recording");
        this._obs.StartRecord();
    },
    () => this.IsRecording ? "already recording" : null);
}

public void ToggleInputMute(String inputName)
{
    this.RunMutation($"toggle input mute for '{inputName}'", () =>
    {
        this._log.Info($"Toggling input mute for '{inputName}'");
        this._obs.ToggleInputMute(inputName);
    },
    () => String.IsNullOrEmpty(inputName) ? "input name is empty" : null);
}
```

A check that should log at `Info` rather than `Warning`, such as "profile already active", goes at the top of the call instead of in `refusal`.

### Queries — `Query<T>` and `TryQuery<T>`

Queries return a safe value on disconnection or error and never throw:

```csharp
public Boolean GetInputMute(String inputName)
{
    return this.Query($"input mute state for '{inputName}'", false, () => this._obs.GetInputMute(inputName));
}
```

Safe fallbacks by type:

- `Boolean` → `false`
- `String` → `String.Empty` or a domain-specific default (e.g. `MediaInputStates.None`)
- `String[]` → `new String[0]`
- Model objects → their `Empty` instance (`OBSStats.Empty`, `OBSStreamStats.Empty`)

A value a render cache fetches uses `TryQuery` in a `TryGet*` method instead, so a timeout is retried later rather than cached as the fallback (`TryGetSceneItemEnabled`, `TryGetMediaInputStatus`, `TryGetInputAudioState`). Don't add a plain `Get*` alongside it unless something outside the render path needs one.

### Delayed Callback Pattern (Toggle Operations)

When toggling state, add a delay before notifying UI to avoid race conditions. The executor doesn't know about the plugin: it reports through a callback it was given in its constructor, which `OBSWebSocketManager` passes on to its `IOBSEventListener` (assessment #20):

```csharp
this._obs.SetSceneItemEnabled(sceneName, sourceName, !currentState);
await Task.Delay(OBSTimings.StateUpdateDelay);
this._sourceVisibilityToggled?.Invoke(sceneName, sourceName);
```

### No Upward Calls From the Services Layer

Nothing in `src/Services` calls `OBSStudioForLogiPlugin.Instance` or a command's static `Instance` (assessment #20). `OBSWebSocketManager` reports to its `IOBSEventListener`, the executor to its callback, and `StatsService` reads stats through the providers it was constructed with. The plugin class forwards to `CommandCoordinator`, never to a command directly. This is what lets `OBSWebSocketManagerEventHandlingTests` drive the manager through a mocked `IOBSWebsocket` and `IOBSEventListener`; an upward call would need the real plugin.

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
        this.DisplayName = "Scenes";
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

Clear state and rebuild button lists on disconnect. Do **not** query OBS for lists in `OnConnected` — `OBSWebSocketManager` loads the input, scene, profile and scene collection lists once per connection and pushes them through the list-aware interfaces (`IInputsListAwareCommand`, `IScenesListAwareCommand`, `IProfilesListAwareCommand`, `ISceneCollectionsListAwareCommand`). Each command querying for itself multiplied every reconnect into dozens of blocking requests (see `assessment.md` #16):

```csharp
public void OnConnected()
{
    this.IsEnabled = true;
}

public void OnInputsChanged(String[] inputs)
{
    this._audioInputs = inputs ?? new String[0];
    this.ButtonActionNamesChanged();
}

public void OnDisconnected()
{
    this._audioInputs = new String[0];
    this.ButtonActionNamesChanged();
}
```

### Rendering Must Not Block on OBS

`GetCommandImage`, `GetAdjustmentValue` and similar are called on the SDK's render threads, and every OBS request blocks until OBS answers (up to the websocket timeout). Render only from state the plugin already holds. Audio mute/volume/monitor type come from `AudioStateCache` via the plugin's `GetInputMute`/`GetInputVolume`/`GetInputAudioMonitorType`, and source visibility and media state come from `OBSWebSocketManager.SourceVisibility`/`MediaState` (both `KeyedStateCache`) via `GetSourceVisibility`/`GetMediaInputStatus`; none of these wait on OBS. A button press that decides what to do from OBS state (like the media single tap) should read it live in the executor's background runner, not from the render cache. For new render state, use a `KeyedStateCache` kept current by the matching OBS event rather than a query.

The same goes for OBS event handlers in `OBSWebSocketManager`: they run on the websocket's event thread, so a handler that makes OBS requests holds up every event behind it. Move that work to a background runner, and drop results a newer event has superseded, as `SceneSourcesLoader` does for the source lists after a scene change.

State keyed by an OBS scene or input name goes stale when the user renames it in OBS. When you add a new cache or name-keyed store, move its entries in `OBSWebSocketManager.ApplyInputRename` or `ApplySceneRename` (assessment #21).

### Load OBS Data Only While It Is Shown

Don't fetch or poll OBS for data that only one folder or button displays unless that folder or button is on screen (assessment #25). A `PluginDynamicFolder` registers as a viewer in `Activate()` (first instance opened) and unregisters in `Deactivate()` (last instance closed): see `SourcesDynamicFolder` with `SceneSourcesLoader`, and the stats folders with `StatsService`. A plain command gets no such hooks, so infer visibility with an `ActivityLease` renewed from `GetCommandImage` and a timer that invalidates the image about once a second and expires the lease: see `StatsDisplay` and `AudioMeterCommand`. A folder whose list loads on open shows `LoadingTiles` until the first list arrives. Set the tiles before starting the load, since the load can finish before the call that starts it returns.

Lists that every command needs (scenes, inputs, profiles, scene collections) stay in the once-per-connection load; the Options+ action picker needs them even when nothing is on the device.

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
    protected override String GetEnabledIcon() => "RecordingOn.svg";
    protected override String GetDisabledIcon() => "RecordingOnDisabled.svg";
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

### ButtonImageHelper and ButtonTextRenderer

Build every button image with these two static classes in `src/Helpers/`. They are the whole API: there are no `StateIcon`, `StateText`, `TextWithIcon` or `StateTextWithIcon` helpers, so pick state-dependent icons and colours with a conditional at the call site.

```csharp
// Icon (embedded SVG)
return ButtonImageHelper.Icon("Screenshot.svg");

// Icon that depends on state
return ButtonImageHelper.Icon(isActive ? "RecordingOn.svg" : "RecordingOff.svg");

// Icon on a coloured background (e.g. the connection-status colours)
return ButtonImageHelper.IconWithBackground("Reconnect.svg", imageSize, backgroundColor);

// Text only; background and text colour default to black and white
return ButtonTextRenderer.RenderText("Connected", imageSize, BitmapColor.Black, BitmapColor.Green);

// Text whose colour depends on state
return ButtonTextRenderer.RenderText(text, imageSize, BitmapColor.Black, !isMuted ? BitmapColor.Green : BitmapColor.Red);

// Text with an optional white border, e.g. to mark the selected item
return ButtonTextRenderer.RenderTextWithBorder(text, imageSize, textColor, showBorder: isSelected);

// Text drawn over an icon on a black background
return ButtonTextRenderer.RenderTextWithIcon(text, imageSize, "SourceVisibilityOn.svg", BitmapColor.Green);
```

`ButtonTextRenderer` picks the font size from the text length and the button size. `RenderTextWithBorder` also has an overload that takes a width and height in pixels instead of a `PluginImageSize`. `ToggleCommandBase` and `StartStopCommandBase` already render their icons through `ButtonImageHelper.Icon`, so subclasses only name the icon files.

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
3. Call `this._commandCoordinator.NotifyXxx()` from `OBSStudioForLogiPlugin` (for an OBS event, from its `IOBSEventListener` method — see New Notification Event below)
4. Implement the interface in relevant commands, and register them (`OBSStudioForLogiPlugin.Instance?.RegisterCommand(this)` in the constructor)

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
        this._executor = new OBSActionExecutor(this._mockObs.Object, this._mockLog.Object, action => action());
    }
}
```

### Async Fire-and-Forget Testing

Don't sleep and hope the background work has finished — that raced the thread pool under full-suite load (assessment #15). Classes that fire work in the background take an `Action<Action>` runner in their constructor (`OBSActionExecutor`, `AudioStateCache`, `KeyedStateCache`, `SceneSourcesLoader`, `OBSFacade`). Pass `action => action()` to run it inline, then assert straight away:

```csharp
[Fact]
public void SetCurrentProfile_WhenConnected_CallsObs()
{
    this._mockObs.Setup(x => x.IsConnected).Returns(true);

    this._executor.SetCurrentProfile("test");

    this._mockObs.Verify(x => x.SetCurrentProfile("test"), Times.Once);
}
```

To assert on what happens before the work runs, queue it instead (`action => background.Enqueue(action)`) and dequeue when ready. Give any new service that starts background work the same constructor parameter. Only code driven by a real timer (e.g. `DoubleTapHelper`) should wait in tests, and then with a bounded poll, not a fixed sleep.

### Static State in Tests

`AudioSelectionState` is static, and xUnit runs test classes in parallel. Put any test class that selects, deselects or renames the audio selection (directly, or through a folder's `OnDisconnected`/`OnInputsChanged`) in `[Collection(AudioSelectionStateCollection.Name)]` so those classes run one at a time.

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
OBSTimings.TestAsyncDelay        // 500ms — test wait for real-timer code only (e.g. DoubleTapHelper)
```

Never hardcode timing values — always use `OBSTimings` constants.

---

## Adding a New Feature — Checklist

### New OBS API Method

1. Add method signature to `IOBSWebsocket`
2. Implement in `OBSWebsocketAdapter` (thin pass-through)
3. Add business logic to `OBSActionExecutor` through `RunMutation`, `Query<T>` or `TryQuery<T>` (see "OBS Calls Go Through the Executor's Helpers")
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
9. Add icon SVG to `src/Resources/icons/` following `docs/ai/icon-style.md`, and register it in `.csproj` as `EmbeddedResource`

### New Notification Event

1. Add interface to `IObsCommand.cs`
2. Add `NotifyXxx()` to `CommandCoordinator` via `this.NotifyEach<TInterface>(...)` — `CommandRegistry` needs no changes
3. Add the library event to `IOBSWebsocket` and forward it in `OBSWebsocketAdapter` (`add => this._obs.Xxx += value;`)
4. Subscribe to it in `OBSWebSocketManager` (and unsubscribe in `Dispose`); the handler updates any state, then calls `this._listener?.OnXxx()`
5. Add `OnXxx()` to `IOBSEventListener` and implement it in `OBSStudioForLogiPlugin`, calling `this._commandCoordinator.NotifyXxx()`
6. Implement interface in relevant commands
7. Test the handler in `OBSWebSocketManagerEventHandlingTests`: raise the event on the mocked `IOBSWebsocket` and verify the listener call

---

## Known Issues to Be Aware Of

### Fixed in v1.6.0 (kept here for history — do not reintroduce)

- **CommandRegistry Bypass** — `OBSWebSocketManager.OnConnected`/`OnDisconnected` used to contain hardcoded singleton calls bypassing the `CommandRegistry`. Now `OBSStudioForLogiPlugin.OnOBSConnected`/`OnOBSDisconnected` call `CommandCoordinator.NotifyConnected()`/`NotifyDisconnected()`, which dispatch through the registry. New self-registering commands correctly receive `OnConnected`/`OnDisconnected` with no extra wiring — do not add hardcoded singleton calls back into `OBSWebSocketManager` (since #20 it can't reach the plugin at all; it reports through `IOBSEventListener`).
- **Scene Change Bypass** — `OnCurrentSceneChanged` used to call `SourcesDynamicFolder.Instance`/`SceneAudioSourcesDynamicFolder.Instance` directly. Now routes through `ISceneSourcesAwareCommand` via the registry.
- **DoubleTapHelper Thread Safety** — `_tapStates` access is now wrapped in `lock(_tapStates)`; `CancellationTokenSource` objects are disposed on every path (single-tap `finally`, double-tap, and `Reset()`).

See `assessment.md` items #1, #2, #4 for the original write-ups and `test-coverage.md` for the tests added alongside each fix.

### Still Open

See the Summary Table in `assessment.md` for the current priority order.
