# Test and Capture Results

## Verified

| Check | Result | Evidence |
|---|---|---|
| Unity project routing | `VERIFIED` | Unity MCP project info exactly equals `C:/Users/User/Documents/ChatGPT/문승준 모바일/canonical/projects/wildfolk/client`; Unity 6000.3.19f1. |
| Canvas/layout basis | `VERIFIED` | Both scenes use Scale With Screen Size, reference 1920×1080, Match Width Or Height 0.5. Builder panel reference is 2050×840 and is fitted to available view; no conversion of Unity units into physical dp/pt is assumed. |
| Compilation | `VERIFIED` | Final `InventoryEquipment.cs` compile completed; Unity Console had 0 errors on the final post-capture read. |
| EditMode layout | `VERIFIED` 2/2 | `InventoryEquipmentLayoutTests.EquipmentTargetsAndLabelsFitSharedInventoryLayout` ran for `Wildfolk.unity` and `Multiplayer.unity`; checks 36-slot binding, 9-column banks, 2×2 non-overlapping gear cells and hit areas, blank unselected-state captions, text bounds, modal mattes and action spacing. Latest job `12fb241b9ec544aa8c79c6f4ef8000ef`. |
| PlayMode regressions | `VERIFIED` 4/4 | Existing pointer armor drag/swap/unequip/use; inventory open/close and modal input block; long-press split; touch split/drop-zone tests. Latest job `84dcc61e9ff843b9976864935cc991b2`, test duration 15.993s. No broad unrelated suite rerun. |
| Live text and equipment state | `VERIFIED` 0 failures | Final QA emitted 2,935 assertions in `captures/final-verified6/qa-live.tsv`; text fits its bounds and the unselected, equipped-selected, replacement-candidate and applied-replacement states are asserted. |
| Actual Unity screenshots | `VERIFIED` 12/12 | Direct Play Mode `ScreenCapture.CaptureScreenshot` at the exact requested dimensions; see [capture list](CAPTURE_INDEX.md). |
| Save isolation | `VERIFIED` | QA profile used `C:\Users\User\AppData\Local\Temp\Lowfire-Phase4C-QA-f185e197f42245d7801a6f715172414d`; AutoSave disabled; `World save was not written`. PlayMode tests were routed to a distinct unique temp directory. |
| User data/schema | `VERIFIED` unchanged by diff | No changes to Save/Profile/Protocol data or migration source. User save was not used or written. |
| Focus/GUI restrictions | `VERIFIED` | Unity state reports `is_focused=false`; screenshots and tests used Unity MCP only. No Windows mouse/keyboard automation or Focus/Show call. |

## Diagnostic failures retained in the work record

- Early capture attempts raced the Game View resolution switch; final capture helper now waits 0.4s and asserts `Screen.width/height` before capture. Final 12 files match their exact dimensions.
- Earlier layout-boundary scans found insufficient height for quick-slot number, stack count, selected equipment category and title text. The rectangles were enlarged without reducing the font size; final scan reports 0 failures.
- The first refreshed-scene split-modal capture showed inventory text through a translucent backing. The canonical builder was corrected for existing scenes, and the matte was made opaque. The before/after PNGs are preserved in [CAPTURE_INDEX.md](CAPTURE_INDEX.md); final EditMode and PlayMode tests and capture were rerun.
- A state assertion in the intermediate `final-verified5` run failed: selecting the equipped slot while a matching replacement item was selected reported `교체 가능` in the wrong state. `InventoryEquipment.SelectSlot` now preserves a compatible replacement as the selected candidate, while selecting an equipped item with no matching candidate retains the equipped detail state. The final `final-verified6` run asserts both paths and passes. The failed iteration remains local, excluded from Git, and is recorded here rather than presented as final evidence.
- One intermediate direct-scene QA run produced NullReference exceptions in unrelated gameplay initialization/teardown components (`PlayerLocomotion`, `WildlifeActionBrain`, `AudioLandscape`, `PlayerSurvival`, `NavigationHud`, `SurvivalHud`, `PlayerStaminaHud`, `LocalFirelight`, `WorldLighting`, plus `InventoryEquipment` and `CraftingPanel` on teardown). The refreshed final Unity session and latest PlayMode test/capture completed with zero Console errors on the final read. Those intermediate non-UI transitions were not patched in this UI pass; see [remaining limits](KNOWN_ISSUES.md).

## Not run

- Android/iOS physical device touch, density/dp/pt, rotation, safe-area/cutout, or one-handed reach checks.
- Save migration tests (no save schema changed), network authority tests (no network code changed), or full project suite (outside this focused UI change).
