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
```

### 2. Install to Plugin Directory

**Windows:**

- Open File Explorer
- Type in address bar: `%LocalAppData%\Logi\LogiPluginService\Plugins`
- Create folder: `OBSStudioForLogiPlugin`
- Copy `bin` and `metadata` folders into it

**macOS:**

- Open Finder
- Press Cmd+Shift+G
- Go to: `~/Library/Application Support/Logi/LogiPluginService/Plugins`
- Create folder: `OBSStudioForLogiPlugin`
- Copy `bin` and `metadata` folders into it

### 3. Restart Logi Plugin Service

**Windows:**

- Right-click system tray → Logi Plugin Service → Quit
- Start Logi Plugin Service from Start Menu

**macOS:**

- Quit Logi Plugin Service from menu bar
- Launch from Applications/Utilities

### 4. Verify Installation

- Open Loupedeck software
- Check for "OBS Studio Assistant" plugin in available plugins
- Commands should appear in groups:
  - **1. OBS**: Screenshot, Reconnect, Studio Mode, Connection Status
  - **2. Streaming**: Start, Stop, Toggle streaming
  - **3. Recording**: Start, Stop, Toggle, Pause/Resume recording
  - **4. Replay Buffer**: Toggle, Save replay buffer
  - **5. Virtual Camera**: Start, Stop, Toggle virtual camera
  - **6. Profiles**: Profile selection and folder
  - **7. Scenes**: Scene collections, scenes folder, sources folder
  - **8. Audio**: Audio Mixer folder, Scene Audio folder

## Requirements

- **OBS Studio** 28.0+ with obs-websocket 5.0+ enabled
- **Logi Plugin Service** installed and running
- **OBS WebSocket** configured (automatically detected from OBS config)
- **Supported Devices**: Loupedeck CT, Live, Live S, Razer Stream Controller, Razer Stream Controller X

## Features

### Connection Management

- Automatic connection discovery from OBS configuration
- Resilient reconnection with exponential backoff (1s to 30s)
- Manual reconnect button
- Real-time connection status display

### Streaming & Recording

- Start, stop, toggle streaming
- Start, stop, toggle, pause/resume recording
- Replay buffer control and save
- Virtual camera control
- Visual state indicators on all buttons

### Scene Management

- Switch between scene collections
- Dynamic scenes folder with all available scenes
- Studio mode support (preview/program workflow)
- Source visibility toggle
- Scene-specific audio controls

### Audio Control

- Audio Mixer folder with all audio inputs
- Scene Audio folder with scene-specific audio
- Mute/unmute toggle with visual feedback (green=unmuted, red=muted)
- Real-time volume display (0-100%)
- Automatic updates when volume or mute changes

### Profile Management

- Switch between OBS profiles
- Dynamic profiles folder
- Current profile display

### Advanced Features

- Studio mode toggle and transition
- Screenshot capture with automatic path detection
- Adjustable scene switching (profile → collection → scene)

## Configuration

No manual configuration required. The plugin automatically:

- Discovers OBS WebSocket settings from OBS configuration files
- Connects when OBS Studio launches
- Reconnects automatically if connection is lost

**OBS WebSocket Config Locations:**

- **Windows**: `%AppData%\obs-studio\plugin_config\obs-websocket\config.json`
- **macOS**: `~/Library/Application Support/obs-studio/plugin_config/obs-websocket/config.json`

**Optional Plugin Configuration:**

- **Windows**: `%AppData%\Loupedeck\OBSStudioForLogiPlugin\config.json`
- **macOS**: `~/Library/Application Support/Loupedeck/OBSStudioForLogiPlugin/config.json`

Example config.json:

```json
{
  "logLevel": "Info"
}
```

Available log levels: `Trace`, `Debug`, `Info`, `Warning`, `Error`

## Troubleshooting

**Plugin doesn't appear:**

- Verify folder structure matches exactly
- Check Logi Plugin Service logs: `%LocalAppData%\Logi\LogiPluginService\Logs`
- Ensure all DLL files are present in `bin` folder

**Commands disabled:**

- Plugin only enables when connected to OBS
- Wait for automatic connection or use manual reconnect button
- Restart OBS Studio if connection fails

**Connection issues:**

- Verify OBS WebSocket is enabled: Tools → WebSocket Server Settings
- Check OBS WebSocket port (default 4455)
- Ensure firewall isn't blocking localhost connections
- Review plugin logs for connection errors
- Try manual reconnect button in plugin

**Audio controls not working:**

- Ensure audio inputs exist in OBS
- Check that inputs are not browser sources or other non-audio types
- Verify inputs are added to scenes for Scene Audio folder

**Folder icons not appearing:**

- Restart Logi Plugin Service
- Verify all icon files are present in plugin directory
- Check Logi Plugin Service logs for errors

## Logs

**Plugin Logs:**

- **Windows**: `%LocalAppData%\Logi\LogiPluginService\Logs\OBSStudioForLogiPlugin.log`
- **macOS**: `~/Library/Logs/Logi/LogiPluginService/OBSStudioForLogiPlugin.log`

**Logi Plugin Service Logs:**

- **Windows**: `%LocalAppData%\Logi\LogiPluginService\Logs`
- **macOS**: `~/Library/Logs/Logi/LogiPluginService`

## Uninstall

Delete the plugin folder:

- Windows: `%LocalAppData%\Logi\LogiPluginService\Plugins\OBSStudioForLogiPlugin`
- macOS: `~/Library/Application Support/Logi/LogiPluginService/Plugins/OBSStudioForLogiPlugin`

Restart Logi Plugin Service.
