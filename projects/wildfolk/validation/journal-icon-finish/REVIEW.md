# Camp Journal icon / status finish — 2026-10-23

## Scope and actual baseline

Start: `198cdc1299fff7d69527d68a11100f21c05c98d0` (fresh remote fetch, fast-forward on main). Unity 6000.3.19f1. Actual MainMenu, Class/Engineer locked and common tree ran at1920×1080; common tree also captured1280×720 before edits. Existing layout, roster, connection geometry, Character Sheet, MainMenu and all gameplay retained.

Measured roster Ownership: centre(15,-45),216×34 inside308×104 card; lower edge−62, row rule−52 =10 units outside. Shared Selected/Owned/Locked component. New centre(−108,−31),90×38 sits under the class mark, above rule; font20 unchanged. Common state32px versus preferred29px =1.5px top/bottom; height40 gives5.5px each. CTA Locked266×62 versus preferred37 already has room and stays unchanged.

## Short visual comparison — viewed actual screens before implementation

|Game / source|Actual screen / observation|Lowfire application|Rejected alternative / reason|
|---|---|---|---|
|[DST — Klei community screenshot](https://forums.kleientertainment.com/forums/topic/156801-winona-and-wurt-skilltree-actually-work-really-well-as-skilltrees-layouts/)|Winona/Wurt skill tree: small dark object motifs on paper stamps, progress medallions outside glyph, shared pen weight|Keep existing paper nodes, separate lock/seal; simplify each symbol to an object plus effect cue|Do not copy branching layout, dense sketch details or skill art; existing Lowfire tree remains|
|[The Flame in the Flood — actual inventory guide](https://steamcommunity.com/sharedfiles/filedetails/?id=2934190346)|Journal inventory with large selected cattail drawing and compact silhouettes in grid; generous negative space|Use transparent ink silhouettes with broad cuts; same class symbol works at roster58 and selected140|Do not adopt colored tile backgrounds or rebuild journal book|
|[Cult of the Lamb — original icon artist, in-game Crown Offering](https://www.behance.net/gallery/151034387/Cult-of-the-Lamb-Icons)|Consistent emblem weights, open interior cuts; selected contour and lock separate from icon|One pen density, visible silhouette at54/36px; existing selection ring remains|Do not copy occult symbols, red/black palette, diamond cards or glowing state effects|

Research images stayed in external local `journal-finish-research`; no external asset entered the game/repository. Source URLs and observations only. This is a short UI reference check, not economy/content research.

## Authored art / chosen mapping

Original Pillow vectors at384, downsampled to128 per cell, transparent, existing Journal Ink(49,63,48), no gradients. One atlas1408×768 contains6 identity marks +6 common skills +54 simple node compositions, using shared object motifs and explicit effect cues. These are compact stamps, not54 illustrated inventory assets. Generator mapping follows actual Classes.json descriptions, with no missing-icon fallback. Existing world-feedback and Character Sheet artwork stays in place.

Class: Survivor ember/logs; Warrior shield; Hunter paw track; Medic medical pouch; Explorer compass; Engineer open wrench. Common: heart, lungs, recovery arrow, boot, meal bowl, bandage. Node mapping is explicit in `tools/generate_journal_symbols.py`: e.g. Cooling turret+drop, Golden Time clock+plus, Overrepair wall+plus, Trap Service trap+wrench. This replaces mixed colored inventory images and indistinguishable repeated item substitutions; composition cues express the actual target/effect, not rank numbers. Locked symbol uses white RGB at72% alpha, preserving authored ink color; lock badge/frame remain existing style.

Validation and final evidence recorded below after execution.

## Executed validation / objective visual results

- Live Unity compile: no errors. Two unrelated existing compiler warnings occurred on the first compile (PoiContentQa reference comparison and DeathFoundationQa unused field); final console error/warning query returned0 after the final reload. This is not a full project warning cleanup.
- Focused EditMode: **5/5 passed**, final `ui-editmode.xml`,1.074s test execution. Isolated batch process44.319s wall including startup/import/compile. Earlier iteration also5/5; unique tests remain5, not10. No Class Foundation suite, LAN, death or repair suite ran.
- `ProjectDataValidator.Validate()` actual live result:0 errors. Source validator: PASS,1421 unique GUIDs, valid sprite references and visible text. First source check stopped for missing new test metadata; after preserving Unity-generated test .meta, final check passed.
- PNG audit:66 distinct cell images, minimum5px transparent border,128×128 cells; texture1408×768 imported without resize/compression/mipmaps. FullRect slicing, consistent padding, no missing sprite fallback. IDs persisted when importer was called again.
- Actual UI events:6 class selections +54 class node selections, correct sprite/focus binding,54 topmost GraphicRaycaster hits to the intended node. Common6 clicks +6 topmost raycasts; actual Invest button on isolated QA growth gave Learned/Available/Locked simultaneously. These are UI interaction checks, not progression/balance QA. Initial class raycast probe ran in the same call that enabled the hidden book and saw no rebuilt draw depth; after waiting for a rendered frame, all54 passed. No input code changed.
-1920×1080: viewed MainMenu, all6 class identities and node sets (including actual capture contact sheet), Locked Class, common Locked and mixed states. No text clipping, icon crop, missing glyph, added raycast target or icon/text/connection overlap observed. Existing selection rings/lock/seal remain separate. Text sizes and node/icon/layout geometry remain unchanged except roster status rect/anchor and common status height.
-1280×720: viewed Class Locked, Korean Class, common mixed states. Status labels fit; primary symbols still distinguishable. Secondary composition cues are smaller here and need the existing name/detail text to communicate precise effects; this pass does not claim54 complex effects are understandable from an icon alone. Physical mobile touch and final aesthetic preference remain user judgments.
- Localized label limit: changing `L.SetLanguage("ko")` after the MainMenu→Gameplay scene transition threw one MissingReferenceException from an already-destroyed LocalizedLabel listener in the unchanged localization path. Manually refreshing the live common tree showed Korean dynamic node/status text, but its static headings remained English. `23-common-ko-1280.png` is deliberately documented as partial localization, **not** a full Korean UI pass. Full Korean Class screen and KO/EN status text metrics passed. No localization code was changed or broader fix started.
- Canonical scene audit: MainMenu30 changed serialized blocks; Wildfolk12; Multiplayer12. No added/removed objects. Only ClassIcons/SkillIcons/m_Sprite and the intended status RectTransform/alignment fields differ. Discarded one unrelated Unity save normalization of an unused best-fit max value40→42 (best-fit disabled; actual font42 unchanged). Book/card/root/node/connection positions and Character Sheet remain intact.
- CharacterGrowth.asset: only6 Icon references differ; all other fields byte-equivalent after excluding Icon lines. Classes.json, gameplay code/data, XP, points, effects, prices and levels unchanged. Save13/Profile1/Protocol20 unchanged.
- Original user save hash **and mtime** match before/after; isolated directory `C:/UnityProjects/Wildfolk/Captures/journal-icon-finish/isolated` used throughout. Live editor stopped on MainMenu, language reset by reload to original English.

## Screenshots

Actual Game View PNGs are outside Assets under `C:/UnityProjects/Wildfolk/Captures/journal-icon-finish/` (absolute paths, sizes and hashes: screenshots.json):

|Evidence|File|
|---|---|
|Locked Class + all6 roster marks + Engineer tree,1920|10-after-class-locked-1920.png|
|Six selected class symbols, crops from actual1920 Game Views; derived contact1920×300|24-class-icons-comparison.png|
|Survivor / Warrior / Hunter / Medic / Explorer trees,1920|11-survivor-1920.png through15-explorer-1920.png|
|Common Locked,1920|20-common-locked-1920.png|
|Common learned/available/locked,1920|21-common-states-1920.png|
|Class / common1280|16-class-locked-1280.png /22-common-states-1280.png|
|Korean Class1280 / partial Korean common1280|17-class-ko-1280.png /23-common-ko-1280.png|
|MainMenu preserved,1920|18-after-main-1920.png|

New game artwork: **1 PNG atlas**,66 subsprites (6 class +6 common +54 compact compositions). New Editor importer/authoring source and focused test source each have standard Unity metadata. No external game images/assets, ImageGen, new content or next Boss/Undead King work.

## Timing / usage

Observed boundaries are in timing.json. First batch73.712s and final batch44.319s overlapped other authoring/visual work; do not add them to overall elapsed time. Tool waits were not exhaustively measured. Per-phase token/cost counters are unavailable; no estimated exact usage is invented. Git SHA equality/clean proof is recorded after commit/push outside the repository to avoid a self-referential SHA.
