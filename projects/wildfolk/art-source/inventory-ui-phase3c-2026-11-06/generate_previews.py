#!/usr/bin/env python3
"""Create Lowfire Phase 3C mobile inventory studies from current game assets."""
from __future__ import annotations

import html
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = Path(__file__).resolve().parent
SCREENS = SOURCE / "screens"
GRAPHICS = SOURCE / "graphics"
VALIDATION = ROOT / "validation" / "inventory-ui-phase3c-2026-11-06"
PREVIEWS = VALIDATION / "previews"
ART = ROOT / "client" / "Assets" / "Wildfolk" / "Art"
DATA = ROOT / "client" / "Assets" / "Wildfolk" / "Data"
RESOURCES = ROOT / "client" / "Assets" / "Wildfolk" / "Resources"
BACKGROUND = ROOT / "docs" / "images" / "lowfire-player-world-armor.png"
AVATAR_CAPTURE = ROOT / "docs" / "images" / "lowfire-player-world-armor.png"
WIDTH, HEIGHT = 844, 390

# A visual layout fixture only. IDs, counts, sprites, and gear values resolve to current data.
STORAGE = [
    ("wood", 24), ("stone", 18), ("berry", 6), ("mushroom", 5), ("plant_fiber", 12),
    ("leather", 6), ("iron_ore", 8), ("copper_ore", 7), ("tin_ore", 5),
    ("iron_ingot", 4), ("torch", 8), ("arrow", 18), ("bone_arrow", 12),
    ("bronze_arrow", 10), ("crossbow_bolt", 6), ("mushroom_stew", 2),
    ("roasted_mushroom", 4), ("raw_water_cup", 1), ("clean_water_cup", 1),
    ("waterskin", 1), ("bow", 1), ("iron_sword", 1), ("iron_axe", 1),
    ("iron_pickaxe", 1), ("steel_head", 1), ("leather_body", 1), ("wood_sword", 1),
]
HOTBAR = [
    ("wood", 10), ("iron_sword", 1), ("torch", 8), ("waterskin", 1),
    ("berry", 12), ("iron_axe", 1), ("arrow", 18), ("mushroom_stew", 1),
    ("iron_pickaxe", 1),
]
EQUIPPED = {"Head": "iron_head", "Body": "iron_body", "Utility": "lantern", "Accessory": None}
WIDTHS = {"slot": 44, "gap": 3, "grid": 9 * 44 + 8 * 3}

COLORS = {
    "ink": "#27372e", "ink2": "#415342", "cream": "#f0e6c9", "paper": "#e8dbb7",
    "paper2": "#d8c69d", "paper3": "#c6ad7c", "wood": "#77533a", "wood2": "#a77a4f",
    "moss": "#526a4f", "moss2": "#71815d", "ember": "#c96b43", "ember2": "#e29a61",
    "muted": "#7a8064", "red": "#a64c39", "shadow": "#1b2a23",
}


def esc(value: object) -> str:
    return html.escape(str(value), quote=True)


def parse(pattern: str, content: str, fallback: str = "") -> str:
    match = re.search(pattern, content, re.M)
    return match.group(1).strip() if match else fallback


def item_catalog() -> dict[str, dict[str, object]]:
    guid_to_art: dict[str, Path] = {}
    for meta in ART.glob("*.png.meta"):
        guid = parse(r"^guid: ([0-9a-f]+)$", meta.read_text(encoding="utf-8", errors="ignore"))
        if guid:
            guid_to_art[guid] = meta.with_suffix("")
    result: dict[str, dict[str, object]] = {}
    for path in DATA.glob("*.asset"):
        raw = path.read_text(encoding="utf-8", errors="ignore")
        if "Wildfolk.Runtime::Wildfolk.ItemDefinition" not in raw:
            continue
        item_id = parse(r"^  Id: ([^\r\n]+)$", raw)
        icon_guid = parse(r"^  Icon: \{fileID: \d+, guid: ([0-9a-f]+)", raw)
        if item_id and icon_guid in guid_to_art:
            result[item_id] = {
                "icon": guid_to_art[icon_guid],
                "max": int(parse(r"^  MaxStack: (\d+)$", raw, "1")),
                "water": int(parse(r"^  WaterCapacity: (\d+)$", raw, "0")),
                "path": path,
            }
    return result


def localization() -> dict[str, list[str]]:
    source = json.loads((RESOURCES / "Localization.json").read_text(encoding="utf-8"))
    return {row["Key"]: row["Values"] for row in source["Entries"]}


def ko(loc: dict[str, list[str]], key: str, fallback: str = "") -> str:
    values = loc.get(key)
    return values[1] if values and len(values) > 1 else fallback or key


def item_name(loc: dict[str, list[str]], item_id: str, catalog: dict[str, dict[str, object]]) -> str:
    raw = Path(catalog[item_id]["path"]).read_text(encoding="utf-8", errors="ignore")
    fallback = parse(r"^  DisplayName: ([^\r\n]+)$", raw, item_id)
    return ko(loc, f"item.{item_id}.name", fallback)


