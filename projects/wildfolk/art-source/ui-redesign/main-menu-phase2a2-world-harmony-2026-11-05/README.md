# Main Menu Phase 2A.2 — World Harmony

Two art-only visual directions for Lowfire H01/H02. The 16 editable SVG files and their 16 PNG exports are UI surface candidates; labels, icons, and interactions remain separate. No Unity project files or existing Phase 2A/2A.1 outputs are changed by this package.

## Rebuild

Run from the repository root in Windows PowerShell:

```powershell
& 'projects/wildfolk/art-source/ui-redesign/main-menu-phase2a2-world-harmony-2026-11-05/source/export-components.ps1'
& 'projects/wildfolk/art-source/ui-redesign/main-menu-phase2a2-world-harmony-2026-11-05/source/render-phase2a2.ps1' -Direction A
& 'projects/wildfolk/art-source/ui-redesign/main-menu-phase2a2-world-harmony-2026-11-05/source/render-phase2a2.ps1' -Direction B
& 'projects/wildfolk/art-source/ui-redesign/main-menu-phase2a2-world-harmony-2026-11-05/source/build-review-sheets.ps1'
```

The preview scripts use Windows `System.Drawing` and the locally installed Noto Sans KR variable font at `%WINDIR%\Fonts\NotoSansKR-VF.ttf`; the font binary is not copied into this package. PNGs are emitted under `projects/wildfolk/validation/ui-redesign-phase2a2-world-harmony-2026-11-05/previews/`.

## Unity handoff

- Each surface PNG is 720×240 pixels; `surface-manifest.json` records the candidate border values: left/right 96 px, top/bottom 56 px.
- Each modal frame PNG is 720×520 pixels; candidate border values are 64 px on each side.
- SVG view boxes are 360×120 for buttons and 360×260 for the modal, exported at 2× raster scale.
- The manifest explicitly records `verifiedInUnity: false`. Slice borders, CanvasScaler behavior, device safe areas, dynamic type, localization overflow, pointer/keyboard/gamepad states, and runtime tinting still need a Unity implementation and device review after the user selects a direction.
- The output is a design study. A PNG preview is not a clickable prototype and must not be treated as proof that each route is implemented.

## World-art basis

The composite previews reconstruct the current `MainMenu` background from the project’s canonical sprite files and the `FrontendSceneBuilder.Background()` placement recipe. They do not use the generated forest concept from Phase 2A/2A.1. `SOURCE_CAPTURE.md` records the direct Unity Game View screenshots used to verify the current menu and the limits of this reconstruction. Every preview is visibly marked as a simulation, not a Unity capture.
