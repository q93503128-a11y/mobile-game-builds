# Lowfire Inventory Mobile Layout — Flat 2D Revision

**Status: unapproved visual study. No Unity implementation.**

**Source main snapshots:** `mobile-game-builds` `2191e937d66ed73934a3fec5c95bbf90c2f5377d`; `dev-playbook` `e022e9e4ff3947fcbd7f5b0237c5905d7c0409c7`. Both were fetched again before commit; neither had advanced.

## Scope and inputs

This review compares two Korean static screens at 844×390. Both use the existing Lowfire world capture (`docs/images/lowfire-player-world-armor.png`), current item-definition icons and localized names, and three current hand-crafting recipes. The fixture is inherited from the Phase 3D visual study; it is not a user save. No original art, earlier inventory study, Unity asset, runtime code, game data, save, or network code was changed.

The four equipment sockets, nine Hotbar slots, and 27 logical Storage slots remain visible or reachable. The UI is a quiet flat spruce-green surface with a single restrained outline, warm selection accent, and no wood/leather/metal texture, bevel, repeated frame, or large ornamental panel.

## Visual comparison

| Measure | Phase 3D | A — all 27 visible | B — 18 per page |
|---|---:|---:|---:|
| Storage capacity shown | 27 | 27, 9×3 | 18 at once, 6×3; page 2 contains the remaining 9 actual slots |
| Storage cell | 48×41 px | 56×57 px | 82×57 px |
| Cell area change vs Phase 3D | — | +62.2% | +137.5% |
| Hotbar cell | 48×43 px | 54×44 px | 54×44 px |
| Fixed mobile touch target | — | Slots remain directly visible | Page controls are 44×44; slots are wider |
| Outer content bounds | x=147…589; bottom=385 | x=16…828; bottom=374 | x=16…828; bottom=374 |

Both layouts reclaim the unused right half, increase the bottom safe margin from 5 to 16 px, raise count text to 11.5 px, and make item names and equipment labels larger than the Phase 3D micro-label treatment. Equipment is now a light 2×2 set of 92×69 px cards instead of a visually dominant paper-doll block. The close target and B page controls are 44×44 px. The world remains visible around and faintly through the panels.

### A — all 27 visible

All storage indices remain on one 9×3 surface. The nearly square 56×57 px cells grow 8 px in width and 16 px in height over Phase 3D while every slot remains a direct drag/drop destination. The storage wells still fit the full 27-slot capacity with 16 px screen-edge clearance. This is the lowest-risk arrangement for current drag, unequip, and split interactions.

### B — 18 per page

Page 1 maps logical indices 0–17 to a 6×3 grid; page 2 maps indices 18–26 to nine actual cells. No additional logical slots are proposed. The 82×57 px cells improve horizontal hit area; a 44×44 page control is large enough to tap. In the inherited fixture, the first 17 Storage slots contain all current example stacks, so the second page would show nine empty slots. That is a limitation of this static fixture, not a claim that live saves will have the same distribution.

The page concept is not interaction-approved. Current `InventorySlotDrag` delegates short vertical touch gestures to a `ScrollRect`, but occupied-slot full-stack drag uses `InventoryDragController` pointer drag/drop. A page flip while keeping a full-stack drag alive is not established by the current UI. Page changes must preserve the logical selected index, held amount/source, and split state; Hotbar and Equipment destinations must stay available. The design should not ship until those paths are proven. If this cannot be made predictable without weakening direct drag, prefer A.

## Reference and source evidence

| Evidence | Observed case | Application here |
|---|---|---|
| Current Phase 3D SVG/PNG | 48×41 Storage wells, 9×3 grid, 442 px-wide lower block, 5 px bottom margin | Measured as the baseline; its information hierarchy remains, but the heavy timber treatment and crowded width are removed |
| Current `InventorySlotDrag` and `InventoryDragController` | Existing slot selection, long-press item actions, partial held stack, drag ghost, drop target, equipment drag/drop, and visible-slot transfers | B is treated as a navigation proposal; no preservation claim is made for cross-page drag until verified |
| Current `PlayerInventory` and `InventoryPanel` | Nine-slot Hotbar and selected logical index; inventory panel refreshes selection from its owner | Hotbar stays fixed; page rendering must not renumber or clear the selected slot |
| Current Lowfire world and item PNGs | Muted green world with outlined low-poly sprites; 136 item icons resolve from current definitions | Used as-is. Palette and border weight use the actual world/sprite contrast; no external game assets were copied |

## Layout fit and visual QA

Both final PNGs were rendered from their editable SVGs at exactly 844×390 and inspected at native pixel dimensions, with Phase 3D also included in a three-way comparison. Their bounds are inside the canvas: leftmost 16 px, rightmost 14 px, bottom 16 px. Craft, Equipment, Hotbar, Storage, and close-control regions do not overlap. A’s 9×3 and B’s 6×3 grids fit within Storage; B’s page controls are below, not on top of, the third row. The sample texts, icons, quantities, and Korean labels stay inside their cards. All occupied fixture entries resolve to current item icons; all nine Hotbar slots and four equipment categories render.

These are static mockups. No phone-density readability, safe-area notch, touch sampling, drag, split, equipment transfer, or live second-page test was performed. Tiny item art remains visually small relative to the wider B wells because the project’s source sprites are intentionally kept at their existing proportions. A selected-state example is included on Hotbar slot 1 (default logical index 0 in `PlayerInventory`); no separate pressed/disabled state sheet was requested in this pass.

## Assessment

Both options are clearly better than Phase 3D for screen use: the working region expands across the right side, the edge clearance improves, the flat panels fit Lowfire’s world palette, and stack counts no longer sit on an overly dark, highly ornamented wooden tile. A provides a meaningful but moderate cell-size improvement while keeping all destinations visible. B provides much larger horizontal targets, but introduces a page barrier and has no visual advantage for vertical tap reach; it also needs cross-page carry behavior that the current code does not establish.

**Design recommendation for the next review:** evaluate A first as the safer mobile base. Keep B as an alternative only if testing demonstrates that 82 px-wide slots materially improve item selection and a page transition can retain drag/split state. Neither layout is approved for Unity yet. At this stage the artwork is a coherent flat UI prototype, not production-ready final art; device readability and interaction behavior remain open.

## Files

- `previews/A-all27-844x390.png` — A, all storage cells visible.
- `previews/B-page18-844x390.png` — B, first 18 storage cells with page navigation.
- `previews/A-B-comparison-1688x390.png` — both previews side by side.
- `previews/Phase3D-A-B-comparison-2532x390.png` — existing Phase 3D preview followed by A and B.
- `../art-source/inventory-mobile-layout-flat-2026-11-07/screens/` — editable SVG sources.
- `../art-source/inventory-mobile-layout-flat-2026-11-07/generate_layouts.py` — regenerates the two SVGs from current project assets.
- `../art-source/inventory-mobile-layout-flat-2026-11-07/render_previews.ps1` — regenerates and renders the PNGs.

## Timing record

Exact work stages are recorded from clock checkpoints; the start time of the initial repository/document review was not captured, so no duration is claimed for that interval. Git commit/push is tracked separately in the final handoff.

| Work stage | UTC interval | Elapsed |
|---|---|---:|
| SVG layout generation and first render | 2026-11-07 03:33:24–03:40:06 | 6m 42s |
| Interaction/source review, readability/selection revision, final renders, visual review, and documentation | 2026-11-07 03:40:06–03:48:51 | 8m 45s |
| Total timed visual-production/review work | 2026-11-07 03:33:24–03:48:51 | 15m 27s |
