# Workbench I–II Content Expansion

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
|Iron|기존 비재생 광물→Furnace|Iron 장비·Cooking Pot·공유 시설·Steel|
|Copper/Tin|기존 채굴/수변 Panning→Furnace|Bronze|
|Bronze|Copper2+Tin1 가공→3|Reinforced Bow/Arrow/Crossbow/Lantern/보강 벽·문·대문/Trap/Brazier|
|Coal|기존 Stone Pickaxe 이상 비재생 광물|Torch/금속 가공/Steel/Lantern/Brazier|
|Steel|Blast Furnace에서Iron+Coal|기존 Steel 장비·선택 Steel Bolt|

기본 Arrow/Bolt는 Wood+Feather라 유한 광물 없이 계속 만들 수 있다. Feather1→기본12발이며 이번에 Chicken breeding을 요구하지 않는다. 고급탄은 선택 소비처다. 탄약은 가방 앞 슬롯의 맞는 종류를 먼저 사용한다. 고급탄을 우선 쓰려면 그 탄약 슬롯을 앞쪽으로 옮긴다. 탄약 선택을 새 무기 업그레이드와 혼동하지 않는다.

## 1인/4인 광물 예산

필수는 플레이어별 기존 Steel Axe/Pickaxe/Sword/Head/Body와 그 이전 단계, Prospecting Pan, 공유 Workbench 및 I→II/Blast Furnace다. 선택은 플레이어별 Reinforced Bow/Crossbow/Lantern/Bronze Arrow24/Steel Bolt16, 공유 Cooking Pot1/보강 Wall6/Door1/Gate1/Trap1/Brazier1이다. 모든 선택 시설 수백 개나 미래 III 비용을 보장하지 않는다.

|재료|1인 필수|1인 선택|4인 필수|4인 선택|1인 보장 목표|4인 보장 목표|Seed9472026 보장 공급|
|---|---:|---:|---:|---:|---:|---:|---:|
|iron_ore|34|4|121|10|57|197|198|
|copper_ore|4|20|10|38|36|72|72|
|tin_ore|1|10|1|19|17|31|33|
|coal|20|36|71|75|84|220|220|

필수/선택 수치는 recipe별 묶음 올림을 포함한 raw source 요구량이다. 보장 목표는 필수와 선택 각각1.5배 안전 여유와 올림을 적용한다. 한 번의 최적 일괄 가공보다 보수적일 수 있다. 실제 Recipe 그래프와 ProgressionPerPlayer/Shared 데이터를 읽어 계산한다. III 비용은 넣지 않았다.

기존 g/source ID와 위치를 바꾸지 않고 부족한 광물만 e<rule>:<index>의 결정적 보충 anchor로 추가한다. 보충은 기존 모든 배치 뒤에 추가하고 기존 자연 source를 밀어내지 않는 위치를 고른다. 기존 generation2 저장에도 같은 additive 경로를 적용한다. 구 generation1 저장은 신규 자연 source를 생성하지 않으며 이전 지형/ID를 유지한다. 광물 무한 재생은 없다.

Berry/Mushroom은 별도 SourceId salt로 기존 source 뒤에 생성한다. 기존 식생/광물의 seed stream과 ID를 유지하고, 기존 chunk materialization/recovery를 사용한다. 별도 chunk manager/물 tile별 Update는 없다.

## 저장과 네트워크

Save9 / Protocol10. v1~v8을 기존 이관 순서로 읽고9로 올린다. 이전 슬롯/드롭에는 charges0/Empty, Utility 없음이 기본이다. Waterskin은 한 item, MaxStack1, Charges0~3/Quality Empty·Raw·Clean을 저장한다. Empty는 반드시0charge다. 새로운 범용 RPG instance framework는 없다.

슬롯 이동/교환/상자/드롭·픽업/휴면/저장/시설 예약·반환은 동일 상태를 보존한다. 정수 중인 용기는 시설 cargo 안에 예약되며 완료 결과를 공간 부족으로 잃지 않는다. 가득 찬 Raw 물부대를 정수하고3회 마신 뒤 같은 item을 다시 채운다. Raw는 기존 Raw Cup의 불이익을 유지한다.

Utility 한 칸에 Lantern을 장착한다. 기존 equipment request/status와 PlayerSnapshot을 확장해 owner·remote 외형/광원/저장·재입장 상태를 전달한다. 기존 held/equipped light hook을 사용한다. 별도 매 frame 광원 메시지는 없다.

낚시 보상·탄약 소모·건축·함정 피해·물부대 상태·요리 예약은 Host에서 검증한다. 원거리 발사는 기존 StoneProjectile/NetworkProjectile에 player shot을 연결하며 NPC slingshot 경로를 보존한다. 시설/벽은 투사체를 막고 같은 팀에 피해를 주지 않는다. 물/낚시 요청은 위치·선택 장비·생존·세션과 phase/cooldown을 검증한다.

## 의도적인 제한

낚시 미끼/물고기 종류/낚시 레벨·음식 buff tree·추가 무기군·III/Diamond·전문 제작대·포탑·지도·자동 채광·새 biome은 없다. 도구 내구도/광원 연료 소비도 추가하지 않았다. 장기 음식/탄약/낚시 속도는 알파 수치다.

실제 검수와 한계는 CONTENT_EXPANSION_REVIEW, validation/content-expansion.json에 기록한다. QA는 Play 전에 별도 DevelopmentProfile을 선택해 저장 경로와 기본 설정/멀티 식별자를 격리한다. 실제 save/profile을 fixture로 읽지 않는다.
