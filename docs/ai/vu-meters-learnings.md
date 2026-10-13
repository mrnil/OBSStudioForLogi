# VU Meters — Design and Learnings

## Status

Shipped in v2.0.0 on obs-websocket-dotnet 5.7.0. The subscription support the meters need was contributed upstream (BarRaider/obs-websocket-dotnet PR #150). The Live Audio Folder (`AudioMetersDynamicFolder`) and the per-source Audio Meter action (`AudioMeterCommand`) are both device-checked.

## How It Works

```text
OBS InputVolumeMeters (about every 50ms, only while subscribed)
    → OBSWebSocketManager.OnInputVolumeMeters maps the library type to Models.AudioMeterLevels
    → AudioMeterService stores the latest levels with a timestamp
    → refresh timer (PluginConfig.AudioMeterRefreshInterval, default 100ms = 10fps)
    → CommandImageChanged per visible input
    → GetCommandImage → VuMeterRenderer
```

- **The subscription is shared and on demand.** `InputVolumeMeters` is a high-volume event that OBS only sends to clients that ask for it. In 5.7.0, attaching the first handler sends a `ReIdentify` that asks for it, and removing the last handler sends another that stops it. `OBSWebSocketManager.SubscribeToVolumeMeters(owner)`/`UnsubscribeFromVolumeMeters(owner)` take a named owner, so the handler stays attached while any owner (the folder, any meter button) needs it.
- **The folder subscribes in `Activate()` and unsubscribes in `Deactivate()`.** `PluginDynamicFolder` calls these when its first instance opens and its last closes. Both return `Boolean`. `PluginDynamicFolder.Close()` isn't virtual, and isn't needed.
- **Plain commands have no visibility hooks**, so `AudioMeterCommand` infers visibility from an `ActivityLease`. Each `GetCommandImage` call renews it for `OBSTimings.AudioMeterRenderLease` (15s), and the refresh timer unsubscribes once it lapses. While idle, the timer still invalidates the images about once a second (`AudioMeterIdleProbeInterval`), so a button that comes into view asks to be redrawn and renews the lease. A 3s lease toggled the subscription, and its `ReIdentify`, on every page flip. `StatsDisplay` uses the same pattern.
- **The library type stays out of the service layer.** `AudioMeterService` only sees `Models.AudioMeterLevels`, so its tests don't depend on the library's event types.

## What the Device Showed

- **OBS only reports active inputs.** `InputVolumeMeters` covers the live scene's audio sources plus global devices such as microphones. An input that is only in a scene that isn't live doesn't appear at all.
- **Some active inputs have no channels.** A browser source without "Control audio via OBS", for example, is reported with an empty channel array. These still count as live and are drawn as one silent channel, because hiding them hid sources users expected to see.
- **The levels need a dB scale.** A linear 0.0–1.0 mapping squeezed normal speech (about -20dB, 0.1 linear) into a 10% bar. The meters use OBS's own scale: -60dB floor, 0dB full scale, green below -20dB, yellow from -20dB to -10dB, red at -10dB and above. `VuMeterRenderer.LinearToDb` reuses `VolumeConverter.MulToDb`.
- **Silent and broken look the same without a baseline.** Each reported channel gets a 2px dim grey baseline, so "live but silent" looks different from "no data".
- **No name in the image.** The SDK draws the action's display name over the button, so drawing the input name as well showed it twice.

## Behaviour Decisions

- **Stale levels expire.** OBS drops an input from the event as soon as it goes inactive, which left its last level frozen on the tile. `AudioMeterService` treats a level older than `OBSTimings.AudioMeterStaleThreshold` (500ms, about 10 missed events) as no data.
- **The folder lists live inputs from the meter data**, not from scene items. `AudioMeterService.GetLiveInputs()` returns fresh inputs in first-seen order, so tiles don't reshuffle when an input goes quiet and comes back. OBS already works out what is active, which covers global devices, nested scenes, groups, hidden items and renames. The folder re-reads the list on each refresh tick and calls `ButtonActionNamesChanged()` only when it changes. It is empty for the first tick after `Activate()`, until the first event arrives.
- **Mute state is cached.** `GetInputMute` is a blocking OBS request, which can't run from `GetCommandImage` at 10–20fps per tile. `AudioMeterService` seeds each input's mute state once, off the event thread, and keeps it current from `InputMuteStateChanged`. An event that arrives during the lookup wins over the lookup's result.
- **Tile states**: not live gives a grey crossed-out speaker (`AudioMeterInactive.svg`), live and muted a red crossed-out speaker (`AudioMeterMuted.svg`), live and unmuted the bars. Not live wins over muted (`VuMeterRenderer.ResolveTileState`).
- **A tap toggles mute.** The meters deliberately don't copy the audio folders' selection, double-tap or encoder behaviour.
- **Refresh rate** is a Plugin Settings option (20, 10 or 5fps). The folder reads it on each `Activate()`, and meter buttons on connect.
- **The folder's display name is "Live Audio Folder"**; the class is still `AudioMetersDynamicFolder`, so existing button assignments keep working.

## Event Payload

Each input in `InputVolumeMeters` carries one array per channel of `[magnitude, peak, inputPeak]`, all linear 0.0–1.0. The meters draw `peak`. `magnitude` is the smoother RMS level, and `inputPeak` is the level before the volume fader. Mono inputs have one channel, stereo two.
