# Workbench content expansion

아래 I–II 표는 2026-10-18 구현 기록이다. 현재 III 확장은 이 문서 마지막 절을 따른다.

2026-10-18. 기준318ddd6b14eb89bbc526c96239d23c8067de76e6. Lowfire 표시명/Wildfolk 내부명을 유지한다. 아이템50→86, Recipe38→71. Workbench III/Diamond/전문 무기·방어구·포탑 제작대는 미구현이다. 기존 런타임 위에 행동/선택지를 추가했으며 출시 밸런스나 실제 단말 합격이 아니다.

## 짧은 공개 자료 확인

- [Grounded 공식 Shroom and Doom](https://www.obsidian.net/news/grounded/shroom-and-doom-update-out-now): 공개 설명의 강한 단발/느린 장전 Crossbow 역할을 참고했다. Lowfire는 기존 Sword와 긴 좁은 Spear, 빠른 Bow, 느린 Crossbow 네 계열로 제한한다.
- [Core Keeper 공식 배포 패치](https://fireshinegames.co.uk/wp-content/uploads/Core-Keeper-Patch-notes-Post-1.0-1.0.0.16.pdf): 낚시 조작 선택지를 별도 개선하는 공개 사례를 확인했다. Lowfire는 대형 미니게임 대신 cast→대기→입질→한 번 입력을 선택했다.

이 문서는 공개 동작을 확인한 짧은 조사와 자체 결정을 구분한다. 게임의 비공개 알고리즘/수치/asset/code/layout을 복제하지 않았다. 신규 raster는 기존 Pillow 방식의 원본이며 Krita bridge는 이번 세션에서 연결되지 않았다.

## 현재 제작법

카테고리별 수: Tools10, Facilities8, Survival19, Armor6, Weapons14, Building14 (합계71).

Workbench I 표에는 I에서 새로 제공하는 Recipe, II 표에는 II 전용 Recipe만 기록한다. 실제 II 제작 화면은 I Recipe도 유지한다. 조건/NEW/필터/순서/재료 보유량은 기존 UI를 사용한다. 자원 생성은 제작대 해금과 독립이다.

### None

|Recipe ID|출력|재료|시간|
|---|---|---|---|
|campfire|campfire×1|wood×4 + stone×3|즉시|
|empty_cup|empty_cup×1|wood×2|즉시|
|workbench|workbench×1|wood×6|즉시|
|torch|torch×4|wood×1 + coal×1|즉시|

### Workbench I

|Recipe ID|출력|재료|시간|
|---|---|---|---|
|axe|axe×1|wood×5|즉시|
|furnace|furnace×1|stone×8 + wood×4|즉시|
|iron_axe|iron_axe×1|stone_axe×1 + iron_ingot×3|즉시|
|iron_body|iron_body×1|iron_ingot×3 + leather×1|즉시|
|iron_head|iron_head×1|iron_ingot×2 + leather×1|즉시|
|iron_pickaxe|iron_pickaxe×1|stone_pickaxe×1 + iron_ingot×3|즉시|
|iron_sword|iron_sword×1|stone_sword×1 + iron_ingot×3|즉시|
|leather_body|leather_body×1|leather×2 + wood×3|즉시|
|leather_head|leather_head×1|leather×1 + wood×2|즉시|
|pickaxe|pickaxe×1|wood×5|즉시|
|stone_axe|stone_axe×1|axe×1 + stone×4|즉시|
|stone_pickaxe|stone_pickaxe×1|pickaxe×1 + stone×4|즉시|
|stone_sword|stone_sword×1|wood_sword×1 + stone×5|즉시|
|storage_chest|storage_chest×1|wood×6|즉시|
|wood_door|wood_door×1|wood×3|즉시|
|wood_sword|wood_sword×1|wood×5|즉시|
|wood_wall|wood_wall×1|wood×2|즉시|
|compass|compass×1|wood×2 + iron_ingot×1|즉시|
|bow|bow×1|wood×5 + plant_fiber×4|즉시|
|arrow|arrow×12|wood×2 + feather×1|즉시|
|wood_spear|wood_spear×1|wood×4 + plant_fiber×2|즉시|
|stone_spear|stone_spear×1|wood_spear×1 + stone×4|즉시|
|iron_spear|iron_spear×1|stone_spear×1 + iron_ingot×3|즉시|
|fishing_rod|fishing_rod×1|wood×4 + plant_fiber×3|즉시|
|cooking_pot|cooking_pot×1|stone×6 + iron_ingot×2|즉시|
|waterskin|waterskin×1|leather×2 + plant_fiber×3|즉시|
|wood_fence|wood_fence×2|wood×3 + plant_fiber×1|즉시|
|wood_fence_gate|wood_fence_gate×1|wood×3 + plant_fiber×2|즉시|
|wood_gate|wood_gate×1|wood×6 + plant_fiber×2|즉시|
|stone_door|stone_door×1|stone×4 + wood×2|즉시|
|stone_fence|stone_fence×2|stone×4 + wood×1|즉시|
|stone_fence_gate|stone_fence_gate×1|stone×4 + wood×2|즉시|
|stone_gate|stone_gate×1|stone×7 + wood×3|즉시|

### Workbench II

|Recipe ID|출력|재료|시간|
|---|---|---|---|
|reinforced_chest|reinforced_chest×1|wood×8 + iron_ingot×2|즉시|
|stone_wall|stone_wall×1|stone×3 + wood×1|즉시|
|prospecting_pan|prospecting_pan×1|copper_ingot×2 + wood×2|즉시|
|blast_furnace|blast_furnace×1|bronze×3 + iron_ingot×3 + stone×8 + wood×4|즉시|
|steel_axe|steel_axe×1|iron_axe×1 + steel×3 + wood×2|즉시|
|steel_pickaxe|steel_pickaxe×1|iron_pickaxe×1 + steel×3 + wood×2|즉시|
|steel_sword|steel_sword×1|iron_sword×1 + steel×4 + leather×1|즉시|
|steel_head|steel_head×1|iron_head×1 + steel×2 + leather×1|즉시|
|steel_body|steel_body×1|iron_body×1 + steel×3 + leather×1|즉시|
|reinforced_bow|reinforced_bow×1|bow×1 + bronze×2 + leather×1|즉시|
|bronze_arrow|bronze_arrow×12|wood×2 + feather×1 + bronze×1|즉시|
|crossbow|crossbow×1|wood×6 + bronze×3 + leather×2|즉시|
|crossbow_bolt|crossbow_bolt×12|wood×2 + feather×1|즉시|
|steel_bolt|steel_bolt×8|wood×2 + feather×1 + steel×1|즉시|
|lantern|lantern×1|bronze×2 + leather×1 + coal×2|즉시|
|reinforced_wall|reinforced_wall×1|stone×3 + wood×2 + bronze×1|즉시|
|reinforced_door|reinforced_door×1|stone×3 + wood×2 + bronze×2|즉시|
|reinforced_gate|reinforced_gate×1|stone×5 + wood×4 + bronze×3|즉시|
|spike_trap|spike_trap×1|wood×4 + stone×2 + bronze×1|즉시|
|brazier|brazier×1|bronze×3 + stone×4 + coal×2|즉시|

### Campfire

|Recipe ID|출력|재료|시간|
|---|---|---|---|
|clean_water_cup|clean_water_cup×1|raw_water_cup×1|5s|
|cooked_meat|cooked_meat×1|raw_meat×1|5s|
|roasted_mushroom|roasted_mushroom×1|mushroom×1|5s|
|cooked_fish|cooked_fish×1|raw_fish×1|5s|
|purify_waterskin|waterskin×1|waterskin×1|10s|

### CookingPot

|Recipe ID|출력|재료|시간|
|---|---|---|---|
|mushroom_stew|mushroom_stew×1|mushroom×2 + berry×2|7s|
|meat_stew|meat_stew×1|raw_meat×1 + mushroom×1 + berry×2|8s|
|fish_stew|fish_stew×1|raw_fish×1 + mushroom×1 + berry×2|8s|
|mixed_stew|mixed_stew×1|raw_meat×1 + raw_fish×1 + mushroom×1 + berry×2|10s|

### Furnace

|Recipe ID|출력|재료|시간|
|---|---|---|---|
|iron_ingot|iron_ingot×1|iron_ore×1|2s|
|copper_ingot|copper_ingot×1|copper_ore×1 + coal×1|3s|
|tin_ingot|tin_ingot×1|tin_ore×1 + coal×1|3s|
|bronze|bronze×3|copper_ingot×2 + tin_ingot×1|5s|

### BlastFurnace

|Recipe ID|출력|재료|시간|
|---|---|---|---|
|steel|steel×1|iron_ingot×1 + coal×1|6s|

Purify Waterskin은 **가득 찬 Raw3/3 하나만** 예약하며 결과는 Clean3/3이다. TSV의 일반 재료 ID만 보고 빈/부분/정수 물부대도 투입 가능하다고 해석하지 않는다. Campfire와 Cooking Pot은 동일 StationProcessing/ProcessingQueue를 사용한다. Campfire의 기존 batch와 Cooking Pot의 sequential 보관 모드는 데이터로 구분한다.

## 재료 source/sink

|재료|공급|지속 또는 주요 소비|
|---|---|---|
|Wood|Tree/Fallen Log|초반 도구·무기·낚싯대·Arrow/Bolt·건축|
|Plant Fiber|기존 Small Plant,2개 드랍|Bow/Spear/Rod/Waterskin/Fence|
|Berry|Grassland/Forest renewable bush,3개 드랍|초반 음식·4종 Stew|
|Mushroom|Forest/일부 Cave renewable source,2개 드랍|간단 조리·4종 Stew|
|Raw Fish|낚싯대와 유효 자연 수면|Cooked Fish/Fish Stew/Mixed Stew|
|Stone|기존 지상 자원|초반 장비·Cooking Pot·Fence/Gate·보강 건축|
|Leather|기존 동물 loot|Armor/Waterskin/Reinforced Bow/Crossbow/Lantern|
|Feather|기존 Chicken loot,1개|Arrow12 또는 Bolt12의 묶음 제작,고급탄|
|Iron|지역 회복 광물→Furnace|Iron 장비·Cooking Pot·공유 시설·Steel|
|Copper/Tin|기존 채굴/수변 Panning→Furnace|Bronze|
|Bronze|Copper2+Tin1 가공→3|Reinforced Bow/Arrow/Crossbow/Lantern/보강 벽·문·대문/Trap/Brazier|
|Coal|Stone Pickaxe 이상 지역 회복 광물|Torch/금속 가공/Steel/Lantern/Brazier|
|Steel|Blast Furnace에서Iron+Coal|기존 Steel 장비·선택 Steel Bolt|

기본 Arrow/Bolt는 Wood+Feather라 유한 광물 없이 계속 만들 수 있다. Feather1→기본12발이며 이번에 Chicken breeding을 요구하지 않는다. 고급탄은 선택 소비처다. 탄약은 가방 앞 슬롯의 맞는 종류를 먼저 사용한다. 고급탄을 우선 쓰려면 그 탄약 슬롯을 앞쪽으로 옮긴다. 탄약 선택을 새 무기 업그레이드와 혼동하지 않는다.

## 1인/4인 광물 예산

현재 예산 정본은 [RESOURCE_TEMPO](RESOURCE_TEMPO.md)다. 공유 기초 시설/Compass/Cup까지 필수 root에 포함하고 Bronze batch 잔량을 재사용한다. 선택 포함1인 Iron39/Coal50/Copper20/Tin9,4인135/140/44/18이다.4인×1.5 목표203/210/66/27,기존 배치의 현재 수율 공급396/550/108/55다. 위 recipe 표의 비용은 바뀌지 않았다.

g/c/e의 기존 ID·위치를 고정하는 layout baseline과 실제 recipe 경제를 분리했다. Iron/Coal/Copper/Tin은 해당 환경의 지역 deficit가 게임 시간 뒤 새로운 p ID와 위치로 회복된다. cap/예약/복원은 RESOURCE_TEMPO를 따른다.

Berry/Mushroom은 별도 SourceId salt로 기존 source 뒤에 생성한다. 기존 식생/광물의 seed stream과 ID를 유지하고, 기존 chunk materialization/recovery를 사용한다. 별도 chunk manager/물 tile별 Update는 없다.

## 저장과 네트워크

Save9 / Protocol11. v1~v8을 기존 이관 순서로 읽고9로 올린다. 이전 슬롯/드롭에는 charges0/Empty, Utility 없음이 기본이다. Waterskin은 한 item, MaxStack1, Charges0~3/Quality Empty·Raw·Clean을 저장한다. Empty는 반드시0charge다. 새로운 범용 RPG instance framework는 없다.

슬롯 이동/교환/상자/드롭·픽업/휴면/저장/시설 예약·반환은 동일 상태를 보존한다. 정수 중인 용기는 시설 cargo 안에 예약되며 완료 결과를 공간 부족으로 잃지 않는다. 가득 찬 Raw 물부대를 정수하고3회 마신 뒤 같은 item을 다시 채운다. Raw는 기존 Raw Cup의 불이익을 유지한다.

Utility 한 칸에 Lantern을 장착한다. 기존 equipment request/status와 PlayerSnapshot을 확장해 owner·remote 외형/광원/저장·재입장 상태를 전달한다. 기존 held/equipped light hook을 사용한다. 별도 매 frame 광원 메시지는 없다.

낚시 보상·탄약 소모·건축·함정 피해·물부대 상태·요리 예약은 Host에서 검증한다. 원거리 발사는 기존 StoneProjectile/NetworkProjectile에 player shot을 연결하며 NPC slingshot 경로를 보존한다. 시설/벽은 투사체를 막고 같은 팀에 피해를 주지 않는다. 물/낚시 요청은 위치·선택 장비·생존·세션과 phase/cooldown을 검증한다.

## 의도적인 제한

낚시 미끼/물고기 종류/낚시 레벨·음식 buff tree·추가 무기군·III/Diamond·전문 제작대·포탑·지도·자동 채광·새 biome은 없다. 도구 내구도/광원 연료 소비도 추가하지 않았다. 장기 음식/탄약/낚시 속도는 알파 수치다.

실제 검수와 한계는 CONTENT_EXPANSION_REVIEW, validation/content-expansion.json에 기록한다. QA는 Play 전에 별도 DevelopmentProfile을 선택해 저장 경로와 기본 설정/멀티 식별자를 격리한다. 실제 save/profile을 fixture로 읽지 않는다.


## Workbench III / Deep Caverns / Turret foundation (2026-10-19)

시작 정본821e493d637e2f25ba94ab843cc9737539cb9ee5. 직전 통합 감사의 readiness A와 source를 확인한 뒤 진행했다. 표시명 Lowfire, 내부 Wildfolk 유지. III는 현재 alpha 범위의 최고 활성 단계이며 최종 tier가 아니다. IV–VI/보스/Defense Objective/전기·물류/총기/별도 Weapon·Armor Workbench는 추가하지 않았다. 현재 item110/recipe91, Save10/Protocol12, 새 월드 generation3다.

### 구현 전 데이터 확인과 조사

MCP로 기존86개 Item과71개 Recipe, Tool/Armor/Ranged/Placement 필드를 내보내고 실제 비용을 확인했다. Steel Pickaxe/Axe는 damage18, Sword43, Head3/Body5 defense. Crossbow31damage/19range/1.55초 주기, 기본 Bolt100stack이다. 기존 Steel Spear는 없어 같은 Spear 계열 안에서 Iron Spear를 소비하는 Steel 중간 단계를 추가했다. 기존 Enemy HP나 I–II recipe/resource tempo는 변경하지 않았다.

- [Subnautica Alterra Science](https://unknownworlds.com/en/news/alterra-science-update-released): 기존 Knife/Tank/Fins를 Workbench에서 변경하는 사례, 이름으로 Beacon을 구별하는 사례를 확인했다. 기존 Steel/Lantern 소비와 식별 가능한 비콘 이름에 적용했다.
- [Grounded Bugs Strike Back](https://grounded.obsidian.net/news/get-ready-as-the-bugs-strike-back): 플레이어가 탑승해 운용하는 두 turret의 역할 차이와 PEEP.R 관찰을 확인했다. Lowfire는 수동 조준·장전·발사를 기본 기능으로 두고 별도 위험 활성 없이 원거리 관찰을 제공한다.
- [Grounded Super Duper](https://grounded.obsidian.net/news/grounded/update-1-2): turret ammunition 추가 사례를 확인했다. Lowfire는 Ballista 전용탄과 기존 Bolt 재사용을 구분하고 Ammo Box도 실제 stack을 보관한다.
- [Minecraft render/simulation guide](https://learn.microsoft.com/en-us/minecraft/creator/documents/simulationrenderdistanceguide?view=minecraft-bedrock-stable): 렌더 범위와 AI/spawn/game tick 범위가 다른 개념임을 확인했다. Telescope 표시와 포탑 전투 휴면 경계에 적용했다.
- [Valheim FAQ](https://www.valheimgame.com/faq/): 탐험·재료·제작·건축과 협동 loop를 참고했다. 희귀 pocket 발견 보상, 산업 chest의 작은 bonus, 1인 개인 장비와 공유 시설의 비용 분리는 Lowfire의 자체 설계 판단이다.

관찰과 자체 판단을 구분한다. 외부 게임의 내부 타기팅/LOS/저장 알고리즘을 확인했다고 주장하지 않는다. Auto Aim의 살아 있는 적·아군 제외·LOS, 고정 역할 module, 초기4인 공급/입구 경로 검증은 이번 Lowfire 요구에 맞춰 설계했다. 타 게임 asset/code/layout은 사용하지 않았다. 새 sprites는 기존 Pillow 방식이다.

### III 재료와 비용

II→III는 Wood10+Stone10+Bronze4+Steel4+Crystal4+Diamond1만 요구한다. 방문·kill·quest 체크는 없다. I→II 기존 비용은 유지한다.

|출력|소비|역할/수치|
|---|---|---|
|Diamond Pickaxe|Steel Pickaxe+Diamond2|동일 damage18/속도·거리, 채집 효율 약+22%|
|Diamond Axe|Steel Axe+Diamond1|동일 damage18/속도·거리, 채집 효율 약+22%|
|Diamond Sword|Steel Sword+Diamond2|damage52, 속도·거리 유지|
|Diamond Spear|Steel Spear+Diamond2|damage42, 속도·거리 유지|
|Diamond Head|Steel Head+Diamond2|defense4|
|Diamond Body|Steel Body+Diamond3|defense6|
|Steel Spear|Iron Spear+Steel3+Leather1|II,damage35; 기존 Iron 타이밍/거리|
|Mechanical Part×2|Bronze1+Steel1|III 제작이 주 공급; Coal 직접 추가 없음|
|Telescope|Crystal2+Bronze1+Leather1|가방/선택 슬롯에서 사용, Utility 차지 없음|
|Crystal Lantern|Lantern+Crystal2+Bronze1|기존 Utility, 반경+35%, 공격 손 유지|
|Signal Beacon|Crystal2+Bronze1+Mechanical Part1+Wood2|팀 공유 설치/회수/저장|
|Turret Workbench|Wood8+Stone8+Bronze4+Steel4+Crystal2+Mechanical Part2|III에서 제작; 현재 빈 상위 tier UI 없음|
|Ballista|Wood10+Steel3+Leather2+Mechanical Part1|전용 Bolt,62damage/26range/2.3초/45도·초,HP180|
|Repeater|Wood8+Bronze3+Steel2+Mechanical Part2|기존 Crossbow Bolt,16damage/17range/.55초/105도·초,HP160|
|Ballista Bolt×6|Wood2+Steel1|Turret Workbench|
|Auto Loader|Mechanical Part2+Steel1+Bronze1|저장 탄약 자동 장전|
|Auto Aim|Mechanical Part1+Crystal2+Bronze1|Host의 유효 적 추적/LOS|
|Ammo Box|Wood4+Steel1|기본1칸→3칸, 각 탄약 기존100stack|
|Rotation Gear|Mechanical Part1+Bronze2|회전 속도+35%|
|Reinforced Frame|Steel2+Leather2|최대HP+50%; 즉시 수리 효과 없음|

Diamond 개인6종은12개, 공유 III 업그레이드를 포함해1인13/4인49개다. 장착한 Steel은 가방으로 해제한 뒤 재료로 소비한다. 장착 상태의 아이템을 몰래 재료로 소모하지 않는다. Ballista 관통은 이번에 추가하지 않았으며 단발·장거리·느린 추적 역할로 구분한다.

### Deep Caverns와 초기 공급

새 generation3는 기존 지형/동굴 graph/기존 g·c·e source를 유지하고 독립적인 iii ID와 새 source stream만 추가한다. generation1/2 저장은 해당 버전으로 계속 읽으며 새 natural source/POI를 소급 생성하지 않는다. 기존 저장에 새 아이템을 보관하거나 시설을 설치할 수 있지만 자연 III 진행은 새 generation3 월드가 필요하다. 강제 재생성·wipe는 없다.

Diamond: Deep Caverns의 pocket 중심, depth .75+, Steel Pickaxe+,source당3,회복300초,target ratio.70. Crystal: 지하 depth .6+,source당6,회복150초,target.85. Surface Diamond/Crystal 공급은 없으며 surface Diamond에 의존하지 않는다. 새 위치/pID로 지역 회복하고 비활성 청크에 GameObject를 생성하지 않는다. 기존 Stone6/75,Iron6/120,Coal10/105,Copper6/150,Tin5/165/Pan 규칙은 유지한다.

Crystal Pocket/Diamond Pocket/Deep Mine/Abandoned Workshop을 표시한다. 산업 지역의 실제 상자만 작은 bonus를 담는다. Mechanical Part는 확률 bonus1~3이며 일반 몹·평범한 상자·맨바닥 자연 spawn에 추가하지 않았다. POI를 찾지 못해도 Bronze+Steel 제작으로 진행 가능하다. 새 적 종은 추가하지 않았다.

새 월드의 보장 deep source는 Diamond27개/81개 yield, Crystal16개/96개 yield다. Diamond는3개 cave pocket에 분산해 어느 하나를 제외해도4인 기본 필요49개를 확보할 수 있도록 보강했다. 초기 공급·geographic distribution·surface entrance A*·동굴 graph 경로와 source까지의 지형 연결을 검사하며 회복을 초기 부족분 보충 수단으로 쓰지 않는다. 검사 Seed와 최종 결과는 validation/tier-three를 따른다.

### 경제: 기본 진행과 대표 선택 방어기지

현재 Recipe graph에서 shared/personal/optional을 나눠 batch 잔여량을 재사용한다. 기존 I–II까지의 초기 도구·시설과 III 개인6종/Telescope/Crystal Lantern,공유 III·Turret Workbench를 포함한다. 선택 시나리오는 Ballista1+Repeater1+Beacon1+각 module2+Ballista Bolt60+기본 Bolt120 및 기존 선택 root다. 이 시나리오를 모든 플레이어의 필수 비용으로 해석하지 않는다.

|최종 raw material|1P|4P|
|---|---:|---:|
|Wood|188|332|
|Stone|98|149|
|Iron Ore|83|197|
|Coal|121|226|
|Copper Ore|40|68|
|Tin Ore|19|30|
|Diamond|13|49|
|Crystal|16|28|
|Leather|17|50|
|Feather|14|26|
|Plant Fiber|6|24|

Bronze/Steel/Mechanical Part의 제작 중간 합계와 그룹별 raw 비용은 audit.tsv에 별도로 기록한다. 위 raw 합계에 중간재를 다시 더하지 않는다. Steel/Bronze는 새 장비·부품·시설·모듈·탄약을 통해 계속 소비되고, Feather는 기존 Bolt에 남는다. Ballista Bolt는6발마다Steel1+Wood2,기본 Crossbow Bolt는12발마다Wood2+Feather1이다.

### Telescope / Navigation / 포탑 구조

Telescope는 공격·채집·이동·dash·상호작용·설치를 막고 드래그/aim으로 관찰 방향을 바꾼다. 접기/다시 사용/피격으로 종료한다. 카메라 offset은64unit chunk의2.5개 앞이다. 기존 WorldChunkRuntime/WorldTerrainRenderer에 표시 전용 root를 요청하며 카메라 footprint에 필요한 최대9개만 허용한다. AI/자원/물리/시설/생물 활성 경로를 호출하지 않는다. 종료3초 후 root를 반환하고 기존 지형 LRU24를 사용한다. 별도 월드 streaming manager는 없다.

비콘 추적은 기본 OFF다. 기존 Compass의 Home/Exit 자동 안내는 유지하며 Navigation에서 Off/Home/Exit/Beacon 중 하나를 고른다. Beacon은 순번 이름을 저장하고 한번에 한 대상의 방향/거리/짧은 이름만 표시한다. 강제 설치 개수 제한은 없다. team registry metadata는 변화가 있을 때 갱신하며 멀리 있는 비콘도 표시를 위해 시뮬레이션하지 않는다.

포탑은 기존 WorldPlaceable/InventoryModel/StoneProjectile/NetworkProjectile를 사용한다. 공용 staggered scheduler가16개까지의 활성 포탑을 tick하며 각 포탑의 검색은 최대64개 근처 collider와32개 LOS hit로 제한한다. 자원/포탑마다 Update를 추가하지 않았다. Loader/Aim/Ammo/Rotation/Frame 슬롯당 한 module이며 상위 module 교체가 가능하다. 확장 탄약이 남으면 Ammo Box 분리를 거부한다. module 제거/교체는 가방 공간을 검사한다.

수동 운용은 드래그 조준→탄약 삽입→장전→발사다. Loader만 있으면 수동 조준/발사, Aim만 있으면 추적과 수동 장전/발사, 둘 다 있으면 실제 투사체와 탄약·cooldown을 이용한 자동 사격이다. Host만 타깃/발사/소모/피해를 결정한다. 원격은 owner request와 변경 state/최대10Hz heading만 전달한다. 시뮬레이션 청크 밖에서는 추적·발사·탄약 소비를 하지 않고 HP/모듈/탄약/방향/재장전 상태만 저장한다. 새 save file을 만들지 않는다.

### Gamefeel / UI / 검증 범위

새 화면은 기존 Canvas/SafeArea/서체/색과 제작 카드를 확장한다. 긴 비용은 have/need와 disabled 상태로 표시한다. 최초 실제 Play에서 새 카드가71개로 남은 문제와6개 upgrade 재료의 가림을 확인해 카드 추가와 우측 높이를 교정했다. 별도 shake/hitstop/zoom/VFX stack이나 placeholder 소리는 추가하지 않았다. 포탑의 장전/대기/빈 탄약, module 설치, scope 종료, 비콘 선택은 각각 즉각적인 상태 변화와 짧은 조작으로 전달한다.

검사(compile/자동/static)와 실제 테스트(MCP Play/Game View)는 validation/tier-three의 결과를 따른다. 실제 OS 터치, 원격4인/Relay, 모바일60fps/장시간 전투·재미는 별도다. 단일 Editor 수치를 출시 성능이나 밸런스 합격으로 해석하지 않는다.
