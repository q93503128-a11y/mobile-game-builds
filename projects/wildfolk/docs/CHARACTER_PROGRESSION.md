# Character / Armor / Locomotion

현재 CharacterGrowth/Locomotion/Armor/Recipe 에셋이 수치 정본이다. Level/Point/Rank는월드캐릭터에속하고영구계정성장이아니다.

Level1~20, 필요XP35+(Level−1)×20,Up마다Point1. 남은XP를유지하며상한이후XP는같은Level에쌓는다. 현재Zombie5/Goblin8 XP보상을변경하지않았다. Rank3인Vitality(MaxHP+10)/Endurance(MaxStamina+15)/Recovery(+15%)/Steady Stride(+12%)를Point1로투자한다. 부족/최대Rank면거절하고Respec은없다. HP투자는최대량만증가하며무료즉시회복을추가하지않았다.

Profile Icon은 Character Information을 연다. Level/XP/HP/Defense/Stamina/회복·소모 수치와 보유 Point를 보여주고 독립 Skill Tree로 이동한다. 세 탭과 Profile 장비 목록은 제거했다. 장착/비교/해제는 Inventory의 Head/Body 슬롯 한 곳에서만 수행한다. 메뉴에서는 플레이 입력을 차단하고 세계 진행은 유지한다.

실제 노드 4개와 연결선을 사용한다. Vitality와 Endurance는 처음부터 투자 가능하며 Recovery/Steady Stride는 Endurance Rank1이 선행 조건이다. Rank당 Point1/최대3은 유지한다. Locked/Available/Learned/현재 Rank/비용/상세/투자 불가 사유를 표시한다. v2에서 이미 습득한 Rank는 선행 조건이 없더라도 보존하고 추가 Rank 투자를 허용한다. Sprint/Dash 자체는 처음부터 사용 가능하다. 가짜 미래 노드는 없다.

Skin과Gear는별도렌더러다. 모두 공통 Motion Root 아래에서 Walking Bob/Scale을 공유하고 Hand Gear는 실제 Hand의 자식이다. Base Skin/Head Gear/Body 외곽/Hand Gear/단일Back Slot/Optional FX로구분한다. 기본몸통은회전하지않으며Eyes/Hands와외곽Gear가Facing을읽게한다. Backpack/Cape/영구Skin/Transmog는콘텐츠가없고Back Slot은향후한개만장착하는방향이다.

Stamina는Sprint/Dash공통이다. 보행은무료,Joystick0.9이상0.4초로Sprint,유지0.72,정지/고갈/심한허기·갈증에서Walk로전환한다. 고갈후20회복과새Hold가필요하다. Dash는이동입력또는Facing으로Cost24/거리2.6/시간0.18/Cooldown0.55다. 장애물/깊은물/모든메뉴입력차단을유지하고무적·타격은없다. Stamina는 Player 아래쪽 120도 Arc로 표시하며 몸통/손/머리 위 HP를 피한다. 좌상단 Stamina는 제거했다. Full 회복 후0.7초 뒤fade하고 메뉴 중에는 숨긴다. Dash Icon은 전진 Double Chevron이다.

Developer Test Console은 Editor 전용이다. 현재 Home/협동/III 장비/탐험/포탑은 CONTENT_EXPANSION을 따르며 계정/Pet/MetaCoin/영구Skin/번식은 없다.

향후Status Effect는CharacterStats의Modifier집계와Health/Survival/Locomotion/Combat경계를사용한다. Raw Water현재불이익은유지하고확률Water Sickness/Raw Meat Food Poisoning은이번에없다. 구체적인원인·기간·주기·중첩은후속콘텐츠패스에서결정한다.

현재Info/Skill/Armor시각과Sprint/Dash엄지위치·성장/제작속도는사용자테스트대상이다. 현재외형은자체초기아트시안이며최종상품품질을주장하지않는다.


Armor는 동일 PlayerEquipment의 Inventory Drag/빠른 Use/슬롯 교환을 공유한다. 큰 Character/Tree/Inventory는 Safe Area 중심이며 뒤 Gameplay UI 입력을 차단한다. Save11(1~10 이관)에 성장/장비/Skill/Seen/Stamina를 보존한다.

## 현재 제작 진행 범위

실제 Workbench I~III, 구조적 상한VI다. Equipment Wood/Stone/Iron/Steel/Diamond, Armor는 Leather/Iron/Steel/Diamond의 Head/Body8개와 Carapace Vest1개다. Copper/Tin/Bronze는 풀 장비 tier가 아니다. Vest는 WB II Chitin4+Leather1+Fiber2, 방어3(철 Body4보다 낮음)과 Stamina 회복속도+15%의 선택 장비다. Skill Recovery와 가산 집계하며 즉시 Stamina를 지급하지 않는다. 현재 Home/Multiplayer·Save11은 README/SAVE_PERSISTENCE를 따른다.

## Food2-B와 성장 경계

CharacterStats는PersistentHealthBonus(Skill)와일시Meal을구분해합친다.Hunter+8은Vitality한rank+10보다작으며현재HP를회복하지않는다.Vitality1이면Max110→Meal118→만료110이며Rank/Point를바꾸지않는다.Trail+10%는Recovery한rank+15%보다작고기존Armor/Skill과가산된다.Forager는기존안전HP회복에만+0.15/s이며Stamina Recovery Skill을복제하지않는다.Endurance최대Stamina/SteadyStride비용효율/Dash/Sprint해금은Meal로대체하지않는다.150초종료/교체/죽음/Load/reHost시임시효과제거,영구성장은보존한다.

## Sidegrade2-C · 성장 경계

새장비는거리유지형사냥꾼창1개이며HP/MaxStamina/Recovery/이동modifier를추가하지않는다. Iron보다피해·속도·조준폭이낮고Range가길다. Armor는기존9개그대로이며CarapaceHead보류. Vest15%는Skill/Meal과기존가산,Defense3/철Body4의손해를유지한다. 장비/Skill/Meal을분리한계산과보류이유는 [2-C REVIEW](../validation/creature-sidegrade-expansion/REVIEW.md).
