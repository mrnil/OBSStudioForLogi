# Icon Tools

Scripts for maintaining the button icons in `src/Resources/icons/`. The rules they follow are in [`docs/ai/icon-style.md`](../../docs/ai/icon-style.md).

| Script | What it does |
|--------|--------------|
| `generate_icons.py` | Draws the icons defined in it (2px strokes, clipped slash gaps), flattens them to fill-only paths with picosvg, and writes the grey `*Disabled` variants by recolouring their enabled icon |
| `sync_csproj.py` | Rewrites the icon `EmbeddedResource` entries in `src/OBSStudioForLogiPlugin.csproj` to exactly the icons code loads, and fails if code references an icon that doesn't exist |
| `build_sheet.py` | Builds `out/icon-reference.html`, a self-contained page showing every icon on a black key, grouped by feature, with the palette and state rules |

## Setup

Python 3.10 or later. Install picosvg into a virtual environment (only `generate_icons.py` needs it):

```bash
python -m venv tools/icons/.venv
tools/icons/.venv/Scripts/python -m pip install -r tools/icons/requirements.txt   # Windows
tools/icons/.venv/bin/python -m pip install -r tools/icons/requirements.txt       # macOS/Linux
```

Run the scripts from the repo root, using the venv's Python for `generate_icons.py`.

## Adding or changing an icon

1. **Draw it.** Add the glyph to `ICONS` in `generate_icons.py` using `strokes()`, or `slashed()` for an off state. If it needs a grey variant, add the enabled icon's name to `DISABLED`.
2. **Generate it.** Run `python tools/icons/generate_icons.py`. It regenerates every icon it defines, so `git diff src/Resources/icons` should show only the icon you changed. To try a design without touching the repo, pass `--out <dir>`.
3. **Reference it from code,** then run `python tools/icons/sync_csproj.py` to update the csproj.
4. **Check it.** Add the icon to `FAMILIES` in `build_sheet.py` with a usage note, run `python tools/icons/build_sheet.py`, and open `tools/icons/out/icon-reference.html`. Icons missing from `FAMILIES` appear under "Not catalogued".

Most of the set was exported from Figma and isn't defined in `generate_icons.py`; the script leaves those files alone. Move an icon into `ICONS` if you need to redraw it.
