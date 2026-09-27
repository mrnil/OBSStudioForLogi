"""Build the icon reference sheet: a self-contained HTML page with every icon inlined.

The page shows each icon on a black key as the device does, grouped by feature with what
uses it, plus the palette and state rules from docs/ai/icon-style.md and a before/after
record of the 2026-09 restyle (old files are read from git at BEFORE).

Any icon in src/Resources/icons that FAMILIES doesn't list is shown under "Not catalogued",
so add new icons to FAMILIES to give them a home and a usage note.

Usage (from the repo root):
    python tools/icons/build_sheet.py [--out FILE]
"""
import argparse
import html
import subprocess
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
ICONS = REPO / "src" / "Resources" / "icons"
DEFAULT_OUT = Path(__file__).resolve().parent / "out" / "icon-reference.html"
BEFORE = "043d847"  # last commit before the restyle

parser = argparse.ArgumentParser(description="Build the icon reference sheet.")
parser.add_argument("--out", type=Path, default=DEFAULT_OUT, help="output HTML file (default: tools/icons/out/icon-reference.html)")
out = parser.parse_args().out


def current(name):
    return (ICONS / f"{name}.svg").read_text(encoding="utf-8")


def old(name):
    return subprocess.run(
        ["git", "-C", str(REPO), "show", f"{BEFORE}:src/Resources/icons/{name}.svg"],
        capture_output=True, text=True, check=True, encoding="utf-8",
    ).stdout


def key(svg, bg=None, extra=""):
    style = f' style="--key:{bg}"' if bg else ""
    return f'<div class="key{extra}"{style}>{svg}</div>'


def esc(s):
    return html.escape(s, quote=True)


FAMILIES = [
    ("Recording", "Toggle, Start, Stop and Pause buttons share one glyph pair.", [
        ("RecordingOn", "Recording toggle while recording; Start; Pause toggle while not paused"),
        ("RecordingOff", "Recording toggle while idle; Stop"),
        ("RecordingPaused", "Pause toggle while paused"),
        ("RecordingOnDisabled", "Start while already recording"),
        ("RecordingOffDisabled", "Stop while idle"),
    ]),
    ("Streaming", "", [
        ("StreamingOn", "Streaming toggle while live; Start"),
        ("StreamingOff", "Streaming toggle while offline; Stop"),
        ("StreamingOnDisabled", "Start while already live"),
        ("StreamingOffDisabled", "Stop while offline"),
    ]),
    ("Virtual camera", "", [
        ("VirtualCameraOn", "Virtual camera toggle while on; Start"),
        ("VirtualCameraOff", "Virtual camera toggle while off; Stop"),
        ("VirtualCameraOnDisabled", "Start while already on"),
        ("VirtualCameraOffDisabled", "Stop while off"),
    ]),
    ("Replay buffer", "", [
        ("ReplayBufferOn", "Replay buffer toggle while running"),
        ("ReplayBufferOff", "Replay buffer toggle while stopped"),
        ("ReplayBufferSave", "Save replay"),
        ("ReplayBufferSaved", "Save replay, just after a save"),
    ]),
    ("Studio mode", "", [
        ("StudioModeOn", "Studio mode toggle while on"),
        ("StudioModeOff", "Studio mode toggle while off"),
        ("StudioModeTransition", "Transition preview to program"),
    ]),
    ("Scenes and collections", "", [
        ("SceneSelected", "Scene buttons and Scenes folder: the live scene"),
        ("SceneUnselected", "Scene buttons and Scenes folder: other scenes"),
        ("SceneFolder", "Scenes folder entry"),
        ("SceneCollectionSelected", "Scene collection buttons: the active collection"),
        ("SceneCollectionUnselected", "Scene collection buttons: other collections"),
    ]),
    ("Profiles and sources", "", [
        ("ProfileSelected", "Profile buttons: the active profile"),
        ("ProfileUnselected", "Profile buttons: other profiles"),
        ("SourceVisibilityOn", "Sources folder: visible source"),
        ("SourceVisibilityOff", "Sources folder: hidden source"),
    ]),
    ("Audio", "Meter tiles show bars while live and unmuted; these icons cover the other two states.", [
        ("AudioMediaFolder", "Audio folder entries"),
        ("AudioMeterMuted", "Audio meters: live but muted"),
        ("AudioMeterInactive", "Audio meters: input not live"),
    ]),
    ("Utility", "Reconnect is the one icon drawn on a coloured status key, so it uses white.", [
        ("Screenshot", "Screenshot"),
        ("Reconnect", "Reconnect: connected", "#008000"),
        ("Reconnect", "Reconnect: WebSocket server disabled", "#C87800"),
        ("Reconnect", "Reconnect: disconnected", "#800000"),
    ]),
    ("Placeholders", "In the icons folder but not embedded. Nothing loads them yet.", [
        ("AudioFilterEnabled", "Reserved for the audio filter toggle (TODO)"),
        ("AudioFilterDisabled", "Reserved for the audio filter toggle (TODO)"),
        ("AudioDisabled", "Offline placeholder"),
        ("FilterDisabled", "Offline placeholder"),
        ("SceneDisabled", "Offline placeholder"),
        ("SourceDisabled", "Offline placeholder"),
    ]),
]

