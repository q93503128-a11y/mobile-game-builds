# Creature / Loot / Material / Recipe Expansion 1

시작 main: `252972006bdc6436cafb31147574f50e0c62a3cc`. 실제 `canonical` checkout에서 origin main fetch, HEAD 일치와 clean 상태를 먼저 확인했다. 조사·감사 시작: 2026-10-19 20:29:59 KST. 2026-10-20 재개 후 아래 실제 검수까지 완료했다. 구현 판정 A이며 실제 원격4인·모바일·재미 평가는 별도 검증 범위다.

## 구현 전 조사와 근거의 범위

2026-10-19 공개 자료를 조회했다. 개발사 문서 → 공식/커뮤니티 Wiki → Store 순으로 찾았다. Wiki의 공개 recipe/drop 설명을 관찰한 것이며 타 게임 내부 AI·저장 구현, 모든 아이템의 개수, 현재 버전 전체 경제를 측정한 것이 아니다. Grounded/Terraria 일부 페이지는 직접 열기 403이어서 검색에 공개된 해당 Wiki 본문을 확인했다. 확인하지 못한 내용은 결론의 근거로 쓰지 않았다. 검색 결과의 비공식 SEO Valheim 사이트는 서로 모순된 설명이 있어 제외했다. 수치·코드·아트·layout을 복제하지 않는다.

