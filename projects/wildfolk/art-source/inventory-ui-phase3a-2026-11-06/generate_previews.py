#!/usr/bin/env python3
"""Generate editable Lowfire inventory UI SVG prototypes from current project assets.

This script only writes under art-source/inventory-ui-phase3a-2026-11-06 and
validation/inventory-ui-phase3a-2026-11-06. It does not touch Unity source,
scenes, save data, or Penpot.
"""
from __future__ import annotations

import html
import json
import re
from pathlib import Path
from xml.sax.saxutils import escape

ROOT = Path(__file__).resolve().parents[2]
SOURCE = Path(__file__).resolve().parent
VALIDATION = ROOT / "validation" / "inventory-ui-phase3a-2026-11-06"
PREVIEWS = VALIDATION / "previews"
SVG_DIR = PREVIEWS / "svg"
ART = ROOT / "client" / "Assets" / "Wildfolk" / "Art"
DATA = ROOT / "client" / "Assets" / "Wildfolk" / "Data"
RESOURCES = ROOT / "client" / "Assets" / "Wildfolk" / "Resources"
AUDIT = ROOT / "validation" / "penpot-ui-ux-audit-2026-11-04" / "evidence" / "live"
OLDER_CAPTURE = ROOT / "validation" / "post-commercial-ui-qa-tools" / "evidence" / "gameplay--inventory-en-844.png"

COLORS = {
    "ink": "#111b17", "shell": "#202e27", "shell_inner": "#26372e",
    "zone": "#1b2923", "slot": "#17231e", "slot_lit": "#31443a",
    "line": "#77836b", "line_dim": "#4d5c4d", "cream": "#e7ddc4",
    "muted": "#abb49d", "amber": "#e5b564", "rust": "#ad6240",
    "rust_hi": "#d08355", "moss": "#98ae76", "danger": "#b86a54",
}

# Source asset states. The full-storage preview uses each ItemDefinition's actual
# MaxStack as its displayed count and is clearly recorded as a static QA fixture.
FULL_IDS = [
    "wood", "stone", "berry", "mushroom", "plant_fiber", "leather",
    "iron_ore", "copper_ore", "tin_ore", "iron_ingot", "torch", "arrow",
    "bone_arrow", "bronze_arrow", "crossbow_bolt", "mushroom_stew",
    "roasted_mushroom", "raw_water_cup", "clean_water_cup", "waterskin",
    "bow", "iron_sword", "iron_axe", "iron_pickaxe", "leather_head",
    "leather_body", "iron_head",
]

DEFS = r"""
<linearGradient id="panelFill" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#293a31"/><stop offset=".08" stop-color="#202e27"/><stop offset="1" stop-color="#1b2923"/></linearGradient>
<linearGradient id="slotFill" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#34473b"/><stop offset=".12" stop-color="#23332b"/><stop offset="1" stop-color="#17231e"/></linearGradient>
<linearGradient id="primaryFill" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#d07a4f"/><stop offset=".18" stop-color="#ad6240"/><stop offset="1" stop-color="#854c37"/></linearGradient>
<filter id="softGlow" x="-40%" y="-40%" width="180%" height="180%"><feGaussianBlur stdDeviation="2.2" result="blur"/><feMerge><feMergeNode in="blur"/><feMergeNode in="SourceGraphic"/></feMerge></filter>
<style>
@font-face{font-family:LowfireNoto;src:url("../../../../client/Assets/Wildfolk/Resources/NotoSansKR.ttf") format("truetype");font-weight:100 900}
text{font-family:LowfireNoto,"Malgun Gothic","Segoe UI",sans-serif;paint-order:stroke;}
</style>
<symbol id="slot-default" viewBox="0 0 64 64"><path d="M5 1H59L63 5V59L59 63H5L1 59V5Z" fill="url(#slotFill)" stroke="#111b17" stroke-width="2"/><path d="M6 3H58L61 6M3 8V56L7 61H57" fill="none" stroke="#77836b" stroke-width="1.4"/><path d="M8 7H56" stroke="#9aa584" stroke-width="1" opacity=".45"/></symbol>
<symbol id="slot-selected" viewBox="0 0 64 64"><path d="M5 1H59L63 5V59L59 63H5L1 59V5Z" fill="url(#slotFill)" stroke="#111b17" stroke-width="2"/><path d="M5 2H59L62 5V59L59 62H5L2 59V5Z" fill="none" stroke="#e5b564" stroke-width="3"/><path d="M7 7H57" stroke="#fff0c9" stroke-width="1" opacity=".6"/></symbol>
<symbol id="slot-compatible" viewBox="0 0 64 64"><path d="M5 1H59L63 5V59L59 63H5L1 59V5Z" fill="url(#slotFill)" stroke="#111b17" stroke-width="2"/><path d="M5 2H59L62 5V59L59 62H5L2 59V5Z" fill="none" stroke="#98ae76" stroke-width="3"/><path d="M7 7H57" stroke="#d0e6a9" stroke-width="1" opacity=".55"/></symbol>
<symbol id="slot-blocked" viewBox="0 0 64 64"><path d="M5 1H59L63 5V59L59 63H5L1 59V5Z" fill="#1a211d" stroke="#111b17" stroke-width="2"/><path d="M5 2H59L62 5V59L59 62H5L2 59V5Z" fill="none" stroke="#6c6254" stroke-width="2"/><path d="M14 14L50 50" stroke="#806659" stroke-width="2" opacity=".55"/></symbol>
<symbol id="button-primary" viewBox="0 0 240 72"><path d="M8 1H232L239 8V64L232 71H8L1 64V8Z" fill="url(#primaryFill)" stroke="#111b17" stroke-width="3"/><path d="M9 4H231L236 9" fill="none" stroke="#efb47d" stroke-width="2"/><path d="M10 66H230" stroke="#643e30" stroke-width="2"/></symbol>
<symbol id="button-secondary" viewBox="0 0 240 72"><path d="M8 1H232L239 8V64L232 71H8L1 64V8Z" fill="#304238" stroke="#111b17" stroke-width="3"/><path d="M9 4H231L236 9" fill="none" stroke="#859178" stroke-width="2"/><path d="M10 66H230" stroke="#17231e" stroke-width="2"/></symbol>
<symbol id="button-pressed" viewBox="0 0 240 72"><path d="M8 5H232L239 12V67L232 71H8L1 64V12Z" fill="#754834" stroke="#111b17" stroke-width="3"/><path d="M11 9H229" fill="none" stroke="#5c392e" stroke-width="2"/></symbol>
<symbol id="button-disabled" viewBox="0 0 240 72"><path d="M8 1H232L239 8V64L232 71H8L1 64V8Z" fill="#303832" stroke="#111b17" stroke-width="3"/><path d="M9 4H231L236 9" fill="none" stroke="#656b5f" stroke-width="2"/><path d="M10 66H230" stroke="#222a25" stroke-width="2"/></symbol>
<symbol id="action-divider" viewBox="0 0 2 100"><path d="M1 0V100" stroke="#77836b" stroke-width="1" opacity=".7"/><path d="M0 0V100" stroke="#111b17" stroke-width="1"/></symbol>
"""


