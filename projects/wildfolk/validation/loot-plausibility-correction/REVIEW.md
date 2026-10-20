# Loot 개연성 교정 · 2026-10-20

시작 remote main `3667baebbaa8d06b7b56f70494b93c9de8da1035`. 작업 전 fetch에서 HEAD와 일치·main·clean을 확인했다. AGENTS/README와 생물/제작/저장/수치/구조 정본,18개 LootTable과 실제 Goblin 행동·외형을 읽었다. 신규8종/재료3종/Recipe99/Station/Camp 구조를 유지하는 작은 데이터 교정이다. 새 기능·재료·시설 설계를 하지 않으므로 외부 게임 조사를 반복하지 않았다. 이전 Expansion REVIEW와 baseline/economy/graph는 당시 기록으로 보존했다.

## 실제 변경

|Table|제거|현재 Item 보상|XP / Gold|이유|
|---|---|---|---:|---|
|SheepLoot|Plant Fiber2 확정|Raw Meat2 확정|0 / 0|털은 식물 섬유가 아님. 고기만 있어도 충분함|
|ZombieLoot|Plant Fiber1–2 70%|Bone1 50%|5 / 1|자연 채집재를 몸/복식 잔재로 동일시하지 않음|
|GoblinLoot|기본 Bolt4–8 60%, Fiber1–2 80%|Leather1 25%|8 / 3|현재 근접 Goblin은 쇠뇌를 쓰지 않음. 가죽 복식·장비 끈의 소량 회수는 기존 그대로 유지|
|SlingshotGoblinLoot|Feather1–2 75%, Fiber1–2 80%|Leather1 25%|8 / 3|현재 새총은 돌을 발사하고 Feather를 사용하지 않음. 전투 역할로 Variant 구분|
|SkeletonLoot|Iron Ore1 20%|Bone1–2 확정|7 / 2|광부 역할·원광 소지 장비가 없는 추가 명백 사례|
|SkeletonSwordLoot|Iron Ore1 20%|Bone1–2 확정, Rusted Sword1 12%|8 / 2|같은 원광 오류만 교정하고 실제 든 검의 rare drop 유지|

before-loot.json/after-loot.json은18개 table 전체 snapshot이다. 나머지12개 table, 남긴 entry의 수량·확률, 모든 XP/Gold를 유지했다. Stone/Crystal Crawler의 Stone/Crystal은 몸·서식지와 연결되고 새8종 유지 조건에도 해당해 수정하지 않았다. 모든 Creature가 여러 Item을 줄 필요가 없으며 Zombie/Goblin은 Item이 없는 처치도 정상이다. 대체 잡템·Wool/Cloth·새 Item을 만들지 않았다.

6개 정본 asset과 CreatureLootAssets의 재생성 초기값을 함께 교정했다. override를 덧붙이거나 전체 asset/scene builder를 실행하지 않았다. Item/Recipe/Prefab ID와 GUID, catalog 순서·population·회복·피해·UI는 그대로다.

## 공급·소비 계산

수치는 actual authored Drop/Recipe/Population의 산술이며 필드 파밍 실측이 아니다. Recipe99의 비용/출력은 변경하지 않았다. 현재121 Item source→recipe→use는 [CURRENT_GRAPH](CURRENT_GRAPH.md), 계산 입력/결과는 [economy.json](economy.json)을 따른다.

|재료/탄약|교정 후 공급과 비용|영향|
|---|---|---|
|Feather|Chicken1 확정, Crow1 75%. cap10/6, recovery180/240초 유지|새총 Goblin의 처치당 기대1.125만 제거. 자연 공급의 확률/수율은 유지|
|Plant Fiber|Small Plant1개에서2, 회복90초 유지|Sheep2, Zombie1.05, 각 Goblin1.2의 처치당 기대 보너스 제거. 관목 채집으로 조달|
|Bone|Zombie0.5, Skeleton/검/늑대/산양 각1.5, Deer1, Giant2, Rabbit0.5, Bat0.4 기대/처치|모든 공급 확률/수량 유지|
|Leather|Boar1, Deer0.75, Wolf0.25, Goat0.5, Giant2, 각 Goblin0.25 기대/처치|모든 공급 확률/수량 유지|
|기본 Bolt|Workbench II에서 Wood2+Feather1→12, Deep POI 보너스 유지|근접 Goblin의 기대3.6/처치 제거. 반복 제작 경로 유지|
|Iron Ore|자연 Iron source6/회복120초 유지|두 Skeleton의 기대0.2/처치 제거. 원광은 채굴로 공급|

