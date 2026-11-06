#!/usr/bin/env python3
"""Build six 844x390 Korean inventory structure studies from Lowfire assets."""
from __future__ import annotations

import html
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = Path(__file__).resolve().parent
VALIDATION = ROOT / "validation" / "inventory-ui-phase3b-2026-11-06"
PREVIEWS = VALIDATION / "previews"
SVG_DIR = PREVIEWS / "svg"
ART = ROOT / "client" / "Assets" / "Wildfolk" / "Art"
DATA = ROOT / "client" / "Assets" / "Wildfolk" / "Data"
RESOURCES = ROOT / "client" / "Assets" / "Wildfolk" / "Resources"
BACKGROUND = ROOT / "validation" / "penpot-ui-ux-audit-2026-11-04" / "evidence" / "live" / "qa-new-world-hud-ko-844.png"
WIDTH, HEIGHT = 844, 390

# All IDs below resolve to existing ItemDefinition assets with existing Lowfire sprites.
STORAGE = [
    "iron_head", "wood", "stone", "berry", "mushroom", "plant_fiber", "leather",
    "iron_ore", "copper_ore", "tin_ore", "iron_ingot", "torch", "arrow", "bone_arrow",
    "bronze_arrow", "crossbow_bolt", "mushroom_stew", "roasted_mushroom", "raw_water_cup",
    "clean_water_cup", "waterskin", "bow", "iron_sword", "iron_axe", "iron_pickaxe",
    "leather_head", "leather_body",
]
HOTBAR = [
    ("wood", 10), ("iron_sword", 1), ("torch", 8), ("waterskin", 1), ("berry", 12),
    ("iron_axe", 1), ("arrow", 18), ("mushroom_stew", 1), ("iron_pickaxe", 1),
]
COUNTS = {
    "iron_head": 1, "wood": 24, "stone": 18, "berry": 6, "mushroom": 5,
    "plant_fiber": 12, "leather": 6, "iron_ore": 8, "copper_ore": 7, "tin_ore": 5,
    "iron_ingot": 4, "torch": 8, "arrow": 18, "bone_arrow": 12, "bronze_arrow": 10,
    "crossbow_bolt": 6, "mushroom_stew": 2, "roasted_mushroom": 4, "raw_water_cup": 1,
    "clean_water_cup": 1, "waterskin": 1, "bow": 1, "iron_sword": 1, "iron_axe": 1,
    "iron_pickaxe": 1, "leather_head": 1, "leather_body": 1,
}
EQUIPPED = {"Head": "leather_head", "Body": "leather_body", "Utility": "lantern", "Accessory": None}

COLORS = {
    "ink": "#101915", "surface": "#18241e", "surface2": "#202f27", "slot": "#15211b",
    "line": "#72816d", "line2": "#536353", "cream": "#eee5cd", "muted": "#b8bea9",
    "ember": "#e69a60", "moss": "#b1c782", "red": "#d27d64",
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
            }
    return result


def localization() -> dict[str, list[str]]:
    source = json.loads((RESOURCES / "Localization.json").read_text(encoding="utf-8"))
    return {row["Key"]: row["Values"] for row in source["Entries"]}


def ko(loc: dict[str, list[str]], key: str, fallback: str = "") -> str:
    values = loc.get(key)
    return values[1] if values and len(values) > 1 else fallback or key


def name(loc: dict[str, list[str]], item_id: str) -> str:
    return ko(loc, f"item.{item_id}.name", item_id)


def rel(svg: Path, asset: Path) -> str:
    return Path(__import__("os").path.relpath(asset, svg.parent)).as_posix()


def img(svg: Path, asset: Path, x: float, y: float, w: float, h: float, opacity: float = 1) -> str:
    return (f'<image href="{esc(rel(svg, asset))}" x="{x:g}" y="{y:g}" width="{w:g}" height="{h:g}" '
            f'preserveAspectRatio="xMidYMid meet" opacity="{opacity:g}"/>')


