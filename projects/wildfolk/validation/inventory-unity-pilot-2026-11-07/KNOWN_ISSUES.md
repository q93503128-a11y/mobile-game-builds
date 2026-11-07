# Known Issues and Deferred QA

## 2026-11-07 Phase 4B disposition

- The former **P1 — Actual visual review is outstanding** was resolved for the listed desktop Unity Game View states. A label/icon overlap was found at 1280×720 and 1920×1080, fixed in the canonical inventory builder and both gameplay scenes, and re-captured. Empty-equipment stale Split activation was also corrected and checked in live QA. Evidence: [Phase 4B result](../inventory-unity-visual-qa-2026-11-07/README.md) and [capture index](../inventory-unity-visual-qa-2026-11-07/CAPTURE_INDEX.md).
- Android device touch targets and Safe Area remain **UNVERIFIED**. The original P1 platform QA concern remains open until a physical Android check.
- **P2 — Visual design approval remains pending.** Desktop QA did not constitute user approval of the visual design.
- **P2 — 844×390 text/count size.** Equipment status and quantity text are visible but small at native landscape dimensions.
- **P2 — Modal background treatment.** Dimmed gameplay HUD remains visible around the modal; no control overlap was observed, but the presentation may need polish after user review.

## Historical Phase 4A issues

The following issue descriptions document the state before actual Game View captures became available. Use the disposition above and the Phase 4B evidence for current desktop status.
## P1 — Actual visual review is outstanding

- **Where:** inventory modal at 844×390, 1280×720, and 1920×1080.
- **What:** no rendered Unity Game View capture was obtained, so clipping, overlap, text size, equipment rail legibility, hotbar/storage spacing, and visual quality are not confirmed.
- **Reproducible:** the capture limitation reproduces in the batch QA environment: the Game View remains 640×480 and `CaptureScreenshotAsTexture` errors before end of frame. The hidden non-batch invocation timed out without a result.
- **Category:** Visual QA / UX.
- **Direction:** review a real Game View from an already-open Unity Editor at the requested sizes and languages. Do not infer approval from scene YAML or the PlayMode test results.

## P1 — Touch usability not tested on Android

- **Where:** slot and action buttons, especially 844×390 landscape with device safe areas.
- **What:** the 54px slot estimate is in rendered pixels for an assumed viewport; it is not a dp measurement or a physical touch result.
- **Reproducible:** not tested on hardware.
- **Category:** UX / platform QA.
- **Direction:** verify on Android hardware before release acceptance; adjust safe-area and touch sizing based on that evidence.

## P2 — Visual quality approval is pending

- **Where:** all new brown 2D frames and equipment rows.
- **What:** the underlying 9-slice sprites are implemented, but their rendered appearance and fit against the Lowfire world art have not been reviewed in Game View. Treat them as pilot art pending approval.
- **Reproducible:** not yet reviewed.
- **Category:** Visual design.
- **Direction:** inspect the requested actual captures first; refine only the inventory pilot before expanding to another screen.

## Deferred functional checks

- Full PlayMode suite and all invalid-drop/partial-stack permutations.
- Disk-based save repository read/write in a dedicated temporary profile.
- Device safe area, touch, and multi-touch behavior.
- UI regeneration through each content-authoring path in a live editor session.
