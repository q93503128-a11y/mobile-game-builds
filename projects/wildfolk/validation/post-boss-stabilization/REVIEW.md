# Post-Boss Stabilization · 2026-10-25

## Scope and Git identity

Started by fetching remote main: **`b4bee20dae6c88b262f2c48a9425cf745b866e59`**. Local main matched and was clean. The final pre-commit fetch still matched this start SHA. Read project AGENTS, README, relevant canonical architecture/generation/persistence/class docs and the previous Boss REVIEW before editing. Work proceeded P0→P1→P2→short Boss regression on main, with no branch/PR or new content.

The final commit is the commit containing this report. Resolve its exact SHA with `git log -1 --format=%H -- projects/wildfolk/validation/post-boss-stabilization/REVIEW.md` from the repository root. The final user report and local post-push receipt `C:/UnityProjects/Wildfolk/Captures/post-boss-stabilization/completion.json` record the exact final remote/local SHA and clean status. A commit cannot contain its own SHA. Historical `validation/undead-king-boss/REVIEW.md` and its evidence were not rewritten.

## Required review index

| # | Required point | Result / evidence |
|---|---|---|
| 1 | Starting remote main | `b4bee20dae6c88b262f2c48a9425cf745b866e59`, fetched first |
| 2 | Final SHA | Containing commit; command and post-push receipt above, exact SHA in final report |
| 3 | P0 reproduction | Actual two-process baseline, Client initial spawn/late join; `p0-before/`, `lan-before-observations.txt` |
| 4 | P0 root cause | QA disabled pending network root; NGO discovered zero Host behaviours versus one Client behaviour |
| 5 | P0 correction | Remove invalid Editor fixture deactivation; no packet/buffer/runtime authority change |
| 6 | P0 regression | Five wire/owner tests; exact observer snapshots across harvest/chunk/reconnect/reHost |
| 7 | Failed seed | `-324239329`; baseline set also exposed `54321`, `1671528757` |
| 8 | Generation root cause | Tin rule6 cluster y480 had zero valid local candidates |
| 9 | Generation correction | Habitat-aware river fallback, at most512 candidates, only after original search exhausted |
| 10 | Seed regression | 36/36 deterministic double generation; all33 previously valid fingerprints unchanged |
| 11 | Class XP failure window | World reward committed before separate profile XP; crash could permanently lose ClassXP |
| 12 | Persistence correction | One pending profile intent→world resolved receipt→atomic profile XP+clear; recover on read |
| 13 | Fault/recovery | Ten durability cases including A–E boundaries passed; no OS kill claimed |
| 14 | Save versions | Save14 unchanged; Profile1→2 migration for PendingXP |
| 15 | Protocol | Protocol21 unchanged; generation3 unchanged |
| 16 | Boss regression | Five Play tests plus actual MCP lifecycle; stats unchanged |
| 17 | LAN regression | Real two-process localhost, Host Continue alone then peer Join; no migration |
| 18 | Automated tests | Latest87/87 Edit; separate36 seed cases in41/41 suite;5/5 Play; source1476GUID/data0 errors |
| 19 | Actual Unity | Failed-seed world/save/tomb,1920×1080 inspection, Boss/ClassXP/save, actual LAN |
| 20 | Performance | No buffer increase/frame scans;512 bounded fallback; one bounded profile intent; details below |
| 21 | Files | Exact source/doc list below plus this validation directory |
| 22 | Known issues | UnityEditor.Search startup indexing exception; missing-world intent waits for proof |
| 23 | Unverified | Android/Relay/physical4devices/manual mobile/OS crash/long performance soak |

## P0 — confirmed resource replication root cause

Baseline actual localhost LAN reproduced `OverflowException: Reading past the end of the buffer`, then `[NetworkStone(Clone)][Deserialize][NetworkBehaviourSynchronization][Size mismatch] Expected: 59 Currently At: 58!` on Client resource spawn/late join. The baseline flow could finish despite malformed actors; successful exit alone was insufficient. Compact original evidence is retained in `p0-before/` and `lan-before-observations.txt`; full local logs are ignored by Git.

`BossLanQa.NewHost` called `Freeze` while freshly created resource actors could remain in `NetworkCreatureWorld.pending`. The old helper used `GameObject.SetActive(false)` for nearby resource roots without network retirement. Actual Host diagnostic: `NetworkStone(Clone) spawned=True behaviours=0 id=`.

