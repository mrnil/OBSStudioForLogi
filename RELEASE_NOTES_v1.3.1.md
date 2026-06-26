# Release Notes - v1.3.1

## Packaging Fix

This is a patch release that fixes a packaging issue from v1.3.0. No functional changes.

---

## Fixed

- **PluginApi.dll excluded from package** — The Logi Plugin Service SDK library was incorrectly being bundled inside the `.lplug4` package. This DLL is provided by the Logi Plugin Service at runtime and should not be distributed with the plugin. This reduces package size by ~15MB and prevents potential version conflicts.

---

## Installation

1. Download `OBSStudioForLogiPlugin-v1.3.1.lplug4`
2. Double-click to install, or import via Logi Options+
3. Restart Logi Plugin Service if prompted

If upgrading from v1.3.0, simply install over the top — no settings will be lost.

---

## Full Changelog

See [v1.3.0 release notes](RELEASE_NOTES_v1.3.0.md) for the complete list of new features added in this release cycle:

- Remote OBS connection support
- OBS performance stats display
- Stream stats folder
- Media controls (play/pause/stop)
- 5 new real-time event subscriptions
- Reconnection race condition fix
- 348 unit tests
