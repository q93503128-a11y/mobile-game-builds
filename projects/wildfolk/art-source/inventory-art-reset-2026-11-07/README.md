# Inventory Art Production Reset — Sample Pack

**Status:** visual material study only; not approved for production or Unity.
**Screen scope:** existing Phase 3D 844×390 inventory candidate. No gameplay, inventory, save, data, or Unity source was changed.

## What is here

- `source-generated/slot-normal.png` — normal slot material
- `source-generated/slot-selected.png` — selected slot material
- `source-generated/equipment-panel.png` — empty equipment panel surface
- `source-generated/button-primary.png` — empty primary button surface
- `exports/*-4x.png` — alpha-preserving proof exports sized to four times each existing screen component
- `build_preview.ps1` — crops alpha bounds, builds proof exports and composites them without editing Phase 3D source files
- `PROMPTS.md` — final generation/edit prompts and input provenance

The proof exports are exactly 192×164 (slots), 832×704 (equipment panel), and 752×176 (button). They were resized to the target component ratios for comparison, not authored or validated as import-ready 9-slice sprites.

Run from the repository root in Windows PowerShell with `System.Drawing` available:

```powershell
& '.\projects\wildfolk\art-source\inventory-art-reset-2026-11-07\build_preview.ps1'
```

The script uses the committed Phase 3D preview as a read-only base and writes only under this reset's `exports/` and validation `previews/`. The two empty slots are placed over the existing empty last row. The panel and button are placed in available background space at their current 208×176 and 188×44 sizes. That composite is a material-fit study, not a proposed screen or an enabled control.

## Pipeline recommendation

Use **Krita layered raster painting** for the production source: one `.kra` per component family, editable layers for base color, broad material facets, shallow edge light, contact shadow, and selected-state overlay. Export transparent PNGs at 4× target resolution, define stable corner/slice guides, then verify each slice in Unity before applying it to the existing UI. Keep all labels, counts, and actions as Unity live text.

Krita is GPL-3.0, and its [official license page](https://krita.org/en/about/license/) says it can be used for commercial work and that artwork created with it belongs to the artist. This makes it a practical commercial art tool, but it does not replace the need to create original assets and document their provenance.

The Krita bridge was unavailable during this pass (`BRIDGE_UNAVAILABLE`), and Blender's live bridge also did not answer. The four included samples were therefore generated and iterated as raster concept art with the built-in image generator; they are **not layered Krita masters**. The exact prompts and PNG outputs make the study inspectable, but image generation is not deterministic and does not establish an editable, hand-authored production source. The current sample set is not approved to ship.

## Provenance and use

- Generation used no Core Keeper, Necesse, Terraria, or other third-party image, texture, icon, or screenshot as an image input.
- The committed Lowfire gameplay capture was used only as a visual style reference for later iterations; no game object was copied.
- [OpenAI's individual Terms of Use](https://openai.com/policies/row-terms-of-use/) state that, as between the user and OpenAI and to the extent permitted by law, the user owns output. The same terms say outputs may not be unique and place responsibility for use on the user. This is not a legal non-infringement guarantee; retain the prompt/output record and review before commercial release.
- Third-party reference screenshots were inspected separately and are not included in this folder or in the generated art.

## Current limits

The four assets do not yet prove 9-slice behavior. Their painted bevel and shadow approach the corners, the selected state is still visually close to normal at native size, and no mobile device or Unity render was used. Treat the 4× exports as review-only samples; do not import them into the game until a human-approved layered source and slice test exist.
