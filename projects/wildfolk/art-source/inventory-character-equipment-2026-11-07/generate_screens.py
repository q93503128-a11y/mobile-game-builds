#!/usr/bin/env python3
"""Generate A/B equipment-region studies using the current Lowfire art and Phase 3D base."""
from __future__ import annotations

import importlib.util
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = Path(__file__).resolve().parent
SCREENS = SOURCE / "screens"
VALIDATION = ROOT / "validation" / "inventory-character-equipment-2026-11-07"
PREVIEWS = VALIDATION / "previews"
PHASE3D = ROOT / "art-source" / "inventory-ui-phase3d-2026-11-06" / "generate_previews.py"
ART = ROOT / "client" / "Assets" / "Wildfolk" / "Art"

spec = importlib.util.spec_from_file_location("lowfire_phase3d", PHASE3D)
if spec is None or spec.loader is None:
    raise RuntimeError(f"Could not load the Phase 3D asset resolver at {PHASE3D}")
phase = importlib.util.module_from_spec(spec)
spec.loader.exec_module(phase)

CATALOG = phase.item_catalog()
LOC = phase.localization()
RECIPES = phase.recipes()
COUNTS: dict[str, int] = {}
for item_id, count in phase.HOTBAR + phase.STORAGE:
    if item_id:
        COUNTS[item_id] = COUNTS.get(item_id, 0) + count
phase.item_catalog_cache = CATALOG

PANEL = "#573b28"
PANEL_INNER = "#34251b"
SLOT = "#291b13"
PAPER = "#f0d5a7"
MUTED = "#c6a77b"
AMBER = "#f2c176"
GREEN = "#a6d27b"
INK = "#170e09"


def tr(key: str, fallback: str = "") -> str:
    values = LOC.get(key, [])
    return str(values[1]) if len(values) > 1 else fallback


def item_name(item_id: str | None) -> str:
    if not item_id:
        return tr("ui.inventory.empty", "미장착")
    return phase.name_for(LOC, item_id, CATALOG)


def art(svg: Path, name: str, x: float, y: float, w: float, h: float,
        preserve: str = "xMidYMid meet") -> str:
    import html
    return phase.image(svg, ART / name, x, y, w, h, preserve=preserve)


def equipment_title(out: list[str]) -> None:
    phase.wood_panel(out, 383, 20, 208, 176, "equipment")
    phase.plaque(out, 451, 10, 78, "장비 · 4", 10)


def actual_world_context(svg: Path, out: list[str]) -> None:
    """Show the current top-down actor at world scale, without inventing a body portrait."""
    # Crop a 270x220 world region around the existing actor, excluding the health bar.
    # This retains visible surroundings so the sprite reads at its actual in-game scale.
    x, y, w, h = 434.0, 68.0, 110.0, 90.0
    source_x, source_y, source_w, source_h = 820.0, 465.0, 270.0, 220.0
    scale_x, scale_y = w/source_w, h/source_h
    clip_id = "actualLowfireActorCapture"
    out.extend([
        f'<defs><clipPath id="{clip_id}"><rect x="{x}" y="{y}" width="{w}" height="{h}" rx="3"/></clipPath></defs>',
        f'<image href="{phase.relative(svg, phase.BACKGROUND)}" '
        f'x="{x-source_x*scale_x:g}" y="{y-source_y*scale_y:g}" '
        f'width="{1920*scale_x:g}" height="{1080*scale_y:g}" '
        f'preserveAspectRatio="none" clip-path="url(#{clip_id})"/>',
        f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="3" fill="none" '
        'stroke="#2a1b11" stroke-width="1.1"/>',
    ])

    # Fine lines connect existing equipment sockets to the corresponding actor region.
    connectors = ((425, 66, 469, 84), (547, 66, 514, 91),
                  (425, 157, 472, 114), (547, 157, 514, 114))
    for x1, y1, x2, y2 in connectors:
        out.append(f'<path d="M{x1} {y1} Q{(x1+x2)/2:g} {(y1+y2)/2-2:g} {x2} {y2}" '
                   'fill="none" stroke="#c28a50" stroke-width="1" stroke-opacity=".7"/>')


def option_a(svg: Path, out: list[str]) -> None:
    equipment_title(out)
    actual_world_context(svg, out)
    sockets = [
        ("iron_head", "머리", 389, 49, "target"),
        ("iron_body", "몸", 547, 49, "normal"),
        ("lantern", "보조 장비", 389, 140, "normal"),
        (None, "장신구", 547, 140, "normal"),
    ]
    for item_id, label, x, y, state in sockets:
        phase.gear_socket(out, svg, CATALOG, item_id, label, x, y, 36, state, "top")


