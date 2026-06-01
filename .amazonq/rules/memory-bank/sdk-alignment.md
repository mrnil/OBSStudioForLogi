# Logitech Actions SDK Alignment Guidelines

## Overview

This document provides SDK-aligned guidance for developing OBSStudioForLogiPlugin based on official Logitech Actions SDK documentation.

## Display Mode Standards

### Rule: Choose One Display Mode Per Action

Every action must use **either** text-only OR icon-only display, never both.

#### Text-Only Actions

Use for dynamic data (status displays, metrics, real-time values):

```csharp
public class CurrentSceneDisplay : PluginDynamicCommand
{
    // Override ONLY GetCommandDisplayName
    protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
    {
        return $"Scene\r\n{this._currentScene}";
    }
    
    // Do NOT override GetCommandImage
}
```

#### Icon-Only Actions

Use for static controls (buttons, toggles, launchers):

```csharp
public class RecordingToggleCommand : PluginDynamicCommand
{
    public RecordingToggleCommand()
    {
        // MUST set IsWidget = true for icon-only
        this.IsWidget = true;
    }
    
    // Override ONLY GetCommandImage
    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        return ButtonImageHelper.StateIcon(isRecording, "RecordingOn.svg", "RecordingOff.svg");
    }
    
    // Do NOT override GetCommandDisplayName
}
```

#### Text-With-Icon Actions (Using BitmapBuilder)

If you need text overlaid on an icon, render it in `GetCommandImage()`:

```csharp
public class AudioMixerButton : PluginDynamicCommand
{
    public AudioMixerButton()
    {
        this.IsWidget = true; // Icon-only mode
    }
    
    protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        // Use ButtonImageHelper which handles text + icon rendering
        String text = $"{inputName}\n\n{volume}%";
        return ButtonImageHelper.StateTextWithIcon(text, imageSize, !isMuted, 
            "AudioMixerUnmuted.svg", "AudioMixerMuted.svg",
            BitmapColor.Green, BitmapColor.Red);
    }
}
```

### Migration Checklist

For existing commands that mix modes:

1. Determine primary purpose (data display vs control)
2. If data display: Remove `GetCommandImage()`, keep `GetCommandDisplayName()`
3. If control: Add `IsWidget = true`, remove `GetCommandDisplayName()`, keep `GetCommandImage()`
4. If text + icon needed: Use `BitmapBuilder` or `ButtonImageHelper` in `GetCommandImage()`

## Resource Organization

### Folder Structure

```
src/
├── Actions/              # Commands and adjustments
├── Services/             # Business logic (OK to keep)
├── Models/               # Data models (OK to keep)
├── Helpers/              # PluginLog, PluginResources
├── Resources/            # Embedded assets
│   ├── icons/            # SVG/PNG icons
│   ├── images/           # Larger images
│   └── data/             # JSON, XML, etc.
└── package/
    └── metadata/
        ├── Icon256x256.png
        └── LoupedeckPackage.yaml
```

### Resource Naming

Use short names for embedded resources:

```csharp
// Good - short name
return ButtonImageHelper.Icon("RecordingOn.svg");

// Avoid - full namespace path
return EmbeddedResources.ReadImage("Loupedeck.OBSStudioForLogiPlugin.Icons.RecordingOn.svg");
```

## Package Metadata Standards

### Required Fields

```yaml
pluginName: OBSStudioForLogiPlugin
displayName: "OBS Studio Plugin for Loupedeck/Logitech Devices"
version: 1.0.0
author: "Your Name"
supportedDevices:
    - LoupedeckCtFamily
minimumLoupedeckVersion: 6.0
license: MIT
```

### Recommended Fields

```yaml
# Add these for better discoverability
category: "Streaming"
keywords: ["obs", "streaming", "recording", "broadcast", "twitch", "youtube"]
homepageUrl: https://github.com/mrnil/OBSStudioForLogi
```

### Optional Fields