# (old file, new file, why, key colour)
REDRAWN = [
    ("AudioMeterMuted", "AudioMeterMuted", "Outline speaker, standard slash with a real gap, palette red instead of #FF0000", None),
    ("AudioMeterInactive", "AudioMeterInactive", "Same glyph in the disabled grey; no black knock-out, so it works on any key colour", None),
    ("Reconnect", "Reconnect", "Redrawn on the 32px grid from a 2048px trace; white for the status key", "#800000"),
    ("StudioModeOn", "StudioModeOn", "Outline windows, matching the rest of the set", None),
    ("StudioModeOff", "StudioModeOff", "Dashed outline replaced by the standard off slash", None),
    ("ScenesSelected", "SceneSelected", "Pointer badge replaced by the shared tick", None),
    ("ScenesUnselected", "SceneUnselected", "No badge means not selected; the × read as an error", None),
    ("ScenesCollectionsSelected", "SceneCollectionSelected", "Pointer badge replaced by the shared tick", None),
    ("ScenesCollectionsUnselected", "SceneCollectionUnselected", "No badge means not selected", None),
    ("ProfileSelected", "ProfileSelected", "Tick no longer overlaps the page outline", None),
    ("RecordingStopDisabled", "RecordingOffDisabled", "Now exactly the Stop glyph in grey, rounded square included", None),
    ("VirtualCameraStart", "VirtualCameraOn", "Start used the recording dot; it now shows a camera", None),
    ("VirtualCameraStop", "VirtualCameraOff", "Stop used the recording square; it now shows a camera", None),
    ("StreamingToggleOff", "StreamingOnDisabled", "Start while live showed the live icon in full colour, so it looked pressable; it is now greyed like Recording", None),
]

RENAMED = [
    ("RecordingPause", "RecordingPaused"),
    ("RecordingStartDisabled", "RecordingOnDisabled"),
    ("RecordingStopDisabled", "RecordingOffDisabled"),
    ("StreamingToggleOff", "StreamingOn"),
    ("StreamingToggleOn", "StreamingOff"),
    ("VirtualCameraStartDisabled", "VirtualCameraOnDisabled"),
    ("VirtualCameraStopDisabled", "VirtualCameraOffDisabled"),
    ("ReplayBufferToggleStop", "ReplayBufferOn"),
    ("ReplayBufferToggleStart", "ReplayBufferOff"),
    ("ScenesSelected", "SceneSelected"),
    ("ScenesUnselected", "SceneUnselected"),
    ("ScenesCollectionsSelected", "SceneCollectionSelected"),
    ("ScenesCollectionsUnselected", "SceneCollectionUnselected"),
    ("ScenesFolder", "SceneFolder"),
]

REMOVED = [
    ("RecordingStart", "Duplicate of RecordingOn"),
    ("RecordingResume", "Duplicate of RecordingOn"),
    ("RecordingStop", "Duplicate of RecordingOff"),
    ("VirtualCameraStart", "Duplicate of RecordingOn; camera glyph used instead"),
    ("VirtualCameraStop", "Duplicate of RecordingOff; camera glyph used instead"),
    ("CurrentProfile", "Unused 90px filled tile"),
    ("CurrentScene", "Unused 90px filled tile"),
    ("CurrentSceneCollection", "Unused 90px filled tile"),
    ("AudioMixerMuted", "Unused; its name was the reverse of its picture"),
    ("AudioMixerUnmuted", "Unused; its name was the reverse of its picture"),
]

