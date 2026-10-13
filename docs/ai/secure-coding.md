# Secure Coding Principles for OBSStudioForLogiPlugin

## Input Validation

* Validate all data received from OBS WebSocket before processing
* Use String.IsNullOrEmpty() for all string inputs from external sources
* Validate port numbers are in valid range (1-65535)
* Validate file paths before reading/writing (prevent path traversal)
* Handle JSON parsing exceptions gracefully with proper error messages
* Validate the remote IP address before connecting (`OBSConnectionSettings` parses it; see assessment #38 for its gaps)

## Error Handling and Logging

* Use try-catch blocks for all external I/O operations (file, network)
* Log errors with sufficient context but without sensitive data
* Never log passwords or authentication tokens
* Sanitize file paths in logs to avoid exposing usernames
* Use centralized logging (PluginLog) for all log operations
* Fail securely - disable functionality when errors occur

## Data Protection

* Never store OBS WebSocket password in plugin code or `config.json`
* Local OBS: read the password from OBS's own config only when needed
* Remote OBS: store the password only through `ISecretStore`, which the plugin backs with the SDK's plugin settings (`SetPluginSetting`, stored encrypted by the Logi Plugin Service, `backupOnline: false`)
* Values typed into an Action Editor control are saved in plaintext in the device profile (`ProfileInfo.json`) by the Logi Plugin Service. If a secret has to be entered through one, store it elsewhere on save and let an empty field keep the stored value, so users can clear the field (as the Plugin Settings Password field does)
* Clear sensitive data from memory when no longer needed
* Use readonly fields for injected dependencies
* Validate data before passing to OBS WebSocket API

## Memory Management

* Properly dispose of all IDisposable resources
* Unsubscribe from events in Unload/Dispose methods
* Use 'using' statements for file operations
* Avoid memory leaks in long-running async operations

## Communication Security

* Local OBS is reached on localhost; a remote OBS only at the address the user set in Plugin Settings
* The connection is `ws://`, not `wss://`. Authentication is a SHA256 challenge-response, so the password isn't sent, but everything after the handshake is unencrypted. Remote connections belong on a trusted network, and the user docs say so
* Use OBS's built-in WebSocket authentication
* Handle connection failures gracefully; `ConnectionManager` is the only place that retries

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
