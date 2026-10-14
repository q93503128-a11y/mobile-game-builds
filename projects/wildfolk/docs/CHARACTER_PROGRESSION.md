# Character / Armor / Locomotion

현재 CharacterGrowth/Locomotion/Armor/Recipe 에셋이 수치 정본이다. Level/Point/Rank는월드캐릭터에속하고영구계정성장이아니다.

Level1~20, 필요XP35+(Level−1)×20,Up마다Point1. 남은XP를유지하며상한이후XP는같은Level에쌓는다. 현재Zombie5/Goblin8 XP보상을변경하지않았다. Rank3인Vitality(MaxHP+10)/Endurance(MaxStamina+15)/Recovery(+15%)/Steady Stride(+12%)를Point1로투자한다. 부족/최대Rank면거절하고Respec은없다. HP투자는최대량만증가하며무료즉시회복을추가하지않았다.

Profile는큰외곽패널없이원형초상/Level/HP/얇은XP/Hunger·Thirst/Point알림을둔다. Character의Information/Equipment/Skills는게임세계는진행하고플레이입력은막는다. Equipment에서Head/Body교체·해제,현재Weapon/Tool을확인한다. 가방공간없으면해제하지않는다.

Skin과Gear는별도렌더러다. Base Skin/Head Gear/Body 외곽/Hand Gear/단일Back Slot/Optional FX로구분한다. 기본몸통은회전하지않으며Eyes/Hands와외곽Gear가Facing을읽게한다. Backpack/Cape/영구Skin/Transmog는콘텐츠가없고Back Slot은향후한개만장착하는방향이다.

Stamina는Sprint/Dash공통이다. 보행은무료,Joystick0.9이상0.4초로Sprint,유지0.72,정지/고갈/심한허기·갈증에서Walk로전환한다. 고갈후20회복과새Hold가필요하다. Dash는이동입력또는Facing으로Cost24/거리2.6/시간0.18/Cooldown0.55다. 장애물/깊은물/모든메뉴입력차단을유지하고무적·타격은없다. Full이면Hud를숨기며사용후회복/0.7초대기뒤fade한다.

Developer Test Console은Editor전용이고Early/Stone/Iron/Workbench II프로필을공유한다. Main Menu/World Select/계정Pet/MetaCoin/영구Skin/번식/고티어/전문Bench/Turret/Dungeon/NewBiome는구현하지않았다.

향후Status Effect는CharacterStats의Modifier집계와Health/Survival/Locomotion/Combat경계를사용한다. Raw Water현재불이익은유지하고확률Water Sickness/Raw Meat Food Poisoning은이번에없다. 구체적인원인·기간·주기·중첩은후속콘텐츠패스에서결정한다.

현재Info/Skill/Armor시각과Sprint/Dash엄지위치·성장/제작속도는사용자테스트대상이다. 현재외형은자체초기아트시안이며최종상품품질을주장하지않는다.
