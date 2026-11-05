# Source and License Notes

## Generated art

The primary/secondary button surfaces, dialog frame candidates, and background cleanup concept were generated for this Lowfire visual pilot with OpenAI ImageGen. No third-party game screenshot, game artwork, font file, or external asset is embedded in them. The initial dialog frame is preserved for provenance but rejected from use because it contains a visible dark streak.

## Reused Lowfire assets

`assets/reused/` contains copies of the project’s existing `lowfire_wordmark.png`, `ui_play.png`, `ui_users.png`, `ui_settings.png`, `ui_profile.png`, `ui_award.png`, `ui_calendar.png`, `journal_gem.png`, and `panel.png`. Their authoritative originals and existing provenance are preserved in the project’s Unity art folders and `projects/wildfolk/docs/SOURCE_ART.md`. These copies do not replace or relocate those sources.

The menu navigation icons are Lucide-derived and carry forward the existing project manifest in `assets/reused/navigation-symbols-sources.json`: source commit `a630cec77eac6568b989e87e4947a8bfe45caacd`, with per-icon source URLs and SHA-256 values. Lucide is ISC licensed; the existing project attribution also retains Feather’s MIT notice in `projects/wildfolk/client/Assets/Wildfolk/Art/Lucide-LICENSE.txt`. No icon source was newly downloaded or modified in this pilot.

## Fonts and compositor

`render-preview.ps1` uses the locally installed Noto Sans KR variable font (`%WINDIR%\Fonts\NotoSansKR-VF.ttf`) through `System.Drawing`; the font binary is not copied into this folder. The project’s distributable font and OFL notice remain in the canonical Unity Resources location, as documented in `projects/wildfolk/docs/SOURCE_ART.md`.

The preview script is a Windows PowerShell composition utility using .NET `System.Drawing`; it adds no Unity/player dependency. All preview labels remain text drawn by the script and should become localized Unity text in an implementation.
