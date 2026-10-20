# 현재 Creature / Loot · Expansion 1

2026-10-19. 기존5동물+Zombie/Goblin/새총 Goblin/Skeleton/검 Variant와 신규8종을 제공한다. Family=Animal/Undead/Humanoid/Invertebrate, Faction과 Trait/Habitat는 별도 데이터다. 종 ID별 runtime AI 분기는 없다.

## 신규 생태와 전투

|종|서식지 / 성격|이동·전투·읽기|
|---|---|---|
|Bat|Caverns / 적대|그림자와 높이·분리 날개로 비행, 경고→방향을 고정한 급강하→이탈. 지상 추적만 하는 Sprite가 아님|
|Stone Crawler|Rocky / Deep Caverns / 방어적|보통 Stone과 닮은 휴면 Sprite+작은 눈·균열·호흡. 근접 경고→다리/몸 해제→접근·물기|
|Bog Leech|Wetlands의 육지 물가, WaterDistance0.2–4 / 적대|짧은 경고·준비 후 도약, Anchor Leash5. 장거리 추적하지 않음|
|Wolf|Forest / 무리 적대|2–3마리 Group, 접근·물기. 같은 Group의 가까운 PackAggro만 합류, 수신 경보 재전파 없음|
|Mountain Goat|Rocky / 평화·도주|독립 뿔/발굽 실루엣과 hop. 동일 Herd 도주 경보|
|Crow|Grassland / ForestEdge / 평화·도주|분리 깃 날개·비행 이탈, 피해·처치·Feather 보상. Chicken은 Feather 확정 보존|
|Giant Boar|Rocky 또는 바위 성분 높은 Forest 경계 / 방어적|경고→방향 고정 준비→고속 돌진→긴 Recovery. 일반 Boar의 도주·경제 유지|
|Crystal Crawler|Deep Caverns CrystalPocket / 적대|긴 분절/결정 돌기 실루엣, 반경 예고→LOS를 확인하는 결정 pulse. 자연 Crystal source를 보조하는 낮은 확률 보상|

새 행동은 WildlifeActionDefinition의 movement/attack/temperament/timing 조합이다. 모든 이동은 traversable terrain·solid sweep를 지키며 고속 이동은 장애물 앞의 허용 거리까지 진행한다. 물기/급강하/돌진/매복은 이동 구간에서 Player Collider 표면까지의 거리, pulse는 반경과 LOS로 피해를 결정한다. 대상 자체의 solid Collider는 시야 장애물에서 제외하며 벽은 차단한다. 공격당 대상1회, pulse의 중복 hurtbox도1회다. 호흡/날개/hop/위장 해제/돌진/반경 표시를 구분한다. 개체의 Home Anchor·Group·HP는 save/sleep에 보존하며 순간 공격 pose/target은 재진입 시 초기화한다.

## 전체 드랍

비율 미표시는 확정, 범위는 해당 table roll의 정수 범위다. XP/Gold는 Item stack이 아니다.

