# 현재 초기 플레이 수치

2026-10-14. 실제 정본은 client/Assets/Wildfolk/Data 에셋이다. 사용자 요구 범위와 기능 검증을 위한 초기값이며 상대 게임의 숫자를 측정/복제하지 않았다. 재미/세부 밸런스는 사용자 테스트로 판단한다.

## 기본 플레이

| 항목 | 값 |
|---|---|
| Player HP / 이동 |100 /4.2유닛·초|
| Camera Smooth |0.18초|
| Inventory / Hotbar / Storage |36 /9 /27|
| Chest |24칸|
| Stack |재료50, 장비1, 시설10, Empty Cup20, Raw/Clean Cup1|
| Drop 퍼짐 / 이동 / 획득 지연 / 거리 |0.85 /0.3초 /0.7초 /0.95|
| 공간 부족 Pickup 재시도 |0.15초|
| Control Size |80~140%|
| Day / Evening / Night / Dawn |240 /60 /150 /30초(480초)|

## 공격과 장비

모든 현재 기본 공격의 판정: MaxTargets1 / Facing Arc120도 / Range1.6(콜라이더 표면 거리). Pierce0/AoE0. 모션용 SwingArc는 Axe65/Pickaxe55/Sword75도이며 판정 Arc와 별개다.

| 장비 | 일반 피해 | Wood 피해 | Stone 피해 | Ore 피해 | 모션/쿨다운 초 |
|---|---:|---:|---:|---:|---|
| Fist |10|10|10|채굴 불가|0.28/0.44|
| Wood Axe |10|30|5|채굴 불가|0.44/0.62|
| Wood Pickaxe |10|5|25|채굴 불가|0.52/0.72|
| Wood Sword |20|5|5|채굴 불가|0.34/0.46|
| Stone Axe |12|42|6|채굴 불가|0.36/0.50|
| Stone Pickaxe |12|6|42|30|0.44/0.60|
| Stone Sword |28|7|7|채굴 불가|0.31/0.42|
| Iron Axe |15|60|8|채굴 불가|0.30/0.42|
| Iron Pickaxe |15|8|60|45|0.36/0.50|
| Iron Sword |36|9|9|채굴 불가|0.28/0.38|

피해 시점은 모션50%다. Resource 최소 Tier를 유지한다. Ore는 Pickaxe/Rank≥2만 가능하다. Upgrade는 이전 장비를 Recipe 비용으로 소비하며 내구도/자동 삭제는 없다.

## 월드 자원과 재생

| 자원 | 개수 | HP/드랍 | 재생 대기 |
|---|---:|---|---|
| Tree |140|150 /Wood6|300초|
| Fallen Log |12|50 /Wood4|480초|
| Stone |55|50 /Stone3|600초|
| Iron Ore |16|75 /Ore3|720초|
| Small Plant |36|12 /Wood1|90초|

성숙 Tree: Wood Axe5타 /Stone Axe4타 /Iron Axe3타. 회복은 지역 밀도 Target0.9 미만에서 새로운 ID로 해당 Region 후보를 검사한다. Rare는 월드 범위 후보이며 Player24/Harvest28유닛·1800초/Cluster12유닛 제외를 사용한다. 흔한 자원은 Player12/Harvest8유닛·Delay×2다. Camera/물/길/장애물/기지 내부 후보를 제외한다. 막히면5초 뒤 재시도한다. 작은 풀은 현재 비충돌 장식이다. Small Plant 관목36개는 비차단 자원으로 HP12/Wood1,90초 재생이다. Chicken10/180초,Boar6/300초,Rabbit12/180초,Deer8/420초,Sheep10/300초의 종별 목표를 지역에 분배해 부족분을 보충한다. 실제 번식/성장 시스템은 없다. Save에는 대기 시간을 게임 시간 기준으로 기록하고 오프라인 시간을 더하지 않는다.

Land1 /Shallow0.7 /Deep 도보 불가. River 폭6, 여울 길이4.4, 월드192×144유닛이다.

## 생존과 물

| 항목 | 값 |
|---|---|
| Hunger/Thirst 최대 |각100|
| 자연 감소 /초 |Hunger0.08 /Thirst0.12|
| 각각0일 때 피해 |5초마다2HP|
| Raw Meat /Cooked Meat |Hunger+8 /+35|
| Raw Water Cup |Thirst+15,60초 Regen 차단/Thirst Drain1.5배|
| Clean Water Cup |Thirst+38,추가 불이익 없음|
| Wooden Cup 제작 |Wood2,Hand crafting|
| 수원 |Empty Cup1→Raw Cup1,재사용1초,중심거리3|
| Campfire 고기/물 |각1개/5초|
| Furnace |1개/2초 순차,Input50/Output100,Speed1|
| HP Regen |두 게이지≥70%,최근 전투8초 뒤0.5HP/초|

