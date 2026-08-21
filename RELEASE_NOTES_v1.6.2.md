# Release Notes — v1.6.2

No functional changes for end users. This is an internal engineering release: a framework migration, a plugin-resilience fix, and a large repository/tooling cleanup.

## Fixed

### CommandCoordinator Exception Isolation

Previously, if a single registered command threw an exception while handling an event (e.g. `OnSceneChanged`, `OnInputMuteChanged`), that exception could prevent every other registered command from receiving the same notification. `CommandCoordinator` now dispatches each command individually with its own try/catch, so one misbehaving command can no longer silently break updates for the rest of the plugin's buttons. `CommandRegistry` was simplified to a plain store with a generic type-filter (`GetCommands<T>()`) to support this without duplicating dispatch logic.

### Recurring Build Failure After Framework Changes

Fixed a bug where the build's intermediate output directory (`obj/`) resolved to a different location depending on whether the project was built via the `.sln`, the bare `.csproj`, or `dotnet test` — causing spurious `CS0579` duplicate-attribute build errors when switching between these. The build now resolves `obj/` consistently regardless of invocation method.

### Test Project Warnings

Resolved nullable-reference build warnings (CS8600/CS8602) in the test project.

## Changed

### .NET 10 Migration

Migrated the plugin and test projects from .NET 8.0 to .NET 10.0, including the CI build workflow. Verified against a real Logi Plugin Service install — the plugin loads, connects to OBS, and responds to button presses correctly under net10.0.

## Documentation & Tooling

- Moved AI coding-assistant reference docs from `.amazonq/rules/` to `docs/ai/`, since Amazon Q is no longer used for this project. Added `AGENTS.md` and `CLAUDE.md` at the repo root so Claude Code and other AI assistants pick up project context automatically.
- Added a GitHub Actions workflow that lints all markdown files on every push/PR (`rumdl`), and fixed the existing violations across the repo.
- `AGENTS.md` now points AI assistants at the Logi Actions SDK's official documentation index (`https://logitech.github.io/actions-sdk-docs/llms.txt`) to consult for SDK questions instead of inferring behavior from the compiled `PluginApi.dll`.
- Removed `tools/InspectSdk`, a reflection-based utility previously used to reverse-engineer the SDK's shape before official documentation was available.
- Backfilled `CHANGELOG.md` entries for v1.5.1, v1.6.0, and v1.6.1, which had been missed at the time.

## Dependencies

| Package | From | To |
|---|---|---|
| `System.Drawing.Common` | 10.0.10 | 10.0.11 |
| `Microsoft.Extensions.Logging.Abstractions` | 10.0.10 | 10.0.11 |

## Testing

- 393 unit tests, all passing
- Verified the net10.0 build runs correctly under a real Logi Plugin Service install

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 10.0 SDK (for development only)

## Installation

Download `OBSStudioForLogiPlugin-v1.6.2.lplug4` and install via Logi Options+ or Loupedeck software.
