# Implementation

## Status

| Area | Status | Evidence |
|---|---|---|
| Brown 2D inventory presentation | IMPLEMENTED | `InventoryUiBuilder` and both gameplay scenes |
| Hotbar and storage | IMPLEMENTED | nine hotbar slots and 27 storage slots serialized in each scene |
| Four equipment rows | IMPLEMENTED | Head, Body, Utility, Accessory; each scene has four button references and four named rows |
| Item details and equipment state | IMPLEMENTED | runtime `InventoryEquipment` refresh and KO/EN localization keys |
| Existing gameplay contracts | PRESERVED | no inventory data model, item catalog, Save14/Profile2/Protocol23, economy, or authority edits |
| Actual on-screen appearance | UNVERIFIED | no Unity Game View screenshot was produced |
| Android touch behavior | UNVERIFIED | no device/touch run was performed |

## Layout

The canonical modal uses a `2050 × 840` reference surface. The left area contains the 27-slot, 9-column storage grid and a visually separate 9-slot hotbar. The right rail contains four equipment rows and a selection-dependent detail/action area. The previous inventory character preview is removed from the inventory surface; the unrelated Character Journal preview remains untouched.

The stored slot cell is `140` reference units. `ModalLayout` reserves `24` reference units per horizontal side. For an assumed safe-area rectangle of 844×390, the scale formula yields approximately `0.388`, or a 54px slot. This is a calculation only; device safe areas and rendered pixel sizes may differ. The 56px prior reference is approached, but actual touch usability is not established.

## Art

Unity UI sprites use sliced imports:

- `inventory_panel.png` — selection/detail panel
- `inventory_slot.png` — item slots
- `inventory_selected_slot.png` — selected slot state
- `inventory_rail.png` — storage, hotbar, and equipment regions
- `inventory_button.png` — actions and close control

Editable originals are in `art-source/`. These are restrained flat 2D surfaces with warm brown, charcoal, cream, and copper/orange accents. The icons and item counts continue to use live catalog sprites and inventory data.

## Canonical integration

`ContentExpansionAssets.ConfigureUtilityUi()` and `BossContentAssets.EquipmentUi()` now call `InventoryUiBuilder.EnsureEquipmentControls()` and `Arrange()`. This keeps Utility and Accessory controls attached when those existing content-authoring paths regenerate their inventory UI. The inventory does not add another runtime controller or replace the existing drag/drop, stack split, crafting, and save systems.

## Scene and contract scope

Changed scenes are `Wildfolk.unity` and `Multiplayer.unity`. Main Menu, HUD data flow, item data, recipes, progression, and network authority were not changed. Equipment operations still call the existing `PlayerEquipment` APIs; no optimistic equipment state is added.
