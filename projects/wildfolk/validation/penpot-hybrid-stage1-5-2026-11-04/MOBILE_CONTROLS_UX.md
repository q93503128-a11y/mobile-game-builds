# Mobile Controls UX — Stage 1.5 Proposal

Status: **future interaction proposal**. Unity currently exposes only a `ControlSize` setting (0.8–1.4). User-positioned controls, opacity controls, left-handed mode, and saved custom layouts are not implemented.

## Current input contract (source-confirmed)

- Landscape HUD includes a floating left joystick, primary action, context interaction, dash, class-specific action, inventory, pause, nine hotbar slots, Health/Hunger/Thirst, Level/XP, and date/time.
- The joystick appears at the touch-down position within the left-side input region, owns the initiating pointer, and returns to zero on release, focus loss, pause, or session/state changes. Sprint is driven by sustained joystick movement with hysteresis; there is no separate sprint button.
- Primary is tap-driven; holding repeats Attack only. Interact is a click for world use. The teammate rescue action is a hold and cancels on release, leaving range, or state change.
- Dash is a 0.18-second movement action; do not depict it as invulnerability.
- Class action is contextual to class; a few role actions share one thumb location. Stamina is a transient player-adjacent arc, not a permanent stat bar.
- Menu/modal screens block gameplay input. Inventory/crafting leave the world running; single-player pause freezes it.

## 844×390 layout candidates

Coordinates below are candidate frame coordinates, not Android pixels or dp. Insets are illustrative and must be remeasured per device.

### Shared zones

- Illustrative safe inset: 18–22 units on left/right and 12–18 at top/bottom.
- Left movement zone: approximately x=20–230, y=165–356. Keep the joystick floating; show a small base only after pointer-down. The hotbar must not become part of the joystick's active gesture region.
- Status row: upper-left x=18–390, y=14–58. Show HP, Hunger, Thirst and Level/XP with labels and values; never add a persistent stamina bar.
- Time/date and pause/inventory are upper-right with independent hit targets and visual separation.
- Hotbar is a nine-slot horizontal band centered near the bottom, target approximately 42–46 units per slot plus small gaps. It begins right of the left movement cluster and ends before the right action reach area.
- Keep the middle play field open. Treat world-covered area as an approximate overlay mask; confirm with real device captures and motion.

### Layout A — attack-centered cluster

- Primary Attack is the largest control (candidate 88–96) in the lower-right thumb rest area.
- Interact (candidate 52–58) sits diagonally above-left; Dash (candidate 52–58) sits above or below-left with a different silhouette and accent.
- Class action occupies a reserved contextual position above the group; do not show mutually exclusive role actions at once.
- Benefit: short thumb travel to repeated combat input. Risk: neighboring utility actions need clear shape, spacing, and label/icon distinction. Reject any overlap that makes Attack/Interact ambiguous.

### Layout B — spread combat actions

- Keep Attack as the largest lower-right anchor.
- Move Interact to the upper-left edge of right reach and Dash to the upper-right edge, each with its own 52–60 target and distinct accent/icon.
- Reserve a class-action target above the action plane when available.
- Benefit: clearer separation and less accidental neighbor activation. Risk: more thumb travel and increased right-side sight obstruction. The hotbar must stay outside the action cluster.

Neither layout is declared the winner from a static mock. A short physical-device task test should measure target acquisition, accidental taps, repeated attack comfort, rescue-hold cancellation, and view obstruction for both hands.

## Manual control-layout editor (future UX, not current settings)

### Editable vs fixed

- Editable: floating joystick visual anchor/scale, Primary, Interact, Dash, contextual Class action, and optional inventory shortcut if the HUD presents it as a thumb control.
- Fixed: health/needs/XP/time information, hotbar slot order and item assignment, pause/navigation, safe-area boundary, and transient stamina/feedback. Fixed does not mean visually immovable in a future redesign; any new movable status element requires separate scope.
- Every visible role-action slot keeps a stable logical action ID. Class variants swap the button content, not its saved identity.

### States (Penpot boards)

A. General Controls: existing control-size value and a clearly marked proposal entry for future layout editing.
B. Edit Mode: gameplay input suspended; safe-area overlay, editable/fixed legend, reset, preview, Apply, and Cancel.
C. Attack selected/dragged: selected outline, anchor/drag ghost, original position ghost, snap feedback, bounds, and a clear cancel affordance.
D. Size/opacity: separate sliders with readable numeric values and hit zones; preview updates immediately without applying persistent data.
E. Restore/Apply/Cancel: restore requires confirmation if edits exist; Apply performs validation then save; Cancel discards preview and restores saved layout. Save failure retains the previous saved values and remains in the editor with a recoverable error.

### Collision and bounds policy

- Clamp editable controls to the safe area with a minimum visible and reachable action footprint.
- Prevent primary action targets from crossing the hotbar band, pause/inventory controls, joystick activation zone, or each other. Allow only explicitly tested visual overlap between non-interactive decoration and hit areas.
- Do not auto-resolve collisions silently. Show both affected controls and a localized conflict message; require the player to move one control or use the default layout.
- Keep a center playfield soft-exclusion region to protect combat visibility. It can be advisory, but required action hit regions must not intrude into fixed navigation/stat elements.
- Revalidate layout on size change, orientation change, or safe-area change. If invalid, preserve the stored candidate and offer a safe default preview; never partially overwrite a known-good saved layout.

### Save model recommendation (implementation decision still open)

| Model | Strength | Risk | Recommendation |
|---|---|---|---|
| Absolute screen pixels | Simple to reason about on one fixed device | Breaks on aspect ratio, resolution, cutout, and UI scale changes | Reject as the persisted source of truth |
| Normalized screen coordinates | Resolution-independent in a fixed full-screen frame | Can drift under notches, gesture areas, or changed safe insets | Better than pixels, but incomplete alone |
| Normalized position relative to a named safe-area edge/anchor, plus logical action ID and relative scale/opacity | Adapts to resolution and edge insets; supports different anchors per side | Requires a versioned schema, migration, clamp rules, collision validation, and device QA | Preferred candidate for implementation research; not finalized |

Store a layout schema version and per-control role, anchor, normalized safe-area offset, size multiplier, and opacity. Reconstruct coordinates after safe-area measurement; validate all hit bounds and conflicts before applying. Test landscape devices with left/right cutouts, different aspect ratios, UI scale extremes, and OS gesture strips. Decide where settings live (device-local/profile/cloud) only in a separate implementation pass.

## Acceptance tasks for a later device test

1. Move while tapping Attack repeatedly; confirm thumb paths do not cross.
2. Approach and use an object while Attack is visible; confirm no accidental attack.
3. Begin and cancel a teammate rescue hold by release and by leaving range.
4. Dash while moving and observe that it communicates movement without implying an invulnerability window.
5. Select each of nine hotbar slots while moving; confirm no joystick/hotbar gesture conflict.
6. Edit, create a collision, preview, cancel, restore, save, then simulate save failure; verify previous saved layout remains intact.
7. Repeat at safe-area and UI-scale extremes in KO and EN.

These are proposed acceptance checks, not tests performed for this design pass.