```yaml
# For marketplace submission
productId: "com.yourcompany.obsplugin"
description: "Control OBS Studio from your Loupedeck device"
supportUrl: https://github.com/mrnil/OBSStudioForLogi/issues
```

## Action Parameter Patterns

### Predefined Parameters

For actions with common configurations:

```csharp
public class ScenePresetCommand : PluginDynamicCommand
{
    public ScenePresetCommand()
        : base("Scene Preset", "Quick scene switches", "Scenes")
    {
        // Add predefined options
        this.AddParameter("gaming", "Gaming Scene", "Presets");
        this.AddParameter("chatting", "Chatting Scene", "Presets");
        this.AddParameter("brb", "Be Right Back", "Presets");
    }
    
    protected override void RunCommand(String actionParameter)
    {
        if (String.IsNullOrEmpty(actionParameter))
            return;
            
        // Map preset to actual scene name
        var sceneMap = new Dictionary<String, String>
        {
            ["gaming"] = "Gaming - Full Screen",
            ["chatting"] = "Chatting - Webcam",
            ["brb"] = "BRB - Overlay"
        };
        
        if (sceneMap.TryGetValue(actionParameter, out var sceneName))
        {
            OBSStudioForLogiPlugin.Instance?.SwitchScene(sceneName);
        }
    }
}
```

### Parameter Validation

Always validate and sanitize parameters:

```csharp
protected override void RunCommand(String actionParameter)
{
    // Validate
    if (String.IsNullOrEmpty(actionParameter))
    {
        PluginLog.Warning("No parameter provided");
        return;
    }
    
    // Sanitize
    var sanitized = actionParameter.Trim().ToUpperInvariant();
    
    // Use
    ProcessParameter(sanitized);
}
```

## Lifecycle Management

### Plugin Initialization

```csharp
public class OBSStudioForLogiPlugin : Plugin
{
    public override Boolean UsesApplicationApiOnly => true;
    public override Boolean HasNoApplication => false; // We detect OBS process
    
    public OBSStudioForLogiPlugin()
    {
        // Initialize helpers first
        PluginLog.Init(this.Log);
        PluginResources.Init(this.Assembly);
        
        // Then initialize services
        this._connectionManager = new ConnectionManager(...);
        this._commandCoordinator = new CommandCoordinator(...);
    }
    
    public override void Load()
    {
        // Subscribe to events
        this.ClientApplication.ApplicationStarted += this.OnApplicationStarted;
        
        // Start services
        this._connectionManager.ConnectAsync();
    }
    
    public override void Unload()
    {
        // Unsubscribe from events
        this.ClientApplication.ApplicationStarted -= this.OnApplicationStarted;
        
        // Stop services
        this._connectionManager.Disconnect();
        
        // Dispose resources
        this._connectionManager?.Dispose();
    }
}
```

### Command Lifecycle

```csharp
public class MyCommand : PluginDynamicCommand
{
    public MyCommand()
        : base("Command Name", "Description", "Group")
    {
        // Register with plugin
        OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
    }
    
    protected override Boolean OnLoad()
    {
        // Initialize command state
        this.IsEnabled = false;
        return true;
    }
    
    // Cleanup happens automatically via plugin Unload
}
```

## Threading and Async Patterns

### Fire-and-Forget Operations

```csharp
protected override void RunCommand(String actionParameter)
{
    Task.Run(() =>
    {
        try
        {
            // Long-running operation
            PerformAction(actionParameter);
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Action failed");
        }
    });
}
```

### Sequential Async Operations

```csharp
private async void OnApplicationStarted(Object sender, EventArgs e)
{
    await Task.Run(async () =>
    {
        try
        {
            // Step 1
            await ConnectAsync();
            
            // Step 2
            await Task.Delay(1000);
            
            // Step 3
            await InitializeAsync();
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Initialization failed");
        }
    });
}
```

### Main Thread Execution

Use sparingly, only for UI-sensitive operations:

