# UI Redesign Phase 2A — Main Menu Visual Pilot

Status: **visual candidate for user review; not approved for Unity replacement**.

## Preview set

`previews/` contains 18 menu PNGs plus a button-state study:

- H01 saved world: Korean and English × 1920×1080, 1280×720, 844×390.
- H01 no saved world: Korean and English × the same three sizes. This follows current `HomeScreen.cs`: Continue is hidden when there is no valid save, and the primary action reads “새 세계 시작” / “Start new world.”
- H02 existing-save replacement confirmation: Korean and English × the same three sizes. The text reflects the current localization warning that saving a new world replaces the current world and the previous world is no longer available through Continue.
- `button-state-study-ko-1920x540.png`: basic, selected, pressed, and disabled surface treatments. These are visual state examples, not a claim that Continue appears disabled in the no-save runtime state.

Render all previews with:

```powershell
& .\projects\wildfolk\art-source\ui-redesign\main-menu-phase2a-2026-11-05\source\render-preview.ps1 -IncludeStateStudy
```

The outputs have exact target pixel dimensions. Text is drawn live by the compositor from strings checked against the current menu/localization behavior; it is not embedded in the generated art.

## Direction and current implementation facts

The candidate integrates C’s open view and action hierarchy, A’s readability, and B’s tactile slot/button treatment principles into a forest survival-game presentation. It reduces the old book-like menu obstruction, keeps the camp scene visible, uses one clear primary action with quieter secondary routes, and rearranges the compact landscape layout at 844×390 rather than merely shrinking the desktop layout. No multi-world selection route is introduced.

The menu exposes Continue, Single Player, Multiplayer, Class, Settings, Profile, Gem Store, Achievements, and Attendance. The project’s existing play/users/settings/profile/award/calendar/gem symbols are reused where they match. Class remains text-led because the available current menu icon set does not provide a reliable class symbol. Menu copy is based on the current game source and localization, not earlier handoff assumptions.

H02’s frame and the button art are candidates for Unity `Image.Type = Sliced` / 9-slice UI components. The preview compositor’s slice values are exploratory; final sprite borders, Canvas Scaler settings, safe areas, input states, and device typography must be verified in Unity after user approval.

## Visual review

The final PNGs were visually reviewed after correcting transparent button padding, a dark streak in the first dialog frame, a visible slice seam, excess vertical space in H02, English tagline wrapping at 844×390, and footer label wrapping/legibility at 844×390. The revised previews showed no clipped menu controls or text in the reviewed layouts. The English H02 and Korean/English H01/H02 were checked across all target sizes; no-save H01 variants are also included across languages and sizes.

Remaining issues / limits:

- `background-cleanup-concept.png` is generated from a current Unity screenshot. It is a presentation concept only, not production environment art or an authorized scene replacement. The current in-game background must be used for an eventual Unity prototype.
- ImageGen button/dialog surfaces are polished prototype assets, but they still require user judgment for final Lowfire art direction. This is not a claim of finished commercial game art.
- Iconography remains partly dependent on the current small project icon set; Class is intentionally text-only rather than using a misleading symbol.
- No runtime accessibility, touch-target, controller, safe-area, localization-overflow, or device-density test was run. These are required in the later Unity implementation pass.
- Krita was unavailable during this pass, so there is no new `.kra` document. Editable source is the PowerShell composition script and preserved generated PNG originals/crops.

## Penpot and asset reuse

No Penpot file/page/layer was modified, exported, or used as a source asset in this pass. The three-direction, A/B/C, and Stage 1.5 boards were inspected as visual references; only their agreed principles were carried forward. Existing Lowfire wordmark, icons, gem, panel art, and navigation-source manifest were copied from the current project for preview use. New visual parts are the button surface candidates, dialog frame candidate, and composition-only background cleanup concept.

## Unity handoff

After the user selects/approves a candidate, implement only the approved menu in a separate Unity pass using actual scene/environment art, live localized text, existing menu routes, and reusable 9-slice sprites. Keep H01 save/no-save behavior aligned with `HomeScreen.cs`; H02 must preserve the existing replacement warning and cancel/confirm semantics. Use device-aware anchors and safe-area constraints for 844×390. Do not use a full preview PNG as a game screen, and do not start HUD or inventory in this phase.

No Unity Runtime, Editor Builder, scene, prefab, game data, save, profile, protocol, network, live UI, or Penpot original was changed for this visual pilot.
