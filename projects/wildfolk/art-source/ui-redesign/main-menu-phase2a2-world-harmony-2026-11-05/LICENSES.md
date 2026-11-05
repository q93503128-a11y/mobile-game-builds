# Source and License Notes

## Authored UI surfaces

The button faces and dialog frames in `assets/svg/` are new, editable vector shapes authored for this Lowfire study. Their PNG counterparts in `assets/png/` are exported from those SVG definitions by `source/export-components.ps1`. They contain no labels, icons, fonts, third-party images, or copied screenshots. No external visual material was downloaded for this pass.

## Existing Lowfire material

The preview compositor reads, but does not copy or alter, existing project assets: `ground.png`, `tree_oak.png`, `tree_olive.png`, `campfire.png`, `fallen_log.png`, `storage_chest.png`, `workbench.png`, `rock_wide.png`, `rock_split.png`, `grass.png`, `contact_shadow.png`, `lowfire_wordmark.png`, `ui_play.png`, `ui_next.png`, `ui_users.png`, `ui_settings.png`, `ui_profile.png`, `ui_award.png`, `ui_calendar.png`, and `journal_gem.png` from `client/Assets/Wildfolk/Art/`. Their project source and attribution remain canonical there and in `projects/wildfolk/docs/SOURCE_ART.md`.

The navigation symbols are existing project assets. The existing Lucide ISC and Feather MIT notices remain in `client/Assets/Wildfolk/Art/Lucide-LICENSE.txt`; no icon was newly sourced or edited here. The compositor reads the local Noto Sans KR font installed on the authoring machine and does not bundle it. The project’s existing Noto Sans KR/OFL attribution remains canonical in the project’s art documentation.

See `projects/wildfolk/validation/ui-redesign-phase2a2-world-harmony-2026-11-05/source-manifest.json` for hashes of referenced game art, Unity source, and audit screenshots.
