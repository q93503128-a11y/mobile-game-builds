# Post-RC1 Quality Pass #2

Status: **READY FOR USER PC REVIEW** — implementation and related verification complete. This is not RC2/Android/production approval.

Start main: `4b109dba5f1277674c592771e7134dfebb87d799` (fetched 2026-11-02).
Common rules: dev-playbook `e022e9e4ff3947fcbd7f5b0237c5905d7c0409c7`, fetched/read only.
Unity 6000.3.19f1 LTS / URP 2D. Canonical `client` and native `C:/UnityProjects/Wildfolk`.
No Android package/dependency/manifest/plugin, AAB, Console, production, ads or billing work.

## Reproduction and decisions

| Actual finding | Cause | Canonical correction | Verification |
|---|---|---|---|
| Existing creature spawn could appear ineffective away from origin | Default `(0,0)` shared with coordinate teleport; generic applied message; only older families exposed | Actual prefab discovery, near-player ground search, family/count/coordinates/failure feedback; original coordinate group actions retain explicit coordinates | Runtime observations recorded below |
| Hand armor differed from portrait | Hand renderer bounds0.64×0.64; worn art0.34×0.38, inherited scale1; hand basis180° from body/portrait basis | Fit only glove renderers to actual base-hand bounds; local position0, rotation180°, no weapon socket/helmet/profile changes | Four materials × five facing directions plus real attack regression |
| HP number floated above narrow bar | Separate label offset; survival readings used dissimilar tiny bars | Integrated centered HP value; related hunger/thirst icon/value/track treatment | Actual KO/EN three-view audit |
| Repair text collided with hotbar | Context root215 from bottom, hint extended below itself | Compact field context sidebar above tool belt; named object/condition, owned/required costs, exact existing refund preview | Actual repair/release/complete/salvage |
| Numeric hold countdown; unclear removal | Only timer/hint text; sound without clear recovered summary | Progress fill, action text, immediate release cancellation, completion/recovered-material feedback after existing successful transaction | Actual transaction + focused PlayMode |
| First paused frame still showed maintenance over modal |5Hz context polling had no synchronous session-change visibility update | Same-frame session event hides service/actions and releases hold | Regression and paused runtime capture |
| First new field colors were too dark | Multiplied tint by existing dark panel pixels | Existing field panel asset at its intended color; shared primary/secondary palette | First rejected/new final screenshots distinguished |
| Text-height risks on survival/count/hint | Insufficient fixed height; long narrow instruction | Increase height without emergency font shrinking; shorten slot instruction | Actual text metrics, final screenshots |
| Buttons visually escaped field paper despite screen bounds | Paper bitmap's visible frame is inset from full RectTransform | Inset Back, class footer, attendance tabs, MP solo action; actual visible-frame margin audit | Actual rendered RectTransforms |
| Generic Sync competed with meta navigation | Extra permanent manual refresh although open/claim already refreshes | Auto-refresh retained; error-only Try again; Account support sync retained | Delayed/failure service fixture and actual authored buttons |

## External comparison (principles, not assets)

