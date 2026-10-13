# Installation Instructions for OBSStudioForLogiPlugin

## Quick Install

### Using LogiPluginTool (Recommended)

```bash
LogiPluginTool.exe install OBSStudioForLogiPlugin-v{VERSION}.lplug4
```

### Manual Installation

### Windows

1. Download `OBSStudioForLogiPlugin-v{VERSION}.lplug4`
2. Run: `LogiPluginTool.exe install OBSStudioForLogiPlugin-v{VERSION}.lplug4`
3. Or manually extract to:

   ```
   %LocalAppData%\Logi\LogiPluginService\Plugins\OBSStudioForLogiPlugin\
   ```

4. Restart Logi Plugin Service (or reboot)
5. Launch OBS Studio - plugin connects automatically

### macOS

1. Download `OBSStudioForLogiPlugin-v{VERSION}.lplug4`
2. Run: `LogiPluginTool install OBSStudioForLogiPlugin-v{VERSION}.lplug4`
3. Or manually extract to:

   ```
   ~/Library/Application Support/Logi/LogiPluginService/Plugins/OBSStudioForLogiPlugin/
   ```

4. Restart Logi Plugin Service
5. Launch OBS Studio - plugin connects automatically

## Detailed Steps

### 1. Extract the Package

The `.lplug4` package contains:

```
bin/                    # Plugin DLL and dependencies
metadata/               # Plugin metadata and icon
profiles/               # Default device profiles
actionsymbols/          # Symbols shown next to each action in the Options+ action picker
```

### 2. Install to Plugin Directory

**Windows:**

- Open File Explorer
- Type in address bar: `%LocalAppData%\Logi\LogiPluginService\Plugins`
- Create folder: `OBSStudioForLogiPlugin`
- Copy all the extracted folders into it

**macOS:**

- Open Finder
- Press Cmd+Shift+G
- Go to: `~/Library/Application Support/Logi/LogiPluginService/Plugins`
- Create folder: `OBSStudioForLogiPlugin`
- Copy all the extracted folders into it

### 3. Restart Logi Plugin Service

**Windows:**

- Right-click system tray → Logi Plugin Service → Quit
- Start Logi Plugin Service from Start Menu

**macOS:**

- Quit Logi Plugin Service from menu bar
- Launch from Applications/Utilities

### 4. Verify Installation

- Open Logi Options+ or the Loupedeck software
- Check for the "Streaming Assistant" plugin in the available plugins
- Its actions appear in groups `1. OBS` to `9. Media` (OBS, Streaming, Recording, Replay Buffer, Virtual Camera, Profiles, Scenes, Audio, Media). The configurable actions are in the `User Defined` sub-groups of Scenes and Audio. See [USER_MANUAL.md](USER_MANUAL.md) for every action

## Requirements

- **OBS Studio** 28.0+ with obs-websocket 5.0+ enabled
- **Logi Plugin Service** installed and running
- **OBS WebSocket** configured (automatically detected from OBS config)
- **Devices**: MX Creative Console, Loupedeck CT, Loupedeck Live

## Features

See [README.md](README.md) for the feature list and [USER_MANUAL.md](USER_MANUAL.md) for how to use each action.

## Configuration

The plugin auto-discovers local OBS settings. For remote OBS or custom settings, use the **Plugin Settings** action or edit the config file manually.

**OBS WebSocket Config Locations:**

- **Windows**: `%AppData%\obs-studio\plugin_config\obs-websocket\config.json`
- **macOS**: `~/Library/Application Support/obs-studio/plugin_config/obs-websocket/config.json`

**Plugin Configuration:**

- **Windows**: `%AppData%\Loupedeck\OBSStudioForLogiPlugin\config.json`
- **macOS**: `~/Library/Application Support/Loupedeck/OBSStudioForLogiPlugin/config.json`

See `CONFIGURATION.md` for full details and examples.

Example config.json:

```json
{
  "logLevel": "Info",
  "useLocalObs": true,
  "remoteIpAddress": "127.0.0.1",
  "remotePort": 4455,
  "statsPollingInterval": 5000,
  "audioMeterRefreshInterval": 100
}
```

The remote OBS password isn't stored in this file: set it with the **Plugin Settings** action, which keeps it encrypted.

## Troubleshooting

**Plugin doesn't appear:**

- Verify folder structure matches exactly
- Check Logi Plugin Service logs: `%LocalAppData%\Logi\LogiPluginService\Logs`
- Ensure all DLL files are present in `bin` folder

**Buttons greyed out:**

- The plugin isn't connected to OBS yet
- Wait for automatic connection or use manual reconnect button
- Restart OBS Studio if connection fails

**Connection issues:**

- Verify OBS WebSocket is enabled: Tools → WebSocket Server Settings
- Check OBS WebSocket port (default 4455)
- For remote OBS, ensure the firewall on the OBS machine allows the WebSocket port
- For remote OBS: verify IP, port, and password in Plugin Settings
- Review plugin logs for connection errors
- Try manual reconnect button in plugin

**Audio controls not working:**

- Ensure audio inputs exist in OBS
- Inputs of kinds the plugin doesn't recognise as audio (for example `monitor_capture`) don't appear in the audio folders yet
- Mixer for Scene Audio shows the inputs in the current scene, plus audio inputs that aren't in any scene

**Folder icons not appearing:**

- Restart Logi Plugin Service (it reads the action symbols only at start-up)
- Button icons are embedded in the plugin DLL; reinstall the package if they are missing
- Check Logi Plugin Service logs for errors

## Logs

**Plugin Logs:**

- **Windows**: `%LocalAppData%\Logi\LogiPluginService\Logs\plugin_logs\OBSStudioForLogi.log`
- **macOS**: `~/Library/Logs/Logi/LogiPluginService/OBSStudioForLogiPlugin.log`

**Logi Plugin Service Logs:**

- **Windows**: `%LocalAppData%\Logi\LogiPluginService\Logs`
- **macOS**: `~/Library/Logs/Logi/LogiPluginService`

## Uninstall

Delete the plugin folder:

- Windows: `%LocalAppData%\Logi\LogiPluginService\Plugins\OBSStudioForLogiPlugin`
- macOS: `~/Library/Application Support/Logi/LogiPluginService/Plugins/OBSStudioForLogiPlugin`

Restart Logi Plugin Service.
