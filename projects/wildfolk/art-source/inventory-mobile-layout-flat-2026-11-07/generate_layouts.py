#!/usr/bin/env python3
"""Generate two flat 844x390 inventory layout studies from current Lowfire assets."""
from __future__ import annotations

import importlib.util
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = Path(__file__).resolve().parent
SCREENS = SOURCE / "screens"
VALIDATION = ROOT / "validation" / "inventory-mobile-layout-flat-2026-11-07"
PREVIEWS = VALIDATION / "previews"
PHASE3D = ROOT / "art-source" / "inventory-ui-phase3d-2026-11-06" / "generate_previews.py"
BACKGROUND = ROOT / "docs" / "images" / "lowfire-player-world-armor.png"
W, H = 844, 390

spec = importlib.util.spec_from_file_location("lowfire_phase3d", PHASE3D)
if spec is None or spec.loader is None:
    raise RuntimeError(f"Could not load current asset resolver: {PHASE3D}")
phase3d = importlib.util.module_from_spec(spec)
spec.loader.exec_module(phase3d)

CATALOG = phase3d.item_catalog()
LOC = phase3d.localization()
RECIPE_BOOK = phase3d.recipes()
HOTBAR = phase3d.HOTBAR
STORAGE = phase3d.STORAGE
EQUIPPED = phase3d.EQUIPPED
CRAFT_IDS = ("campfire", "torch", "workbench")

PANEL = "#1e382c"
PANEL_INNER = "#294535"
CELL = "#263f31"
CELL_EMPTY = "#22392d"
EDGE = "#73896a"
EDGE_DARK = "#14261d"
TEXT = "#f1edcf"
TEXT_SOFT = "#c9d1ae"
EMBER = "#e28b4b"
MOSS = "#a9c67a"
DIM = "#74806b"


def esc(value: object) -> str:
    import html
    return html.escape(str(value), quote=True)


def tr(key: str, index: int = 1, fallback: str = "") -> str:
    values = LOC.get(key, [])
    return str(values[index]) if len(values) > index else fallback


def item_name(item_id: str | None) -> str:
    if not item_id:
        return tr("ui.inventory.empty", fallback="비어 있음")
    return phase3d.name_for(LOC, item_id, CATALOG)


def rel(source_svg: Path, target: Path) -> str:
    import os
    return Path(os.path.relpath(target, source_svg.parent)).as_posix()


def text(x: float, y: float, label: object, size: float = 10,
         fill: str = TEXT, weight: int = 600, anchor: str = "start") -> str:
    return (f'<text x="{x:g}" y="{y:g}" font-family="Lowfire, Malgun Gothic, sans-serif" '
            f'font-size="{size:g}" font-weight="{weight}" fill="{fill}" '
            f'text-anchor="{anchor}">{esc(label)}</text>')


def icon(svg: Path, item_id: str, x: float, y: float, w: float, h: float,
         opacity: float = 1) -> str:
    if item_id not in CATALOG:
        raise KeyError(f"Missing current Lowfire icon: {item_id}")
    return (f'<image href="{esc(rel(svg, Path(CATALOG[item_id]["icon"]))) }" '
            f'x="{x:g}" y="{y:g}" width="{w:g}" height="{h:g}" '
            f'preserveAspectRatio="xMidYMid meet" opacity="{opacity:g}"/>')


def panel(x: float, y: float, w: float, h: float, title: str | None = None,
          right_label: str | None = None) -> list[str]:
    out = [
        f'<rect x="{x:g}" y="{y:g}" width="{w:g}" height="{h:g}" rx="8" '
        f'fill="{PANEL}" fill-opacity=".96" stroke="{EDGE}" stroke-width="1.1"/>',
    ]
    if title is not None:
        out.append(text(x + 13, y + 20, title, 11.5, TEXT, 750))
    if right_label is not None:
        out.append(text(x + w - 13, y + 20, right_label, 10, TEXT_SOFT, 550, "end"))
    return out


def close_button() -> list[str]:
    # 44x44 touch target; visual X remains compact.
    return [
        f'<rect x="786" y="4" width="44" height="44" rx="8" fill="{PANEL}" '
        f'stroke="{EDGE}" stroke-width="1"/>',
        '<path d="M802 16 L814 28 M814 16 L802 28" stroke="#e7ebd1" '
        'stroke-width="2.2" stroke-linecap="round"/>',
    ]


