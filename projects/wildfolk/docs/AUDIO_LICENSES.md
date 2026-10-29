# Audio source licenses — Foundation5

Only selected files under `client/Assets/Wildfolk/Audio/` are covered here. This is the source ledger, not a claim of completed runtime integration or subjective listening approval.

All selected sources use [CC0 1.0 Universal](https://creativecommons.org/publicdomain/zero/1.0/): commercial use, modification and redistribution are permitted. Credits are retained as provenance. No attribution-required, NC, unknown-license or reference-game soundtrack material was selected. No AI-generated audio was obtained: Runway's single music capability probe was refused by the existing Free plan, with no task/output created.

The author/license was checked on the linked primary page on 2026-10-29. Windows HTTPS to Kenney/OpenGameArt failed during TLS negotiation; certificate validation was not disabled. The exact retrieval used pinned public redistribution source files, not extraction from game builds. A mirror's author text is not authoritative: e.g. dungeon ambience is credited to primary-page author JaggedStone rather than the mirror's IgnasD. The downloaded file is a source candidate and may already be transcoded; original-master byte identity is unverified.

`validation/audio-production/assets.json` is the per-output ledger: original source page, actual download URL, author, license, pinned repository commit/blob, downloaded and output SHA256, acquisition/production dates and edits. `source-acquisition.json` includes the acquisition pool; unselected raw files are kept outside the canonical Assets and game release. Edits include DC removal, silence/crop/fades, loop crossfade for beds, measured loudness/peak-limited gain and mono/stereo resampling/encoding. No source is generated from a sine/beep fallback.

| Author | Primary source and license | Selected output names |
|---|---|---|
| CaveboyTup | [Source](https://opengameart.org/content/wolf-monster-sound) · CC0-1.0 | `wolf` |
| JaggedStone | [Source](https://opengameart.org/content/loopable-dungeon-ambience) · CC0-1.0 | `cave_air` |
| Kenney | [Source](https://kenney.nl/assets/impact-sounds) · CC0-1.0 | `step_grass_0`, `step_grass_1`, `step_grass_2`, `step_grass_3`, `step_stone_0`, `step_stone_1`, `step_stone_2`, `step_stone_3`, `wood_hit_0`, `wood_hit_1`, `wood_hit_2`, `wood_break_0`, `wood_break_1`, `wood_break_2`, `stone_hit_0`, `stone_hit_1`, `stone_hit_2`, `ore_hit_0`, `ore_hit_1`, `ore_hit_2`, `ore_break_0`, `ore_break_1`, `flesh_hit_0`, `flesh_hit_1`, `flesh_hit_2`, `heavy_hit_0`, `heavy_hit_1`, `bone_hit_0`, `bone_hit_1`, `bone_hit_2`, `chitin_hit_0`, `chitin_hit_1`, `chitin_hit_2`, `plant_0`, `plant_1`, `plant_2`, `body_fall_0`, `body_fall_1` |
| Kenney | [Source](https://kenney.nl/assets/rpg-audio) · CC0-1.0 | `ui_click`, `ui_back`, `ui_error`, `journal`, `page`, `engineer`, `gold`, `pickup`, `craft_start`, `craft_complete`, `door_open`, `door_close`, `chest`, `equip`, `food`, `bow_draw`, `bow_release`, `crossbow`, `fishing_catch`, `blade_0`, `blade_1` |
| Luke.RUSTLTD | [Source](https://opengameart.org/content/wind1) · CC0-1.0 | `graveyard_air` |
| PagDev | [Source](https://opengameart.org/content/fireplace-sound-loop) · CC0-1.0 | `campfire` |
| Paul Wortmann | [Source](https://opengameart.org/content/dark-cavern-ambient) · CC0-1.0 | `deep_air` |
| RandomMind | [Source](https://opengameart.org/content/fantasy-lament-for-a-warriors-soul) · CC0-1.0 | `graveyard` |
| RandomMind | [Source](https://opengameart.org/content/medieval-exploration) · CC0-1.0 | `day` |
| RandomMind | [Source](https://opengameart.org/content/medieval-rejoicing) · CC0-1.0 | `victory` |
| RandomMind | [Source](https://opengameart.org/content/medieval-the-bards-tale) · CC0-1.0 | `menu` |
| RandomMind | [Source](https://opengameart.org/content/medieval-the-old-tower-inn) · CC0-1.0 | `night` |
| TinyWorlds | [Source](https://opengameart.org/content/forest-ambience) · CC0-1.0 | `forest_day` |
| Wolfgang_ | [Source](https://opengameart.org/content/battle-theme-0) · CC0-1.0 | `boss` |
| Wolfgang_ / Ted Kerr | [Source](https://opengameart.org/content/crickets-ambient-noise-loopable) · CC0-1.0 | `forest_night` |
| artisticdude | [Source](https://opengameart.org/content/rpg-sound-pack) · CC0-1.0 | `ui_confirm`, `drink`, `treat`, `water_splash`, `fishing_bite`, `bead_reward`, `ring_reward`, `boss_roar`, `boss_summon`, `swing_0`, `swing_1`, `swing_2`, `undead_0`, `undead_1`, `undead_2`, `goblin_0`, `goblin_1`, `goblin_2`, `slime_0`, `slime_1`, `slime_2`, `insect_0`, `insect_1`, `insect_2`, `bat_0`, `bat_1` |
| beardalaxy | [Source](https://opengameart.org/content/mysterious-cave-theme-loop) · CC0-1.0 | `cave` |
| pauliuw | [Source](https://opengameart.org/content/animal-or-beast-sounds) · CC0-1.0 | `beast` |
| syncopika | [Source](https://opengameart.org/content/bird-chirping-sounds) · CC0-1.0 | `bird` |

Luke.RUSTLTD's wind1 is explicitly a designed PureData wind recording/render rather than a field recording. The original author credits Andy Farnell's tutorial. This is source disclosure, not a claim of a new Lowfire procedural sound generator. Originals/license artifacts and local editing helpers currently remain outside Git at the task workspace's `audio-production-work/`; the pinned URLs/hashes in the ledger allow the exact downloaded files to be identified. Before public release, finish the runtime/audio-quality gates in REVIEW.md.

## Runtime selection update

Current selection is101files: Music6/Ambience6/SFX89. Three unsupported wood-floor footstep candidates were removed; the former104-file checkpoint remains historical in REVIEW. All current files retain their original source/hash/processing ledger. This pass adds no AI-generated audio. Technical playback/output verification is separate from human/device listening approval.