def rect(x: float, y: float, w: float, h: float, fill: str, opacity: float = 1,
         stroke: str = "none", sw: float = 1, rx: float = 0) -> str:
    return (f'<rect x="{x:g}" y="{y:g}" width="{w:g}" height="{h:g}" rx="{rx:g}" '
            f'fill="{fill}" fill-opacity="{opacity:g}" stroke="{stroke}" stroke-width="{sw:g}"/>')


def text(x: float, y: float, value: object, size: float = 12, color: str = "cream",
         weight: int = 500, anchor: str = "start", opacity: float = 1) -> str:
    return (f'<text x="{x:g}" y="{y:g}" font-size="{size:g}" fill="{COLORS.get(color,color)}" '
            f'font-weight="{weight}" text-anchor="{anchor}" opacity="{opacity:g}">{esc(value)}</text>')


def line(x1: float, y1: float, x2: float, y2: float, color: str = "line2", width: float = 1,
         opacity: float = 1, dash: str = "") -> str:
    ds = f' stroke-dasharray="{dash}"' if dash else ""
    return (f'<path d="M{x1:g} {y1:g}L{x2:g} {y2:g}" fill="none" stroke="{COLORS.get(color,color)}" '
            f'stroke-width="{width:g}" opacity="{opacity:g}"{ds}/>')


def button(x: float, y: float, w: float, h: float, label: str, state: str = "quiet",
           font: float = 12) -> str:
    fill = {"primary": "#ad6240", "quiet": "#25352b", "active": "#344539", "disabled": "#202923"}[state]
    ink = "#7e867a" if state == "disabled" else COLORS["cream"]
    border = COLORS["ember"] if state == "primary" else COLORS["line2"]
    return (rect(x,y,w,h,fill,.98,border,1 if state!="primary" else 1.5,7) +
            text(x+w/2,y+h/2+font*.35,label,font,ink,700,"middle"))


def slot(x: float, y: float, w: float, h: float, state: str = "normal") -> str:
    stroke = {"selected": "ember", "target": "moss", "normal": "line2"}[state]
    width = 2.2 if state == "selected" else 2 if state == "target" else 1
    return rect(x,y,w,h,COLORS["slot"],.96,COLORS[stroke],width,6)


def item_cell(out: list[str], svg: Path, catalog: dict[str, dict[str, object]], item_id: str | None,
              count: int, x: float, y: float, w: float, h: float, state: str = "normal",
              number: str | None = None, icon_scale: float = .69) -> None:
    out.append(slot(x,y,w,h,state))
    if item_id and item_id in catalog:
        pad_x=(w-w*icon_scale)/2; pad_y=(h-h*icon_scale)/2 - 1
        out.append(img(svg,Path(catalog[item_id]["icon"]),x+pad_x,y+pad_y,w*icon_scale,h*icon_scale))
        if count > 0:
            label=f"{count}"
            if int(catalog[item_id].get("water",0)) > 0:
                label=f"{max(1,int(catalog[item_id]['water'])-1)}/{catalog[item_id]['water']}"
            badge=max(18,7+len(label)*6)
            out.append(rect(x+w-badge-2,y+h-14,badge,11,COLORS["ink"],.95,"#455447",.5,4))
            out.append(text(x+w-4,y+h-5,label,8.5,"cream",700,"end"))
    if number:
        out.append(text(x+4,y+10,number,8.5,"muted",600))


def gear_tile(out: list[str], svg: Path, catalog: dict[str, dict[str, object]], loc: dict[str, list[str]],
              label_key: str, item_id: str | None, x: float, y: float, w: float, h: float,
              state: str = "normal", label_override: str | None = None) -> None:
    border=COLORS["ember"] if state=="selected" else COLORS["moss"] if state=="target" else COLORS["line2"]
    fill="#302d24" if state=="target" else COLORS["surface2"]
    out.append(rect(x,y,w,h,fill,.97,border,1.7 if state!="normal" else .9,8))
    label=label_override or ko(loc,f"ui.slot.{label_key}",label_key)
    box=min(42,h*.54)
    if item_id and item_id in catalog:
        out.append(img(svg,Path(catalog[item_id]["icon"]),x+(w-box)/2,y+5,box,box))
    else:
        out.append(line(x+w*.39,y+box/2+5,x+w*.61,y+box/2+5,"line",1.5,.7))
    out.append(text(x+w/2,y+h-7,label,10.5,"cream",650,"middle"))


