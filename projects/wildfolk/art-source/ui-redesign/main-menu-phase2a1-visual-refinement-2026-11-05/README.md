# Lowfire Main Menu Phase 2A.1 — Visual Refinement

This is a visual-only iteration on Phase 2A. It preserves the H01/H02 functions and overall split between the camp view, primary action area, utility actions, and lower meta actions. It does not alter Unity, Penpot, or the Phase 2A baseline files.

## Contents

- `assets/svg/` — editable SVG vector originals for the common button states and confirmation frame.
- `assets/png/` — transparent 2× button raster exports and confirmation-frame raster, suitable for later Unity 9-slice review.
- `assets/reused/` — current Lowfire `ui_next.png` with the existing navigation icon source manifest.
- `source/export-menu-surfaces.ps1` — deterministic SVG and PNG exporter; shape coordinates and state colors are authored in this file.
- `source/render-phase2a1.ps1` — editable screen compositor. It uses the Phase 2A background concept and existing Phase 2A icon/logo copies without altering them.
- `ASSET_SPEC.md` and `LICENSES.md` — state, palette, slicing, source, and license notes.

## Regenerate

```powershell
& .\projects\wildfolk\art-source\ui-redesign\main-menu-phase2a1-visual-refinement-2026-11-05\source\export-menu-surfaces.ps1
& .\projects\wildfolk\art-source\ui-redesign\main-menu-phase2a1-visual-refinement-2026-11-05\source\render-phase2a1.ps1 -IncludeStateStudy
```

The renderer writes H01/H02 saved/no-save variants in Korean and English at 1920×1080, 1280×720, and 844×390. It also writes three before/after comparison sheets and a component state comparison sheet into `projects/wildfolk/validation/ui-redesign-phase2a1-main-menu-refinement-2026-11-05/previews/`.

## Design changes

- Replaced generator-made irregular strokes and surface texture with consistent clipped wood-plank profiles, a calm edge thickness, simple vertical cap seams, and one restrained top glint.
- Replaced a shared long plate behind each three-item row with three visible independent button surfaces and gaps, so each auxiliary function reads as its own control.
- Introduced separate primary, secondary, selected, pressed, and disabled artwork. Pressed surfaces sink by a small offset; selected uses a moss face plus a short leading marker; disabled states are desaturated while retaining a readable surface.
- Increased 844×390 action/button text and heights. No-save H01 keeps Continue hidden and exposes “Start new world” plus Multiplayer, in line with current `HomeScreen.cs` behavior.
- Continue uses the existing right-arrow icon; Start new world / Single Player use the existing play symbol. This removes the duplicate play cue on Continue.
- Rebuilt the H02 frame as a quiet, single warm wood edge around the deep forest surface. Text remains separate and live in the previews.

## Approval and Unity boundary

The preview background is the Phase 2A generated cleanup concept, retained solely to make the before/after surface comparison fair. It is not the real Unity scene art. The screen PNGs are not a proposed full-screen sprite. The vector/button assets are review candidates and have not been imported into Unity. The new art still needs user approval before any Unity implementation, and later real-device validation is required for safe area, physical touch size, typography, and input behavior.

This pass did not change Unity Runtime or Editor code, scenes, prefabs, game data, saves, profiles, protocol, networking, actual game UI, Penpot, HUD, or inventory.