def rel(svg: Path, target: Path) -> str:
    return Path(__import__("os").path.relpath(target, svg.parent)).as_posix()


def image(svg: Path, target: Path, x: float, y: float, w: float, h: float,
          opacity: float = 1, preserve: str = "xMidYMid meet") -> str:
    return (f'<image href="{esc(rel(svg, target))}" x="{x:g}" y="{y:g}" '
            f'width="{w:g}" height="{h:g}" preserveAspectRatio="{preserve}" opacity="{opacity:g}"/>')


def text(x: float, y: float, value: object, size: float = 11, color: str = "ink",
         weight: int = 600, anchor: str = "start", letter: float = 0) -> str:
    return (f'<text x="{x:g}" y="{y:g}" font-size="{size:g}" fill="{COLORS.get(color, color)}" '
            f'font-weight="{weight}" text-anchor="{anchor}" letter-spacing="{letter:g}">{esc(value)}</text>')


def path(d: str, fill: str, stroke: str = "none", sw: float = 1, extra: str = "") -> str:
    return f'<path d="{d}" fill="{fill}" stroke="{stroke}" stroke-width="{sw:g}" {extra}/>'


def line(x1: float, y1: float, x2: float, y2: float, color: str = "paper3",
         width: float = 1, dash: str = "", opacity: float = 1) -> str:
    ds = f' stroke-dasharray="{dash}"' if dash else ""
    return (f'<path d="M{x1:g} {y1:g} L{x2:g} {y2:g}" fill="none" '
            f'stroke="{COLORS.get(color, color)}" stroke-width="{width:g}" opacity="{opacity:g}"{ds}/>')


def start(svg: Path, title: str, active: str) -> list[str]:
    font = rel(svg, RESOURCES / "NotoSansKR.ttf")
    campfire = ART / "campfire.png"
    out = [
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{WIDTH}" height="{HEIGHT}" viewBox="0 0 {WIDTH} {HEIGHT}">',
        '<defs>',
        '<linearGradient id="canvas" x1="0" y1="0" x2="0.9" y2="1"><stop stop-color="#f2e9ce"/><stop offset=".52" stop-color="#e7d9b4"/><stop offset="1" stop-color="#d9c79e"/></linearGradient>',
        '<linearGradient id="wood" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#a77b50"/><stop offset=".45" stop-color="#865d3c"/><stop offset="1" stop-color="#64452f"/></linearGradient>',
        '<linearGradient id="well" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#607354"/><stop offset="1" stop-color="#435840"/></linearGradient>',
        '<linearGradient id="wellSelected" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#795239"/><stop offset="1" stop-color="#553f31"/></linearGradient>',
        '<linearGradient id="creamButton" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#f4e8c6"/><stop offset="1" stop-color="#d9c59a"/></linearGradient>',
        '<linearGradient id="primaryButton" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#d97d4c"/><stop offset="1" stop-color="#aa5337"/></linearGradient>',
        '<pattern id="canvasGrain" width="28" height="20" patternUnits="userSpaceOnUse"><path d="M1 4h6M15 13h8M5 18h2" stroke="#b7a47c" stroke-width=".65" opacity=".25"/><circle cx="19" cy="5" r=".7" fill="#fbf1d3" opacity=".55"/></pattern>',
        '<pattern id="woodGrain" width="42" height="14" patternUnits="userSpaceOnUse"><path d="M0 4C8 2 15 6 23 4s12-2 19 0M6 10c7-2 13 1 20 0" fill="none" stroke="#3f3024" stroke-width=".7" opacity=".28"/></pattern>',
        '<filter id="shadow" x="-30%" y="-30%" width="160%" height="170%"><feGaussianBlur in="SourceAlpha" stdDeviation="3"/><feOffset dy="2"/><feComponentTransfer><feFuncA type="linear" slope=".24"/></feComponentTransfer><feMerge><feMergeNode/><feMergeNode in="SourceGraphic"/></feMerge></filter>',
        '<filter id="glow" x="-50%" y="-50%" width="200%" height="200%"><feGaussianBlur stdDeviation="2.4" result="blur"/><feColorMatrix in="blur" type="matrix" values="1 0 0 0 .65 0 1 0 0 .27 0 0 1 0 .08 0 0 0 .45 0" result="tint"/><feMerge><feMergeNode in="tint"/><feMergeNode in="SourceGraphic"/></feMerge></filter>',
        f'<style>@font-face{{font-family:Lowfire;src:url("{esc(font)}") format("truetype");font-weight:100 900}} text{{font-family:Lowfire,"Malgun Gothic","Segoe UI",sans-serif}}</style>',
        '</defs>',
        image(svg, BACKGROUND, 0, 0, WIDTH, HEIGHT, preserve="xMidYMid slice"),
        '<rect width="844" height="390" fill="#17241d" opacity=".32"/>',
        '<rect width="844" height="390" fill="url(#canvasGrain)" opacity=".13"/>',
    ]
    # A small stitched canvas tab, deliberately independent from the storage tray.
    out.extend([path('M24 8 Q28 5 38 7 L277 7 Q282 8 282 14 L282 46 Q278 51 270 51 L35 51 Q25 49 24 43 Z', 'url(#canvas)', '#354737', 1.6, 'filter="url(#shadow)"'),
                path('M30 12 Q38 10 44 11 H267 Q275 12 275 16 V41 Q273 46 265 46 H38 Q30 45 30 40 Z', 'none', '#b39c6b', .8, 'stroke-dasharray="2.2 3.1" opacity=".84"'),
                image(svg, campfire, 36, 14, 28, 28), text(70, 31, title, 16, "ink", 800),
                text(71, 44, "가방 27  ·  빠른 슬롯 9", 9.5, "muted", 650),
                path('M320 15 Q323 13 328 15 H540 Q546 16 546 22 V36 Q545 42 540 42 H327 Q320 41 320 36 Z', 'url(#creamButton)', '#8c7652', 1),
                text(433, 32, "정적 시안 · 조작 미검증", 9.5, "ink", 700, "middle")])
    # Purposeful screen switcher: only the two already existing inventory views.
    nav = [("bag", "가방", 584), ("gear", "장비", 672)]
    for key, label, x in nav:
        out.extend(nav_button(x, 8, 82, 44, label, key == active))
    # Existing close action; four-corner sewing marks make this unlike generic web cards.
    out.extend(button(772, 8, 48, 44, "×", "quiet", 22))
    return out


