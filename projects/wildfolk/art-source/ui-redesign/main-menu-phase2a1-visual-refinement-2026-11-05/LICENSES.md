# Source and License Notes

## New artwork

The button surfaces and confirmation frame are authored for Lowfire in editable SVG source and rasterized by `source/export-menu-surfaces.ps1` using .NET `System.Drawing`. No external game art, screenshot, font, texture, or downloaded visual material is embedded in those assets.

## Reused material

- The renderer refers to the Phase 2A generated background concept and project logo/navigation/gem art; their source notes remain at `projects/wildfolk/art-source/ui-redesign/main-menu-phase2a-2026-11-05/source/LICENSES.md`.
- `assets/reused/ui_next.png` is copied from the current project UI symbol. `assets/reused/navigation-symbols-sources.json` preserves the existing Lucide source commit, URLs, and hashes. The project’s Lucide ISC and Feather MIT attribution remains in the canonical art license file. No icon artwork was changed.
- The preview compositor uses the locally installed Noto Sans KR through `System.Drawing`; it does not copy or redistribute the font. The project’s existing Noto Sans KR OFL files remain canonical.

## Export and preview tools

Both PowerShell files are local authoring/preview utilities only. They add no Unity runtime, player, or package dependency. Existing Phase 2A originals and PNGs remain untouched.
