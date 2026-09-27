"""Generate the drawn-from-source button icons and their grey Disabled variants.

Icons listed in ICONS are authored here as 2px round-capped strokes on the 32px grid,
with a clip path cutting the gap around the standard off slash. picosvg flattens each
one into the fill-only paths the plugin ships (see docs/ai/icon-style.md).

Icons listed in DISABLED are recoloured copies of an enabled icon, so the grey variant
can never drift from the glyph it greys out.

Icons not listed in either (most of the set, originally exported from Figma) are left
alone.

Usage (from the repo root):
    python tools/icons/generate_icons.py [--out DIR]
"""
import argparse
import re
from pathlib import Path

from picosvg.svg import SVG

REPO = Path(__file__).resolve().parents[2]
ICONS_DIR = REPO / "src" / "Resources" / "icons"

BLUE = "#396CF6"
GREY = "#666666"
RED = "#E5484D"
WHITE = "#FFFFFF"

STROKE = 'fill="none" stroke="{c}" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"'

# Standard off slash, top-left to bottom-right (the same line as VirtualCameraOff).
SLASH = "M6 6L26 26"
# Everything except a 6px-wide band along the slash: 2px of stroke plus a 2px gap each side.
SLASH_GAP_CLIP = "M0 0H32V32H0ZM1.757 6L6 1.757L30.243 26L26 30.243Z"

ROOT_ELEMENT = '<svg width="32" height="32" viewBox="0 0 32 32" fill="none" xmlns="http://www.w3.org/2000/svg">'


def svg(body, clip=None):
    defs = ""
    if clip:
        defs = f'<defs><clipPath id="gap"><path clip-rule="evenodd" d="{clip}"/></clipPath></defs>'
    return f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 32 32">{defs}{body}</svg>'


def strokes(color, *paths, clipped=False):
    attr = STROKE.format(c=color)
    clip = ' clip-path="url(#gap)"' if clipped else ""
    inner = "".join(f'<path d="{d}" {attr}/>' for d in paths)
    return f"<g{clip}>{inner}</g>"


def slashed(color, *paths):
    """The given glyph with the standard off slash cut through it."""
    return svg(strokes(color, *paths, clipped=True) + strokes(color, SLASH), clip=SLASH_GAP_CLIP)


def flatten(source):
    text = SVG.fromstring(source).topicosvg().tostring(pretty_print=True)
    # picosvg drops width/height; put back the 32x32 root element the rest of the set declares.
    text = re.sub(r"<svg [^>]*>", ROOT_ELEMENT, text, count=1)
    return text.replace("  <defs/>\n", "")


# --- Glyph geometry ---------------------------------------------------------

SPEAKER = "M7 13H11L16 8.5V23.5L11 19H7Z"
WAVE_INNER = "M19.83 13.17A4 4 0 0 1 19.83 18.83"
WAVE_OUTER = "M22.66 10.34A8 8 0 0 1 22.66 21.66"

# Selected variants open the outline at the bottom-right and put the tick in the gap.
SCENE_CLOSED = ["M7 8H23A1 1 0 0 1 24 9V20A1 1 0 0 1 23 21H7A1 1 0 0 1 6 20V9A1 1 0 0 1 7 8Z", "M6 12H24"]
SCENE_OPEN = ["M15 21H7A1 1 0 0 1 6 20V9A1 1 0 0 1 7 8H23A1 1 0 0 1 24 9V16", "M6 12H24"]
SCENE_TICK = "M20 21L22 23L26 19"

COLLECTION_BACK = "M5 20V8A1 1 0 0 1 6 7H23"
COLLECTION_CLOSED = [COLLECTION_BACK, "M10 11H26A1 1 0 0 1 27 12V23A1 1 0 0 1 26 24H10A1 1 0 0 1 9 23V12A1 1 0 0 1 10 11Z", "M9 15H27"]
COLLECTION_OPEN = [COLLECTION_BACK, "M16 24H10A1 1 0 0 1 9 23V12A1 1 0 0 1 10 11H26A1 1 0 0 1 27 12V17", "M9 15H27"]
COLLECTION_TICK = "M20 23L22 25L26 21"