기존 Raw 불이익은 Clean을 마셔도 즉시 치료하지 않는다. 조건 미충족/Raw 불이익 중에는 Regen0이다. 두 수치의 최소 비율30% 미만 CanSprint=false,30~70% SprintEfficiency0.55~1,70% 이상1. 실제 Sprint 조건에 연결한다. 최소 비율15% 미만 일반 이동0.9배다. Singleplayer Pause는 시간을 정지하고 Inventory는 세계 진행을 유지한다.

## 적·빛·보상

| 항목 | 값 |
|---|---|
| Zombie HP /속도 |40 /2.2|
| Zombie 감지 /추적 상실 |36 /50유닛에서15초 대기|
| 공격 피해/사거리/정지거리 |8 /1.55 /1.35|
| 공격 준비/회복/Player 분리 |0.38초 /1.15초 /1.12유닛|
| Night Target |Night1~2=6,3~4=8,5~6=10,7~8=12…최대18|
| Global Cap /Local Radius |18 /45|
| 밤 초기 출현 |65%,0.9초 간격|
| 보충 |Local 수가Target50% 미만일 때15초에1개|
| 출현/잠복 |1.2 /1.1초|
| 원거리 비추적 정리 |60유닛 밖/화면 밖/비추적20초 후 잠복|
| Safe Light |Radius4,Threshold0.65,Grace2초|
| Dim /Safe 영향 |Radius1.35배까지 감지0.8 /Safe 이동0.75|
| Goblin HP/속도/순찰 |50 /2.4 /0.65|
| Goblin Watch/Inner/Personal/Leash |5 /2.5 /1.8 /10|
| Goblin 공격 |피해6/사거리1.5/준비0.45초/회복1.3초|
| Goblin Encounter |낮35초마다30%,3~5 Warband/로밍 Soft Cap3/전체Hard Cap24|
| Enemy Soft Separation |Radius1.35/Strength0.8/Footprint0.45|
| Zombie/Goblin 처치 |XP5·Gold1 /XP8·Gold3|
| Chicken/Boar 처치 |Raw1·Feather1 /Raw2·Leather1|

Friendly Fire 기본 OFF. 자연 Despawn에는 보상이 없다. XP/Gold는 별도 Wallet이며 Save 대상이다. Level/초기 Skill은 제공하며 Shop/Meta Coin은 없다. 실제 인원별 Scaling은 없다.

## 시설과 UI

Workbench HP60,Campfire60,Furnace100,Chest70,Wall100,Door80. 실제 비용은 CRAFTING을 따른다. General Workbench I Tier1/II Tier2, Campfire Capacity1, Furnace Capacity3다. 시설 Cargo는 처리 중 재료/완료 결과를 보존한다.

Long Press0.45초. Split1~N−1,Half/±1,Confirm 후 목적지 선택. 일반 Drag는 전체 수량이다. 실제 손가락 터치 크기/세부 압박/장시간 프레임·발열·메모리는 별도 실기 검증 대상이다.

## Armor / Character / Stamina 초기값

| 항목 | 값 |
|---|---|
| Leather Cap / Guard 방어 |1 /2 (합3)|
| Iron Helmet / Guard 방어 |2 /4 (합6)|
| 일반 피해 |max(1,피해−Head−Body),생존 피해는 우회|
| Level |1~20,필요 XP=35+(Level−1)×20,Up마다Point1,남은 XP 유지|
| Vitality / Endurance |Rank당MaxHP+10 /MaxStamina+15,각3Rank|
| Recovery /Steady Stride |Rank당Recovery+15% /SprintEfficiency+12%,각3Rank|
| Sprint 비용 |14/초 ×1/(1+Skill효율Bonus) /생존SprintEfficiency|
| Stamina |100,회복16/초,사용 후1.2초 대기|
| 나쁜 상태 회복 |Condition0→45%,70%→100% 보간|
| Sprint 입력 |진입0.9/0.4초,유지0.72,정지/CanSprint=false/고갈 시 Walk|
| 고갈 후 재진입 |Stamina20 회복 후 새 Hold|
| Sprint 속도 |최대1.55배,생존Efficiency로1~1.55 보간|
| Dash |Cost24,2.6유닛/0.18초,재입력0.55초,Footprint0.55|
| Stamina 숨김 |Full 후0.7초,fade0.2초|
| Workbench II /Reinforced Chest /Stone Wall HP |120 /140 /220|
| Reinforced Chest |32칸,기존 Chest24 유지|

