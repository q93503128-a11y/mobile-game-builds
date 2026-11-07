# Inventory Phase 4C — Commercial UI & Equipment Redesign

Status: **implemented and Unity-verified for review; final design approval pending**.

Phase 4C preserves the current inventory/equipment data and interaction model, while correcting the equipment presentation, repeated status labels, heavy slot treatment, and item-modal backdrop. It does not implement a new inventory system.

## Source of truth

- Game repository start: `mobile-game-builds` main `87e58e04cfbdff317e98da91502141cd47e72791`.
- Shared playbook main fetched during this pass: `dev-playbook` `e022e9e4ff3947fcbd7f5b0237c5905d7c0409c7`.
- Unity MCP project: `C:/Users/User/Documents/ChatGPT/문승준 모바일/canonical/projects/wildfolk/client` (exact canonical path), Unity `6000.3.19f1`.
- Both gameplay scenes use `CanvasScaler` Scale With Screen Size, 1920×1080 reference resolution, Match Width Or Height at 0.5; the inventory builder uses a 2050×840 reference-space panel and adapts it to the screen through the existing fit component. These editor units are not physical dp/pt.
- Phase 4B input: [inventory-unity-visual-qa-2026-11-07](../inventory-unity-visual-qa-2026-11-07/README.md) and its 13 Game View PNGs.

## Result

- `VERIFIED` — 12 final Unity Play Mode Game View captures: 844×390 first, plus 1280×720 and 1920×1080, Korean and English; selected equipment, replacement candidate/applied, split modal, long English name, and empty equipment states.
- `VERIFIED` — 0 automated text boundary failures across the final capture run; final QA profile isolated in a unique temp directory, AutoSave disabled, world save not written.
- `VERIFIED` — focused EditMode 2/2 and PlayMode 4/4 after the final UI source change.
- `FIXED` — equipment gear rail now uses four item-first 2×2 cells; redundant “equipped” captions are limited to the selected equipped item. Empty/candidate states remain visible.
- `FIXED` — selecting a gear slot no longer loses its selected state when clearing the inventory drag selection; compatible replacement candidates remain actionable.
- `FIXED` — split and item-action modal surfaces receive opaque backings in both new and refreshed existing scenes. This prevents the inventory grid from bleeding through the modal.
- `UNVERIFIED` — Android hardware touch target size in dp, one-handed reach, display cutout/safe area, and physical readability. Unity screenshot pixels are not treated as dp.
- `UNVERIFIED` — final visual/design approval by the user.

## Read next

- [External comparison and adopted principles](RESEARCH.md)
- [Final captures and Phase 4B comparison](CAPTURE_INDEX.md)
- [Visual findings and corrections](VISUAL_REVIEW.md)
- [Tests and run isolation](TESTS.md)
- [Remaining verification limits](KNOWN_ISSUES.md)
- [Time log](TIME_LOG.md)

The final authoritative PNGs are under [`captures/final-verified6/`](captures/final-verified6/). Before/after evidence is retained in [`captures/before-fixes/`](captures/before-fixes/). Older diagnostic capture iterations remain local and are excluded from Git by [`captures/.gitignore`](captures/.gitignore); only `final-verified6` represents the final source state.

## Change boundaries

Canonical Inventory UI builder/runtime and its two gameplay scenes were changed. Inventory items/counts, equipment rules, save/profile/protocol, multiplayer authority, recipes/economy, and other UI screens were not changed. The user still needs to review the PNGs before design approval or further UI expansion.
