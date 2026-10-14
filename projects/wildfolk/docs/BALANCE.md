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
| Fallen Log |12|50 /Wood4|300초|
| Stone |55|50 /Stone3|1200초|
| Iron Ore |16|75 /Ore3|2400초|
| Small Plant |36|12 /Wood1|90초|

성숙 Tree: Wood Axe5타 /Stone Axe4타 /Iron Axe3타. 재생은 원점 주변6~26유닛에서 검사하며 Player12유닛 안/Camera 안/물/길/장애물/기지 내부 후보를 제외한다. 막히면5초 뒤 재시도한다. 작은 풀은 현재 비충돌 장식이다. Small Plant 관목36개는 비차단 자원으로 HP12/Wood1,90초 재생이다. Chicken1 Slot/180초, Boar1 Slot/300초로 자연 재생한다. 실제 번식/성장 시스템은 없다. Save에는 대기 시간을 게임 시간 기준으로 기록하고 오프라인 시간을 더하지 않는다.

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
| Furnace |최대3개/6초,Speed1|
| HP Regen |두 게이지≥70%,최근 전투8초 뒤0.5HP/초|

기존 Raw 불이익은 Clean을 마셔도 즉시 치료하지 않는다. 조건 미충족/Raw 불이익 중에는 Regen0이다. 두 수치의 최소 비율30% 미만 CanSprint=false,30~70% SprintEfficiency0.55~1,70% 이상1. 실제 Sprint는 없다. 최소 비율15% 미만 일반 이동0.9배다. Singleplayer Pause는 시간을 정지하고 Inventory는 세계 진행을 유지한다.

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
| Goblin Encounter |낮35초마다30%,최대3|
| Enemy Soft Separation |Radius1.35/Strength0.8/Footprint0.45|
| Zombie/Goblin 처치 |XP5·Gold1 /XP8·Gold3|
| Chicken/Boar 처치 |Raw1·Feather1 /Raw2·Leather1|

Friendly Fire 기본 OFF. 자연 Despawn에는 보상이 없다. XP/Gold는 별도 Wallet이며 Save 대상이다. Level/Skill/Shop/Meta Coin은 없다. 실제 인원별 Scaling은 없다.

## 시설과 UI

Workbench HP60,Campfire60,Furnace100,Chest70,Wall100,Door80. 실제 비용은 CRAFTING을 따른다. General Workbench Tier1, Campfire Capacity1, Furnace Capacity3다. 시설 Cargo는 처리 중 재료/완료 결과를 보존한다.

Long Press0.45초. Split1~N−1,Half/±1,Confirm 후 목적지 선택. 일반 Drag는 전체 수량이다. 실제 손가락 터치 크기/세부 압박/장시간 프레임·발열·메모리는 별도 실기 검증 대상이다.
