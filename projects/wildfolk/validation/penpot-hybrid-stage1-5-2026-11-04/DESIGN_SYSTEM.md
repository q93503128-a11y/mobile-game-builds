# Hybrid Direction — Design System Proposal

Status: **design proposal; awaiting user approval**. These values describe the Stage 1.5 Penpot candidate and are not Unity tokens or runtime configuration.

## Direction

Use Modern Adventure's compact screen structure as the base, Forest Utility's stronger contrast and clear status hierarchy, and Crafted Survival's warmer, tactile item-slot treatment. Keep the world art visible. Use dark forest surfaces for navigation and action, warm light surfaces for dense inventory work, and terracotta only for the primary commitment or destructive action. Avoid a universal parchment panel, gold trim, leaf ornament, and ungrounded fantasy illustration.

## Color roles

| Role | Candidate | Use |
|---|---|---|
| World ink | `#20362D` | Main text on light surfaces; high contrast |
| Deep forest | `#294237` | Navigation, HUD backing, modal header |
| Moss action | `#436D50` | Primary non-destructive action and selected navigation |
| Leaf accent | `#6F8E61` | Secondary indicator, progress, subtle selection |
| Warm surface | `#F3EBD5` | Dense inventory, menu cards, settings surfaces |
| Raised surface | `#E6DEC4` | Slot fill and secondary card separation |
| Ember action | `#C96D4A` | Main menu CTA, selection edge, critical action emphasis |
| Health | `#B95C49` | Health only; pair with label and value |
| Thirst | `#66AFC1` | Thirst only; pair with label and value |
| XP / warmth | `#D6A956` | XP and neutral warmth; not a success color |
| Muted ink | `#718073` | Secondary labels only when measured contrast remains adequate |
| Divider | `#B6B18F` | Quiet separation on light surfaces |

These are starting values, not a contrast certification. Do not encode state with color alone. Pair error, success, selection, and disabled states with text, iconography, shape, and/or position.

## Typography and scale

- Use Noto Sans KR for the shared Korean/Latin candidate. Confirm actual Unity font asset coverage and Android glyph rendering separately.
- Roles: brand, screen title, section title, item/class identity, body, numeric value, action label, and caption. Keep values tabular where available; never shrink all text to fit.
- Candidate minimums at the 844×390 design frame: body and interactive labels 14–16 frame units; secondary labels 12–14 only for noncritical metadata; item counts 14 or larger. Penpot frame units are not Android dp.
- Work from a 4-unit rhythm: 4, 8, 12, 16, 24, 32, 48. Use optical exceptions only where icon geometry requires them.
- Touch target candidate floor is 44×44 frame units, with 52–60 preferred for frequent secondary actions and 80–96 for the primary combat action. These are design targets, not device certification.

## Surface and state rules

- World HUD: low-opacity forest surfaces with a dark edge/underlay behind text. Keep the central play field clear. Validate both generated exploration art and the imported Unity forest capture.
- Inventory: warm light utility surface, darker green section headers, individually readable slots, restrained 1–2 px frame hierarchy, no oversized book-shaped container.
- Menu: world image remains dominant; the panel occupies a clear side region and uses a compact set of action cards.
- Modal: opaque enough to separate its actions from live play; show caller context and preserve explicit cancel/back behavior.
- Selection: persistent edge plus small index/badge or background shift. Do not rely on a glow alone.
- Pressed: short inset/value shift; disabled: lower contrast plus a reason where needed; busy: explicit progress/label; error: icon plus actionable text.
- Scroll: visible clipped viewport and edge/continuation cue; retain selection and position when returning from details.

## Component catalog

The Penpot page `08 — Hybrid Components` should hold editable source components and example states for Primary, Secondary, Danger, Icon, Navigation Row, Inventory Slot, Equipment Slot, Hotbar Slot, Item Detail, Resource Bar, Progress, Modal, Scroll Surface, Touch Action, Joystick, Selection, Disabled, Error, and Confirmation. Components are design artifacts only and are not linked to Unity prefabs.

## Responsive rules

- 1920×1080: full world context and separated utility regions.
- 1280×720: preserve hierarchy and touch scale; reduce gaps and secondary chrome before shrinking labels.
- 844×390: landscape-specific reflow, compact status row, protected hotbar band, and separated left/right thumb zones. Keep the 9-slot hotbar and 36-slot inventory intact.
- Safe area is represented as a design inset and must be checked against actual device cutouts and gesture areas. No export proves Android dp, device reach, contrast under motion, or runtime hitboxes.

## Source and reuse

Existing Lowfire wordmark, existing item icons, and actual in-game capture are reused as references where available. Generated concept scenery remains clearly identified as exploration art and is not a proposed replacement for the game's art. Any component screenshot or icon is an image reference; the surrounding UI geometry and labels remain editable Penpot shapes.
