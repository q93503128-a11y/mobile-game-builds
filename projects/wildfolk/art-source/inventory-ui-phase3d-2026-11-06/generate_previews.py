#!/usr/bin/env python3
"""Build a Core Keeper-first Lowfire inventory reconstruction from current project assets."""
from __future__ import annotations

import html
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = Path(__file__).resolve().parent
SCREENS = SOURCE / "screens"
GRAPHICS = SOURCE / "graphics"
VALIDATION = ROOT / "validation" / "inventory-ui-phase3d-2026-11-06"
PREVIEWS = VALIDATION / "previews"
ART = ROOT / "client" / "Assets" / "Wildfolk" / "Art"
DATA = ROOT / "client" / "Assets" / "Wildfolk" / "Data"
RESOURCES = ROOT / "client" / "Assets" / "Wildfolk" / "Resources"
BACKGROUND = ROOT / "docs" / "images" / "lowfire-player-world-armor.png"
AVATAR_CAPTURE = BACKGROUND
W, H = 844, 390

# A layout fixture; no user save is read. IDs and quantities use current Lowfire assets.
HOTBAR = [
    ("wood", 10), ("iron_sword", 1), ("torch", 8), ("waterskin", 1),
    ("berry", 12), ("iron_axe", 1), ("arrow", 18), ("mushroom_stew", 1),
    ("iron_pickaxe", 1),
]
STORAGE = [
    ("wood", 24), ("stone", 18), ("plant_fiber", 12), ("coal", 6),
    ("medicinal_herb", 4), ("medicinal_gland", 2), ("steel_head", 1),
    ("leather_body", 1), ("iron_ore", 8), ("copper_ore", 7), ("tin_ore", 5),
    ("iron_ingot", 4), ("leather", 6), ("mushroom", 5), ("berry", 6),
    ("iron_spear", 1), ("bow", 1),
] + [(None, 0)] * 10
EQUIPPED = {"Head": "iron_head", "Body": "iron_body", "Utility": "lantern", "Accessory": None}
CRAFT_IDS = ("campfire", "torch", "workbench", "empty_cup", "field_dressing", "healing_salve")

INK = "#20140d"
WOOD_DARK = "#2c1b11"
WOOD = "#89572f"
WOOD_LIT = "#be8750"
WOOD_EDGE = "#d0a06a"
WOOD_SHADOW = "#170e09"
SLOT = "#382518"
SLOT_INNER = "#281a11"
PAPER = "#f0d5a7"
MUTED = "#c6a77b"
EMBER = "#e49150"
MOSS = "#84ad66"
RED = "#c37052"


def esc(value: object) -> str:
    return html.escape(str(value), quote=True)


def parse(pattern: str, content: str, fallback: str = "") -> str:
    found = re.search(pattern, content, re.M)
    return found.group(1).strip() if found else fallback


def item_catalog() -> dict[str, dict[str, object]]:
    guid_to_art: dict[str, Path] = {}
    for meta in ART.glob("*.png.meta"):
        guid = parse(r"^guid: ([0-9a-f]+)$", meta.read_text(encoding="utf-8", errors="ignore"))
        if guid:
            guid_to_art[guid] = meta.with_suffix("")
    result: dict[str, dict[str, object]] = {}
    for asset in DATA.glob("*.asset"):
        raw = asset.read_text(encoding="utf-8", errors="ignore")
        if "Wildfolk.Runtime::Wildfolk.ItemDefinition" not in raw:
            continue
        item_id = parse(r"^  Id: ([^\r\n]+)$", raw)
        icon_guid = parse(r"^  Icon: \{fileID: \d+, guid: ([0-9a-f]+)", raw)
        if item_id and icon_guid in guid_to_art:
            result[item_id] = {
                "icon": guid_to_art[icon_guid],
                "max": int(parse(r"^  MaxStack: (\d+)$", raw, "1")),
                "path": asset,
            }
    return result


def localization() -> dict[str, list[str]]:
    raw = json.loads((RESOURCES / "Localization.json").read_text(encoding="utf-8"))
    return {row["Key"]: row["Values"] for row in raw["Entries"]}


def name_for(loc: dict[str, list[str]], item_id: str, catalog: dict[str, dict[str, object]]) -> str:
    text = loc.get(f"item.{item_id}.name")
    if text and len(text) > 1:
        return text[1]
    return parse(r"^  DisplayName: ([^\r\n]+)$", Path(catalog[item_id]["path"]).read_text(errors="ignore"), item_id)


