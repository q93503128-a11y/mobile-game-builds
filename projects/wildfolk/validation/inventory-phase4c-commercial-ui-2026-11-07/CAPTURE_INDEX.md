# Capture Index

## Final authoritative Unity screenshots

These 12 PNG files are direct `ScreenCapture.CaptureScreenshot` outputs from the connected Unity Editor Play Mode. Each has the exact dimensions in its filename. Capture session: `final-verified6`; isolated QA profile; autosave disabled; world save not written. The run checked selected-equipped versus compatible-candidate status plus text containment; 0 boundary failures.

| Screenshot | Language | Resolution | State |
|---|---|---:|---|
| [inventory-ko-844x390-base.png](captures/final-verified6/inventory-ko-844x390-base.png) | Korean | 844×390 | Storage 27, Hotbar 9, four equipped icons |
| [inventory-en-844x390-base.png](captures/final-verified6/inventory-en-844x390-base.png) | English | 844×390 | Base |
| [inventory-ko-1280x720-base.png](captures/final-verified6/inventory-ko-1280x720-base.png) | Korean | 1280×720 | Base |
| [inventory-en-1280x720-base.png](captures/final-verified6/inventory-en-1280x720-base.png) | English | 1280×720 | Base |
| [inventory-ko-1920x1080-base.png](captures/final-verified6/inventory-ko-1920x1080-base.png) | Korean | 1920×1080 | Base |
| [inventory-en-1920x1080-base.png](captures/final-verified6/inventory-en-1920x1080-base.png) | English | 1920×1080 | Base |
| [inventory-ko-844x390-equipment-selected.png](captures/final-verified6/inventory-ko-844x390-equipment-selected.png) | Korean | 844×390 | Selected equipped item |
| [inventory-ko-844x390-replacement-candidate.png](captures/final-verified6/inventory-ko-844x390-replacement-candidate.png) | Korean | 844×390 | Compatible candidate |
| [inventory-ko-844x390-replacement-applied.png](captures/final-verified6/inventory-ko-844x390-replacement-applied.png) | Korean | 844×390 | Replacement applied |
| [inventory-ko-844x390-split-modal.png](captures/final-verified6/inventory-ko-844x390-split-modal.png) | Korean | 844×390 | Opaque split modal over full inventory |
| [inventory-en-844x390-long-item-name.png](captures/final-verified6/inventory-en-844x390-long-item-name.png) | English | 844×390 | “Undead King's Crown” long title and description |
| [inventory-ko-844x390-empty-equipment.png](captures/final-verified6/inventory-ko-844x390-empty-equipment.png) | Korean | 844×390 | Four empty equipment slots |

## Before / after evidence

- [Phase 4B storage-full selected-wood screenshot](../inventory-unity-visual-qa-2026-11-07/screenshots/ko-844x390-storage-full-selected-wood.png) → [Phase 4C 2×2 equipment base](captures/final-verified6/inventory-ko-844x390-base.png).
- [Before redundant equipped captions were removed](captures/before-fixes/inventory-ko-844x390-before-status-cleanup.png) → [final compact equipment state](captures/final-verified6/inventory-ko-844x390-base.png).
- [Before opaque modal backing](captures/before-fixes/inventory-ko-844x390-before-opaque-modal.png) → [final opaque split modal](captures/final-verified6/inventory-ko-844x390-split-modal.png).

## Phase 4B source set

The previous visual QA folder contains 13 direct Unity PNGs: 844×390 Korean inventory/equipment/split states, 844×390 English long-name states, and before/after layout screenshots at 1280×720 and 1920×1080. Its original capture README and screenshots remain unchanged.