def tray(x: float, y: float, w: float, h: float, variant: str = "canvas") -> list[str]:
    # Asymmetric canvas-and-wood field-kit surface with stitched inset and corner toggles.
    fill = "url(#canvas)" if variant == "canvas" else "url(#wood)"
    line_color = COLORS["wood"] if variant == "canvas" else COLORS["paper3"]
    d = (f'M{x+10:g} {y+1:g} Q{x+17:g} {y-2:g} {x+29:g} {y+1:g} '
         f'L{x+w-12:g} {y+1:g} Q{x+w-2:g} {y+2:g} {x+w-2:g} {y+12:g} '
         f'L{x+w-1:g} {y+h-13:g} Q{x+w-2:g} {y+h-2:g} {x+w-13:g} {y+h-1:g} '
         f'L{x+15:g} {y+h:g} Q{x+3:g} {y+h-1:g} {x+2:g} {y+h-11:g} '
         f'L{x+1:g} {y+13:g} Q{x+1:g} {y+3:g} {x+10:g} {y+1:g} Z')
    return [path(d, fill, COLORS["ink"], 1.4, 'filter="url(#shadow)"'),
            path(d, 'url(#canvasGrain)' if variant == "canvas" else 'url(#woodGrain)', "none", 0, 'opacity=".52"'),
            f'<path d="M{x+8:g} {y+8:g} L{x+w-8:g} {y+8:g} L{x+w-7:g} {y+h-8:g} L{x+9:g} {y+h-7:g} Z" fill="none" stroke="{line_color}" stroke-width=".8" stroke-dasharray="2.5 4" opacity=".8"/>',
            f'<circle cx="{x+10:g}" cy="{y+11:g}" r="2.1" fill="{COLORS["wood2"]}" stroke="{COLORS["paper"]}" stroke-width=".6"/>',
            f'<circle cx="{x+w-10:g}" cy="{y+11:g}" r="2.1" fill="{COLORS["wood2"]}" stroke="{COLORS["paper"]}" stroke-width=".6"/>']


def nav_button(x: float, y: float, w: float, h: float, label: str, active: bool) -> list[str]:
    fill = "url(#wood)" if active else "url(#creamButton)"
    ink = "cream" if active else "ink"
    outline = COLORS["ember"] if active else COLORS["wood"]
    d = f'M{x+7:g} {y+1:g} H{x+w-7:g} Q{x+w:g} {y+2:g} {x+w-1:g} {y+9:g} V{y+h-8:g} Q{x+w-2:g} {y+h:g} {x+w-9:g} {y+h-1:g} H{x+8:g} Q{x:g} {y+h-2:g} {x+1:g} {y+h-9:g} V{y+8:g} Q{x+2:g} {y+1:g} {x+7:g} {y+1:g} Z'
    return [path(d, fill, outline, 1.4, 'filter="url(#shadow)"'),
            path(d, 'url(#woodGrain)' if active else 'url(#canvasGrain)', "none", 0, 'opacity=".38"'),
            text(x+w/2, y+h/2+4, label, 12, ink, 750, "middle")]