| Public game/source | Concrete case | Lowfire problem | Adopted | Rejected / difference |
|---|---|---|---|---|
| [Terraria publisher mobile1.4.3 notes](https://support.505games.com/support/solutions/articles/150000146926-terraria-console-mobile-v1-4-3-patch-notes-2022-april-27-) | Explicit mobile optimization of crafting/reforge/shop/build/filter menus; settings expand/collapse/close defaults and toggle/hold choice | Fixing only Home is insufficient; hold and modal input need their own inspection | Check actual affected gameplay menus, hold release, language/viewport states | No wholesale Terraria layout/control editor or new mechanics; historical2022 primary source, not evidence of current2026 pixels |
| [Klei development retrospective](https://support.klei.com/hc/en-us/articles/360035456011-Intrinsic-vs-Extrinsic-rewards) | Original quest checklist taught players to wait for objectives; final crafting presentation instead exposed possibilities and encouraged experimentation | Maintenance should explain the actual action/cost without turning field play into a management checklist | Compact contextual information, atmosphere retained, no extra tasks/rewards | Do not copy DST widgets or change Lowfire quests/economy; original2012 article reposted2025 |

Existing Lowfire forest/journal art, fonts and symbols are reused. Prior commercial-ui research remains a dated record, not new play evidence.

## Tools / data boundary

WorldOverview holds the exact passed WorldMap instance. It samples Continental.Environment and water/coast, overlays actual Roads, Anchors, Spawn, and exposes actual cavern-room targets. Thin main-river centerline and POI markers are enlarged to be legible at continental scale; this is explicitly labelled, not terrain reauthoring. Surface overview does not project underground world coordinates onto the surface.

On-demand PNG + labelled self-contained SVG + JSON export defaults to native project Captures/world-overview. SVG contains seed, generation, legend, Spawn, roads and distinct actual POI colors.12-seed batch uses the actual WorldMap constructor/current definition and validator; does not regenerate the Play world. Generated batches remain outside Git; selected evidence is retained here.

Teleport finds walkable/clear ground, activates destination chunks and rechecks; a failed materialized destination restores origin. Targets include Spawn, early Forest/Rocky, sampled Wetlands, river bank, actual cave/deep rooms and POIs. No fictional anchors. Legacy generation1 explicitly reports that continental overview is unavailable.

1×/4×/10×/25× QA walking uses a UNITY_EDITOR-only driver, scales resolved walking velocity and checks intervening terrain/physics. It changes neither GameplaySettings nor saved stats and does not scale dash. Set1/disable/window-close restores ordinary walking. Active mode is visibly labelled. Editor factories/windows/exporters never ship; both QA runtime helper types are stripped by UNITY_EDITOR. They are never serialized into scenes/prefabs.

Showcase discovers real local creature prefabs; player is the current actor. Sword Skeleton and isolated King use their real existing implementations. Near-player spawn, group×3 and clear are available. Forced showcase locations and enemy night setup are QA, not evidence of natural habitats. Clear owns instance leases and drops ownership on pool release before a natural actor can reuse the instance. Existing Combat Sandbox remains available.

Optional free camera omitted: overview + validated travel + speed provide mandatory coverage without a second camera-control system.

## Multi-seed world findings

Seeds9472026–9472037: all four sampled surface biomes,8 cave networks,75–79 roads and263–269 POIs; no reported overview invariants failed. Exact per-seed counts/percentages are in seed-comparison.json. Spawn remains origin. Three early routes lead to Forest, Rocky and river ford; additional routes are local trails at eligible important anchors, not an all-POI path network. Endpoints/layout and biome proportions vary with seed. Gen3 code, planned early anchors, resource IDs and map scale are unchanged.

Continental images expose real coast, lake, main river/tributary and biome patches. Overview sampling is not a proof that every possible global route or future placed structure is reachable. Existing generation validator checks progression supply/required paths; cavern graph and actual teleport checks are recorded separately.

## Verification log / honest limits

Initial probe mistakes are preserved outside Git: first KO-named captures had EN content because LocalSettings data was changed without UserPreferences notification. These are rejected as KO evidence. First hold-tail capture had an ambiguous ko/en filename; language assertion stopped it. Corrected capture API asserts requested language.

Initial test initialization failed with one test iterator missing yield; fixed before execution. Initial4-case run had3 pass/1 fail because Unity cannot attach a MonoBehaviour from the Editor assembly. The corrected UNITY_EDITOR runtime driver passed the targeted retry. Subsequent five-case related suite passed5/5 (job907e81ec5b5b4cde9dbd030ae80101eb); prior attempts are not counted as passing full runs.

Native source import caused an external scene reload modal and MCP lost ping; user performed Reload/Refresh. Scene files were removed from subsequent source-sync copying. Final scene authoring saves the native scene and copies normalized canonical content, without reloading a stale scene from disk.

User six protected files: before/after SHA256, size and mtime_ns; QA profile path C:/UnityProjects/Wildfolk/Captures/Codex_QA_QualityPass2. LocalSettings has an isolated in-memory store; ordinary PlayerPrefs and Continue are not used. Final six-file hash/size/mtime comparisons passed. Editor is MainMenu, Play OFF, paused=false, clean scene, isolated QA marker cleared.

Unchanged: Item136 / Recipe111 / Save14 / Profile2 / CloudProfile1 / Economy2 / Protocol23 / generation3 / IAP1. No rewards, combat values, costs/refunds or authority architecture changed.

## Final validation / evidence

| Check | Final result | Exact scope |
|---|---|---|
| PlayMode |8/8 PASS, job072ef079af3d4af6880853e7f4fb5f67 | Final input-gate correction included; actual touch module/ownership/multitouch/modal exclusion, armor4materials×5directions and ordinary attack, HP/Pause callbacks, repair cancellation/cost/completion/full-bag preservation, auto-refresh/error retry, speed restoration and pooled spawn lease |
| EditMode |5/5 PASS, job9ec96adc8a1847da8194f3e3d7fe319d | Canonical map marker identity and deterministic export, frontend/save/content/scene bindings; this run precedes the final display-title/input-gate corrections, which were checked by final runtime/data validation |
| Data validator |0 errors | Final native canonical assets/data; no balance/catalog/schema changes |
| Source/project validator |PASS,1889 unique GUIDs | Landscape/source/sprite/text structure |
| Actual UI matrix |146 records,0 unresolved height/offscreen/paper-frame/button-label risks |66 Home/meta +53 gameplay/action/death +20 additional Class states +7 focused probes. KO/EN at1920×1080,1280×720,844×390; additional class states at844. Portrait level caption deliberately sits below its thumbnail and is excluded from button-label containment. Scroll content is masked/reachable, not incorrectly treated as escaped buttons |
| Actual world/tools |11 walkable destinations, invalid destination preserves position | Spawn/Forest/Rocky/Wetlands/river/cave/deep/Graveyard/GoblinCamp/Quarry/Mine; safe points may move a few units from an occupied marker |
| Actual speed |4.2 /16.8 /42 /105 unit/s |1×/4×/10×/25×; over10 real fixed steps(0.2s), displacement0.84/3.36/8.40/21.00. Earlier rough0.3s probe had no warm-up/zero baseline and is not the accepted measurement. Stats unchanged and1× restored |
| Actual showcase |19 current families retained and clear of upper/lower HUD | First front/down probe exposed fleeing Crow/Deer behind hotbar; final near-player side placement and stronger viewport check passed all19. Real Goblin/Crawler hurt/death; actual King Phase1 AI windup; group3/clear3 |
| Actual maintenance |Repair/release/complete and salvage/refund PASS | WorkbenchIII long lists are scrollable/reachable in settled KO/EN; full HP shows no misleading cost. Disabled button cannot start hold; disabled/inactive controls release it. Completion summary max2 material types + remaining-type count; full exact refund remains in preview |
| Actual death |Six Dead-state views accepted | First hit activated Survivor's existing lethal-hit passive(HP1,Alive). These images were not accepted as death. Second hit after protection expired produced HP0/Dead/Defeated; six KO/EN views replaced the earlier samples |
| Console / cleanup |Final error entries0; two pre-existing Editor QA compiler warnings(CS0252 PoiContentQa, CS0414 DeathFoundationQa) retained; temporary observers removed | No claim that all earlier attempts were error-free. Exact prefab/scene GUID scan finds no authored DeveloperMovement/DeveloperSpawnOwner. Both helper types are UNITY_EDITOR-only |
| User files |6/6 unchanged | SHA256 + bytes + mtime_ns; final metadata retained, no save content copied |

The variant audit found `Focused Treatment` wrapping in Medic node2 at844. The English display title is now `Focused Care` at the original font size; node ID, prerequisite, effect, KO title and costs are unchanged. The value lives in Resources/Classes.json; its diff changes that one display field only. A final actual capture/metrics row replaces that failed record. Diff review also caught/restored two accidentally decoded middle-dot separators before final compilation.

The final showcase placement prefers the horizontal facing side, otherwise right of the current actor, to keep mobile header/hotbar clear. Exact spawn coordinates/failure are shown in the Editor console. Windows input/Focus/Show was not used; Editor GUI buttons were exercised through their shared APIs/runtime, not OS mouse automation. The user opens the console manually.

### Reviewer route / selected screenshots

1. Open MainMenu and Play normally to inspect Home/meta and KO/EN. [Home](evidence/menus--home-ko-844.png), [Attendance](evidence/menus--attendance-ko-844.png), [Multiplayer](evidence/menus--multiplayer-ko-844.png).
2. Inspect gameplay context and Pause: [before repair](evidence/before-repair.png), [after HUD/repair](evidence/gameplay--hud-repair-ko-844.png), [before Pause](evidence/before-pause.png), [after Pause](evidence/gameplay--pause-ko-844.png), [hold](evidence/gameplay--repair-hold.png), [salvage result](evidence/gameplay--salvage-complete.png).
3. Compare hand gear: [before](evidence/before-armor-weapon.png), [after world](evidence/tools--armor-moving-attack.png), [preserved portrait](evidence/tools--armor-character-comparison.png).
4. Use Wildfolk > Developer > Open Test Console manually; choose Start Isolated QA Play, start a new world, Inspect actual current seed; select a real target/export. [Seed9472026](evidence/world-overview-seed-9472026-labelled.png), [9472027](evidence/world-overview-seed-9472027-labelled.png), [9472028](evidence/world-overview-seed-9472028-labelled.png). Native export is PNG map + fully labelled SVG/JSON; labelled review PNGs are rasterized from that exported SVG with the existing Noto font/resvg2.6.2, not redrawn maps.
5. Try1/4/10/25×, selected family/group3/clear. [Speed badge](evidence/final-probe--qa-speed-25-verified.png), [gallery1](evidence/creature-gallery-final-0.png), [gallery2](evidence/creature-gallery-final-1.png), [gallery3](evidence/creature-gallery-final-2.png), [actual King windup](evidence/gallery-final--king-actual-windup.png).
6. Review longest costs and death: [III repair costs](evidence/final-probe--wbiii-costs-ko.png), [all refunds](evidence/final-probe--wbiii-refunds-en.png), [death](evidence/gameplay--death-ko-844.png).

`screens.json` indexes49 selected evidence files(about9MiB) and their SHA256/source paths. Raw screenshots, rejected probes and full batches remain under native project Captures/Codex_QA_QualityPass2; they are not a huge committed batch. Gallery contact sheets are labelled crops of actual screenshots; original full frames remain available. `runtime-audit.json`, seed comparison, final test jobs, state/console/GUID/save checks and `files-changed.txt` provide inspectable evidence.

### Performance and limitations

World overview/batch work is explicit and on demand. The runtime current-map overview/export plus11 destination activations took7666.94ms total in this Editor fixture; this includes chunk materialization and is not isolated sampling cost or Android frame time. Overview has no per-frame sampler/texture allocation and disposed textures are released. The QA movement driver runs only when deliberately attached in Editor; inactive1× exits immediately. Ordinary game stats and time scale are untouched.

No Android device, mobile FPS/thermal, Relay/remote2–4-player maintenance/armor playthrough, natural full progression or complete boss-phase clearance was performed. Remote hold fill remains presentation on existing requests; server transactions retain authority. Exact new repair/salvage completion toast on a remote client is not certified by this SP pass. Global reachability of every world route/dynamic structure is not proved. Optional free camera was omitted. Subjective commercial design approval belongs to user PC review. These limits do not imply RC2 packaging approval.

## Files and durable state

`files-changed.txt` lists every path in the completion commit. Changes are confined to canonical Editor QA/presentation builders, two UNITY_EDITOR helper types, glove/hold/context/meta presentation, display strings, authored scenes, focused tests and related docs/evidence. Native changed assets match canonical content after newline/trailing-whitespace normalization. No last-known-good player state was replaced; final scene is saved and clean.

## Closure / version boundary

Repository implementation and related checks are complete, **READY FOR USER PC REVIEW**. Item136 / Recipe111 / Save14 / Profile2 / CloudProfile1 / Economy2 / Protocol23 / generation3 / IAP1 remain unchanged. No generator/balance/loot/recipe/network schema/plugin/manifest/Gradle/AAB/Play Console/production/ads/IAP activation changes.

Starting SHA is above. Ending revision is the main commit containing this completed report (`git log -1 --format=%H -- validation/post-commercial-ui-qa-tools/REVIEW.md` from projects/wildfolk); its literal SHA and local/remote equality are recorded in the final user response and outside-Git `quality-pass2/final-git.json` to avoid a self-referential commit hash. Main is committed/pushed only after diff/data/evidence review; final worktree and editor state are checked again after push. Stop here; RC2 needs a separate instruction.
