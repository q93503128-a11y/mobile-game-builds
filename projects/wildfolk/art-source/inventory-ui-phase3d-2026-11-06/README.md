# Inventory Phase 3D — Reference-first reconstruction

This directory contains a static, editable visual prototype. It is not connected to Unity and does not alter runtime content.

## Rebuild

From the repository root, run:

```powershell
./projects/wildfolk/art-source/inventory-ui-phase3d-2026-11-06/render_previews.ps1
```

The generator reads current item definitions, existing item PNGs, recipes, armor values, Korean localization, Noto Sans KR, and the committed Lowfire gameplay capture. It builds a local visual fixture, never reads a player save, and checks the existing 9 Hotbar, 27 Storage, and 4 Equipment slots. All six recipe cards resolve to current hand-crafting recipes and their ingredient counts are covered by the fixture.

Editable screens are in `screens/`; the original reusable wood surface and slot/action states are in `graphics/`. 844×390 PNG previews and the 900×440 UI kit preview are in `validation/inventory-ui-phase3d-2026-11-06/`.

The reference informed panel order, item wells, paper-doll relationship, visible stack counts, and the hotbar-over-storage composition. No Core Keeper screenshot, texture, icon, label, or game asset is included in the Lowfire art source. The player and world imagery is the existing project capture `projects/wildfolk/docs/images/lowfire-player-world-armor.png`; item icons and names resolve from current Lowfire sources.

The fixture’s compact slot cells and all labels remain a static screen study. Phone touch accuracy, Unity 9-slice behavior, safe-area fit, dragging, split confirmation, equipping, and world dropping have not been device-tested. The design remains unapproved and must not be implemented in Unity before the user approves it.
