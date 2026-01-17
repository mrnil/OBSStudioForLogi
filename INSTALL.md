# Installation Instructions for OBSStudioForLogiPlugin v0.8.0

## Quick Install

### Using LogiPluginTool (Recommended)
```bash
LogiPluginTool.exe install OBSStudioForLogiPlugin-v0.8.0.lplug4
```

### Manual Installation

### Windows
1. Download `OBSStudioForLogiPlugin-v0.8.0.lplug4`
2. Run: `LogiPluginTool.exe install OBSStudioForLogiPlugin-v0.8.0.lplug4`
3. Or manually extract to:
   ```
   %LocalAppData%\Logi\LogiPluginService\Plugins\OBSStudioForLogiPlugin\
   ```
4. Restart Logi Plugin Service (or reboot)
5. Launch OBS Studio - plugin connects automatically

### macOS
1. Download `OBSStudioForLogiPlugin-v0.8.0.lplug4`
2. Run: `LogiPluginTool install OBSStudioForLogiPlugin-v0.8.0.lplug4`
3. Or manually extract to:
   ```
   ~/Library/Application Support/Logi/LogiPluginService/Plugins/OBSStudioForLogiPlugin/
   ```
4. Restart Logi Plugin Service
5. Launch OBS Studio - plugin connects automatically

## Detailed Steps

### 1. Extract the Package
The ZIP contains:
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
- Check for "OBS Studio" plugin in available plugins
- Commands should appear in "1. OBS", "2. Streaming", "3. Recording", "4. Profiles", "5. Scenes" groups

## Requirements

- **OBS Studio** 28.0+ with obs-websocket 5.0+ enabled
- **Logi Plugin Service** installed and running
- **OBS WebSocket** configured (default port 4455)

## Troubleshooting

**Plugin doesn't appear:**
- Verify folder structure matches exactly
- Check Logi Plugin Service logs: `%LocalAppData%\Logi\LogiPluginService\Logs`
- Ensure all DLL files are present in `bin` folder

**Commands disabled:**
- Ensure OBS Studio is running
- Check OBS WebSocket is enabled: Tools → WebSocket Server Settings
- Try manual reconnect button

**Connection issues:**
- Verify OBS WebSocket port (default 4455)
- Check firewall isn't blocking localhost connections
- Review plugin logs for connection errors

## Uninstall

Delete the plugin folder:
- Windows: `%LocalAppData%\Logi\LogiPluginService\Plugins\OBSStudioForLogiPlugin`
- macOS: `~/Library/Application Support/Logi/LogiPluginService/Plugins/OBSStudioForLogiPlugin`

Restart Logi Plugin Service.
