# Character / Armor / Locomotion

현재 CharacterGrowth/Locomotion/Armor/Recipe 에셋이 수치 정본이다. Level/Point/Rank는월드캐릭터에속하고영구계정성장이아니다.

게임플레이 Level 상한은 없다. 필요XP=35+(Level−1)×20, Up마다 일반Point1이다. 누적 구간합과 이진 탐색으로 큰 XP를 처리하며 정수 저장 범위를 검증한다. Vitality 최대10Rank(+10HP/Rank), Endurance3(+15Stamina), Recovery3(+15%), Steady Stride3(Sprint비용=1/(1+.12×Rank)), Metabolism5(허기·갈증 소모−5%/Rank), Field Care3(받는 Medicine+10%/Rank)다. 모두 소진한 뒤 일반Point는 남겨 둔다. 부족/최대Rank면거절하고Respec은없다. HP투자는최대량만증가하며무료즉시회복을추가하지않았다.

Profile Icon은 Character Information을 연다. Level/XP/HP/Defense/Stamina/회복·소모 수치와 보유 Point를 보여주고 독립 Skill Tree로 이동한다. 세 탭과 Profile 장비 목록은 제거했다. 장착/비교/해제는 Inventory의 Head/Body 슬롯 한 곳에서만 수행한다. 메뉴에서는 플레이 입력을 차단하고 세계 진행은 유지한다.

실제 공용 노드 6개와 연결선을 사용한다. Vitality와 Endurance는 처음부터 투자 가능하며 Recovery/Steady Stride는 Endurance Rank1이 선행 조건이다. Rank당 Point1이며 위 개별 최대Rank를 적용한다. Metabolism/Field Care는 독립 root다. Locked/Available/Learned/현재 Rank/비용/상세/투자 불가 사유를 표시한다. v2에서 이미 습득한 Rank는 선행 조건이 없더라도 보존하고 추가 Rank 투자를 허용한다. Sprint/Dash 자체는 처음부터 사용 가능하다. 가짜 미래 노드는 없다.

Skin과Gear는별도렌더러다. 모두 공통 Motion Root 아래에서 Walking Bob/Scale을 공유하고 Hand Gear는 실제 Hand의 자식이다. Base Skin/Head Gear/Body 외곽/Hand Gear/단일Back Slot/Optional FX로구분한다. 기본몸통은회전하지않으며Eyes/Hands와외곽Gear가Facing을읽게한다. Backpack/Cape/영구Skin/Transmog는콘텐츠가없고Back Slot은향후한개만장착하는방향이다.

Stamina는Sprint/Dash공통이다. 보행은무료,Joystick0.9이상0.4초로Sprint,유지0.72,정지/고갈/심한허기·갈증에서Walk로전환한다. 고갈후20회복과새Hold가필요하다. Dash는이동입력또는Facing으로Cost24/거리2.6/시간0.18/Cooldown0.55다. 장애물/깊은물/모든메뉴입력차단을유지하고무적·타격은없다. Stamina는 Player 아래쪽 120도 Arc로 표시하며 몸통/손/머리 위 HP를 피한다. 좌상단 Stamina는 제거했다. Full 회복 후0.7초 뒤fade하고 메뉴 중에는 숨긴다. Dash Icon은 전진 Double Chevron이다.

Developer Test Console은 Editor 전용이다. 현재 Home/협동/III 장비/탐험/포탑은 CONTENT_EXPANSION을 따르며 Pet/MetaCoin/영구Skin/번식은 없다. Class 소유·XP·노드는 로컬 profile에 별도 저장한다.

향후Status Effect는CharacterStats의Modifier집계와Health/Survival/Locomotion/Combat경계를사용한다. Raw Water현재불이익은유지하고확률Water Sickness/Raw Meat Food Poisoning은이번에없다. 구체적인원인·기간·주기·중첩은후속콘텐츠패스에서결정한다.

