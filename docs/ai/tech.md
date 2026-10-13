# Technology Stack

## Runtime and Language

| Item | Value |
|------|-------|
| Language | C# |
| Target Framework | .NET 10.0 (`net10.0`) |
| Nullable Reference Types | Disabled in plugin project; Enabled in test project |
| Implicit Usings | Enabled |
| Root Namespace | `Loupedeck.OBSStudioForLogiPlugin` |

## Key Dependencies

### Plugin Project (`src/OBSStudioForLogiPlugin.csproj`)

| Package | Version | Purpose |
|---------|---------|---------|
| `PluginApi.dll` | (runtime-provided) | Logi Actions SDK — base classes for Plugin, PluginDynamicCommand, PluginDynamicFolder, ActionEditorCommand, BitmapBuilder, etc. |
| `obs-websocket-dotnet` | 5.7.0 | OBS WebSocket 5.x client library (5.7.0+ required for `InputVolumeMeters` high-volume event subscription) |
| `Microsoft.Extensions.Logging.Abstractions` | 10.0.12 | Logging abstractions (must be >= the version `obs-websocket-dotnet` depends on, or restore fails with NU1605) |
| `System.Drawing.Common` | 10.0.12 | Image rendering support |

### Test Project (`tests/OBSStudioForLogiPlugin.Tests/OBSStudioForLogiPlugin.Tests.csproj`)

| Package | Version | Purpose |
|---------|---------|---------|
| `xunit` | 2.9.3 | Test framework |
| `xunit.runner.visualstudio` | 4.0.0 | VS/IDE test runner |
| `Moq` | 4.21.0 | Mocking framework |
| `Microsoft.NET.Test.Sdk` | 18.10.1 | Test SDK |
| `coverlet.collector` | 10.1.0 | Code coverage collection |

`obs-websocket-dotnet` reaches the test project transitively through the plugin project reference.

## Loupedeck SDK Base Classes Used

| SDK Class | Used By |
|-----------|---------|
| `Plugin` | `OBSStudioForLogiPlugin` |
| `PluginDynamicCommand` | All toggle/start/stop/display commands |
| `PluginDynamicFolder` | All dynamic folder commands |
| `PluginDynamicAdjustment` | `SelectedSourceVolumeAdjustment` |
| `ActionEditorCommand` | All user-defined commands + `PluginSettingsCommand` |
| `BitmapBuilder` | `ButtonTextRenderer` |
| `BitmapImage` | Return type of all `GetCommandImage` overrides |
| `BitmapColor` | Colour constants and custom colours |
| `EmbeddedResources` | Icon loading |
| `PluginImageSize` | Image size parameter in GetCommandImage |
| `DeviceType` | Device-specific folder navigation |

## SDK Plugin API Location

- **Windows**: `C:\Program Files\Logi\LogiPluginService\PluginApi.dll`
- **macOS**: `/Applications/Utilities/LogiPluginService.app/Contents/MonoBundle/PluginApi.dll`
- **CI**: `ci/PluginApi.dll` (stub for build validation without installed service)

## Code Style — EditorConfig Enforced Rules

The `.editorconfig` in `src/` sets the following as warnings. The editor shows them, but the build doesn't report them because `EnforceCodeStyleInBuild` isn't set, and the file doesn't apply to `tests/` (assessment #36):

- **`this.` qualification required** for all fields, methods, properties, events
- **No `var`** — explicit types always (`csharp_style_var_*` = false)
- **BCL type keywords forbidden** — use `String`, `Int32`, `Boolean`, `Single` not `string`, `int`, `bool`, `float`
- **Braces always required** (`csharp_prefer_braces = true`)
- **`using` directives inside namespace** (`csharp_using_directive_placement = inside_namespace`)
- **Allman brace style** — opening brace on new line (`csharp_new_line_before_open_brace = all`)
- **4-space indentation**, CRLF line endings
- **Readonly fields** preferred (`dotnet_style_readonly_field = true`)
- **Null propagation** preferred (`dotnet_style_null_propagation = true`)
- **Private fields**: `_camelCase` with underscore prefix (enforced by SX1309/SX1309S)
- **Interfaces**: `I` prefix (PascalCase)
- **Types, methods, properties**: PascalCase

## Build Commands

```bash
# Development build (triggers hot-reload via .link file)
dotnet build src/OBSStudioForLogiPlugin.csproj

# Release build (for packaging)
dotnet build src/OBSStudioForLogiPlugin.csproj -c Release

# Run all tests
dotnet test tests/OBSStudioForLogiPlugin.Tests/OBSStudioForLogiPlugin.Tests.csproj

# Run tests with coverage
dotnet test tests/OBSStudioForLogiPlugin.Tests/OBSStudioForLogiPlugin.Tests.csproj --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~OBSActionExecutorAudioTests"

# Clean
dotnet clean OBSStudioForLogiPlugin.sln
```

## Packaging and Release

```bash
# Create .lplug4 package (use dotnet global tool)
LogiPluginTool pack "b:\development\OBSStudioForLogiPlugin\bin\Release" "b:\development\OBSStudioForLogiPlugin\OBSStudioForLogiPlugin-v{VERSION}.lplug4"

# Verify package
LogiPluginTool verify "b:\development\OBSStudioForLogiPlugin\OBSStudioForLogiPlugin-v{VERSION}.lplug4"

# Check metadata
LogiPluginTool metadata "b:\development\OBSStudioForLogiPlugin\OBSStudioForLogiPlugin-v{VERSION}.lplug4"
```

**LogiPluginTool locations:**

- Preferred: `C:\Users\steph\.dotnet\tools\logiplugintool.exe` (dotnet global tool, callable as `LogiPluginTool`)
- Alternative: `B:\development\LogitechBeta\LogiPluginSdkTools\LogiPluginTool.exe`
- ❌ Do NOT use: `C:\Program Files\Logi\LogiPluginService\LogiPluginTool.exe` (broken — missing companion DLL)

## Version Management

The version is defined in two places, and a release updates both (see `release-process.md`). At v2.0.1 the `.csproj` was missed and still says 2.0.0 (assessment #39):

1. `src/OBSStudioForLogiPlugin.csproj` — `<Version>`, `<AssemblyVersion>`, `<FileVersion>`
2. `src/package/metadata/LoupedeckPackage.yaml` — `version:`

## CI/CD

- **GitHub Actions**: `.github/workflows/dependency-check.yml` — dependency vulnerability scanning + build validation
- **GitHub Actions**: `.github/workflows/markdown-lint.yml` — `rumdl` on every Markdown file
- **Tests**: run locally only. They were kept out of CI because fire-and-forget tests raced the thread pool. That race is fixed (#15), but CI hasn't been switched on yet (#42)
- **Dependabot**: `.github/dependabot.yml` — automated dependency update PRs

## OBS WebSocket Protocol

- **Protocol version**: OBS WebSocket 5.x
- **Library**: obs-websocket-dotnet 5.7.0
- **Required OBS version**: 28.0+ (with obs-websocket 5.0+ built-in)
- **Default port**: 4455
- **Authentication**: SHA256 challenge-response (handled by library)
- **Connection**: ws:// (unencrypted — note for remote connections)

## Test Architecture

- **Framework**: xUnit 2.9.3
- **Mocking**: Moq 4.21.0 — `IOBSWebsocket` and `IPluginLog` are the primary mock targets
- **Pattern**: Arrange-Act-Assert
- **Async testing**: background work goes through an injected `Action<Action>` runner; tests pass `action => action()` to run it inline instead of sleeping (see `guidelines.md`)
- **Test count and coverage**: see `test-coverage.md`