def recipes() -> dict[str, dict[str, object]]:
    guid_to_item: dict[str, str] = {}
    for asset in DATA.glob("*.asset"):
        if "Wildfolk.Runtime::Wildfolk.ItemDefinition" not in asset.read_text(encoding="utf-8", errors="ignore"):
            continue
        meta = asset.with_suffix(asset.suffix + ".meta")
        if not meta.exists():
            continue
        guid = parse(r"^guid: ([0-9a-f]+)$", meta.read_text(encoding="utf-8", errors="ignore"))
        item_id = parse(r"^  Id: ([^\r\n]+)$", asset.read_text(encoding="utf-8", errors="ignore"))
        if guid and item_id:
            guid_to_item[guid] = item_id
    output: dict[str, dict[str, object]] = {}
    for asset in DATA.glob("*Recipe.asset"):
        raw = asset.read_text(encoding="utf-8", errors="ignore")
        if "Wildfolk.Runtime::Wildfolk.RecipeDefinition" not in raw:
            continue
        recipe_id = parse(r"^  Id: ([^\r\n]+)$", raw)
        result_guid = parse(r"^  Result: \{fileID: \d+, guid: ([0-9a-f]+)", raw)
        station = int(parse(r"^  RequiredStation: (\d+)$", raw, "-1"))
        ingredients = []
        for guid, count in re.findall(r"^  - Item: \{fileID: \d+, guid: ([0-9a-f]+).*?\n    Count: (\d+)", raw, re.M):
            ingredients.append((guid_to_item.get(guid, ""), int(count)))
        if recipe_id:
            output[recipe_id] = {
                "result": guid_to_item.get(result_guid, ""),
                "station": station,
                "ingredients": ingredients,
                "path": asset,
            }
    return output


def relative(source_svg: Path, target: Path) -> str:
    import os
    return Path(os.path.relpath(target, source_svg.parent)).as_posix()


def image(source_svg: Path, target: Path, x: float, y: float, w: float, h: float,
          opacity: float = 1, preserve: str = "xMidYMid meet", extra: str = "") -> str:
    return (f'<image href="{esc(relative(source_svg, target))}" x="{x:g}" y="{y:g}" '
            f'width="{w:g}" height="{h:g}" preserveAspectRatio="{preserve}" '
            f'opacity="{opacity:g}" {extra}/>')


def txt(x: float, y: float, value: object, size: float, fill: str = PAPER, weight: int = 650,
        anchor: str = "start", extra: str = "") -> str:
    return (f'<text x="{x:g}" y="{y:g}" font-family="Lowfire" font-size="{size:g}" '
            f'font-weight="{weight}" fill="{fill}" text-anchor="{anchor}" {extra}>'
            f'{esc(value)}</text>')