|Loot table|XP / Gold|일반 Item|Rare|
|---|---:|---|---|
|BatLoot|3 / 0|medicinal_gland×1 (75%), bone×1 (40%)|없음|
|BoarLoot|0 / 0|raw_meat×2, leather×1|없음|
|BogLeechLoot|5 / 0|medicinal_gland×1|없음|
|ChickenLoot|0 / 0|raw_meat×1, feather×1|없음|
|CrowLoot|0 / 0|feather×1 (75%), raw_meat×1 (25%)|없음|
|CrystalCrawlerLoot|12 / 0|chitin×2, crystal×1 (15%)|없음|
|DeerLoot|0 / 0|raw_meat×2–3, leather×1 (75%), bone×1|없음|
|GiantBoarLoot|15 / 0|raw_meat×4, leather×2, bone×2|없음|
|GoblinLoot|8 / 3|crossbow_bolt×4–8 (60%), plant_fiber×1–2 (80%), leather×1 (25%)|없음|
|MountainGoatLoot|0 / 0|bone×1–2, raw_meat×1, leather×1 (50%)|없음|
|RabbitLoot|0 / 0|raw_meat×1, bone×1 (50%)|없음|
|SheepLoot|0 / 0|raw_meat×2, plant_fiber×2|없음|
|SkeletonLoot|7 / 2|bone×1–2, iron_ore×1 (20%)|없음|
|SkeletonSwordLoot|8 / 2|bone×1–2, iron_ore×1 (20%)|rusted_sword×1 (12%)|
|SlingshotGoblinLoot|8 / 3|feather×1–2 (75%), plant_fiber×1–2 (80%), leather×1 (25%)|없음|
|StoneCrawlerLoot|7 / 0|chitin×1, stone×1–2 (70%)|없음|
|WolfLoot|8 / 0|bone×1–2, raw_meat×1, leather×1 (25%)|없음|
|ZombieLoot|5 / 1|plant_fiber×1–2 (70%), bone×1 (50%)|없음|

기존 Chicken/Boar 보상은 유지한다. Rabbit/Deer/Skeleton은 Bone 공급, Sheep는 Meat+Fiber, Zombie는 Fiber+Bone으로 바꾸었다. Goblin은 Bolt/Fiber/소량 Leather, 새총 Goblin은 Feather/Fiber/소량 Leather를 주어 Wood/Stone 기초 채집과 중복을 줄였다. 검 Skeleton의 Rusted Sword12%는 유지한다. 새 raw material은 Bone/Chitin/Medicinal Gland3종이며 별도 Fur/Hide/종별 Trophy는 없다. 각 sink는 [전체 제작 정본](CONTENT_EXPANSION.md)을 따른다.

## 권위와 population

WildlifeActionScheduler가0.05초 간격에 최대32개를 순환 처리한다. 신규 AI의 개별 Update/월드 전체 검색이 없고, 활성 종 상한 합계98과 공용 HardCap80을 구분한다. 종 목표는 서식지별 soft 목표이며 전체 활성 상한을 올리지 않는다. 청크별 quota와 sleep을 적용하고 휴면 개체는 청크 index로 조회한다. 지역 부족 보충은5초마다 최대32 ticket·2 group·각24후보로 제한한다. 죽은 지역 목표만 보충하고 멀리 있는 생물은 simulation chunk를 붙잡지 않는다. Save11 ExpandedRegions로 기존 방문 청크에 새8규칙을 한 번만 보충한다.

Host만 spawn/AI/목표/피해/사망/loot roll/보충/상자 claim을 수행한다. Client는 기존 NetworkCreature frame의 phase/server timestamp/facing/awakening과 NetworkTransform을 표현한다. 신규 wildlife 행동 frame은 최대10Hz이며 HP 변경은 즉시 반영한다. 수신 알림으로 다시 공격/roll하지 않는다. 멀티 보상은 유효한 처치24unit 안의 살아 있는 접속 Player에 table XP/Gold를1회 주고, ground item은 유효한 pickup을 먼저 확정한 가방에 간다. 싱글은 실제 마지막 공격자 보상이다.

## Goblin Camp 보급품

Camp ID+:supplies가 persistent identity다. 거주 Goblin의 활성·휴면 생존 수를 모두 확인한다.6칸 take-only UI, 개봉/잔량/revision, 최초 deterministic roll을 저장한다. 재방문·거주 Goblin 보충·Load·reHost는 상자를 다시 채우지 않는다. Host가 alive/range/상자와 가방 revision/공간을 검증해 whole-stack 교환을 한 번 확정한다. 플레이어 Storage로 사용할 수 없다. 보상표는 [CONTENT_EXPANSION](CONTENT_EXPANSION.md), 버전 이관은 [SAVE_PERSISTENCE](SAVE_PERSISTENCE.md)를 따른다.
