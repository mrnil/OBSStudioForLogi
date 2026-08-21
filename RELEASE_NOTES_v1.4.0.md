# Release Notes - v1.4.0

## FPS Fix, Stats Improvements & Logging Refactor

This release fixes the FPS display bug, adds two new stats tiles, refactors logging for cleaner production output, and improves the internal architecture.

---

## Fixed

- **FPS display** — Was showing ~3200 instead of 60. The plugin now uses the direct `stats.FPS` property from OBS rather than incorrectly deriving it from `AverageFrameTime` (which is render time per frame, not frame interval).
- **Flaky test** — `StartReplayBuffer_WhenOBSThrows_LogsError` intermittent failure under thread pool contention resolved with extended delay.

---

## New Stats Tiles

Two new tiles added to the **OBS Stats Folder** (8 tiles total):

| Tile | Colour Thresholds |
|------|------------------|
| **Disk Space** | Red <1GB, Yellow <10GB, Blue ≥10GB |
| **Render Time** | Red >10ms, Yellow >5ms, Green ≤5ms |

---

## Logging Improvements

Production logs are now significantly cleaner:

- **Render/UI calls** (GetCommandImage, GetEncoderNames) → Trace level (previously Info)
- **Operational detail** (folder updates, selection state, intermediate steps) → Debug level (previously Info)
- **Duplicate logging removed** — Volume and monitoring changes now logged once at the execution point instead of three times across layers
- **IPluginLog.Debug** added to interface — services layer can now log at Debug level through dependency injection

**Info level now only contains significant state changes** (connected, disconnected, profile switched, stream started, etc.)

---

## Architecture Improvements

- **ConnectionManager event relay** — Main plugin no longer holds a direct reference to `OBSWebSocketManager`. Connection events are relayed through `ConnectionManager.Connected` / `Disconnected` events, clarifying ownership.
- **PluginSettingsCommand** — Renamed from `ConnectionConfigureCommand` to match its broader scope (connection + stats polling).

---

## Documentation

All markdown files updated to reflect current state:

- INSTALL.md, CONTRIBUTING.md, CONFIGURATION.md, config.sample.json
- USER_MANUAL.md, README.md
- Memory bank (test-coverage, tech, structure, product, obs-websocket-api-complete)

---

## Requirements

- OBS Studio 28.0+ with obs-websocket 5.0+
- Logi Plugin Service installed
- .NET 8.0 runtime

---

## Installation

1. Download `OBSStudioForLogiPlugin-v1.4.0.lplug4`
2. Double-click to install, or import via Logi Options+
3. Launch OBS Studio — the plugin connects automatically

---

## Test Status

348 unit tests, all passing.
