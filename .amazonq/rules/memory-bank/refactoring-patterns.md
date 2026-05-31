# Refactoring Patterns

## God Class Refactoring (Completed)

### Problem

The `OBSStudioForLogiPlugin` class was a God Class with ~400 lines handling 8 different responsibilities, violating the Single Responsibility Principle.

### Solution

Split into 4 focused classes, each with a single responsibility:

#### 1. ConnectionManager (52 lines)

**Responsibility:** OBS Connection Lifecycle

Handles:

- Reading OBS configuration
- Waiting for port availability
- Connecting to WebSocket
- Disconnecting and cleanup

#### 2. CommandCoordinator (93 lines)

**Responsibility:** Command Registration & Event Notification

Handles:

- Command registration
- Event notification to commands
- Delegation to CommandRegistry

#### 3. OBSFacade (227 lines)

**Responsibility:** OBS State & Actions Interface

Handles:

- State queries (7 properties)
- Query methods (10 methods)
- Action methods (20+ methods)
- Connection validation

#### 4. OBSStudioForLogiPlugin (289 lines)

**Responsibility:** Orchestration & Event Routing

Handles:

- Plugin lifecycle orchestration
- Event routing to coordinator
- Public API delegation to facade
- Resource management

### Results

- Main class reduced from ~400 to 289 lines (-28%)
- Responsibilities reduced from 8 to 2 (-75%)
- All 140 tests passing
- 0 warnings, clean build

### Benefits

- **Single Responsibility Principle** - Each class has one clear purpose
- **Easier to Test** - Mock only what you need
- **Easier to Understand** - Focused, cohesive classes
- **Easier to Change** - Changes isolated to specific classes
- **Better Reusability** - Components can be used independently

## Command Registry Pattern (Completed)

### Problem

Manual command registration in main plugin class:

- 15+ manual notification calls
- Easy to forget new commands
- No compile-time safety
- Scattered notification logic

### Solution

Interface-based self-registration with CommandRegistry:

```csharp
// Interface hierarchy
public interface IObsCommand
{
    void OnConnected();
    void OnDisconnected();
}

public interface ISceneAwareCommand : IObsCommand
{
    void OnSceneChanged(String sceneName);
}

// Commands self-register
public class ScenesDynamicFolder : PluginDynamicFolder, IObsCommand, ISceneAwareCommand
{
    public ScenesDynamicFolder()
    {
        Instance = this;
        OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
    }
    
    public void OnConnected() { }
    public void OnDisconnected() { }
    public void OnSceneChanged(String sceneName) { }
}

// Registry notifies via interfaces
public class CommandRegistry
{
    public void NotifySceneChanged(String sceneName)
    {
        foreach (var command in this._commands.OfType<ISceneAwareCommand>())
        {
            command.OnSceneChanged(sceneName);
        }
    }
}
```

### Results

- Eliminated 15+ manual notification calls
- Commands self-register automatically
- Compile-time safety via interfaces
- Centralized notification logic

### Benefits

- **Self-Documenting** - Commands register themselves
- **Compile-Time Safety** - Interfaces enforce implementation
- **No Maintenance** - Adding new commands requires no changes to main plugin
- **Centralized** - All notification logic in CommandRegistry
- **Testable** - Registry can be mocked independently

## Facade Pattern (Completed)

### Problem

Direct access to OBSWebSocketManager from main plugin:

- Repeated null checking
- Repeated connection validation
- Complex delegation chains
- Scattered error handling

### Solution

OBSFacade provides simplified interface:

```csharp
public class OBSFacade
{
    private readonly OBSWebSocketManager _obsManager;
    
    // Simple interface
    public Boolean IsRecording => this._obsManager?.IsRecording ?? false;
    
    public void ToggleRecording()
    {
        this._obsManager?.Actions.ToggleRecording();
    }
    
    public void SwitchScene(String sceneName)
    {
        if (!this._obsManager.IsConnected)
        {
            PluginLog.Warning($"Cannot switch to scene '{sceneName}' - not connected");
            return;
        }
        this._obsManager.Actions.SetCurrentScene(sceneName);
    }
}
```

### Results

- Single point of access to OBS functionality
- Consistent error handling
- Simplified main plugin class
- Better testability

### Benefits

- **Simplified Interface** - Hide complexity
- **Consistent Validation** - Connection checks in one place
- **Better Error Handling** - Centralized logging
- **Easier Testing** - Mock facade instead of manager

## Refactoring Principles Applied

### 1. Single Responsibility Principle (SRP)

Each class should have one reason to change.

**Applied to:**

- ConnectionManager - Connection lifecycle
- CommandCoordinator - Command coordination
- OBSFacade - OBS interface
- Main plugin - Orchestration

### 2. Open/Closed Principle (OCP)

Open for extension, closed for modification.

**Applied to:**

- Command Registry - Add new commands without modifying registry
- Interface hierarchy - Add new event types without changing base

### 3. Dependency Inversion Principle (DIP)

Depend on abstractions, not concretions.

**Applied to:**

- IObsCommand interfaces - Commands depend on interface
- IOBSWebsocket - Services depend on interface

### 4. Don't Repeat Yourself (DRY)

Eliminate duplication.

**Applied to:**

- Removed TryDirectConnection() duplication
- Centralized connection logic in ConnectionManager
- Centralized notification logic in CommandRegistry

### 5. Separation of Concerns

Different concerns should be in different classes.

**Applied to:**

- Connection concerns → ConnectionManager
- Command concerns → CommandCoordinator
- OBS concerns → OBSFacade
- Orchestration concerns → Main plugin

## When to Refactor

### Warning Signs

- Class exceeds 300-400 lines
- Class has 5+ distinct responsibilities
- Hard to write tests without mocking everything
- Developers avoid changing it (fear of breaking things)
- Pull requests touching it are large and risky
- New features always require editing this class

### Refactoring Process

1. **Identify responsibilities** - List what the class does
2. **Group related concerns** - Connection, Commands, Actions, etc.
3. **Extract focused classes** - One responsibility per class
4. **Delegate from main class** - Thin orchestration layer
5. **Test thoroughly** - Ensure no regressions

### Anti-Patterns to Avoid

- Creating too many tiny classes (over-engineering)
- Splitting arbitrarily (not by responsibility)
- Leaving main class still doing too much
- Not updating tests

### Best Practices

- Split by responsibility (SRP)
- Keep classes focused and cohesive
- Use dependency injection
- Test each class independently
- Document the architecture
