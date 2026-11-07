# Inventory Mobile Layout — Flat 2D Revision

This is a new, unapproved, static 844×390 Korean layout study. It leaves the earlier Phase 3D and Inventory Art Reset sources intact.

## Build the previews

From the repository root, run:

```powershell
./projects/wildfolk/art-source/inventory-mobile-layout-flat-2026-11-07/render_previews.ps1
```

The generator reads current item definitions, their existing Lowfire PNG icons, localization, hand-crafting recipes, and the existing Lowfire world capture. The fixture comes from the Phase 3D study; it is not a player save. The screens keep the 9 Hotbar and 4 Equipment positions and compare 27 simultaneously visible Storage slots with 18 slots per page.

Editable SVG sources are in `screens/`. PNG outputs and the visual review are in `projects/wildfolk/validation/inventory-mobile-layout-flat-2026-11-07/`. The render script also creates a side-by-side comparison with the unmodified Phase 3D basic-state PNG. No Unity source, scene, prefab, data, save, network code, source sprite, or Phase 3D file is changed.

Page mode is a layout proposal only. In particular, uninterrupted full-stack dragging between items hidden on different pages requires an additional interaction treatment and must be verified against the existing pointer/drag controller before implementation.
