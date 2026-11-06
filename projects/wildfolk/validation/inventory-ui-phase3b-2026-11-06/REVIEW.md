# Lowfire Inventory Phase 3B — Structural Design Review

**Date:** 2026-11-06
**Status:** A/B candidates prepared for user selection; neither is approved for Unity implementation.
**Scope:** Korean landscape screens at 844×390 only. Six editable SVG screens and six PNG renders, plus one comparison contact sheet.

## Source baseline

- `mobile-game-builds` `main` was fetched before review: `13a4a16b9817a2062860a61a4ae0a1887235ef76`.
- `dev-playbook` `main` was fetched before review: `e022e9e4ff3947fcbd7f5b0237c5905d7c0409c7`.
- The Phase 3A [research](../inventory-ui-phase3a-2026-11-06/RESEARCH.md) and [design review](../inventory-ui-phase3a-2026-11-06/DESIGN_REVIEW.md) informed the baseline. The current game assets and UI source were checked ahead of those earlier design conclusions.
- The source inventory contract remains 27 storage slots, 9 hotbar slots, and 4 equipment slots: Head, Body, Utility, and Accessory. Existing item selection, drag movement, stack splitting, equipment, unequip, and world-drop behaviors remain the functional boundary.
- The background is the committed real Lowfire Unity gameplay capture `validation/penpot-ui-ux-audit-2026-11-04/evidence/live/qa-new-world-hud-ko-844.png`. The inventory icons, stack names, and amounts use existing Lowfire item data and sprites. No new character, world art, item, or Unity/runtime data was made.

## External reference comparison

