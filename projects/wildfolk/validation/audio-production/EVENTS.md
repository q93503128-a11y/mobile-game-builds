# Current audio event inventory

Before: no wired clips. Current:74mapped cues/101referenced clips. A code reference is binding evidence, not proof of every possible real gameplay scenario. Runtime cue cooldown/category reservation may coalesce valid bursts. Loop beds are documented separately in AUDIO.md.

| Cue | Role/category | Priority | Cooldown s | Loop | Spatial policy | Variants / source author (CC0) | Runtime binding |
|---|---|---:|---:|---|---|---|---|
| UiClick | UI | 20 | 0.09 | No | Local UI / confirmed owner | ui_click / Kenney | Runtime/Presentation/AudioButton.cs |
| UiBack | UI | 20 | 0.09 | No | Local UI / confirmed owner | ui_back / Kenney | Runtime/Presentation/AudioButton.cs, Runtime/Presentation/GameAudio.cs |
| UiConfirm | UI | 20 | 0.09 | No | Local UI / confirmed owner | ui_confirm / artisticdude | Runtime/UI/ClassLobbyScreen.cs |
| UiError | UI | 20 | 0.09 | No | Local UI / confirmed owner | ui_error / Kenney | Runtime/Accounts/EconomyService.cs, Runtime/UI/ClassLobbyScreen.cs, Runtime/UI/PlayerFeedback.cs |
| Journal | UI | 20 | 0.09 | No | Local UI / confirmed owner | journal / Kenney | Runtime/Presentation/GameAudio.cs |
| Page | UI | 20 | 0.09 | No | Local UI / confirmed owner | page / Kenney | Runtime/Presentation/AudioButton.cs |
| ClassUnlock | UI | 20 | 0.09 | No | Local UI / confirmed owner | bead_reward / artisticdude | Runtime/UI/ClassLobbyScreen.cs |
| Engineer | UI | 20 | 0.09 | No | Local UI / confirmed owner | engineer / Kenney | Runtime/Accounts/EconomyService.cs |
| Achievement | UI | 20 | 0.09 | No | Local UI / confirmed owner | bead_reward / artisticdude | Runtime/Accounts/EconomyService.cs |
| Attendance | UI | 20 | 0.09 | No | Local UI / confirmed owner | wood_hit_0 / Kenney, wood_hit_1 / Kenney, wood_hit_2 / Kenney | Runtime/Accounts/EconomyService.cs |
| Gem | UI | 20 | 0.09 | No | Local UI / confirmed owner | ring_reward / artisticdude | Runtime/Accounts/EconomyService.cs, Runtime/Accounts/RewardedAds.cs, Runtime/Accounts/StoreService.cs |
| Gold | UI | 20 | 0.09 | No | Local UI / confirmed owner | gold / Kenney | Runtime/Networking/NetworkCommonGrowth.cs, Runtime/Player/PlayerProgression.cs |
| Pickup | UI | 20 | 0.09 | No | Local UI / confirmed owner | pickup / Kenney | Runtime/UI/PlayerFeedback.cs |
| CraftStart | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | craft_start / Kenney | Runtime/UI/PlayerFeedback.cs |
| CraftComplete | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | craft_complete / Kenney | Runtime/Presentation/AudioActor.cs, Runtime/UI/PlayerFeedback.cs |
| Collect | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | pickup / Kenney | Runtime/UI/PlayerFeedback.cs |
| Place | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | wood_break_0 / Kenney, wood_break_1 / Kenney, wood_break_2 / Kenney | Runtime/Networking/NetworkConstruction.cs, Runtime/World/PlayerPlacement.cs |
| Upgrade | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | engineer / Kenney | Runtime/UI/PlayerFeedback.cs |
| Repair | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | craft_start / Kenney | Runtime/World/StructureMaintenance.cs |
| Dismantle | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | wood_break_0 / Kenney, wood_break_1 / Kenney, wood_break_2 / Kenney | Runtime/World/StructureMaintenance.cs |
| DoorOpen | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | door_open / Kenney | Runtime/Presentation/AudioActor.cs |
| DoorClose | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | door_close / Kenney | Runtime/Presentation/AudioActor.cs |
| Chest | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | chest / Kenney | Runtime/Player/PlayerStorage.cs |
| Equip | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | equip / Kenney | Runtime/Presentation/AudioActor.cs |
| Eat | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | food / Kenney | Runtime/Presentation/PlayerActionAudio.cs |
| Drink | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | drink / artisticdude | Runtime/Presentation/PlayerActionAudio.cs |
| Treat | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | treat / artisticdude | Runtime/Presentation/PlayerActionAudio.cs |
| Cast | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | water_splash / artisticdude | Runtime/Player/PlayerFishing.cs, Runtime/Presentation/AudioActor.cs |
| Bite | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | fishing_bite / artisticdude | Runtime/Player/PlayerFishing.cs |
| Catch | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | fishing_catch / Kenney | Runtime/UI/PlayerFeedback.cs |
| Dash | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | swing_0 / artisticdude, swing_1 / artisticdude, swing_2 / artisticdude | Runtime/Player/PlayerLocomotion.cs, Runtime/Presentation/AudioActor.cs |
| Revive | Critical | 16 | 0.12 | No | 38m 2D; owner life recovery local | bead_reward / artisticdude | Runtime/Presentation/AudioActor.cs |
| Downed | Critical | 16 | 0.12 | No | 38m 2D; owner life recovery local | body_fall_0 / Kenney, body_fall_1 / Kenney | Runtime/Presentation/AudioActor.cs |
| PlayerHurt | Critical | 16 | 0.12 | No | 38m 2D; owner life recovery local | flesh_hit_0 / Kenney, flesh_hit_1 / Kenney, flesh_hit_2 / Kenney | Runtime/Presentation/AudioActor.cs |
| PlayerDeath | Critical | 16 | 0.12 | No | 38m 2D; owner life recovery local | body_fall_0 / Kenney, body_fall_1 / Kenney | Runtime/Presentation/AudioActor.cs |
| StepGrass | Footstep | 180 | 0.055 | No | 11m 2D; owner life recovery local | step_grass_0 / Kenney, step_grass_1 / Kenney, step_grass_2 / Kenney, step_grass_3 / Kenney | Runtime/Presentation/AudioActor.cs, Runtime/Presentation/AudioLandscape.cs |
| StepStone | Footstep | 180 | 0.055 | No | 11m 2D; owner life recovery local | step_stone_0 / Kenney, step_stone_1 / Kenney, step_stone_2 / Kenney, step_stone_3 / Kenney | Runtime/Presentation/AudioLandscape.cs |
| Swing | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | swing_0 / artisticdude, swing_1 / artisticdude, swing_2 / artisticdude | Runtime/Presentation/AudioActor.cs |
| Blade | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | blade_0 / Kenney, blade_1 / Kenney | Runtime/Presentation/AudioActor.cs |
| BowDraw | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | bow_draw / Kenney | Runtime/Player/PlayerRanged.cs, Runtime/Presentation/AudioActor.cs |
| BowRelease | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | bow_release / Kenney | Runtime/Enemies/StoneProjectile.cs, Runtime/Networking/NetworkProjectile.cs, Runtime/Presentation/AudioActor.cs |
| Crossbow | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | crossbow / Kenney | Runtime/Enemies/StoneProjectile.cs, Runtime/Networking/NetworkProjectile.cs, Runtime/Presentation/AudioActor.cs |
| ProjectileStone | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | stone_hit_0 / Kenney, stone_hit_1 / Kenney, stone_hit_2 / Kenney | Runtime/Enemies/StoneProjectile.cs |
| WoodHit | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | wood_hit_0 / Kenney, wood_hit_1 / Kenney, wood_hit_2 / Kenney | Runtime/Presentation/AudioActor.cs |
| WoodBreak | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | wood_break_0 / Kenney, wood_break_1 / Kenney, wood_break_2 / Kenney | Runtime/Presentation/AudioActor.cs |
| StoneHit | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | stone_hit_0 / Kenney, stone_hit_1 / Kenney, stone_hit_2 / Kenney | Runtime/Presentation/AudioActor.cs |
| StoneBreak | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | stone_hit_0 / Kenney, stone_hit_1 / Kenney, stone_hit_2 / Kenney | Runtime/Presentation/AudioActor.cs |
| OreHit | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | ore_hit_0 / Kenney, ore_hit_1 / Kenney, ore_hit_2 / Kenney | Runtime/Presentation/AudioActor.cs |
| OreBreak | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | ore_break_0 / Kenney, ore_break_1 / Kenney | Runtime/Presentation/AudioActor.cs |
| Plant | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | plant_0 / Kenney, plant_1 / Kenney, plant_2 / Kenney | Runtime/Presentation/AudioActor.cs |
| FleshHit | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | flesh_hit_0 / Kenney, flesh_hit_1 / Kenney, flesh_hit_2 / Kenney | Runtime/Presentation/AudioActor.cs |
| BoneHit | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | bone_hit_0 / Kenney, bone_hit_1 / Kenney, bone_hit_2 / Kenney | Runtime/Presentation/AudioActor.cs |
| ChitinHit | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | chitin_hit_0 / Kenney, chitin_hit_1 / Kenney, chitin_hit_2 / Kenney | Runtime/Presentation/AudioActor.cs |
| BodyFall | Impact | 80 | 0.045 | No | 22m 2D; owner life recovery local | body_fall_0 / Kenney, body_fall_1 / Kenney | Runtime/Presentation/AudioActor.cs |
| Undead | Creature | 140 | 0.35 | No | 18m 2D; owner life recovery local | undead_0 / artisticdude, undead_1 / artisticdude, undead_2 / artisticdude | Runtime/Presentation/AudioActor.cs |
| Goblin | Creature | 140 | 0.35 | No | 18m 2D; owner life recovery local | goblin_0 / artisticdude, goblin_1 / artisticdude, goblin_2 / artisticdude | Runtime/Presentation/AudioActor.cs |
| Slime | Creature | 140 | 0.35 | No | 18m 2D; owner life recovery local | slime_0 / artisticdude, slime_1 / artisticdude, slime_2 / artisticdude | Runtime/Presentation/AudioActor.cs |
| Beast | Creature | 140 | 0.35 | No | 18m 2D; owner life recovery local | beast / pauliuw | Runtime/Presentation/AudioActor.cs |
| Wolf | Creature | 140 | 0.35 | No | 18m 2D; owner life recovery local | wolf / CaveboyTup | Runtime/Presentation/AudioActor.cs |
| Bat | Creature | 140 | 0.35 | No | 18m 2D; owner life recovery local | bat_0 / artisticdude, bat_1 / artisticdude | Runtime/Presentation/AudioActor.cs |
| Insect | Creature | 140 | 0.35 | No | 18m 2D; owner life recovery local | insect_0 / artisticdude, insect_1 / artisticdude, insect_2 / artisticdude | Runtime/Presentation/AudioActor.cs |
| Bird | Creature | 140 | 8 | No | 18m 2D; owner life recovery local | bird / syncopika | Runtime/Presentation/AudioLandscape.cs |
| BossIntro | Critical | 8 | 0.12 | No | 38m 2D; owner life recovery local | boss_roar / artisticdude | Runtime/Presentation/AudioActor.cs |
| BossPhase | Critical | 8 | 0.12 | No | 38m 2D; owner life recovery local | ring_reward / artisticdude | Runtime/Presentation/AudioActor.cs |
| BossWarningFan | Critical | 8 | 0.12 | No | 38m 2D; owner life recovery local | bow_draw / Kenney | Runtime/Presentation/AudioActor.cs |
| BossWarningLine | Critical | 8 | 0.12 | No | 38m 2D; owner life recovery local | engineer / Kenney | Runtime/Presentation/AudioActor.cs |
| BossWarningCircle | Critical | 8 | 0.12 | No | 38m 2D; owner life recovery local | boss_roar / artisticdude | Runtime/Presentation/AudioActor.cs |
| BossStrike | Critical | 8 | 0.12 | No | 38m 2D; owner life recovery local | heavy_hit_0 / Kenney, heavy_hit_1 / Kenney | Runtime/Presentation/AudioActor.cs |
| BossSlam | Critical | 8 | 0.12 | No | 38m 2D; owner life recovery local | wood_break_0 / Kenney, wood_break_1 / Kenney, wood_break_2 / Kenney | Runtime/Presentation/AudioActor.cs |
| BossCharge | Critical | 8 | 0.12 | No | 38m 2D; owner life recovery local | heavy_hit_0 / Kenney, heavy_hit_1 / Kenney | Runtime/Presentation/AudioActor.cs |
| BossSummon | Critical | 8 | 0.12 | No | 38m 2D; owner life recovery local | boss_summon / artisticdude | Runtime/Presentation/AudioActor.cs |
| BossHurt | Critical | 8 | 0.12 | No | 38m 2D; owner life recovery local | bone_hit_0 / Kenney, bone_hit_1 / Kenney, bone_hit_2 / Kenney | Runtime/Presentation/AudioActor.cs |
| BossDeath | Critical | 8 | 0.12 | No | 38m 2D; owner life recovery local | boss_roar / artisticdude | Runtime/Presentation/AudioActor.cs |
| BossVictory | UI | 20 | 0.09 | No | Local UI / confirmed owner | victory / RandomMind | Runtime/Presentation/AudioLandscape.cs |
