# VU Meters Implementation Learnings

## Status: Rolled Back (Implementation Incomplete)

## Overview

Attempted to implement real-time VU meters as a dynamic folder. The core rendering and service architecture is sound, but the OBS WebSocket event subscription model blocked the implementation at the library level.

## Key Findings

### 1. InputVolumeMeters Event Requires Opt-In (Critical Blocker)

The OBS WebSocket 5.x protocol categorises `InputVolumeMeters` as a **high-volume event** that is NOT included in the default `eventSubscriptions` bitmask during connection identification.

- Default subscription = `0x1FF` (511) = all standard event categories
- `InputVolumeMeters` = bit 16 = `1 << 16` = 65536
- To receive meter events: must send `eventSubscriptions: 66047` (511 | 65536) during Identify or via Reidentify (OpCode 3)

The obs-websocket-dotnet library v5.0.1:

- Has only `ConnectAsync(String url, String password)` — no subscription parameter
- Does NOT expose a `Reidentify` method
- Does NOT have an `EventSubscription` enum
- The internal `SendIdentify` hardcodes `rpcVersion` only, no `eventSubscriptions` field
- The internal WebSocket client (`wsConnection`) is a private field of type `WebsocketClient` from `Websocket.Client`

### 2. Library Modifications Required

To receive `InputVolumeMeters` events, the obs-websocket-dotnet library needs:

1. **`EventSubscription` flags enum** in `Communication/` namespace:
   - `None = 0`, `General = 1<<0`, through `Ui = 1<<10`
   - `All = 0x7FF` (all standard categories)
   - `InputVolumeMeters = 1<<16`, `InputActiveStateChanged = 1<<17`, `InputShowStateChanged = 1<<18`

2. **`EventSubscriptions` property** on `OBSWebsocket` (default = `EventSubscription.All`):
   - Read during `SendIdentify` and included as `"eventSubscriptions"` in the Identify payload

3. **`Reidentify(EventSubscription)` method** on `OBSWebsocket`:
   - Sends OpCode 3 message: `{"op": 3, "d": {"eventSubscriptions": N}}`
   - Allows changing subscriptions without reconnecting

4. **Strongly-typed `InputVolumeMeter` model**:
   - `InputName` (string)
   - `InputLevelsMul` (`List<List<float>>`) — per-channel [magnitude, peak, inputPeak]

5. **Updated `InputVolumeMetersEventArgs`**:
   - New `Inputs` property (`List<InputVolumeMeter>`) for typed access
   - Keep deprecated `inputs` (`List<JObject>`) for backward compatibility

6. **Updated event parsing in `Events.cs`**:
   - Parse `body["inputs"]` as JArray (not string — the old code used `(string)body["inputs"]` which was wrong)
   - Deserialize into `InputVolumeMeter` objects manually for performance

### 3. InputVolumeMetersEventArgs Data Format

Per the OBS WebSocket 5.x protocol, each input in the event contains:

```json
{
  "inputName": "Desktop Audio",
  "inputLevelsMul": [
    [0.023, 0.154, 0.154],  // Channel 0 (left): [magnitude, peak, inputPeak]
    [0.019, 0.132, 0.132]   // Channel 1 (right): [magnitude, peak, inputPeak]
  ]
}
```

- `magnitude` = RMS level (index 0) — lower, smoother
- `peak` = peak level (index 1) — use this for VU bar height
- `inputPeak` = pre-fader peak (index 2) — raw input before volume
- All values are linear 0.0-1.0
- Mono sources have 1 channel array, stereo have 2
- Event fires at OBS video framerate (~20-60Hz depending on output FPS)

### 4. Plugin Architecture (Validated Design)

The architecture designed for VU meters is sound:

```
OBS fires InputVolumeMeters (~60Hz)
    ↓
OBSWebSocketManager.OnInputVolumeMeters()
    ↓
AudioMeterService.UpdateLevels(levels) — stores latest peaks
    ↓ (100ms timer = 10fps refresh)
AudioMetersDynamicFolder.RefreshMeters()
    ↓
CommandImageChanged(inputName) per visible input
    ↓
GetCommandImage() → VuMeterRenderer.Render(peakL, peakR, inputName, imageSize)
```

