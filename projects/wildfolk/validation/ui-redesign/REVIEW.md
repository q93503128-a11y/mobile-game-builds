# Class correction / Camp Journal redesign

Start: `7502810ffcf957de6a79c2141c7657e9705eccd4`, fetched remote main before work. Existing Item134/Recipe111/Save13/Profile1/Protocol20.

## Before: actual Unity visual audit

Captured in Play at 1920×1080, inspected the saved images before editing. Original user saves are excluded; DevelopmentProfile points to Captures.

|Surface|Observed strengths|Problem / direction|
|---|---|---|
|MainMenu / Home Canvas|Forest, campfire, hand-lettered Lowfire, bound paper with folds; warm brown primary, ink-green labels|Detached CLASS button breaks book navigation; integrate a menu row|
|Class roster / tree|Six distinct existing class symbols, functional ownership and prerequisite checks|Dark rectangle, equal-weight cards/buttons, weak tree links; split roster, identity and illustrated branching tree|
|Character / common tree|Portrait, readable numerical values, point/rank labels|Same dark rectangle, single tall stat string, crossing root lines; journal sheet and deliberate branches|
|Inventory / equipment / tooltips|36-slot structure, large item icons, focused detail on right, clear selected border|Dark utility visual; audit only, preserve existing interactions and layout in this pass|
|Crafting / building windows|Recipe list and selected details, station identity and actions|Legacy dark surfaces; retain scope, do not rebuild unrelated economy UI|
|Settings / modal windows|Large controls, existing localized labels and confirmation guard|Legacy dark surface; new class confirmation uses journal components, unrelated settings retained|
|Multiplayer lobby|Already uses bound paper with large brown Host CTA|Detached CLASS button; move entry into book margin as a bookmark|
|HUD|Compact profile/HP, item icons, thumb controls; shared muted olive palette|Keep gameplay roles; journal overlays must block the correct inputs|
|Typography / borders|Existing bundled Noto Sans KR (OFL); current headings32–38/body22–29/captions18–21; page rules and irregular border already authored|New hierarchy: identity42, heading32, body24–26, short node names22, caption20; no new font dependency|

Before evidence: 01-before-main.png,02-before-class.png,03-before-settings.png,04-before-lobby.png,05-before-character.png,06-before-shared.png,07-before-inventory.png,08-before-crafting.png.

## External visual comparison — images actually inspected before UI design

Six full source images were fetched to a local research folder outside Git and inspected together at 1800×1860. Sources below distinguish developer/community/press, not all are official. Images are research evidence only and are not distributed as game art.

