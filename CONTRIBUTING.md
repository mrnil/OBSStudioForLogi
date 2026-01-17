# Contributing to OBSStudioForLogiPlugin

Thank you for your interest in contributing! This document provides guidelines for contributing to the project.

## Development Setup

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Logi Plugin Service installed
- OBS Studio 28.0+ with obs-websocket 5.0+

### Getting Started
1. Clone the repository
2. Open `OBSStudioForLogiPlugin.sln` in Visual Studio
3. Build the solution: `dotnet build`
4. Run tests: `dotnet test`

## Development Guidelines

### Code Style
- Follow the conventions in `.amazonq/rules/memory-bank/guidelines.md`
- Use BCL type names (`String`, `Boolean`, `Int32`) instead of C# keywords
- Prefix private fields with underscore (`_fieldName`)
- Use PascalCase for classes, methods, and properties
- Use camelCase for parameters and local variables

### Test-Driven Development
- Write tests before or alongside implementation
- All business logic must have accompanying tests
- Maintain 80%+ test coverage for services layer
- Run all tests before committing: `dotnet test`

### Commit Messages
- Use conventional commit format: `type: description`
- Types: `feat`, `fix`, `docs`, `test`, `refactor`, `chore`
- Include detailed body explaining what changed and why
- Example:
  ```
  feat: add audio mixer mute/unmute controls
  
  Implemented AudioMixerMuteCommand and AudioMixerUnmuteCommand to control
  audio source muting in OBS Studio. Commands use multi-state buttons to
  show muted/unmuted state with visual indicators.
  
  Added 6 unit tests covering mute/unmute operations and state management.
  
  Why: Users need quick access to mute audio sources during live streams
  without switching to OBS window.
  ```

### Pull Request Process
1. Create a feature branch from `main`
2. Implement changes with tests
3. Ensure all tests pass
4. Update documentation (README, TODO, memory bank)
5. Commit with conventional commit messages
6. Create pull request with detailed description

## Adding New Features

### Adding a New Command
1. Create command class in `src/Actions/`
2. Inherit from `PluginDynamicCommand` or `PluginMultistateDynamicCommand`
3. Implement `RunCommand()` method
4. Add icon resources to `src/Icons/`
5. Update `.csproj` with embedded resources
6. Add tests in `tests/OBSStudioForLogiPlugin.Tests/`
7. Update README and TODO

### Adding OBS Integration
1. Add method to `IOBSWebsocket.cs` interface
2. Implement in `OBSWebsocketAdapter.cs`
3. Add business logic to `OBSActionExecutor.cs`
4. Add tests with Moq for all layers
5. Wire up in `OBSStudioForLogiPlugin.cs`

## Testing

### Running Tests
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test class
dotnet test --filter "FullyQualifiedName~OBSActionExecutorTests"
```

### Writing Tests
- Use xUnit framework
- Use Moq for mocking dependencies
- Follow Arrange-Act-Assert pattern
- Name tests: `MethodName_Condition_ExpectedBehavior`
- Add `Thread.Sleep(100)` when testing `Task.Run` fire-and-forget methods

## Documentation

### Update These Files
- `README.md`: User-facing features and usage
- `TODO.md`: Roadmap and completed items
- `CHANGELOG.md`: Version history
- `.amazonq/rules/memory-bank/*.md`: Architecture and patterns

### Memory Bank Structure
- `product.md`: Product overview and use cases
- `structure.md`: Project structure and components
- `tech.md`: Technology stack and build system
- `guidelines.md`: Code quality standards and patterns

## Questions?

Open an issue for:
- Bug reports
- Feature requests
- Documentation improvements
- Questions about architecture or patterns
