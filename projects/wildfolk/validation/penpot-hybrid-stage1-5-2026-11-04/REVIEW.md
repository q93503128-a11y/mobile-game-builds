# Penpot Stage 1.5 — Hybrid Direction

Status: **checkpoint only — Penpot design authoring is blocked by the MCP session; final design review is not complete.** User approval is pending. No Unity or game data was changed.

## Requested direction

The candidate is to use Modern Adventure's structure, Forest Utility's legibility and hierarchy, and Crafted Survival's item-slot material. The supplied screenshots are references, not feature authority or whole-screen art to copy. Current menu, inventory, gameplay, and control behavior must stay source-accurate. The new custom control editor remains a future proposal until separately approved.

## Confirmed Penpot file state

- File: `Lowfire — UI Redesign`
- File ID: `c6f51e89-5d00-81ce-8008-e483b7f6725d`
- The six original A/B/C, component, and responsive pages remain present.
- Four Stage 1.5 pages exist: `06 — Stage 1.5 Hybrid Direction`, `07 — Mobile Control UX`, `08 — Hybrid Components`, and `09 — Stage 1.5 QA`.
- The latest successful read found those four new pages empty. No Stage 1.5 boards or reusable components were added in this continuation.
- Prior confirmed state includes nine direction-specific library components and an imported genuine Unity gameplay capture as Penpot media. The media has not yet been used as a Stage 1.5 board backdrop.
- Revision and save state could not be read after the MCP heartbeat stopped. No new Penpot changes were made in this continuation, so there is nothing new from this turn to save.

## Work completed in this checkpoint

- Re-fetched `origin/main`; local and remote were both `58c56328d7ac4f09209dd733c567fb2c69318fc3` at start.
- Re-read the Stage 1.5 attachment, project instructions, README, Stage 1 review/resume, and UI design handoff. Ran the Product Design user-context preflight; no saved plugin context exists, so the repository and supplied screenshots remain the grounding sources.
- Added a token/component proposal, current input contract, two 844×390 combat-layout principles, future control-editor state and persistence spec, and Unity handoff boundaries in the companion documents.
- The documentation-only checkpoint was pushed to `main` as `1494c02c9510f2ab91891745daed9fa45cfb56ec`; local and remote matched and the worktree was clean at 20:59:31 KST. This is not the final design commit.
- Penpot read-only calls after the initial successful read repeatedly returned `The Penpot plugin tab appears to be suspended by the browser (no heartbeat for 52s)`. The user had already reported a green MCP dot and visible canvas. This design checkpoint did not manipulate the browser or call Windows input tools.

## Planned boards still required

- Hybrid main menu, 1920×1080 and a 1280×720 responsive check.
- Hybrid gameplay HUD, 1920×1080 and 844×390 over the actual Unity forest capture.
- Inventory, 1920×1080 and 844×390 with 9 hotbar + 27 storage slots and exactly Head/Body/Utility/Accessory equipment.
- General controls settings; edit mode; selected/dragging Primary; size/opacity and restore/apply/cancel states.
- Layout A attack-centered and Layout B spread controls, both at 844×390.
- Component catalog and QA board covering 1920×1080, 1280×720, 844×390 and KO/EN review states.
- Export the nine requested PNGs, inspect their pixels, revise any issues, save a named Penpot recovery version, and update this report with actual board/component IDs and verified results.

No preview PNG has been generated for this Stage 1.5 checkpoint. The empty `previews/` directory is not a preview deliverable.

## Unverified

- All new Penpot screen geometry, component library additions, rendered previews, KO/EN overflow, the 1280×720 reflow, and actual Unity-forest contrast review.
- Touch comfort, thumb travel, safe-area behavior, Android scaling, performance, and every runtime interaction.
- Penpot named recovery version, final revision, and Git commit/push for the full design pass.

Continue from the existing four empty Stage 1.5 pages when Penpot MCP can read and mutate the file. Re-query before creating anything. Do not create duplicate pages or claim this checkpoint is the finished design.