현재Info/Skill/Armor시각과Sprint/Dash엄지위치·성장/제작속도는사용자테스트대상이다. 현재외형은자체초기아트시안이며최종상품품질을주장하지않는다.


Armor는 동일 PlayerEquipment의 Inventory Drag/빠른 Use/슬롯 교환을 공유한다. 큰 Character/Tree/Inventory는 Safe Area 중심이며 뒤 Gameplay UI 입력을 차단한다. Save13(1~12 이관)에 성장/장비/Skill/Seen/Stamina를 보존한다.

## 현재 제작 진행 범위

실제 Workbench I~III, 구조적 상한VI다. Equipment Wood/Stone/Iron/Steel/Diamond, Armor는 Leather/Iron/Steel/Diamond의 Head/Body8개와 Carapace Vest1개다. Copper/Tin/Bronze는 풀 장비 tier가 아니다. Vest는 WB II Chitin4+Leather1+Fiber2, 방어3(철 Body4보다 낮음)과 Stamina 회복속도+15%의 선택 장비다. Skill Recovery와 가산 집계하며 즉시 Stamina를 지급하지 않는다. 현재 Home/Multiplayer·Save13은 README/SAVE_PERSISTENCE를 따른다.

## Food2-B와 성장 경계

CharacterStats는PersistentHealthBonus(Skill)와일시Meal을구분해합친다.Hunter+8은Vitality한rank+10보다작으며현재HP를회복하지않는다.Vitality1이면Max110→Meal118→만료110이며Rank/Point를바꾸지않는다.Trail+10%는Recovery한rank+15%보다작고기존Armor/Skill과가산된다.Forager는기존안전HP회복에만+0.15/s이며Stamina Recovery Skill을복제하지않는다.Endurance최대Stamina/SteadyStride비용효율/Dash/Sprint해금은Meal로대체하지않는다.150초종료/교체/죽음/Load/reHost시임시효과제거,영구성장은보존한다.

## Sidegrade2-C · 성장 경계

새장비는거리유지형사냥꾼창1개이며HP/MaxStamina/Recovery/이동modifier를추가하지않는다. Iron보다피해·속도·조준폭이낮고Range가길다. Armor는기존9개그대로이며CarapaceHead보류. Vest15%는Skill/Meal과기존가산,Defense3/철Body4의손해를유지한다. 장비/Skill/Meal을분리한계산과보류이유는 [2-C REVIEW](../validation/creature-sidegrade-expansion/REVIEW.md).

## Character Progression + Class Foundation (2026-10-22)

일반 Level/XP/Rank/잔여 Point는 월드 캐릭터 성장이다. 별도 profile Schema1은 여섯 Class의 소유/XP/선택/노드/구매 intent를 보존한다. 처치별 receipt는 영구 저장하지 않는다. Survivor는 기본 소유, Warrior/Hunter/Medic/Explorer는 기존 저장 월드 Gold 각60, Engineer는 PremiumGem 공급자·가격 미확정으로 production 구매 불가다. 여러 월드 잔액을 합산하지 않는다. local checksum/backup은 인증·위조 방지가 아니다.

Home 책 안의 직업 기록(Host/Join 연결 전 포함)에서 선택·투자·무료 재분배한다. admission/world 진입 때 loadout을 잠그고 같은 세션 재접속은 Host lock을 복원한다. 일반 gameplay XP1에 Class XP1이며 개발자 Reward는 Class XP를 주지 않는다. 로컬 SP/Host의 클래스별 XP를 보존한다. 원격 미인증 참가자는 기본 Survivor/XP0/노드0에서 시작하며 원격 Class XP는 해당 Host 세션에만 유지된다. 누적50/180/400/750/1200/1800에서 현재6Point, 데이터 milestone 수로 계산하며 코드 상한6은 없다. branch1→2→3 선행, 각 노드1Point, 현재9개 중6개를 선택한다.