def start(svg: Path, title: str) -> list[str]:
    font = rel(svg, RESOURCES / "NotoSansKR.ttf")
    out=[f'<svg xmlns="http://www.w3.org/2000/svg" width="{WIDTH}" height="{HEIGHT}" viewBox="0 0 {WIDTH} {HEIGHT}">',
         '<defs><linearGradient id="scrim" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#0d1511" stop-opacity=".64"/><stop offset="1" stop-color="#101914" stop-opacity=".77"/></linearGradient>',
         '<marker id="arrow" markerWidth="8" markerHeight="8" refX="6" refY="3" orient="auto"><path d="M0 0L6 3L0 6Z" fill="#e69a60"/></marker>',
         f'<style>@font-face{{font-family:Lowfire;src:url("{esc(font)}") format("truetype");font-weight:100 900}}text{{font-family:Lowfire,"Malgun Gothic","Segoe UI",sans-serif}}</style></defs>',
         img(svg,BACKGROUND,0,0,WIDTH,HEIGHT),rect(0,0,WIDTH,HEIGHT,"url(#scrim)",1),
         rect(12,4,820,42,COLORS["surface"],.96,COLORS["line2"],.8,8),
         rect(0,336,844,54,COLORS["ink"],.96)]
    out.append(text(22,28,title,18,"cream",750))
    out.append(button(690,5,88,40,"제작","quiet",12))
    out.append(button(784,5,44,40,"×","quiet",19))
    out.append(line(16,45,828,45,"line2",.8,.8))
    return out


def fill_storage(out: list[str], svg: Path, catalog: dict[str, dict[str, object]],
                 x: float, y: float, cols: int, cw: float, ch: float, gap: float,
                 selected: str | None = None, selected_index: int | None = None,
                 size: int = 27) -> tuple[tuple[float,float], ...]:
    centers=[]
    for i in range(size):
        row,col=divmod(i,cols)
        px=x+col*(cw+gap); py=y+row*(ch+gap)
        item=STORAGE[i]
        count=COUNTS[item]
        max_stack=int(catalog[item]["max"])
        count=min(count,max_stack)
        state="selected" if item==selected or i==selected_index else "normal"
        item_cell(out,svg,catalog,item,count,px,py,cw,ch,state)
        centers.append((px+cw/2,py+ch/2))
    return tuple(centers)


def draw_hotbar(out: list[str], svg: Path, catalog: dict[str, dict[str, object]],
                x: float, y: float, cw: float = 50, ch: float = 45, gap: float = 5,
                selected: int | None = None) -> None:
    for i,(item,count) in enumerate(HOTBAR):
        item_cell(out,svg,catalog,item,count,x+i*(cw+gap),y,cw,ch,"selected" if i==selected else "normal",str(i+1),.67)