첫35XP는Zombie7마리 또는Goblin5마리(남은5XP)다. 첫 두 레벨35/55는XP90이며 기능 검증을 위한 초기 곡선이다. Leather는Zombie8을5,Iron은2로 줄인다. 가죽 세트Leather3,철 세트Ingot5+Leather2를 기존Boar1Leather/kill,Ore3/drop와 비교했다. 성장 만족도/실제 획득 속도는 사용자 테스트 대상이며 출시 밸런스를 주장하지 않는다.


현재 재생 수치는 최초 개수를 항상 복원하는 규칙이 아니라 부족한 Region의 회복 대기 초기값이다. 글로벌 Count 합×1.25는 성능용 상한이다. Tree/Log/Stone/Iron/Plant 회복 속도와 지역 고갈 체감은 사용자 밸런스 검수 대상이다. Sprint/Dash는 기본 조작이고 Skill은 성능을 강화한다. Recovery/Stride 선행은 Endurance Rank1이며 v2 기존 Rank를 보존한다.


## 생태·전투 추가 초기값

| 종류 | HP | 보행 / 도주 | 감지 | Group | Separation |
|---|---:|---:|---:|---:|---:|
| Chicken |20|0.55 /2.4|2.4|2~4|0.34|
| Boar |20|0.55 /2.4|2.4|1~2|0.62|
| Rabbit |20|0.6 /4.2|3.5|1~3|0.30|
| Deer |55|0.75 /3.6|5.5|2~4|0.70|
| Sheep |40|0.45 /2.0|3.0|2~5|0.60|

종별 목표 합46/성능 상한80,Player14/Base8 회피. 지역 목표는 Soft Target이며 한 Group이 일부 지역 목표를 넘을 수 있으나 종별 목표와 Hard Cap을 넘지 않는다. 일반 보충 검사는5초마다 수행한다. 초기 배치에도 Player14/Base8를 적용한다. Recovery 후보는 카메라 밖이다.

Skeleton HP32/속도2.65/피해6/Reach1.45/준비0.4/Swing0.14/회복1초. 기존 Zombie HP40/속도2.2보다 낮은 HP와 높은 이동 속도다. Night 생성 중 Skeleton 확률35%,그 중 검 Variant25%. 검은 피해+2/Reach+0.25,Rusted Sword Drop12%. 기본 Skeleton XP7/Gold2,검 Variant XP8/Gold2. 플레이어 Rusted Sword 피해11/Reach1.85/Arc110/Duration0.3/Cooldown0.48,Single Target.

Zombie LightSensitive Dim 감지0.8/Safe 이동0.75, Skeleton0.7/0.65. 공통 Dim 이동0.9/안전빛 Aggro 억제/Grace 배수1. Light Damage 없음. Undead Family 자체는 이 계산 조건이 아니다.

Goblin 시각 Separation0.85/Preferred Spacing2.7유닛. Goblin Ally Assist7유닛,원래 위협에서 최대12유닛,재전파 없음/Leash10. Warband3~5,새총 확률30%/상한2명. Camp1/거주3/보충480초/순찰4/Player18/Base12/전체Goblin 상한24. 로밍 Soft Cap3은 한 무리가3~5이므로 일부 초과할 수 있다.

새총 피해4/최소2.4/선호4.8/최대7유닛. 재배치0.65초/재시도 간격2.5초. Ready0.12/Draw0.42/Hold0.10/Release0.08/Recovery0.28. 발사0.64초/속도10/수명1.4초. 끝없는 후퇴 대신 제한된 재배치 후 거리 안에서 공격한다.

밀착 Arc 완화는 실제 Collider 간 거리≤0.12유닛이며 Range는 그대로 검사한다. 정상 거리는 기존 Facing Arc다. Door0.32초/eased Open70%에서 통로 Collider 해제. 기존 Armor 방어/제작 Tier는 그대로다. 초기 검증 수치이며 최종 난이도와 체감은 사용자가 테스트한다.

## 초기 금속 패스 수치

기존 Ore 열의 도구 피해/최소 Rank2는 Iron Ore다. Copper/Coal은 Wood Pickaxe Rank1, Tin은 Stone Pickaxe Rank2부터 가능하다. 신규 ResourceKind의 피해는 Tool.Damage 기본값을 사용한다(기존 Wood/Stone/Iron efficiency는 유지). Copper HP90/Drop4/Count14/회복600초, Tin75/3/10/720초, Coal90/4/12/600초다. 신규 제련/합금의 비용·시간은 METAL_PROGRESSION을 따른다. I→II 비용8Wood+6Stone+2Iron Ingot과 기존 Iron 제련1Ore/2초는 유지한다. 현재 콘텐츠는 I~II이고 VI는 구조적 상한이다. 출시 밸런스/장시간 scarcity는 미확정이다.