def button(x: float, y: float, w: float, h: float, label: str, state: str = "quiet",
           size: float = 12, sub: str | None = None) -> list[str]:
    if state == "primary":
        fill, outline, ink = "url(#primaryButton)", "#713e2e", "cream"
    elif state == "disabled":
        fill, outline, ink = "#b8b5a0", "#898a71", "#6e7565"
    elif state == "pressed":
        fill, outline, ink = "url(#wellSelected)", "#47362c", "cream"
    else:
        fill, outline, ink = "url(#creamButton)", COLORS["wood"], "ink"
    dy = 1 if state == "pressed" else 0
    d = f'M{x+8:g} {y+dy:g} H{x+w-8:g} Q{x+w:g} {y+dy:g} {x+w:g} {y+8+dy:g} V{y+h-8+dy:g} Q{x+w:g} {y+h+dy:g} {x+w-8:g} {y+h+dy:g} H{x+8:g} Q{x:g} {y+h+dy:g} {x:g} {y+h-8+dy:g} V{y+8+dy:g} Q{x:g} {y+dy:g} {x+8:g} {y+dy:g} Z'
    return ([path(d, fill, outline, 1.4, 'filter="url(#shadow)"'),
             path(d, 'url(#woodGrain)' if state in ("primary", "pressed") else 'url(#canvasGrain)', "none", 0, 'opacity=".34"'),
             f'<path d="M{x+7:g} {y+5+dy:g} H{x+w-7:g}" stroke="#fff3d4" stroke-width=".9" opacity=".44"/>',
             text(x+w/2, y+ h/2 + (4 if not sub else -1) + dy, label, size, ink, 750, "middle")] +
            ([text(x+w/2, y+h/2+12+dy, sub, 8.5, ink, 600, "middle")] if sub else []))


def drop_target(x: float, y: float, w: float, h: float) -> list[str]:
    # Destination preview, not a one-tap delete/drop button. It appears as a drop zone during drag.
    return [f'<path d="M{x+8:g} {y:g} H{x+w-8:g} Q{x+w:g} {y:g} {x+w:g} {y+8:g} V{y+h-8:g} Q{x+w:g} {y+h:g} {x+w-8:g} {y+h:g} H{x+8:g} Q{x:g} {y+h:g} {x:g} {y+h-8:g} V{y+8:g} Q{x:g} {y:g} {x+8:g} {y:g} Z" fill="#4a5d43" stroke="#c9d39e" stroke-width="1.5" stroke-dasharray="4 3"/>',
            small_glyph("drop", x+19, y+21, .62, "cream"),
            text(x+34, y+19, "월드 드롭", 9.5, "cream", 750),
            text(x+w/2, y+36, "드래그 중 표시", 8, "paper", 600, "middle")]


def slot(x: float, y: float, size: float, state: str = "normal") -> list[str]:
    if state == "selected":
        outer, inner, stroke, innerstroke, filter_attr = "url(#primaryButton)", "url(#wellSelected)", "#783f2d", "#f0bd78", 'filter="url(#glow)"'
    elif state == "target":
        outer, inner, stroke, innerstroke, filter_attr = "#93a36a", "#405a42", "#3d583d", "#d6deaa", 'filter="url(#shadow)"'
    elif state == "disabled":
        outer, inner, stroke, innerstroke, filter_attr = "#abae98", "#c6c2a5", "#868c74", "#d7d2b7", 'opacity=".58"'
    elif state == "pressed":
        outer, inner, stroke, innerstroke, filter_attr = "#74563c", "#304434", "#4c3b2d", "#ad9b71", 'filter="url(#shadow)"'
    else:
        outer, inner, stroke, innerstroke, filter_attr = "url(#wood)", "url(#well)", "#593d2c", "#ddcfaa", 'filter="url(#shadow)"'
    # Cut-corner, bevelled item well. A small notch identifies top-left; count badge belongs at bottom-right.
    d = f'M{x+6:g} {y:g} H{x+size-5:g} L{x+size:g} {y+6:g} V{y+size-6:g} L{x+size-6:g} {y+size:g} H{x+5:g} L{x:g} {y+size-6:g} V{y+6:g} Z'
    d2 = f'M{x+6:g} {y+3:g} H{x+size-7:g} L{x+size-3:g} {y+7:g} V{y+size-7:g} L{x+size-7:g} {y+size-3:g} H{x+7:g} L{x+3:g} {y+size-7:g} V{y+7:g} Z'
    return [path(d, outer, stroke, 1.5, filter_attr), path(d2, inner, innerstroke, .75),
            f'<path d="M{x+8:g} {y+5:g} H{x+size-10:g}" stroke="#fff4d7" stroke-width=".8" opacity=".42"/>',
            f'<path d="M{x+4:g} {y+11:g}v4m{size-8:g} {size-26:g}v4" stroke="{COLORS["paper3"]}" stroke-width=".8" opacity=".46"/>']


