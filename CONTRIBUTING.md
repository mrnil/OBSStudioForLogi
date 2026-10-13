# Contributing to OBSStudioForLogiPlugin

Thank you for your interest in contributing! This document provides guidelines for contributing to the project.

## Development Setup

### Prerequisites

- .NET 10.0 SDK
- Visual Studio 2022 or VS Code
- Logi Plugin Service installed
- OBS Studio 28.0+ with obs-websocket 5.0+

### Getting Started

1. Clone the repository
2. Open `OBSStudioForLogiPlugin.sln` in Visual Studio
3. Build the plugin: `dotnet build src/OBSStudioForLogiPlugin.csproj` (this also links it into Logi Plugin Service and reloads it)
4. Run tests: `dotnet test tests/OBSStudioForLogiPlugin.Tests/OBSStudioForLogiPlugin.Tests.csproj`

## Development Guidelines

### Code Style

- Follow the conventions in `docs/ai/guidelines.md`
- Use BCL type names (`String`, `Boolean`, `Int32`) instead of C# keywords, and explicit types instead of `var`
- Qualify members with `this.`, and use Allman braces, always with braces around single-line bodies
- Prefix private fields with underscore (`_fieldName`)
- Use PascalCase for classes, methods, and properties
- Use camelCase for parameters and local variables
- Markdown must pass `rumdl` (run in CI by `markdown-lint.yml`)

### Test-Driven Development

- Write tests before or alongside implementation
- All business logic must have accompanying tests
- Maintain 80%+ test coverage for services layer
- Run all tests before committing

### Commit Messages

- Use conventional commit format: `type(scope): description`
- Types: `feat`, `fix`, `perf`, `docs`, `test`, `refactor`, `build`, `chore`
- Include detailed body explaining what changed and why
- Example:

  ```
  fix(outputs): redraw streaming and recording buttons on state change (#27)

  The seven streaming and recording commands never registered, and
  StreamStateChanged/RecordStateChanged notified nothing, so the buttons
  kept showing the old state until the page was redrawn.

  Add IStreamingAwareCommand and IRecordingAwareCommand, route both
  events through IOBSEventListener and CommandCoordinator, and have the
  commands register and redraw.
  ```

### Pull Request Process

1. Create a feature branch from `main`
2. Implement changes with tests
3. Ensure all tests pass
4. Update documentation (README, USER_MANUAL, TODO, CHANGELOG, `docs/ai/`)
5. Commit with conventional commit messages
6. Create pull request with detailed description

## Adding New Features

### Adding a New Command

1. Create command class in `src/Actions/`
2. Consider using base classes:
   - `ToggleCommandBase` for toggle commands (on/off)
   - `StartStopCommandBase` for start/stop commands
   - `PluginDynamicCommand` for simple actions
   - `PluginMultistateDynamicCommand` for multi-state buttons
   - `PluginDynamicFolder` for dynamic folders
   - `ActionEditorCommand` for user-configurable actions
3. Implement `IObsCommand` interface for connection state awareness
4. Register with `OBSStudioForLogiPlugin.Instance?.RegisterCommand(this)` in constructor
5. Implement the notification interface for any OBS state the button shows, or it won't redraw
6. Use `ButtonImageHelper`/`ButtonTextRenderer` for all image rendering
7. Add icons with the scripts in `tools/icons/` (see `docs/ai/icon-style.md`), including the action symbol for the new class
8. Add tests in `tests/OBSStudioForLogiPlugin.Tests/`
9. Update README, USER_MANUAL, CHANGELOG, TODO, and `docs/ai/`

The full checklist is under "Adding a New Feature" in `docs/ai/guidelines.md`.

### Adding OBS Integration

1. Add method to `IOBSWebsocket.cs` interface
2. Implement in `OBSWebsocketAdapter.cs`
3. Add business logic to `OBSActionExecutor.cs` through its `RunMutation`/`Query`/`TryQuery` helpers
4. Expose it through `OBSFacade.cs` and `OBSStudioForLogiPlugin.cs`
5. Add tests with Moq for the connected, disconnected and error paths

## Testing

### Running Tests

```bash
# Run all tests
dotnet test tests/OBSStudioForLogiPlugin.Tests/OBSStudioForLogiPlugin.Tests.csproj

# Run with coverage
dotnet test tests/OBSStudioForLogiPlugin.Tests/OBSStudioForLogiPlugin.Tests.csproj --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test tests/OBSStudioForLogiPlugin.Tests/OBSStudioForLogiPlugin.Tests.csproj --filter "FullyQualifiedName~OBSActionExecutorTests"
```

**Important:** All tests must pass locally before committing. The GitHub Actions workflows check dependencies, validate the build and lint the Markdown, but don't run the tests yet.

### Writing Tests

- Use xUnit framework
- Use Moq for mocking dependencies
- Follow Arrange-Act-Assert pattern
- Name tests: `MethodName_Condition_ExpectedBehavior`
- For fire-and-forget methods, construct the class with an inline background runner (`action => action()`) and assert directly — don't `Thread.Sleep` and hope the work has finished
- See `docs/ai/test-coverage.md` for the test files and current coverage

## Documentation

### Update These Files

- `README.md`: User-facing features and usage
- `USER_MANUAL.md`: End-user guide with detailed instructions
- `INSTALL.md`: Installation and verification steps
- `CONFIGURATION.md`: All plugin configuration options
- `TODO.md`: Roadmap and completed items
- `CHANGELOG.md`: Version history
- `docs/ai/*.md`: Architecture and patterns

### AI Context Docs (`docs/ai/`)

Reference material for both human contributors and AI coding assistants (Claude Code reads `CLAUDE.md`/`AGENTS.md` at the repo root, which point here). Key files:

- `product.md`: Product overview and use cases
- `structure.md`: Project structure and components
- `tech.md`: Technology stack and build system
- `guidelines.md`: Code quality standards and patterns
- `secure-coding.md`: Security principles for this project
- `commit-conventions.md`: Commit message format and rules
- `assessment.md` / `test-coverage.md` / `release-process.md`: Findings backlog, test architecture, release checklist
- `agents.md`: The Claude subagents in `.claude/agents/`

## Questions?

Open an issue for:

- Bug reports
- Feature requests
- Documentation improvements
- Questions about architecture or patterns
