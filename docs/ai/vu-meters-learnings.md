# VU Meters Implementation Learnings

## Status: Complete — Merged to `main`, Using Published obs-websocket-dotnet 5.7.0

## Overview

Attempted to implement real-time VU meters as a dynamic folder. The core rendering and service architecture is sound, but the OBS WebSocket event subscription model blocked the implementation at the library level (see "Key Findings" below — this is the original blocker analysis, kept for context).

**Current state (2026-09-06)**: confirmed working end-to-end on a real device — bars render and move with live mic input. A local fork of `obs-websocket-dotnet` on branch `feat/high-volume-event-subscription` (at `B:\development\obs-websocket-dotnet\`) implements everything this document originally scoped as needed, plus a cleaner subscription mechanism than planned — see "Update: Actual Implementation" below. The fork was merged upstream (BarRaider/obs-websocket-dotnet PR #150) and published as NuGet `obs-websocket-dotnet` 5.7.0 (2026-09-27); `src/OBSStudioForLogiPlugin.csproj` now uses that package, and the temporary absolute-path `ProjectReference` in both `.csproj` files has been removed.

The plugin-side implementation (`AudioMeterService`, `VuMeterRenderer`, `AudioMetersDynamicFolder`, plumbing through `OBSWebSocketManager`/`OBSFacade`/`OBSStudioForLogiPlugin`) is written, passing tests locally, and verified against real OBS audio.

## Real-Device Findings (2026-09-06)

- **The pipeline works end-to-end** — confirmed via temporary diagnostic logging (added and then removed once root-caused) that live mic levels reach `AudioMeterService` correctly, and that bars render and move on the device for an input with real data.
- **Not every audio input reports levels.** OBS's `InputVolumeMeters` only reports "active" inputs — in practice this tracked the currently-active scene's items (plus device-capture inputs like a mic, which are active regardless of scene). Inputs living in a scene that isn't currently live simply don't appear in the event at all; their tiles correctly show no bars (nothing to render), which is expected OBS behavior, not a plugin bug.
- **Some reported-active inputs still have an empty channel array** (`browser_source`/`game_capture` kinds specifically, in the case tested) — OBS is reporting them as active but with zero audio channels flowing (e.g. a browser source without "Control audio via OBS" enabled). These inputs are still listed in the live-only folder and drawn as a single silent channel (baseline only). An earlier version of the live-only folder excluded them, which hid browser sources the user expected to see (2026-09-27).
- **Removed the in-image name label.** `VuMeterRenderer.Render` originally drew the input name via `DrawText` inside the bitmap, on top of the bars — redundant with the SDK's own button title (driven by the action's display name, since `AudioMetersDynamicFolder` doesn't override `GetCommandDisplayName`). Dropped the `inputName` parameter from `Render` entirely rather than leave it unused.
- **A genuinely silent-but-active input (2 channels, both 0.0) is indistinguishable from "not working"** at a glance, since a 0-height bar is invisible against the black background. Fixed 2026-09-27 with a 2px baseline per reported channel (see "Update: Live-Only Folder, Staleness, Baseline, Mute" below).
- **Switched from linear to dB scale, matching OBS's own meter** (`-60dB` floor to `0dB` full scale; green `< -20dB`, yellow `-20dB` to `-10dB`, red `>= -10dB`). The original linear 0.0-1.0 mapping compressed normal speech (~-20dB, ~0.1 linear) into a barely-visible ~10% bar height — real feedback after seeing it live on the device. `VuMeterRenderer.LinearToDb` reuses `VolumeConverter.MulToDb` (same amplitude-ratio-to-dB formula already used for volume-fader displays); `CalculateMeterFraction` maps the clamped dB value onto the visible 0.0-1.0 range before `CalculateBarHeight` scales it to pixels. Color zone thresholds also moved from linear to dB for the same reason.

## Update: Live-Only Folder, Staleness, Baseline, Mute (2026-09-27)

- **Stale levels expired.** `OBSWebSocketManager.OnInputVolumeMeters` only updates inputs present in each event, and OBS drops an input from the event as soon as it goes inactive, so its last level used to stay frozen on its tile until the folder closed. `AudioMeterService` now timestamps every update (injectable clock for tests) and treats anything older than `OBSTimings.AudioMeterStaleThreshold` (500ms, about 10 missed events) as no data.
- **The folder shows only live inputs, using the meter data to decide.** `AudioMeterService.GetLiveInputs()` returns fresh inputs in first-seen order, including those reported with zero channels (`AudioMeterLevels.IsLive` marks them, and `VuMeterRenderer.GetDisplayPeaks` draws them as one silent channel). The folder re-reads it on every refresh tick and only calls `ButtonActionNamesChanged()` when the sequence changes. We chose the meter payload over walking scene items (`SceneAudioSourcesDynamicFolder`'s approach via `ISceneSourcesAwareCommand`) because OBS already works out what is "active": that covers global audio devices, nested scenes, groups, hidden items and renames, which scene items would need extra events and recursion for. Scene switches and source changes appear within one tick, and inputs drop out one stale threshold later. Because first-seen order is kept, tiles don't reshuffle when an input goes stale and comes back. The folder no longer implements `IInputsListAwareCommand`.
- **Trade-off:** the folder is empty for the first tick after `Activate()`, until OBS sends its first meter event (about 50ms).
- **Baseline.** Each reported channel gets a 2px dim grey baseline, so "live but silent" (baseline) looks different from "no data" (nothing).
- **Mute state is cached, not queried per repaint.** `GetInputMute` is a synchronous OBS request, so calling it from `GetCommandImage` at 10-20fps per tile isn't an option. `AudioMeterService` caches mute state. It is seeded once per input, off-thread, via `TryBeginMuteLookup`/`CompleteMuteLookup` from `OnInputVolumeMeters`, and kept current by `SetMuted` from `InputMuteStateChanged`. An event that arrives while a lookup is in flight wins over the lookup result. The cache is cleared on unsubscribe and on disconnect. (The first muted styling, grey bars with a red frame, was replaced by a red crossed-out speaker; see the next update.)

## Update: Per-Source Meter Action, Icon States, Folder Rename (2026-09-27)

- **Folder renamed** to "Live Audio Folder" (`DisplayName` only; the class stays `AudioMetersDynamicFolder`, so existing folder placements aren't broken).
- **`AudioMeterCommand`** adds one parameter per audio input (from `GetInputList()`, live or not) in the same `8. Audio###Meters` group, so a user can put a meter for one source on any button. Tap toggles mute.
- **Tile states** come from `VuMeterRenderer.ResolveTileState`: not live gives a grey crossed-out speaker (`AudioMeterInactive.svg`); live and muted gives a red crossed-out speaker (`AudioMeterMuted.svg`); live and unmuted gives the bars. Not live wins over muted. Both meter UIs share this.
- **Subscription for plain commands.** The SDK docs (checked via `llms.txt`) only document `Activate()`/`Deactivate()` for dynamic folders; `PluginDynamicCommand` has no visibility lifecycle. `AudioMeterCommand` infers visibility instead. Each `GetCommandImage` call renews an `ActivityLease` (`OBSTimings.AudioMeterRenderLease`, 15s — was 3s, which toggled the subscription and its `ReIdentify` on every page flip). The first renewal subscribes, and the refresh timer unsubscribes once the lease lapses. While idle, the timer still invalidates images about once a second (`OBSTimings.AudioMeterIdleProbeInterval`) so a button coming into view asks to be redrawn and renews the lease.
- **Assumption to verify on a device:** the SDK only calls `GetCommandImage` for buttons it's showing. If it renders every parameter regardless, the subscription just stays on while connected, which is harmless but loses the bandwidth saving. The timer runs whenever OBS is connected: once a second while idle, and at the refresh rate while a meter is visible.
- **Shared subscription.** `OBSWebSocketManager.SubscribeToVolumeMeters(owner)`/`UnsubscribeFromVolumeMeters(owner)` now take a named owner. The event handler is attached for the first owner and detached (and levels cleared) when the last one leaves, so closing the folder doesn't blank a visible meter button, or the reverse.
- **Refresh rate for meter buttons** is read on connect, so a changed setting applies to them after the next reconnect. The folder still reads it on each `Activate()`.

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

No longer needed (the plugin uses the NuGet package, 5.7.0+). Kept for reference if library changes need to be developed locally again. To use modified library source instead of NuGet:

```xml
<!-- Replace in .csproj -->
<!-- Old: <PackageReference Include="obs-websocket-dotnet" Version="5.7.0" /> -->
<!-- New: <ProjectReference Include="..\..\obs-websocket-dotnet\obs-websocket-dotnet\obs-websocket-dotnet.csproj" /> -->
```

Test project needs relative path: `..\..\...\obs-websocket-dotnet\obs-websocket-dotnet\obs-websocket-dotnet.csproj`

## Remaining Steps (as of 2026-09-27)

1. [x] Complete obs-websocket-dotnet library modifications — done on the fork (`EventSubscription` enum, typed `InputVolumeMeter` model, auto-subscribing event accessor)
2. [x] Switch plugin project to local ProjectReference — done, since reverted to NuGet 5.7.0 (see step 9)
3. [x] Wire `OBSWebSocketManager.SubscribeToVolumeMeters()`/`UnsubscribeFromVolumeMeters()` using the fork's event accessor
4. [x] Map the fork's typed `InputVolumeMeter`/`ChannelLevel` into the plugin's own `Models.AudioMeterLevels`
5. [x] Verify events arrive with non-zero peak values against real OBS — confirmed 2026-09-06 (see "Real-Device Findings")
6. [x] Test rendering with real data on a real device — done 2026-09-06; the in-image `DrawText` name label was removed and the scale switched to dB (see "Real-Device Findings")
7. [x] Decide on `RunCommand`'s tap-to-mute behavior once seen in practice — kept as simple tap-to-mute after real-device use (2026-09-27); no double-tap/selection parity with `AudioMixerDynamicFolder`
8. [x] Add tests for the new library-facing API surface (`AudioMeterServiceTests.cs`, `VuMeterRendererTests.cs`, `OBSFacadeTests.cs` additions)
9. [x] Raise the PR to merge `feat/high-volume-event-subscription` upstream, then revert both `.csproj` files from `ProjectReference` back to the NuGet package once merged and published — merged as PR #150, published in 5.7.0

## Files Created (committed in `0ff3173`)

- `src/Models/AudioMeterLevels.cs` — plugin-owned model, decoupled from the fork's types
- `src/Services/AudioMeterService.cs` — level storage (10 tests)
- `src/Helpers/VuMeterRenderer.cs` — bar rendering + testable color-zone/height-calculation logic (16 tests)
- `src/Actions/AudioMetersDynamicFolder.cs` — folder UI, `Activate()`/`Deactivate()`-driven subscription (2 tests)
- `tests/.../AudioMeterServiceTests.cs`, `tests/.../VuMeterRendererTests.cs`, `tests/.../Actions/AudioMetersDynamicFolderTests.cs`
- Modified: `OBSWebSocketManager.cs`, `OBSFacade.cs`, `OBSStudioForLogiPlugin.cs`, `PluginConfig.cs`, `PluginSettingsCommand.cs`, both `.csproj` files (library reference)