def icon_cell(out: list[str], svg: Path, catalog: dict[str, dict[str, object]], item_id: str | None,
              count: int | None, x: float, y: float, size: float, state: str = "normal",
              number: str | None = None, label: str | None = None) -> None:
    out.extend(slot(x, y, size, state))
    if item_id:
        icon_size = size * .72
        out.append(image(svg, Path(catalog[item_id]["icon"]), x+(size-icon_size)/2,
                         y+(size-icon_size)/2-1, icon_size, icon_size))
        if count is not None and count > 0:
            count_text = f"{count}"
            width = max(18, len(count_text)*7+7)
            out.append(f'<path d="M{x+size-width-2:g} {y+size-13:g} h{width-4:g} q4 0 4 4 v5 h-{width:g} v-5 q0-4 4-4 Z" fill="#26372e" stroke="#eddfb7" stroke-width=".55" opacity=".98"/>')
            out.append(text(x+size-4, y+size-3.5, count_text, 10, "cream", 750, "end"))
    if number:
        out.append(text(x+5, y+11, number, 9.5, "cream", 700))
    if label:
        out.append(text(x+size/2, y+size+11, label, 8.5, "ink", 650, "middle"))


def gear_slot(out: list[str], svg: Path, catalog: dict[str, dict[str, object]],
              label: str, item_id: str | None, x: float, y: float,
              state: str = "normal", size: float = 58, sub: str | None = None,
              label_position: str = "bottom") -> None:
    # An independent suspended socket with a clear 58 px touch area.
    out.extend(slot(x, y, size, state))
    if item_id:
        icon_size = 36
        out.append(image(svg, Path(catalog[item_id]["icon"]), x+(size-icon_size)/2, y+5, icon_size, icon_size))
    else:
        out.append(path(f'M{x+17:g} {y+29:g}h24', 'none', 'paper3', 1.4, 'stroke-linecap="round" opacity=".9"'))
    if label_position == "top":
        out.append(text(x+size/2, y-5, label, 9.5, "ink", 750, "middle"))
    else:
        out.append(text(x+size/2, y+size+11, label, 9.5, "ink", 750, "middle"))
    if sub:
        out.append(text(x+size/2, y+size+21, sub, 8, "muted", 650, "middle"))


def small_glyph(name: str, cx: float, cy: float, scale: float = 1, color: str = "ink") -> str:
    # Original monochrome interaction marks; labels always name the existing action.
    stroke = COLORS.get(color, color)
    common = f'fill="none" stroke="{stroke}" stroke-width="{2*scale:g}" stroke-linecap="round" stroke-linejoin="round"'
    if name == "split":
        return (f'<path d="M{cx-8*scale:g} {cy-4*scale:g}h13m-4-4 4 4-4 4M{cx+8*scale:g} {cy+5*scale:g}h-13m4-4-4 4 4 4" {common}/>')
    if name == "drop":
        return (f'<path d="M{cx:g} {cy-9*scale:g}C{cx-1*scale:g} {cy-4*scale:g} {cx-7*scale:g} {cy+1*scale:g} {cx-7*scale:g} {cy+5*scale:g}a7 7 0 0 0 14 0c0-4-6-9-7-14Z" {common}/><path d="M{cx-9*scale:g} {cy+12*scale:g}h18" {common}/>')
    if name == "drag":
        return (f'<path d="M{cx-8*scale:g} {cy-6*scale:g}v12m-4-8 4-4 4 4M{cx+8*scale:g} {cy+6*scale:g}v-12m-4 4 4-4 4 4" {common}/>')
    return f'<circle cx="{cx:g}" cy="{cy:g}" r="7" {common}/>'


def inventory_tray(out: list[str], svg: Path, catalog: dict[str, dict[str, object]], loc: dict[str, list[str]],
                   selected: bool) -> None:
    # 47px touch wells fit nine across with a one-pixel gutter inside the right field.
    x, y, size, gap = 379, 60, 47, 1
    out.extend(tray(370, 58, 450, 239, "canvas"))
    # Hotbar above the backpack, following Core Keeper's spatial hierarchy.
    out.extend([text(379, 70, "빠른 슬롯", 9.5, "ink", 750), text(810, 70, "09", 8.5, "muted", 700, "end")])
    for i, (item_id, count) in enumerate(HOTBAR):
        icon_cell(out, svg, catalog, item_id, count, x+i*(size+gap), 75, size,
                  "normal", number=str(i+1))
    out.extend([line(379, 128, 810, 128, "paper3", .85, opacity=.85),
                text(379, 144, "가방 · 보관 27", 10.5, "ink", 800),
                text(810, 144, "27 / 27", 8.5, "muted", 650, "end")])
    for i, (item_id, count) in enumerate(STORAGE):
        row, col = divmod(i, 9)
        state = "selected" if selected and item_id == "wood" else "normal"
        icon_cell(out, svg, catalog, item_id, count, x+col*(size+gap), 150+row*(size+2), size,
                  state)


def craft_rail(out: list[str], svg: Path) -> None:
    x, y = 24, 82
    # The actual crafting entry is a single action; no invented recipe cards or workbench inventory.
    out.extend(tray(x, y, 80, 142, "wood"))
    out.append(text(64, y+20, "제작", 11, "cream", 800, "middle"))
    out.append(image(svg, ART / "workbench.png", 43, y+30, 42, 42))
    out.extend(button(31, y+82, 66, 44, "제작 열기", "quiet", 9.1))
    out.extend([line(32, y+132, 96, y+132, "paper3", .8, opacity=.6),
                text(64, y+150, "기존 제작 화면", 8, "cream", 600, "middle")])