기본 능력: Survivor 치명상 전HP1·보호1.5초·360초(공격 시 보호 종료), Warrior 정면120도 Guard 피해50%·기력12/초·이속65%·공격간격1.6배(공격 가능), Hunter 피해 준 생물 한 마리20초 Quarry·후속피해+8%, Medic generic Medicine 아군 사용, Explorer Optic/Survey/Signal 범위+15% 및 관측점 임시 표식, Engineer 무료 HP수리·기본20% buffer·Overclock12초/30초(+25%reload/+35%rotation)다.

|Class|Branch|Node|효과|data 값 / 선행|
|---|---|---|---|---|
|Survivor|끈기|A1 생명줄|발동 시 최대 체력의 10%로 버팁니다.|0.1 / 없음|
|Survivor|끈기|A2 숨 고르기|발동 시 기력 25를 회복합니다.|25 / A1|
|Survivor|끈기|A3 노련한 생존|생존 본능 재사용 시간이 25% 줄어듭니다.|0.25 / A2|
|Survivor|적응|B1 위기 적응|체력 30% 이하에서 기력 회복이 15% 빨라집니다.|0.15 / 없음|
|Survivor|적응|B2 침착|체력 30% 이하에서 의약품 회복량이 15% 늘어납니다.|0.15 / B1|
|Survivor|적응|B3 악조건 적응|위급할 때 허기·갈증의 이동 및 기력 불이익을 25% 덜 받습니다.|0.25 / B2|
|Survivor|야전생활|C1 알뜰한 식사|음식과 물의 허기·갈증 회복량이 10% 늘어납니다.|0.1 / 없음|
|Survivor|야전생활|C2 오래가는 식사|식사의 준비 효과가 20% 오래갑니다.|0.2 / C1|
|Survivor|야전생활|C3 재정비|Home 출발 후 60초 동안 허기·갈증 소모가 15% 줄어듭니다.|0.15 / C2|
|Warrior|방어|A1 호흡 조절|방어 자세의 기력 소모가 20% 줄어듭니다.|0.2 / 없음|
|Warrior|방어|A2 하체 단련|방어 자세 중 이동 속도가 10%p 늘어납니다.|0.1 / A1|
|Warrior|방어|A3 충격 흡수|강한 공격을 막아내면 1.5초 동안 피해를 15% 덜 받습니다.|0.15 / A2|
|Warrior|반격|B1 빈틈 공격|방어 자세의 공격 간격이 1.6배에서 1.35배로 줄어듭니다.|0.25 / 없음|
|Warrior|반격|B2 반격|방어 후 2초 안의 첫 근접 공격 피해가 15% 늘어납니다.|0.15 / B1|
|Warrior|반격|B3 리듬 회복|반격 적중 시 기력 12를 회복합니다. 3초마다 발동합니다.|12 / B2|
|Warrior|전선|C1 전진 압박|방어 자세로 정면 이동할 때 10%p 더 빠릅니다.|0.1 / 없음|
|Warrior|전선|C2 빠른 호흡|방어 해제 후 기력 회복 대기가 0.6초로 줄어듭니다.|0.6 / C1|
|Warrior|전선|C3 넓은 방어|정면 방어 각도가 120도에서 150도로 늘어납니다.|30 / C2|
|Hunter|추적|A1 긴 흔적|사냥감 추적이 10초 오래갑니다.|10 / 없음|
|Hunter|추적|A2 흔적 읽기|사냥감의 거리와 체력 상태를 더 세밀하게 구분합니다.|1 / A1|
|Hunter|추적|A3 추격 본능|사냥감 방향 질주 소모가 추가로 10% 줄어듭니다.|0.1 / A2|
|Hunter|사격|B1 사냥꾼의 눈|사냥감 방향 활·쇠뇌 사거리가 10% 늘어납니다.|0.1 / 없음|
|Hunter|사격|B2 침착한 사격|0.6초 멈춰 준비한 사격은 사냥감에게 12% 더 강합니다.|0.12 / B1|
|Hunter|사격|B3 마무리|체력 30% 이하 사냥감에게 원거리 피해가 15% 늘어납니다.|0.15 / B2|
|Hunter|창|C1 긴 찌르기|창 사거리가 10% 늘어납니다.|0.1 / 없음|
|Hunter|창|C2 추격 찌르기|사냥감 쪽으로 0.8초 이동 후 첫 창 적중이 15% 강해집니다.|0.15 / C1|
|Hunter|창|C3 대형 사냥|최대 체력 120 이상인 사냥감에게 창 피해가 12% 늘어납니다.|0.12 / C2|
|Medic|치료|A1 치료 숙련|자신과 동료에게 제공하는 의약품 회복량이 10% 늘어납니다.|0.1 / 없음|
|Medic|치료|A2 빠른 처치|자신과 동료에게 의약품을 사용한 뒤의 대기 시간이 20% 줄어듭니다.|0.2 / A1|
|Medic|치료|A3 집중 치료|지속 회복 의약품이 같은 총량을 25% 빠르게 전달합니다.|0.25 / A2|
|Medic|구조|B1 구조 훈련|동료 구조 시간이 4초에서 3.2초로 줄어듭니다.|0.25 / 없음|
|Medic|구조|B2 소생 처치|구조한 동료가 최대 체력의 50%로 일어납니다.|0.25 / B1|
|Medic|구조|B3 보호 조치|구조 후 보호가 1초 늘어납니다. 공격하면 끝납니다.|1 / B2|
|Medic|응급 대응|C1 안정화|동료를 구조하는 동안 쓰러짐 시간이 절반 속도로 줄어듭니다.|0.5 / 없음|
|Medic|응급 대응|C2 골든 타임|쓰러진 동료의 첫 구조 시작 시 남은 시간이 3초 늘어납니다. 한 번만 적용됩니다.|3 / C1|
|Medic|응급 대응|C3 위급 환자|체력 30% 이하인 자신이나 동료의 의약품 회복량이 15% 늘어납니다.|0.15 / C2|
|Explorer|광학|A1 원거리 관측|광학 도구 관측 거리가 추가로 20% 늘어납니다.|0.2 / 없음|
|Explorer|광학|A2 지형 판독|실제로 관측한 지형 표식의 종류를 표시합니다.|1 / A1|
|Explorer|광학|A3 관측 기록|팀 표식이 30초 더 오래갑니다.|30 / A2|
|Explorer|탐사|B1 광역 조사|조사 도구 범위가 추가로 25% 늘어납니다.|0.25 / 없음|
|Explorer|탐사|B2 정밀 조사|광물 방향 안내가 3초 더 오래갑니다.|3 / B1|
|Explorer|탐사|B3 신속 탐사|조사 도구 재사용 시간이 20% 줄어듭니다.|0.2 / B2|
|Explorer|신호|C1 강한 신호|신호 도구 범위가 추가로 25% 늘어납니다.|0.25 / 없음|
|Explorer|신호|C2 지속 신호|신호가 50% 오래갑니다.|0.5 / C1|
|Explorer|신호|C3 신호 구분|이동·위험·자원 중 팀 표식의 뜻을 고릅니다.|1 / C2|
|Engineer|정비|A1 손빠른 정비|수리 시간이 25% 줄어듭니다.|0.25 / 없음|
|Engineer|정비|A2 과잉 정비|임시 보강 한도가 기본 체력의 30%로 늘어납니다.|0.1 / A1|
|Engineer|정비|A3 현장 복구|마지막 피해 후 2초부터 수리를 시작합니다.|1 / A2|
|Engineer|자동화|B1 지속 과부하|포탑 과부하가 4초 오래갑니다.|4 / 없음|
|Engineer|자동화|B2 냉각 관리|과부하 재사용 시간이 6초 줄어듭니다.|6 / B1|
|Engineer|자동화|B3 정밀 구동|과부하의 장전·회전 속도 증가가 각각 10%p 늘어납니다.|0.1 / B2|
|Engineer|방어공학|C1 방벽 보강|벽·문·대문의 임시 보강 한도가 10%p 늘어납니다.|0.1 / 없음|
|Engineer|방어공학|C2 함정 정비|지속형 함정의 수리 시간이 15% 줄고 임시 보강이 10%p 늘어납니다.|0.1 / C1|
|Engineer|방어공학|C3 포탑 정비|포탑 수리 시간이 15% 줄고 임시 보강이 10%p 늘어납니다.|0.1 / C2|

