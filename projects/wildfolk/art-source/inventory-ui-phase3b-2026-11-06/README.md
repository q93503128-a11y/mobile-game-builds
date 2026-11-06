# Inventory UI Phase 3B — Editable source

This folder contains the generator for the six Korean structural studies. It creates SVG source screens from current Lowfire `ItemDefinition` assets, localized item names, the existing `NotoSansKR.ttf`, and the committed live Unity world/HUD capture. It does not create or modify Unity assets.

## Rebuild

From the repository root, run:

```powershell
./projects/wildfolk/art-source/inventory-ui-phase3b-2026-11-06/render_previews.ps1
```

The script checks each referenced item ID against the current item asset catalog, writes editable SVG to `projects/wildfolk/validation/inventory-ui-phase3b-2026-11-06/previews/svg/`, then renders PNGs with headless Microsoft Edge. Each of the six screens is 844×390; the comparison sheet is a 908×737 contact sheet only.

The layout fixture is illustrative, not a captured player save. All item IDs, item sprites, the Lowfire world/HUD capture, labels, and font are existing project assets. No external game image or asset is embedded in the mockups. External screenshots are cited only as research evidence in `REVIEW.md`.

## Generated screens

| File | Structure | State |
|---|---|---|
| `a-basic.svg` | A — bag-centered | Bag view, no item selected |
| `a-selected.svg` | A — bag-centered | Wood stack selected; split, discard, drag affordance |
| `a-equipment.svg` | A — bag-centered | Iron helmet moved toward Head equipment slot |
| `b-basic.svg` | B — tabbed | Bag tab, no item selected |
| `b-selected.svg` | B — tabbed | Wood stack selected; split, discard, drag affordance |
| `b-equipment.svg` | B — tabbed | Equipment tab; Iron helmet selected for Head slot |

No external asset license is introduced. Existing project asset attributions and licenses remain authoritative.