### 5. VuMeterRenderer Design (Validated)

- Vertical stereo bars using `BitmapBuilder.FillRectangle(x, y, width, height, color)`
- Colour zones: Green (<-12dB / 0.25 linear), Yellow (-12 to -3dB / 0.25-0.71), Red (>-3dB / 0.71+)
- `BitmapColor(R, G, B)` 3-arg constructor works fine for FillRectangle colours
- Layout: two bars side-by-side with 4px margins, source name at bottom
- Bar height = `Math.Clamp(peak, 0, 1) * meterHeight`

### 6. On-Demand Subscription Pattern

- Subscribe to `InputVolumeMeters` ONLY when meter folder is open
- Call `Reidentify(All | InputVolumeMeters)` to start receiving events
- Call `Reidentify(All)` to stop (reduces bandwidth when not needed)
- `GetButtonPressActionNames` signals folder open (start metering)
- 60-second safety timeout auto-stops if folder state is lost
- `PluginDynamicFolder.Close()` is NOT virtual — cannot override for cleanup

### 7. Loupedeck SDK Constraints

- `PluginDynamicFolder.Close()` is not virtual/override-able
- No explicit folder open/close lifecycle hooks
- `GetButtonPressActionNames(DeviceType)` is called when folder opens — use as "opened" signal
- `BitmapBuilder` supports: `Clear()`, `FillRectangle()`, `DrawText()`, `DrawImage()`, `ToImage()`
- Button sizes: 80×80 (observed), 90×90, or 60×60 depending on device

### 8. obs-websocket-dotnet Library Structure

Located at: `b:\development\obs-websocket-dotnet\obs-websocket-dotnet\`

Key files:

- `OBSWebsocket.cs` — main class, `ConnectAsync`, `SendIdentify`, `HandleHello`, `WebsocketMessageHandler`
- `Events.cs` — `ProcessEventType` switch statement, all event handlers
- `Communication/MessageTypes.cs` — OpCodes (Hello=0, Identify=1, Identified=2, ReIdentify=3, Event=5, Request=6)
- `Communication/MessageFactory.cs` — builds JSON messages
- `Types/Events/InputVolumeMetersEventArgs.cs` — event args
- Private field: `WebsocketClient wsConnection` (from Websocket.Client package)
- `SendRequest(MessageTypes opCode, string requestType, JObject fields, bool waitForReply)` — internal method

### 9. Project Reference Switch

To use modified library source instead of NuGet:

```xml
<!-- Replace in .csproj -->
<!-- Old: <PackageReference Include="obs-websocket-dotnet" Version="5.0.1" /> -->
<!-- New: <ProjectReference Include="..\..\obs-websocket-dotnet\obs-websocket-dotnet\obs-websocket-dotnet.csproj" /> -->
```

Test project needs relative path: `..\..\...\obs-websocket-dotnet\obs-websocket-dotnet\obs-websocket-dotnet.csproj`

## Implementation Checklist (For Future)

When ready to re-implement:

1. [ ] Complete obs-websocket-dotnet library modifications (EventSubscription enum, Reidentify method, typed model)
2. [ ] Switch plugin project to local ProjectReference
3. [ ] Update `OBSWebSocketManager.SubscribeToVolumeMeters()` to use `_obs.Reidentify()`
4. [ ] Update `OnInputVolumeMeters` to use typed `e.Inputs` property
5. [ ] Verify events arrive with non-zero peak values
6. [ ] Test rendering with real data
7. [ ] Remove debug logging once working
8. [ ] Update tests for new library API
9. [ ] Consider publishing forked library as NuGet package or git submodule

## Files Created (Now Rolled Back)

- `src/Helpers/VuMeterRenderer.cs` — bar rendering logic (12 tests)
- `src/Services/AudioMeterService.cs` — level storage + throttle (10 tests)
- `src/Actions/AudioMetersDynamicFolder.cs` — folder UI
- `tests/.../VuMeterRendererTests.cs`
- `tests/.../AudioMeterServiceTests.cs`

## Estimated Effort to Complete

- Library modifications: 1 hour (code exists, just needs clean integration)
- Plugin integration: 30 minutes (remove reflection hack, use typed API)
- Testing with real OBS: 30 minutes
- Total: ~2 hours
