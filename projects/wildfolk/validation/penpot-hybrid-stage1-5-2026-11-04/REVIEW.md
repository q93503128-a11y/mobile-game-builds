# Penpot Stage 1.5 — Hybrid Direction

Status: **in progress — Penpot responded briefly on 2026-11-05, but full design authoring is still blocked by intermittent MCP suspension. Final design review is not complete. User approval is pending. No Unity or game data was changed.

## Requested direction

The candidate is to use Modern Adventure's structure, Forest Utility's legibility and hierarchy, and Crafted Survival's item-slot material. The supplied screenshots are references, not feature authority or whole-screen art to copy. Current menu, inventory, gameplay, and control behavior must stay source-accurate. The new custom control editor remains a future proposal until separately approved.

## Confirmed Penpot file state

- File: `Lowfire — UI Redesign`
- File ID: `c6f51e89-5d00-81ce-8008-e483b7f6725d`
- The six original A/B/C, component, and responsive pages remain present.
- Four Stage 1.5 pages exist: `06 — Stage 1.5 Hybrid Direction`, `07 — Mobile Control UX`, `08 — Hybrid Components`, and `09 — Stage 1.5 QA`.
- At reentry, before this continuation created menus, the four new pages were empty and the file still had only its original 9 reusable components.
- The file contains nine direction-specific library components. A genuine Unity gameplay capture was imported as media in earlier work (ID `c6f51e89-5d00-81ce-8008-e4dbd9dc326a`); the 2026-11-05 continuation also uploaded the same capture under media ID `542abcb8-15f9-80f7-8008-e582c31d88bd`. Neither media object has been used on a Stage 1.5 board yet.
- During this continuation, an initial failed helper left an empty frame and two empty top-level objects; these were removed before the successful menu boards were created. The menu frame IDs and current state are recorded below.

## Work completed in this checkpoint

- Re-fetched `origin/main`; local and remote were both `58c56328d7ac4f09209dd733c567fb2c69318fc3` at start.
- Re-read the Stage 1.5 attachment, project instructions, README, Stage 1 review/resume, and UI design handoff. Ran the Product Design user-context preflight; no saved plugin context exists, so the repository and supplied screenshots remain the grounding sources.
- Added a token/component proposal, current input contract, two 844×390 combat-layout principles, future control-editor state and persistence spec, and Unity handoff boundaries in the companion documents.
- The documentation-only checkpoint was pushed to `main` as `1494c02c9510f2ab91891745daed9fa45cfb56ec`; local and remote matched and the worktree was clean at 20:59:31 KST. This is not the final design commit.
- On 2026-11-05 the MCP initially read the file and accepted the capture upload. The first menu-building call then stopped with `Cannot set property width ... which has only a getter`; inspection showed the constructor had left an empty frame and two empty top-level objects. Calls subsequently returned `The Penpot plugin tab appears to be suspended by the browser (no heartbeat ...)`. The helper must use `.resize(width, height)` and must not append a `createBoard()` result a second time. No Windows input or Computer Use was used.

## Planned boards still required

- Hybrid main menu: editable KO frames now exist at 1920×1080 (`30c64dc0-bf08-8034-8008-e58395b4c4fc`), 1280×720 (`30c64dc0-bf08-8034-8008-e58519e44871`), and 844×390 (`30c64dc0-bf08-8034-8008-e5852defbe15`), each with 56 children. The full-size frame uses the existing generated concept backdrop; it is not shipping art.
- Hybrid gameplay HUD, 1920×1080 and 844×390 over the actual Unity forest capture.
- Inventory, 1920×1080 and 844×390 with 9 hotbar + 27 storage slots and exactly Head/Body/Utility/Accessory equipment.
- General controls settings; edit mode; selected/dragging Primary; size/opacity and restore/apply/cancel states.
- Layout A attack-centered and Layout B spread controls, both at 844×390.
- Component catalog and QA board covering 1920×1080, 1280×720, 844×390 and KO/EN review states.
- Export the nine requested PNGs, inspect their pixels, revise any issues, save a named Penpot recovery version, and update this report with actual board/component IDs and verified results.

No preview PNG has been generated for this Stage 1.5 checkpoint. The empty `previews/` directory is not a preview deliverable.

## Unverified

- The menu boards exist but have not been pixel-inspected because PNG export timed out. English variants, all HUD and inventory screens, controls/editor states, new reusable components, and the 1280×720/844 render review remain incomplete. Actual Unity-forest contrast review also remains incomplete.
- Touch comfort, thumb travel, safe-area behavior, Android scaling, performance, and every runtime interaction.
- Penpot named recovery version, final revision, and Git commit/push for the full design pass.

Continue from the existing Stage 1.5 pages when Penpot MCP is responsive. Page 06 now contains the three menu boards above; Pages 07–09 are empty. Re-query before creating anything. Do not duplicate menu boards or claim this checkpoint is the finished design.

## 2026-11-05 continuation checkpoint

The Penpot API was responsive for several read/import calls and menu-board creation, then suspended during PNG export. The three menu frames were confirmed with 56 child objects each before the export attempt. The 1920×1080 export timed out after 120 seconds; no Stage 1.5 PNG was saved or visually inspected. The remote main SHA was `7b84cb75ee110372e295be0b1776816793c84782` at start. See `RESUME.md` for current board IDs and continuation steps.