Feather를 소비하는6 Recipe는 Arrow/Bone Arrow/Bronze Arrow/기본 Bolt가1개당12발, Barbed/Steel Bolt가1개당8발이다. Bone/Chitin/금속 등의 추가 비용도 그대로다. Ballista Bolt는 Wood2+Steel1→6으로 Feather를 소비하지 않는다. Repeater는 기본 Bolt를 계속 실제로 소비한다.

### 기존과 같은 분모로 본1P/4P

직전 Expansion의 명시적 개인 kit(Bone Arrow96+Barbed64+Vest1+Salve4+Bone Broth2+Restorative2+Ration2)+공유 Bone Trap4를 재계산했다. 탄약은 개인160발이며1P/4P 전체160/640발이다.

|요구량|1P|4P|자연 공급으로 환산|
|---|---:|---:|---|
|Feather|16|64|Chicken만 사용하면16/64마리 확정, Crow만 사용하면 기대21.333/85.333마리|
|Plant Fiber|10|40|Small Plant5/20개 확정|
|Bone|28|88|기존 Bone 공급량 그대로|
|Leather|1|4|기존 Leather 공급량 그대로|

기존 선택 방어기지까지 합치면 Feather30/90, Fiber16/64, Leather16/52다. Chicken만으로30/90마리, 관목8/32개로 해당 Feather/Fiber 비용을 충당할 수 있다. 이는 요구 채집/처치 횟수이며 확보 시간 보장이 아니다.4인 개인 비용은4배, 공유 시설·Camp 보급은1회로 유지한다. 필수 Diamond13/49와 Crystal10/22 등 진행 원재료는 그대로이며 잘못된 드랍을 재료 해금 조건으로 쓰는 Recipe는 없다.

### 반복 탄약의 조건부 생산 한계

서식지·후보·chunk quota·hard cap 제약을 모두 충족하고 해당 종 cap을 반복 보충·즉시 처치할 수 있다는 조건에서 `10×1÷180×60 + 6×0.75÷240×60 = 4.458333 Feather/분`이다. 이 값은 실제 공급 속도나 보장된 하한이 아니다. 두 종 soft cap이 동시에 유지되는 것도 보장하지 않는다.

이 조건부 산술은 기본 탄약53.5발/분 또는8발 배치 탄약35.666667발/분, 위160발 혼합 비율로44.583333발/분(4인 균등11.145833발/인/분)에 해당한다. 각 무기를 계속 연사하는 소비량 전체를 감당한다고 주장하지 않는다. 새 키트 Feather16/64를 이 정상상태 기대율로 나누면3.588785/14.35514분 상당이지만 초기 재고·이동·전투·출현 지연을 제외한 산술이므로 실제 소요시간으로 쓰지 않는다.

결론: 유한한 현재 kit와 Recipe99의 제작 경로는 유지되고 Fiber/Bone/Leather·탄약의 반복 공급이 끊기지 않는다. Feather의 실제 장시간 부족을 보여 주는 관측값은 이번 작은 correction에서 얻지 않았으므로 Chicken/Crow 수율을 임의로 올리지 않았다. 실제4인 지속 사격·자연 탐험의 대기감은 후속 사용자 검수 대상이다. Goblin에 Feather를 되돌리거나 가상의 파밍 실측을 만들지 않았다.

## 자동 검사

|검사|결과|Runner 내부 시간|범위|
|---|---:|---:|---|
|Data validator|errors0|별도 합산 없음|현재 Item121/Recipe99|
|EditMode edit-final.xml|21/21|0.3283673초|Loot source 계약/기존 재료 sink·Station/Camp·이관/저장 codec·복구 검증|
|PlayMode play-final.xml|3/3|42.9152705초|6종 실제 사망→roll→pickup/XP·Gold, 이전 보유 stack Save11 roundtrip, Save10 이관, Camp 잔량 sleep/save/load|