NGO `NetworkObject.InitializeChildNetworkBehaviours` enumerates child behaviours including inactive objects but associates them using `behaviour.GetComponentInParent<NetworkObject>()`, whose default lookup excludes inactive parents. A live Editor probe found the owner when active, no owner when inactive, and one actual NetworkResource component in both cases. The disabled pending root therefore produced zero wire behaviours on Host; the active Client prefab had one. Host sent only the synchronization-count byte; Client read absent ResourceNetworkState data starting with FixedString64Bytes. This explains the one-byte discrepancy. Capacity, sequence rollover and packet loss were not the cause.

Removed the unnecessary root deactivation in the Editor-only LAN fixture. Normal runtime streaming already unloads via ResourceRegeneration.UnloadChunk→PrefabPool.Release→NetworkObject.Despawn before retiring presentation roots; network resource creation does not parent actors to those roots. No runtime resource serialization, batch size, capacity, spawn/delta authority or logical IDs changed.

New ResourceReplicationQa compares the entire actual Client spawned-resource set to Host observer manifests: network ID, unique logical ID, rule, HP, active roots, no missing/stale/duplicate replicas. It exercises real ResourceNode.Harvest damage, death/drop/despawn, two chunk leave/return cycles, disconnect/rejoin and save/reHost. Latest run `730885b003e742a2b86c15def6f8f052`: initial476; rich502; after harvest501; late join501; reHost414, all exact matches. Different observer counts after movement are expected. Harvest tombstone persisted through both chunk cycles and reHost. See `lan-final/resources-{host,client}.tsv`.

Five Edit tests cover the inactive-owner mechanism and exact wire roundtrip for empty/short/chunk/POI IDs. Initial assertions mistakenly expected12 fixed bytes; package source and actual serialization established **14+UTF8 ID bytes** (uint string length4 + rule4 + HP4 + hits2). The initial1/5 failure is retained as `p0-model-initial.xml`; the corrected five pass in generation-after and final-model. Three fixed actual LAN runs had no resource overflow/size mismatch; the latest compact log is `lan-rejoin-observations.txt`.

## P1 — bounded generation repair

