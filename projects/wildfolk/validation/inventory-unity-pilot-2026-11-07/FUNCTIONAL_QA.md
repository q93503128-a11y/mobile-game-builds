# Functional QA

## Targeted Unity PlayMode run

Unity `6000.3.19f1`; PlayMode; isolated test profile. Final result: **7 passed, 0 failed, 0 skipped**. Test window: `2026-11-07 08:33:43–08:34:32 UTC`.

| Test | Result | Coverage |
|---|---|---|
| `InventoryButtonsOpenCloseAndModalBlocksGameplayInput` | PASS | inventory open/close and gameplay input while modal is open |
| `HotbarInventoryAndToolsShareRealSlotsAndApplyResourceEfficiencies` | PASS | hotbar/inventory use the same inventory model and existing tool efficiency behavior |
| `ActualPointerArmorDragSwapUnequipAndUse` | PASS | pointer-driven armor equip, replacement, unequip, and item use flow |
| `GrowthSkillsEquipmentAndStaminaSurviveValidatedSaveRoundTrip` | PASS | validated serialized snapshot round trip for progression, equipment, health, and stamina |
| `V1MigrationPreservesWorldInventoryAndOldXPWithoutForcingNewGame` | PASS | legacy save migration preserves inventory and progression values |
| `LongPressSplitHoldsChosenAmountUntilExplicitPlacementOrCancel` | PASS | long-press split, amount selection, held stack, explicit placement/cancel semantics |
| `SplitHalfViaTouchButtonAndExplicitDropZonePreserveItems` | PASS | split-half action, destination slot, and world drop zone preserve item totals |

Result XML: `test-results/inventory-playmode-final.xml`. The raw Unity log remains in the local QA workspace and is excluded by the repository's `*.log` ignore rule.

## Scene serialization checks

After the canonical scene builder ran successfully, static inspection found 36 `InventoryPanel.Slots` references, four equipment button references, and four named equipment rows in each of `Wildfolk.unity` and `Multiplayer.unity`. This is serialized scene evidence, not a rendered visual test.

## Not covered by the passing run

- Real Android touch targets, multi-touch, device safe areas, or dp sizing.
- Invalid drop destinations and every combination of partial-stack drag/drop.
- A live file-system save read/write using a separate profile; the passing save tests use the existing validated snapshot/migration paths.
- Full PlayMode suite; only the seven inventory-relevant existing tests above ran.
- On-screen confirmation that the four equipment controls persist after every separate content-generation path.
- Visual text fit, clipping, contrast, and edge spacing at any requested resolution/language.

No user save was loaded or modified. No data/schema migration was added.
