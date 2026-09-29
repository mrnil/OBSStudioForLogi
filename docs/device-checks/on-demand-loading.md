# Device Check: On-Demand Loading (Assessment #25)

This check confirms on real hardware what the unit tests can't: that the SDK calls the folder and image hooks the way the on-demand loading relies on, and that the loading tiles look right. It takes about 15 minutes.

The script `tools/device-check/device-check.ps1` collects the plugin log for the session and marks where each step starts. You do the steps on the device and in OBS; the log records what the plugin did.

## What it answers

| # | Question | Steps |
|---|----------|-------|
| A | With no folder open, do scene changes skip the source lists (no OBS requests)? | 1 |
| B | Does opening a folder run `Activate`, and closing it (Back or leaving the page) run `Deactivate`? | 2, 4, 5 |
| C | After the first load, does a scene change cost one request (cache hit)? | 3 |
| D | Does adding or removing a source in OBS clear the cache? | 6 |
| E | If a folder is open when OBS connects or reconnects, does it load? | 7 |
| F | Do the stats folders poll only while open, with numbers straight away? | 8, 9 |
| G | Does the stats summary button's image-request lease notice when it scrolls in and out of view? | 10 |
| H | Do the loading tiles draw one message across the two buttons right of Back, on the top row? | 2, 5 |

## Before you start

1. Build Debug: `dotnet build src/OBSStudioForLogiPlugin.csproj`. The build reloads the plugin.
2. In OBS, have at least **three scenes**, with an audio input (a microphone or media source) in at least one of them. Keep a global audio device such as **Desktop Audio** enabled.
3. In Logi Options+, put these on the device:
   - **Scene Sources** folder (`7. Scenes`)
   - **Mixer for Scene Audio** folder (`8. Audio`)
   - **OBS Stats Folder** (`1. OBS`) and **Stream Stats Folder** (`2. Streaming`)
   - **OBS Stats Summary** (`1. OBS`) on a **different page** from the one you start on
4. Start OBS and wait until the device shows it as connected.
5. Open a terminal in the repository root.

If you have more than one device (for example an MX Creative Console and a Loupedeck Live), run step 2 on each, since the tile layout depends on the device.

## Start

```powershell
./tools/device-check/device-check.ps1 Begin
```

Wait a few seconds for the device to show the plugin again after the reload. Before each step below, run the `Mark` command shown, then do the step.

## Steps

1. **Idle scene changes.** `./tools/device-check/device-check.ps1 Mark "1 idle scene changes"`

   With no folder open and the stats summary button off screen, switch scenes in OBS three times, a couple of seconds apart.

2. **Open Scene Sources.** `./tools/device-check/device-check.ps1 Mark "2 open sources"`

   Open the Scene Sources folder. Watch the buttons as it opens: you may see **"Loading sources..."** across the two buttons to the right of Back before the sources appear. It may be too quick to see; if so, that's fine. If you do see it, note:
   - whether it sits on the top row, in the two positions right of Back;
   - whether the text reads as one message across both buttons, or looks cut off, too small or misplaced.

   A photo helps if anything looks wrong.

3. **Scene changes with Scene Sources open.** `./tools/device-check/device-check.ps1 Mark "3 scene changes, sources open"`

   Leave the folder open and switch scenes in OBS three times. The buttons should follow each scene.

4. **Close Scene Sources.** `./tools/device-check/device-check.ps1 Mark "4 close sources, scene change"`

   Leave the folder with its Back button, then switch scenes in OBS once.

5. **Open Mixer for Scene Audio.** `./tools/device-check/device-check.ps1 Mark "5 open scene audio"`

   Open the Mixer for Scene Audio folder. Check the same loading tiles as step 2 (**"Loading audio sources..."**). Check that the list shows the scene's audio inputs followed by the global ones (Desktop Audio and so on), in the same order as before this change.

6. **Change a scene in OBS with the audio folder open.** `./tools/device-check/device-check.ps1 Mark "6 add and remove audio source"`

   With the folder still open, add an audio source to the current scene in OBS (for example **Audio Input Capture**) and wait for the new button. Then delete that source in OBS and wait for its button to go.

7. **Restart OBS with the audio folder open.** `./tools/device-check/device-check.ps1 Mark "7 restart OBS with folder open"`

   Keep the folder open. Close OBS completely and wait until the device shows it as disconnected. Start OBS again and wait for the folder's buttons to come back (you may see the loading tiles first). Then leave the folder with Back.

8. **OBS Stats Folder.** `./tools/device-check/device-check.ps1 Mark "8 stats folder"`

   Open the OBS Stats Folder. Note whether the numbers appear within about a second, or show zeros until a few seconds later. Leave it open for about 15 seconds, then leave it with Back.

9. **Stream Stats Folder.** `./tools/device-check/device-check.ps1 Mark "9 stream stats folder"`

   Open the Stream Stats Folder, wait about 15 seconds, then leave it. (It shows "Offline" unless you are streaming; that's expected.)

10. **Stats summary button.** `./tools/device-check/device-check.ps1 Mark "10 stats summary on"`

    Go to the page with the OBS Stats Summary button and note whether its numbers appear within about a second. Stay on that page for about 15 seconds.

    Then `./tools/device-check/device-check.ps1 Mark "10 stats summary off"`, go to a page without it, and wait **at least 10 seconds**.

## Finish

```powershell
./tools/device-check/device-check.ps1 End
```

It prints a summary and the path of the log extract, under `%TEMP%\OBSStudioForLogiDeviceCheck`. It also restores the log level if Begin changed it.

Tell Claude the check is done, along with anything you saw on the device: the tile positions and look from steps 2 and 5, whether the numbers came straight away in steps 8 and 10, and anything that looked wrong. Claude reads the extract directly; you don't need to paste it.

## What the log should show

For reference, when reviewing the extract yourself:

| Step | Expected log lines |
|------|--------------------|
| 1 | `Skipping source load for scene '...' - no scene source folder is open` for each change; no `Loaded ... sources` lines |
| 2 | `SourcesDynamicFolder opened - loading sources`, `Cache miss for audio input scene membership`, `Loaded N sources and M audio sources ... in X ms`, and, if tiles showed, `Rendering loading tile 0/1 ... at WxH` |
| 3 | One `Loaded ...` line per change, each with `Cache hit for audio input scene membership` |
| 4 | `SourcesDynamicFolder closed`, then `Skipping source load` for the scene change |
| 5 | `SceneAudioSourcesDynamicFolder opened - loading audio sources`, then `Loaded ...` |
| 6 | `Cleared cached audio input scene membership (scene item created in '...')`, a cache miss and a new `Loaded ...`; the same for the removal |
| 7 | Disconnect; on reconnect `SceneAudioSourcesDynamicFolder open when OBS connected - loading audio sources` and a `Loaded ...` line. If OBS was still starting: `OBS is not ready for the initial state load yet`, then `Initial state loaded once OBS was ready`, with no stats errors |
| 8 | `StatsDynamicFolder opened`, `polling started`, `polled stats in X ms (first viewer)`, timer polls, then `closed` and `polling stopped` |
| 9 | The same for `StreamStatsDynamicFolder` |
| 10 | `Stats summary button visible - requesting stats` and `polling started`; after moving away, `no longer visible - releasing stats` and `polling stopped` within about 6 seconds |