def craft_panel(svg: Path) -> list[str]:
    out = panel(16, 49, 208, 110, tr("ui.crafting.hand", fallback="손제작"))
    out.append(f'<path d="M27 69 H213" stroke="{EDGE}" stroke-opacity=".42" stroke-width=".8"/>')
    y_positions = (75, 102, 129)
    for recipe_id, y in zip(CRAFT_IDS, y_positions):
        recipe = RECIPE_BOOK.get(recipe_id)
        if not recipe or not recipe.get("result"):
            raise RuntimeError(f"Current hand-craft recipe did not resolve: {recipe_id}")
        result_id = str(recipe["result"])
        out.extend([
            f'<rect x="26" y="{y}" width="188" height="23" rx="5" fill="{PANEL_INNER}"/>',
            icon(svg, result_id, 30, y + 2, 20, 19),
            text(56, y + 15.5, item_name(result_id), 9.4, TEXT, 600),
            text(207, y + 15.5, "›", 13, TEXT_SOFT, 500, "end"),
        ])
    return out


def equipment_panel(svg: Path) -> list[str]:
    out = panel(16, 168, 208, 206, tr("ui.inventory.equipment", fallback="장비"), "4")
    specs = [
        ("Head", "ui.slot.Head", EQUIPPED.get("Head")),
        ("Body", "ui.slot.Body", EQUIPPED.get("Body")),
        ("Utility", "ui.slot.Utility", EQUIPPED.get("Utility")),
        ("Accessory", "ui.slot.Accessory", EQUIPPED.get("Accessory")),
    ]
    positions = ((25, 197), (123, 197), (25, 273), (123, 273))
    for (slot_key, label_key, item_id), (x, y) in zip(specs, positions):
        label = tr(label_key, fallback=slot_key)
        out.extend([
            f'<rect x="{x}" y="{y}" width="92" height="69" rx="6" fill="{PANEL_INNER}" '
            f'stroke="{EDGE}" stroke-opacity=".72" stroke-width=".8"/>',
            text(x + 7, y + 14, label, 9.4, TEXT_SOFT, 650),
        ])
        if item_id:
            out.extend([
                icon(svg, str(item_id), x + 6, y + 22, 30, 34),
                text(x + 39, y + 43, item_name(str(item_id)), 10.1, TEXT, 650),
            ])
        else:
            out.append(text(x + 7, y + 45, item_name(None), 9.5, DIM, 500))
    return out


def slot(svg: Path, x: float, y: float, w: float, h: float,
         item_id: str | None, count: int, index: int | None = None,
         selected: bool = False) -> list[str]:
    fill = CELL if item_id else CELL_EMPTY
    stroke = EMBER if selected else EDGE
    width = 1.7 if selected else .85
    out = [f'<rect x="{x:g}" y="{y:g}" width="{w:g}" height="{h:g}" rx="5" '
           f'fill="{fill}" stroke="{stroke}" stroke-width="{width:g}"/>']
    if selected:
        out.append(f'<path d="M{x+5:g} {y+2:g} H{x+w-5:g}" stroke="{EMBER}" stroke-width="2.2"/>')
    if item_id:
        size = min(36, h - 15, w - 10)
        out.append(icon(svg, item_id, x + (w - size) / 2, y + 4, size, size))
        if count > 1:
            badge_w = max(21, len(str(count)) * 7 + 9)
            bx, by = x + w - badge_w - 2, y + h - 16
            out.extend([
                f'<rect x="{bx:g}" y="{by:g}" width="{badge_w:g}" height="14" rx="4" '
                f'fill="{EDGE_DARK}" fill-opacity=".94"/>',
                text(x + w - 5, y + h - 5, count, 11.5, TEXT, 800, "end"),
            ])
    if index is not None:
        out.append(text(x + 4, y + 9, index, 8, TEXT_SOFT, 700))
    return out


def hotbar_panel(svg: Path) -> list[str]:
    out = panel(236, 49, 592, 58)
    out.extend([
        text(248, 72, tr("ui.inventory.hotbar", fallback="핫바"), 10.4, TEXT, 700),
        text(248, 87, "9", 8.5, TEXT_SOFT, 550),
    ])
    cell_w, cell_h, gap = 54, 44, 3
    total = 9 * cell_w + 8 * gap
    start_x = 236 + (592 - total) / 2
    y = 56
    for index, (item_id, count) in enumerate(HOTBAR, 1):
        out.extend(slot(svg, start_x + (index - 1) * (cell_w + gap), y,
                        cell_w, cell_h, item_id, count, index=index, selected=index == 1))
    return out


