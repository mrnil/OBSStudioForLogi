# Release Notes - v1.5.0

## WebSocket Disabled Status Indicator

This minor release adds a third distinct state to the connection status button, making it clear when OBS is running but the WebSocket server is disabled in its configuration.

---

## Added

- **WebSocket Disabled state** — The connection status button now shows "WebSocket Disabled" with an orange background when the OBS config file has `server_enabled=false`. This is distinct from:
  - **Connected** (green) — successfully connected to OBS
  - **Disconnected** (red) — OBS is not running or unreachable
  - **WebSocket Disabled** (orange) — OBS is installed but WebSocket server is turned off
- **Plugin status message** — Shows "OBS WebSocket server is disabled. Enable it in OBS Tools menu." to guide users to fix the issue

---

## How to Fix

If you see the orange "WebSocket Disabled" state:

1. Open OBS Studio
2. Go to **Tools → WebSocket Server Settings**
3. Check **Enable WebSocket Server**
4. Click OK
5. The plugin will automatically connect

---

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 8.0 runtime

---

## Installation

1. Download `OBSStudioForLogiPlugin-v1.5.0.lplug4`
2. Double-click to install, or import via Logi Options+
3. Launch OBS Studio — the plugin connects automatically

---

## Test Status

362 unit tests, all passing.
