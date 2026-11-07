# Visual QA

## Status: UNVERIFIED

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