def avatar(out: list[str], x: float, y: float, size: float, svg: Path, capture: Path = AVATAR_CAPTURE) -> None:
    # Crop the real Lowfire character from a committed Unity capture; no generated character/background.
    view_size = 130
    clip_id = f'avatar-{int(x)}-{int(y)}'
    out.append(f'<defs><clipPath id="{clip_id}"><circle cx="{x+size/2:g}" cy="{y+size/2:g}" r="{size/2-2:g}"/></clipPath></defs>')
    scale = size / view_size
    out.append(f'<image href="{esc(rel(svg, capture))}" x="{x-900*scale:g}" y="{y-470*scale:g}" width="{1920*scale:g}" height="{1080*scale:g}" preserveAspectRatio="none" clip-path="url(#{clip_id})"/>')
    out.append(f'<circle cx="{x+size/2:g}" cy="{y+size/2:g}" r="{size/2-1:g}" fill="none" stroke="{COLORS["paper"]}" stroke-width="2.2"/>')


def bag_screen(catalog: dict[str, dict[str, object]], loc: dict[str, list[str]], selected: bool) -> str:
    svg = SCREENS / ("bag-selected.svg" if selected else "bag-basic.svg")
    out = start(svg, "야영 배낭", "bag")
    craft_rail(out, svg)
    inventory_tray(out, svg, catalog, loc, selected)
    # Core Keeper-like paper-doll field: equipment sockets orbit the existing Lowfire character.
    out.extend([text(236, 69, "장비 · 4", 10, "cream", 800, "middle"),
                f'<circle cx="236" cy="182" r="40" fill="#e6d7ae" fill-opacity=".9" stroke="{COLORS["wood"]}" stroke-width="2" filter="url(#shadow)"/>',
                f'<circle cx="236" cy="182" r="34" fill="#617252" stroke="{COLORS["paper"]}" stroke-width="1.3"/>'])
    avatar(out, 204, 150, 64, svg)
    # Four functional targets, not decoration; their icons map to the real equipped item fixture.
    gear_slot(out, svg, catalog, "머리", EQUIPPED["Head"], 208, 82, "normal", 56, label_position="top")
    gear_slot(out, svg, catalog, "몸", EQUIPPED["Body"], 300, 137, "normal", 56)
    gear_slot(out, svg, catalog, "도구", EQUIPPED["Utility"], 208, 226, "normal", 56)
    gear_slot(out, svg, catalog, "장신구", EQUIPPED["Accessory"], 116, 137, "normal", 56)
    out.append(line(236, 138, 236, 150, "paper", 1, opacity=.9))
    if selected:
        # Only the selected-item state opens actions. It sits below the grid, not in a permanent right card.
        out.extend(tray(370, 302, 450, 77, "wood"))
        icon_cell(out, svg, catalog, "wood", 24, 380, 317, 42, "normal")
        wood_count = next(count for item_id, count in STORAGE if item_id == "wood")
        material = ko(loc, "ui.item_category.Material", "재료")
        out.extend([text(430, 331, item_name(loc, "wood", catalog), 12, "cream", 800),
                    text(430, 348, f"{material}  ·  {wood_count} / {catalog['wood']['max']}", 9.5, "paper", 650),
                    small_glyph("drag", 455, 364, .72, "cream"), text(470, 368, "드래그 이동", 8, "paper", 600)])
        out.extend(button(588, 318, 92, 44, "나누기", "quiet", 10.5))
        out.extend(drop_target(690, 318, 108, 44))
    else:
        out.extend([small_glyph("drag", 482, 344, .8, "cream"),
                    text(502, 348, "드래그로 핫바·가방 간 이동", 10, "cream", 700),
                    small_glyph("split", 690, 344, .8, "cream"),
                    text(710, 348, "선택 후 나누기", 10, "cream", 700)])
        out.extend([text(610, 370, "보관 27  ·  빠른 슬롯 9  ·  장비 4", 8.5, "paper", 650, "middle")])
    return "\n".join(out + ["</svg>"])


