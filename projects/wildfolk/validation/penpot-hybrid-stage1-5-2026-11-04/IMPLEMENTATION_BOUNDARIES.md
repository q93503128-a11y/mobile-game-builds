# Implementation Boundaries — Stage 1.5

This deliverable is a design candidate and interaction specification. User approval is pending. Nothing in the Penpot file or exported previews is connected to Unity runtime behavior.

## Current implementation facts relevant to these screens

- Unity client is the source of truth for current actions, item data, equipment slots, save/profile/network behavior, and current Main Menu navigation.
- Main Menu currently supports Continue, Single Player/new-world flow, Multiplayer, Class, Settings, Profile, Gem Store, Achievements, and Attendance. Continue refers to the current save; Single Player begins a new world and may ask to replace existing save data.
- There is no current player-facing world-select screen or multi-world save flow. Any selection entry is future information architecture only and must not appear in this candidate as a currently usable feature.
- Inventory is 36 total bag positions: 9 hotbar positions plus 27 storage positions. Equipment roles are Head, Body, Utility, and Accessory. Existing equip/unequip, split, drag/move, discard, selected-item detail, and crafting navigation are preserved as requirements.
- Current controls include floating joystick, Primary, Interact, Dash, contextual Class action, Inventory, Pause, hotbar, survival state, Level/XP, and date/time. `ControlSize` is the implemented customization setting. User-positioned layout, button opacity, and left-handed mode are future proposal only.
- Current item IDs, stack counts, slot indices, recipe details, button callbacks, save identifiers, protocol values, and network authority are not redesigned here.

## This pass may change

- New pages/boards and editable vector/text UI objects in the existing Penpot file.
- Local PNG preview exports and design/UX documentation under `validation/penpot-hybrid-stage1-5-2026-11-04/`.
- Penpot named recovery version and documentation-only Git commit on `main`.

## This pass must not change

- Unity C# source, scenes, prefabs, serialized assets, item/recipe/world/balance data, save/profile/protocol versions, runtime settings, actual touch controls, backend/monetization gates, or shipped game art.
- User save data, Unity preferences, or project asset imports.
- Any actual world selection, custom control placement, opacity editor, or left-handed setting.
- Previous Stage 1 pages, source components, or review history.

## Handoff after user approval

A separate implementation task must map approved Penpot component and state IDs to canonical Unity UI builders, preserve existing serialized/runtime references, implement or explicitly defer new behavior, and produce locale, aspect-ratio, safe-area, touch, input-cancellation, accessibility/contrast, and performance evidence. New custom control placement additionally requires a versioned storage schema, atomic validation/save/rollback behavior, migration policy, cloud/device ownership decision, and device acceptance testing. A Penpot preview cannot substitute for those checks.

## Evidence labels

- `SOURCE-CONFIRMED`: found in current Unity source/project docs.
- `PENPOT-DESIGN`: created as editable design geometry.
- `PROPOSAL`: suggested behavior or layout requiring approval.
- `UNVERIFIED`: requires Unity runtime, Android device, or service environment.