```csharp
this.ExecuteOnMainThread(() =>
{
    // UI-sensitive operation
    UpdateDisplay();
});
```

## Error Handling Standards

### Comprehensive Try-Catch

```csharp
public void PerformAction(String parameter)
{
    try
    {
        // Validate first
        if (String.IsNullOrEmpty(parameter))
            throw new ArgumentException("Parameter required");
        
        // Perform operation
        ExecuteOperation(parameter);
        
        PluginLog.Info($"Action completed: {parameter}");
    }
    catch (ArgumentException ex)
    {
        PluginLog.Warning($"Invalid parameter: {ex.Message}");
    }
    catch (Exception ex)
    {
        PluginLog.Error(ex, $"Action failed for parameter: {parameter}");
        
        // Surface to user if critical
        this.OnPluginStatusChanged(PluginStatus.Error, "Action failed");
    }
}
```

### Status Reporting

```csharp
// Report errors to user
this.OnPluginStatusChanged(PluginStatus.Error, "Connection lost");

// Clear errors when resolved
this.OnPluginStatusChanged(PluginStatus.Normal, "Connected");
```

## Testing Patterns

### Unit Test Structure

```csharp
[Fact]
public void RunCommand_WithValidParameter_ExecutesAction()
{
    // Arrange
    var mockObs = new Mock<IOBSWebsocket>();
    mockObs.Setup(x => x.IsConnected).Returns(true);
    var command = new MyCommand(mockObs.Object);
    
    // Act
    command.RunCommand("test-parameter");
    
    // Assert
    mockObs.Verify(x => x.ExecuteAction("test-parameter"), Times.Once);
}
```

### Integration Test Pattern

```csharp
[Fact]
public void Plugin_LoadUnload_CleansUpResources()
{
    // Arrange
    var plugin = new OBSStudioForLogiPlugin();
    
    // Act
    plugin.Load();
    plugin.Unload();
    
    // Assert
    Assert.False(plugin.IsConnected);
    // Verify no resource leaks
}
```

## Performance Best Practices

### Cache Frequently Used Data

```csharp
private Dictionary<String, StockData> _cache = new();

protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
{
    // Use cached data
    if (_cache.TryGetValue(actionParameter, out var data))
    {
        return $"{data.Symbol}\r\n${data.Price}";
    }
    
    return "Loading...";
}
```

### Refresh Only When Needed

```csharp
private void OnDataChanged(String parameter)
{
    // Refresh only affected action
    this.ActionImageChanged(parameter);
    
    // NOT this (refreshes all instances)
    // this.ActionImageChanged();
}
```

### Reuse Resources

```csharp
// Good - reuse HttpClient
private static readonly HttpClient _httpClient = new HttpClient();

// Bad - creates new client each time
private HttpClient CreateClient() => new HttpClient();
```

## Security Considerations

### Validate External Input

```csharp
protected override void RunCommand(String actionParameter)
{
    // Validate format
    if (!IsValidParameter(actionParameter))
    {
        PluginLog.Warning($"Invalid parameter format: {actionParameter}");
        return;
    }
    
    // Sanitize
    var sanitized = SanitizeParameter(actionParameter);
    
    // Use
    ProcessParameter(sanitized);
}
```

### Secure Network Operations

```csharp
// Only connect to localhost
if (!uri.Host.Equals("127.0.0.1") && !uri.Host.Equals("localhost"))
{
    PluginLog.Error("Only localhost connections allowed");
    return;
}
```

### Protect Sensitive Data

```csharp
// Never log passwords
PluginLog.Info($"Connecting to {url}"); // Good
PluginLog.Info($"Connecting with password {password}"); // BAD!

// Read from secure config
var password = ReadPasswordFromConfig();
```

## Related Documentation

- **SDK Assessment**: `sdk-assessment.md` - Compliance analysis
- **Guidelines**: `guidelines.md` - Existing project conventions
- **Structure**: `structure.md` - Project organization
- **Official SDK**: https://logitech.github.io/actions-sdk-docs/
