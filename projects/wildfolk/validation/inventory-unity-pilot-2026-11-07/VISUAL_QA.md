# Visual QA

## 2026-11-07 Phase 4B review addendum

The earlier Phase 4A statement below records the status at implementation time. It is superseded for desktop Unity Game View review by [Phase 4B visual QA](../inventory-unity-visual-qa-2026-11-07/README.md). Live captures now cover Korean 844×390 inventory, selected detail, empty/equipped/candidate/replacement equipment states, split modal, all 27 storage slots, plus English long-name checks at 844×390 and English 1280×720 / 1920×1080 layout comparisons.

The first visual review found equipment category/item text overlapping its icon at 1280×720 and 1920×1080. The canonical builder and both gameplay scenes were corrected, then captured again. An empty equipment slot could also keep a stale selected stack's Split action enabled; runtime selection synchronization was corrected and checked live.

The 844×390 interface fits without observed clipping, but quantity and equipment status text remain small. Android safe area, dp sizing, and physical touch targets are still UNVERIFIED. This is a visual review, not user design approval.

## Original Phase 4A checkpoint — historical

No Unity Game View image is included because no rendered capture was successfully produced. This report does not claim a visual inspection or visual approval.

The batch PlayMode runner reported a fixed `640×480` screen and `CaptureScreenshotAsTexture()` failed before the end-of-frame render state. A hidden non-batch editor run did not produce a test result within its 240-second limit; that isolated process was stopped. No Unity window was opened or focused, and no Windows mouse/keyboard automation was used.

Capture attempt XML results are retained under `test-results/`. Raw Unity `.log` files remain in the local QA workspace and are excluded by the repository's `*.log` ignore rule. The temporary capture fixture was removed after it proved unreliable in this environment; no synthetic or resized image is substituted for an actual Game View capture.

## Required review still pending

| Target | Status |
|---|---|
| 844×390 Korean: basic, item selected, equipment selected, replacement, split, full inventory | UNVERIFIED |
| 1280×720 English | UNVERIFIED |
| 1920×1080 English | UNVERIFIED |
| Korean/English text fit | UNVERIFIED |
| World and UI visual harmony | UNVERIFIED |
| Nine-slice appearance and clipping | UNVERIFIED |
| Android touch target size and safe area | UNVERIFIED |

The current layout calculation predicts an approximately 54px slot at a safe-area rectangle of 844×390. The actual safe-area size, Game View scaling, text legibility, and hardware touch dimensions still require direct review. The 844×390 screenshot set is a blocking visual-review item before treating Phase 4A as approved.
