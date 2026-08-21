# Multi-Instance OBS Support — Design Notes

## Status: Not Implemented (Future Consideration)

## Overview

This document records the analysis of supporting two or more OBS Studio instances from a single plugin installation.

## Current Limitation

The plugin assumes a single OBS instance:

- Single `OBSWebSocketManager` → single `OBSActionExecutor`
- `OBSFacade` delegates to one manager
- `ConnectionManager` reads one config file, connects to one port
- All commands reference `OBSStudioForLogiPlugin.Instance` with one set of state
- `AudioSelectionState` is a static singleton
- Loupedeck SDK provides one plugin instance (cannot run two copies)

## Recommended Approach: Named Connection Instances

Each OBS instance gets a named connection (e.g., "Main", "Preview"). Users configure which instance a button targets.

### Architecture

1. **ConnectionRegistry** — replaces single `ConnectionManager`, holds `Dictionary<String, OBSWebSocketManager>`
2. **Per-instance configuration** — plugin config file (`%AppData%/Logi/LogiPluginService/OBSStudioForLogiPlugin/connections.json`):

   ```json
   [
     { "name": "Main", "host": "127.0.0.1", "port": 4455, "password": "abc" },
     { "name": "Preview", "host": "127.0.0.1", "port": 4456, "password": "xyz" }
   ]
   ```

3. **ActionEditorCommand pattern** — commands get optional "Instance" dropdown/textbox targeting a specific OBS instance (default: primary)
4. **OBSFacade becomes instance-aware** — methods accept optional instance name:

   ```csharp
   public void ToggleRecording(String instanceName = null)
   ```

### Impact Assessment

| Component | Change Level | Details |
|-----------|-------------|---------|
| `ConnectionManager` | Heavy rewrite | Becomes `ConnectionRegistry` managing N connections |
| `OBSWebSocketManager` | No change | Already self-contained per connection |
| `OBSActionExecutor` | No change | Already scoped to one `IOBSWebsocket` |
| `OBSFacade` | Major refactor | Must route calls to correct instance |
| `CommandCoordinator` | Moderate | Events must carry instance context |
| `IObsCommand` interfaces | Breaking change | `OnConnected(String instance)` signature |
| `CommandRegistry` | Moderate | Notify with instance context |
| All Action commands | Moderate | Add instance parameter support |
| `AudioSelectionState` | Moderate | Per-instance selection state |
| Display commands | Moderate | Show which instance they report on |
| Dynamic folders | Moderate | Scope scene/source lists to an instance |
| Tests | Significant | ~50% of tests need instance parameter added |

### Key Risks

- **Singleton pattern**: Commands use `static Instance` assuming one plugin state. Multi-instance needs instance-scoped state or context via action parameters.
- **Event routing**: Events from "Preview OBS" shouldn't update "Main OBS" buttons. Events must carry instance identity.
- **Loupedeck SDK**: One plugin instance only. All multi-instance logic must live within the single plugin, routed via action parameters.

### Phased Implementation Plan

1. **Phase 1**: Refactor `OBSFacade` and `ConnectionManager` to be instance-aware internally, defaulting to single instance (non-breaking).
2. **Phase 2**: Add `connections.json` config and `ConnectionRegistry`.
3. **Phase 3**: Add instance parameter to `ActionEditorCommand`-based commands first (they already have config UI).
4. **Phase 4**: Extend dynamic folders and display commands.

### Estimated Effort

- Full implementation: ~2-3 weeks of focused work

## Alternative Approaches Considered

### Primary/Secondary Fixed Pair

Hardcode exactly two instances. Simpler (~1 week) but doesn't scale and is less elegant.

### Separate Plugin Instances

Duplicate plugin with different name/namespace per OBS instance. Zero code changes but terrible UX, maintenance burden, and no cross-instance operations.

## Decision

Deferred. Record for future consideration when multi-OBS use cases become a priority.
