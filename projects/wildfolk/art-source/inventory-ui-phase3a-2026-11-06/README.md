# Inventory UI Phase 3A — Editable Art Source

This folder contains a static inventory redesign prototype and its editable vector source. It does not change Unity scenes, assets, runtime code, save data, or Penpot.

## Files

- `inventory-ui-kit.svg` — authored component sheet with slot states (idle, selected, compatible, incompatible) and action-button states (primary, secondary, pressed, disabled).
- `generate_previews.py` — generates the component sheet and 19 screen/comparison SVGs using current Lowfire item sprites and localization data.
- `render_previews.ps1` — renders the SVG previews to PNG with headless Edge. Run from the repository root:

  ```powershell
  .\projects\wildfolk\art-source\inventory-ui-phase3a-2026-11-06\render_previews.ps1
  ```

Outputs are written only under [`validation/inventory-ui-phase3a-2026-11-06/previews`](../../validation/inventory-ui-phase3a-2026-11-06/previews/).

## Authoring and reuse

The SVG defines reusable symbols and screen text remains live text. Existing Lowfire icon PNGs, localization JSON, and the bundled Noto Sans KR font are referenced in place; none are copied into this folder. The new slot and button surfaces are authored vector geometry. No external game art or generated scenery is included.

The kit uses 64×64 slot symbols with 8-unit corner cuts and 240×72 button symbols with 8-unit corner cuts. These are prototype slicing guides, not approved Unity import settings. See [`ASSET_MANIFEST.md`](../../validation/inventory-ui-phase3a-2026-11-06/ASSET_MANIFEST.md) before any later implementation.
