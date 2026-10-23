# Boss Foundation / Undead King — 2026-10-23

Start remote main: `82084f79bcf8cce11454b21ef67e79a3174bb82d`, fetched before inspecting source; clean main. Production implementation/verification follows this audit. Final SHA is recorded after commit in the completion record, avoiding a self-referential SHA.

## Public boss references, checked before implementation

|Game / actual source|Specific case|Lowfire principle|Alternative rejected and reason|
|---|---|---|---|
|[Terraria official wiki: Skeletron](https://terraria.wiki.gg/wiki/Skeletron)|Old Man night interaction, once per night until next dawn; separate skull/hands and visible spin posture; at dawn extreme enrage|Persist a world night identity when offering succeeds. Distinct body/weapon preparation makes attacks readable|No dawn enrage/despawn, independently killable limbs or platform movement. Lowfire fight can continue into day|
|[Valheim community wiki: Bonemass](https://valheim.fandom.com/wiki/Bonemass), [boss strategies](https://valheim.fandom.com/wiki/Boss_strategies)|Offering10 Withered Bones at altar. Backward lean warns poison area; slow armpit throw exposes summon window. Four adds per throw, per-type caps; altar-height cheese documented|Short bone offering; separate ritual and heavy attacks; cap live boss adds; prevent AFK attacks outside encounter during return|No poison-cloud spam, repeated altar-height safe damage or copied model. Lowfire allows only2–5 living summoned adds, ordinary stats fixed across player counts|
|[Core Keeper community wiki: Hive Mother](https://corekeeper.atma.gg/en/The_Hive_Mother)|Player can prepare before first hit; marked acid landing circle; eggs produce no-loot adds;60% enrage;20-tile/10-second loss resets full health; tough summoned larva can break walls|Readable ground bounds, preparation remains useful, summoned adds have separate ownership/cleanup and no farm rewards; attack blocking structures|No instant full heal, stationary acid field or invulnerable structure maze. Lowfire walks home and regenerates gradually|

These are source-documented mechanics, not frame measurements of videos. Search-index excerpts were readable for Terraria/Valheim; direct fetch returned403/402, respectively. Core Keeper's full page was read. No externally authored code/art entered the project. Emergence3.6s, ritual1.5s and death/loot delay are Lowfire authored starting values, not claimed timings of those games. The sources establish attack preparation, bounded adds and staged rewards; camera/control choices remain deliberately minimal on mobile.

## Current source audit / numerical baseline

Health100 base; armor is flat subtraction with minimum1, then class mitigation. Steel Head3+Body5=8; Diamond Head/Body improve that. SteelSword43/0.38=113.16 theoretical DPS, Diamond52/0.38=136.84. Duration0.28 overlaps cooldown; do not incorrectly add both intervals. Dash2.6/0.18, cost24/cooldown0.55, **no invulnerability**. Walk4.2, sprint×1.55. Healing salve30 HP/common4s, bandage20/8s, common medicine modifiers and existing meal buffs remain. Common vitality can add100HP; no boss scaling to invested HP/classes.

Initial HP budget9000: Steel35% actual hit uptime≈39.6DPS→227s before rituals/repositioning; Diamond same uptime≈47.9DPS→188s. This is an arithmetic estimate, not a measured clear. Damage at100HP/Steel8: cleave26→18%, execution41→33%, march21→13%, grave26→18%, crown36→28%. Vitality/meals improve margin, no one-hit kill at unarmored100HP. Phase3 keeps windups, uses movement×1.05/recovery×0.9. Final telemetry/play results and ranged/turret/economy audit are appended after verification.

## Chosen shared boundaries

BossDefinition/attack data owns stats and schedules; BossEncounter owns lifecycle/phase/territory/scheduler/minions/reward handoff; segmented UndeadKingVisual only owns presentation. BossWorld owns per-boss last-night identity and pending reward save transaction; existing Graveyard guard-cleared feature is the permanent tomb unlock. NetworkBoss owns compact server state and timestamp-driven replica; existing server NetworkTransform moves the actor. Existing Health, placement damage, pickup/stack codec, equipment exchange and Host inventory request validation remain the shared paths.

Save schema must extend for accessory and encounter state; migrate13 normally, preserve all existing fields and version readers. Active combat restores as Returning with saved HP; pose/targets/adds are transient. Dying saves retain the already-rolled pending reward and replay the short death; common XP/Gold, shared pickups and resolved encounter flag commit in one world snapshot. Class XP is delivered after that world commit (no duplicate world loot or common rewards). Physical loot is one shared set. Failed writes must roll back provisional common rewards/drops and retry without rerolling. Per-boss night identity is world-wide, not one summon per Graveyard.

No fake Ads/Gem grant, IV recipe, extra boss or unrelated system redesign. Exact implementation, tests, actual Play and LAN evidence will be recorded below; unexecuted checks are not claimed.

## Requested checkpoint — not final completion

The user requested a pause after the current verification stage. See [CONTINUE](CONTINUE.md) and [checkpoint](checkpoint.json). Related EditMode42/42 and PlayMode2/2 passed; source validator passed and data validator returned0 errors. Actual controlled MCP lifecycle and visual captures ran; this does not establish a natural Steel clear or actual LAN success. The two-process LAN fixture is authored but unexecuted. No commit/push yet; canonical source changes are preserved on main. Current documentation consolidation, combat balance measurement, real LAN and final audit remain.

## Second requested checkpoint (resumed verification)

The user requested another pause after the running Steel measurement. This remains an uncommitted production pass. See [latest checkpoint](checkpoint-latest.json), [updated continuation](CONTINUE.md), and [resumed evidence](resumed-verification/).

Actual two-process LAN completed offering/single spawn/3adds/mark transfer/full wipe/Returning/regen/re-entry/accessory/shared physical rewards/per-participant XP-Gold/save/reHost. Pre-spawn boss clock used the wrong epoch initially and is now corrected and asserted. One ResourceNetworkState buffer read anomaly occurred in the completed Client run; its cause remains unconfirmed and must not be reported as a clean-console run. The new exact telegraph timestamp handshake is authored but has not run yet.

EditMode42/42 and PlayMode3/3 passed, including offering/reward failed-write rollback and pending-death reload. Actual controlled defense checks passed for real trap/projectile damage, one ammunition per shot, LOS, return immunity and blocking wall destruction. Language scene/locale cycle3 passed and the visible Class menu was inspected. Gameplay measurements use ordinary HP/armor/weapons, finite salves and a scripted input bot at TimeScale2; they are not physical mobile/manual feel tests. First Diamond attempt stalled at the tomb corner; boss and bot local avoidance were corrected before the successful measured trial.

Current implementation and documentation are saved; final authority/reward audit, exact LAN telegraph check, final REVIEW and Git commit/push remain. Original user save hash and mtime remain unchanged. Class-profile XP cross-store crash loss, actual Relay/Android/4-device/manual touch and long-term balance remain explicit limits.

## Final production record

The dated checkpoint paragraphs above describe their respective earlier state. This section supersedes their outstanding-work lists. Start main and both resumed fetches were `82084f79bcf8cce11454b21ef67e79a3174bb82d`. The final commit containing this record is identified by `git log -1 --format=%H -- projects/wildfolk/validation/undead-king-boss/REVIEW.md`; its full SHA and remote equality are reported after push in the completion message. Embedding the commit's own SHA in itself is impossible.

### Required design and verification record

|#|Topic|Implemented result / evidence|
|---|---|---|
|1|Starting remote main|`82084f79bcf8cce11454b21ef67e79a3174bb82d`; fetched before source audit and again on both resumes|
|2|Ending SHA|Commit that adds this final record; full local/remote SHA in completion message, verified after push|
|3–4|References / adopted principles|Three game comparison above. Night offering identity, visible weapon/body preparation, limited no-loot waves, staged death/reward. No copied assets/code, long cutscene, dawn enrage, full reset heal or permanent hazard carpet|
|5|Reusable foundation|BossDefinition+BossAttack data; BossEncounter lifecycle/target/phase/scheduler; BossWorld offering/night/save/reward transaction; BossMinion ownership/reset; NetworkBoss and NetworkBossWorld authority/replica. Existing Health/navigation/structure damage/POI/pickup/equipment/save remain shared|
|6|Phases|100→70% basic three attacks;70→35% ritual + Grave Cleave +2–5 Skeletons;≤35% cracked crown, Crownfall, bounded two-attack chain, speed×1.05/recovery×0.9, no new wave. Existing wave/mark persists|
|7|Telegraphs|Royal .80s / Execution1.15s / March .75s each of3 / Grave .95s / Crownfall1.30s. Recovery .70/1.20/1.30/.90/1.50s; last phase does not shorten windup. Body/sword poses and low contrast fan/line/circle bounds. Actual LAN checks matching attack index, server timestamp, locked direction and enabled Edge|
|8|HP/damage basis|9000HP, speed2.1; raw26/41/21/26/36.100HP + Steel8 armor→18/33/13/18/28. Diamond10 armor→16/31/11/16/26. Biggest raw hit41 cannot one-shot unarmored100HP. Existing class/armor then Crown modifiers; no boss stat scaling by class/growth|
|9|Steel/Diamond estimate and measurement|At35% uptime, arithmetic227/188s before ritual/reposition. Final ordinary-stat input-bot clear262.4973/217.2021 game seconds;6/4salves, ending65/88HP,10/17hits. TimeScale2, no automatic heal/forced boss HP/teleport during the measured fight. Not manual/mobile feel validation|
|10|Multiplayer scaling|HP/damage/speed fixed1–4P. Wave1/2/3/4living→2/3/4/5adds. Actual1P and2P verified;3/4P configuration/model assertions, not real devices|
|11–12|Bone10 / night once|Existing Bone10. Live session, living actor, proximity, non-scout, guardian clear, current night identity and bag revision validated. World boss ID last-night checkpoint prevents duplicate summon across tombs or death/reHost. Next-night test passes. Failed write restores Bone/record/actor and publishes prior state|
|13|Royal Tomb|Existing Graveyard anchor and one guardian feature; permanent unlock. Larger authored sarcophagus/lid. Local radius10 timber/stone exclusion, retaining minerals and player structures. Emergence3.6s; no player control lock; awakening damage blocked|
|14|Skull Mark|One stable head icon, all replicas. Wave clear removes it. Downed/dead/out-of-territory/disconnected target is invalidated; new living target selected. Adds movement×1.11 only, no attack/damage buff and no expiry timer. Actual Downed transfer verified; active-mark disconnect remains code-audited only|
|15|Returning / regeneration|Territory23, grace4s, walk home×.78, bounded obstacle sidesteps,2s delay then90HP/s. Outside damage blocked, cannot cancel return. Living territory re-entry resumes. Actual SP/LAN abandon/wipe/re-entry and gradual heal checked; fight continues into day|
|16|Natural spawn suppression|New random night undead only inside active territory; no world night switch and no deletion of existing natural actors. Boss adds transient, no natural population registration, zero loot/XP/Gold; death/return cleanup and pool restoration checked|
|17|Core T1|One shared physical guaranteed stack, no new recipe/IV gate. Stored for future progression; not multiplied per player|
|18–19|Crown / Accessory|One new slot(index3), inventory exchange/unequip/drag/save/owner RPC/appearance. Crown20% one shared roll, HP≤35% move+8% and incoming×.9 once after armor/class, offense0. Whole small floating player crown; broken boss crown phase3. Generic AboveHead/BodySide/Waist/AroundActor anchor and bounded .16unit follow lag|
|20|Loot semantics|Shared Core1/Bone14–20/Iron2–4/Steel1–2, Crown0–1 at20%; each living connected eligible participant within24 getsXP120/Gold35 once. Roll locked at death; dying reload retains exact loot/eligibility; physical pickups+common XP/Gold+resolved flag one world commit, failed writes rollback/retry. No reward re-roll/duplicate on resolved reload/reHost|
|21|Day/Night HUD|Existing localized HUD now DayN/current phase, state/locale changes only text refresh,1.6s brief color emphasis; no new full screen prompt or per-frame formatted string|
|22|Save migration|Save13→14 accessory null/boss list empty; earlier supported1–12 readers preserved. Active saved boss→Returning with savedHP; target/pose/adds/mark transient. Pending death saves retain roll. Checksum/atomic replace/backup, filenames/generation unchanged; original user save SHA+mtime unchanged|
|23|Protocol|20→21 for BossFrame/offering/POI unlock/accessory appearance. Profile1 and generation3 unchanged. Item134→136, Recipe111 unchanged|
|24|Automated|Related EditMode42/42; final PlayMode3/3, including failed offering/reward write rollback, exact pending-death reload, same/next night, active/resolved reload, pooled minion restore and accessory threshold/exchange. Source validator1471GUID, data validator0, diff whitespace check. XML and final-verification/test-summary.json|
|25|Actual Unity|Isolated MCP lifecycle/equipment/defense/language checks, controlled visual sequence1920×1080 and1280×720. Final changed mark height, cleave pose and dim Returning bar inspected. Real weapon/input bot Steel/Diamond clears separately recorded. Language issue did not freshly reproduce; localized-label unsubscribe/dead-object defense and3scene/locale cycles passed, active Class label checked|
|26|Actual multiplayer|Two separate Unity processes over localhost LAN. Owner offering RPC/Bone10/single spawn/client mutation rejection, exact windup sample,3adds/mark transfer/full wipe/regen/Home respawn/re-entry, accessory equip replica, one shared core/each eligible reward, Host save/solo reHost/peer rejoin/no duplicate. Added actual failed offering mirror and resolved same-night no-cost checks. Final evidence in final-verification/actual-lan|
|27|Not verified|Actual Relay/Android/physical4-device/manual thumb input/fun/long-term economy/performance stress/active-mark disconnect;3/4P are model counts only. BGM/SFX content remains absent|
|28|Known limits|ResourceNetworkState buffer read/NetworkStone size mismatch observed in LAN; cause unconfirmed, source unchanged. Editor Search indexing exception may also appear in batch startup. Random generation seed-324239329 fails rule6 supply repair in both134clone and136catalog(generation-comparison.txt). Class-profile XP is delivered after world commit: process crash in between can lose Class XP; cross-store atomicity not claimed|

### Combat, preparation and reward economy

|System|Actual source values / calculation|Decision|
|---|---|---|
|Melee uptime|Steel43/.38=113.16DPS theoretical, Diamond52/.38=136.84.9000HP/(.35×DPS)=227.2/187.9s. Attack .28duration overlaps cooldown|Final input-bot4:22.5/3:37.2 is within a multi-minute solo fight; invested growth/classes/meals/co-op can shorten it. Bot reads encounter state and is not a claim about human hit rate|
|Healing|Salve30HP/common4s, bandage20HP/8s, default100HP. Steel/Diamond used6/4salves=180/120potentialHP, finite12in bag. Existing Survivor innate lethal survival can affect results even with0nodes; no invented invulnerability|Medicine remains meaningful. Measured costs6/4Gland +6/4Herb +12/8Fiber using existing one-salve recipe; no new consumable or economy rebalance|
|Dash / class|Dash2.6unit/.18s,24stamina/.55s cooldown, no i-frames. Common maximum vitality+100HP. Class mitigation/outgoing/medicine/meal paths reused; no class-ID branch in boss|No boss adaptation to build, permanent stats or party size. Actual LAN uses Explorer0nodes to avoid Survivor lethal-prevention invalidating deliberate Downed fixture|
|Ranged|Existing arrow/bolt damage and finite ammunition are unchanged. Boss can be kited in territory; leaving it long enough engages return immunity/regen|No projectile immunity or artificial resistance. Ranged-only timed clear not measured; no claim of full ranged balance|
|Turrets|Ballista62/2.3=26.96DPS, Repeater16/.55=29.09DPS ideal unmodified; at least146/563successful shots for9000HP alone. Existing aim/reload modules, LOS, turning and real1ammo per shot remain|Preparation deals real damage; target shooting structures within12unit and blocking walls through existing Health with×3structure damage. No turret damage deletion. Outside player territory cannot AFK cancel return; controlled actual damage/LOS/ammo/wall tests passed. Arbitrarily large turret farms not stress-tested|
|Traps|Spike16/1.2s≈13.33ideal continuousDPS; Bone Trap32once(one target)|Real damage, no hard stun added. Controlled actual checks16/32 pass|
|Offering / materials|Bone10 upfront, on victory14–20back→net+4–10(avg7); failure costs10. Boss adds return0materials. Iron2–4/Steel1–2 supplement reward; no Diamond/Crystal/unique recipe gate|No new creature-material supply or recipe/station expansion. Reward does not make normal smelting obsolete without repeatedly winning a multi-minute once-night fight|
|Crown chance|p=.20shared per win→expected5wins; no crown after5/10wins probability.8^5=32.768%/.8^10=10.737%. At measured clear speed five wins≈21:52.5Steel or18:06Diamond combat alone; nights/travel add more|20% maintained, no pity system or per-player duplicate. No promise of guaranteed acquisition within five kills|
|Common rewards|XP120/Gold35 per eligible living connected participant, not for absent/downed/dead. Roll/reward/persistence are Host-owned|Existing progression/Gold/class XP boundaries kept. Class XP cross-store crash loss is explicitly separate from atomic world/common rewards|

### Manual code / failure audit

- Generic EnemyController only gained PreferredTarget and MovementMultiplier; UndeadKing asset/name branches are confined to authoring/presentation/QA and boss config. No parallel patch/override implementation. Boss simulation is separate from visuals.
- Phase thresholds interrupt pending attacks, clear repeat/chain cursor, and new heavy attacks cannot overlap transition/ritual. Phase3 cannot schedule a new wave. Three March slashes retain the same direction; there is no infinite pursuit combo.
- Minion cleanup cancels preferred target/speed; pooled reuse reenables ordinary AI/lifecycle/loot. Actual regression assertions pass. Natural undead are not despawned on boss return.
- Host checks offering proximity/night/bag revision/owner request sequence. Client direct Health mutation and BeginReturn rejection were observed. Final audit restores prior offering record before release/publish, and checks present connected actors again at reward roll/commit.
- Actual fault injection generated expected `Cooperative world save failed: IOException`. The first two final LAN retries timed out because Unity Error Pause suspended the Host on that deliberate log; editor state confirmed paused. Error Pause was temporarily disabled for the completed run and restored afterward. These are failed test attempts, not game success. Resource wire errors remain a separate unconfirmed issue, not the proven cause of those timeout attempts.
- Initial Diamond bot stall at tomb corner prompted shared Chase sidestep correction before final measured trials. Final visual fixture does controlled damage/healing/teleport and is intentionally excluded from balance measurements.
- No Windows input or editor focus calls; source/scripts plus existing non-focusing GameView repaint/resolution helper only. No user save used by QA.

### Changed files and scope

Full relative file list is [changed-files.txt](changed-files.txt). Main groups: new Runtime/Bosses, boss network/save/presentation/accessory types; existing combat/AI/POI/equipment/save/UI integration; boss/tomb/pickup data and prefabs/SP-MP scene wiring;15original PNGs and Pillow generator; Editor-only authoring/QA and related tests; README and10canonical docs. Item136/Recipe111/Save14/Profile1/Protocol21/generation3. No station/recipe/WorkbenchIV/Mount/extra boss/audio/commerce/network migration expansion.

### Timing and usage

Stage UTC timestamps are [timing.json](timing.json); phase durations use adjacent recorded boundaries, with user-requested pauses excluded. Tool launch/wait wall times are recorded where measured, overlap is not summed into work totals. Exact per-operation token usage is not exposed by the tools and remains null; time is not presented as token usage.