def storage_panel(svg: Path, paged: bool) -> list[str]:
    end = 18 if paged else 27
    right = "01–18 / 27" if paged else "27칸 모두 표시"
    out = panel(236, 111, 592, 263, tr("ui.inventory.storage", fallback="보관함"), right)
    if paged:
        cell_w, cell_h, cols, gap_x, gap_y, start_x = 82, 57, 6, 5, 4, 273
        shown = STORAGE[:end]
    else:
        cell_w, cell_h, cols, gap_x, gap_y, start_x = 56, 57, 9, 4, 4, 264
        shown = STORAGE
    start_y = 146
    for i, (item_id, count) in enumerate(shown):
        x = start_x + (i % cols) * (cell_w + gap_x)
        y = start_y + (i // cols) * (cell_h + gap_y)
        out.extend(slot(svg, x, y, cell_w, cell_h, item_id, count))
    if paged:
        # Each navigation target is 44x44. Page 1 is active; previous is visibly disabled.
        y, group_x = 326, 451
        out.extend([
            f'<rect x="{group_x}" y="{y}" width="44" height="44" rx="7" fill="{CELL_EMPTY}" '
            f'stroke="{DIM}" stroke-width=".9"/>',
            text(group_x + 22, y + 28, "‹", 20, DIM, 500, "middle"),
            f'<rect x="{group_x+52}" y="{y}" width="58" height="44" rx="7" fill="{PANEL_INNER}" '
            f'stroke="{EDGE}" stroke-width=".9"/>',
            text(group_x + 81, y + 27, "1 / 2", 12, TEXT, 700, "middle"),
            f'<rect x="{group_x+118}" y="{y}" width="44" height="44" rx="7" fill="{PANEL_INNER}" '
            f'stroke="{EDGE}" stroke-width=".9"/>',
            text(group_x + 140, y + 28, "›", 20, TEXT, 650, "middle"),
        ])
    else:
        out.append(text(532, 352, "1–27", 9, TEXT_SOFT, 550, "middle"))
    return out


def screen(filename: str, paged: bool) -> None:
    path = SCREENS / filename
    path.parent.mkdir(parents=True, exist_ok=True)
    out = [
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}">',
        '<defs>',
        '<style>@font-face{font-family:Lowfire;src:url("../../../client/Assets/Wildfolk/Resources/NotoSansKR.ttf") format("truetype");font-weight:100 900}</style>',
        '</defs>',
        f'<image href="{esc(rel(path, BACKGROUND))}" x="0" y="0" width="{W}" height="{H}" preserveAspectRatio="xMidYMid slice"/>',
        '<rect width="844" height="390" fill="#0e1d16" fill-opacity=".38"/>',
        '<path d="M0 47 H844" stroke="#a6ba7e" stroke-opacity=".26" stroke-width="1"/>',
        text(17, 31, "LOWFIRE", 13, TEXT, 800),
        text(91, 31, "·", 12, MOSS, 700),
        text(104, 31, tr("ui.inventory.title", fallback="인벤토리"), 12, TEXT_SOFT, 650),
    ]
    out.extend(close_button())
    out.extend(craft_panel(path))
    out.extend(equipment_panel(path))
    out.extend(hotbar_panel(path))
    out.extend(storage_panel(path, paged))
    out.append("</svg>")
    path.write_text("\n".join(out) + "\n", encoding="utf-8")


def main() -> None:
    if len(HOTBAR) != 9 or len(STORAGE) != 27 or len(EQUIPPED) != 4:
        raise RuntimeError("The visual fixture no longer matches the canonical 9+27+4 inventory.")
    unknown = {item for item, _ in [*HOTBAR, *STORAGE] if item and item not in CATALOG}
    unknown.update(item for item in EQUIPPED.values() if item and item not in CATALOG)
    if unknown:
        raise RuntimeError(f"Current Lowfire assets are missing: {sorted(unknown)}")
    SCREENS.mkdir(parents=True, exist_ok=True)
    PREVIEWS.mkdir(parents=True, exist_ok=True)
    screen("A-all27.svg", paged=False)
    screen("B-page18.svg", paged=True)
    print(f"Generated A (27 visible) and B (18/page) from {len(CATALOG)} current Lowfire item icons.")


if __name__ == "__main__":
    main()
