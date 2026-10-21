# Content Expansion 2-F · Base / Defense / POI

## 구현 전 조사와 결정 · 2026-10-21

시작 remote main `1f355614a5a6849f0f009b1199ca383a5022f43d`, main/clean 확인 후 감사했다. Item134/Recipe111/Save11/Protocol17/generation3. 아래 결정은 runtime 수정 전에 기록했다. 공개 문서 조사이며 타 게임을 직접 플레이한 검수는 아니다. wiki.gg/Fandom 일부 본문은403/robots 차단으로 검색 색인에 노출된 구체 본문을 확인했다. 수치/배치/asset/코드를 복제하지 않는다.

## 외부 비교

|게임|해당 시스템|실제 사례 / 출처|장점|Lowfire와 차이|적용 여부|이유|
|---|---|---|---|---|---|---|
|Grounded|수리|[Repair Tool](https://grounded.wiki.gg/wiki/Repair_Tool): 타격으로 시설40HP 회복, 도구도 마모|방어 후 복구 역할|Lowfire 도구 내구/수리 입력 없음|후속|도구 하나 때문에 내구 framework를 추가하지 않음|
|Grounded|접촉 함정|[Spike Strip](https://grounded.wiki.gg/wiki/Spike_Strip): 플레이어도 피해, 공격마다 마모|무한 수동 방어에 비용|Lowfire Team 피해 OFF|사용 소모 원칙만|friendly fire를 갑자기 열지 않음|
|Grounded|적의 시설 공격|[Factional Raids](https://grounded.wiki.gg/wiki/Factional_Raids): 접근 가능한 기지/건축물 조건, 적들이 기지 파괴|경고·접근성 조건|Lowfire 일반 추적 중 barrier 공격|새 raid 배제|강제 wave/방어 objective 범위 밖|
|Grounded|point defense|[MIX.R](https://grounded.wiki.gg/wiki/MIX.R): 플레이어 시작, 약4분 장치 방어, 실패 후 수리/재시도|준비→전투→보상|Lowfire objective 없음|배제|이번 POI는 작은 탐험 장소|
|Grounded|반복 utility|[Dew Collector](https://grounded.wiki.gg/wiki/Dew_Collector): 이슬 최대4, 생성 간격, 만량이면 중지|회복 속도와 저장량 분리|Lowfire 강물은 무제한 raw|Well 원칙 채택|cap3/480초, 새 제작 시설 없음|
|Core Keeper|방어/안전|[Spike Trap](https://corekeeper.atma.gg/en/Spike_Trap): Tin Workbench, 적만 작동,2.5초 간격; 일부 적이 함정 공격|아군 안전·통로 활용|Lowfire 함정1.2초/팀 판정|기존 안전 유지|회로/새 tier 불필요|
|Core Keeper|자동화|[Automation](https://corekeeper.atma.gg/en/Automation): 함정/포탑+Robot Arm/Conveyor 회수|작업 분담|Lowfire 이미 Loader+Aim 자동화|기존 유지|전기·자동 loot 확대 배제|
|Core Keeper|유한 광맥|[Mining](https://corekeeper.atma.gg/en/Mining):2×2 Ore Boulder는 drill 전용, 유한1800광석|눈에 보이는 장소 자체가 보상|Lowfire 손도구/작은 node|Quarry 유한 기존 node|대량 광맥/전기/원격 채굴 배제|
|Core Keeper|다른 POI 기능|[Scenes](https://corekeeper.atma.gg/en/Scenes): Maze 중앙 보상, Harbor의 Vending Machine 등|장소마다 다른 행동|Lowfire6종 landmark만 있음|행동 차이만|상인/퀘스트/고유 필수템 배제|
|Valheim|barrier|[Stakewall](https://valheim.fandom.com/wiki/Stakewall):1000HP,Wood4,접촉 피해 없음|벽과 함정 분리|Lowfire벽100/220/360|기존 역할 유지|wall damage를 얹지 않음|
|Valheim|소모 함정|[Sharp stakes](https://valheim.fandom.com/wiki/Sharp_stakes): 접촉 피해/knockback, 타격마다 내구 소모, 아군도 위험|병목에도 유지비|Lowfire 두 함정 사실상 동일|Bone 일회성 burst|복잡한 reset/내구 단계 대신 기존 Health 사망 재사용|
|Valheim|수리|[Building](https://valheim.fandom.com/wiki/Building): Hammer와 해당 제작 시설 범위로 건물 수리|기지 유지 행동|Lowfire 수리/철거 없음|빈칸 기록·보류|POI 우선, 별도 유틸 설계 없이 임의 무료수리 추가 안 함|
|Valheim|자동 포탑|[Ballista](https://valheim.fandom.com/wiki/Ballista): trophy로 목표 종류 제한|자동 방어의 목표 통제|Lowfire Aim 모듈/아군 안전 이미 있음|현행 유지|세번째 포탑/추가 필수 모듈 배제|
|Valheim|guarded reward|[Burial Chambers](https://valheim.fandom.com/wiki/Burial_Chambers): 내부 Skeleton/Ghost와 재생 안 되는 보상, Surtling Core|위험과 보상 연결|Core는 주요 제작 progression|Graveyard 소규모 기존 Skeleton|필수 RNG 재료/던전 복제 배제|
|Necesse|수동/자동 함정|[Dungeon Arrow Trap](https://necessewiki.com/Dungeon_Arrow_Trap):35피해/13tile/1초, 배선 활성화; [Traps](https://www.necessewiki.com/Traps)는 플레이어 포함 entity를 손상|발동 경로가 명시적|Lowfire 배선 없음|기존 Host trigger만|전기·회로·friendly fire 배제|
|Necesse|노력 후 보상|[Trial rooms](https://necessewiki.com/Trial_rooms): 동굴 계단을 통해 memory/trap/portal trial, biome상자2개+coins|상자 자체보다 선행 행동|Lowfire surface anchor|작은 guard/채광 행동|새 trial/실내/던전 framework 배제|
|Necesse|entrance identity|[Dungeon](https://necessewiki.com/Dungeons): 지상 입구 guard, 내부2층/고유 적/장비|입구와 내부의 연결|Lowfire Mine는 입구가 아님|Mine 보류|CaveEntrance와 혼동시키지 않음|
|DST|유지비/병목|[Tooth Trap](https://dontstarve.wiki.gg/wiki/Tooth_Trap/DST):60피해/10회/수동 reset, [Hound guide](https://dontstarve.wiki.gg/wiki/Guides/Hound_Wave_Survival_Guide)는 trap corridor와 reset 부담 설명|반복 방어에도 행동 비용|Lowfire reset UI 없음|Bone burst / Spike 반복 구분|새 reset 시스템과 강제 wave 배제|
|DST|자원 장소|[Reeds/DST](https://dontstarve.wiki.gg/wiki/Reeds/DST): Reed Trap은 빽빽한 갈대와 Tentacle 위험, 갈대 재생|지형 자원과 위험 자체가 reward|Lowfire quarry는 전투 필수 아님|현지 채광 원칙|모든 자원 POI에 guard 강제 안 함|
|Terraria|함정 safety/주기|[Traps](https://terraria.wiki.gg/wiki/Trap), [Dart Trap](https://terraria.wiki.gg/wiki/Dart_Trap): 회로·pressure plate, dart는 무탄약/재사용 간격, 자연 함정 위험|위험을 배치로 읽음|Lowfire 탄약 경제 이미 있음|발동 가독성만|무한 포탑 탄약/배선/플레이어 피해 배제|
|Terraria|작은 보급 장소|[Underground Cabin](https://terraria.wiki.gg/wiki/Underground_Cabin): biome별 작은 구조/상자/소모품/가구, [Sandstone Chest](https://terraria.wiki.gg/wiki/Sandstone_Chest)는 primary와 optional secondary pool|작은 bounded variation|Lowfire progression 장비는 제작|Campsite 소모품3안|고급 장비 RNG와 모든 POI 상자화 배제|

## 현재 방어 inventory

실제 전체 비용/HP/피해/간격/station/tier는 [defense-before.tsv](defense-before.tsv). 0은 해당 수치가 없는 항목이다. Item recipe tier가 실제 gate이며 Placeable.WorkbenchTier와 혼동하지 않았다.

|항목|역할/HP|발동·cooldown·차이|Host/persistence|
|---|---|---|---|
|Wood/Stone/Reinforced Wall|통로 차단100/220/360|접촉 피해 없음, footprint/placement collision|Host 설치/피해, 시설 HP/위치/ID 저장|
|Wood/Stone/Reinforced Door|개폐80/200/320|0.32초,70% 열린 뒤 collider 해제; 점유 중 닫기 거절|Host 개폐, DoorOpen 저장|
|Wood/Stone/Reinforced Gate|넓은 개폐140/240/420|문과 같은 규칙/더 넓은 footprint|동일|
|Wood/Stone Fence + Fence Gate|경계/출입90/160|둘 다 barrier, gate만 개폐. 낮은 art가 통과 가능을 뜻하지 않음|동일|
|Bone Trap|cheap passive40HP/10damage|Spike와 동일16접촉버퍼/1.2초/반복, 비용Bone2+Wood3,WBI|Host trigger, HP 저장|
|Spike Trap|expensive passive100HP/16damage|동일1.2초,Wood4+Stone2+Bronze1,WBII|Host trigger, HP 저장|
|Ballista|수동/모듈 자동180HP/62damage|26거리/2.3초/45도초; Ballista Bolt 사용|Host 발사,탄약·heading·reload 저장|
|Repeater|수동/모듈 자동160HP/16damage|17거리/.55초/105도초; 기존 bolt 계열|동일|
|Ammo|Arrow/Bone/Bronze, Bolt/Barbed/Steel, Ballista Bolt|기본 Bolt12발당Wood2+Feather1; Ballista6발당Wood2+Steel1|Host 한발 소비/실패 rollback, 가방/cargo 저장|
|Ammo Box|장전 보관1→3칸|각100stack, 소비 속도 불변|Host 모듈 장착/가방 transfer, module 저장|
|Auto Loader/Auto Aim|장전/표적 자동화|둘 다면 자동 발사,LOS/사거리/회전 필요|Host scheduler /module 저장|
|Rotation Gear/Frame|회전×1.35/최대HP×1.5|Frame 즉시 회복 안 함|module 저장|
|Repair/reclaim|없음|시설 HP 손상 영속, 일반 시설 재료 반환 없음; turret cargo/modules는 파괴 때 drop|부족 인정, 이번 보류|
|Enemy/nav|Player 추적 중 BarrierAhead 시설 타격|국소 steering/solid collision, full pathfinder 없음; CanSpawn enclosure 차단|Host AI, actor HP/위치 저장|

Zombie 피해8,준비.38/회복1.15/Swing.12의65%에서 타격하므로 벽100/220/360은13/28/45타다. 검수 중 Swing을 포함해 정정한 이론 하한은 약19.75/43.87/71.21초(첫타.458,이후1.608초,접근·경로·frame 양자화 제외). 명확한 생존시간 차이로 wall 수치 변경 불필요. Stone Door/Fence/Gate는 실제 WBI이고 Stone Wall은 WBII다. 본 패스에서 gate tier를 정렬하는 overhaul은 하지 않는다.

## Defense gap matrix

|역할|판정|현재 근거/이번 결정|
|---|---|---|
|A Barrier/funnel|충분|벽3재질/문/대문/울타리, 피해와 차단 분리|
|B cheap passive|부분적|Bone이 Spike 저피해판→32피해 단일 대상 1회 소모로 교정|
|C expensive passive|충분|Spike 반복16/1.2초, 비용Bronze·WBII|
|D manual ranged|충분|Ballista/Repeater 수동 조작|
|E automated ranged|충분|Loader+Aim 실제 완전 자동, 유한 탄약/LOS/회전|
|F repair/recovery|비어 있음|복구 별도 패스, 이번 새 Recipe 안 만듦|
|G emergency|부분적|신속 배치 Bone burst/기존 개인 전투·회복, 새 barricade 없음|
|H choke control|충분|gate+wall+trap+turret 조합; slowing 새 framework 없음|
|I early/mid/late|충분|WBI Bone/Wood→WBII Spike/Stone/Reinforced→WBIII 포탑, 수리 빈칸 별도|

## POI 전수 audit / 채택

|POI|현재 시각|현재 기능/적/보상|재방문·고유성·문제|이번 결정|
|---|---|---|---|---|
|Campsite|텐트/불터 art|없음/없음/없음|landmark뿐|소형 one-time expedition supplies, 기존 UI|
|Ruin|무너진 돌 art|없음/없음/없음|salvage 가능성 있으나 현재 비기능|보류: cache를 또 만들면 Campsite/DeepWorkshop 중복, 필수 Part RNG 배제|
|Well|우물 art|없음/없음/없음|강물과 구별 필요|cap3 clean 회복 수원|
|Quarry|노출 암석 art|없음/없음/없음|채광할 실제 이유 없음|기존 Stone2/Copper1/Coal1 node, 유한/도구 gate|
|Graveyard|묘비 art|없음/없음/없음|Undead 장소지만 위협 없음|Skeleton1 소규모/잔여 보급품 접근 gate|
|Mine|버려진 광산 art|없음/없음/없음|surface landmark, underground entry 아님|보류: Quarry/기존 DeepMine와 중복, surface identity 보존|
|GoblinCamp|거주지/보급 상자|resident3 clear→한번 supplies|보충480초와 loot 별도|보존, 더 강한 전투/보상|
|CaveEntrance|동굴 입구 art|Surface↔Cavern portal|이동/깊은 progression 고유|보존, 상자 없음|

### 결정별 근거와 배제 대안

1. **4개 POI / 신규 item·recipe·방어물0.** CoreKeeper Scenes와 Terraria Cabin은 다른 행동/규모를 가진다 → Lowfire6개가 비기능 → 수원/소모 보급/현지 채광/guarded 접근 네 패턴 → 8개 전부 cache와 Ruin·Mine까지 동시 확장은 중복 때문에 배제.
2. **Well.** Grounded dew cap/회복과 DST 재생 자원 장소 → 무제한 raw 강물과 무기능 우물 사이 빈칸 → 공유3회,480 game초당1회,한번에 Clean1charge만 선택 Waterskin에 넣거나 EmptyCup1→CleanCup1 → 무제한 clean/한번에 skin3충전/별도 water item 배제. 지속38/480=.07917Thirst초 < drain.12,1인 약66%;4인도 동일 shared budget. 처음3회114는 탐험 보너스, 오프라인 회복 없음. MP 자연 감소 OFF라는 기존 정책도 유지한다.
3. **Campsite.** Terraria biome상자/secondary variation와 CoreKeeper 작은 scene 보상 → 보급 준비를 조금 돕는 장소 부재 → seed+stableID로3안 중1개, Torch2+CookedMeat1+Arrow4 / Torch3+CookedFish1+FieldDressing1 / Torch1+TrailRation1+CleanCup1 → 무기/광물/Part/같은4종 고정표 배제.
4. **Quarry.** CoreKeeper finite boulder와 DST ReedTrap은 자원 현장 자체가 가치 → Lowfire 채석장이 그림뿐 → Stone2/Copper1/Coal1 기존 node, 기존 HP/도구 gate/drop, 수확 후 tombstone → chest/자동 공개/새 raw/새 광물/재생 debt 배제. 최대Stone12/CopperOre6/Coal10, 채굴 노동 필요. 원래 source ID/밀도/회복 ticket 바꾸지 않음.
5. **Graveyard.** Valheim Burial guard와 Necesse effort→reward → 묘지에서 Undead identity 부재 → 기존 Skeleton1/32HP, 처치 후Bone3+Herb1+FieldDressing1, guard 자체 기존 loot 유지 → 새 종/전리품/보스/무조건 대규모 전투 배제. hardcap/막힌 spawn이면 잠김 유지, luring만으로 clear 안 됨.
6. **Bone/Spike.** Grounded/Valheim 소모 함정과 DST reset, CoreKeeper 안전 함정 비교 → 현재 두 trap이 피해만 다름 → Bone32damage/한 적/한번만 작동 후 기존Health 사망, Spike16 반복 유지 → 새 TrapII/poison/reset UI/friendly fire 배제. 비용·tier 기존 유지, wall 수치 유지. 수리는 Grounded 도구와 Valheim 시설범위 모델이 서로 달라 별도 설계 없이 얹지 않음.
7. **Authority/persistence.** 유한 장소 보상을 재방문마다 주면 위 사례의 소비/현장 가치가 무너짐 → stable anchor에 최소 state 추가/Host 공유원자 transfer → player×4/Client roll 배제. 기존Visited는 청크 활성화 기록이므로 구 feature는 새 one-time 보상/광맥/guard 지급 없이 빈 state; Well은0에서480초 뒤 회복. 미방문 anchor만 신규 content를 초기화. generation1 새 anchor 생성 안 함.

## 구현 계약

Save11 선택 nested state에 Schema1 판별(누락 필드가 empty object로 역직렬화되는 경우 포함), immutable capture clone. One-time6slot/revision/남은량, well clock, quarry node HP/tombstone. 기존 feature/cave encounter/DeepSite는 유지. generation3/기존 anchor 및 자원 ID 고정. 새 NGO representation/RPC 때문에 Protocol18 예정, Save11 유지 가능 여부를 검증한다.

표현은 active chunk에서만, 중앙 scheduler 재사용. 새 perPOI Update/전체월드 frame scan 없음. CampLootPanel을 공용 유한 supplies view로 재사용. Graveyard의 기존 undead는 feature actor로 낮/휴면/Save/reHost 보존. 실제 사용자 save는 hash/mtime 전후 비교, QA isolated만 사용.

## 진행/검증

아래는 실제 실행 결과다. 초기 실패 XML/로그도 남겼고, 최종 판정은 각 test의 마지막 실행 결과를 기준으로 한다.


## 실제 구현 / 경제

사전 채택한4개 POI와 신규 item/recipe/defense/turret0을 구현했다. Campsite/Graveyard는 공용 IWorldSupplies를 통해 기존 CampLootPanel만 사용한다. Well은 기존 Cup/Waterskin을 채우고, Quarry는 실제 기존 ResourceNode를 채광/Drop/Pickup한다. 별도 junk/raw/열쇠/필수 RNG progression/새 station/wave/보스를 만들지 않았다. GoblinCamp의 원래 보급 표·resident 보충과 CaveEntrance 이동은 보존했다.

[경제표](economy.tsv)는 실제 data 값을 따른다. Campsite 한 번의 음식은25/26/35 Hunger이고, 약품이 있는 안만20HP/8초 붕대1개다. Arrow4는 기존12발 batch의1/3이다. Campsite cache는 Bronze/Iron/Coal/Leather/Bone/MechanicalPart를 직접 주지 않는다. Coal은 Quarry의 실제 채굴에서만 최대10이다. Quarry당 Stone12/CopperOre6/Coal10 상한이며 Copper는 기존 Iron 도구, Coal은 Stone 도구 조건을 그대로 쓴다. 잘못된 Stone 도구 가정을 실제 asset 확인 후 바로잡았다. 기존 건축/노드를 피해 배치하지 못한 slot은 생략하며 재방문 때 보충하지 않는다.

Graveyard는 cache Bone3+MedicinalHerb1+Dressing1, 정상 Skeleton Bone1~2까지 합계4~5다. 2Bone 소비의 Bone Trap/반복 Arrow와 기존 치료 수요를 유지한다. guard 자체 XP7/Gold2와 정상 loot 규칙은 그대로다. 기존 GoblinCamp는 BoneArrow8~16/Salve1~2/Bronze1~2/Leather1~2/선택 Part·BarbedBolt로 더 강한 전투 보상이다. MechanicalPart 제작 가능성은 유지하며 새 POI 필수 RNG가 아니다.

Seed9472026의 실제 anchor는 Campsite56/Ruin60/Well38/Quarry36/Graveyard13/Mine32/GoblinCamp19/CaveEntrance12,합266이다. 새 기능 대상143개를 전부 entity로 살려 두지 않는다. 이 seed의 모든 Quarry를 채굴해도 추가 상한은 Stone432/CopperOre216/Coal360이고 실제 도구·이동·채굴이 필요하다. 모든 Campsite의 음식도 유한56개다. 이는 seed별 상한 예시이며 체감 획득 속도나 측정된 장기 경제를 뜻하지 않는다.

1P/4P 모두 동일 유한 공유 잔량이다. 인원수×4/개인 claim/거주 적 respawn 재충전은 없다. 우물은 cap3,38/480=.07917Thirst/초로 SP 감소.12보다 작다. MP의 기존 자연 감소 OFF는 유지했다. 기존 사냥/채집/정수/제작을 대체하는 무제한 금속·약품·식량 공급은 추가하지 않았다. 실제 경로별 식량 효율과 재미는 사용자 QA로 남긴다.

## 영속성 / 기존 저장 / multiplayer

- Save11/generation3 유지. generation1에 POI를 새로 만들지 않는다. generation2/3 기존 anchor·normal source ID·건축·채집 delta를 이동/재생성하지 않았다.
- 기존 feature+누락/Schema0 Site는 Legacy empty6칸/광맥0/guard0이다. Well은0에서480 game초 뒤 회복한다. ‘Visited=false로 초기화→무료 새 보상’ 정책을 쓰지 않는다. 이미 Schema1인 잔량/HP/clock은 그대로 읽는다.
- Snapshot capture는 nested state를 복제한다. Quarry는 embedded ID/HP/tombstone이고 자연 recovery debt에 포함되지 않는다. Well은 게임 시간만 사용하고 offline 회복을 더하지 않는다.
- Graveyard guard는 stable feature에 연결한 기존 Skeleton이다. 낮에도 행동하고, home12/Player16 경계에서 기존 steering으로 돌아간다. 유인/낮/휴면은 HP나 clear를 초기화하지 않는다. spawn hardcap/장애물이 막으면 잠김을 유지하고 active registry에서 재시도한다.
- Protocol18은 새 NetworkPoiSite/NetworkList/owner POI RPC에 필요하다. Host가 roll/transfer/물 소비/자원/guard clear를 결정한다. Client entry는 서버 전용 Bag.CanOperate로 막지 않고, 실제 서버 Execute에서 생존·거리·세션/scouting·독립 site/bag revision을 검사한다. Owner sequence는 중복/역행 요청을 거절한다.
- 실제 별도 LAN Client가 기존 UI로 campsite를 수령했다. Host의 오래된 경합 요청은 거절됐다. 같은 well revision의 두 Client 요청은 clean charge1/공유 water1만 소비했다. Client는 잠김→Host 처치→해제 replica→Bone3 수령을 확인했다. disconnect 후 실제 MCP reHost에서도3개 잔량이 일치했다. 4 actor 경합은 **서버 actor 시뮬레이션**이고4개 실제 단말 검수가 아니다.

## 방어 / 성능

Bone Trap32/단일 대상/성공1회 후 파괴,Spike16/1.2초 반복으로 구분했다. 비용/tier/HP는 유지하고 원래 CreatureLootAssets authoring도 같은 역할로 수정했다. 같은 팀/자원은 소모하지 않는다. Wood/Stone/Reinforced wall·door·gate 수치, Ballista/Repeater·탄약·모듈·완전 자동화는 바꾸지 않았다. Repair/일반 철거·재료 회수는 여전히 빈칸이다.

새 PoiSite/NetworkPoiSite에 Update가 없고 전체 POI frame scan도 없다. 기존 chunk 활성화에서만 장소를 표현하며, central0.25초 scheduler는 active registry만 재시도한다. 시간 경과 물은 지연 계산, 먼 guard는 기존 sleepingNight로 보관한다. 실제 MCP 마지막 관측은266anchor 중 active site1/active chunks9/pending0이었다. active chunk retire 시 cache/network representation과 node를 정리한다. 이 관측은 Android FPS·장시간 성능 보장이 아니다.

## 자동 검사

최종 **EditMode41/41, PlayMode10/10**, data validator0 errors, source validator1362 unique GUIDs PASS. [RESULTS](RESULTS.json)의 latest 항목은 중복 실행을 합산하지 않은 test별 마지막 결과다. XML은 성공과 초기 실패를 모두 남겼다. `.log`는 repo ignore 정책을 유지한다.

EditMode는 POI 정의/seed 변형/음식·물 상한/구 Schema/forged loot·물 clock·node rule/중복 guard/Save11/Protocol18 serialization/기존 catalog·시설·save migration/generation2 replay를 포함한다. PlayMode는 실제 POI/도구 gate·채굴·Pickup/유한 보상/fullbag/중복/휴면/Save/Load/구 visited/함정·문 collision/구 v3 load/Host 두 차례 reHost/기존 Camp 보존/guard 경계/실제 별도 Client/기존 Camp modal/Cave 왕복을 포함한다.

|검수 중 발견|구분|최종 처리|
|---|---|---|
|나무 곡괭이 ID를 wood_pickaxe로 가정|fixture|실제 pickaxe 사용|
|구리의 도구 조건을 Stone으로 가정|fixture/감사 정정|실제 Iron gate 유지·확인|
|청크 활성화 직후 sleeping actor를 즉시 찾음|fixture|중앙 tick 후 표현 확인|
|grave cache에서 root 기준 이동해2.6m 상호작용 밖|fixture|실제 Location 기준으로 접근/Interact|
|NetworkCreature spawn이 guard 야간 lifecycle을 다시 켬|runtime|PersistentGuard만 재활성화 제외, 낮/두 reHost 통과|
|Client submit에 서버 전용 Bag.CanOperate 사용|runtime|Client 요청/서버 검증 경계 수정, 실제 별도 Client 통과|
|멀리 유인한 guard가 장소를 떠남|runtime|기존 steering의 작은 home 경계,HP/lock 유지 확인|

## 실제 Unity MCP · 1920×1080

[live.tsv](live.tsv)와 [screens](screens)는 Editor MCP로 실행한 격리 Play 기록이다. 프로그램 fixture의 teleport·직접 fixture damage를 이용했고, 사람의 터치 플레이나 재미 검수로 주장하지 않는다. 8개 POI 각각에 접근해 원래 landmark art·추가 chest/node/guard 구분을 확인했다. 변경된 cache/Well은 실제 PlayerInteraction과 기존 UI action으로 사용했다.

- Campsite:3slot variant,실제 modal,fullbag rollback,독립 revision 중복 거절,leave/revisit/unload 잔량 보존.
- Well:선택 물통의 natural raw shortcut 대신 clean1,cap3,empty/480초 회복,컵 교환,가방/용기 실패와 raw partial 거절,unload 보존.
- Quarry:물리4node,약한 도구 거절/기존 Iron gate,실제6 Copper Drop·Pickup,fullbag ground 보존,고갈/tombstone,자연 recovery 미증가.
- Graveyard:낮 실제 추적,잠김 feedback,낮 dormant HP,유인/재방문 미해제,실제 죽음1회 clear,fullbag/중복/잔량 보존,home 복귀.
- Defense:실제 Bone/Spike 설치,팀 안전,32 한 번 후 소모/16 반복,소모·생존 trap Save/Load,닫힌 gate를 실제 적이 공격하고 열린 choke로 통과,enclosure spawn 거절.
- Save/Load:정상 isolated repository 검증·Apply 외에 실제 SceneFlow Load로 다른 WorldSaveService instance/Save11/gen3/Spike1/Bone0을 확인했다.
- Multiplayer:실제 MCP Host+별도 headless Client의 owner RPC/UI/replica/종료/reHost 잔량 일치. Client 화면 screenshot은 검수하지 않았다.

[user-save-before](user-save-before.json)와 [after](user-save-after.json)의 모든 hash/mtime는 일치했다. 사용자 파일을 수정하지 않았다. 끝에 Play를 종료하고 MainMenu/기본 profile로 돌렸다. Windows 입력/Unity Focus 호출은 사용하지 않았다.

## 미검증 / 판정 / 다음 범위

**A — 요청한 기능 범위.** 네 장소가 네 행동 패턴으로 기능하며 기존 GoblinCamp/CaveEntrance와 구분된다. 새 방어물 없이 두 trap을 구분했고 공유 보상 복제를 막았다. Ruin/Mine·Repair/reclaim은 감사 후 명시적으로 보류했다.

실제4인 Relay/원격 latency·loss/Android 터치·발열·FPS/장시간 자원 경제·재미는 미검증이다. Local avoidance가 복잡한 기지 전체 경로를 보장하는 것도 아니다. 밸런스/재미·여러 우물을 연결하는 실제 이동 효율은 사용자 QA다. 이 A는 출시 인증이 아니다.

Content Expansion2는 이번 범위에서 마감한다. 다음은 Skill Point/Class/Job **기획**,그 후 Undead King+Reusable Boss Foundation이며 이번에 시작하지 않았다. 시간과 계정 공용 사용률 관측은 [timing-usage.json](timing-usage.json),각 자동 실행의 시작/종료는 XML에 있다. 병행한 headless 시간은 총 wall time에 다시 더하지 않았다.
