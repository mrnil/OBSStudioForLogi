# Architecture Overview

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Loupedeck Hardware                        │
│                  (Physical Buttons/Displays)                 │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                  Logi Plugin Service                         │
│              (Plugin Host Environment)                       │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│              OBSStudioForLogiPlugin                          │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Actions Layer (Commands)                            │   │
│  │  - Recording/Streaming/Virtual Camera Controls       │   │
│  │  - Scene/Profile/Source Management                   │   │
│  │  - Display Commands (Status Indicators)              │   │
│  └────────────────────┬─────────────────────────────────┘   │
│                       │                                      │
│  ┌────────────────────▼─────────────────────────────────┐   │
│  │  Plugin Coordinator (OBSStudioForLogiPlugin.cs)      │   │
│  │  - Lifecycle Management                              │   │
│  │  - Event Routing                                     │   │
│  │  - Command Coordination                              │   │
│  └────────────────────┬─────────────────────────────────┘   │
│                       │                                      │
│  ┌────────────────────▼─────────────────────────────────┐   │
│  │  Services Layer                                      │   │
│  │  - OBSWebSocketManager (Connection & Events)         │   │
│  │  - OBSActionExecutor (Command Execution)             │   │
│  │  - OBSConfigReader (Configuration Discovery)         │   │
│  │  - OBSLifecycleManager (Port Monitoring)             │   │
│  └────────────────────┬─────────────────────────────────┘   │
│                       │                                      │
│  ┌────────────────────▼─────────────────────────────────┐   │
│  │  Adapter Layer                                       │   │
│  │  - OBSWebsocketAdapter (IOBSWebsocket)               │   │
│  └────────────────────┬─────────────────────────────────┘   │
└───────────────────────┼──────────────────────────────────────┘
                        │
                        ▼ WebSocket (ws://127.0.0.1:4455)
┌─────────────────────────────────────────────────────────────┐
│                    OBS Studio                                │
│              (obs-websocket v5.0+)                           │
└─────────────────────────────────────────────────────────────┘
```

## Key Design Patterns

### 1. Singleton Pattern
Commands use static `Instance` property for global access:
```csharp
public static ProfileSelectCommand Instance { get; private set; }
```
Enables plugin to notify commands without maintaining explicit references.

### 2. Event-Driven Architecture
- **OBS Events** → OBSWebSocketManager → Plugin → Commands → UI Update
- **User Actions** → Command → Plugin → OBSActionExecutor → WebSocket Request
- Decouples components and enables real-time synchronization

### 3. Adapter Pattern
`OBSWebsocketAdapter` wraps `obs-websocket-dotnet` library:
- Provides testable interface (`IOBSWebsocket`)
- Isolates third-party dependency
- Enables mocking in unit tests

### 4. Dependency Injection
Services accept interfaces for testability:
```csharp
public OBSActionExecutor(IOBSWebsocket obs, IPluginLog log)
```

### 5. Command Pattern
Loupedeck commands implement `RunCommand(String actionParameter)`:
- Encapsulates user actions
- Provides consistent execution interface
- Supports dynamic parameters

## Connection Management

### Reconnection Strategy
```
Connection Lost
    ↓
Timer Started (1s delay + jitter)
    ↓
Reconnect Attempt
    ↓
Success? ──Yes──→ Reset Timer, Load State
    ↓
    No
    ↓
Increase Delay (2s, 4s, 8s, 15s, 30s max)
    ↓
Apply Jitter (0.85-1.15x)
    ↓
Schedule Next Attempt
    ↓
(Loop until connected or disposed)
```

### Jitter Calculation
- Base delays: 1s, 2s, 4s, 8s, 15s, 30s
- Jitter range: 0.85x to 1.15x (30% spread)
- Prevents thundering herd problem
- Example: 4s base → 3.4s to 4.6s actual

## Data Flow Examples

### User Switches Scene
```
1. User presses scene button on hardware
2. Loupedeck → ScenesDynamicFolder.RunCommand("Scene Name")
3. ScenesDynamicFolder → Plugin.SwitchScene("Scene Name")
4. Plugin → OBSActionExecutor.SetCurrentScene("Scene Name")
5. OBSActionExecutor → OBSWebsocketAdapter.SetCurrentProgramScene()
6. Adapter → obs-websocket-dotnet → WebSocket → OBS Studio
```

### OBS Scene Changes
```
1. OBS Studio changes scene (user or automation)
2. WebSocket event → obs-websocket-dotnet
3. Event → OBSWebsocketAdapter → OBSWebSocketManager
4. Manager → Plugin.OnCurrentSceneChanged("New Scene")
5. Plugin → ScenesDynamicFolder.OnCurrentSceneChanged()
6. Plugin → CurrentSceneDisplay.UpdateScene()
7. Commands update UI → Loupedeck hardware displays change
```

## State Management

### Connection States
- **Disconnected**: Initial state, no WebSocket connection
- **Connecting**: Connection attempt in progress
- **Connected**: Active WebSocket connection, commands enabled
- **Reconnecting**: Connection lost, attempting to reconnect

### Command States
- **Disabled**: OBS not connected, commands grayed out
- **Enabled**: OBS connected, commands active
- **Multi-state**: Selected/unselected for profile/scene buttons

### Output States (Recording/Streaming/Virtual Camera)
- **Stopped**: Output inactive
- **Starting**: Transition to active
- **Started**: Output active
- **Stopping**: Transition to inactive
- **Paused**: Recording paused (recording only)
- **Resumed**: Recording resumed from pause

## Testing Strategy

### Unit Tests (80 tests)
- **Services**: Mock IOBSWebsocket, test business logic
- **Commands**: Test state management and UI updates
- **Connection**: Test reconnection logic, backoff, jitter
- **Disposal**: Test thread safety and cleanup

### Test Doubles
- `Mock<IOBSWebsocket>`: Mock OBS WebSocket operations
- `Mock<IPluginLog>`: Mock logging for verification
- `Thread.Sleep(100)`: Wait for `Task.Run` completion

## Security Considerations

### Localhost-Only Connections
- WebSocket connections restricted to 127.0.0.1 or ::1
- Prevents remote OBS control
- Validated in `OBSConnectionSettings`

### Password Handling
- Read from OBS config only when needed
- Never logged or stored in plugin
- Passed directly to WebSocket connection

### Input Validation
- All string inputs validated with `String.IsNullOrEmpty()`
- Connection state checked before operations
- Guard clauses prevent invalid operations

## Performance Characteristics

### Async Operations
- All OBS operations wrapped in `Task.Run`
- Prevents UI thread blocking
- Fire-and-forget pattern for user actions

### Event Handling
- Events processed asynchronously
- UI updates batched where possible
- Minimal processing in event handlers

### Resource Management
- Comprehensive disposal pattern
- Thread-safe cleanup with lock
- Timer and event handler cleanup
- No memory leaks in long-running operation
