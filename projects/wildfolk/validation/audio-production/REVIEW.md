# Launch Foundation 5 — Audio production review

Current completion evidence is in **Runtime completion pass** below; initial acquisition/checkpoint sections are historical.

Start SHA: `b1e6c7e68572ac342812c78e946f773101a6dc6c` (remote main fetched before work; local/remote equal and clean). Foundation5 final SHA: not applicable yet; incomplete. Checkpoint commit is the latest main commit touching this review.

## Evidence before design

The repository audit found **zero audio clips and zero AudioMixers** in client Assets. Existing GameAudio has two empty AudioSources and applies Master × Music/SFX from the existing persisted settings. The previous sine action.wav and its generator were deliberately removed; they are not a usable source or a fallback. Current settings retain `wildfolk.user.master/music/sfx`; isolated QA has its own in-memory preferences. Existing scenes have one camera listener each. These observations precede new production integration.

This comparison uses public developer/composer/store/wiki descriptions and track/context lists. It is **not** a claim to have measured or listened to these games' mixes, footsteps, mobile speakers, LUFS, or compressor settings. No reference-game music or effects are copied or ripped.

| Game / actual source | System | Concrete case | Benefit | Difference from Lowfire | Applied? | Reason / rejected alternative |
|---|---|---|---|---|---|---|
| [Core Keeper, official June 2022 developer announcement and composer interview](https://store.steampowered.com/news/posts/?appids=1621690&enddate=1656592072&feed=steam_community_announcements) | Region score | OST expanded from 13 to 21 pieces with Sunken Sea; Dirt, Forgotten Ruins, Clay Caves and Azeos have identities. Jonathan Geer describes the requested lush, organic palette. | Location identity without relying on UI signals. | Lowfire is a woodland survival game with a smaller current location set and no existing clips. | Selected principles; no asset copying | Adopt organic timbres and a bounded context map. Reject copying themes or matching the reference's track count; current map transitions must justify each bed. |
| [Terraria official wiki OST](https://terraria.wiki.gg/wiki/OST) | Time/biome/boss | Overworld Day, Night, Underground and Boss 1 have explicit gameplay contexts; wind ambience is separate from Windy Day music and Graveyard has its own cue. | Environment and exceptional danger are distinguishable. | Lowfire's ordinary fights are frequent and its graveyard is a local area rather than a large biome. | Selected principles; no asset copying | Adopt time/location priorities and boss override. Reject starting a battle song for every ordinary enemy; that would repeatedly interrupt exploration. |
| [Don't Starve community wiki soundtrack](https://dontstarve.wiki.gg/wiki/Soundtrack) | Work/fight segmentation | Cave Work has work and fight sections; Shipwrecked has a non-boss fight cue. Page search cache was readable; full page returned 403. | Action can change musical energy within a location. | Short Lowfire fights and mobile interruptions make rapid score changes especially noticeable. | Selected principles; no asset copying | Use sustained, confirmed encounter state and transitions; reject attack-button-driven instant music switching. No claim about the game's measured transition duration. |
| [Crashlands official Steam soundtrack](https://store.steampowered.com/app/432800/Crashlands_Soundtrack/) | Build/night/fight context | Fat Bard's list separates Build Bossa (2:43), Night Time (3:07), Fight Time (2:40), biome night remixes, and Final Delivery (0:20). | Long beds and brief result cues serve different purposes. | Its science-fiction and lively electronic identity differs from Lowfire. | Selected principles; no asset copying | Adopt the bed/stinger distinction. Reject the electronic/arcade palette and use no copyrighted soundtrack material. |
| [Stardew Valley options](https://wiki.stardewvalley.net/Options), [audio documentation](https://wiki.stardewvalley.net/Modding%3AAudio), [soundtrack](https://wiki.stardewvalley.net/Soundtrack) | Category/silence | Separate music, sound, ambience and footstep controls; cue definitions can randomly choose multiple files. Seasonal music can leave silence rather than immediately loop. | Players can manage repetitive sounds; variations and gaps reduce fatigue. | Lowfire already has a compact Master/Music/SFX settings layout. | Selected principles; no asset copying | Adopt real category routing, variations and restrained repetition. Reject adding sliders just to mirror another game; UI and ambience can remain under the existing SFX control if clearly documented. No reference mix values are treated as Lowfire loudness targets. |
| [Forager composer Hernán Marandino's original album](https://hernanmarandino.bandcamp.com/album/forager-original-game-soundtrack) | Biome score | Crafting Love (2:57), Ice Cave (5:08), Ancient Tomb (2:05), Graveyard (4:43), Boss Fight (2:06) have separate contexts. | Exploration and exceptional spaces can have distinct pacing. | Lowfire does not need Forager's constant upbeat reward density. | Selected principles; no asset copying | Adopt cave/graveyard/boss differentiation, reject reward-like sounds on every passive stat or inventory update. All-rights-reserved reference music is excluded from acquisition. |

## Source audit and integration constraints

| Existing source / event | Actual trigger or authority | Audio requirement / duplication risk |
|---|---|---|
| Core/LocalSettings, UserPreferences; UI/SettingsPanel | Persisted three volume values; Changed after setters; QA uses isolated directory | Keep keys/default values. Real bus mute at zero, log-volume mapping. Changing language/loading settings is silent. |
| FrontendSceneBuilder / GameplaySceneBuilder / MultiplayerSceneBuilder; GameAudio | Authoring creates empty Music/SFX channels and camera listeners | Replace canonical empty arrangement with one production owner. Scene transitions must not create overlapping music/listeners. |
| PlayerMotor / PlayerLocomotion / NetworkPlayer.Motion | Actual authoritative movement and existing replicated positions | Distance-based steps; suppress stationary/teleport steps; attenuate remote actors. Do not add network messages for footsteps. |
| PlayerCombat.AttackStarted / NetworkPlayer.AttackState.Sequence | Melee start; existing server-authored sequence for replicas | Swing once per accepted start, not input press + network echo. Empty swings differ from actual impacts. |
| PlayerRanged / NetworkActivities.Activity.Sequence / StoneProjectile | Draw start then committed ammo drop and projectile spawn; impact is separate | Bow/crossbow release at real release time; no release on cancelled draw or failed ammo transaction. Reuse existing sequence and projectile state. |
| Health.Damaged / Died / ApplyReplica | Damage after mitigation, replica HP/hit updates; replica application does not invoke Died | One hit/death edge, no initial spawn/load injury cue, no destruction-as-death assumption. Resource harvest health has different material identity. |
| ResourceNode / NetworkResource.State.Hits | Actual harvest damage and final break; replicas already have hit counters | Wood/stone/ore/plant distinctions and variant pools, coalesced nearby repetitions. No extra harvest RPC. |
| PlayerFeedback.Presented; PlayerCrafting; StationProcessing | Actual pickup/catch/craft/processing/collect/upgrade presentation | Completion distinct from start; no success cue for full bags/denial/pending work. Existing feedback event alone does not identify all material subtypes. |
| PlayerBuildingService / NetworkConstruction / WorldDoor / WorldTurret | Place/upgrade/repair/door state/projectile success | Placement after commit; remote world state must not replay local optimistic feedback. Processing updates and selection are not success events. |
| PlayerSurvival / PlayerFishing / NetworkActivities | Accepted food/treatment, cast/bite/catch result | Natural cloth/food/water Foley; no cue on rejected consume/full-bag catch. Local UX plus authoritative world action classified separately. |
| CreatureMember / EnemyBrain / NetworkCreature / Creature presentation | Family behavior, alert and committed combat/death frames | Family voices/steps/impacts with distance, sparse idle calls. Cold replication and normal despawn are silent. |
| BossEncounter.Changed / NetworkBoss / BossIdentity | Shared encounter scheduler, actual phase/attack/state/started fields | Intro, persistent encounter music, phase edge, telegraph, committed attack, death/victory; no imaginary phase or duplicate frame cue. |
| GameplaySession / PlayerDeath | Pause, downed/rescue/defeat and actual life state | Local low-health/downed/death/revive cues; menus do not retrigger on state refresh. |
| UI Button, journal, class/Engineer, achievement/attendance | Interaction, local progression or authoritative claim results | Quiet physical UI, no hover/scroll/polling sounds. Ordinary attendance stamp differs from Gem milestone. |
| EconomyService / RewardedAds / StoreService | Authoritative successful transaction/claim versus pending/failed/sync; overlay lifecycle | Gem reward only after a new confirmed grant. Gold/world Crystal are different events. Duck/resume during real overlays without granting on balance refresh. |
| SaveRepository / profile/cloud/telemetry | Background persistence/synchronization | Intentional silence. Do not change schemas or real user save data for audio. |

## Acquisition status and provenance gate

Runway's connected Free plan refused the single music capability probe (`paid_plan_required`). No output or generation task was created and no paid plan/credit purchase was authorized. The official plan card was shown once; production continues using openly licensed recordings/compositions. FreePD's current homepage says the service has closed, so obsolete assumed download URLs are rejected.

Kenney Impact Sounds / RPG Audio / Interface Sounds have official CC0 pages. OpenGameArt music candidates are checked on individual creator pages, not collection tags. Raw Windows HTTPS downloads to Kenney and OpenGameArt currently fail during TLS negotiation; verification is not disabled. Public GitHub redistribution mirrors may be used only for identifiable licensed source audio, with pinned commit, downloaded blob hash and primary creator/rights page retained. A mirror's credit text is not allowed to override the original creator page. No game build or reference OST extraction is permitted. Unverified asset candidates are not release assets.

Implementation, asset selection, actual Unity playback, voice/memory/Android measurements and final results are still pending. This review will be completed with measured evidence, not assumed passes.

## User-requested checkpoint — 2026-10-29

This is an **unfinished Foundation5 checkpoint**, not the 24-item completion report. The user asked to finish the current unit of work and stop. Source acquisition/editing/provenance is that unit; do not start the runtime integration in this session. No gameplay, economy, backend, schema, scene or network source was changed.

- Selected files: **104 = Music6 + Ambience6 + SFX92**, **13,623,935 bytes**; raw pool300files/56,540,545bytes remains outside Git/release. Source names are candidates, not completed event bindings.
- Offline decoding/measurement: all104finite, non-silent and without clipped samples; peak/RMS/LUFS when long enough/DC and encoded-loop boundary measurements are in assets.json. LUFS is not assigned to too-short effects. These are **sample peaks, not independently measured true peaks**. Loop boundary measurements do not prove subjective musical phrasing or listening approval.
- Import QA is an isolated Unity6000.3.19f1 asset/settings/short-effect decode check. See unity-import.json and unity-import-process.json for actual outcome. It is **not PlayMode, output-device playback or a listening session**. Native meta files keep short mono effects decompressed/ADPCM and stereo beds streaming/Vorbis, with Android32kHz overrides; runtime memory/build impact still needs the final voice/cue implementation.
- No runtime audio owner, AudioMixer, cue bank or event bindings have been implemented. Old empty channels/settings remain. Runtime music transitions, local/remote behavior, grant-confirmed reward audio, overlays/background and settings output are pending.
- Save14 / Profile2 / CloudProfile1 / Economy2 / Protocol23 / generation3 / IAPSchema1 unchanged. Real user save fingerprints are compared at this checkpoint; no live project asset synchronization or actual-user-save QA.
- Android native/manifest/Gradle/dependency changes0. No full AAB rebuild is warranted for this asset-only checkpoint; Android script/runtime validation belongs to the later integration stage. Prior Foundation4 AAB is not presented as an audio build.
- Final Polish readiness: **not ready to declare**; finish Foundation5 first.

### Exact resumption

1. Fetch remote main first, verify this checkpoint and read AGENTS/README/AUDIO/AUDIO_LICENSES/this review. Re-read the complete Foundation5 request (attachment `4bb99031-4b15-45b7-89a5-cc534c192865/붙여넣은 텍스트.txt`) before implementation.
2. Inspect/audition selected licensed source candidates and any unresolved mix/loop quality. Do not treat clip existence or measured PCM as subjective approval. No new generic tone fallback.
3. Implement canonical AudioMixer Master/Music/SFX/UI/Ambience, persistent runtime owner/pool/music director and existing three settings controls. Remove superseded empty scene channels when replacement works; keep one camera listener. Use current source/event authority audit, not assumed method names.
4. Bind real movement/materials/craft/build/combat/creature/boss/UI/meta and grant-confirmed economy events. Reuse replicated state/counters; no speculative cue on input, initial replica/load, failed/pending grant or silent autosave. Complete actual lifecycle/overlay handling.
5. Run appropriate data/EditMode/PlayMode plus real Unity output/play routes, actual boss phases, two-process LAN, scene/reload/re-host and accelerated leak gates; settings KO/EN at three requested resolutions; Android script compile. Keep actual device/listening limits explicit. Do not rebuild full AAB absent native changes.
6. Complete all39 requested REVIEW subjects and the24-item final report, commit/push main, verify remote equality and clean worktree; then STOP before Foundation6. Put phase timings last in the user-facing report.

Local preparation helpers are outside Git: audio_work.py, audio_review.py, audio_download.py, audio_produce.py, audio_checkpoint.py and audio_checkpoint_docs.py. `audio-production-work/source-downloads.json` identifies the original local files; the release source ledger is in this validation folder. Unity MCP was read without focus/show; editor was stopped on MainMenu at inspection. The live project's audio/play behavior was not altered.

## Runtime completion pass — 2026-10-29

The earlier checkpoint/stop/restart instructions above are historical. The user resumed this pass; this section is current. **구현·저장소 검증 완료**.

### 1. Start SHA

Initial asset pass b1e6c7e68572ac342812c78e946f773101a6dc6c; this resume fetched remote main first at04:56:51UTC and verified8782da4220a2761f72086acc7488c9c95fdebdb0 equal/clean.

### 2. End SHA

The completion commit is the commit containing this review; resolve with git log -1 --format=%H -- projects/wildfolk/validation/audio-production/REVIEW.md. The user-facing completion report records the pushed full SHA; no self-referential fabricated hash is written here.

### 3. Before

Initial project0clips/0mixers/two empty channels; prior104asset checkpoint had not wired playback. Historical checkpoint below is not current runtime status.

### 4. Inventory

Current101files=Music6/Ambience6/SFX89;74ordinal cues, all bank-referenced, exact duplicates0. Removed three wood-floor candidates because no floor material system supports them.

### 5. Reference games

The seven-column six-game comparison above records concrete public cases, differences, chosen/rejected principles. It is textual research, not listening or measured mix comparisons.

### 6. Sonic identity

Organic woodland/physical material/quiet paper-leather UI. No previous sine generator, reference OST extraction, bright puzzle beeps or default fallback tone.

### 7. BGM

menu/day/night/cave/graveyard/boss; Deep shares cave score but has its own ambience.6unique tracks/7contexts, native loop streams; individual author/rights pages in AUDIO_LICENSES/assets.json.

### 8. Ambience

Forest day, crickets night, cave, deep cave, designed graveyard wind, nearby fire. Sparse randomized birds; forest calls suppressed in cave/boss; only strongest nearby fire loop.

### 9. SFX event coverage

74mapped cues. Runtime actor/confirmed action/UI hooks cover movement, resource, craft, build, survival, combat, families and boss; all-cue smoke is explicitly separate from real gameplay evidence. Hover/scroll/sync/save/ordinary XP/natural despawn and low-health audio are intentionally silent.

### 10. UI audio

All authored123Menu/261Wildfolk/254Multiplayer buttons carry AudioButton; dynamic factories add it. Click/back and category/tab/board page cues; actual successful class/Engineer/achievement/attendance/Gem/Gold, not polling/pending balance updates.

### 11. Boss audio

Actual scheduler Awakening/phase/windup shape/strike/ritual/Dying, delayed victory. Incremental real boss cue counters were checked after all-cue smoke, so smoke cannot satisfy lifecycle assertions. Distant bosses do not change local score/duck.

### 12. Transitions

Boss→underground/deep→graveyard→time/menu.0.3s poll/3s stable context. Separate two-source music/air crossfades; actual full music gain ramps about1.05s/0.42s; same cave track or Menu reload not restarted. Pause/danger duck and delayed death resolution.

### 13. Mixer

Native Unity authored Master→Music,SFX→UI,Ambience; exact-name group lookup and exposed volume GUIDs. No synthetic handwritten mixer surrogate.

### 14. Defaults

Existing Master1/Music0.65/SFX0.85.20log10 linear gain;0→−80dB plus mute. No new fourth/fifth slider.

### 15. Persistence

Existing wildfolk.user.master/music/sfx and language/reset semantics retained. Actual mixer values/zero mute/isolated LocalSettings persistence exercised; no real PlayerPrefs deletion.

### 16. Spatial policy

2D manual distance gain/pan, no Doppler: default22m, step11m, creature18m, critical38m. Owner recovery UI is nonspatial to survive a home teleport before camera catch-up. Movement distance/teleport guards prevent footsteps while stationary.

### 17. Multiplayer

Existing movement/attack/activity/creature/boss state/sequence reused. Owner Growth JSON gets optional semantic ActionSequence/Action to distinguish confirmed food/drink/treatment from passive meter changes. No new RPC/wire type, but one extra owner snapshot update can occur per consumed action; not zero network traffic. Old JSON and first/same sequence replay rules tested.

### 18. Pooling

Dedicated persistent owner,25preallocated oneshot+5loop sources. No per-hit Instantiate, scene-wide scan or temporary collection in voice selection; resource observers stop continuous work after initial prime.

### 19. Voice limits

UI4/impact8/creature5/footstep4/critical4. Lower-number priority, bounded replacement/culling; HUD/resource spam cannot consume reserved critical source slots.

### 20. Spam suppression

Per-cue cooldown, immediate variant-repeat avoidance, distance cull, sequence/state edge/cold prime; bounded256transaction receipt set complements existing durable server/purchase journals. This set is presentation, never currency authority.

### 21. Import/compression

Short mono32kHz ADPCM/DecompressOnLoad; stereo32kHz Vorbis/Streaming beds; Android overrides; normalize off. Avoid total music PCM residency; native imported formats checked.

### 22. Memory

Actual Editor clip object sum 3,150,277bytes/30owned sources. End-to-end real30minute maximum-volume result: PASS. Editor memory is not an Android budget guarantee; no unmeasured GC-free whole-game claim.

### 23. Audio disk size

13,591,135source bytes; SFX2,325,734/Music8,970,341/Ambience2,295,060. Largest day2,836,675bytes. Current hash/no-duplicate report current-media-metrics.json.

### 24. Build-size impact

13,591,135source bytes is not a measured AAB delta. Build recompression/native platform packaging differ; no whole-AAB size claim for this audio pass.

### 25. Licensing

Current source ledger is CC0 only; primary creator pages and individual file mirrors/hash retained. Designed PureData wind is disclosed. Source rights checks are not ownership adjudication or subjective audio approval.

### 26. Provenance

No AI-generated output. Paid-plan-required Runway probe produced nothing/no charge; pinned source redistributions, not game builds/rips. Legacy TLS failures were not bypassed. Mirror hashes verified where published, not claimed byte-identical to inaccessible primary masters. Raw300files56,540,545bytes outside Git/release.

### 27. Unity playback QA

Actual non-nographics Play real movement/resource/craft/place/processing/food/drink/fishing/hurt/lethal-passive/death/home recovery, plus all74cue service smoke. 13448output buffers, peak0.381110,RMS0.022080; own mix samples, not microphone/human listening.

### 28. Boss QA

Existing real BossProductionQa lifecycle passed with actual intro/phase/death/victory counter increases. Existing class survival trait was respected by waiting/providing real repeated damage rather than forcing life state.

### 29. Multiplayer QA

Two separate Editor processes, real LocalNetwork transport/resource/guardian/boss/reconnect/reHost regression passed on both; headless event observers recorded remote warnings/phase/attack/death/victory/downed/recovery. Maximum owned sources30 on each. Not a two-device acoustic approval; optional consumption receipt edge also has focused unit/Play coverage.

### 30. Scene reload

Repeated Menu reload retained one owner/listener and did not restart same context; actual Home/Save14/Continue retained one30source owner. Native scene authoring changed only audio components/removed empty sources; unrelated serialized UI values/order preserved.

### 31. Long session

Actual wall-clock≥1800second maximum-volume multi-context DSP soak passed; see soak.json. Earlier1800logical-second/36,000request stress is event-count acceleration, explicitly not30minutes listening or continuous soak.

### 32. KO/EN Settings

Six1920×1080/1280×720/844×390 captures. Final capture applies actual UserPreferences language so selected language label matches strings; text bounds/click targets/three original sliders retained. Contact sheet manually inspected; no visual redesign.

### 33. Android/static

Source validator passed1864unique GUIDs before final QA helpers; final validator count may grow only for new Editor/test scripts. Alpha/Production59assemblies each compile; development identity/ads/billing fixtures excluded. Full EditMode/focused Play results XML and summary state actual outcomes, not simulated device success.

### 34. AAB decision

No new SDK/plugin/manifest/Gradle/dependency; no full AAB rerun per task instruction. Android scripts compiled instead. Previous Foundation4 AAB is historical, not an audio-integrated release binary.

### 35. Versions

Save14/Profile2/CloudProfile1/Economy2/Protocol23/generation3/IAPSchema1 unchanged. Owner JSON optional fields use tolerant existing serialization; no save fields/migration added.

### 36. Save protection

All action tests/captures/LAN/soak use separate QA directories/projects; actual user save hash/mtime/size compared before/after, final result recorded in user-save-compatibility.json. User actual save/PlayerPrefs not modified.

### 37. Modified files

Canonical AudioBank/Runtime/Landscape/Actor/Button/Transactions/PlayerActionAudio, confirmed gameplay/UI/economy hooks; native mixer/bank/scene audio components; import normalize settings; Editor QA/tests/docs. No gameplay economy/catalog/server/SDK redesign. git diff/status is the exact file inventory.

### 38. Remaining limits

No headphone/speaker/actual Android acoustic/performance certification, live Google Ads/IAP focus/purchase or public-release approval. Static74cue mapping and smoke are not74different real creature/gameplay scenarios. Remote bare-wall impact and third-observer build thunk need actual existing semantic state before any future expansion; no speculative audio RPC added.

### 39. Final polish readiness

Foundation5 repository/content and scoped technical QA complete; ready for a separately authorized Final Polish/human-device audio review. Stop here.

## Final regression interpretation

Related PlayMode: **41 unique cases passed**, combining40/41 in play-final.xml and the corrected pause-fixture focused retest1/1 in pause-final.xml. This is not falsely reported as a single41/41 invocation. The pause test now places its zombie within melee range to test world activity while paused; unrelated navigation approach timing is not its assertion.

Full EditMode: **361/363 passed**. The two failures reproduce identically in an isolated archive of starting main8782da4220a2761f72086acc7488c9c95fdebdb0 (baseline-existing-failures.xml): NetworkCreatureContractTests expects64 registered ecology entries but current baseline has66; WorldGenerationTests expects total resource count<300 but current baseline has307. No audio data or runtime change is used in that baseline replay. The relevant test/data inputs remain unchanged. These existing budgets are recorded for separately authorized maintenance; this audio pass does not alter ecology, world resources or weaken their assertions.

First audio fixture attempts exposed wrong literal Gameplay scene name (canonical SceneFlow.GameplayScene is Wildfolk), placement overlapping the fixture workbench and a real class survival passive on lethal damage. Fixtures were corrected; actual rules were retained. Initial menu screenshots were re-captured through actual UserPreferences language to make the selected-language label match KO/EN strings. Native scene serialization initially rewrote unrelated UI fields/order; canonical export retains only added AudioButton component references and removal of empty audio channels.

[Per-cue inventory](EVENTS.md) · [Test summary](test-summary.json) · [Real gameplay/DSP](actual.json) · [Real30minute soak](soak.json) · [LAN Host](lan-host.json) · [LAN client](lan-client.json) · [Settings](settings.json) · [Android scripts](android-scripts.json).

## Local Editor closure

The actual C:/UnityProjects/Wildfolk Editor was verified stopped/clean before synchronization.30 unrelated local file differences were preserved; source scenes differed only in whitespace/block order. Changed files plus missing checkpoint dependencies were copied. Initial local playback had null media references because the previous checkpoint's raw clip binaries were absent; this mirror issue was corrected before final QA. Final actual MCP Play verified nonzero Menu and gameplay DSP, total scene AudioSources30, one listener, Master0 mute, a real tree break and automatic pickup of4wood. Console errors0; Play stopped, original MainMenu and prior QA directory restored. No Focus/Show/Windows input was used. Real save/profile hashes/mtime/size remain identical after final stop.

[Local MCP summary](live-mcp-summary.json) · [Real save compatibility](user-save-compatibility.json). Foundation6/Final Polish is not started. Android device/headphone/speaker listening and real monetization overlay audio focus remain human/device gates.
