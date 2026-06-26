# Plugin Configuration

## Overview

The OBSStudioForLogiPlugin supports configuration via a JSON file or the **Plugin Settings** action in Logi Options+. The Plugin Settings action is the recommended way to configure the plugin — it persists settings automatically and reconnects when changed.

## Configuration Methods

### Method 1: Plugin Settings Action (Recommended)

1. Drag **Plugin Settings** (from Group 1. OBS) onto a button
2. Configure the settings in Logi Options+
3. Press the button to save and apply

### Method 2: Manual JSON File

For advanced users or automation, you can edit the config file directly.

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

4. Restart Logi Plugin Service to apply changes

## Configuration Options

### Log Level

Controls the verbosity of plugin logging.

| Level | Description |
|-------|-------------|
| `Trace` | Very detailed logging for debugging specific issues |
| `Debug` | Detailed logging for development and troubleshooting |
| `Info` | General informational messages (default) |
| `Warning` | Warning messages for non-critical issues |
| `Error` | Error messages only |

### Connection Settings

Controls how the plugin connects to OBS Studio.

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `useLocalObs` | Boolean | `true` | When true, auto-discovers local OBS WebSocket settings. When false, uses manual remote settings. |
| `remoteIpAddress` | String | `"127.0.0.1"` | IP address of the remote OBS machine (used when `useLocalObs` is false) |
| `remotePort` | Integer | `4455` | WebSocket port (used when `useLocalObs` is false) |
| `remotePassword` | String | `""` | WebSocket password (used when `useLocalObs` is false) |

### Stats Settings

Controls performance statistics polling.

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `statsPollingInterval` | Integer | `5000` | How often to poll OBS for stats, in milliseconds. Valid values: 2000, 5000, 10000 |

## Full Example Configuration

### Local OBS (default behaviour)

```json
{
  "logLevel": "Info",
  "useLocalObs": true,
  "remoteIpAddress": "127.0.0.1",
  "remotePort": 4455,
  "remotePassword": "",
  "statsPollingInterval": 5000
}
```

### Remote OBS

```json
{
  "logLevel": "Info",
  "useLocalObs": false,
  "remoteIpAddress": "192.168.1.50",
  "remotePort": 4455,
  "remotePassword": "my-obs-password",
  "statsPollingInterval": 2000
}
```

### Troubleshooting (verbose logging)

```json
{
  "logLevel": "Trace",
  "useLocalObs": true,
  "remoteIpAddress": "127.0.0.1",
  "remotePort": 4455,
  "remotePassword": "",
  "statsPollingInterval": 5000
}
```

## Sample Configuration File

See `config.sample.json` in the project root for a complete example with default values.

## Viewing Logs

Plugin logs are written to the Logi Plugin Service log file:

**Windows**: `%LocalAppData%\Logi\LogiPluginService\Logs\`

## Behaviour Notes

- When `useLocalObs` is `true`, the plugin reads OBS's own WebSocket config file to discover port and password automatically. The remote settings are ignored.
- When `useLocalObs` is `false`, the plugin connects directly to the specified IP/port without waiting for port availability (since it can't detect a remote OBS process).
- Changes made via the Plugin Settings action overwrite the config file.
- If the config file doesn't exist or is invalid, the plugin uses defaults (local OBS, Info logging, 5s polling).

## Troubleshooting

### Configuration Not Loading

1. Check the file path is correct
2. Ensure the JSON is valid (use a JSON validator)
3. Check the plugin log for configuration loading messages
4. Restart Logi Plugin Service after creating/modifying the config file

### Invalid JSON

If the configuration file contains invalid JSON, the plugin will:

- Log a warning message
- Use default settings
- Continue operating normally

### Remote Connection Not Working

1. Verify OBS is running on the remote machine
2. Verify the WebSocket server is enabled in OBS (Tools → WebSocket Server Settings)
3. Check the IP address is reachable from your machine
4. Verify the port and password match the remote OBS settings
5. Check firewall rules allow the connection
