# Inventory Phase 3C — Reference-led mobile study

This folder contains an editable, static UI study. It is not wired to Unity and does not edit runtime content.

## Rebuild

From the repository root, run:

```powershell
./projects/wildfolk/art-source/inventory-ui-phase3c-2026-11-06/render_previews.ps1
```

The Python source reads current `ItemDefinition` assets, sprites, armor values, Korean localization, and the project Noto Sans KR font. It checks that the fixture has exactly 27 storage items, 9 hotbar items, 4 equipment slots, and that the Head comparison still resolves to the current Iron Head (2 defense) and Steel Head (3 defense).

Editable screen SVGs live in `screens/`. `graphics/lowfire-inventory-ui-kit.svg` shows the original Lowfire field-canvas, carved item well, selected/target/pressed/disabled states, and action surfaces. Rendered 844×390 PNGs and the UI-kit PNG live in `validation/inventory-ui-phase3c-2026-11-06/`.

The sample quantities are a design fixture, not a user save or an actual loaded player profile. Every item ID, item sprite, displayed armor number, gameplay background, player crop, and Korean name is sourced from the current repository. The equipment portrait is clipped from the committed Lowfire Unity gameplay capture `docs/images/lowfire-player-world-armor.png`; it is not a generated character or new world art.

## Structural decision

The Bag view follows Core Keeper's left-to-right functional zoning: a single existing Crafting entry on the left, the four equipment sockets around the actual Lowfire character in the center, and the Hotbar-above-backpack pair on the right. The selected-item ribbon appears only after selection. The Equipment view focuses the same four slots around the character and keeps a compatible candidate selected across the view change. It offers the existing Equip and Unequip paths and reports the current bag-space constraint. The Crafting entry returns to the existing crafting screen; no recipes or functions were invented.

Controls in these images are static. The world-drop tile illustrates the existing drag destination while an item is being dragged; it does not imply a one-tap drop action. No Unity compile, Play Mode, Android touch, or live interaction was tested. Bag and hotbar cells are 47×47 px; header/close/craft actions are 44 px high; equipment sockets are 56×56 px. These mockup measurements are not Android touch certification, and the device-scale mapping to dp remains unverified.

External screenshots are linked and analyzed in `validation/inventory-ui-phase3c-2026-11-06/REVIEW.md`. Third-party screenshots are not copied into Lowfire's art assets.
