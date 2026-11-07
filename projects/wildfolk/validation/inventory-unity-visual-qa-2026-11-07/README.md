# Lowfire Phase 4B — Unity Inventory Visual QA

## Current result — resumed on 2026-11-07

**VERIFIED:** The Unity MCP instance was connected to the exact canonical project requested: C:\Users\User\Documents\ChatGPT\문승준 모바일\canonical\projects\wildfolk\client (Unity 6000.3.19f1). The previous stale-project checkpoint below is historical and no longer describes the current session.

Actual Unity Game View screenshots were captured in Play Mode and inspected at original PNG pixels. Korean 844×390 was the primary target. English 844×390, 1280×720, and 1920×1080 were also reviewed. The selected evidence is in [screenshots](screenshots/README.md); statuses and image mapping are in [CAPTURE_INDEX.md](CAPTURE_INDEX.md).

**FIXED and rechecked:**

- Equipment category/item labels overlapped the equipment icon at 1280×720 and 1920×1080. Their layout now starts beside the icon; before/after Unity captures show the correction. The same canonical builder change was applied to both single-player and multiplayer scenes.
- Selecting an empty equipment slot could retain the prior drag-controller item selection, so Split could remain enabled for an unrelated stack. Equipment selection now clears or synchronizes that selection. In the live QA session, selecting an empty Head slot left Split disabled and did not open the split panel; equipment candidate, equip, replacement, and replacement-applied states were also exercised.

No P0 remains in the inspected scope. The small 844×390 labels/counts and dimmed gameplay HUD visible behind the modal are recorded as P2 observations. Android safe area, device dp sizing, and physical touch behavior remain **UNVERIFIED**. This visual QA is not a design approval.

The existing seven passing Phase 4A PlayMode tests were not rerun. The modified C# scripts compiled in the live Editor, direct inventory interactions were checked in the isolated QA session, and the final Unity Console query returned zero error/warning entries. No Save/Profile/Protocol/data/economy/network changes were made. QA used a temporary profile path with autosave disabled; no user save was opened or modified. C:\UnityProjects\Wildfolk was not modified.

## Changes in this resumed pass

- Canonical source: client/Assets/Wildfolk/Editor/InventoryUiBuilder.cs, client/Assets/Wildfolk/Runtime/UI/InventoryEquipment.cs.
- Serialized layout coordinates only: client/Assets/Wildfolk/Scenes/Wildfolk.unity, client/Assets/Wildfolk/Scenes/Multiplayer.unity.
- This report and actual Game View PNG evidence.
- No new item, scene flow, save schema, or inventory data model.

## Earlier checkpoint — historical

The earlier checkpoint reported **CAPTURE BLOCKED** because Unity MCP was attached to C:\UnityProjects\Wildfolk, a stale copy without Phase 4A. That statement was accurate for that checkpoint. After the user reopened the canonical project and Unity MCP attached to it, this resumed pass captured and reviewed the actual Phase 4A Game View. The earlier finding is superseded by the current result above, not erased as history.

See [MCP_DIAGNOSIS.md](MCP_DIAGNOSIS.md) for capture/recovery details and transient failures. See [TIME_LOG.md](TIME_LOG.md) for measured work intervals.
