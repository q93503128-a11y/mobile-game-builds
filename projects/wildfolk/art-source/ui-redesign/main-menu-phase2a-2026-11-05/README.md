# Lowfire Main Menu Phase 2A — Art Source

This folder contains the editable preview compositor, generated button/dialog art, preserved prompt outputs, and copies of existing Lowfire assets used by the Phase 2A visual pilot. It is a review artifact; none of these files have been imported into Unity or Penpot.

## Contents

- `source/render-preview.ps1` — editable layout and compositing source. It draws live labels and action hierarchy over the visual assets and writes PNG previews into `projects/wildfolk/validation/ui-redesign-phase2a-main-menu-2026-11-05/previews/`.
- `source/asset-prompts.md` — prompt provenance and intended use for generated art.
- `source/LICENSES.md` — source and license notes for original and reused materials.
- `assets/generated/` — generated originals and cropped 9-slice candidates. The first dialog attempt is retained and marked rejected; use `dialog-frame-clean-candidate.png` / `dialog-frame-9slice.png` for review.
- `assets/reused/` — copies of current Lowfire logo, menu symbols, gem, panel, and Lucide source manifest. The originals remain in their canonical Unity/art-source locations.

## Preview workflow

On Windows PowerShell, run:

```powershell
& .\projects\wildfolk\art-source\ui-redesign\main-menu-phase2a-2026-11-05\source\render-preview.ps1 -IncludeStateStudy
```

The script uses the installed Noto Sans KR font for compositing, `System.Drawing`, the generated candidates, and existing project art. It does not invoke Unity, modify Unity assets, or contact Penpot. H01 is rendered for saved and no-save states; H02 is the existing-save replacement confirmation. Korean and English are rendered at 1920×1080, 1280×720, and 844×390.

## Unity handoff boundary

These PNGs are design previews, not runtime screens. After visual approval, Unity work should rebuild the layout with live localized text, buttons, icons, and sliced images. Do not use a whole preview PNG as a full-screen background or bake its text into the game. The generated background cleanup is a concept derived from a current Unity screenshot and is not canonical production environment art.

No Unity Runtime, Editor Builder, scene, prefab, game data, save, profile, protocol, network, or live UI files were changed for this pilot.