Original exception: `Deterministic source repair exhausted for rule 6`, seed `-324239329`. Rule6 is Tin Ore, Panning/RiverGravel. The original algorithm tried1200 golden-angle candidates in a disk about78units around each of three channel centers at y0/240/480. Actual diagnostic allowed counts:203/205/**0**. The third disk contained no valid habitat. Retrying the same disk more often would not repair this seed. See `seed-before.txt` and `seed-diagnostic.txt`.

WorldProgressionSupply.TryRiverFallback runs only for exhausted panning searches. It follows the same channel with128 longitudinal positions at32unit alternating offsets and four banks(±8/±12), at most512 candidate checks. It preserves SourceAllowed terrain/water/road/tags,4unit spacing, and natural-source collision checks where the original Supplement/Deep stage used them. A temporary per-call chunk cache avoids repeated natural generation. Build, Supplement and DeepCavernContent surface panning use this one helper. No RNG reseeding, terrain-tag relaxation, unbounded retries, global flattening, saved-world migration or generation version change.

Fixed set: default9472026, failing seed, int min/max,0,±1,42,12345,54321 and26 reproducible random seeds. Baseline33/36; failures `-324239329`, `54321`, `1671528757` all rule6. Corrected36/36; each generated twice with matching fingerprints of anchors/caves/roads/guaranteed sources and sampled natural resources. Full canonical generation validator checks progression supply/reachability/caves/deep; additional tests cover spawn, required POI/biomes, finite/bounded coordinates and critical anchor-center overlaps. **All33 previously passing fingerprints remain identical**; this is a defined fingerprint comparison, not a claim of exhaustive equality of every possible chunk. See before/after TSV and XML.

Actual MCP constructed the failing seed with full invariants (~7116ms single observed run), entered its Gameplay world (321 active resources), saved Save14, and materialized Graveyard `poi4:-8` with RoyalTomb. `seed-world-1920.png` was visually inspected at1920×1080; terrain, tomb/guard and HUD were normal. Existing valid world coordinates are not relocated.

## P2 — local Class XP durability

Old order committed physical loot/commonXP/Gold/resolved Boss to Save14, then called PlayerClass.GrantXP into a separate profile. A stop between the two left no durable proof of pending ClassXP.

Chosen bounded local journal reuses profile atomic replace/checksum/backup and existing Save14 resolved Boss records:

1. Atomic Profile2 PendingXP intent records original Class, XP, world path/seed/generation, Boss/tomb/night/receipt. XP is not incremented yet.
2. Existing world transaction commits physical loot, commonXP/Gold and resolved Boss together. Failure rolls all back and cancels the intent.
3. Confirmed world proof allows one atomic profile XP increment **and intent removal together**. Failed profile write leaves durable intent.
4. LocalAccountProgressionStore.Read reconciles only outstanding intent. Valid matching resolved world→credit original Class and clear; valid uncommitted world→clear without XP; unreadable/missing world or failed profile write→retain for retry. Existing save/pause/quit boundaries also retry pending recovery. New-night summon cannot overwrite an unresolved local receipt.

One intent per profile, no permanent per-kill list. In-flight guard prevents reentrant profile reads cancelling a transaction while the world commit is underway. Copy-before-mutation keeps failed backend writes from mutating reference-returning store state. Session-only Host verifiers still receive memory-only XP. Boss reward write failure retries at most once/second. Normal attack/loot timing and balance remain unchanged.

Profile1 checksum is checked before migration; owned/selected/XP/nodes/purchase Pending survive, PendingXP starts null, and a plain read does not rewrite an otherwise healthy legacy profile. Next normal write stores Profile2. Save14/Protocol21/generation3 and Item136/Recipe111 are unchanged.

### Controlled fault boundaries

Tests use isolated temporary real world/profile files, canonical validation, atomic backend and fresh store instances to represent restart. They are fault injection, **not OS process-kill/crash tests**. Intent records promised XP before applying the actual XP change, closing the old window by construction.

| Boundary | Durable state at interruption | Reload result |
|---|---|---|
| A before change/intent | Old profile1800, old world |1800; no intent |
| B intent saved, world not committed | Profile intent, old world |1800; intent cancelled |
| C world committed | Profile intent, world commonXP120 |1920; intent cleared |
| D profile completion write fails | Intent retained, committed world |1920 on successful retry; once |
| E both commits finished | Profile1920, resolved world |1920 across repeated complete/read |

All five preserve selection, owned Survivor and nodeA1. Extra tests cover intent-write refusal before world commit, world rollback, profile completion retry, valid backup recovery and session-only no profile write: ten cases total. Existing genuine Profile1 fixture preserves Hunter ownership/selection/XP1800/A1/A2 through migration. Class/common XP remain separate. Normal MCP lifecycle finished with runtime/profile ClassXP127 (guardian7+Boss120), no pending intent; pause(true)/quit save hooks were invoked under isolated QA, Save14 retained. Those hook calls do not claim a real OS background/quit event.

### Related session-only leak corrected

Actual LAN exposed existing NetworkClassActions.ProgressRpc writing an ephemeral Host verifier result into Client profile: fixture ExplorerXP1800 became1927. Evidence retained in `preexisting-client-reward-evidence.json`. ProgressRpc now updates transient PlayerClass replica XP only; Host sends its session snapshot on network spawn as well as reward changes, so reconnect receives current XP immediately. Server ownership/RPC authority and wire signatures stay unchanged. Latest actual LAN proves ClassXP+120 once, Host Profile2 with no pending intent, Client profile bytes unchanged, Explorer identity and initial/reconnected XP snapshot. This correction is within the requested Class/session-only regression.

## P3 — Boss, save and actual LAN regression

Existing three BossProductionPlayTests plus Class lobby/session-lock and shared-growth/Profile migration tests: **5/5**. Separate actual MCP BossProductionQa lifecycle checked guardian seal/unlock; Bone10/night/same-night rejection; one Boss;70/35 phases; Skull Mark persistence/clear; solo2adds; Returning grace/gradual regen/re-entry; active fight across day; active/pending/resolved saves; Core and forced-Crown physical set; no duplicate reward on load; next-night summon. Forced Crown verifies its transaction path, not empirical20% distribution. See `actual-mcp-boss/checks.tsv`.

Latest actual LAN used two independent Unity processes over localhost, run ID above, complete with no resource buffer error. Checks additionally include2P three adds and unscaled HP/speed, same telegraph direction/timestamp, Client damage/Returning authority rejection, party wipe/mark transfer/Home respawn, accessory RPC/visual, one Core and at-most-one Crown, each eligible commonXP120/Gold35 once, persistent/transient ClassXP boundaries, **Client owner Core pickup acknowledged by Host with exactly one Core**, Host Save14, Host Continue alone then later Client Join, no duplicate physical loot, consumed night preserved and same-night offering rejected without consuming Bone. No Host migration. Full final actual assertions: `lan-final/host.tsv`, `client.tsv`, resource TSVs and `complete.txt`.

Boss assets/scenes/data are unchanged: HP9000; damage26/41/21/26/36; windup0.8/1.15/0.75/0.95/1.3sec. Previous Steel4:22.5/Diamond3:37.2 combat timings are historical values, not remeasured in this stabilization pass.

## Automated results and evidence

| Run | Result | Test duration | Meaning |
|---|---|---|---|
| generation-before |33/36 |138.582s | Original3 failures retained |
| p0-model-initial |1/5 |0.257s | Initial wrong12-byte expectation retained |
| generation-after |41/41 |142.859s |36 seed checks +5 resource cases |
| persistence-model |62/62 |18.716s | P2 and related model regressions |
| core-play |5/5 |96.727s | Boss/Class PlayMode |
| final-model |87/87 |16.804s | Final persistence/Class/Boss/network/resource/scene/character/inventory selection |

Suites overlap; counts are not summed as unique tests. XML plus `test-summary.json` records exact tests/timestamps; `*-process.json` records outer Unity startup/exit wall time separately. Source validator passed1476GUID and final live ProjectDataValidator returned zero errors. Latest final-model and LAN include the final Client snapshot change. No unrelated full-suite rerun claimed.

## Performance and remaining limits

- Resource packet layout remains14+UTF8 ID bytes; no giant buffer, permanent capacity increase or new runtime resource-list scan. Full observer scans are Editor-only explicit QA.
- Healthy original candidate paths remain unchanged; only exhausted panning clusters pay at most512 additional candidates. Temporary per-call chunk cache is bounded by that search.36 seeds and unchanged33 fingerprints support regression behavior, not a worst-case proof for every integer seed.
- Profile journal costs one extra small atomic intent write per local Boss reward before the former completion write. Only one pending object is retained. No per-frame file poll or new UI rebuild loop; retry at save/read boundaries, deferred Boss writes capped1Hz. Normal profile reads with no intent do not read world files.
- Latest LAN outer process73.068s; final-model45.638s; core-play127.453s. These are execution records, not isolated before/after CPU/GC/startup benchmarks. No long soak or mobile performance claim.
- Client batch startup still logs **UnityEditor.Search.SearchDatabase ArgumentOutOfRangeException**, an Editor search-index issue distinct from the resource error. It was present in baseline/fixed runs and did not prevent the complete assertions; retained in compact observations. Host logs one deliberate offering-save IOException from fault injection; expected, with rollback verified.
- Missing/unreadable world retains its intent until proof is available. Already-lost legacy Profile1 rewards cannot be reconstructed retroactively without an intent. Arbitrary manual world backup rollback, independent media loss, malicious account edits and account/cloud exactly-once are not solved by local two-file reconciliation.
- Android build/device, Relay, physical4devices, manual mobile controls, actual OS kill/power loss, exhaustive seed space and long performance/GC soak remain unverified. Actual LAN was two localhost Unity processes, not those environments.

## Files and protection

Changed existing source (all under `client/Assets/Wildfolk/`):

- `Editor/BossLanQa.cs`, `Editor/ClassFoundationQa.cs` (latter fixes stale Save13 assertions to14).
- `Runtime/Bosses/BossEncounter.cs`, `BossWorld.cs`.
- `Runtime/Generation/WorldProgressionSupply.cs`, `DeepCavernContent.cs`.
- `Runtime/Networking/NetworkClassActions.cs`, `NetworkWorldPersistence.cs`.
- `Runtime/Persistence/ClassProfiles.cs`, `ClassProgressSession.cs`, `WorldSaveService.cs`.
- `Runtime/Player/PlayerClass.cs`, `Tests/EditMode/ClassCorrectionTests.cs`.

New source and their Unity metadata:

- `Editor/ResourceReplicationQa.cs`.
- `Runtime/Persistence/ClassRewardPersistence.cs`.
- `Tests/EditMode/ClassRewardDurabilityTests.cs`, `GenerationStabilizationTests.cs`, `ResourceReplicationRegressionTests.cs`.

Canonical docs: README plus ARCHITECTURE, SAVE_PERSISTENCE, WORLD_GENERATION, CHARACTER_PROGRESSION. This validation directory contains compact evidence/XML/screenshot/timing; full logs remain local ignored artifacts. No new material/recipe/station/boss/art/scene changes.

Only isolated QA stores were used. Original UserData/profile JSON SHA256 **and mtime** match before/after (`user-save-before.json`, `user-save-after.json`). Live Editor stopped at MainMenu, QA directory SessionState cleared, original console settings restored. No Windows input/Focus/Show automation was used.

## Timing and usage

UTC boundaries, adjacent non-overlapping wall intervals and token availability are in `timing.json`; stage end includes tool execution/waits and report preparation. Process wall time is in each `*-process.json`, overlapping suite/tool waits are not added again. Per-operation token consumption is unavailable and recorded **null**, never estimated as measured usage. Final post-push receipt separately records Git completion. Stop after this pass; no Mount Backend started.