def a_screen(state: str, svg: Path, catalog: dict[str, dict[str, object]], loc: dict[str, list[str]]) -> str:
    out=start(svg,"인벤토리")
    out.append(text(22,62,"보관함  ·  27",12,"muted",650))
    positions=fill_storage(out,svg,catalog,22,70,9,56,55,5,
                           selected="wood" if state=="selected" else None,
                           selected_index=0 if state=="equipment" else None)
    out.append(text(22,278,"핫바  ·  9",12,"muted",650))
    draw_hotbar(out,svg,catalog,48,286,50,46,5)
    if state=="basic":
        out.append(button(606,76,218,52,"장비  4칸 열기  ›","active",14))
        out.append(text(615,152,"아이템은 끌어 놓아 이동",11,"muted",550))
        out.append(text(615,171,"선택 후 나누기 · 버리기",11,"muted",550))
    elif state=="selected":
        out.append(rect(600,68,228,232,COLORS["surface"],.97,COLORS["line2"],1,9))
        out.append(text(616,91,"선택한 아이템",11,"muted",600))
        out.append(img(svg,Path(catalog["wood"]["icon"]),614,103,47,47))
        out.append(text(670,124,name(loc,"wood"),15,"cream",700))
        out.append(text(670,145,"24개  ·  소재",11,"muted",550))
        out.append(line(614,163,814,163,"line2",.8,.8))
        out.append(button(614,176,96,42,"나누기","quiet",12))
        out.append(button(718,176,96,42,"버리기","quiet",12))
        out.append(line(620,244,657,244,"ember",2,.9))
        out.append(text(666,248,"끌어 다른 칸으로 이동",10.5,"cream",550))
    else:
        out.append(rect(596,57,232,301,COLORS["surface"],.98,COLORS["line2"],1,9))
        out.append(text(610,81,"장비",16,"cream",750))
        out.append(text(811,80,"×",19,"muted",600,"middle"))
        targets=[]
        for idx,(label,item) in enumerate(EQUIPPED.items()):
            row,col=divmod(idx,2)
            px=608+col*104; py=92+row*82
            gear_tile(out,svg,catalog,loc,label,item,px,py,96,72,"target" if label=="Head" else "normal")
            if label=="Head": targets.append((px+48,py+36))
        source=positions[0]
        tx,ty=targets[0]
        out.append(f'<path d="M{source[0]:g} {source[1]:g} C560 116 575 127 {tx-42:g} {ty:g}" fill="none" stroke="{COLORS["ember"]}" stroke-width="2" stroke-dasharray="5 4" marker-end="url(#arrow)"/>')
        out.append(text(610,273,"철 투구  →  머리",11,"cream",650))
        out.append(button(608,289,100,40,"장착","primary",12))
        out.append(button(716,289,100,40,"해제","quiet",12))
    out.append('</svg>')
    return "\n".join(out)


def b_screen(state: str, svg: Path, catalog: dict[str, dict[str, object]], loc: dict[str, list[str]]) -> str:
    out=start(svg,"인벤토리")
    bag_state=state!="equipment"
    out.append(button(22,51,132,40,"가방","active" if bag_state else "quiet",12))
    out.append(button(162,51,132,40,"장비","active" if not bag_state else "quiet",12))
    if bag_state:
        out.append(text(23,98,"보관함  ·  27",12,"muted",650))
        fill_storage(out,svg,catalog,23,105,7,60,47,4,
                     selected="wood" if state=="selected" else None)
        out.append(text(23,319,"핫바  ·  9",12,"muted",650))
        draw_hotbar(out,svg,catalog,23,326,55,45,4)
        if state=="basic":
            out.append(text(500,153,"선택한 아이템의 상세와",11,"muted",550))
            out.append(text(500,171,"나누기 · 버리기가 이곳에 열립니다",11,"muted",550))
        if state=="selected":
            out.append(rect(489,91,337,214,COLORS["surface"],.97,COLORS["line2"],1,9))
            out.append(text(508,115,"선택한 아이템",11,"muted",600))
            out.append(img(svg,Path(catalog["wood"]["icon"]),508,127,56,56))
            out.append(text(576,149,name(loc,"wood"),16,"cream",750))
            out.append(text(576,170,"24개  ·  소재",12,"muted",550))
            out.append(line(508,195,807,195,"line2",.8,.8))
            out.append(button(508,211,142,44,"나누기","quiet",13))
            out.append(button(664,211,142,44,"버리기","quiet",13))
            out.append(line(512,289,551,289,"ember",2,.9))
            out.append(text(562,293,"끌어 다른 칸으로 이동",11,"cream",550))
    else:
        # Equipment is a distinct view; selecting an equipped slot exposes its existing actions.
        out.append(text(24,100,"장비 슬롯  ·  4",12,"muted",650))
        gear_specs=list(EQUIPPED.items())
        for idx,(label,item) in enumerate(gear_specs):
            row,col=divmod(idx,2)
            px=24+col*143; py=111+row*100
            gear_tile(out,svg,catalog,loc,label,item,px,py,132,88,"selected" if label=="Head" else "normal")
        out.append(rect(330,98,496,254,COLORS["surface"],.97,COLORS["line2"],1,9))
        out.append(text(350,123,"선택 장비",11,"muted",600))
        out.append(img(svg,Path(catalog["iron_head"]["icon"]),350,137,58,58))
        out.append(text(420,158,name(loc,"iron_head"),17,"cream",750))
        out.append(text(420,180,"가방에서 선택 · 머리 슬롯",11,"muted",550))
        out.append(text(350,220,"현재 장착: 가죽 모자",11,"muted",550))
        out.append(text(350,244,"철 투구를 머리 슬롯으로 이동",11,"cream",600))
        out.append(button(350,276,210,44,"장착","primary",13))
        out.append(button(574,276,226,44,"해제","quiet",12))
        out.append(f'<path d="M350 164 C278 132 220 135 156 151" fill="none" stroke="{COLORS["ember"]}" stroke-width="2" stroke-dasharray="5 4" marker-end="url(#arrow)"/>')
    out.append('</svg>')
    return "\n".join(out)


