# Button and Frame Asset Specification

## Visual tokens

| Surface | Top | Bottom | Edge / cap | State cue |
|---|---|---|---|---|
| Primary | `#9F5439` | `#8B432D` | `#543326` | Ember face, raised at rest |
| Secondary | `#3B5947` | `#2A4336` | `#172D24` | Deep forest face |
| Secondary selected | `#526F47` | `#426645` | `#203B2B` | Moss face plus short pale leading mark |
| Primary pressed | `#95492F` | `#7A3829` | `#42281F` | Darker face and 2px down shift |
| Secondary pressed | `#2F493B` | `#243B31` | `#12251D` | Darker face and 2px down shift |
| Disabled primary | `#626457` | `#505448` | `#343A32` | Low-saturation neutral surface |
| Disabled secondary | `#37443A` | `#2D3931` | `#17251F` | Low-saturation neutral surface |

The palette follows the Phase 1 candidate tokens: ember is reserved for the dominant action; forest and moss carry secondary/selected states. The edge is a dark contour compatible with the bold outlines in the existing world art. There is no gold outline or generated texture.

## Contrast check

The button text colors used by the screen compositor were checked against the lighter endpoint of each authored surface gradient using the WCAG relative-luminance contrast formula. After the final tone adjustment, the lowest calculated ratios are 4.65:1 for the warm primary surface with `#F4EBD3` supporting text and 4.90:1 for selected moss with `#F4EFDC` text. Bright `#FAF4E3` labels on every state are at least 5.03:1. Secondary and disabled surfaces exceed 5:1 with their intended light text. Ratios describe the authored flat endpoint colors only; they are not Unity, display, antialiasing, or accessibility certification.

## Repeatable source shape

Button SVGs use one 360×120 viewBox with mirrored, fixed clipped corners. The face is a flat vertical gradient. Two short end-cap seams stay inside the fixed 96px raster slice caps; the center stretch region contains only flat color and one thin highlight. There are no labels baked into the image.

- Raster: 720×240 transparent PNG at 2× from the SVG.
- Button Unity slice candidate: left/right 96px, top/bottom 56px in the PNG source.
- Frame SVG viewBox / raster: 720×520.
- Frame Unity slice candidate: 64px each side.

These borders are geometry-based candidates, not approved Unity Sprite Editor metadata. Inspect the seams and corners under actual Unity slicing before production use.

## State package

`assets/png/` and `assets/svg/` contain:

- `button-primary-default`
- `button-primary-pressed`
- `button-primary-disabled`
- `button-secondary-default`
- `button-secondary-selected`
- `button-secondary-pressed`
- `button-secondary-disabled`
- `dialog-frame`

The state sheet covers every exported state and shows the same live-text sample on all surfaces, including disabled states. Unity should preserve readable text/icon contrast while applying state changes; this visual pilot does not create runtime state wiring.