def rack_row(svg: Path, out: list[str], category: str, item_id: str | None,
             state: str, y: int, selected_target: bool = False) -> None:
    x = 406
    if selected_target:
        out.append(f'<rect x="{x}" y="{y}" width="176" height="31" rx="4" '
                   f'fill="#4f5735" stroke="{GREEN}" stroke-width="1.25" stroke-dasharray="4 2"/>')
    else:
        out.append(f'<path d="M{x} {y+31} H582" stroke="#c79a69" stroke-width=".65" opacity=".6"/>')
    out.extend([
        phase.txt(408, y+11, category, 8.2, MUTED, 750),
        art(svg, CATALOG[item_id]["icon"].name, 450, y+4, 23, 23) if item_id else
            f'<circle cx="461.5" cy="{y+15.5}" r="7" fill="none" stroke="#866e55" stroke-width="1"/>',
        phase.txt(478, y+20, item_name(item_id), 9.4, PAPER if item_id else MUTED, 700 if item_id else 500),
    ])
    pill_fill = "#455736" if item_id else "#3b2b20"
    pill_edge = GREEN if selected_target else "#8d755a"
    out.extend([
        f'<rect x="534" y="{y+5.5}" width="44" height="20" rx="8" fill="{pill_fill}" '
        f'stroke="{pill_edge}" stroke-width=".65"/>',
        phase.txt(556, y+19.2, state, 8.6, GREEN if selected_target else PAPER, 700, "middle"),
    ])


def option_b(svg: Path, out: list[str]) -> None:
    equipment_title(out)
    # One connected equipment rail replaces the character. Continuous spine and row dividers
    # form a loadout list, rather than four independent square cards.
    out.append('<path d="M398 43 V174" stroke="#c28a50" stroke-width="1.35"/>')
    rows = [
        ("머리", "iron_head", "교체", 39, True),
        ("몸", "iron_body", "착용", 73, False),
        ("보조 장비", "lantern", "착용", 107, False),
        ("장신구", None, "비어 있음", 141, False),
    ]
    for category, item_id, state, y, target in rows:
        dot_fill = "#415232" if item_id else "#3c291c"
        dot_edge = GREEN if target else "#b17b47" if item_id else "#80654a"
        out.append(f'<circle cx="398" cy="{y+15}" r="4.2" fill="{dot_fill}" stroke="{dot_edge}" stroke-width="1.2"/>')
        rack_row(svg, out, category, item_id, state, y, target)
    out.extend([
        phase.txt(397, 187, "방어력 6", 8.5, PAPER, 750),
        phase.txt(454, 187, "현재 슬롯 선택 → 장착 해제", 8.0, MUTED, 600),
    ])


def candidate_detail(svg: Path, out: list[str]) -> None:
    phase.wood_panel(out, 596, 101, 205, 108, "context")
    out.append(phase.image(svg, Path(CATALOG["steel_head"]["icon"]), 605, 109, 40, 40))
    out.extend([
        phase.txt(653, 121, item_name("steel_head"), 11.1, phase.PAPER, 800),
        phase.txt(653, 138, "방어력 2 → 3", 9.4, phase.MUTED, 750),
        phase.txt(653, 151, "현재 철 투구 · 슬롯 선택으로 해제", 8.5, phase.MUTED, 650),
    ])
    phase.action_button(out, 604, 160, 188, 44, "장착", "primary", 10.5)


def make_screen(filename: str, use_character: bool) -> None:
    svg = SCREENS / filename
    out = phase.frame(svg, "equipment")
    phase.close_button(out)
    phase.craft_panel(out, svg, CATALOG, RECIPES, LOC, COUNTS)
    option_a(svg, out) if use_character else option_b(svg, out)
    phase.draw_inventory(out, svg, CATALOG, selected_item="steel_head",
                         equipment_candidate="steel_head")
    # The selected bag item and compatible equipment target share the existing Phase 3D path.
    candidate_detail(svg, out)
    out.append("</svg>")
    svg.parent.mkdir(parents=True, exist_ok=True)
    svg.write_text("\n".join(out) + "\n", encoding="utf-8")


def main() -> None:
    if len(phase.HOTBAR) != 9 or len(phase.STORAGE) != 27 or len(phase.EQUIPPED) != 4:
        raise RuntimeError("Phase 3D fixture no longer matches the current 9+27+4 inventory.")
    required = {"player_body.png", "player_eye.png", "player_hand.png", "iron_head_worn.png",
                "iron_body_worn.png", "iron_hand_worn.png"}
    missing = sorted(name for name in required if not (ART / name).is_file())
    if missing:
        raise FileNotFoundError(f"Current player sprite layers missing: {missing}")
    if "steel_head" not in CATALOG:
        raise RuntimeError("The existing compatible equipment candidate steel_head is missing.")
    SCREENS.mkdir(parents=True, exist_ok=True)
    PREVIEWS.mkdir(parents=True, exist_ok=True)
    make_screen("A-topdown-character.svg", True)
    make_screen("B-equipment-rail.svg", False)
    print("Generated A from the existing gameplay capture and B from a no-character equipment rail.")
    print(f"Verified slots: Hotbar={len(phase.HOTBAR)}, Storage={len(phase.STORAGE)}, Equipment={len(phase.EQUIPPED)}")


if __name__ == "__main__":
    main()
