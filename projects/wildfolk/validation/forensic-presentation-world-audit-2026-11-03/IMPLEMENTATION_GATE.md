# Implementation gate — 2026-11-03

Baseline fetched main/clean HEAD: `0ae9feae389c24abae40bc68da9569dd2eb5a676`.
Investigated `4b109dba → 0ae9feae` and relevant earlier commits. This gate was written before product source changes.

## Completed audit observations

- FACT: actual resource gallery identifies orange bands as rule5 CopperOreNode / CopperOreNodeResource / copper_ore_node.png / copper_ore×6. Natural instantiated identities also inspected. Original user object's stable ID is unavailable.
- FACT: thirteen actual wildlife prefabs inspected together in down/right presentation. Crow, Bat, Wolf and MountainGoat art has its head at local +Y. Their shared presentation assumes local −Y. All four render with their heads to the left when supplied facing right. Source art primitives independently establish the same axes.
- FACT: actual Crow wander/flee displacement samples include forward-dot-velocity −1. `flipX/flipY=false`; AI points away from player correctly. This was introduced with `3667bae`, before both investigated UI endpoints. Shared replica presentation uses the same defective basis.
- FACT: latest water/material/generator dependencies are unchanged between UI endpoints. SampleContinental function hash is identical in `4502ab8`, `318ddd6`, `2529720`, `4b109dba`, `0ae9feae`. Historical Gen1 screenshot has ripple/shallows detail absent from Gen2's already-flat water. Earlier correct water with CURRENT geometry is not established.
- FACT: Pan's natural g6:0 bank node (seed9472027) was harvested with two ordinary Primary actions into Tin Ore×5. Actor was dry, signed water distance4.153. Bank-sediment design is explicit since `cf4ecd6`; in-water panning would be a policy/mechanic change.
- FACT:12 newly constructed validated seeds each contain8 networks/12 entrances; Graveyards9–15, not exactly one. Forest distance210/easterly bearings, Rocky westerly650–655, main river east. Roads75–79. Graveyard count/distance is not validated as a guaranteed progression invariant.
- FACT: two actual Play traversal trials used movement input,1×30s and four10×45s legs per seed. No teleport within trials, no collision/terrain removal. No central viewport landmark sightings; much time was collision-blocked. These scripted straight legs are not a complete human exploration/discoverability study. Cave/grave teleport inspection happened AFTER trials and is not counted as discovery.
- FACT: current repair card inspected in KO/EN at1920×1080/1280×720/844×390. Wide text panel occupies roughly40% screen width. Player-loop hold/release/repair/dismantle observed: cancel preservesHP60; repairHP100; salvage removes and returns Wood2. Earlier Editor-update timing probe failed to complete repair and is rejected as transaction proof, retained transparently.
- FACT: Pause hides maintenance immediately. Main/meta current Play capture and old supplied validation captures are separately labelled.
- INFERENCE: frontal/overhead perspective mismatch in Wolf/Goat art contributes to a lying/rotated appearance. No original user screenshot/position exists, so the precise originally seen species/state remains UNKNOWN; no persistent pooled-death pose has been established.
- External comparison: fresh official Kefir repair/removal/recycler help, Bscotch1.1 illustrated patch notes (equipment salvage, compendium, selected-tab contrast, telegraph alignment, night navigation), Terraria mobile-controls announcement, Klei cave support and official gameplay pages,10tons handcrafted-world explanation; community wiki/search-image evidence is explicitly secondary. Raw remote image retrieval timed out/403 for several hosts; no pixel-perfect/latest-client benchmark is claimed.

## Automatic implementation authorized by the task and selected here

**BUGFIX / HIGH:** one data-driven visual-facing offset in the existing WildlifeActionDefinition/Brain presentation; set180° for the four proven +Y-authored sprites, preserve default0° for all other assets. Update the existing authoring builder so regeneration retains the correct bindings. Verify all eight action-driven wildlife, cardinal/diagonal directions, host/shared replica presentation and pool reset; actual Crow displacement before/after. No AI/stat/flee/habitat/loot/art redraw/wire/save changes.

## Explicitly not implemented

Repair/salvage **REBUILD** proposal only, water **REBUILD** proposal only (recent regression attribution not proven), roads/early-anchor **REBUILD** proposal only, cave/grave discovery **REBUILD** proposal only, resource/art identity presentation **REBUILD** proposal only. Counts/seed/save/network/economy/QA tools retained. No broad rollback.

Final complete classification/evidence/limitations are in FORENSIC_AUDIT.md. Passing tests will not constitute commercial visual approval.

## User-requested checkpoint

After this gate was written and before any product implementation, the user requested a suitable stopping point. **The facing correction above is selected but NOT IMPLEMENTED.** This commit contains audit documentation/evidence only. Resume from RESUME.md; do not report this checkpoint as the completed original audit-and-correction request.
