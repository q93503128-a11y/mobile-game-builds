# Wildfolk 기획 정본

기준일2026-10-14. main의 실제 코드/데이터가 최우선 정본이다. Unity6000.3.19f1 LTS / Universal2D, 모바일 Landscape, 기준1920×1080.

## 방향과 아트

탑다운 생존 게임이다. Mine Survival의 낮 채집/밤 방어/생존 수치/제작/보관/기지라는 극초반 흐름을 강하게 참고한다. 캐릭터·맵·아트·UI·코드·고유 수치를 복제하지 않는다. 자연색·무광·원형 플레이어 양손/작은 눈·절제된 외곽선/그림자를 유지하고 볼터치/큰 눈/네온/장난감/어린이 마스코트 표현을 피한다. 현재 아트는 초기 시안이다.

1~4인 협동·무한 생존·월드 저장/이어하기·직업 특성은 향후 방향이며 현재 제공 기능이 아니다. 영구 능력치 성장 비중은 낮게 고려한다.

## 실제 월드 생성1차

192×144유닛 유한 월드, 실제 Biome는 Temperate 하나다. Seed가 강/호수/연못/길/숲·돌 밀도/자원 배치를 결정한다. 시작점은 빈 육지이며 근처 Wood/Stone과 수원을 보장한다. 깊은 물 충돌과 강 여울을 제공한다. Tree140/Stone55/Ore16/FallenLog12, 장식 최대480, Chicken1/Boar1을 생성한다. 자원/동물 재생성·영구 변경 저장·무한 Streaming은 없다. 세부 규칙은 WORLD_GENERATION을 따른다.

## 실제 극초반 진행

빈 Inventory36/Hotbar9, 맨손 시작이다. 일반 나무HP150/Wood6, 큰 FallenLog HP50/Wood4. Wood Axe는 일반 나무5회, Stone4회, Iron3회다. Workbench Wood6을 설치하고 Wood 도구·검각Wood5를 제작한다. Stone 업그레이드는 이전 Wood 장비1개+Stone4(검5), Iron은 이전 Stone 장비1개+Ingot3다. 직접 얻은 상위 장비가 하위 장비를 자동으로 지우지 않는다. 도구·무기 내구도는 사용하지 않는다.

Iron Ore는 Stone 이상의 Pickaxe가 필요하다. Furnace는 Stone8+Wood4, Ore최대3→6초→Ingot최대3. Campfire는 Wood4+Stone3, Raw1→5초→Cooked1. 현재 연료·처리 Queue·자동화는 없다. 결과는 Take로 수령하고 공간 부족이면 남겨둔다.

Workbench에서 Wall(Wood2/HP100), Door(Wood3/HP80), Chest(Wood6/HP70)를 제작한다. 기존 Placement의 거리/Footprint/충돌 검사와0/90도 벽·문 배치를 사용한다. 문은 열기/닫기, 점유된 닫기 거절, 좀비 차단/공격/파괴가 있다. 설치물 회수·수리·완성된 철거 UX는 없다.

## 입력·UI·Inventory

큰 Primary는 맨손/장비 공격, 설치, 선택 소비품 섭취만 담당한다. 작은 Interact는 시설/상자/문/물만 담당한다. 공격으로 기지 UI가 열리지 않으며 설치물을 자동 타격하지 않는다. 조이스틱/Hotbar/독립 가방/Pause/생존 및 작은 시간 HUD를 유지하고 상단 중앙은 비워둔다.

Player36칸 중0~8Hotbar,9~35Storage다. Chest는 독립24칸(3×8)이며 아래Player27+9와 함께 표시한다. 드래그 이동/합치기/교환, 선택 스택 Split half, 명시적인 Drop Zone에만 바닥 버리기를 제공한다. UI 밖 임의 드랍은 취소한다. Inventory/Chest는 입력만 막고 세계를 계속 진행한다.

Control Size80~140%, Master/BGM/SFX 로컬 설정을 유지한다. SFX/BGM 콘텐츠는 현재 무음이며 전자음/beep 임시 에셋을 다시 넣지 않는다. Android 실기 검증 완료를 주장하지 않는다.

