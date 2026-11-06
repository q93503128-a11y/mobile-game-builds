# Phase 3A — Visual Review and Scope Record

## Current Lowfire evidence

The design was based on the current `main` source, not on the old UI report alone:

- [InventoryUiBuilder.cs](../../client/Assets/Wildfolk/Editor/InventoryUiBuilder.cs) creates nine hotbar cells, a fixed nine-column grid and the split/drop controls.
- [InventoryPanel.cs](../../client/Assets/Wildfolk/Runtime/UI/InventoryPanel.cs), [InventorySlotView.cs](../../client/Assets/Wildfolk/Runtime/UI/InventorySlotView.cs), [InventoryEquipment.cs](../../client/Assets/Wildfolk/Runtime/UI/InventoryEquipment.cs), [InventoryDragController.cs](../../client/Assets/Wildfolk/Runtime/UI/InventoryDragController.cs), [InventoryDropZone.cs](../../client/Assets/Wildfolk/Runtime/UI/InventoryDropZone.cs), and [StackSplitPanel.cs](../../client/Assets/Wildfolk/Runtime/UI/StackSplitPanel.cs) provide the existing view and supported actions.
- The current `Wildfolk.unity` scene contains **9 hotbar + 27 storage + 4 equipment slots** (Head, Body, Utility, Accessory), and the current scene's `InventoryEquipment` references all four buttons and icons. `PlayerEquipment`, `InventoryEquipment`, and `EquipmentSlotDrag` support those equipment categories, selection, drag move, partial-stack drag, equipment drop, world drop, stack split, and equip/unequip. One authoring caveat matters for later Unity implementation: base `InventoryUiBuilder.BuildEquipment()` creates only Head and Body; `ContentExpansionAssets.ConfigureUtilityUi()` adds Utility, and `BossContentAssets.EquipmentUi()` adds Accessory to the saved gameplay scenes. Rebuilding the scene from only the base builder would therefore omit two slots unless those authoring steps (or an equivalent canonical builder change) are included. This pass did not change that pipeline. A held stack opens the existing actions panel only when count is greater than one; the hold threshold in current UI code is 0.45 seconds.
- The current slot view renders stack count, or water charges / `WaterCapacity` for water containers. `ItemDefinition` has no durability property. No durability, sorting, filter, quick-stack, sell, or new slot was added to the mockup.
- The selected Iron Sword uses the current source-derived stats displayed by the project: damage 36, 2.63 attacks/s, range 1.6 m. The full-storage fixture uses actual item definitions and icons; counts are shown at each item's current `MaxStack`, and the waterskin sample shows 2/3 charges. This is a static representative fixture, not a real player save.
- No runtime or save code was changed, so the inventory-open world-time policy remains whatever the current runtime implements. This visual pass did not re-test it.

## Actual Unity frame and fair comparison

No live Unity capture was taken. Unity MCP was unavailable in this execution, and the user specifically asked not to bring focus to the Unity window. The accepted fallback is the committed real Unity capture [`gameplay--inventory-en-844.png`](../post-commercial-ui-qa-tools/evidence/gameplay--inventory-en-844.png). The evidence index records it at **2026-11-02 19:47:35 +0900** with image-added commit `0ae9feae389c24abae40bc68da9569dd2eb5a676`; that commit is also the last source change to `InventoryUiBuilder.cs`. This is useful historical evidence, but the evidence index explicitly warns that an image-added commit does not prove the exact runtime build used for capture.

`previews/before-after-844-en.png` uses the exact same 844×390 Unity frame for the left (original) and right (new static overlay). It is not a live build or a pixel-for-pixel implementation comparison. The 11/4 audit sidecars report an 844×390 Wildfolk capture at `2026-11-04T04:55:09Z`; the audit's InventoryUiBuilder SHA-256 (`2fb566bfb0c830c3f264073e20282fd9714d71b65ea4c9d1f1e29351c7ab2c8b`) matches the current file. Those audit screenshots provide additional language/resolution/state evidence, but are still historical and do not prove the running Unity assembly hash. No capture is presented as a live view from this turn.

## Visual findings

### Improvements visible against the 844 baseline

- Hotbar, storage, equipment, selected-item details, and actions now have named regions. The old view used a crowded mobile arrangement; the redesign gives storage a clear 7×4 landscape grid and keeps the nine-cell hotbar separate.
- The selected state uses a strong warm outline while equipment compatibility is shown separately in moss. The same selection persists in the item-detail region rather than relying on a tiny slot highlight alone.
- New shapes are authored once and reused across idle, selected, compatible, incompatible, primary, secondary, pressed, and disabled examples. The 9-slice guides avoid the irregular AI-painted edges of earlier proposals.
- Existing Lowfire item sprites and a real Lowfire gameplay background are used. No foreign character, scenery, item, or decorative illustration is introduced.
- Item action and split states identify the affected item and quantity, provide explicit confirmation/cancellation, and display the current drag-to-ground target. The equipment-head state shows current armor details and comparison.

### Remaining design issues / approval risks

- The art is a clean editable vector prototype, **not finished commercial game art**. The panel and slot surfaces are still intentionally restrained and may look too geometric/box-heavy to the user. The component PNG should be judged before any implementation is approved.
- Some 844×390 secondary labels are small (around 10–12 canvas pixels). The modal has 44-pixel-high controls, but canvas pixels do not equal Android dp. Device safe areas, touch size, reach, drag accuracy, and finger occlusion remain unverified.
- The mobile screen fits every slot without scrolling. This preserves current access paths and avoids inventing a scroll/page feature, but the 52×52 storage cells and 44×44 hotbar cells need physical-device testing before approval.
- Most main-state item slots are intentionally empty, matching a partial-inventory state. The separate full-storage PNG proves counts/charges and all 27 storage positions, but is not a screenshot of an actual profile.
- The full-screen surface reduces world visibility while the inventory is open. Review whether the static UI should retain more world context before implementation.
- Korean and English short/representative labels were rendered; extreme long-name/description wrapping, text scaling/accessibility settings, color-vision cases, and localized layouts outside KO/EN were not exhaustively tested.

## Visual QA performed

The PNG files were opened and inspected directly, including:

- partial inventory in KO/EN at 844×390, 1280×720, and 1920×1080;
- full storage, selected equipment, item actions, stack split modal, drag-move, and partial drag in KO/EN at 844×390;
- component state sheet;
- same-background before/after at 844×390.

The render script generated 19 SVG preview files and a component-kit SVG, with 20 PNG outputs including the comparison and kit. Dimensions were checked from the rendered images. SVG/PNG generation is **not** Unity compilation, runtime, PlayMode, Android, or touch validation. No Unity tests were run because Unity was deliberately not changed.

## Review gate

This is a candidate for user visual review only. It is not approved for Unity implementation. Unity remains unchanged until the user explicitly approves the visual direction. HUD and other screens are outside this pass.
