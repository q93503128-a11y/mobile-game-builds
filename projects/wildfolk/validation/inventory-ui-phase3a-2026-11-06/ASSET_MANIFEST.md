# Asset Manifest and Future Unity Handoff

## New editable design source

| File | Purpose |
|---|---|
| `art-source/inventory-ui-phase3a-2026-11-06/inventory-ui-kit.svg` | Hand-authored component states and component-sheet preview source. |
| `art-source/inventory-ui-phase3a-2026-11-06/generate_previews.py` | Reproducible SVG screen and state generation from current Lowfire data/assets. |
| `art-source/inventory-ui-phase3a-2026-11-06/render_previews.ps1` | Headless SVG-to-PNG rendering using Edge. |
| `validation/inventory-ui-phase3a-2026-11-06/previews/svg/*.svg` | 19 editable screen/state/comparison SVGs. Each screen's text remains vector text, not baked into an image. |
| `validation/inventory-ui-phase3a-2026-11-06/previews/*.png` | 18 screen/state PNG previews, one before/after plate, and one kit PNG. |

## Existing assets referenced in place

- Item sprites and equipment icons are resolved from current `ItemDefinition` assets under `client/Assets/Wildfolk/Art/` using their existing `.meta` GUID references. The script checks the catalog (136 existing `ItemDefinition` assets) and every item used by the full-storage fixture (27 unique IDs); it copies no icon into the art-source directory.
- KO/EN strings come from existing localization resources under `client/Assets/Wildfolk/Data/`.
- Preview text references the already bundled `client/Assets/Wildfolk/Resources/NotoSansKR.ttf`. Its adjacent `NotoSansKR-OFL.txt` records the SIL Open Font License 1.1. The font file/license was not duplicated or modified.
- Existing real Unity screenshots are linked from the repository's validation evidence. The English 844 before/after plate uses the same screenshot file on both sides.

## New geometry and states

The slot and action surfaces are original SVG paths with a dark forest/ember palette. The kit shows idle, selected, compatible, incompatible, primary, secondary, pressed, and disabled appearances. There are no copied third-party textures, icons, screenshots, or generated character/world illustrations.

Prototype geometry reference:

- Slot symbol: 64×64 SVG viewBox; 8-unit clipped corner references.
- Action button symbol: 240×72 SVG viewBox; 8-unit clipped corner references.
- These shapes can be exported as transparent PNG sprites and configured with Unity Sprite Editor border guides as a starting point. The component sheet itself should not be sliced as one sprite. Export each symbol to its own transparent sprite or make a deliberate atlas, then define and inspect each border in Unity.
- Use Unity `Image.Type = Sliced` for the surfaces only if the actual exported sprite preserves the corner/border at target sizes. Keep labels, quantities, item icons, status values, and descriptions as existing live localized text/images.
- Actual CanvasScaler, safe area, EventSystem hit targets, drag/drop events, states, audio, and keyboard/back behavior must be wired to the existing inventory controller/view code. This static prototype supplies no functional interaction.

## External references and rights

Reference screenshots and pages are listed in [`RESEARCH.md`](RESEARCH.md). They were viewed for layout and visual principles only; none were copied into the repository. Lowfire item art is reused only from the existing project assets and retains its existing project ownership/licensing. No new third-party art license or asset dependency is introduced.

## Rebuild

From repository root, run:

```powershell
.\projects\wildfolk\art-source\inventory-ui-phase3a-2026-11-06\render_previews.ps1
```

This regenerates the SVG/PNG previews under this validation folder. It requires the repo's existing Python 3.13 launcher and installed Edge browser. It does not open or focus Unity.
