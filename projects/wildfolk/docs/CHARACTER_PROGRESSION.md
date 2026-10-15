# Character / Armor / Locomotion

현재 CharacterGrowth/Locomotion/Armor/Recipe 에셋이 수치 정본이다. Level/Point/Rank는월드캐릭터에속하고영구계정성장이아니다.

Level1~20, 필요XP35+(Level−1)×20,Up마다Point1. 남은XP를유지하며상한이후XP는같은Level에쌓는다. 현재Zombie5/Goblin8 XP보상을변경하지않았다. Rank3인Vitality(MaxHP+10)/Endurance(MaxStamina+15)/Recovery(+15%)/Steady Stride(+12%)를Point1로투자한다. 부족/최대Rank면거절하고Respec은없다. HP투자는최대량만증가하며무료즉시회복을추가하지않았다.

Profile Icon은 Character Information을 연다. Level/XP/HP/Defense/Stamina/회복·소모 수치와 보유 Point를 보여주고 독립 Skill Tree로 이동한다. 세 탭과 Profile 장비 목록은 제거했다. 장착/비교/해제는 Inventory의 Head/Body 슬롯 한 곳에서만 수행한다. 메뉴에서는 플레이 입력을 차단하고 세계 진행은 유지한다.

실제 노드 4개와 연결선을 사용한다. Vitality와 Endurance는 처음부터 투자 가능하며 Recovery/Steady Stride는 Endurance Rank1이 선행 조건이다. Rank당 Point1/최대3은 유지한다. Locked/Available/Learned/현재 Rank/비용/상세/투자 불가 사유를 표시한다. v2에서 이미 습득한 Rank는 선행 조건이 없더라도 보존하고 추가 Rank 투자를 허용한다. Sprint/Dash 자체는 처음부터 사용 가능하다. 가짜 미래 노드는 없다.

Skin과Gear는별도렌더러다. 모두 공통 Motion Root 아래에서 Walking Bob/Scale을 공유하고 Hand Gear는 실제 Hand의 자식이다. Base Skin/Head Gear/Body 외곽/Hand Gear/단일Back Slot/Optional FX로구분한다. 기본몸통은회전하지않으며Eyes/Hands와외곽Gear가Facing을읽게한다. Backpack/Cape/영구Skin/Transmog는콘텐츠가없고Back Slot은향후한개만장착하는방향이다.

Stamina는Sprint/Dash공통이다. 보행은무료,Joystick0.9이상0.4초로Sprint,유지0.72,정지/고갈/심한허기·갈증에서Walk로전환한다. 고갈후20회복과새Hold가필요하다. Dash는이동입력또는Facing으로Cost24/거리2.6/시간0.18/Cooldown0.55다. 장애물/깊은물/모든메뉴입력차단을유지하고무적·타격은없다. Stamina는 Player 아래쪽 120도 Arc로 표시하며 몸통/손/머리 위 HP를 피한다. 좌상단 Stamina는 제거했다. Full 회복 후0.7초 뒤fade하고 메뉴 중에는 숨긴다. Dash Icon은 전진 Double Chevron이다.

Developer Test Console은Editor전용이고Early/Stone/Iron/Workbench II프로필을공유한다. Main Menu/World Select/계정Pet/MetaCoin/영구Skin/번식/고티어/전문Bench/Turret/Dungeon/NewBiome는구현하지않았다.

향후Status Effect는CharacterStats의Modifier집계와Health/Survival/Locomotion/Combat경계를사용한다. Raw Water현재불이익은유지하고확률Water Sickness/Raw Meat Food Poisoning은이번에없다. 구체적인원인·기간·주기·중첩은후속콘텐츠패스에서결정한다.

현재Info/Skill/Armor시각과Sprint/Dash엄지위치·성장/제작속도는사용자테스트대상이다. 현재외형은자체초기아트시안이며최종상품품질을주장하지않는다.


이번 생태 패스는 새 Armor Tier를 추가하지 않는다. 기존4개를 Inventory Drag/빠른 Use로 장착하고 동일 PlayerEquipment의 슬롯 교환을 공유한다. 큰 Character/Tree/Inventory는 Safe Area 중심이며 뒤 Gameplay UI는 입력이 차단된다. Save v4(v1~v3 이관)에도 성장/Armor/Skill/Seen/Stamina를 유지한다. Iron을 최종 Progression으로 고정하지 않는다.
