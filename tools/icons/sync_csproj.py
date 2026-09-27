"""Rewrite the csproj icon EmbeddedResource entries to exactly the icons code loads.

Scans src/**/*.cs for "<Name>.svg" string literals, fails if any of them is missing from
src/Resources/icons, and replaces the icon entries in the csproj with one per referenced
icon in alphabetical order. Icons that nothing references (placeholders) stay unembedded.

Usage (from the repo root):
    python tools/icons/sync_csproj.py
"""
import re
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
SRC = REPO / "src"
CSPROJ = SRC / "OBSStudioForLogiPlugin.csproj"
ICONS_DIR = SRC / "Resources" / "icons"

BS = "\\"
ENTRY = re.compile(
    r'[ \t]*<EmbeddedResource Include="Resources\\icons\\[^"]+">\s*'
    r"<LogicalName>[^<]+</LogicalName>\s*</EmbeddedResource>\r?\n"
)


def referenced_icons():
    used = set()
    for cs in SRC.rglob("*.cs"):
        if "obj" in cs.parts or "bin" in cs.parts:
            continue
        used.update(re.findall(r'"([A-Za-z]+\.svg)"', cs.read_text(encoding="utf-8")))
    return used


def main():
    icons = sorted(f.name for f in ICONS_DIR.glob("*.svg"))
    used = referenced_icons()

    missing = sorted(used - set(icons))
    if missing:
        raise SystemExit(f"Referenced from code but not in {ICONS_DIR.relative_to(REPO)}: {', '.join(missing)}")

    raw = CSPROJ.read_bytes().decode("utf-8")
    newline = "\r\n" if "\r\n" in raw else "\n"
    first = ENTRY.search(raw)
    if first is None:
        raise SystemExit(f"No icon EmbeddedResource entries found in {CSPROJ.name}")

    embedded = [name for name in icons if name in used]
    block = "".join(
        f'    <EmbeddedResource Include="Resources{BS}icons{BS}{name}">{newline}'
        f"      <LogicalName>Loupedeck.OBSStudioForLogiPlugin.Icons.{name}</LogicalName>{newline}"
        f"    </EmbeddedResource>{newline}"
        for name in embedded
    )
    CSPROJ.write_bytes((raw[: first.start()] + block + ENTRY.sub("", raw[first.start():])).encode("utf-8"))

    print(f"Embedded {len(embedded)} icons.")
    unembedded = [name for name in icons if name not in used]
    if unembedded:
        print(f"Not embedded (unreferenced): {', '.join(unembedded)}")


if __name__ == "__main__":
    main()