PROFILE_LINES = ["M12 11H20", "M12 15H20", "M12 19H17"]
PROFILE_CLOSED = ["M9 7H23A1 1 0 0 1 24 8V24A1 1 0 0 1 23 25H9A1 1 0 0 1 8 24V8A1 1 0 0 1 9 7Z"] + PROFILE_LINES
PROFILE_OPEN = ["M15 25H9A1 1 0 0 1 8 24V8A1 1 0 0 1 9 7H23A1 1 0 0 1 24 8V16"] + PROFILE_LINES
PROFILE_TICK = "M20 22L22 24L26 20"

# Plug (left) meeting a socket (right), with cable stubs.
RECONNECT = [
    "M4 16H8",
    "M9 11H13A1 1 0 0 1 14 12V20A1 1 0 0 1 13 21H9A1 1 0 0 1 8 20V12A1 1 0 0 1 9 11Z",
    "M14 13.5H17",
    "M14 18.5H17",
    "M20 13.5V11H24L26 13V19L24 21H20V18.5",
    "M26 16H28",
]

# Studio mode: preview (back, top-left) and program (front, bottom-right) windows.
STUDIO_PREVIEW = "M13 19H7A1 1 0 0 1 6 18V9A1 1 0 0 1 7 8H19A1 1 0 0 1 20 9V13"
STUDIO_PROGRAM = ["M14 13H26A1 1 0 0 1 27 14V23A1 1 0 0 1 26 24H14A1 1 0 0 1 13 23V14A1 1 0 0 1 14 13Z", "M13 17H27"]

ICONS = {
    "AudioDisabled": svg(strokes(GREY, SPEAKER, "M21 13L27 19", "M27 13L21 19")),
    "AudioMeterInactive": slashed(GREY, SPEAKER, WAVE_INNER, WAVE_OUTER),
    "AudioMeterMuted": slashed(RED, SPEAKER, WAVE_INNER, WAVE_OUTER),
    "SceneSelected": svg(strokes(BLUE, *SCENE_OPEN, SCENE_TICK)),
    "SceneUnselected": svg(strokes(BLUE, *SCENE_CLOSED)),
    "SceneDisabled": svg(strokes(GREY, *SCENE_CLOSED)),
    "SceneCollectionSelected": svg(strokes(BLUE, *COLLECTION_OPEN, COLLECTION_TICK)),
    "SceneCollectionUnselected": svg(strokes(BLUE, *COLLECTION_CLOSED)),
    "ProfileSelected": svg(strokes(BLUE, *PROFILE_OPEN, PROFILE_TICK)),
    "ProfileUnselected": svg(strokes(BLUE, *PROFILE_CLOSED)),
    "Reconnect": svg(strokes(WHITE, *RECONNECT)),
    "StudioModeOn": svg(strokes(BLUE, STUDIO_PREVIEW, *STUDIO_PROGRAM)),
    "StudioModeOff": slashed(BLUE, STUDIO_PREVIEW, *STUDIO_PROGRAM),
}

# Enabled icons whose <Name>Disabled.svg is a grey recolour of them.
DISABLED = [
    "RecordingOn",
    "RecordingOff",
    "StreamingOn",
    "StreamingOff",
    "VirtualCameraOn",
    "VirtualCameraOff",
]


def write(path, text):
    # LF endings to match the rest of the repo, whatever the host OS.
    path.write_text(text, encoding="utf-8", newline="\n")


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--out", type=Path, default=ICONS_DIR, help="output directory (default: src/Resources/icons)")
    args = parser.parse_args()
    args.out.mkdir(parents=True, exist_ok=True)

    for name, source in ICONS.items():
        write(args.out / f"{name}.svg", flatten(source))
        print(f"drew     {name}.svg")

    # Recolour from the output directory when the enabled icon is there, else from the repo.
    for name in DISABLED:
        enabled = args.out / f"{name}.svg"
        if not enabled.exists():
            enabled = ICONS_DIR / f"{name}.svg"
        write(args.out / f"{name}Disabled.svg", enabled.read_text(encoding="utf-8").replace(BLUE, GREY))
        print(f"greyed   {name}Disabled.svg")


if __name__ == "__main__":
    main()