Medicine 발신=(1+Medic치료+응급치료), 수신=(1+FieldCare+Survivor저체력보너스)를 곱한다. 현재 붕대/연고 ApplicationSeconds는0이고 강제 준비 시간을 추가하지 않는다. A2는 ConsumableKind.Medicine 공통 경계에서 사용 간격4초를3.2초로 줄이며 Self/Ally에 동일하게 적용한다. 음식의 기존 간격은 바꾸지 않는다. A1은 회복량+10%, A3는 HoT 총량을 유지하며 기간−25%다.

Quarry HUD는 방위/거리구간/체력구간만 제공한다. Explorer는 도구 category/IAimableExplorationTool을 사용하며 normal combat camera/simulation/chunk 상한을 바꾸지 않는다. [외부 사례·가격/XP 산술·검수](../validation/class-progression/REVIEW.md).

## Admission / receipt correction · 2026-10-22

기본 SurvivorClassAdmission은 원격 Owned/Selected/XP/Nodes claim을 권위로 사용하지 않는다. Host 자신만 자신의 store에서 읽는다. 원격 profile GUID는 routing 식별자이며 인증이 아니다. Host profile을 remote route로 주장하면 거절한다. IClassAdmissionVerifier는 향후 인증된 entitlement/progression provider의 교체 경계다. 현재 원격 유료 직업 인증 backend는 없다.

