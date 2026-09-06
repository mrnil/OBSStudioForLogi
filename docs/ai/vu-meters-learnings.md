# VU Meters Implementation Learnings

## Status: Working on a Real Device — Not Yet Committed

## Overview

Attempted to implement real-time VU meters as a dynamic folder. The core rendering and service architecture is sound, but the OBS WebSocket event subscription model blocked the implementation at the library level (see "Key Findings" below — this is the original blocker analysis, kept for context).

**Current state (2026-09-06)**: confirmed working end-to-end on a real device — bars render and move with live mic input. A local fork of `obs-websocket-dotnet` on branch `feat/high-volume-event-subscription` (at `B:\development\obs-websocket-dotnet\`) implements everything this document originally scoped as needed, plus a cleaner subscription mechanism than planned — see "Update: Actual Implementation" below. `src/OBSStudioForLogiPlugin.csproj` and the test project both currently point at this fork via `ProjectReference` (temporary — intended to be reverted to the NuGet package once the fork is upstreamed via PR). **This `ProjectReference` uses an absolute local path and has not been committed** - pushing it as-is would break CI (the GitHub Actions runner doesn't have this path). Do not commit the `ProjectReference` change until either the fork is published somewhere CI can reach, or the PR merges upstream and the reference reverts to the NuGet package.

The plugin-side implementation (`AudioMeterService`, `VuMeterRenderer`, `AudioMetersDynamicFolder`, plumbing through `OBSWebSocketManager`/`OBSFacade`/`OBSStudioForLogiPlugin`) is written, passing tests locally, and verified against real OBS audio.

## Real-Device Findings (2026-09-06)

- **The pipeline works end-to-end** — confirmed via temporary diagnostic logging (added and then removed once root-caused) that live mic levels reach `AudioMeterService` correctly, and that bars render and move on the device for an input with real data.
- **Not every audio input reports levels.** OBS's `InputVolumeMeters` only reports "active" inputs — in practice this tracked the currently-active scene's items (plus device-capture inputs like a mic, which are active regardless of scene). Inputs living in a scene that isn't currently live simply don't appear in the event at all; their tiles correctly show no bars (nothing to render), which is expected OBS behavior, not a plugin bug.
- **Some reported-active inputs still have an empty channel array** (`browser_source`/`game_capture` kinds specifically, in the case tested) — OBS is reporting them as active but with zero audio channels flowing (e.g. a browser source without "Control audio via OBS" enabled). The renderer correctly draws nothing when there's no channel data.
- **Removed the in-image name label.** `VuMeterRenderer.Render` originally drew the input name via `DrawText` inside the bitmap, on top of the bars — redundant with the SDK's own button title (driven by the action's display name, since `AudioMetersDynamicFolder` doesn't override `GetCommandDisplayName`). Dropped the `inputName` parameter from `Render` entirely rather than leave it unused.
- **A genuinely silent-but-active input (2 channels, both 0.0) is indistinguishable from "not working"** at a glance, since a 0-height bar is invisible against the black background. Not fixed yet — worth a follow-up (e.g. a thin floor/baseline indicator) if it turns out to be confusing in practice.
- **Switched from linear to dB scale, matching OBS's own meter** (`-60dB` floor to `0dB` full scale; green `< -20dB`, yellow `-20dB` to `-10dB`, red `>= -10dB`). The original linear 0.0-1.0 mapping compressed normal speech (~-20dB, ~0.1 linear) into a barely-visible ~10% bar height — real feedback after seeing it live on the device. `VuMeterRenderer.LinearToDb` reuses `VolumeConverter.MulToDb` (same amplitude-ratio-to-dB formula already used for volume-fader displays); `CalculateMeterFraction` maps the clamped dB value onto the visible 0.0-1.0 range before `CalculateBarHeight` scales it to pixels. Color zone thresholds also moved from linear to dB for the same reason.

## Update: Actual Implementation (supersedes the original blocker-era design below where noted)

- **Subscription is simpler than planned**: the fork's `InputVolumeMeters` C# event uses custom `add`/`remove` accessors — subscribing (`+=`) sends the `ReIdentify` automatically on the first handler, unsubscribing (`-=`) sends it again once the last handler is removed. No manual `EventSubscription`/`Reidentify()` calls needed from the plugin side at all.
- **`PluginDynamicFolder` has `Activate()`/`Deactivate()` lifecycle hooks** (confirmed via the SDK's official docs, not assumed) - `Activate()` fires when the first instance of the folder opens, `Deactivate()` when the last instance closes. This replaces the 60-second safety-timeout workaround this document originally called for (see "Loupedeck SDK Constraints" below, now outdated) - `AudioMetersDynamicFolder.Activate()`/`Deactivate()` subscribe/unsubscribe cleanly with no timeout needed. Both are declared `public override Boolean Activate()`/`Deactivate()` (returning `Boolean`, not `void` - discovered via compiler error, not documented).
- **Library types decoupled from the testable service layer**: `AudioMeterService` (the testable piece) never touches the fork's `InputVolumeMeter` type directly - `OBSWebSocketManager.OnInputVolumeMeters` maps it into the plugin's own `Models.AudioMeterLevels` first. This insulates `AudioMeterService`'s tests from the fork's (temporary, pre-PR) type shapes changing.
- **Refresh rate is configurable** (`PluginConfig.AudioMeterRefreshInterval`, default 100ms/10fps, exposed via `PluginSettingsCommand` alongside `StatsPollingInterval`) rather than hardcoded - read fresh by the folder on each `Activate()`, no live-update plumbing needed since the timer only exists while the folder is open.
- **Scope for this iteration**: one dynamic folder (`AudioMetersDynamicFolder`, `8. Audio###Meters`) covering every audio input, tap-to-mute (no double-tap/encoder/selection-state parity with `AudioMixerDynamicFolder` - kept deliberately simple for a first pass). Bar-style meter (not a scrolling graph) per the original design intent.

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

**Superseded** — `PluginDynamicFolder.Close()` is indeed not overridable, but the claim of "no explicit folder open/close lifecycle hooks" was wrong (or the SDK gained this since): the official SDK docs confirm `Activate()`/`Deactivate()` exist for exactly this purpose (see "Update: Actual Implementation" above). Kept below for historical context only — do not use the `GetButtonPressActionNames`-as-open-signal + safety-timeout workaround it describes.

- ~~`PluginDynamicFolder.Close()` is not virtual/override-able~~ — still true, but irrelevant now that `Deactivate()` covers this
- ~~No explicit folder open/close lifecycle hooks~~ — false; see `Activate()`/`Deactivate()` above
- ~~`GetButtonPressActionNames(DeviceType)` is called when folder opens — use as "opened" signal~~ — unnecessary workaround, superseded by `Activate()`
- `BitmapBuilder` supports: `Clear()`, `FillRectangle()`, `DrawText()`, `DrawImage()`, `ToImage()` — still accurate, verified via existing usage in `ButtonTextRenderer.cs`
- Button sizes: 80×80 (observed), 90×90, or 60×60 depending on device — still accurate

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

## Remaining Steps (as of 2026-08-21)

1. [x] Complete obs-websocket-dotnet library modifications — done on the fork (`EventSubscription` enum, typed `InputVolumeMeter` model, auto-subscribing event accessor)
2. [x] Switch plugin project to local ProjectReference — done, uncommitted (see status note above)
3. [x] Wire `OBSWebSocketManager.SubscribeToVolumeMeters()`/`UnsubscribeFromVolumeMeters()` using the fork's event accessor
4. [x] Map the fork's typed `InputVolumeMeter`/`ChannelLevel` into the plugin's own `Models.AudioMeterLevels`
5. [ ] Verify events arrive with non-zero peak values against real OBS
6. [ ] Test rendering with real data on a real device — tune bar/text layout (see `VuMeterRenderer`'s render-method comment on unverified `DrawText` positioning)
7. [ ] Decide on `RunCommand`'s tap-to-mute behavior once seen in practice — reconsider double-tap/selection parity with `AudioMixerDynamicFolder` if the simple version feels wrong
8. [x] Add tests for the new library-facing API surface (`AudioMeterServiceTests.cs`, `VuMeterRendererTests.cs`, `OBSFacadeTests.cs` additions)
9. [ ] Raise the PR to merge `feat/high-volume-event-subscription` upstream, then revert both `.csproj` files from `ProjectReference` back to the NuGet package once merged and published

## Files Created (Current — Not Yet Committed)

- `src/Models/AudioMeterLevels.cs` — plugin-owned model, decoupled from the fork's types
- `src/Services/AudioMeterService.cs` — level storage (10 tests)
- `src/Helpers/VuMeterRenderer.cs` — bar rendering + testable color-zone/height-calculation logic (16 tests)
- `src/Actions/AudioMetersDynamicFolder.cs` — folder UI, `Activate()`/`Deactivate()`-driven subscription (2 tests)
- `tests/.../AudioMeterServiceTests.cs`, `tests/.../VuMeterRendererTests.cs`, `tests/.../Actions/AudioMetersDynamicFolderTests.cs`
- Modified: `OBSWebSocketManager.cs`, `OBSFacade.cs`, `OBSStudioForLogiPlugin.cs`, `PluginConfig.cs`, `PluginSettingsCommand.cs`, both `.csproj` files (library reference)