def escape_text(value: object) -> str:
    return escape(str(value), {'"': "&quot;"})


def load_localization() -> dict[str, list[str]]:
    payload = json.loads((RESOURCES / "Localization.json").read_text(encoding="utf-8"))
    return {row["Key"]: row["Values"] for row in payload["Entries"]}


def localize(loc: dict[str, list[str]], key: str, lang: str, fallback: str = "") -> str:
    values = loc.get(key)
    if values and len(values) >= 2:
        return values[0 if lang == "en" else 1]
    return fallback or key


def parse_asset(pattern: str, source: str, fallback: str = "") -> str:
    match = re.search(pattern, source, re.M)
    return match.group(1).strip() if match else fallback


def item_catalog() -> dict[str, dict[str, str | int | Path]]:
    art_guid: dict[str, Path] = {}
    for meta in ART.glob("*.png.meta"):
        guid = parse_asset(r"^guid: ([0-9a-f]+)$", meta.read_text(encoding="utf-8", errors="ignore"))
        if guid:
            art_guid[guid] = meta.with_suffix("")
    catalog: dict[str, dict[str, str | int | Path]] = {}
    for path in DATA.glob("*.asset"):
        raw = path.read_text(encoding="utf-8", errors="ignore")
        if "Wildfolk.Runtime::Wildfolk.ItemDefinition" not in raw:
            continue
        item_id = parse_asset(r"^  Id: ([^\r\n]+)$", raw)
        icon_guid = parse_asset(r"^  Icon: \{fileID: \d+, guid: ([0-9a-f]+)", raw)
        if not item_id or icon_guid not in art_guid:
            continue
        catalog[item_id] = {
            "display": parse_asset(r"^  DisplayName: ([^\r\n]+)$", raw, item_id),
            "max": int(parse_asset(r"^  MaxStack: (\d+)$", raw, "1")),
            "water_capacity": int(parse_asset(r"^  WaterCapacity: (\d+)$", raw, "0")),
            "icon": art_guid[icon_guid],
        }
        capacity = int(catalog[item_id]["water_capacity"])
        if capacity > 0:
            catalog[item_id]["preview_charges"] = max(1, capacity - 1)
    return catalog


def text(x: float, y: float, value: object, size: float, color: str = "cream",
         weight: int = 500, anchor: str = "start", letter: float = 0.0,
         opacity: float = 1.0) -> str:
    content = escape_text(value)
    return (f'<text x="{x:g}" y="{y:g}" font-size="{size:g}" fill="{COLORS.get(color, color)}" '
            f'font-weight="{weight}" text-anchor="{anchor}" letter-spacing="{letter:g}" opacity="{opacity:g}">{content}</text>')


def line(x1: float, y1: float, x2: float, y2: float, color: str = "line_dim", width: float = 1.0,
         opacity: float = 1.0) -> str:
    return f'<path d="M{x1:g} {y1:g}H{x2:g}" transform="translate(0 {y1-y1:g})" stroke="{COLORS.get(color,color)}" stroke-width="{width:g}" opacity="{opacity:g}" fill="none"/>' if y1 == y2 else f'<path d="M{x1:g} {y1:g}L{x2:g} {y2:g}" stroke="{COLORS.get(color,color)}" stroke-width="{width:g}" opacity="{opacity:g}" fill="none"/>'