def gear_screen(catalog: dict[str, dict[str, object]], loc: dict[str, list[str]]) -> str:
    svg = SCREENS / "equipment-focus.svg"
    out = start(svg, "야영 배낭", "gear")
    # The equipment tab focuses the doll and compatible item. Bag selection persists across the view change.
    out.extend(tray(24, 64, 312, 310, "canvas"))
    out.extend([text(180, 83, "현재 장비", 11, "ink", 800, "middle"),
                text(322, 83, "방어 6", 9, "muted", 700, "end"),
                f'<circle cx="180" cy="190" r="42" fill="#e5d4aa" stroke="{COLORS["wood"]}" stroke-width="2.2" filter="url(#shadow)"/>',
                f'<circle cx="180" cy="190" r="36" fill="#617252" stroke="{COLORS["paper"]}" stroke-width="1.3"/>'])
    avatar(out, 144, 154, 72, svg)
    # The existing four slots are arranged around the actual character, not in a grid beside it.
    gear_slot(out, svg, catalog, "머리", "iron_head", 152, 101, "target", 56, label_position="top")
    gear_slot(out, svg, catalog, "몸", "iron_body", 246, 157, "normal", 56, label_position="top")
    gear_slot(out, svg, catalog, "도구", "lantern", 152, 244, "normal", 56)
    gear_slot(out, svg, catalog, "장신구", None, 58, 157, "normal", 56, label_position="top")
    out.extend([line(180, 157, 180, 154, "wood", 1.2), line(216, 190, 246, 185, "wood", 1.1),
                line(144, 190, 114, 174, "wood", 1.1), line(180, 226, 180, 244, "wood", 1.1)])
    # Candidate is an actual inventory item carried across the Bag -> Equipment view transition.
    out.extend(tray(350, 64, 470, 154, "canvas"))
    out.extend([text(368, 88, "가방에서 선택한 장비", 11, "ink", 800),
                f'<circle cx="403" cy="147" r="32" fill="#52654a" stroke="{COLORS["wood"]}" stroke-width="1.4"/>'])
    icon_size = 46
    out.append(image(svg, Path(catalog["steel_head"]["icon"]), 380, 124, icon_size, icon_size))
    out.extend([text(439, 137, item_name(loc, "steel_head", catalog), 15, "ink", 800),
                text(439, 156, "머리 장비  ·  1개", 9.5, "muted", 700),
                text(439, 179, "방어력  2  →  3", 11, "ink", 750),
                text(439, 197, "현재 철 투구보다 +1", 9, "moss", 800)])
    out.extend(tray(350, 230, 470, 144, "wood"))
    out.extend([text(362, 255, "머리 슬롯 대상", 10, "cream", 750),
                text(812, 255, "선택 유지 · 가방으로 돌아가도 후보 보존", 8.5, "paper", 600, "end")])
    out.extend(button(360, 276, 196, 48, "장착", "primary", 13, "선택한 강철 투구"))
    out.extend(button(570, 276, 236, 48, "현재 머리 슬롯 해제", "quiet", 10.5, "빈 가방 칸이 필요합니다"))
    out.extend([text(360, 350, "장착: 가방 아이템을 선택 → 장비 화면 → 장착", 9, "cream", 650),
                text(360, 365, "해제: 장착 슬롯을 선택 → 해제  ·  드래그 해제도 지원", 8.5, "paper", 600)])
    return "\n".join(out + ["</svg>"])


def ui_kit(catalog: dict[str, dict[str, object]]) -> str:
    svg = GRAPHICS / "lowfire-inventory-ui-kit.svg"
    out = [
        '<svg xmlns="http://www.w3.org/2000/svg" width="900" height="430" viewBox="0 0 900 430">',
        '<defs><linearGradient id="canvas" x1="0" y1="0" x2=".8" y2="1"><stop stop-color="#f2e9ce"/><stop offset=".55" stop-color="#e7d9b4"/><stop offset="1" stop-color="#d9c79e"/></linearGradient><linearGradient id="wood" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#a77b50"/><stop offset="1" stop-color="#64452f"/></linearGradient><linearGradient id="well" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#607354"/><stop offset="1" stop-color="#435840"/></linearGradient><linearGradient id="wellSelected" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#795239"/><stop offset="1" stop-color="#553f31"/></linearGradient><linearGradient id="primaryButton" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#d97d4c"/><stop offset="1" stop-color="#aa5337"/></linearGradient><filter id="shadow" x="-30%" y="-30%" width="160%" height="170%"><feGaussianBlur in="SourceAlpha" stdDeviation="3"/><feOffset dy="2"/><feComponentTransfer><feFuncA type="linear" slope=".24"/></feComponentTransfer><feMerge><feMergeNode/><feMergeNode in="SourceGraphic"/></feMerge></filter><filter id="glow" x="-50%" y="-50%" width="200%" height="200%"><feGaussianBlur stdDeviation="2.4" result="blur"/><feMerge><feMergeNode in="blur"/><feMergeNode in="SourceGraphic"/></feMerge></filter><pattern id="canvasGrain" width="28" height="20" patternUnits="userSpaceOnUse"><path d="M1 4h6M15 13h8M5 18h2" stroke="#b7a47c" stroke-width=".65" opacity=".25"/></pattern><pattern id="woodGrain" width="42" height="14" patternUnits="userSpaceOnUse"><path d="M0 4C8 2 15 6 23 4s12-2 19 0M6 10c7-2 13 1 20 0" fill="none" stroke="#3f3024" stroke-width=".7" opacity=".28"/></pattern></defs>',
        '<rect width="900" height="430" fill="#405343"/>',
        text(32, 44, "LOWFIRE · FIELD KIT MATERIALS", 17, "cream", 800, letter=1.4),
        text(32, 68, "One tactile language across trays, item wells, selection and action", 11, "paper", 600),
    ]
    out.extend(tray(32, 94, 390, 284, "canvas"))
    out.extend([text(58, 127, "CAMP CANVAS / 9-SLICE PANEL", 10, "ink", 800),
                text(58, 150, "Inset stitching · asymmetrical shoulder · visible grain", 9, "muted", 600)])
    out.extend([text(58, 190, "Item wells", 12, "ink", 800)])
    for i, state in enumerate(["normal", "selected", "target", "pressed", "disabled"]):
        x = 58 + i*66
        out.extend(slot(x, 204, 50, state))
        out.append(text(x+25, 265, state.upper(), 7.5, "ink", 700, "middle"))
    out.extend([text(58, 302, "Cut-stone corners · recessed moss · quantity overlay", 8.7, "muted", 650),
                text(58, 319, "Selected and target states change material depth as well as color.", 8, "muted", 600)])
    # Raised wood action and two existing-context states.
    out.extend(tray(456, 94, 412, 284, "wood"))
    out.extend([text(481, 127, "CARVED WOOD / ACTION SURFACES", 10, "cream", 800),
                text(481, 150, "Warm edge + quiet etched grain; no ornamental gold frame", 9, "paper", 600)])
    out.extend(button(484, 180, 164, 48, "장착", "primary", 13, "선택한 장비"))
    out.extend(button(660, 180, 172, 48, "월드에 버리기", "quiet", 10.5))
    out.extend(button(484, 244, 164, 48, "눌림 상태", "pressed", 11))
    out.extend(button(660, 244, 172, 48, "비활성 상태", "disabled", 11))
    out.extend([text(484, 329, "Pressed: 1 px inset / lower value", 8.5, "cream", 600),
                text(484, 346, "Disabled: lower contrast / no response implied", 8.5, "paper", 600),
                text(484, 364, "All control labels describe existing Lowfire actions.", 8.5, "paper", 600)])
    return "\n".join(out + ["</svg>"])