## 낮밤과 적

Day240/Evening60/Night150/Dawn30초, URP2D 실제 조명을 부드럽게 바꾼다. Day에는 Zombie0. Night에는 카메라 밖/플레이어12밖/물·시설·안전빛 밖에서 짧게 올라온 뒤 활동한다. Dawn 시작 시 공격/추적 중단→잠복→자연 소멸하며 XP/Gold/Loot는 없다.

Night1~2 Target6,3~4=8,5~6=10,7~8=12, 이후2씩 증가하여HardCap18. 초기65%를0.9초 간격으로 분산 출현시키고 절반 미만으로 줄었을 때15초 간격으로 천천히 보충한다. 등록 GameplayLight의 안전 반경4/Strength1/Threshold.65를 사용하며 새 Aggro를 막고 기존 추적은2초 유예 후 포기한다. 실제 픽셀 색을 읽지 않는다.

Goblin은 낮 Road 기반 기본형1종, 최대3,35초마다30% Encounter 시도다. 경계/순찰하며 Territory2.5 깊은 접근이나 선공에 반응하고 Leash10을 벗어나면 돌아간다. Evening/Night 신규 생성을 멈추고 화면 밖 비전투 개체를 보상 없이 정리한다. 변형/마을/POI는 없다.

## 생존·Loot·진행 기반

Hunger/Thirst 각각Max100, 초당.08/.12 감소, 0에서각5초마다HP2 피해. 두 수치가70% 이상이고 최근 전투8초가 지나면HP.5/초 회복한다. 그 아래는 자연 회복하지 않는다. 치명적 상태15% 미만은 일반 이동90%; 향후 Sprint용 CanSprint/SprintEfficiency를 제공하되 실제Sprint/버튼은 없다.

닭/멧돼지는 중립 이동·도망, 공격 없음. LootTable로 닭Raw1+Feather1, 멧돼지Raw2+Leather1. Feather/Leather를 소비하는 Recipe는 없다. Enemy 직접 처치 시 ZombieXP5/Gold1, GoblinXP8/Gold3을 별도 세션 PlayerProgression에 기록한다. 아이템 스택이 아니며 현재 레벨/Skill/Shop/영구 계정 저장은 없다.

Armor 카테고리/방어 수치/빈 슬롯/외형 소켓만 있고 실제 Armor 아이템·장착 UI는 없다. 방어는 일반 피해 최소1을 유지하고 허기/갈증 피해는 우회한다.

## 향후 Progression 방향

General Workbench가 메인 제작 Progression Spine이다. WorkbenchTier/RequiredWorkbenchTier 기반을 두었으며 현재 실제 Tier1에서Wood/Stone/Iron을 만든다. 향후Workbench II/III 등이 일반 Recipe와 새로운 전문 제작대의 Recipe를 해금한다. 일반 재료 장비는 적절한 범용 작업대에서 Diamond 수준까지 확장할 방향이다. 실제Diamond는 없다.

후반 Weapon/Armor/Tool/Engineering/Defense 전문 제작대는 전문 장비·포탑·특수 능력/공격 방식/채집/방어 효과·던전 재료와 연결할 방향이다. 현재 실제 전문 제작대를 만들지 않았다.

Gold는 월드/캐릭터용 일반 재화다. 향후 Merchant/서비스 등이 후보이며 현재 사용처는 없다. Meta Coin은 향후 Account 단위 Skin/Class Unlock/Horizontal Unlock 등의 별도 영구 재화 방향이며 현재 시스템/드랍/저장은 없다. 향후 Monster/Challenge의 낮은 확률·소량 보상도 후보이며 현재 지급하지 않는다. Gold와 합치지 않는다.

월드 생성은 다른 핵심 콘텐츠/Armor/Skill/방어/POI 재료가 마련된 뒤 다시 다듬는다. 그때 여러Biome/Transition/Dungeon/Goblin Hut/Ruins/Camp/POI/Road망/자원Region/난이도 분포를 확장한다. 현재 하나의Biome 내부 밀도 변화를 여러Biome로 부르지 않는다.
