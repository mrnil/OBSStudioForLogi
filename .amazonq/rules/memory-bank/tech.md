# Technology Stack

## Programming Languages
- **C# 11**: Primary language (with implicit usings enabled)
- **.NET 8.0**: Target framework
- **Nullable Reference Types**: Disabled (`<Nullable>disable</Nullable>`)

## Core Dependencies

### Loupedeck SDK
- **PluginApi.dll**: Proprietary SDK for Loupedeck/Logi hardware integration
- **Location (Windows)**: `C:\Program Files\Logi\LogiPluginService\`
- **Location (macOS)**: `/Applications/Utilities/LogiPluginService.app/Contents/MonoBundle/`
- Provides base classes: `Plugin`, `PluginDynamicCommand`, `PluginMultistateDynamicCommand`, `ClientApplication`

### NuGet Packages
- **obs-websocket-dotnet** (v5.0.1): Official OBS WebSocket client library
  - Provides WebSocket communication with OBS Studio
  - Supports OBS WebSocket protocol v5.0+
  - Handles authentication, event subscriptions, and request/response patterns

## Build System

### MSBuild Configuration
- **SDK**: Microsoft.NET.Sdk
- **Build Tool**: MSBuild (via Visual Studio or dotnet CLI)
- **Solution File**: OBSStudioForLogiPlugin.sln (Visual Studio 2022 format)

### Build Properties
```xml
<TargetFramework>net8.0</TargetFramework>
<ImplicitUsings>enable</ImplicitUsings>
<Nullable>disable</Nullable>
<RootNamespace>Loupedeck.OBSStudioForLogiPlugin</RootNamespace>
<AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
```

### Custom Build Paths
- **Base Output**: `[ProjectRoot]/../bin/`
- **Binary Output**: `bin/[Configuration]/bin/`
- **Plugin Link File**: `%LocalAppData%\Logi\LogiPluginService\Plugins\OBSStudioForLogiPlugin.link` (Windows)
- **Plugin Link File**: `~/Library/Application Support/Logi/LogiPluginService/Plugins/` (macOS)

### Build Targets
1. **CopyPackage** (AfterTargets: PostBuildEvent)
   - Copies `package/**/*` to output directory
   - Includes metadata and icon files

2. **PostBuild** (AfterTargets: PostBuildEvent)
   - Creates `.link` file pointing to build output
   - Triggers plugin hot-reload: `loupedeck:plugin/OBSStudioForLogi/reload`

3. **PluginClean** (AfterTargets: CoreClean)
   - Removes `.link` file
   - Cleans output directories

## Development Commands

### Build
```bash
# Build Debug configuration
dotnet build src/OBSStudioForLogiPlugin.csproj

# Build Release configuration
dotnet build src/OBSStudioForLogiPlugin.csproj -c Release

# Build entire solution
dotnet build OBSStudioForLogiPlugin.sln
```

### Clean
```bash
# Clean build artifacts
dotnet clean src/OBSStudioForLogiPlugin.csproj

# Clean entire solution
dotnet clean OBSStudioForLogiPlugin.sln
```

### Test
```bash
# Run all tests
dotnet test tests/OBSStudioForLogiPlugin.Tests/OBSStudioForLogiPlugin.Tests.csproj

# Run tests with verbosity
dotnet test tests/OBSStudioForLogiPlugin.Tests/OBSStudioForLogiPlugin.Tests.csproj -v normal
```

### Restore Dependencies
```bash
# Restore NuGet packages
dotnet restore OBSStudioForLogiPlugin.sln
```

## Development Environment

### IDE Support
- **Visual Studio 2022** (v17.8+): Primary IDE with full solution support
- **Visual Studio Code**: Supported via .vscode configuration
  - `launch.json`: Debug configurations
  - `tasks.json`: Build tasks

### Platform Requirements
- **Windows**: Windows 10/11 with .NET 8.0 SDK
- **macOS**: macOS 10.15+ with .NET 8.0 SDK
- **Logi Plugin Service**: Must be installed for plugin deployment and testing

### Editor Configuration
- **.editorconfig**: Code style and formatting rules (located in `src/.editorconfig`)
- **Directory.Build.props**: Shared MSBuild properties (located in `src/Directory.Build.props`)

## Testing Framework

### Test Project
- **Project**: OBSStudioForLogiPlugin.Tests
- **Framework**: Likely xUnit or NUnit (based on standard .NET test project structure)
- **Test Categories**:
  - `OBSActionExecutorTests.cs`: Action execution logic
  - `OBSConfigReaderTests.cs`: Configuration file parsing
  - `OBSLifecycleManagerTests.cs`: Connection lifecycle
  - `OBSWebSocketManagerTests.cs`: WebSocket management
  - `OBSWebSocketManagerStateTests.cs`: State management
  - `OBSWebSocketManagerReconnectionTests.cs`: Reconnection logic
  - `OBSWebSocketManagerLoggingTests.cs`: Logging behavior

## Embedded Resources

### Resource Types
- **Icons**: SVG and PNG files embedded as resources
- **Metadata**: Plugin icon (Icon256x256.png)
- **Logical Names**: Resources use fully qualified names (e.g., `Loupedeck.OBSStudioForLogiPlugin.Icons.RecordingOn.svg`)

### Resource Loading
```csharp
// Example from csproj
<EmbeddedResource Include="Icons\RecordingOn.svg">
  <LogicalName>Loupedeck.OBSStudioForLogiPlugin.Icons.RecordingOn.svg</LogicalName>
</EmbeddedResource>
```

## Version Control
- **Git**: Source control system
- **.gitignore**: Standard Visual Studio ignore patterns
  - Excludes: bin/, obj/, .vs/, user-specific files
  - Includes: .vscode/settings.json, .vscode/tasks.json, .vscode/launch.json

## Deployment

### Plugin Installation
1. Build creates `.link` file in Logi Plugin Service plugins directory
2. Link file contains path to build output
3. Plugin Service loads plugin from linked directory
4. Hot-reload triggered automatically via `loupedeck:plugin/OBSStudioForLogi/reload` protocol

### Distribution
- Plugin distributed as directory structure containing:
  - Compiled DLL (OBSStudioForLogiPlugin.dll)
  - Dependencies (obs-websocket-dotnet.dll)
  - Metadata files (package/metadata/)
  - Icon resources (embedded in DLL)
