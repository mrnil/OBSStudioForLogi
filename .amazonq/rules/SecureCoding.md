# Secure Coding Principles for OBSStudioForLogiPlugin

## Input Validation

* Validate all data received from OBS WebSocket before processing
* Use String.IsNullOrEmpty() for all string inputs from external sources
* Validate port numbers are in valid range (1-65535)
* Validate file paths before reading/writing (prevent path traversal)
* Handle JSON parsing exceptions gracefully with proper error messages
* Validate IP addresses are localhost (127.0.0.1 or ::1) before connecting

## Error Handling and Logging

* Use try-catch blocks for all external I/O operations (file, network)
* Log errors with sufficient context but without sensitive data
* Never log passwords or authentication tokens
* Sanitize file paths in logs to avoid exposing usernames
* Use centralized logging (PluginLog) for all log operations
* Fail securely - disable functionality when errors occur

## Data Protection

* Never store OBS WebSocket password in plugin code or config
* Read password from OBS config only when needed
* Clear sensitive data from memory when no longer needed
* Use readonly fields for injected dependencies
* Validate data before passing to OBS WebSocket API

## Memory Management

* Properly dispose of all IDisposable resources
* Unsubscribe from events in Unload/Dispose methods
* Use 'using' statements for file operations
* Avoid memory leaks in long-running async operations

## Communication Security

* WebSocket connections must only connect to localhost (127.0.0.1 or ::1)
* Validate IP address is localhost before connecting
* Use OBS's built-in WebSocket authentication
* Handle connection failures gracefully with retry logic

## File Operations

* Only read from known OBS configuration directories
* Only write screenshots to user-approved directories (Pictures/Documents/Desktop)
* Validate file paths before operations
* Use Path.Combine() for cross-platform path construction
* Handle file I/O exceptions gracefully

## General Practices

* Use managed code (.NET) for automatic memory management
* Avoid dynamic code execution
* Never execute OS commands from user input
* Use async/await properly to avoid blocking
* Validate connection state before all OBS operations
* Use guard clauses for early validation