def main() -> None:
    for directory in (SCREENS, GRAPHICS, PREVIEWS, VALIDATION / "assets"):
        directory.mkdir(parents=True, exist_ok=True)
    for path in (BACKGROUND, AVATAR_CAPTURE, RESOURCES / "NotoSansKR.ttf"):
        if not path.exists():
            raise FileNotFoundError(path)
    catalog, loc = item_catalog(), localization()
    item_ids = {item_id for item_id, _ in STORAGE + HOTBAR} | {v for v in EQUIPPED.values() if v} | {"steel_head"}
    missing = sorted(item_ids - catalog.keys())
    if missing:
        raise RuntimeError(f"Item IDs missing from current ItemDefinition catalog: {missing}")
    if len(STORAGE) != 27 or len(HOTBAR) != 9 or len(EQUIPPED) != 4:
        raise RuntimeError("Inventory fixture must render 27 storage, 9 hotbar, and 4 equipment slots.")
    # Read actual armor definitions to keep the comparison numeric and data-backed.
    def defense(item_id: str) -> int:
        item = Path(catalog[item_id]["path"]).read_text(encoding="utf-8", errors="ignore")
        guid = parse(r"^  Armor: \{fileID: \d+, guid: ([0-9a-f]+)", item)
        for candidate in DATA.glob("*.asset"):
            raw = candidate.read_text(encoding="utf-8", errors="ignore")
            meta = candidate.with_suffix(".asset.meta")
            asset_guid = parse(r"^guid: ([0-9a-f]+)$", meta.read_text(encoding="utf-8", errors="ignore")) if meta.exists() else ""
            if asset_guid == guid and "Wildfolk.ArmorDefinition" in raw:
                return int(parse(r"^  Defense: (\d+)$", raw, "0"))
        raise RuntimeError(f"ArmorDefinition missing for {item_id}")
    current_def, candidate_def = defense("iron_head"), defense("steel_head")
    if (current_def, candidate_def) != (2, 3):
        raise RuntimeError(f"Unexpected current armor data: iron_head={current_def}, steel_head={candidate_def}")
    if current_def + defense("iron_body") != 6:
        raise RuntimeError("The equipment preview's current defense total no longer matches source data.")
    (SCREENS / "bag-basic.svg").write_text(bag_screen(catalog, loc, False), encoding="utf-8")
    (SCREENS / "bag-selected.svg").write_text(bag_screen(catalog, loc, True), encoding="utf-8")
    (SCREENS / "equipment-focus.svg").write_text(gear_screen(catalog, loc), encoding="utf-8")
    (GRAPHICS / "lowfire-inventory-ui-kit.svg").write_text(ui_kit(catalog), encoding="utf-8")
    print("Wrote 3 editable 844x390 screens and the reusable Lowfire UI kit.")
    print("Data check: 27 storage, 9 hotbar, 4 equipment; iron head defense 2 -> steel head defense 3; total equipped defense 6.")


if __name__ == "__main__":
    main()
