# Technology Stack

## Runtime and Language

| Item | Value |
|------|-------|
| Language | C# |
| Target Framework | .NET 10.0 (`net10.0`) — migrated from net8.0 in commit `5d04506` (2026-08) |
| Nullable Reference Types | Disabled in plugin project; Enabled in test project |
| Implicit Usings | Enabled |
| Root Namespace | `Loupedeck.OBSStudioForLogiPlugin` |

## Key Dependencies

### Plugin Project (`src/OBSStudioForLogiPlugin.csproj`)

| Package | Version | Purpose |
|---------|---------|---------|
| `PluginApi.dll` | (runtime-provided) | Logi Actions SDK — base classes for Plugin, PluginDynamicCommand, PluginDynamicFolder, ActionEditorCommand, BitmapBuilder, etc. |
| `obs-websocket-dotnet` | 5.0.1 | OBS WebSocket 5.x client library |
| `Microsoft.Extensions.Logging.Abstractions` | 10.0.11 | Logging abstractions |
| `System.Drawing.Common` | 10.0.11 | Image rendering support |

### Test Project (`tests/OBSStudioForLogiPlugin.Tests/OBSStudioForLogiPlugin.Tests.csproj`)

| Package | Version | Purpose |
|---------|---------|---------|
| `xunit` | 2.9.3 | Test framework |
| `xunit.runner.visualstudio` | 3.1.5 | VS/IDE test runner |
| `Moq` | 4.20.72 | Mocking framework |
| `Microsoft.NET.Test.Sdk` | 18.8.1 | Test SDK |
| `coverlet.collector` | 10.0.1 | Code coverage collection |
| `obs-websocket-dotnet` | 5.0.1 | Required for type references in tests |

## Loupedeck SDK Base Classes Used

| SDK Class | Used By |
|-----------|---------|
| `Plugin` | `OBSStudioForLogiPlugin` |
| `PluginDynamicCommand` | All toggle/start/stop/display commands |
| `PluginDynamicFolder` | All dynamic folder commands |
| `PluginDynamicAdjustment` | `SelectedSourceVolumeAdjustment`, `AudioVolumeWheelTool` |
| `ActionEditorCommand` | All user-defined (Group 99) commands + `PluginSettingsCommand` |
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

The `.editorconfig` in `src/` enforces the following (warnings, not errors):

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

Version is defined in two places — both must be updated for a release:
1. `src/OBSStudioForLogiPlugin.csproj` — `<Version>`, `<AssemblyVersion>`, `<FileVersion>`
2. `src/package/metadata/LoupedeckPackage.yaml` — `version:`

## CI/CD

- **GitHub Actions**: `.github/workflows/dependency-check.yml` — dependency vulnerability scanning + build validation
- **Tests**: Run locally only (not in CI — fire-and-forget Task.Run patterns cause timing issues in CI)
- **Dependabot**: `.github/dependabot.yml` — automated dependency update PRs

## OBS WebSocket Protocol

- **Protocol version**: OBS WebSocket 5.x
- **Library**: obs-websocket-dotnet 5.0.1
- **Required OBS version**: 28.0+ (with obs-websocket 5.0+ built-in)
- **Default port**: 4455
- **Authentication**: SHA256 challenge-response (handled by library)
- **Connection**: ws:// (unencrypted — note for remote connections)

## Test Architecture

- **Framework**: xUnit 2.9.3
- **Mocking**: Moq 4.20.72 — `IOBSWebsocket` and `IPluginLog` are the primary mock targets
- **Pattern**: Arrange-Act-Assert
- **Async testing**: `Thread.Sleep(OBSTimings.TestAsyncDelay)` (500ms) after fire-and-forget operations
- **Test count**: 389 unit tests (verified 2026-08-21, all passing on net10.0)
- **Coverage**: ~37.4% line / ~19.8% branch (Cobertura, measured 2026-08-21) — services layer 80-100%, actions layer exempt from strict TDD. Coverage % has drifted down slightly from prior 39.5%/22.6% as more Actions-layer (SDK-exempt) commands were added faster than services-layer surface area grew.