def rect(x: float, y: float, w: float, h: float, fill: str, stroke: str = "none", sw: float = 1.0,
         rx: float = 0, opacity: float = 1.0) -> str:
    return f'<rect x="{x:g}" y="{y:g}" width="{w:g}" height="{h:g}" rx="{rx:g}" fill="{fill}" stroke="{stroke}" stroke-width="{sw:g}" opacity="{opacity:g}"/>'


def polygon(points: str, fill: str, stroke: str, sw: float = 1.0, opacity: float = 1.0) -> str:
    return f'<path d="{points}" fill="{fill}" stroke="{stroke}" stroke-width="{sw:g}" opacity="{opacity:g}"/>'


def link_path(svg_path: Path, asset_path: Path) -> str:
    return Path(__import__("os").path.relpath(asset_path, svg_path.parent)).as_posix()


def image(svg_path: Path, asset_path: Path, x: float, y: float, w: float, h: float,
          opacity: float = 1.0) -> str:
    href = escape_text(link_path(svg_path, asset_path))
    return (f'<image href="{href}" x="{x:g}" y="{y:g}" width="{w:g}" height="{h:g}" '
            f'preserveAspectRatio="xMidYMid meet" opacity="{opacity:g}"/>')


def slot(x: float, y: float, w: float, h: float, state: str = "default") -> str:
    stroke = {"selected": "amber", "compatible": "moss", "blocked": "#806659"}.get(state, "line")
    fill = "#1a241f" if state == "blocked" else "url(#slotFill)"
    sw = 2.4 if state == "selected" else 1.6 if state == "compatible" else 1.2
    cut = min(6.0, w * .10, h * .12)
    d = f"M{x+cut:g} {y:g}H{x+w-cut:g}L{x+w:g} {y+cut:g}V{y+h-cut:g}L{x+w-cut:g} {y+h:g}H{x+cut:g}L{x:g} {y+h-cut:g}V{y+cut:g}Z"
    out = [polygon(d, fill, COLORS.get(stroke, stroke), sw)]
    out.append(line(x+cut+2, y+2, x+w-cut-2, y+2, "cream" if state == "selected" else "line", .8, .38))
    if state == "selected":
        out.append(rect(x+cut+1, y+h-3, w-2*cut-2, 2, COLORS["amber"]))
    if state == "blocked":
        out.append(f'<path d="M{x+12:g} {y+12:g}L{x+w-12:g} {y+h-12:g}" stroke="{COLORS["danger"]}" stroke-width="1.4" opacity=".65"/>')
    return "".join(out)


def button(x: float, y: float, w: float, h: float, label: str, state: str = "secondary",
           font_size: float = 12, icon: str | None = None) -> str:
    symbol = {"primary": "button-primary", "secondary": "button-secondary",
              "pressed": "button-pressed", "disabled": "button-disabled"}[state]
    out = [f'<use href="#{symbol}" x="{x:g}" y="{y:g}" width="{w:g}" height="{h:g}"/>']
    if icon:
        out.append(text(x+18, y+h/2+font_size*.34, icon, font_size+1, "cream", 700, "middle"))
        out.append(text(x+34, y+h/2+font_size*.34, label, font_size, "cream", 650, "start"))
    else:
        out.append(text(x+w/2, y+h/2+font_size*.34, label, font_size, "cream", 650, "middle"))
    return "".join(out)


def item_cell(svg_path: Path, catalog: dict[str, dict[str, str | int | Path]], item_id: str | None,
              count: int, x: float, y: float, w: float, h: float, number: str | None = None,
              state: str = "default", ghost: bool = False) -> str:
    out = [slot(x, y, w, h, state)]
    if item_id and item_id in catalog:
        icon = Path(catalog[item_id]["icon"])
        pad = max(5, w * .18)
        out.append(image(svg_path, icon, x+pad, y+pad, w-2*pad, h-2*pad-2, .62 if ghost else 1.0))
        if count > 0:
            capacity = int(catalog[item_id].get("water_capacity", 0))
            count_label = f"{catalog[item_id].get('preview_charges', 0)}/{capacity}" if capacity > 0 else str(count)
            badge_w = max(17, 7 + len(count_label)*6.0)
            out.append(rect(x+w-badge_w-1, y+h-13, badge_w, 11, "#101915", "#596554", .55, 2, .94))
            out.append(text(x+w-3, y+h-4, count_label, max(8.5 if w<54 else 8.0, w*.19), "cream", 700, "end"))
    if number:
        out.append(text(x+4, y+11, number, max(8.5 if w<54 else 8.0, w*.19), "muted", 600))
    if ghost:
        out.append(rect(x+1, y+1, w-2, h-2, "#9bb079", opacity=.25))
    return "".join(out)


