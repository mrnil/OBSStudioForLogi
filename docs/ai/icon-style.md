# Icon Style

Rules for the button icons in `src/Resources/icons/`. Follow them when adding or changing an icon, so the set stays consistent.

## Canvas and Construction

- **Canvas:** `width="32" height="32" viewBox="0 0 32 32"`. The SDK scales the SVG to the key, so don't draw at other sizes.
- **Glyph area:** keep the glyph inside 4–28px on both axes.
- **No background shapes.** Code paints the key (`ButtonImageHelper.IconWithBackground`, or black by default), so a background rectangle fights the status colours.
- **Line:** 2px outlines with round caps and round joins. Corner radius 1–2px.
- **Markup:** shipped files contain filled `<path>` elements only: no `stroke`, `transform`, `<rect>`, `<defs>`, masks or clip paths. It's the lowest common denominator for the SDK's SVG renderer, which we can't inspect (`PluginApi.dll` is reference-only).
- **Folder entries** (`AudioMediaFolder`, `SceneFolder`) are the one deliberate exception to outlines. They use a solid folder silhouette with the glyph knocked out, so a folder reads differently from an action.

## Palette

| Colour | Role | Use |
|--------|------|-----|
| `#396CF6` | Active | Every enabled glyph |
| `#666666` | Disabled | Unavailable actions, audio meters for inputs that aren't live, placeholders |
| `#E5484D` | Muted | Muted audio |
| `#39B478` | Success | Brief confirmation, e.g. `ReplayBufferSaved` |
| `#FFFFFF` | On status | Only on coloured status keys (`Reconnect`) |

Don't introduce other colours. The VU meter bars are drawn in code and have their own green/yellow/red zones; they aren't icons.

## State Language

| State | How to show it |
|-------|----------------|
| Off | The on glyph plus the standard slash from (6,6) to (26,26), with a 2px gap cut either side of it. Families that have a natural stopped glyph use it instead: record dot / stop square, replay play / stop. |
| Unavailable | The same glyph recoloured to `#666666`. |
| Selected | The outline opens at the bottom-right and a tick sits in the gap. |
| Not selected | The plain glyph, with no badge. |
| Muted | The off treatment in `#E5484D`. |

Start and Stop buttons use their family's On and Off glyphs. When the action isn't available (Start while already running, Stop while idle) they use the grey `Disabled` variant. They never show the opposite state in full colour, because that looks pressable.

## Naming

`<Feature><State>[Disabled].svg`, named after what the picture shows, not the action a button performs:

- `RecordingOn.svg` / `RecordingOff.svg` / `RecordingPaused.svg`
- `StreamingOnDisabled.svg`: the Streaming On glyph in grey
- `SceneSelected.svg` / `SceneUnselected.svg`: singular feature names, matching `ProfileSelected.svg`

Several commands can share one file (Recording toggle and Recording Start both use `RecordingOn.svg`). Don't add a byte-identical copy under a new name.

## Workflow

1. **Draw with strokes.** Write the icon as 2px round-capped strokes on the 32px grid. For the off slash, clip the glyph with a path covering everything except a 6px-wide band along the slash, then draw the slash on top.
2. **Flatten.** Convert to filled paths with [picosvg](https://github.com/googlefonts/picosvg) (`pip install picosvg`, then `picosvg source.svg > Icon.svg`). It turns strokes into fills and bakes in clips and transforms. picosvg drops `width`/`height`, so put back `width="32" height="32"` on the root element.
3. **Make disabled variants by recolouring,** never by redrawing, so they can't drift from the enabled glyph:

    ```bash
    sed 's/#396CF6/#666666/g' RecordingOn.svg > RecordingOnDisabled.svg
    ```

4. **Check on black at key size** (48–120px) next to its family before committing.
5. **Embed only what code loads.** Add an `EmbeddedResource` entry to `src/OBSStudioForLogiPlugin.csproj` (kept in alphabetical order) for each icon referenced from code, and remove the entry when the last reference goes.

## Placeholders

These are in the folder but not embedded, because nothing loads them yet:

- `AudioFilterEnabled.svg`, `AudioFilterDisabled.svg`: reserved for the audio filter toggle in `TODO.md`
- `AudioDisabled.svg`, `FilterDisabled.svg`, `SceneDisabled.svg`, `SourceDisabled.svg`: offline placeholders for dynamic folders

Embed a placeholder when code starts using it.