def overview(svg: Path) -> str:
    # Contact sheet only; each of the six individual screens remains exactly 844x390.
    scale=.5; iw,ih=WIDTH*scale,HEIGHT*scale; margin=22; label_h=28; gap_y=12; gap_x=20
    total_w=margin*2+iw*2+gap_x; total_h=margin*2+(label_h+ih)*3+gap_y*2
    out=[f'<svg xmlns="http://www.w3.org/2000/svg" width="{total_w:g}" height="{total_h:g}" viewBox="0 0 {total_w:g} {total_h:g}">',
         '<defs><style>text{font-family:"Malgun Gothic","Segoe UI",sans-serif}</style></defs>',rect(0,0,total_w,total_h,"#111a15"),
         text(margin,24,"A  ·  가방 중심형",15,"cream",750),text(margin+iw+gap_x,24,"B  ·  탭 전환형",15,"cream",750)]
    names={"basic":"기본","selected":"아이템 선택","equipment":"장비 확인 / 이동"}
    for idx,(key,a_name) in enumerate([("basic","a-basic"),("selected","a-selected"),("equipment","a-equipment")]):
        y=margin+idx*(label_h+ih+gap_y)
        out.append(text(margin,y+17,names[key],11,"muted",650))
        out.append(text(margin+iw+gap_x,y+17,names[key],11,"muted",650))
        out.append(img(svg,VALIDATION/"previews"/f"{a_name}.png",margin,y+label_h,iw,ih))
        b_name={"basic":"b-basic","selected":"b-selected","equipment":"b-equipment"}[key]
        out.append(img(svg,VALIDATION/"previews"/f"{b_name}.png",margin+iw+gap_x,y+label_h,iw,ih))
    out.append('</svg>')
    return "\n".join(out)


def main() -> None:
    if not BACKGROUND.is_file():
        raise SystemExit(f"Missing existing Lowfire gameplay capture: {BACKGROUND}")
    catalog=item_catalog(); loc=localization()
    required=set(STORAGE+[item for item,_ in HOTBAR]+[item for item in EQUIPPED.values() if item])
    missing=sorted(required-catalog.keys())
    if missing:
        raise SystemExit(f"Missing existing ItemDefinition icons: {missing}")
    SVG_DIR.mkdir(parents=True,exist_ok=True); PREVIEWS.mkdir(parents=True,exist_ok=True)
    for name_,fn in [("a-basic",lambda p:a_screen("basic",p,catalog,loc)),
                     ("a-selected",lambda p:a_screen("selected",p,catalog,loc)),
                     ("a-equipment",lambda p:a_screen("equipment",p,catalog,loc)),
                     ("b-basic",lambda p:b_screen("basic",p,catalog,loc)),
                     ("b-selected",lambda p:b_screen("selected",p,catalog,loc)),
                     ("b-equipment",lambda p:b_screen("equipment",p,catalog,loc))]:
        path=SVG_DIR/f"{name_}.svg"; path.write_text(fn(path),encoding="utf-8")
    sheet=SVG_DIR/"ab-overview.svg"
    sheet.write_text(overview(sheet),encoding="utf-8")
    print(f"Generated 6 editable 844x390 SVG screens from {len(catalog)} current item definitions; no Unity files touched.")


if __name__=="__main__":
    main()