def gear_cell(x: float, y: float, w: float, h: float, label: str,
              item_id: str | None, catalog: dict[str, dict[str, str | int | Path]],
              svg_path: Path, lang: str, selected: bool = False, compatible: bool = False) -> str:
    state = "selected" if selected else "compatible" if compatible else "default"
    horizontal = w >= h * 2.0
    box = h if horizontal else min(h * .66, w * .68)
    bx = x if horizontal else x + (w-box)/2
    by = y if horizontal else y + 2
    out = [slot(bx, by, box, box, state)]
    if item_id and item_id in catalog:
        pad = max(6, box*.16)
        out.append(image(svg_path, Path(catalog[item_id]["icon"]), bx+pad, by+pad, box-pad*2, box-pad*2))
    else:
        # Empty is represented by the real slot well alone; no invented avatar or gear art.
        out.append(f'<path d="M{bx+box*.34:g} {by+box*.5:g}H{bx+box*.66:g}" stroke="{COLORS["line"]}" stroke-width="1.5" opacity=".65"/>')
    label_size = 11 if w < 170 else 12
    if horizontal:
        out.append(text(x+h+8, y+h*.52+label_size*.34, label, label_size, "cream", 600))
    else:
        out.append(text(x+w/2, y+h-5, label, label_size, "cream", 600, "middle"))
    if compatible:
        if horizontal:
            out.append(text(x+w-5, y+h*.52+label_size*.34, "✓", label_size, "moss", 700, "end"))
        else:
            out.append(text(x+w-4, y+h-5, "✓", label_size, "moss", 700, "end"))
    return "".join(out)


def shell(svg_path: Path, w: int, h: int, background: Path, title: str, label: str,
          tag: str = "STATIC MOCKUP · UNITY UNCHANGED") -> list[str]:
    out = [image(svg_path, background, 0, 0, w, h)]
    out.append(rect(0, 0, w, h, "#101915", opacity=.42))
    if w < 900:
        x, y, pw, ph, bevel = 16, 10, w-32, h-20, 12
    elif w < 1500:
        x, y, pw, ph, bevel = 14, 14, w-28, h-28, 16
    else:
        x, y, pw, ph, bevel = 22, 58, w-44, h-116, 22
    d = f"M{x+bevel} {y}H{x+pw-bevel}L{x+pw} {y+bevel}V{y+ph-bevel}L{x+pw-bevel} {y+ph}H{x+bevel}L{x} {y+ph-bevel}V{y+bevel}Z"
    out.append(polygon(d, "url(#panelFill)", COLORS["ink"], 5))
    out.append(f'<path d="M{x+bevel+2} {y+3}H{x+pw-bevel-2}L{x+pw-3} {y+bevel+2}" fill="none" stroke="{COLORS["line"]}" stroke-width="1.4" opacity=".65"/>')
    out.append(text(x+20, y+34 if w < 900 else y+52, title, 16 if w < 900 else 25 if w < 1500 else 36, "cream", 750, letter=.2))
    tag_x = x + (132 if w < 900 else 198 if w < 1500 else 275)
    tag_y = y + (30 if w < 900 else 44 if w < 1500 else 59)
    tag_w = 126 if w < 900 else 176 if w < 1500 else 236
    out.append(rect(tag_x, tag_y-11, tag_w, 16, "#111a16", "#6c775e", .6, 1, .88))
    out.append(text(tag_x+6, tag_y, tag, 6.4 if w < 900 else 7.8 if w < 1500 else 10, "amber", 650, letter=.18))
    if w < 900:
        bx, by, bw, bh = w-158, 12, 92, 44
        closex, closew = w-60, 44
    elif w < 1500:
        bx, by, bw, bh = w-206, 26, 142, 44
        closex, closew = w-60, 42
    else:
        bx, by, bw, bh = w-391, 84, 270, 68
        closex, closew = w-95, 58
    out.append(button(bx, by, bw, bh, label, "secondary", 11 if w < 900 else 16 if w < 1500 else 20))
    out.append(button(closex, by, closew, bh, "×", "secondary", 18 if w < 900 else 26 if w < 1500 else 34))
    # A strong horizontal seam and open zones replace the old empty card subdivisions.
    sep_y = y + (48 if w < 900 else 76 if w < 1500 else 105)
    out.append(line(x+12, sep_y, x+pw-12, sep_y, "line", 1.2, .78))
    return out