PALETTE = [
    ("#396CF6", "Active", "Every enabled glyph"),
    ("#666666", "Disabled", "Unavailable actions, non-live meters, placeholders"),
    ("#E5484D", "Muted", "Muted audio"),
    ("#39B478", "Success", "Brief confirmation, e.g. replay saved"),
    ("#FFFFFF", "On status", "Only on coloured status keys (Reconnect)"),
]

STATES = [
    ("VirtualCameraOn", "VirtualCameraOff", "Off", "Same glyph plus the diagonal slash, with a 2px gap either side. Families with a natural stopped glyph (record/stop, play/stop) use that instead."),
    ("RecordingOn", "RecordingOnDisabled", "Unavailable", "Same glyph, recoloured to #666666. Disabled files are generated from the enabled file, never drawn separately."),
    ("SceneUnselected", "SceneSelected", "Selected", "Outline opens at the bottom-right and a tick sits in the gap. Not selected is the plain glyph, with no badge."),
]


def tile(name, usage, bg=None):
    return (
        '<figure class="tile">'
        f'{key(current(name), bg)}'
        f'<figcaption><code>{esc(name)}.svg</code><span>{esc(usage)}</span></figcaption>'
        "</figure>"
    )


listed = {item[0] for _, _, items in FAMILIES for item in items}
unlisted = sorted(f.stem for f in ICONS.glob("*.svg") if f.stem not in listed)
families = FAMILIES
if unlisted:
    families = FAMILIES + [(
        "Not catalogued",
        "In the icons folder but not listed in tools/icons/build_sheet.py. Add them to FAMILIES with a usage note.",
        [(name, "No usage note yet") for name in unlisted],
    )]

parts = []
for title, note, items in families:
    slug = title.lower().replace(" ", "-")
    tiles = "".join(tile(*item) for item in items)
    note_html = f'<p class="note">{esc(note)}</p>' if note else ""
    parts.append(f'<section class="family" id="{slug}"><h3>{esc(title)}</h3>{note_html}<div class="tiles">{tiles}</div></section>')
families_html = "".join(parts)

redrawn_html = "".join(
    '<figure class="pair">'
    f'<div class="pair-keys">{key(old(o), bg)}<span class="arrow" aria-hidden="true">→</span>{key(current(n), bg)}</div>'
    f'<figcaption><div class="names"><code>{esc(o)}</code>{"" if o == n else f" → <code>{esc(n)}</code>"}</div><span>{esc(why)}</span></figcaption>'
    "</figure>"
    for o, n, why, bg in REDRAWN
)

renamed_html = "".join(f"<tr><td><code>{esc(o)}.svg</code></td><td><code>{esc(n)}.svg</code></td></tr>" for o, n in RENAMED)
removed_html = "".join(
    f'<li>{key(old(n), extra=" key-sm")}<div><code>{esc(n)}.svg</code><span>{esc(why)}</span></div></li>'
    for n, why in REMOVED
)
palette_html = "".join(
    f'<li><span class="swatch" style="--c:{c}"></span><div><strong>{esc(role)}</strong><code>{c}</code><span>{esc(use)}</span></div></li>'
    for c, role, use in PALETTE
)
states_html = "".join(
    f'<div class="state"><div class="state-keys">{key(current(a), extra=" key-sm")}{key(current(b), extra=" key-sm")}</div>'
    f"<div><strong>{esc(label)}</strong><p>{esc(text)}</p></div></div>"
    for a, b, label, text in STATES
)

count = len(list(ICONS.glob("*.svg")))
template = (Path(__file__).parent / "sheet_template.html").read_text(encoding="utf-8")
page = (
    template.replace("{{FAMILIES}}", families_html)
    .replace("{{REDRAWN}}", redrawn_html)
    .replace("{{RENAMED}}", renamed_html)
    .replace("{{REMOVED}}", removed_html)
    .replace("{{PALETTE}}", palette_html)
    .replace("{{STATES}}", states_html)
    .replace("{{COUNT}}", str(count))
)
out.parent.mkdir(parents=True, exist_ok=True)
out.write_text(page, encoding="utf-8", newline="\n")
print(f"Wrote {out} ({count} icons{f', {len(unlisted)} not catalogued' if unlisted else ''})")