DevelopmentAlphaClassAdmission은 Editor/Development에서 명시 주입한 경우에만 claim을 canonical node/XP 규칙으로 검사한 세션 account를 만든다. Host 디스크에 원격 ownership/XP/nodes를 기록하지 않는다. ClassAdmissionLocks가 class/account/recent reward window를 같은 세션 재접속까지 보관하고 새 Host에서 지운다.

ClassProgressSession은 영구 프로필과 세션 전용 진행을 구분한다. RewardReceiptWindow는 HashSet+Queue512개 상한이다. 1차 once-only 경계는 LootOnDeath의 deathPresented/per-spawn ID 및 NetworkDeathResult.TryCommit이다. Client가 과거 receipt를 제출하는 RPC는 없다. Profile1의 옛 Receipts 필드는 checksum 검증 후 무시하고 다음 정상 Write에서 제거한다. Class 구매를 위한 월드 ClassPurchases는 별개이며 그대로 유지한다. Save13/Profile1/Protocol20 유지.

## Camp Journal UI

MainMenu 책 안 직업 기록, MP 책의 연결된 bookmark, Class 명단/큰 선택 직업/3갈래 아이콘 경로/상세 footer, Character sheet와 공용6Skill diagram이 기존 Lowfire 수첩을 공유한다. 직업/공용 node는 선택 ring·학습 seal·잠금 mark와 상태 문구를 사용한다. 선택/구매/투자/무료 재분배 권위 경계는 기존 Lobby 제한을 따른다. [수정 전 감사·외부6화면·검수](../validation/ui-redesign/REVIEW.md).

2026-10-23 시각 마감: Class 명단 상태 글씨를 카드 내부로 옮기고 공용 상태 label 높이32→40(폰트20 유지), Class6/공용6/node54는 UI 전용 잉크 atlas의 의미별 표식으로 표시한다. 잠금은 기존 mark와72% icon alpha로 구분한다. 책·명단/연결선 geometry·Character Sheet·능력/수치/Save13/Profile1/Protocol20은 유지한다. [실제 화면·관련 검수](../validation/journal-icon-finish/REVIEW.md).