def storage_grid(out: list[str], svg_path: Path, catalog: dict[str, dict[str, str | int | Path]],
                 loc: dict[str, list[str]], lang: str, state: str, x: float, y: float,
                 cell_w: float, cell_h: float, gap: float, cols: int, storage: list[tuple[str, int]],
                 selected_id: str | None = None, ghost_id: str | None = None) -> list[tuple[float, float, float, float]]:
    out.append(text(x, y-10, localize(loc, "ui.inventory.storage", lang, "Storage"), 12 if cell_w < 54 else 14 if cell_w < 70 else 20, "cream", 650))
    positions=[]
    for i in range(27):
        cx=x+(i%cols)*(cell_w+gap); cy=y+(i//cols)*(cell_h+gap)
        positions.append((cx,cy,cell_w,cell_h))
        entry=storage[i] if i<len(storage) else ("",0)
        item_id,count=entry
        mode="selected" if item_id and item_id==selected_id else "compatible" if state=="equipment-head" and item_id=="iron_head" else "default"
        if state=="drag-move" and i==5: mode="compatible"
        if state=="drag-split" and i==5: mode="compatible"
        number=None
        out.append(item_cell(svg_path,catalog,item_id or None,count,cx,cy,cell_w,cell_h,number,mode))
        if state=="drag-move" and i==5:
            out.append(item_cell(svg_path,catalog,"wood",10,cx+cell_w*.35,cy-cell_h*.55,cell_w,cell_h,None,"compatible",True))
        if state=="drag-split" and i==5:
            out.append(item_cell(svg_path,catalog,"wood",5,cx+cell_w*.35,cy-cell_h*.55,cell_w,cell_h,None,"compatible",True))
    return positions


def hotbar(out: list[str], svg_path: Path, catalog: dict[str, dict[str, str | int | Path]],
           loc: dict[str, list[str]], lang: str, state: str, x: float, y: float,
           cell_w: float, cell_h: float, gap: float, selected: int | None = 1) -> None:
    out.append(text(x, y-8, localize(loc,"ui.inventory.hotbar",lang,"Hotbar"), 12 if cell_w<50 else 14 if cell_w<70 else 20,"cream",650))
    values=[("wood",10),("iron_sword",1),("",0),("",0),("",0),("",0),("",0),("",0),("",0)]
    if state=="storage-full":
        values=[("axe",1),("stone_pickaxe",1),("waterskin",1),("berry",50),("mushroom",50),("torch",50),("leather",50),("iron_sword",1),("arrow",100)]
    if state=="equipment-head": values=[("wood",10),("iron_sword",1),("",0),("",0),("",0),("",0),("",0),("",0),("",0)]
    if state in ("split-modal","item-actions","drag-move","drag-split"): values[0]=("wood",5 if state=="drag-split" else 10)
    for i,(item_id,count) in enumerate(values):
        mode="selected" if selected is not None and i==selected else "default"
        if state in ("split-modal","item-actions","drag-move","drag-split") and i==0: mode="selected"
        cx=x+i*(cell_w+gap)
        out.append(item_cell(svg_path,catalog,item_id or None,count,cx,y,cell_w,cell_h,str(i+1),mode))


def equipment_zone(out: list[str], svg_path: Path, catalog: dict[str, dict[str, str | int | Path]],
                   loc: dict[str, list[str]], lang: str, state: str,
                   x: float, y: float, tile_w: float, tile_h: float, gap_x: float, gap_y: float,
                   font_scale: float = 1.0) -> None:
    out.append(text(x, y-10, localize(loc,"ui.inventory.equipment",lang,"Equipment"), 10*font_scale, "cream", 650))
    slots=["Head","Body","Utility","Accessory"]
    equipped={"head":"iron_head","body":"iron_body"} if state in ("partial","selected","drag-move","drag-split") else {}
    if state=="equipment-head": equipped={"body":"iron_body"}
    for i,name in enumerate(slots):
        row=i//2; col=i%2
        ix=x+col*(tile_w+gap_x); iy=y+row*(tile_h+gap_y)
        label=localize(loc,f"ui.slot.{name}",lang,name)
        item_id=equipped.get(name.lower())
        selected=state=="equipment-head" and name=="Head"
        compatible=state=="equipment-head" and name=="Head"
        out.append(gear_cell(ix,iy,tile_w,tile_h,label,item_id,catalog,svg_path,lang,selected,compatible))


def main_layout(w: int, h: int) -> dict[str, float | int]:
    if w < 900:
        return {"shell_x":16,"shell_y":10,"equip_x":474,"equip_y":69,"equip_w":156,"equip_h":44,"gear_gap_x":8,"gear_gap_y":6,
                "storage_x":18,"storage_y":77,"storage_cell_w":52,"storage_cell_h":52,"storage_gap":4,"storage_cols":7,
                "hotbar_x":18,"hotbar_y":310,"hotbar_cell_w":44,"hotbar_cell_h":44,"hotbar_gap":4,
                "detail_x":472,"detail_y":193,"detail_w":352,"detail_h":177}
    if w < 1500:
        return {"shell_x":14,"shell_y":14,"equip_x":42,"equip_y":127,"equip_w":92,"equip_h":72,"gear_gap_x":12,"gear_gap_y":15,
                "storage_x":282,"storage_y":132,"storage_cell_w":54,"storage_cell_h":54,"storage_gap":5,"storage_cols":9,
                "hotbar_x":282,"hotbar_y":407,"hotbar_cell_w":54,"hotbar_cell_h":54,"hotbar_gap":5,
                "detail_x":832,"detail_y":115,"detail_w":410,"detail_h":510}
    return {"shell_x":22,"shell_y":58,"equip_x":132,"equip_y":241,"equip_w":122,"equip_h":100,"gear_gap_x":16,"gear_gap_y":22,
            "storage_x":455,"storage_y":261,"storage_cell_w":76,"storage_cell_h":76,"storage_gap":8,"storage_cols":9,
            "hotbar_x":455,"hotbar_y":566,"hotbar_cell_w":76,"hotbar_cell_h":76,"hotbar_gap":8,
            "detail_x":1325,"detail_y":224,"detail_w":425,"detail_h":650}


def make_screen(w: int, h: int, lang: str, state: str, bg: Path,
                catalog: dict[str, dict[str, str | int | Path]], loc: dict[str, list[str]], svg_path: Path) -> str:
    label=localize(loc,"ui.crafting.title",lang,"Crafting")
    out=[f'<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" width="{w}" height="{h}" viewBox="0 0 {w} {h}"><defs>{DEFS}</defs>']
    out.extend(shell(svg_path,w,h,bg,localize(loc,"ui.inventory.title",lang,"Inventory"),label))
    layout=main_layout(w,h)
    scale=0.92 if w<900 else 1.0 if w<1500 else 1.35
    sx=float(layout["storage_x"]); sy=float(layout["storage_y"])
    storage: list[tuple[str,int]]=[]
    if state=="storage-full":
        storage=[(item_id,int(catalog[item_id]["max"])) for item_id in FULL_IDS]
    elif state=="equipment-head":
        storage=[("iron_head",1),("wood",10),("iron_sword",1)]+[("",0)]*24
    elif state in ("partial","selected","drag-move","drag-split","split-modal","item-actions"):
        storage=[("",0)]*27
    selected="iron_sword" if state in ("partial","selected") else "iron_head" if state=="equipment-head" else "iron_head" if state=="storage-full" else ""
    positions=storage_grid(out,svg_path,catalog,loc,lang,state,sx,sy,float(layout["storage_cell_w"]),float(layout["storage_cell_h"]),float(layout["storage_gap"]),int(layout["storage_cols"]),storage,selected if selected else None)
    gear_y=float(layout["equip_y"]); gear_x=float(layout["equip_x"])
    equipment_zone(out,svg_path,catalog,loc,lang,state,gear_x,gear_y,float(layout["equip_w"]),float(layout["equip_h"]),float(layout["gear_gap_x"]),float(layout["gear_gap_y"]),scale)
    hotbar(out,svg_path,catalog,loc,lang,state,float(layout["hotbar_x"]),float(layout["hotbar_y"]),float(layout["hotbar_cell_w"]),float(layout["hotbar_cell_h"]),float(layout["hotbar_gap"]),None if state in ("equipment-head","storage-full") else 0 if state in ("split-modal","item-actions","drag-move","drag-split") else 1)
    dx=float(layout["detail_x"]); dy=float(layout["detail_y"]); dw=float(layout["detail_w"]); dh=float(layout["detail_h"])
    out.append(line(dx-11,dy-2,dx-11,dy+dh,"line",1.2,.72))
    if w < 900:
        out.append(line(dx+1,dy-9,dx+dw-7,dy-9,"line_dim",1.0,.75))
        content_y=dy+15
    elif w < 1500:
        content_y=dy+24
    else:
        content_y=dy+38
    out.append(line(dx+1,content_y-14,dx+dw-7,content_y-14,"line_dim",1.0,.85))
    if state in ("equipment-head","storage-full"):
        item_id="iron_head"; count=1
        icon_size=48 if w<900 else 92 if w<1500 else 124
        out.append(image(svg_path,Path(catalog[item_id]["icon"]),dx+4,content_y,icon_size,icon_size))
        name=localize(loc,"item.iron_head.name",lang,str(catalog[item_id]["display"]))
        out.append(text(dx+icon_size+12,content_y+16,name,14 if w<900 else 20 if w<1500 else 27,"cream",700))
        out.append(text(dx+icon_size+12,content_y+34,localize(loc,"ui.item_category.Armor",lang,"Armor"),10.5 if w<900 else 12 if w<1500 else 16,"muted",500))
        out.append(text(dx+4,content_y+(62 if w<900 else icon_size+19),localize(loc,"ui.inventory.armor_detail",lang,"{0} · Defense +{1}").format(localize(loc,"ui.slot.Head",lang,"Head"),2),11 if w<900 else 15 if w<1500 else 20,"cream",600))
        comparison=localize(loc,"ui.inventory.compare",lang,"Defense {0} → {1} ({2:+#;-#;0})")
        comparison=comparison.replace("{0}","0").replace("{1}","2").replace("{2:+#;-#;0}","+2")
        out.append(text(dx+4,content_y+(80 if w<900 else icon_size+40),comparison,10 if w<900 else 13 if w<1500 else 18,"moss",600))
        out.append(button(dx+dw-132,dy+dh-(48 if w<900 else 48),124,44,localize(loc,"ui.action.equip",lang,"Equip"),"primary",10 if w<900 else 15 if w<1500 else 20))
    else:
        item_id="iron_sword" if state in ("partial","selected") else "wood"
        count=1 if item_id=="iron_sword" else 10
        if state in ("split-modal","item-actions","drag-move","drag-split"): item_id="wood"; count=10 if state!="drag-split" else 5
        icon_size=48 if w<900 else 84 if w<1500 else 110
        out.append(image(svg_path,Path(catalog[item_id]["icon"]),dx+4,content_y,icon_size,icon_size))
        name=localize(loc,f"item.{item_id}.name",lang,str(catalog[item_id]["display"]))
        out.append(text(dx+icon_size+12,content_y+16,name,13 if w<900 else 20 if w<1500 else 27,"cream",700))
        item_category="Weapon" if item_id=="iron_sword" else "Material"
        out.append(text(dx+icon_size+12,content_y+34,localize(loc,f"ui.item_category.{item_category}",lang,item_category),10.5 if w<900 else 12 if w<1500 else 16,"muted",500))
        if item_id=="iron_sword":
            stats=localize(loc,"ui.weapon.stats",lang,"Damage {0}\nAttacks/s {1}\nRange {2} m").format(36,"2.63","1.6")
            lines=stats.splitlines()
            stats_y=icon_size+16 if w>=900 else 64
            for i,s in enumerate(lines): out.append(text(dx+4,content_y+(stats_y+i*(18 if w<900 else 25)),s,11 if w<900 else 14 if w<1500 else 19,"cream",550))
            if state not in ("drag-move","drag-split"):
                out.append(button(dx+dw-132,dy+dh-48,124,44,localize(loc,"ui.action.split",lang,"Split"),"disabled",10 if w<900 else 15 if w<1500 else 20))
        else:
            if state not in ("drag-move","drag-split"):
                out.append(button(dx+dw-132,dy+dh-48,124,44,localize(loc,"ui.action.split",lang,"Split"),"primary",10 if w<900 else 15 if w<1500 else 20))
    if state=="item-actions":
        mx=w*.25; my=h*.21; mw=w*.50; mh=h*.55
        out.append(rect(0,0,w,h,"#050a08",opacity=.46))
        out.append(rect(mx-3,my+3,mw+6,mh+6,"#050a08",opacity=.62))
        out.append(polygon(f"M{mx+10:g} {my:g}H{mx+mw-10:g}L{mx+mw:g} {my+10:g}V{my+mh-10:g}L{mx+mw-10:g} {my+mh:g}H{mx+10:g}L{mx:g} {my+mh-10:g}V{my+10:g}Z", "#293a31", "#101915", 3))
        out.append(rect(mx+3,my+3,mw-6,mh-6,"none",COLORS["line"],1))
        out.append(image(svg_path,Path(catalog["wood"]["icon"]),mx+20,my+18,48,48))
        out.append(text(mx+78,my+47,localize(loc,"item.wood.name",lang,"Wood"),15,"cream",700))
        out.append(button(mx+24,my+mh-104,mw-48,46,localize(loc,"ui.action.split",lang,"Split"),"secondary",13))
        out.append(button(mx+24,my+mh-52,mw-48,44,localize(loc,"ui.action.cancel",lang,"Cancel"),"secondary",13))
    if state=="split-modal":
        mx=w*.12 if w<900 else w*.34; my=h*.17 if w<900 else h*.25; mw=w*.76 if w<900 else w*.36; mh=h*.66 if w<900 else h*.49
        out.append(rect(0,0,w,h,"#050a08",opacity=.46))
        out.append(rect(mx-3,my+3,mw+6,mh+6,"#050a08",opacity=.62))
        out.append(polygon(f"M{mx+10:g} {my:g}H{mx+mw-10:g}L{mx+mw:g} {my+10:g}V{my+mh-10:g}L{mx+mw-10:g} {my+mh:g}H{mx+10:g}L{mx:g} {my+mh-10:g}V{my+10:g}Z", "#293a31", "#101915", 3))
        out.append(rect(mx+3,my+3,mw-6,mh-6,"none",COLORS["line"],1))
        out.append(text(mx+16,my+28,localize(loc,"ui.action.split",lang,"Split"),14 if w<900 else 18,"cream",700))
        out.append(image(svg_path,Path(catalog["wood"]["icon"]),mx+18,my+42,44,44))
        out.append(text(mx+76,my+61,localize(loc,"item.wood.name",lang,"Wood"),12 if w<900 else 16,"cream",650))
        out.append(text(mx+76,my+80,"5 / 10",12 if w<900 else 16,"amber",700))
        out.append(text(mx+mw/2,my+117,localize(loc,"ui.split.instruction",lang,"Choose amount to split"),12 if w<900 else 14,"muted",500,"middle"))
        by=my+mh-(52 if w<900 else 45); bh=44 if w<900 else 42; gap=5; bw=(mw-32-4*gap)/5
        labels=["−1","+1",localize(loc,"ui.split.half",lang,"Half"),localize(loc,"ui.split.confirm",lang,"Confirm"),localize(loc,"ui.action.cancel",lang,"Cancel")]
        for i,label in enumerate(labels): out.append(button(mx+16+i*(bw+gap),by,bw,bh,label,"secondary" if i<3 or i==4 else "primary",11 if w<900 else 12))
    if state in ("drag-move","drag-split"):
        drop_h=44 if w<900 else 40
        out.append(rect(dx+5,dy+dh-drop_h-4,dw-13,drop_h,"#17251e",COLORS["moss"],1.2,0,.92))
        out.append(text(dx+dw/2,dy+dh-drop_h/2+4,localize(loc,"ui.inventory.drop",lang,"Drop to ground"),11 if w<900 else 13,"cream",600,"middle"))
    out.append("</svg>")
    return "".join(out)


def component_sheet() -> str:
    kit_defs=DEFS.replace("../../../../client/", "../../client/")
    out=['<svg xmlns="http://www.w3.org/2000/svg" width="1120" height="390" viewBox="0 0 1120 390"><defs>'+kit_defs+'</defs>',
         rect(0,0,1120,390,"#18231e"), text(32,44,"LOWFIRE · INVENTORY UI KIT",22,"cream",750),
         text(32,72,"9-slice ready vector states · hand authored · no copied game art",12,"muted",500)]
    labels=[("slot-default","Slot · idle"),("slot-selected","Slot · selected"),("slot-compatible","Slot · compatible"),("slot-blocked","Slot · incompatible")]
    for i,(symbol,label) in enumerate(labels):
        x=32+i*155; out.append(f'<use href="#{symbol}" x="{x}" y="104" width="72" height="72"/>'); out.append(text(x+36,194,label,11,"cream",550,"middle"))
    buttons=[("button-primary","Action · primary"),("button-secondary","Action · secondary"),("button-pressed","Action · pressed"),("button-disabled","Action · disabled")]
    for i,(symbol,label) in enumerate(buttons):
        x=32+i*260; out.append(f'<use href="#{symbol}" x="{x}" y="245" width="228" height="68"/>'); out.append(text(x+114,340,label,12,"cream",600,"middle"))
    out.append(text(1088,378,"Cuts: 8px corners · preserve 8px cap/edges",9,"muted",450,"end")); out.append("</svg>")
    return "".join(out)


def before_after(bg: Path, right_svg: Path, out_path: Path) -> str:
    rel_bg=escape_text(link_path(out_path,bg)); rel_after=escape_text(link_path(out_path,right_svg))
    return (f'<svg xmlns="http://www.w3.org/2000/svg" width="1688" height="428" viewBox="0 0 1688 428"><defs>{DEFS}</defs>'
            f'{rect(0,0,1688,428,"#101915")}{text(12,25,"BEFORE · REAL UNITY CAPTURE · 844×390 · 2026-11-02 source commit 0ae9fea",12,"cream",650)}'
            f'{text(856,25,"AFTER · STATIC SVG COMPOSITE · SAME CAPTURE BACKGROUND",12,"amber",650)}'
            f'<image href="{rel_bg}" x="0" y="34" width="844" height="390" preserveAspectRatio="none"/>'
            f'<image href="{rel_after}" x="844" y="34" width="844" height="390" preserveAspectRatio="none"/>'
            f'</svg>')


def main() -> None:
    PREVIEWS.mkdir(parents=True, exist_ok=True); SVG_DIR.mkdir(parents=True, exist_ok=True); SOURCE.mkdir(parents=True, exist_ok=True)
    loc=load_localization(); catalog=item_catalog()
    missing=[item_id for item_id in FULL_IDS if item_id not in catalog]
    if missing: raise SystemExit(f"Missing existing ItemDefinition icon/data: {missing}")
    kit=SOURCE/"inventory-ui-kit.svg"; kit.write_text(component_sheet(),encoding="utf-8")
    specs=[(844,390,"mobile"),(1280,720,"desktop"),(1920,1080,"desktop")]
    states=["partial","equipment-head","storage-full","item-actions","split-modal","drag-move","drag-split"]
    for w,h,layout in specs:
        for lang in ("ko","en"):
            if w==844 and lang=="en":
                # The before/after plate must use the exact same historic Unity frame.
                bg=OLDER_CAPTURE
            else:
                bg=AUDIT/f"resume-v2-inventory-{lang}-{w}.png"
                if w==844 and not bg.exists(): bg=AUDIT/f"resume-v2-inventory-{lang}-844.png"
            out=SVG_DIR/f"inventory-{layout}-partial-{lang}-{w}x{h}.svg"
            out.write_text(make_screen(w,h,lang,"partial",bg,catalog,loc,out),encoding="utf-8")
    for lang in ("ko","en"):
        for state in states[1:]:
            bg=AUDIT/f"resume-v2-inventory-{lang}-844.png"
            if state in ("partial","selected","drag-move","drag-split") and lang=="en": bg=OLDER_CAPTURE
            out=SVG_DIR/f"inventory-mobile-{state}-{lang}-844x390.svg"
            out.write_text(make_screen(844,390,lang,state,bg,catalog,loc,out),encoding="utf-8")
    en_after=SVG_DIR/"inventory-mobile-partial-en-844x390.svg"
    comp=SVG_DIR/"before-after-844-en.svg"
    comp.write_text(before_after(OLDER_CAPTURE,en_after,comp),encoding="utf-8")
    print(f"Generated {len(list(SVG_DIR.glob('*.svg')))} editable SVG previews and component kit.")
    print(f"Catalog checked: {len(catalog)} existing ItemDefinition assets; Full fixture: {len(FULL_IDS)} unique IDs.")


if __name__=="__main__": main()