| Game | Actual public case reviewed | Benefit observed | Difference from Lowfire | Applied to Lowfire? | Reason |
|---|---|---|---|---|---|
| Necesse | [Inventory screenshot](https://necessewiki.com/File:Inventory.png), 1080×307, added 18 Jun 2022; build/platform not identified | A compact inventory work area makes storage and equipment visibly separable. The related interface guide documents separate equipment and item-management areas. | The screenshot is an older, unversioned desktop-like reference. Lowfire has a fixed 27+9+4 contract and does not implement Necesse quick-stack, sort, or restock controls. | **Grouping only.** Both candidates preserve named storage, hotbar, and equipment groups. | Reuse the clarity of grouping, not its panel geometry or unimplemented commands. |
| Core Keeper | [Player-uploaded Steam screenshot](https://images.steamusercontent.com/ugc/17340593094209561879/797FB708FCE3093ECDB8E3C9DD41153A8657CADD/?ima=fit&imcolor=%23000000&impolicy=Letterbox&imw=1024&letterbox=false); upload build/date/platform not stated. The [Steam community page](https://steamcommunity.com/app/1621690) identifies the game and community screenshot context. | The cited frame shows inventory and quick access over a visible cavern; stack counts and item silhouettes read against repeated slot surfaces. | Core Keeper is pixel art with different item scale and equipment rules; Lowfire's wide 844×390 layout needs its own 9+27+4 arrangement. | **World context, repeated slot language, and visible stack counts.** | These are transferable readability principles; the pixels, slot proportions, and equipment layout are not. |
| Terraria Mobile | [Mobile crafting UI screenshot](https://terraria.wiki.gg/zh/wiki/File:New_Mobile_crafting_gui.png), identified by the file page as version 1.4.0.5.1 | The mobile screenshot keeps the world behind the inventory and exposes context-specific split and other actions. Separate mobile controls show that a touch UI needs its own interaction layout. | Terraria exposes sort, favorite, research, quick-stack, filters, and other actions Lowfire does not have. Its screenshot is not a Lowfire capacity or feature specification. | **Purpose-built mobile layout and explicit selected-item actions only.** | Split, discard, move, and equipment actions shown here already exist in Lowfire; Terraria-only actions are deliberately absent. |
| Don't Starve Together | [Klei community screenshot post](https://forums.kleientertainment.com/forums/topic/142452-make-feather-pencil-refillable-with-charcoalincrease-its-durability/) includes a player screenshot of the in-world item row and sign-labelled chest storage; build/platform are not stated. | World, quick-access items, and labeled storage are visually part of one authored survival world. | The cited frame is a base/storage scene, not an open inventory panel; Don't Starve's hand-drawn material language is distinct from Lowfire's existing art. | **World-art continuity only.** The actual Lowfire scene remains behind the inventory. | The screenshot does not justify copying its art style or changing Lowfire's storage behavior. |

The Phase 3A report had cited a different Klei forum thread about griefers as Don't Starve visual evidence. That thread is not an inventory/UI source, so this pass does not rely on it. The replacement source above is still a user screenshot, not official build documentation, and is used only for the limited world/quick-access observation stated in the table.

## A/B structural options

| | A — Bag-centered | B — Tabbed |
|---|---|---|
| Primary composition | 27 slots in a 9×3 field; 9-slot hotbar below; a compact gear opener occupies the otherwise open right side. | Bag and Gear tabs; bag uses a 7×4 field with exactly 27 occupied positions, a right-side contextual detail area, and the hotbar below. Gear tab uses four larger equipment slots and a separate item-action area. |
| Basic state | No detail card; gear is opened on demand. | Bag is the initial tab; details/actions appear only after selection. |
| Selected item | Selection highlights the single Wood stack in storage. Split/discard appear in a contextual card; drag movement remains indicated. | Same selected item and action set, with the details card using the space reserved beside the 7×4 grid. |
| Equipment/move state | Opens a temporary equipment surface over the right side while the bag and hotbar remain visible. | Switches to a dedicated equipment page; a candidate Iron Helmet and current Head slot are visible with equip/unequip controls. |
| Main advantage | One bag screen retains hotbar and storage context while avoiding a permanently occupied detail pane; equipment takes one explicit step to reveal. | More room for each equipment slot and its details; the bag grid and item actions have separate regions. |
| Main cost | The gear panel temporarily occupies the open context area and reduces visible world on that side. | Adds a view switch; the hotbar is on the Bag tab, so it is not shown on the Gear tab. |

Both are presentation/navigation arrangements only; neither creates or removes gameplay behavior. No sort, filter, quick-stack, new storage capacity, or equipment type was added.

The current input code was checked to keep the proposed screen states grounded: tapping a slot selects it; a 0.45-second hold opens the item-action panel for stack counts above one; the split control opens the existing quantity modal; dragging an item to the current drop zone calls the existing world-drop path; compatible equipment drops equip, and dragging equipped gear back to inventory unequips it. The contextual detail/action panel is a proposed UI route to those same operations. “버리기” means dropping the selected item into the world, not deleting it. No new item or save semantics are implied. The mockup does not test these interactions in Unity.

## Phase 3A comparison

Phase 3A kept a 7×4 storage grid while showing equipment and item details persistently beside it. That left less useful room for the storage field and made several functions compete for the same small landscape frame. Phase 3B tests two alternatives instead of reskinning that structure:

1. **A** expands the bag field to 9×3 and removes permanent detail/equipment occupancy. Those areas open when needed.
2. **B** gives the bag and equipment separate views. The bag keeps selection details in a reserved right column; the gear view uses larger equipment targets and explicit actions.
3. Both retain the 9-slot hotbar, show stack numbers, and reuse actual Lowfire item sprites and the existing world capture.
4. Selection states highlight the selected storage stack only. Hotbar items stay visually neutral so a bag selection is not confused with the active quick slot.

## Fidelity, checks, and limitations

- The six SVG files are editable sources. The generator resolves item IDs to current project sprites and localized Korean names; it fails if a referenced item is missing.
- All six rendered screen PNGs were visually inspected at 1:1 size after the final render. An overlapping duplicate hint in B's item-selected state was removed. Tab hit areas were raised from 32 to 40 px, and the top close target from 38 to 44 px. Main action buttons are 40–44 px high; equipment tiles are 132×88 px. These are mockup dimensions, not Android touch certification.
- Each individual screen is exactly 844×390. The 908×737 overview is only a contact sheet for comparison.
- The sample inventory arrangement and counts are a **layout fixture, not a captured or loaded player save**. No user save was read or changed.
- This is a structural prototype, not a production visual approval. The surface treatment is deliberately restrained and uses clean vector panels; it has no bespoke inventory artwork, press animation, focus state, or live pointer feedback. The darkened real screenshot still shows some of the original HUD controls through the open world areas. Those should be re-evaluated in an actual implementation without replacing the world background.
- No Unity editor, runtime, scene, prefab, game data, localization, save/profile/protocol, or Penpot file was changed. No Unity compile, runtime, or Android touch test was run.

## Output map

- Source and reproduction steps: `../../art-source/inventory-ui-phase3b-2026-11-06/README.md`
- Editable screens and PNGs: [`previews/`](previews/)
- A/B contact sheet: [`previews/ab-overview.png`](previews/ab-overview.png)
- Time log: [`TIME_LOG.md`](TIME_LOG.md)

**Decision requested:** choose A or B, or request one focused refinement. Unity implementation, other resolutions, and HUD/inventory expansion remain out of scope until the visual direction is approved.
