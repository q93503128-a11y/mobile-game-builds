# Phase 4B Capture Index

All screenshots below are unresized PNGs captured from the live Unity Game View in Play Mode using UnityEngine.ScreenCapture.CaptureScreenshot; each file pixel dimensions were read from the PNG. They are not static mockups or composites. Korean 844×390 is primary. VERIFIED applies only to the observed Unity frame and listed state; it does not certify device touch behavior.

| Target | Status | Evidence |
|---|---|---|
| Unity MCP project identity | VERIFIED | Instance client@114cecc6a8d1b6b9; project/info returned exact canonical .../canonical/projects/wildfolk/client; Unity 6000.3.19f1 |
| 844×390 KO: 27/27 storage, hotbar and selected Wood detail | VERIFIED | [Screenshot](screenshots/ko-844x390-storage-full-selected-wood.png) |
| 844×390 KO: empty Head equipment selection | VERIFIED | [Screenshot](screenshots/ko-844x390-empty-head-selected.png); Split was disabled and did not open |
| 844×390 KO: equipment candidate | VERIFIED | [Screenshot](screenshots/ko-844x390-equip-candidate.png) |
| 844×390 KO: equipped Head item | VERIFIED | [Screenshot](screenshots/ko-844x390-equipped-head.png) |
| 844×390 KO: replacement candidate and replacement applied | VERIFIED | [Candidate](screenshots/ko-844x390-replacement-candidate.png), [applied](screenshots/ko-844x390-head-replaced.png) |
| 844×390 KO: stack split modal, Wood 15/30 | VERIFIED | [Screenshot](screenshots/ko-844x390-split-modal.png) |
| 844×390 EN: long equipment and item names | VERIFIED | [Equipment](screenshots/en-844x390-long-equipment-name.png), [item detail](screenshots/en-844x390-long-selected-item.png) |
| 1280×720 EN equipment label overlap | FIXED | [Before](screenshots/en-1280x720-before-layout-fix.png) / [after](screenshots/en-1280x720-after-layout-fix.png) |
| 1920×1080 EN equipment label overlap | FIXED | [Before](screenshots/en-1920x1080-before-layout-fix.png) / [after](screenshots/en-1920x1080-after-layout-fix.png) |
| Storage/hotbar clipping and nine-slice appearance | VERIFIED | Examined at 844×390, 1280×720, 1920×1080; no slot clipping or obvious corner distortion seen |
| Tiny 844×390 counts/equipment text | VERIFIED — P2 | Visible, but small at native phone-landscape dimensions; see README and KNOWN_ISSUES |
| Background HUD/scrim relationship | VERIFIED — P2 observation | Dimmed HUD remains visible outside inventory modal; no overlap with modal slot controls observed |
| Android safe area / dp size / touch targets | UNVERIFIED | No Android device run |
| Existing Phase 4A seven-test functional run | VERIFIED as prior evidence | 7 passed, 0 failed, 0 skipped per Phase 4A FUNCTIONAL_QA.md; not rerun |
| Unity compile and final Console | VERIFIED | Modified scripts compiled; final MCP Console query returned 0 errors/warnings |
| User save isolation | VERIFIED | Temporary QA path, autosave disabled; no user save used |

Prior Phase 2B images remain historical reference only: [review](../ui-redesign-phase2b-main-menu-2026-11-06/REVIEW.md), [index](../ui-redesign-phase2b-main-menu-2026-11-06/SCREEN_INDEX.md).
