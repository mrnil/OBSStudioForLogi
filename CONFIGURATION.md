# Plugin Configuration

## Overview

The OBSStudioForLogiPlugin supports optional configuration via a JSON file. This allows you to customize plugin behavior without recompiling.

## Configuration File Location

**Windows**: `%AppData%\Loupedeck\OBSStudioForLogiPlugin\config.json`

**Full Path Example**: `C:\Users\YourUsername\AppData\Roaming\Loupedeck\OBSStudioForLogiPlugin\config.json`

## Creating the Configuration File

1. Create the directory if it doesn't exist:
   ```
   %AppData%\Loupedeck\OBSStudioForLogiPlugin\
   ```

2. Create a file named `config.json` in that directory

3. Add your configuration settings (see below)

4. Reload the plugin in Logi Plugin Service (or restart the service)

## Configuration Options

### Log Level

Controls the verbosity of plugin logging.

**Available Levels** (from most to least verbose):
- `Trace` - Very detailed logging for debugging specific issues (disabled in production)
- `Debug` - Detailed logging for development and troubleshooting
- `Info` - General informational messages (default for Release builds)
- `Warning` - Warning messages for non-critical issues
- `Error` - Error messages only

**Default Values**:
- Debug builds: `Debug`
- Release builds: `Info`

**Example Configuration**:

```json
{
  "logLevel": "Info"
}
```

**To enable detailed logging for troubleshooting**:

```json
{
  "logLevel": "Trace"
}
```

**To reduce logging overhead**:

```json
{
  "logLevel": "Warning"
}
```

## Sample Configuration File

See `config.sample.json` in the project root for a complete example.

## Viewing Logs

Plugin logs are written to the Logi Plugin Service log file:

**Windows**: `%LocalAppData%\Logi\LogiPluginService\Logs\`

## Troubleshooting

### Configuration Not Loading

1. Check the file path is correct
2. Ensure the JSON is valid (use a JSON validator)
3. Check the plugin log for configuration loading messages
4. Restart Logi Plugin Service after creating/modifying the config file

### Invalid JSON

If the configuration file contains invalid JSON, the plugin will:
- Log a warning message
- Use the default log level
- Continue operating normally

## Future Configuration Options

Additional configuration options may be added in future versions, such as:
- Custom reconnection delays
- Screenshot format preferences
- UI customization options