확률 entry의 성공 branch를 보기 위해 fixture RNG를 고정했다. 드랍률 실측이 아니며 확률 asset은 바꾸지 않았다. 새 Play 검사는 기존 Fiber/Feather/Bolt/Iron Ore 각5개가 없어지지 않는 것과 새 보상을 합친 전체 가방의 저장/복원을 확인한다. 처음 Edit 실행은 현재 NUnit에 없는 AnyOf constraint로 compile 실패했고 지원되는 constraint로 바로 교체한 뒤 위21/21을 얻었다. 전체 회귀를 반복하지 않았다.

## 실제 Unity MCP

[live.tsv](live.tsv):1920×1080 실제 Play의 Sheep/Zombie/Goblin/새총 Goblin/일반 Skeleton/검 Skeleton 처치와 ground drop, 자동 pickup, XP/Gold를 모두 확인했다. 고정 fixture seed4에서 Meat2/Bone1/Leather1/Leather1/Bone1/Bone1을 관찰했다. Skeleton 검 rare 확률은 바꾸지 않았으며 해당 실제 표본에서는 검이 나오지 않았다. 최초 QA fixture의 Zombie prefab 이름 추측이 틀려 중단된 기록은 live-fixture-initial.tsv에 보존했고, 실제 spawner prefab 참조로 고친 전체6종 실행을 최종 결과로 사용했다.

[final-live.json](final-live.json):실제 isolated Save=true/Load=true, 전체 inventory 동일, 이전 Fiber/Feather/Bolt/Iron Ore 각5개 유지, Data validator errors=[]를 확인했다. 캡처는 captures/에 있다. 사용자 UserData2개 hash/mtime_ns는 user-save-before/after.json으로 비교했고 일치한다. 마지막 Play 종료·QA profile 해제를 확인했다. Windows 입력/Focus/Show를 사용하지 않았다. 원격4인/모바일/장시간 자연 파밍은 이번 범위가 아니다.

## 호환·다음 단계

Save11 / Protocol13 유지. Item/Recipe/Network schema와 persistent ID를 변경하지 않아 버전 이관을 추가하지 않았다. 이미 저장된 아이템·ground pickup·Camp 보급 잔량은 유효하고 이후 생물 사망의 table만 달라진다. 직전 Expansion 실제 MCP에서 교정 전에 쓴 격리 Save11도 현재 Rules로 읽어 검증했다(previous-expansion-save.json); 원본 파일을 저장하거나 수정하지 않았다. 과거 Expansion review는 수정하지 않았다. 이번 correction은 완료했으며 다음 작업은 Undead King/Boss Foundation이다.

## 시간·사용량

usage.json과 아래 표는 단계 경계에서 읽은 계정 공유 한도 스냅샷이다. usedPercent는 정수 표시이며 토큰 수가 아니다. 다른 채팅의 사용과 반올림이 섞일 수 있으므로 해당 단계의 정확한 과금/토큰 사용량으로 해석하지 않는다. 이 환경은 단계별 실제 token 수를 제공하지 않아 숫자를 만들어 추정하지 않았다. Tool/Test 대기는 아래 작업 시간과 겹치며 별도 합산하지 않는다.

|단계|UTC 시작→종료|경과|5시간 한도 usedPercent|주간 usedPercent|
|---|---|---:|---|---|
|최신 상태·전체 드랍/경제 감사|08:58:41→09:01:11|2분30초|32→35 (+3%p)|5→6 (+1%p)|
|데이터/재생성·관련 검사 작성|09:01:11→09:04:07|2분56초|35→38 (+3%p)|6→6 (+0%p)|
|자동 검사·실제 MCP·저장 검수|09:04:07→09:09:34|5분27초|38→43 (+5%p)|6→7 (+1%p)|
|경제 계산·정본/검수 문서|09:09:34→09:12:13|2분39초|43→45 (+2%p)|7→7 (+0%p)|

Git commit/push/remote 확인은 이후 구간이며 완료 응답에 실제 결과를 기록한다.

- 최종 diff·교정 전 실제 Save11 호환 확인·Git 준비: UTC 09:12:13→09:13:48, 1분35초. 계정5시간 45→47%, 주간 7→7%.