|Game / group|Actual screen / source|Good observed pattern|Lowfire difference / adopted principle|Rejected|
|---|---|---|---|---|
|Don't Starve Together / A|[Klei-hosted community actual skill screen](https://forums.kleientertainment.com/forums/topic/156801-winona-and-wurt-skilltree-actually-work-really-well-as-skilltrees-layouts/)|Ink-like connections, visually separated branches, lock medallions|54 nodes are nine per class; visibly join a root to three paths, seal/lock states|Dense PC tree, tiny labels, gothic art|
|Grounded / A|[Developer 1.2 teaser image via Steam CDN](https://clan.steamstatic.com/images/36885361/bdb82a8de6dc2134781ce47b5326799f59c0708b.jpg), [source mirror](https://devtrackers.gg/grounded/p/c9489872-here-s-a-teaser-for-update-1-2)|Large mutation symbols, selected detail kept apart from icon field|Lowfire has point prerequisites, not equip slots; separate detail area and read the selection quickly|Orange terminal tabs, purple neon, debug label|
|The Flame in the Flood / A|[Steam community actual inventory spread](https://steamcommunity.com/sharedfiles/filedetails/?id=2934190346)|Bound two-page book, left catalogue/right focused details, paper tabs|Use existing Lowfire journal sprite; roster and field notes form a spread|Tiny inventory grid and copied paper texture/layout|
|Cult of the Lamb / A|[Original icon artist's portfolio, Crown Offering](https://www.behance.net/gallery/151034387/Cult-of-the-Lamb-Icons)|Strong selected symbol, small lock badges, one focused explanation|Reuse our six class symbols and authored item icons; keep short labels and clear primary action|Occult ornament, red diamond frames, monochrome black backdrop|
|Wild Rift / B|[Engadget actual mobile champion selection](https://www.engadget.com/league-legends-wild-rift-moba-mobile-impressions-150022756.html)|Selected champion visibly larger, secondary roster below, abilities grouped nearby|Lowfire has compact original symbols; prioritize selected class name/symbol and role before tree detail|3D model scene, countdown, blue/gold competitive presentation|
|Brawl Stars / B|[Creator actual choose-a-brawler screen](https://x.com/KairosTime/status/1607919986571632640)|Three readable silhouettes, large names, one unambiguous choice task|Only six classes; persistent short roster entries, selected ribbon and distinct ownership state|Saturated blue, giant character advertising, rarity monetization language|

## Chosen direction before implementation

**Camp Journal**. Existing lowfire_journal.png and Lowfire wordmark remain the visual source. Paired paper pages, spine at center, warm ink and ember-brown emphasis. Left: six roster entries plus a larger selected-class field note. Right: one root splitting into three ink paths, icon nodes with seals/locks, separate detail footer. Shared growth uses a field-guide diagram and matching detail page. Character uses portrait, progression notes and vitals sections. Buttons remain direct, with no page-turn delays. Main entry becomes an actual journal menu row; multiplayer gets an attached bookmark.

Reuse small authored Editor components (book surface/page, heading, rule, bookmark, node, detail footer, currency badge, primary/secondary action). Runtime node state is shared by class/common trees. Currency icons are original generated shapes, not the mining Crystal sprite. Existing Noto Sans KR/OFL and original game icons are reused. No new commercial dependency or external game asset. No Fanpot/Penpot canvas needed for this existing Unity source target.

## Objective corrections

- Production default remote admission: fresh Survivor, zero claimed class XP and nodes. Host local profile is read from the trusted Host store. A peer cannot claim the Host profile route. IClassAdmissionVerifier remains replaceable for a future authenticated provider.
- Explicit Editor/Development Alpha verifier constructs an ephemeral validated account; never writes remote claims into Host durable storage. Session lock and recent reward window survive reconnect, clear at new Host.
- Profile1 drops obsolete reward Receipts on next successful write; JsonUtility ignores the legacy field. Ownership, XP, nodes, selected class and purchase intent survive. Durable purchase receipts remain separate and unchanged. Runtime recent reward window is capped512; death commit is the primary idempotence boundary.
- Medic A2 now reduces generic Medicine use cooldown20% for self/allies (4→3.2s), leaving application0 and A1/A3 unchanged.

## Implementation / visual revision / final evidence

### Self-review and corrections actually performed

First implementation was captured and opened (`iteration-first-class.png`). Missing class names were real font-metric clipping (42px text preferred61 in a60px box; roster27px preferred39 in36). Increased those boxes, made role labels concise, moved actions inside the paper, joined the pages with a cloth spine, and routed the common Metabolism root connection around unrelated nodes. English identity text needed more height; Premium and Fortification labels were shortened. Final image review caught Metabolism's state too near the bottom and incorrect fallback icons; moved it inward, bound actual existing icons and removed the silent icon fallback and unused hidden class state text objects. No unchanged full132-minute suite was run.

Final actual screenshots were opened in contact sheets, with the final1280 shared tree,1920 Medic and MP entry also opened individually. These are original Unity GameView captures, not rendered mockups. The final layout is source-authored and saved in MainMenu/Wildfolk/Multiplayer scenes.

### Actual UI evidence

|Surface|1920×1080 KO|1280×720 EN|Performed checks|
|---|---|---|---|
|Main journal entry|20-main-1920.png|20-main-1280.png|Detached CLASS removed, row aligned within the existing book|
|Gold class locked|21-gold-locked-1920.png|21-gold-locked-1280.png|Price60, Select disabled, ownership shown|
|All six classes|22-class-*-1920.png|22-class-*-1280.png|Every one of54 node descriptions in each language/resolution, roster selection, owned/selected/locked, seal/ring/lock, XP/point labels|
|Engineer premium|22-class-engineer-1920.png|22-class-engineer-1280.png|Purchase disabled; natural premium lock text; Gem badge shows—because no authenticated balance provider exists|
|Character sheet|23-character-1920.png|23-character-1280.png|Level1001, XP, HP, defense, stamina, recovery, consumption, hunger/thirst and points retained|
|Common six-node growth|24-shared-tree-1920.png|24-shared-tree-1280.png|All six details and ranks, prerequisite locks, actual Invest, Back/Close callbacks|
|Safe area|25-character-safe-area.png|—|Existing SafeArea rect inset90 on each side; text fit; simulated inset, not a physical notch device|
|Multiplayer entry|26-multiplayer-entry-1920.png|—|Class entry attached to existing MP book header; inspected screenshot|

`visual-qa.tsv` contains 26187 passing assertions, no failures in the final run. It checks preferred text height, every active text rectangle within the journal's inner page bounds, and enabled button screen-space targets≥47.5px (48px at720,72px at1080) plus top EventSystem raycast. Learn/Select/Respec/common Invest invoke serialized Unity button callbacks. These are Editor/code-driven interactions, not physical touchscreen input. MainMenu and multiplayer bookmark were inspected directly; no claim that every legacy settings/crafting/HUD screen was redesigned.

### Objective correction validation

|Check|Result|Evidence|
|---|---|---|
|Targeted EditMode|30/30 (20 existing class models +10 correction cases)|correction-models.xml|
|Release forged Engineer/Gold class/XP999999/all nodes|Fresh Survivor/XP0/no nodes, no Host store writes; Host-profile impersonation rejected|ClassCorrectionTests in correction-models.xml|
|Explicit development / Host local / reconnect|Validated ephemeral QA snapshot; Host reads own store; same session class+XP+window retained|model tests + LAN|
|10,000 class XP rewards|Profile JSON406→410bytes, totalXP10000; runtime receipt count512; last duplicate rejected|correction-models.xml test output|
|Profile1 migration|Old checksum-valid Receipts ignored, next primary write compacted; ownership/XP/nodes/selected retained|correction-models.xml|
|Write failure|No receipt/XP partial commit; retry succeeds exactly once|correction-models.xml|
|Targeted PlayMode|3/3: Lobby purchase/tree/session, common growth/migration/reward, generic Medicine|correction-flows.xml|
|Medic A2|Generic self/ally4→3.2s; application0 unchanged; A1 healing and A3 same-total duration retained|PlayMode + actual LAN|
|Actual two-process LAN|PASS, run `9c9c2a6c57c94950bcdd7bf64b2260a2`; separate hidden Unity client and MCP live Host|actual-lan/9c9c2a6c57c94950bcdd7bf64b2260a2/host.tsv,client.tsv,complete.txt|
|LAN concrete results|Warrior Host, explicit QA Medic remote; self33HP/early retry no second consume; ally33HP after3.25s; cooldown sample3.030052s after elapsed frame; XP1800→1808 once; Engineer reconnect claim remains Medic; no peer profile file on Host before/after; Save13 succeeds|same LAN run|
|Canonical data/source|ProjectDataValidator0, console errors0, Python source validator PASS/1418GUIDs|data-validation.txt,source-validation.txt|
|User save protection|All discovered original JSON bytes and mtime equal before/after; only Captures QA stores/worlds written|user-save-before.json,user-save-final.json|

### Versions / scope / limitations

WorldSave13, Profile1, Protocol20, generation3, Item134/Recipe111 retained. No new wire field or network behaviour. Renamed class effect retains numeric slot28; allocations remain A2. Legacy Profile1 backup may retain the previous legacy payload until the normal following atomic write replaces it; no new per-kill durable ledger is created. Purchase ClassPurchases receipts are preserved.

No authenticated remote class entitlement backend exists. Default remote class progression is authoritative only for the current Host session; a client-side local profile copy is not accepted as proof in the next session. Real4-device/Relay/Android, physical touch target comfort, long sessions/performance, actual Ads/IAP are not verified in this pass. The final aesthetic, atmosphere, information comfort and gameplay feel require user judgment. Existing legacy inventory/crafting/settings/HUD visuals remain an audited future consistency opportunity. Boss/Undead King/newbiome/WBIV work was not started.

### Stale validation cleanup

Removed44 specifically listed stale files: HANDOFF, four intermediate/duplicate XMLs, two intermediate original-save snapshots, and unsuccessful old LAN runs. `removed-stale-artifacts.txt` is the exact list. Retained prior final LAN654feae46b104f1a9916c7ed040a5948, final-related/final-poi XML, final relevant PlayMode evidence, original before/final save hashes, timing, screenshots and historical REVIEW. Historical findings were not rewritten to claim the old release policy/UI was correct.

### Timing / usage / Git

Stage boundaries and account-wide usage snapshots: timing-usage.json. Measured process wall times: EditMode47.533s, PlayMode60.848s, LAN client45.821s. These background processes overlap editing and are not added to overall elapsed time. Other tool/compile waits were not exhaustively separated. Account snapshots are rounded shared-account usage and cannot attribute exact task tokens/cost.

Final local/remote SHA and clean state are confirmed after the main commit/push in the completion message and external completion record, avoiding a self-referential commit SHA inside this source document.
