# Lowfire Inventory UI — Phase 3A Visual Prototype

Status: **design review requested; not approved for Unity implementation.** The screens are static SVG/PNG studies. No Unity source, scene, prefab, game data, save, runtime behavior, or Penpot file was changed.

## Review files

- [`RESEARCH.md`](RESEARCH.md) — concrete external examples and what was / was not carried into Lowfire.
- [`DESIGN_REVIEW.md`](DESIGN_REVIEW.md) — visual comparison, current source behavior, resolution/language coverage, and unverified risks.
- [`ASSET_MANIFEST.md`](ASSET_MANIFEST.md) — reused project assets, new editable art, licensing, and future Unity handoff notes.
- [`TIME_LOG.md`](TIME_LOG.md) — elapsed time by work stage.
- [`previews/`](previews/) — 20 PNG outputs (19 screen/comparison outputs plus component sheet), with editable screen SVGs in `previews/svg/`.

## Preview index

### Main inventory, partial storage

| Canvas | Korean | English |
|---|---|---|
| 844×390 | `previews/inventory-mobile-partial-ko-844x390.png` | `previews/inventory-mobile-partial-en-844x390.png` |
| 1280×720 | `previews/inventory-desktop-partial-ko-1280x720.png` | `previews/inventory-desktop-partial-en-1280x720.png` |
| 1920×1080 | `previews/inventory-desktop-partial-ko-1920x1080.png` | `previews/inventory-desktop-partial-en-1920x1080.png` |

### Mobile states, 844×390

For both Korean and English: `equipment-head`, `storage-full`, `item-actions`, `split-modal`, `drag-move`, and `drag-split`. Example: `previews/inventory-mobile-split-modal-ko-844x390.png`.

### Before/after and components

- `previews/before-after-844-en.png` — same historic Unity frame on both sides; the right side is a static SVG overlay, not a live Unity build.
- `previews/inventory-ui-kit.png` — authored reusable visual states.

Regenerate all SVG and PNG previews using the command in the art-source README. Review the source/build caveat and design risks before evaluating the comparison.