def wood_panel(out: list[str], x: float, y: float, w: float, h: float, name: str = "panel") -> None:
    # Original beveled plank frame with uneven shoulder and restrained carved grain.
    d = (f'M{x+7:g} {y+1:g} L{x+w-10:g} {y:g} L{x+w-1:g} {y+8:g} '
         f'L{x+w:g} {y+h-7:g} L{x+w-8:g} {y+h:g} L{x+8:g} {y+h-1:g} '
         f'L{x:g} {y+h-9:g} L{x+1:g} {y+8:g} Z')
    out.extend([
        f'<path d="{d}" fill="url(#woodFace)" stroke="{WOOD_SHADOW}" stroke-width="2.2" filter="url(#shadow)"/>',
        f'<path d="M{x+5:g} {y+7:g} L{x+11:g} {y+4:g} H{x+w-12:g} L{x+w-5:g} {y+10:g} '
        f'V{y+h-11:g} L{x+w-12:g} {y+h-5:g} H{x+12:g} L{x+5:g} {y+h-12:g} Z" '
        'fill="url(#plankRun)" stroke="#cf9a60" stroke-width=".9"/>',
        f'<path d="M{x+7:g} {y+8:g} H{x+w-7:g} V{y+h-8:g} H{x+7:g} Z" fill="url(#woodgrain)" opacity=".8"/>',
        f'<path d="M{x+8:g} {y+10:g} H{x+w-10:g}" fill="none" stroke="#e0b47a" stroke-width="1.1" opacity=".78"/>',
        f'<path d="M{x+8:g} {y+h-10:g} H{x+w-10:g}" fill="none" stroke="#382216" stroke-width="1.1" opacity=".85"/>',
    ])
    for row in range(1, max(2, int(h // 28))):
        yy = y + 10 + row * ((h - 20) / max(1, int(h // 28)))
        if yy < y + h - 9:
            out.append(f'<path d="M{x+10:g} {yy:g} H{x+w-10:g}" stroke="#3f2819" stroke-width=".55" opacity=".32"/>')
    # Short organic grain strokes, not a tiled rectangle texture.
    for i in range(4):
        gx = x + 18 + ((i * 47) % max(1, int(w - 50)))
        gy = y + 15 + ((i * 19) % max(1, int(h - 28)))
        out.append(f'<path d="M{gx:g} {gy:g} q7 -2 13 0 t12 0" fill="none" stroke="#342014" stroke-width=".6" opacity=".32"/>')
    if w > 90 and h > 60:
        for nx, ny in ((x+9, y+10), (x+w-10, y+10), (x+9, y+h-10), (x+w-10, y+h-10)):
            out.extend([
                f'<circle cx="{nx:g}" cy="{ny:g}" r="2.1" fill="#432a19" stroke="#d7a76b" stroke-width=".65"/>',
                f'<path d="M{nx-1:g} {ny:g} l2 .4" stroke="#f0c58a" stroke-width=".45" opacity=".7"/>',
            ])


def plaque(out: list[str], x: float, y: float, w: float, label: str, size: float = 10) -> None:
    h = 20
    d = (f'M{x+5:g} {y+1:g} H{x+w-5:g} L{x+w:g} {y+5:g} V{y+h-4:g} '
         f'L{x+w-5:g} {y+h:g} H{x+5:g} L{x:g} {y+h-4:g} V{y+5:g} Z')
    out.extend([
        f'<path d="{d}" fill="url(#raisedWood)" stroke="{WOOD_SHADOW}" stroke-width="1.3" filter="url(#shadowLite)"/>',
        f'<path d="M{x+5:g} {y+4:g} H{x+w-6:g}" stroke="#e1b77f" stroke-width=".8" opacity=".7"/>',
        txt(x+w/2, y+14, label, size, PAPER, 800, "middle"),
    ])


def item_slot(out: list[str], source_svg: Path, catalog: dict[str, dict[str, object]],
              item_id: str | None, count: int, x: float, y: float, w: float, h: float,
              state: str = "normal", hotbar_index: int | None = None) -> None:
    palette = {
        "normal": ("#52351f", "#d1a06a", "#291a10"),
        "selected": ("#a5582c", "#f2c176", "#422317"),
        "target": ("#536d3e", "#a7cd79", "#243520"),
        "pressed": ("#2a1b12", "#98704d", "#1b110b"),
        "disabled": ("#51473c", "#8f806a", "#342d25"),
    }
    outer, rim, face = palette[state]
    dy = 1 if state == "pressed" else 0
    points = (f'M{x+5:g} {y+dy:g} H{x+w-5:g} L{x+w:g} {y+5+dy:g} '
              f'V{y+h-5+dy:g} L{x+w-5:g} {y+h+dy:g} H{x+5:g} '
              f'L{x:g} {y+h-5+dy:g} V{y+5+dy:g} Z')
    inner = (f'M{x+4:g} {y+5+dy:g} H{x+w-4:g} V{y+h-5+dy:g} '
             f'H{x+4:g} Z')
    out.extend([
        f'<path d="{points}" fill="{outer}" stroke="#1b100a" stroke-width="1.35"/>',
        f'<path d="M{x+2:g} {y+5+dy:g} L{x+6:g} {y+2+dy:g} H{x+w-7:g} L{x+w-2:g} {y+6+dy:g}" '
        f'fill="none" stroke="{rim}" stroke-width="1.35"/>',
        f'<path d="{inner}" fill="{face}" stroke="#180f09" stroke-width=".8"/>',
        f'<path d="M{x+6:g} {y+h-7+dy:g} q7 -2 13 0 t12 0" fill="none" stroke="#9e7146" stroke-width=".55" opacity=".23"/>',
        f'<path d="M{x+w-11:g} {y+10+dy:g} v3 m-2 0 h4" stroke="#d1a06a" stroke-width=".65" opacity=".42"/>',
    ])
    if state == "selected":
        out.append(f'<path d="{points}" fill="none" stroke="#f6c976" stroke-width="1.5" filter="url(#amberGlow)"/>')
        out.append(f'<path d="M{x+5:g} {y+4+dy:g} H{x+w-6:g}" stroke="#ffe0a5" stroke-width=".8"/>')
    elif state == "target":
        out.append(f'<path d="{points}" fill="none" stroke="#a9d785" stroke-width="1.5" stroke-dasharray="3 1.5"/>')
    elif state == "disabled":
        out.append(f'<path d="{points}" fill="#80725f" opacity=".28"/>')
    if item_id:
        icon = Path(catalog[item_id]["icon"])
        scale = .70
        iw, ih = min(w*scale, h*.72), min(w*scale, h*.72)
        out.append(image(source_svg, icon, x+(w-iw)/2, y+(h-ih)/2-1+dy, iw, ih))
        if count > 1:
            count_text = str(count)
            badge_w = max(13, 5.4*len(count_text)+5)
            bx, by = x+w-badge_w-1, y+h-12+dy
            out.extend([
                f'<path d="M{bx:g} {by+1:g} H{x+w-2:g} V{y+h-2+dy:g} H{bx:g} Z" fill="#17100b" opacity=".92"/>',
                f'<text x="{x+w-3:g}" y="{y+h-3+dy:g}" font-family="Lowfire" font-size="9.2" font-weight="800" '
                f'fill="#fff0ce" text-anchor="end" stroke="#1b110b" stroke-width="1.5" paint-order="stroke">{count}</text>',
            ])
    if hotbar_index is not None:
        out.append(f'<text x="{x+4:g}" y="{y+10+dy:g}" font-family="Lowfire" font-size="7.6" font-weight="800" '
                   f'fill="#f5dbaf" stroke="#22160d" stroke-width="1.2" paint-order="stroke">{hotbar_index}</text>')


def action_button(out: list[str], x: float, y: float, w: float, h: float, label: str,
                  state: str = "primary", font_size: float = 9) -> None:
    fill = {"primary": "url(#buttonFire)", "quiet": "url(#buttonWood)",
            "pressed": "url(#buttonDown)", "disabled": "url(#buttonDisabled)"}[state]
    stroke = {"primary": "#452615", "quiet": "#352215", "pressed": "#22160d", "disabled": "#70604f"}[state]
    dy = 1 if state == "pressed" else 0
    d = (f'M{x+6:g} {y+dy:g} H{x+w-6:g} L{x+w:g} {y+6+dy:g} V{y+h-6+dy:g} '
         f'L{x+w-5:g} {y+h+dy:g} H{x+5:g} L{x:g} {y+h-5+dy:g} V{y+6+dy:g} Z')
    out.extend([
        f'<path d="{d}" fill="{fill}" stroke="{stroke}" stroke-width="1.4" filter="url(#shadowLite)"/>',
        f'<path d="M{x+7:g} {y+4+dy:g} H{x+w-8:g}" stroke="#f0c68c" stroke-width=".85" opacity=".75"/>',
        txt(x+w/2, y+h/2+font_size*.34+dy, label, font_size, PAPER if state=="primary" else "#f2d7aa", 800, "middle"),
    ])


def close_button(out: list[str]) -> None:
    # Keep the existing inventory exit path in the new spatial composition.
    out.extend([
        '<circle cx="812" cy="29" r="19" fill="#24170e" stroke="#170e09" stroke-width="1.4" filter="url(#shadowLite)"/>',
        '<circle cx="812" cy="28" r="15" fill="url(#raisedWood)" stroke="#e0b77f" stroke-width="1.1"/>',
        '<circle cx="812" cy="28" r="11" fill="url(#buttonWood)" stroke="#55361f" stroke-width=".8"/>',
        '<path d="M808 24 L816 32 M816 24 L808 32" stroke="#f0d5a7" stroke-width="2.2" stroke-linecap="round"/>',
    ])


def icon_glyph(kind: str, cx: float, cy: float, color: str, size: float = 14) -> str:
    if kind == "drop":
        d = (f'M{cx:g} {cy-size/2:g} C{cx-size*.08:g} {cy-size*.08:g} {cx-size*.42:g} {cy+size*.1:g} '
             f'{cx-size*.42:g} {cy+size*.3:g} a{size*.42:g} {size*.42:g} 0 1 0 {size*.84:g} 0 '
             f'C{cx+size*.42:g} {cy+size*.1:g} {cx+size*.08:g} {cy-size*.08:g} {cx:g} {cy-size/2:g} Z')
        return f'<path d="{d}" fill="none" stroke="{color}" stroke-width="1.4" stroke-linejoin="round"/>'
    if kind == "split":
        return (f'<path d="M{cx-size*.48:g} {cy-size*.2:g} H{cx+size*.36:g} m-3 -3 3 3-3 3 '
                f'M{cx+size*.48:g} {cy+size*.2:g} H{cx-size*.36:g} m3 -3-3 3 3 3" fill="none" '
                f'stroke="{color}" stroke-width="1.25" stroke-linecap="round" stroke-linejoin="round"/>')
    if kind == "drag":
        return (f'<path d="M{cx:g} {cy-size*.4:g} v{size*.8:g} m-{size*.24:g} -{size*.3:g} {size*.24:g} -{size*.24:g} {size*.24:g} {size*.24:g} '
                f'M{cx-size*.25:g} {cy:g} h{size*.5:g} m-{size*.2:g} -{size*.2:g} {size*.2:g} {size*.2:g}-{size*.2:g} {size*.2:g} '
                f'M{cx:g} {cy-size*.5:g} q{-size*.15:g} {size*.12:g} 0 {size*.22:g} q{size*.15:g} {-size*.1:g} 0 {-size*.22:g}Z" '
                f'fill="none" stroke="{color}" stroke-width="1.15" stroke-linecap="round" stroke-linejoin="round"/>')
    return f'<circle cx="{cx:g}" cy="{cy:g}" r="{size*.28:g}" fill="none" stroke="{color}" stroke-width="1.2"/>'


def avatar(out: list[str], source_svg: Path, x: float, y: float, size: float) -> None:
    clip_id = f"avatar_{int(x)}_{int(y)}"
    scale = size/130
    out.extend([
        f'<defs><clipPath id="{clip_id}"><circle cx="{x+size/2:g}" cy="{y+size/2:g}" r="{size/2-2:g}"/></clipPath></defs>',
        f'<image href="{esc(relative(source_svg, AVATAR_CAPTURE))}" x="{x-910*scale:g}" y="{y-470*scale:g}" '
        f'width="{1920*scale:g}" height="{1080*scale:g}" preserveAspectRatio="none" clip-path="url(#{clip_id})"/>',
        f'<circle cx="{x+size/2:g}" cy="{y+size/2:g}" r="{size/2-1:g}" fill="none" stroke="#e0bd8b" stroke-width="2.1"/>',
    ])


def gear_socket(out: list[str], source_svg: Path, catalog: dict[str, dict[str, object]],
                item_id: str | None, label: str, x: float, y: float, size: float,
                state: str = "normal", label_position: str = "top") -> None:
    item_slot(out, source_svg, catalog, item_id, 1 if item_id else 0, x, y, size, size, state)
    if label_position == "left":
        lx, ly, anchor = x-5, y+size/2+3, "end"
    elif label_position == "top":
        lx, ly, anchor = x+size/2, y-3, "middle"
    else:
        lx, ly, anchor = x+size/2, y+size+10, "middle"
    out.append(txt(lx, ly, label, 8, "#f2d9ad", 750, anchor, 'stroke="#27190f" stroke-width="1.3" paint-order="stroke"'))


def frame(source_svg: Path, title: str) -> list[str]:
    font = relative(source_svg, RESOURCES / "NotoSansKR.ttf")
    return [
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}">',
        '<defs>',
        '<linearGradient id="woodFace" x1="0" y1="0" x2=".8" y2="1"><stop stop-color="#b17a44"/><stop offset=".42" stop-color="#8a592f"/><stop offset="1" stop-color="#644022"/></linearGradient>',
        '<linearGradient id="raisedWood" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#c08a50"/><stop offset=".45" stop-color="#986436"/><stop offset="1" stop-color="#704829"/></linearGradient>',
        '<linearGradient id="plankRun" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#a46e3c"/><stop offset=".32" stop-color="#986136"/><stop offset=".34" stop-color="#7e4e2b"/><stop offset=".68" stop-color="#82512d"/><stop offset=".7" stop-color="#744728"/><stop offset="1" stop-color="#6e4326"/></linearGradient>',
        '<linearGradient id="slotFace" x1="0" y1="0" x2=".3" y2="1"><stop stop-color="#4a301e"/><stop offset=".42" stop-color="#312015"/><stop offset="1" stop-color="#21150e"/></linearGradient>',
        '<linearGradient id="buttonFire" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#d58143"/><stop offset=".46" stop-color="#b46332"/><stop offset="1" stop-color="#874725"/></linearGradient>',
        '<linearGradient id="buttonWood" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#a57242"/><stop offset="1" stop-color="#704727"/></linearGradient>',
        '<linearGradient id="buttonDown" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#694327"/><stop offset="1" stop-color="#4b2d1b"/></linearGradient>',
        '<linearGradient id="buttonDisabled" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#756451"/><stop offset="1" stop-color="#554737"/></linearGradient>',
        '<radialGradient id="night" cx="50%" cy="48%" r="75%"><stop offset="0" stop-color="#11150f" stop-opacity=".34"/><stop offset="1" stop-color="#090b08" stop-opacity=".66"/></radialGradient>',
        '<pattern id="woodgrain" width="44" height="24" patternUnits="userSpaceOnUse"><path d="M1 5 C9 2 15 7 24 4 S37 3 43 5 M7 14 C17 12 22 16 31 14 S37 12 43 14 M2 21 Q11 18 17 21" fill="none" stroke="#392416" stroke-width=".75" opacity=".28"/><path d="M0 9 Q7 8 13 9" fill="none" stroke="#e4b77d" stroke-width=".55" opacity=".24"/></pattern>',
        '<filter id="shadow" x="-25%" y="-25%" width="150%" height="165%"><feGaussianBlur in="SourceAlpha" stdDeviation="2.5"/><feOffset dy="2.2"/><feComponentTransfer><feFuncA type="linear" slope=".43"/></feComponentTransfer><feMerge><feMergeNode/><feMergeNode in="SourceGraphic"/></feMerge></filter>',
        '<filter id="shadowLite" x="-20%" y="-20%" width="140%" height="150%"><feGaussianBlur in="SourceAlpha" stdDeviation="1.4"/><feOffset dy="1.2"/><feComponentTransfer><feFuncA type="linear" slope=".3"/></feComponentTransfer><feMerge><feMergeNode/><feMergeNode in="SourceGraphic"/></feMerge></filter>',
        '<filter id="amberGlow" x="-35%" y="-35%" width="170%" height="170%"><feGaussianBlur stdDeviation="1.4" result="b"/><feMerge><feMergeNode in="b"/><feMergeNode in="SourceGraphic"/></feMerge></filter>',
        f'<style>@font-face{{font-family:Lowfire;src:url("{esc(font)}") format("truetype");font-weight:100 900}} text{{font-family:Lowfire,"Malgun Gothic","Segoe UI",sans-serif}}</style>',
        '</defs>',
        image(source_svg, BACKGROUND, 0, 0, W, H, preserve="xMidYMid slice"),
        '<rect width="844" height="390" fill="url(#night)"/>',
    ]


def item_name_for(loc: dict[str, list[str]], item_id: str, catalog: dict[str, dict[str, object]]) -> str:
    return name_for(loc, item_id, catalog)


def draw_inventory(out: list[str], source_svg: Path, catalog: dict[str, dict[str, object]],
                   selected_item: str | None = None, selected_count: int | None = None,
                   equipment_candidate: str | None = None) -> None:
    # Bottom compartments follow the reference's quick row over the backpack grid.
    wood_panel(out, 147, 199, 442, 51, "quick")
    wood_panel(out, 147, 253, 442, 132, "backpack")
    plaque(out, 156, 183, 94, "핫바 · 9", 8.8)
    plaque(out, 156, 235, 112, "가방 · 보관 27", 8.8)
    slot_w, slot_h, gap = 48, 42, 1
    for i, (item_id, count) in enumerate(HOTBAR):
        item_slot(out, source_svg, catalog, item_id, count, 148+i*(slot_w+gap), 204, slot_w, 43,
                  "selected" if selected_item == item_id and i == 0 and selected_count is None else "normal",
                  hotbar_index=i+1)
    for i, (item_id, count) in enumerate(STORAGE):
        row, col = divmod(i, 9)
        shown_count = count
        state = "normal"
        if item_id == selected_item and i == 0 and selected_count is not None:
            shown_count, state = selected_count, "selected"
        elif item_id == equipment_candidate and item_id is not None:
            state = "selected"
        item_slot(out, source_svg, catalog, item_id, shown_count,
                  148+col*(slot_w+gap), 258+row*42, slot_w, 41, state)


def craft_panel(out: list[str], source_svg: Path, catalog: dict[str, dict[str, object]],
                recipe_data: dict[str, dict[str, object]], loc: dict[str, list[str]],
                item_counts: dict[str, int]) -> None:
    wood_panel(out, 147, 91, 149, 106, "craft")
    plaque(out, 185, 81, 74, "제작", 10)
    for i, recipe_id in enumerate(CRAFT_IDS):
        row, col = divmod(i, 3)
        x, y = 153+col*45, 104+row*44
        recipe = recipe_data[recipe_id]
        result_id = str(recipe["result"])
        if int(recipe["station"]) != 0 or not result_id:
            raise ValueError(f"Hand-crafting recipe does not match current source: {recipe_id}")
        if not all(item_counts.get(iid, 0) >= count for iid, count in recipe["ingredients"]):
            raise ValueError(f"Fixture cannot make the displayed hand-crafting recipe: {recipe_id}")
        item_slot(out, source_svg, catalog, result_id, 1, x, y, 43, 40, "normal")
        out.append(f'<circle cx="{x+38:g}" cy="{y+4:g}" r="2.7" fill="{MOSS}" stroke="#d4e6ad" stroke-width=".7"/>')


def equipment_panel(out: list[str], source_svg: Path, catalog: dict[str, dict[str, object]],
                    candidate: str | None = None) -> None:
    wood_panel(out, 383, 20, 208, 176, "equipment")
    plaque(out, 451, 10, 78, "장비 · 4", 10)
    # Original Core Keeper layout uses a paper doll surrounded by equipment sockets.
    # Lowfire has exactly four sockets, so no pants/pouch/other equipment slots are invented.
    out.append('<path d="M487 83 L428 93 M487 83 L546 93 M487 130 L487 144" fill="none" stroke="#d2a36c" stroke-width="1.1" opacity=".85"/>')
    out.append('<circle cx="487" cy="103" r="35" fill="#352216" stroke="#d0a06a" stroke-width="2.1" filter="url(#shadowLite)"/>')
    out.append('<circle cx="487" cy="103" r="30" fill="#33251a" stroke="#edc58f" stroke-width=".9"/>')
    avatar(out, source_svg, 459, 75, 56)
    gear_socket(out, source_svg, catalog, EQUIPPED["Head"], "머리", 467, 31, 40,
                "target" if candidate == "steel_head" else "normal", "left")
    gear_socket(out, source_svg, catalog, EQUIPPED["Body"], "몸", 529, 84, 40, "normal", "top")
    gear_socket(out, source_svg, catalog, EQUIPPED["Utility"], "도구", 467, 135, 40, "normal", "bottom")
    gear_socket(out, source_svg, catalog, EQUIPPED["Accessory"], "장신구", 405, 84, 40, "normal", "top")
    if candidate == "steel_head":
        # The selected bag item is a candidate; the connector maps it to the compatible socket.
        out.append('<path d="M478 307 C430 259 435 185 470 74" fill="none" stroke="#a6d27b" stroke-width="1.4" stroke-dasharray="4 3" marker-end="url(#arrowGreen)"/>')


def detail_card(out: list[str], source_svg: Path, catalog: dict[str, dict[str, object]],
                loc: dict[str, list[str]], state: str, defenses: tuple[int, int]) -> None:
    if state == "basic":
        # The Core Keeper screenshot's right pouch surface has no Lowfire equivalent.
        # Leave the world visible instead of manufacturing four pseudo slots.
        return
    wood_panel(out, 596, 101, 205, 108, "context")
    if state == "selected":
        item_id, count = "wood", 24
        out.append(image(source_svg, Path(catalog[item_id]["icon"]), 605, 110, 38, 38))
        out.extend([txt(649, 123, item_name_for(loc, item_id, catalog), 11, PAPER, 800),
                    txt(649, 140, f"{count} / {catalog[item_id]['max']} · 드래그 이동", 8, MUTED, 700)])
        action_button(out, 604, 157, 92, 44, "나누기", "quiet", 8.8)
        action_button(out, 700, 157, 92, 44, "취소", "quiet", 8.8)
    elif state == "equipment":
        item_id = "steel_head"
        out.append(image(source_svg, Path(catalog[item_id]["icon"]), 605, 109, 40, 40))
        out.extend([txt(653, 121, item_name_for(loc, item_id, catalog), 10.2, PAPER, 800),
                    txt(653, 137, f"방어력 {defenses[0]} → {defenses[1]}", 8.4, MUTED, 750),
                    txt(653, 152, "해제: 현재 장비 선택", 7.4, MUTED, 700)])
        action_button(out, 604, 157, 188, 44, "장착", "primary", 10)


def build_screen(catalog: dict[str, dict[str, object]], loc: dict[str, list[str]],
                 recipe_data: dict[str, dict[str, object]], item_counts: dict[str, int],
                 state: str, defenses: tuple[int, int]) -> str:
    file_name = {"basic": "basic-inventory.svg", "selected": "item-selected.svg",
                 "equipment": "equipment-equip.svg"}[state]
    source_svg = SCREENS / file_name
    out = frame(source_svg, state)
    close_button(out)
    craft_panel(out, source_svg, catalog, recipe_data, loc, item_counts)
    equipment_panel(out, source_svg, catalog, "steel_head" if state == "equipment" else None)
    draw_inventory(out, source_svg, catalog,
                   selected_item="wood" if state == "selected" else "steel_head" if state == "equipment" else None,
                   selected_count=24 if state == "selected" else None,
                   equipment_candidate="steel_head" if state == "equipment" else None)
    detail_card(out, source_svg, catalog, loc, state, defenses)

    if state == "selected":
        # Existing split action context; this is the item-action state before quantity confirmation.
        # The world-drop destination is intentionally absent until the resulting partial stack is held.
        pass
    elif state == "equipment":
        pass
    out.append('</svg>')
    return '\n'.join(out)


def ui_kit() -> str:
    path = GRAPHICS / "lowfire-reference-wood-kit.svg"
    # A separate material/state sheet, not an additional in-game screen.
    pieces = [
        f'<svg xmlns="http://www.w3.org/2000/svg" width="900" height="440" viewBox="0 0 900 440">',
        '<defs>',
        '<linearGradient id="woodFace" x1="0" y1="0" x2=".8" y2="1"><stop stop-color="#b17a44"/><stop offset=".42" stop-color="#8a592f"/><stop offset="1" stop-color="#644022"/></linearGradient>',
        '<linearGradient id="raisedWood" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#c08a50"/><stop offset=".45" stop-color="#986436"/><stop offset="1" stop-color="#704829"/></linearGradient>',
        '<linearGradient id="plankRun" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#a46e3c"/><stop offset=".32" stop-color="#986136"/><stop offset=".34" stop-color="#7e4e2b"/><stop offset=".68" stop-color="#82512d"/><stop offset=".7" stop-color="#744728"/><stop offset="1" stop-color="#6e4326"/></linearGradient>',
        '<linearGradient id="slotFace" x1="0" y1="0" x2=".3" y2="1"><stop stop-color="#4a301e"/><stop offset="1" stop-color="#21150e"/></linearGradient>',
        '<filter id="shadow" x="-20%" y="-20%" width="140%" height="155%"><feGaussianBlur in="SourceAlpha" stdDeviation="2"/><feOffset dy="2"/><feComponentTransfer><feFuncA type="linear" slope=".35"/></feComponentTransfer><feMerge><feMergeNode/><feMergeNode in="SourceGraphic"/></feMerge></filter>',
        '</defs>',
        '<rect width="900" height="440" fill="#182019"/>',
        '<text x="32" y="42" font-family="Segoe UI,sans-serif" font-size="17" font-weight="800" fill="#f0d5a7">LOWFIRE · CARVED WOOD INVENTORY KIT</text>',
        '<text x="32" y="62" font-family="Segoe UI,sans-serif" font-size="10" fill="#c6a77b">Plank relief, recessed wells, state edge; original vector surfaces for the reference reconstruction</text>',
    ]
    # Recreate a cropped showcase panel with the same production helpers.
    original = Path(__file__).resolve()
    wood_panel(pieces, 28, 86, 398, 322, "sample")
    plaque(pieces, 54, 76, 138, "패널 · 목재 프레임", 10)
    for i, state in enumerate(("normal", "selected", "target", "pressed", "disabled")):
        item_slot(pieces, original, {}, None, 0, 52+i*70, 132, 52, 52, state)
        state_label = "COMPATIBLE" if state == "target" else state.upper()
        pieces.append(f'<text x="{78+i*70:g}" y="196" font-family="Segoe UI,sans-serif" font-size="7.2" font-weight="700" fill="#f0d5a7" text-anchor="middle">{state_label}</text>')
    pieces.extend([
        '<text x="54" y="228" font-family="Segoe UI,sans-serif" font-size="10" font-weight="700" fill="#f0d5a7">CUT PLANK EDGE · RECESSED SLOT BED · STATE OUTLINE</text>',
        '<path d="M54 244 H390" stroke="#3b2517" stroke-width="1"/>',
        '<path d="M54 263 Q83 256 116 261 T180 261 M223 276 Q249 271 277 275 T390 275 M66 292 Q93 286 120 291" fill="none" stroke="#e4b77d" stroke-width="1.1" opacity=".38"/>',
        '<path d="M54 315 H390 M54 341 H390 M54 367 H390" stroke="#4a2e1c" stroke-width=".75" opacity=".58"/>',
        '<text x="54" y="395" font-family="Segoe UI,sans-serif" font-size="8.4" fill="#c6a77b">Warm wood is functional hierarchy, not rarity: selection is ember; valid drop target is leaf.</text>',
    ])
    # Action surface / states.
    wood_panel(pieces, 456, 86, 416, 322, "actions")
    plaque(pieces, 480, 76, 162, "동작 표면 · low profile", 10)
    action_button(pieces, 488, 132, 164, 50, "장착", "primary", 12)
    action_button(pieces, 675, 132, 168, 50, "나누기", "quiet", 12)
    action_button(pieces, 488, 208, 164, 50, "눌림", "pressed", 12)
    action_button(pieces, 675, 208, 168, 50, "비활성", "disabled", 12)
    pieces.extend([
        '<text x="488" y="292" font-family="Segoe UI,sans-serif" font-size="9" fill="#f0d5a7">Primary/secondary actions use the same carved face.</text>',
        '<text x="488" y="311" font-family="Segoe UI,sans-serif" font-size="9" fill="#c6a77b">Pressed sinks into the wood; disabled removes edge contrast.</text>',
        '<text x="488" y="330" font-family="Segoe UI,sans-serif" font-size="9" fill="#c6a77b">No rarity meaning is attached to slot colors or rim tones.</text>',
        '<text x="488" y="366" font-family="Segoe UI,sans-serif" font-size="8" fill="#dfc296">Source: Lowfire-specific SVG; no Core Keeper texture or icon copied.</text>',
        '</svg>',
    ])
    return '\n'.join(pieces)


def defense(item_id: str) -> int:
    item_raw = Path(item_catalog_cache[item_id]["path"]).read_text(encoding="utf-8", errors="ignore")
    armor_guid = parse(r"^  Armor: \{fileID: \d+, guid: ([0-9a-f]+)", item_raw)
    for asset in DATA.glob("*.asset"):
        raw = asset.read_text(encoding="utf-8", errors="ignore")
        if "Wildfolk.Runtime::Wildfolk.ArmorDefinition" not in raw:
            continue
        meta = asset.with_suffix(asset.suffix + ".meta")
        if meta.exists() and parse(r"^guid: ([0-9a-f]+)$", meta.read_text(encoding="utf-8", errors="ignore")) == armor_guid:
            return int(parse(r"^  Defense: (\d+)$", raw, "0"))
    raise RuntimeError(f"ArmorDefinition missing for {item_id}")


item_catalog_cache: dict[str, dict[str, object]] = {}


def main() -> None:
    for folder in (SCREENS, GRAPHICS, PREVIEWS, VALIDATION / "assets"):
        folder.mkdir(parents=True, exist_ok=True)
    for path in (BACKGROUND, AVATAR_CAPTURE, RESOURCES / "NotoSansKR.ttf"):
        if not path.exists():
            raise FileNotFoundError(path)
    catalog = item_catalog()
    global item_catalog_cache
    item_catalog_cache = catalog
    loc = localization()
    recipe_data = recipes()
    used_ids = {item for item, _ in HOTBAR + STORAGE if item}
    used_ids |= {item for item in EQUIPPED.values() if item}
    used_ids |= set(CRAFT_IDS) | {"steel_head"}
    missing = sorted(used_ids-catalog.keys())
    if missing:
        raise RuntimeError(f"Current ItemDefinition/icon missing: {missing}")
    if len(HOTBAR) != 9 or len(STORAGE) != 27 or len(EQUIPPED) != 4:
        raise RuntimeError("Fixture must preserve Hotbar 9, Storage 27, Equipment 4.")
    for recipe_id in CRAFT_IDS:
        if recipe_id not in recipe_data:
            raise RuntimeError(f"RecipeDefinition missing: {recipe_id}")
        result = recipe_data[recipe_id]["result"]
        if result not in catalog:
            raise RuntimeError(f"Recipe result has no current item icon: {recipe_id} => {result}")
    item_counts: dict[str, int] = {}
    for item, count in HOTBAR+STORAGE:
        if item:
            item_counts[item] = item_counts.get(item, 0)+count
    current_def = defense("iron_head")
    candidate_def = defense("steel_head")
    total_def = current_def + defense("iron_body")
    if (current_def, candidate_def, total_def) != (2, 3, 6):
        raise RuntimeError(f"Source armor values changed: current={current_def}, candidate={candidate_def}, total={total_def}")
    for state in ("basic", "selected", "equipment"):
        screen = SCREENS / {"basic":"basic-inventory.svg", "selected":"item-selected.svg", "equipment":"equipment-equip.svg"}[state]
        screen.write_text(build_screen(catalog, loc, recipe_data, item_counts, state, (current_def, candidate_def)), encoding="utf-8")
    (GRAPHICS / "lowfire-reference-wood-kit.svg").write_text(ui_kit(), encoding="utf-8")
    print("Generated 3 Core Keeper-first screens and the original wood UI kit.")
    print("Verified: Storage27, Hotbar9, Equipment4; 6 current hand recipes; Head defense 2 -> 3.")
    print("Hand craft recipe cards:")
    for recipe_id in CRAFT_IDS:
        recipe = recipe_data[recipe_id]
        print(f"  {recipe_id}: {recipe['result']} / station={recipe['station']} / {recipe['ingredients']}")


if __name__ == "__main__":
    main()