|게임|해당 시스템|실제 공개 사례|장점 / 관찰에서 얻은 해석|Lowfire와 차이|Lowfire 적용 여부|이유|
|---|---|---|---|---|---|---|
|Grounded|종 부품의 다중 sink|[Red Ant Part](https://grounded.wiki.gg/wiki/Ant_Part): Worker/Soldier 공용 Part가 Club·Armor뿐 아니라 Grinder/Spinning Wheel 제작에 사용. Head/Mandibles는 별도 재료다.|한 계통의 공급이 전투와 생산에 함께 연결됨|Lowfire는36칸이고 곤충 장비 계열이 적음|공유 재료·다른 역할 sink 원칙만 적용|종마다 머리/이빨/날개를 분리할 실제 수요가 없음|
|Grounded|전문 가공|[Resources](https://grounded.wiki.gg/wiki/Resources_%28Grounded%29): Grinder의 Mushroom Slurry, Jerky Rack의 Berry Leather, 여러 부품으로 Repair Glue. [공식 Home Stretch](https://grounded.obsidian.net/news/grounded/grounded-heads-into-the-home-stretch)는 Glue Masher 명칭을 확인.|가공은 실제 변환/수요가 있을 때 존재함|Lowfire는 장비 내구도·수리 sink 없음|독립 가공대·접착제 수리 loop 미채택|내구도가 없는 게임에 수리 경제를 억지로 추가할 이유 없음|
|Core Keeper|범용 생물 재료|[Larva Meat](https://corekeeper.atma.gg/en/Larva_Meat): 여러 Larva와 Caveling 계열 공급, Potion·Club·Grubzooka 및 음식에 사용.|종 이름 재료여도 여러 공급/용도가 있음|Lowfire는 기존 Raw Meat와 ranged 계열을 유지|다중 공급·서로 다른 최종 용도 채택|개체마다 전용 잡재료를 만들 필요 없음|
|Core Keeper|반복 소모와 탐험 보너스|[Healing Potion](https://corekeeper.atma.gg/en/Healing_Potion): Slime+Heart Berry로 제작하며 Abandoned Camp 등 chest에도 존재.|제작 가능 소비품을 탐험에서 얻으면 비용 절약 보상|Lowfire Camp에 실제 loot container가 없음|선택 소모품/탄약/가공재 보너스 채택|필수 기술을 RNG로 잠그지 않고 캠프 전투의 가치를 만듦|
|Core Keeper|음식 기능|[Cooking](https://corekeeper.atma.gg/en/Cooking): 두 재료의 효과를 조합, 동일 효과는 높은 쪽 사용. Bomb Pepper는 이동, Mushroom은 회복 사례.|음식의 선택이 Hunger 크기 외의 기능으로 구분됨|Lowfire는 전투 buff stack·음식 rarity 시스템 없음|소수 음식의 즉시 HP/Stamina 보충만 채택|재료 조합 전수·rarity·장기 buff framework는 이번 규모에 과함|
|Core Keeper|제련 단계|[Crafting](https://corekeeper.atma.gg/en/Crafting), [Smelter Kiln](https://corekeeper.atma.gg/en/Smelter_Kiln): Furnace는 Copper/Tin/Iron, Kiln은 Gold/Scarlet/Octarine, Fury Forge는 이후 광물.|단계 분리는 온도/재료군을 구별하는 기술 gate|Lowfire에는 III 이전 Steel이 고온 생산의 중심|기존 저단계/Steel 역할 유지|추가 광물 tier나 가공대 수를 맞추기 위한 filler 불필요|
|Valheim|종 전용·공유 부품|[Wolf Fang](https://valheim.fandom.com/wiki/Wolf_fang): Wolf/Fenring/Ulv가 공급하고 Spear와 Armor에 사용. [Crafting](https://valheim.fandom.com/wiki/Crafting): Wolf Pelt와 다른 가죽이 장비에서 구별됨.|여러 종 공유와 특정 생태 장비가 공존함|Lowfire에는 추위/모피 방한 역할이 없음|계통 공유 원칙 채택, Fur/전용 Tooth는 미채택|Leather와 역할이 겹치는 raw item만 늘어남|
|Valheim|Chitin|[Crafting](https://valheim.fandom.com/wiki/Crafting): Chitin이 Abyssal Harpoon과 Razor의 재료.|적은 sink여도 기능이 다른 두 장비면 정체성이 명확함|Lowfire에 Harpoon/바다 진행은 없음|Chitin의 방호·탄두 역할 채택|같은 이름의 도구/수치를 복제하지 않음|
|Valheim|고온 제련|[Blast Furnace](https://valheim.fandom.com/wiki/Blast_furnace): Black Metal Scrap/Flametal Ore를 Coal로 처리하며 Artisan Table 뒤에 해금.|고급로가 반드시 하급로 전체를 대체할 필요 없음|Lowfire는 Steel 하나가 다수 장비/부품/탄약의 중간재|고급로의 제한된 전문성 채택|Recipe1개라는 이유만으로 역할이 빈 것은 아님|
|Valheim|캠프·반복 공급 분리|[Loot Chest](https://valheim.fandom.com/wiki/Loot_chest): Plains 거점 chest에 Barley/금속 scrap/Coins/Needle 및 희귀 장비. [Fuling](https://valheim.fandom.com/wiki/Fuling): 거점 배치와 야외 낮밤 spawn을 별도로 설명.|거점 발견 보너스와 생태 재공급을 따로 설계 가능|Lowfire Camp 주민은 이미480초 보충됨|상자는 비재충전·주민은 기존 보충 유지|주민 재생을 상자 재추첨으로 연결하면 무한 보너스 생성|
|Don't Starve Together|치료 생물 재료|[Spider Gland/DST](https://dontstarve.wiki.gg/wiki/Spider_Gland/DST): 직접 회복·Healing Salve·Telltale Heart 용도.|단일 생물 재료가 즉시/가공 용도로 이어짐|Lowfire에서 revive 정책은 이번 범위 아님|공유 약용 재료와 반복 치료 sink만 채택|revive·별도 생명 체계를 가져오지 않음|
|Don't Starve Together|반복 방어 sink|[Hound's Tooth](https://dontstarve.wiki.gg/wiki/Hound%27s_Teeth): 여러 Hound 계열, Blow Dart/Sewing Kit/Tooth Trap에 사용.|재료가 장비 완성 후에도 소모 가능한 용도를 가짐|Lowfire는 durability나 Sewing 수요 없음|Bone의 탄약·함정·음식 연결 채택|단순 Trophy와 무의미한 종별 Tooth를 배제|
|Don't Starve Together|음식 tradeoff|[Pierogi/DST](https://dontstarve.wiki.gg/wiki/Pierogi/DST): Meat/Egg/Vegetable 조합, 비슷한 Bacon and Eggs와 Hunger/Health 역할이 다름.|동일 시설 안에서도 음식 선택을 만들 수 있음|Lowfire는 Egg/농축산/부패 없음|기존 Pot에서 회복/지구력 역할 분리|Egg·spoilage·새 농사 체계까지 확장하지 않음|
|Terraria|범용 Bone|[Bone](https://terraria.wiki.gg/wiki/Bone), [Necro Armor](https://terraria.wiki.gg/wiki/Bone_armor): Dungeon 재료가 Spinal Tap·Necro 장비·건축 쪽에 쓰임.|생물 재료가 한 장비가 아닌 여러 분기로 연결|Lowfire의 기존 Skeleton은 Stone/Iron을 주로 공급|Skeleton의 Bone 공급 채택|새 Bone armor tier로 Diamond 진행을 대체하지 않음|
|Terraria|탐험 보상|[Locked Gold Chest](https://terraria.wiki.gg/wiki/Locked_Gold_Chest): Dungeon의 잠긴 상자에 고유 loot와 열쇠 흐름.|거점 위험과 발견 보상을 연결|Lowfire는 핵심 진행을 희귀 상자에 의존시키지 않아야 함|발견 보너스만 채택, 고유 진행 열쇠는 배제|캠프를 못 찾아도 III 진행이 가능해야 함|
|Necesse|재료 구분|[Armor](https://necessewiki.com/Armor): Leather와 Wool이 서로 다른 장비군에 쓰이고, [Leather Dashers](https://necessewiki.com/Leather_Dashers)는 둘을 함께 사용.|다른 최종 기능이 있을 때 가죽/섬유를 구분할 근거가 됨|Lowfire에는 마법 직업/방한/방직 수요 없음|기존 Leather/Fiber 유지|Fur/Wool 분리는 수요를 먼저 만들지 않으면 중복 재료|
|Necesse|전문 Station 규모|[Crafting](https://necessewiki.com/Crafting): 기본 Alchemy Table에 회복/전투/이동/채굴/건축/낚시 Potion군과 Bottle. [Potions](https://necessewiki.com/Potions): Health Potion과 Regen 등 서로 다른 기능.|같은 작업을 묶는 실제 recipe군이 전문대의 이유|Lowfire 이번 치료 관련 두 기능은 Hand/Pot에 자연스럽게 들어감|새 Medicine Table 미채택|작은 수요를 위해 Alchemy 제작대·병 중간재·해금 비용을 추가하지 않음|
|Mine Survival|초기 재료·방어|[개발사 Guide](https://wildsoda.wordpress.com/): Bone이 Wood Trap/Wood Tower에 연결되고 Campfire는 Meat/Fish 조리. Creature list는 비선공/위험 야생생물을 구분.|생태·방어·생존이 초반부터 이어지는 구체적 사례|Lowfire에는 이미 Bronze Trap/Turret·Day/Night가 있음|Bone의 값싼 선택 방어와 기존 보존 채택|다른 게임 장비 tier/수치를 가져오지 않음|
|Mine Survival|제작·희귀 보상|[같은 개발사 Guide](https://wildsoda.wordpress.com/): Bandage는 제작, Medical Kit는 보석 상자에서 획득.|반복 제작 치료와 좋은 발견을 구분 가능|Lowfire는 건강 회복 소비품이 현재 없음|보급 상자에 제작 가능한 Salve 보너스|희귀 Medical Kit 전용 Item·필수 RNG gate는 만들지 않음|

## 변경 전 실제 source / sink / Station 감사

`baseline.json`은 연결된 Unity Data/Catalog를 읽기 전용으로 내보낸110 Item/91 Recipe/9 LootTable/12 자연 source 목록이다. `BASELINE_GRAPH.md`는 모든 Item에 대한 source → processing recipe → downstream recipe → final use와 Station별 목록을 추적한다. Prefab/코드와 baseline의 ID/경로를 비교했다. live 자산의 직렬화는 일부 기본0필드/공백/필드 순서를 더 기록하므로 단순 byte 차이를 수치 변경으로 해석하지 않는다. Recipe 목록과 재료 비용은 canonical YAML을 우선한다.

|Station|기존 Recipe수|역할 / 중복 / 공백|
|---|---:|---|
|Hand|4|첫 Workbench/불/용기/Torch. 야외 소모품 제작 공백|
|Workbench I|33신규, 누적33|Iron까지 초반 장비·시설·생활. 종류 밀집은 있지만 Category/Filter 존재|
|Workbench II|21신규, 누적54|Steel·Bronze 기술. 생물재료 sidegrade 연결은 얕음|
|Workbench III|11신규, 누적65|Diamond/탐험기술/공유 시설. 새 저단계 Recipe를 이곳에 더할 이유 없음|
|Campfire|5|Raw→Cooked·정수. 낮은 재료 부담의 현장 처리|
|Furnace|4|Iron/Copper/Tin/Bronze 금속 processing. 생물 재료를 넣을 물리적 필요 없음|
|Blast Furnace|1|Steel 전용. Steel이 장비/부품/시설/module/탄약에 지속 사용되므로 생산 역할은 큼|
|Cooking Pot|4|네 Stew가 Hunger/Thirst 숫자만 달라 기능적 선택이 약함|
|Turret Workbench|8|두 포탑/전용탄/다섯 module. 생물 방호재와 기존 Frame의 연결 후보|

Leather는15개 Recipe, Feather는4종 탄약 Recipe에 소비된다. Raw Meat는 세 Recipe의 직접 재료다. Wood는45개 Recipe와 upgrade, Bronze19개와 upgrade, Steel15개와 upgrade에 쓰인다(그래프의 upgrade edge는 일반 Recipe 수와 별도). Feather 공급은 Chicken 한 종뿐이다. Rabbit/Sheep는 Raw Meat만, Deer/Boar는 고기·가죽, Zombie는 Wood/Leather, Skeleton은 Stone/Iron이라 전투 보상과 자연 채집이 상당히 중복된다. Bone은 실제 Item이 없다. 기존 Ammo는 Recipe sink0이어도 발사라는 실제 sink가 있으므로 고아 재료로 세지 않았다. Cup의 상태 전환/빈 용기 반환, 업그레이드 결과인 Workbench II/III, Loot 장비 Rusted Sword도 고아가 아니다. Gold는 지갑이며 이번 Merchant를 만들지 않는다.

## 조사 후 설계 결정 — 아래는 Lowfire 자체 판단

각 행은 외부 사례 → 현재 문제 → 채택 → 대안 배제의 순서다. 신규 Material·Recipe·Station의 설계는 위 비교와 실제 baseline 산출 후 결정했다.

|결정|외부 사례|현재 Lowfire 문제|채택한 원칙 / 이번 설계|채택하지 않은 대안과 이유|
|---|---|---|---|---|
|추가 Material 수|Grounded는 부품 세분화, Core Keeper는 여러 공급의 Larva Meat, Terraria는 다목적 Bone|36칸 가방에 종별8개 잡템은 부담|raw Material은 Bone/Chitin/Medicinal Gland 세 종류만. 서로 다른 실제 sink를 먼저 확정|Fur·Hide·Bat Wing·Wolf Tooth·별도 Crystal 생물재료는 수요가 중복되거나 없음|
|종 전용 Material|Grounded Part는 Worker/Soldier 공유, Valheim Fang도 여러 종 공유|각 종마다 이름 다른 동일 기능 재료를 만들 위험|단일 종 전용 새 raw Material0. Bone은 Skeleton/포유류/Bat, Chitin은 두 Crawler, Gland는 Leech/Bat 공급|모든 생물을 한 Monster Essence로 합치면 서식지와 사냥 선택이 사라짐|
|Bone|Terraria 장비/건축, Mine Survival 방어, DST Tooth의 반복탄/함정|Skeleton Stone 보상의 정체성 약함|Bone Arrow의 반복 소비, 저비용 Bone Trap, Pot의 Bone Broth. Skeleton은 Bone 중심으로 변경|Bone Armor 풀 tier는 기존 Iron/Steel/Diamond와 역할 중복|
|Fur/Hide|Valheim Pelt 방한, Necesse Wool은 다른 장비군과 결합|Lowfire에는 해당 기능적 수요가 없음|기존 Leather·Fiber 유지, Goat/Wolf는 Bone 등으로 구분|Tannery/Fur raw item만 늘리는 가짜 가공 loop 배제|
|Chitin|Valheim Chitin의 서로 다른 두 장비, Grounded 계통 Part 다용도|Crawler가 Stone만 주면 자연 Stone과 경쟁|Carapace Vest(낮은 방어+Stamina회복), Barbed Bolt(반복), 기존 Reinforced Frame의 보조재|강제 Diamond 대체 tier·새 Carapace 가공 중간재·별도 Crawler Trophy 배제|
|약용/범용 생물 Material|DST Gland 치료, Core Keeper Slime+Berry Potion|직접 치료 소비품 부재와 음식의 단조로움|Gland→Hand Salve / Pot Restorative Broth 두 반복 용도|Zombie Essence/종별 Gland/포션 병 raw item은 이번 기능에 불필요|
|전문 Station|Grounded Grinder는 변환군, Necesse Alchemy는 다수 기능, Core Keeper Potion군|WB 누적65개지만 이번 새 immediate 조립은4개, 치료1·조리3개는 기존 Station에 맞음|새 Station0. Hand1/WB I2/WB II2/Campfire1/Pot2 추가. III 신규0. 기존 Turret Recipe1개 재료 보강|새 Medicine Table에 실제2개만 넣거나, Chitin1변환만의 Processing Station은 시설비/동선 대비 역할 부족. 미래 Recipe를 개수에 포함하지 않음|
|Furnace/Blast Furnace|Valheim 고급로 전용 재료, Core Keeper 단계별 금속군|Blast1개가 곧 빈 역할이라는 오해|Furnace4개와 Blast Steel1개 유지. 생물 가공은 원료 성질에 맞게 조리/조립|Bone ash/Crystal metal/Chitin steel을 recipe 수 채우기로 추가하지 않음. IV·신규 광물 없음|
|Cooking 범위|Core Keeper 음식 기능, DST Pierogi tradeoff, Mine Survival Raw/Cooked|Pot4개는 수치 크기만 달라 선택 약함|Bone Broth는HP, Restorative Broth는Stamina, Campfire Trail Ration은 휴대식. 기존 Mixed Stew는 소량HP를 보조. 즉시 회복만 사용|buff stack/rarity/spoilage/농축산/새 Cooking Station은 이번 범위를 넘음|
|Goblin Camp loot|Valheim biome chest·거점 배치, Core Keeper Camp의 제작 가능 Potion, Terraria 탐험 chest|Box/Junk Prop만 있어 전투 후 보상 약함|거주 적이 남아 있으면 확보 전 안내, 이후 take-only 실제 보급 상자. deterministic persistent ID/최초 내용+잔량 저장, 재충전 없음. 탄약·Salve·가공재와 희귀 Part 비용 절약|일반 창고 Deposit/상자 재생/필수 Diamond·기술 RNG/방문 kill quest gate는 배제|

## 최종 구현 · 현재 정본

Item110→121(신규 raw3+최종 사용 Item8), Recipe91→99, 야생 생물5→13이다. 종 전용 신규 raw0, 전문 Station0이다. 신규 코드·에셋·Pillow 원본을 canonical source에 직접 반영했다. 기존 메타 GUID와 첫5종 Rule index를 보존하고 종 ID별 runtime 행동 분기를 추가하지 않았다.

|신규 종|실제 habitat·행동|HP / 공격 피해|Drop / XP·Gold|
|---|---|---:|---|
|Bat|Caverns, 분리 날개/높이 비행, 경고→고정 방향 swoop→이탈|24 / 5|Gland1 75%, Bone1 40%; XP3/Gold0|
|Stone Crawler|Rocky/Deep, Stone과 닮은 휴면·균열·눈·호흡→경고→몸/다리 해제→물기|80 / 10|Chitin1, Stone1–2 70%; XP7/Gold0|
|Bog Leech|Wetlands 육지 water edge0.2–4, 전조→짧은 매복 도약, Leash5|28 / 6|Gland1; XP5/Gold0|
|Wolf|Forest,2–3마리 Pack, 가까운 같은 Group만 합류·수신 경보 재전파 없음|60 / 9|Bone1–2, Raw Meat1, Leather1 25%; XP8/Gold0|
|Mountain Goat|Rocky, 뿔/발굽과 hop, 평화·무리 도주|45 / 0|Bone1–2, Raw Meat1, Leather1 50%; XP0/Gold0|
|Crow|Grassland/ForestEdge, 분리 깃 날개·비행 도주·실제 처치/보상|14 / 0|Feather1 75%, Raw Meat1 25%; XP0/Gold0|
|Giant Boar|Rocky/바위 성분 높은 Forest 경계, 경고→준비→고정 방향 돌진→긴 회복|130 / 18|Raw Meat4, Leather2, Bone2; XP15/Gold0|
|Crystal Crawler|Deep CrystalPocket, 긴 분절·결정 돌기, 반경 예고→LOS pulse|95 / 12|Chitin2, Crystal1 15%; XP12/Gold0|

모든 신규 행동은 movement/attack/temperament/timing 데이터 조합이다. 같은 terrain·solid sweep를 사용하며 비행도 벽을 통과하지 않는다. 이동 구간에서 대상 Collider 표면까지의 거리로 접촉을 확인한다. LOS는 대상 몸을 제외하고 벽을 차단한다. Pulse는 여러 hurtbox가 있어도 대상당1회다. 각 경고/행동/회복 시간과 전체 drop은 [생물 정본](../../docs/CREATURES_AND_LOOT.md), [BALANCE](../../docs/BALANCE.md), [after.json](after.json)을 따른다.

### 기존 보상 교정

Chicken의 Feather1+Meat1과 Boar의 Leather1+Meat2 확정은 보존했다. Rabbit/Deer에 Bone 공급을 연결하고 Sheep는 Meat+Fiber, Zombie는 Wood/Leather 대신 Fiber/Bone, Skeleton은 Stone 중심 대신 Bone1–2+Iron Ore20%로 바꾸었다. 검 Skeleton의 Rusted Sword12%는 유지했다. Goblin은 기본 Bolt/Fiber/소량 Leather, 새총 Goblin은 Feather/Fiber/소량 Leather다. 모든18개 table의 실제 XP/Gold/확률/범위는 생물 정본에 기록했다.

### Material과 실제 Recipe

|새 raw|이번 실제 sink|공급 구분|
|---|---|---|
|Bone|Bone Arrow / Bone Trap / Bone Broth|Skeleton·포유류·Bat, 전투와 음식의 반복 수요|
|Chitin|Barbed Bolt / Carapace Vest / 기존 Reinforced Frame|Stone·Crystal Crawler, 반복 탄약·방호 sidegrade·모듈|
|Medicinal Gland|Healing Salve / Restorative Broth|Leech·Bat, HP와 Stamina를 구분한 반복 소비|

|신규 Recipe|출력|Station|실제 비용 / 기능|
|---|---:|---|---|
|Bone Arrow|12|Workbench I|Bone2+Wood1+Feather1, 기본 Arrow 대비+3피해|
|Bone Trap|1|Workbench I|Bone2+Wood3, HP40/피해10의 저비용 선택 방어|
|Barbed Bolt|8|Workbench II|Chitin1+Wood2+Feather1, 기본 Bolt 대비+6피해|
|Carapace Vest|1|Workbench II|Chitin4+Leather1+Fiber2, 방어3/Stamina회복+15%|
|Healing Salve|1|Hand|Gland1+Fiber2, HP25/소비 간격4초|
|Bone Broth|1|Cooking Pot8초|Bone2+Mushroom1+Raw Meat1, H24/T12/HP18/간격4초|
|Restorative Broth|1|Cooking Pot8초|Gland1+Mushroom1+Berry2, H22/T10/Stamina35/간격4초|
|Trail Ration|1|Campfire7초|Raw Meat1+Mushroom1+Berry2, H25/Stamina20/간격4초|

기존 Reinforced Frame은 Leather2→Leather1+Chitin2, Mixed Stew는 기존 허기/갈증과 HP8/4초 간격을 제공한다. 새 Armor는 철 Body 방어4보다 낮고, Bone/Barbed 탄은 Bronze/Steel 탄보다 낮아 상위 진행을 대체하지 않는다. Repeater는 기존 지정 기본 Bolt를 계속 소비한다. 전체99 Recipe와 처리시간은 [제작 콘텐츠](../../docs/CONTENT_EXPANSION.md)에 있다.

### Station 최종 감사

|Station|변경 전→후|구현 결론|
|---|---:|---|
|Hand|4→5|현장 연고1개 추가|
|Workbench I|33→35|Bone Arrow/Trap, 누적35|
|Workbench II|21→23|Barbed Bolt/Vest, 누적58|
|Workbench III|11→11|신규0, 누적69. Diamond/탐험·방어 기술 유지|
|Campfire|5→6|단순 가열/정수와 휴대식1개|
|Cooking Pot|4→6|복합 조리2개와 Mixed Stew 회복 지원|
|Furnace|4→4|Iron/Copper/Tin/Bronze 유지|
|Blast Furnace|1→1|Steel 지속 생산 역할 유지|
|Turret Workbench|8→8|Frame 재료 연결만 보강|

새 전문 Station은0개다. 생물 재료 별도 processing을 강제할 필요가 없고 Hand의 치료1개와 Pot의 기능 음식2개는 현재 시설의 실제 역할에 맞는다. 계획된 미래 recipe를 실제 개수에 보태지 않았다. Blast의 Recipe1개는 Steel 생산의 여러 downstream sink를 통해 역할을 수행한다. Furnace/Blast에 생물 재료나 filler 광물을 추가하지 않았다.

## Goblin Camp 실제 보급 상자

기존 Prop 위치에 상호작용 가능한 보급 상자·개봉 시각·take-only6칸 modal을 연결했다. Persistent ID는 Camp ID+:supplies다. 활성/휴면 거주 적이 남아 있으면 확보 전 안내를 한다. 살아 있는 유효 거리 Player만 열고 가져갈 수 있다. 일반 창고 Deposit/아이템 배치 기능은 없다.

|Slot|최초 보상|확률|Camp당 기대량|
|---|---|---:|---:|
|1|Bone Arrow8–16|100%|12|
|2|Salve1–2|100%|1.5|
|3|Bronze1–2|100%|1.5|
|4|Leather1–2|100%|1.5|
|5|Mechanical Part1|20%|0.2|
|6|Barbed Bolt8|35%|2.8|

기대량은 확률 산술이며 플레이 실측/보장 수량이 아니다. Seed+Camp ID로 최초 내용을 결정하고 initialized/opened/revision/잔여6칸을 저장한다. 재방문·거주 적480초 보충·Load·reHost는 상자를 재충전하지 않는다. Whole-stack 교환에 실패하면 양쪽 상태가 그대로다. Host가 request owner/sequence/가방 revision/상자 revision/생존/거리/공간/거주 적을 검사한다. Part는 Bronze1+Steel1→2로 계속 제작할 수 있고 Camp는 필수 진행 gate가 아니다.

## Save / Multiplayer / 비용 제한

Save10→11: Camp loot 상태와 Animal ExpandedRegions를 더했다. Save1–10의 기존 이관, generation1/2 terrain reader, 기존 ID/HP/Group/자원/시설/가방을 보존한다. 이전 Camp는 아직 초기화하지 않은 보급 상태로 이관하고 방문 때 최초 생성한다. 기존 방문 지역에는 새8규칙만 한 번 추가한다. Protocol12→13은 늘어난 catalog/prefab/creature frame과 Camp list 계약을 구분한다.

Host만 spawn/AI/target/damage/death/loot/recovery/claim한다. Client는 phase/server time/facing/awakening과 transform 표현만 수행한다. 새 Player NetworkBehaviour를 추가하지 않았다. Wildlife frame은 최대10Hz, HP 변경은 즉시 publish한다. 자동 계약·직렬화 검사와 실제 Local Host를 검증했다. 원격 Client의 실제 수신, 다중 기기 동시 claim/4인 Relay는 수행하지 않았다.

개별 expensive Update 대신0.05초/최대32 actor 공용 scheduler다. HardCap80 유지, 기존5종 목표46+새8종52=98은 동시에 보장하는 개체 수가 아니다. 새 cap10/8/6/8/6/6/3/5, recovery180/300/180/360/300/240/600/450초다. Sleep은 chunk index로 조회하고 생존 logical count는 birth region을 보존한다. Recovery는5초마다32 ticket/최대2 group/각24후보로 제한한다. 멀리 있는 개체는 simulation chunk를 유지하지 않는다. 고속 이동은 solid cast의 허용 거리까지 진행하고 terrain 경계 보정은 최대5회로 제한한다.

## 경제 검증 · 계산과 실측의 구분

[ECONOMY](ECONOMY.md), [economy.json](economy.json), [kit-costs.json](kit-costs.json)에1P/4P의 선택 root·공유/개인 분모와 비용을 남겼다. 필수 Diamond13/49, Crystal10/22는 유지하고 새 raw3종의 필수 progression 요구량은0이다. 기존 선택 방어기지까지의 원재료 변화는 공유 Frame2개의 Leather17→15/50→48, Chitin0→4/0→4다. 나머지 광물·목재 비용은 유지했다. 제작량 Bronze57/90, Steel58/118, Part14/14를 raw 비용에 중복 합산하지 않는다.

명시적 새 개인 kit(Bone Arrow96/Barbed64/Vest1/Salve4/Bone Broth2/Restorative2/Ration2)+공유 Bone Trap4의 새 비용은 Bone28/88, Chitin12/48, Gland6/24(1P/4P)다. 다른 재료와 전체 분모는 ECONOMY 표에 있다.4P는 개인 장비/소비품4배, 공유 시설과 Camp 보상1회로 계산한다.

Feather 직접 Recipe sink4→6, Leather15→16, Raw Meat3→5다. Chicken/Boar의 확정 보상을 보존하고 Crow는 확률 보조다. 자연 Crystal은 source당6/회복150초, Crystal Crawler는 처치당0.15기대량이라 기대값상40회 처치가6개다.40회 결과 보장·실제 사냥 시간은 측정하지 않았다. 새 생물은 Diamond/Part를 떨어뜨리지 않는다. Population/수요는 같은 세계 cap에서 경쟁하므로4인 파밍 속도가 정확히4배라고 가정하지 않았다.

## 자동 검사와 실제 Unity MCP 검수

### 자동 검사

|실행 artifact|결과|Runner 내부 시간|확인한 범위|
|---|---:|---:|---|
|edit-final.xml|66/66|14.1954487초|Catalog/Recipe/생태/경제/이관/네트워크 계약·직렬화|
|play-final.xml|21/21|125.2062553초|생물/소모품/장비/생태/저장/권위 관련 회귀|
|persistence-final.xml|12/12|150.7116684초|휴면/빈 슬롯/실제 modal/Host save·reHost|
|edit-resumed-final.xml|14/14|0.757413초|새 효과 유효성/Scene 계약, NaN·음수 거부|
|combat-resolved.xml|11/11|69.3546898초|6공격의 실제 피해/중복 hurtbox/벽 차단/전조와 기존 관련 회귀|

이 실행들은 일부 같은 test case를 포함하므로 합계를 고유 검사 수로 보고하지 않는다. 작은 변경마다 전체 회귀를 반복하지 않았다. 이전 실패 artifact는 보존하고 아래 수정 경위와 구분한다. 최종 ProjectDataValidator errors=[], Item121/Recipe99. validate_visual_project.py는 source/Landscape/**1256 unique GUID**/Sprite 참조/표시 텍스트 PASS. 새 script 동기화 뒤 실제 Unity console compilation error0을 확인했다.

### 실제 Unity MCP ·1920×1080·PlayUnfocused

- [live-sp-resumed.tsv](live-sp-resumed.tsv):8종 gallery와 실제 authored habitat·이동·위장/해제·전조/행동/회복·Pack·도주·사망/drop/pickup, 새8 Recipe의 실제 버튼/대기 queue/collect, 연고 Primary와4초 제한, Camp 거주 적/claim/replay/Save11/Load 모두 PASS. Fixture 이동·재료 지급은 자연 진행 실측과 다르다.
- [live-host-resumed.tsv](live-host-resumed.tsv):실제 Local Network Host 시작/실제 NetworkCamp6칸/owner claim/stale revision 거부/Save/Leave/reHost 후 opened/revision/잔여와 bag 보상1회 유지 PASS.
- [manual-resumed.json](manual-resumed.json):한·영 Camp UI, Full Bag 무변경 거부, 모든 잔량 획득 후 empty/no-refill, 거리 이탈 자동 닫힘과 세계 Running 유지. 장비 효과/재료 줄바꿈/Upgrade 비용을 확인했다.
- [live-combat-resolved.tsv](live-combat-resolved.tsv):싱글과 실제 Local Host에서 Bat5/Stone10/Leech6/Wolf9/Giant18/Crystal12의 HP 감소와 공격당1회 PASS. 첫 phase 전용 검수의 빈틈을 실제 피해 검사로 보완했다.
- [captures/resumed](captures/resumed):실제 캡처 원본. Gallery/실제 habitat 전투/제작/처리/상자/재Host/한·영/Full/Empty/피격을 포함한다. capture-index.json에 화면 크기를 기록했다. 모바일 터치 가독성까지 검증한 결과는 아니다.

### 발견한 오류와 직접 수정

1. 신규 catalog/종/Save 버전의 변경 전 숫자를 기대하던 자동 계약을 실제 상태로 갱신했다. SaveValidation의 실제 예외형과 spy count도 일치시켰다.
2. Camp 빈 rare slot/획득 뒤 null stack을 JSON으로 저장하면 복원에서 실패했다. 최초/소비 후 빈 SavedStack을 명시하고 active/dormant Camp capture를 분리 복사했다. 자동 persistence와 실제 Save/Load/reHost를 모두 재검수했다.
3. 휴면 Camp 경비를 확보 판정에서 누락하지 않도록 resident count와 chunk index를 유지했다. Population 부족 보충은 logical count/region을 보존한다.
4. 효과·방어 설명과 지역화된 재료/Take/빈 상자·공간 부족 안내를 실제 layout에 연결했다. Safe Area 중심 modal,6칸 take-only 의미와 거리 이탈 닫힘을 확인했다. 별도 전문대 UI를 만들지 않았다.
5. 최종 검토에서 LOS가 대상 solid Collider를 벽으로 세던 오류를 찾았다. 대상 몸만 제외하고 벽 차단은 보존했다. 돌진은 큰 step 전체를 취소해 접촉 전에 멈추던 오류가 있어 허용 거리까지 sweep하며 대상 표면 거리를 검사하도록 고쳤다. 최초 combat-final10/11 실패는 giant contact에서 발생했고 combat-resolved11/11와 실제 SP/Host6종 재검수로 해결했다.
6. 재개한 validator test의 Editor assembly 직접 참조는 compile 실패했다. 기존 test 방식의 reflection 경계로 수정한 edit-resumed-final14/14를 최종 결과로 사용했다.

원래 UserData2개(world-v1.json/backup)의 SHA256와 mtime_ns가 시작/중단/재개 전/최종 모두 동일하다. user-save-before-resume.json과 user-save-after-resume.json을 비교했다. QA는 Captures 전용 profile/repository만 사용했고 마지막 Play 종료·Host 종료·격리 profile 해제를 확인했다. Windows 입력/Focus/Show를 사용하지 않았다.

## 제한 / readiness / 다음 단계

**A — 이번 콘텐츠·저장·Host 권위 구현과 위 범위의 검수를 완료했다.** Undead King/Boss Foundation 설계를 이어갈 수 있다. Boss 자체는 이번에 구현하지 않았다. 이 판정은 출시·재미·실기기 통합 합격을 뜻하지 않는다.

원격 Client,4인 Relay,다중 기기 동시 claim·지연/packet loss,Android 빌드/FPS/배터리/메모리,장시간 생태 회복·경제·자연 사냥 분당 수율,실제 엄지 조작·전조 읽기·난이도/재미는 미검증이다. 다음 통합 QA에서 이를 실제 사용자와 확인한다. 새 framework/추가 station/material을 이 한계를 숨기기 위해 만들지 않는다. [RESUME](RESUME.md)은10월19일 중단 시점의 과거 기록이다. 현재 결과는 이 문서와 final-checks.json을 따른다.

## 작업 시간

- 조사·baseline·소스 작성:10월19일20:29:59→20:55:10 KST,25분11초.
- 초기 Unity asset/compile:20:55:10→20:58:32,3분22초. 내부 asset build24.265909초는 이 구간에 포함.
- 통합 구현·QA 수정·문서 초안:20:58:32→21:24:11,25분39초.
- 중단 정리:21:24:11→21:27:00,2분49초.
- 재개 실제 Play·경제/정본 문서·전투 correction·검수:10월20일17:26:01→17:49:37 KST,23분36초. Git 마감은 이후 별도 기록.
- 백그라운드 Test Runner/도구 대기는 위 작업 구간과 겹친다. 각 XML의 내부 시간은 검사 실행 측정이며 작업 시간에 다시 더하지 않는다. 전체 compile 대기를 별도로 합산하지 않았다.

- 최종 artifact 복사본의 줄 끝 공백을 제거했다. 실행 시각·결과·실패 메시지는 유지했으며 원래 live 원본 파일은 보존했다.
- 최종 diff/graph 보완/정리·Git staging:10월20일17:49:37→17:52:11 KST,2분34초. commit/push/remote SHA 확인 결과는 완료 응답에 기록한다.
