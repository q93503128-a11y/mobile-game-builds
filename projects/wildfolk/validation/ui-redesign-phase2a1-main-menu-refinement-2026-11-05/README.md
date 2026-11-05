# UI Redesign Phase 2A.1 — Main Menu Visual Refinement

**Status: review candidate; user design approval is still required.**

## Outputs

`previews/` contains 18 replacement menu previews, one seven-state component board, three before/after layout boards, and one before/after component-state board.

The 18 menu previews cover:

- H01 saved world — Korean and English at 1920×1080, 1280×720, 844×390.
- H01 no save — Korean and English at the same sizes.
- H02 existing-save replacement confirmation — Korean and English at the same sizes.

Every preview was generated at the exact target pixel dimensions. Screen text is rendered as live text by the compositor, not baked into button art. Current save behavior is preserved in the mockup: Continue appears only when a valid save exists. No world-selection screen or new product action is added.

## Visual review and correction

The common button/frame was reviewed as an isolated 9-slice candidate before application. The irregular ImageGen outlines and texture were replaced with crisp mirrored profiles, fixed edge thickness, and simple cap seams. The 9-slice stretch region has no repeating grain to distort. H01 three-button utility rows and bottom meta routes now have separate visible surfaces and gaps. H02 Cancel and confirm use the same secondary/primary family.

The first H01 no-save 844×390 render placed the single Multiplayer label too high relative to its icon. That layout was corrected before final output. The final KO/EN mobile screens show increased label sizes and 58–84px preview target heights; these are canvas pixels, not Android dp. Text and control dimensions still require physical-device verification.

The before/after composites compare the complete Korean/English H01 save/no-save and H02 states at each target resolution. `comparison-button-states.png` compares the old four-state material study with the new seven-state package.

## Remaining limits

- The established Phase 2A generated background concept was deliberately held constant for a surface-only comparison. It is not canonical Unity environment art.
- The new authored vector surfaces are cleaner and repeatable, but remain flat two-dimensional prototype graphics. They do not establish that final Lowfire production art is complete or commercially approved.
- The state study displays visual assets only. Unity must connect these states to live labels, icons, keyboard/gamepad focus, pointer/touch press, and true disabled reasons.
- No Unity or Android runtime, touch, safe-area, accessibility, or performance validation was performed because implementation was expressly out of scope.

## Scope and handoff

Only the Phase 2A.1 visual source, export PNGs, composed previews, and this report were added. Unity source/data/scenes/prefabs/UI, Penpot, HUD, and inventory remain unchanged. If approved, the next implementation pass should wire these 9-slice candidates into the existing canonical home-screen builder, use the real game background, preserve H01/H02 behavior/localization, and run the required Unity and device checks. Do not replace the Unity menu before approval.
