# Lowfire

Unity **6000.3.19f1 LTS / Universal 2D**, 모바일 Landscape(1920×1080) 탑다운 생존 게임이다. 표시명 Lowfire, 내부 Wildfolk namespace·경로·저장 식별자는 유지한다. 현재 alpha 콘텐츠는 **Item136 / Recipe111 / Save14 / Profile2 / Protocol22**, Workbench I→II→III와 Wood/Stone/Iron/Steel/Diamond 장비다.

## 실행과 플레이

Unity Hub에서 `client/`를 열고 `Assets/Wildfolk/Scenes/MainMenu.unity`에서 Play한다. Continue는 검증된 저장, Single Player는 새 무작위 Seed 월드다. Gameplay 씬 직접 실행은 기본 Seed9472026이다. 신규 generation3는13120×13120유닛 유한 대륙, Grassland/Forest/Wetlands/Rocky와 Caverns를 제공한다. 기존 generation1/2의 지형과 자원 ID는 보존한다.

낮 채집·도구/시설 제작 → Copper/Tin/Bronze/Steel → Deep Caverns Diamond/Crystal → III 장비·탐험·방어로 이어진다. 밤 Zombie/Skeleton, 낮 Goblin 무리·Camp, 동물·야생 위험 생물13종을 제공한다. 신규8종은 Bat/Stone Crawler/Bog Leech/Wolf/Mountain Goat/Crow/Giant Boar/Crystal Crawler다. Bone/Chitin/Medicinal Gland는 탄약·함정·장비·치료·음식으로 소비된다. Goblin Camp 보급품은 거주 적 처치 후 획득하는 비재충전6칸 상자다.

Primary는 공격·도구·설치·선택 소비품·Armor 장착, Interact는 시설·문·물·상자다. Joystick/Sprint/Dash, Inventory36(Hotbar9+Storage27), 일반 Chest24/강화36칸, 드래그·합치기·소분·명시적 버리기를 제공한다. Inventory/제작/보급품 화면에서는 플레이 입력만 막고 세계는 진행한다. Singleplayer Pause/사망 선택은 정지한다.

기본 Workbench는 Wood6. 장비 업그레이드는 이전 장비를 재료로 소비한다. 설치 제작대 I→II는 Wood8+Stone6+Iron2, II→III는 Wood10+Stone10+Bronze4+Steel4+Crystal4+Diamond1이다. 별도 II/III Recipe는 없다. Furnace는 Iron/Copper/Tin/Bronze, Blast Furnace는 Steel, Campfire는 단순 가열·정수, Cooking Pot은 복합 조리, Turret Workbench는 포탑·탄약·모듈을 맡는다. 전체 비용/시간은 제작 정본을 따른다.

Telescope는 원거리 표시만 요청하며 simulation 범위를 늘리지 않는다. Compass/Home/동굴 Exit/선택 Beacon 안내, Torch와 Utility Lantern/Crystal Lantern을 제공한다. Ballista/Repeater는 실제 호환 탄약을 소비하며 모듈로 장전·조준·보관·회전·내구를 조합한다.

Hunger/Thirst, 재사용 Cup/Waterskin, 낚시, HP/Stamina 즉시 회복 음식·연고를 제공한다. 치료 효과가 있는 새 소모품은 공용4초 간격을 사용한다. 성장은 월드 캐릭터의 Level/6 Node Skill이며 영구 계정 성장과 구분한다.

## 저장과 협동

Save14는 기존1~13를 읽고 다음 정상 Save에서 이관한다. Seed·generation version·game time·자원/생물 logical delta·시설·queue·가방·장비·성장·Camp 보급 잔량을 저장한다. Editor는 프로젝트 UserData, 빌드는 persistentDataPath, 격리 QA는 전용 Captures 경로를 쓴다. 원래 파일명과 checksum/atomic replace/backup을 유지한다. Home Continue/Pause Save·Save & return home, 기기 Background/Quit 저장을 제공하며 플레이어용 Load 버튼은 없다.

Multiplayer는 Relay Room Code 또는 Local Network Host/Join이다. Host가 이동·전투·spawn/AI·채집·loot·가방·시설·상자·save를 결정한다. Client는 요청과 replica 표시다. 최대4인 admission 구조를 유지하며 실제4인 Relay·모바일·장시간 통합 검수는 별도다. Host 종료 시 세션도 끝나며 Host migration은 없다. Account/Cloud Foundation1은 아래 외부 설정이 완료되기 전 비활성이다.

## 정본·개발·검증

- 작업 전 [AGENTS](AGENTS.md)를 읽는다. main 직접 작업, canonical source 수정, PR/임시 branch 금지.
- [기획](docs/DESIGN_BIBLE.md), [콘텐츠/전체111 Recipe](docs/CONTENT_EXPANSION.md), [생물·드랍](docs/CREATURES_AND_LOOT.md), [제작](docs/CRAFTING.md), [음식](docs/FOOD_AND_FISHING.md), [수치](docs/BALANCE.md), [월드](docs/WORLD_GENERATION.md), [저장](docs/SAVE_PERSISTENCE.md), [구조](docs/ARCHITECTURE.md).
- [현재 Class Foundation 외부 비교·구현·경제·검수](validation/class-progression/REVIEW.md). 과거 validation/review는 당시 기록이다.
- 소스 검사: `python tests/validate_visual_project.py client`. Unity EditMode/PlayMode는 Assets/Wildfolk/Tests. 자동 검사와 실제 Play 검수는 구분한다.
- 신규 생물/아이콘은 기존 Pillow 방식의 `tools/generate_creature_loot_art.py` 원본이다. 기존 art 출처는 [SOURCE_ART](docs/SOURCE_ART.md). 외부 게임 asset/code/layout을 복제하지 않는다.
- Unity MCP/code는 허용한다. Windows 입력과 Focus/Show는 금지한다. View 메뉴는 기존 Game View만 Repaint하고 PlayUnfocused를 적용한다. 씬/에셋 재생성은 수작업 변경을 먼저 확인한다.

실제 터치·손맛·재미·장시간 경제·Android 성능은 사용자 검증 대상이다. BGM/SFX 콘텐츠는 비어 있다. 첫 보스 Undead King과 공용 Boss Foundation을 제공한다. Defense Objective/IV~VI/전기/총기/Pet/번식/상인/영구 Skin은 제공하지 않는다.

약초는 Forest/Wetlands에서 맨손으로 채집한다. 응급 붕대는 Hand에서 Fiber2+Herb1→20HP/8초, 기존 회복 연고는 Gland1+Herb1+Fiber2→즉시30HP다. 공용4초·붕대 비중첩, 자연회복/음식·기존 Station은 유지한다. [의약 조사·경제·검수](validation/medicine-expansion/REVIEW.md). Food Expansion2-B는 아래 Meal 선택을 추가했다.

기본식·일반 Stew·기능성 Meal을 구분한다. Cooking Pot의 사냥꾼 스튜(MaxHP+8),채집꾼 수프(안전 HP회복+0.15/초),원정 국물(Stamina회복+10%)은150초간 하나만 유지되며 교체된다. 기본 HP를 즉시 채우거나 Skill Rank를 올리지 않는다. Campfire/기존 의약·즉시회복 Broth는 유지한다. [음식 전체 감사·외부 비교·경제·검수](validation/food-expansion/REVIEW.md). 2-C는 사냥꾼 창1개를 WBII에 추가했다. Iron Spear1+Bone6+Leather2, 피해27/사거리3.35/공격간격0.90초이며 철창보다 약하고 느리다. Carapace Head는 지원 modifier 중복으로 보류, Vest/Skill/Meal은 유지한다. [2-C 감사·경제·검수](validation/creature-sidegrade-expansion/REVIEW.md). 2-D는 기존 Raw Fish+붕어/농어/고등어/장어의5 catch identity,수역/안쪽수면/밤 조건과Hand손질4개·Campfire소분식1개를 추가했다. 미끼/새Rod/Skill/Station0,Save11/Protocol16. [2-D 감사·외부비교·경제·검수](validation/fishing-aquatic-expansion/REVIEW.md). 다음 예정은2-E Exploration / Utility Expansion이다.

## Exploration / Utility 2-E

2-E 도입 당시 **Item134 / Recipe111 / Save11 / Protocol17**. WBII 광물 조사기(정지1.2초·반경18·보유 도구로 채취 가능한 광물8방향·6초 안내·12초 간격), WBI 신호 호각(근처 동료60m·20초·30초 간격)을 추가했다. 기존 Compass/Telescope/Beacon/Pan/3조명은 유지한다. Recall/Bedroll/Backpack은 보류. [감사·외부 비교·검증](validation/exploration-utility-expansion/REVIEW.md).

## Base / Defense / POI2-F

Campsite 한 번 보급,Well 공유3회/480초 회복 clean 물,Quarry 기존 유한 채광,Graveyard Skeleton1 처치 후 보급을 제공한다. Bone Trap은32피해 한 대상1회,Spike는16/1.2초 반복이다. 신규 item/recipe/포탑0,Save11/generation3 유지,Protocol18. 구 visited POI는 새 일회 보상을 받지 않고 Well은0에서 회복한다. [감사·외부 비교·경제·검수](validation/base-defense-poi-expansion/REVIEW.md). Content Expansion2는 여기서 마감하며 이후 Skill Point/Class/Job 기획→Undead King/Boss Foundation 순서다.

## Death / Revive / Respawn

SP 사망은 현장 광고(원정당 성공1회)·Premium Gem(횟수 제한 없음)·무료 Home 복귀로 나뉜다. 실제 광고는 미연결이다. Gem 지갑은 Foundation2의 온라인 계정 경계에 연결되며 외부 backend 설정 전에는 제품 계정 기능이 비활성이다. Crystal/Gold는 차감하지 않는다. MP는24초 Downed→동료4초 hold 구조(40%HP/2초 보호, 공격 시작시 종료) 또는 Dead→무료 Home이다. 구조는 release/이동/피격/거리/시야/무력화/연결 상실로 중단한다. 전체 down은 각자 Home으로 돌아갈 수 있게 Dead로 확정한다.

사망 HomeAnchor는 개인별 초기 WorldMap.Spawn이며 Workbench 방향 안내 Home과 별개다. 소지품·장비·성장·월드를 유지한다. Save12는 죽음·광고 사용·원정·Home을 저장하고 구조 진행/무적은 저장하지 않는다. Protocol19. [감사·외부 비교·거래 경계·검수](validation/death-revive-respawn/REVIEW.md). 다음 Class/Medic/Boss는 이번에 구현하지 않았다.

## Character / Class Foundation (2026-10-22)

Item134/Recipe111, generation3 유지. 공용 무제한Level·6Skill, 로컬 Class profile/6Class/54node, Repair/Dismantle/지원 능력. Save13/Profile1/Protocol20. [성장 정본](docs/CHARACTER_PROGRESSION.md), [근거·검수](validation/class-progression/REVIEW.md).

Class Foundation 검수: 관련 EditMode고유53, Class PlayMode고유9(관련실행합집합), 실제2프로세스LAN/4actor simulation/MCP1920×1080검수통과. Android/Relay/실제4인/실제Ads·IAP는미검증이다. [실제결과와한계](validation/class-progression/REVIEW.md).

Class 교정·Camp Journal(2026-10-22): remote admission 기본 Survivor/XP0/노드0, 명시 개발 verifier는 session-only, 처치 receipt 영속 제거·runtime512상한, Medic A2 의약 간격4→3.2초. MainMenu/Class/Character/공용Skill UI는 기존 수첩 계열로 재작성했다. Save13/Profile1/Protocol20 유지. [현재 교정·UI 검수](validation/ui-redesign/REVIEW.md).

## Undead King / Boss Foundation (2026-10-23)

기존 Graveyard 수호자 처치로 큰 왕묘가 영구 개방된다. 밤에 Bone10으로 소환하며 성공한 그 밤은 세계 전체에서 같은 보스를 다시 부를 수 없다. 다음 밤에 재시도한다. 보스 HP/피해/속도는1~4인 동일하고 Phase2 소환수만2/3/4/5마리다. 기존 방어 시설을 준비할 수 있다. 전원 이탈·전멸 시 걸어서 귀환하며 점진 회복하고, 낮이 되어도 진행 중 전투는 유지한다.

Core T1은 공유 물리 보상1개 확정이며 현재 Recipe가 없다. Crown은20% 공유 추첨, Head와 별개 Accessory1칸에 장착한다. 체력35% 이하에서 이동+8%/받는 피해−10%, 공격 보너스는 없다. 날짜/현재 시기 HUD를 사용한다. [설계·수치·실제 검증 및 한계](validation/undead-king-boss/REVIEW.md). 아래 날짜별 절은 각 패스 도입 당시 기록이다.

## Post-Boss Stabilization (2026-10-25)

LAN 검증 fixture의 잘못된 비활성 resource spawn을 제거하고 Host/Client 자원 집합·채집·청크 왕복·재접속·reHost 회귀를 추가했다. 주석 공급처의 지역 후보 소진 때만 실제 강변을 따라 제한된 결정적 fallback을 사용한다. 기존 정상 배치는 유지한다. Boss ClassXP는 Profile2의 최대1개 미완료 거래와 저장된 보상 확정을 대조해 복구한다. Save14/Protocol21/generation3 및 보스 수치·Item/Recipe는 유지한다. [원인·회귀·한계](validation/post-boss-stabilization/REVIEW.md).

## Launch Backend Foundation1 (2026-10-25)

Google Play Games v2 + 기존 Unity Authentication, 별도 CloudProfile1/계정 cache, Alpha 참여·영구 자격 이전 기반, Support ID·문의·audit/의심 flag·삭제 요청 UI와 서버 작업을 제공한다. 현재 UGS production 환경 한 개만 읽기 조회로 확인했고, Provider/Play Console 설정과 배포는 미확인이다.  BackendSettings.Enabled=false다. 로컬 SP/LAN/Host save는 유지하며 Save14/Profile2/Protocol21/generation3와 인게임 콘텐츠를 바꾸지 않는다. Alpha/현재 Production 현금 결제는 항상 불가하다.

[1.0 범위](docs/LAUNCH_SCOPE.md) · [운영/데이터 경계](docs/BACKEND_OPERATIONS.md) · [외부 설정](docs/SETUP_REQUIRED.md) · [출시 차단 항목](docs/RELEASE_BLOCKERS.md) · [검증 기록](validation/launch-backend-foundation/REVIEW.md).

## Launch Backend Foundation1.5 (2026-10-25)

현재 **PARTIALLY ACTIVE**: 실제 development/alpha 환경, Cloud Code2개, Protected profile/Support index·Access Control을 배포했다. development 실제 서비스76개, Unity SDK 왕복과2프로세스 LAN/인터넷 Relay를 확인했다. Android package는 사용자 확정 `com.gltam.lowfire`다. Google OAuth/PGS/Integrity·실제 Privacy/삭제 URL이 없어 제품 backend Enabled=false와 release gate는 유지한다. [현재 검증과 한계](validation/launch-backend-activation/REVIEW.md), [남은 설정](docs/SETUP_REQUIRED.md). Save14/Profile2/Protocol21/generation3와 인게임 콘텐츠는 유지한다.

2026-10-26 후속: 빈 미설정 PGS Android library를 정리하고 실제 **Development AAB(IL2CPP/ARM64, debug 서명)**를 생성·검사했다. 제품 backend와 Alpha/Production release gate는 계속 비활성/차단이다. [빌드·원인·한계](validation/launch-backend-activation/android-development/REVIEW.md).

## Launch Economy Foundation2 (2026-10-26)

계정 Gem/원장,22family58tier 업적(특수9), 신규 누적7회/월간28회/기간형 출석, Profile 수첩과 badge, Engineer120Gem·SP 부활8Gem을 연결했다. 초기0Gem, 업적213/신규45/월간40Gem. 실제 development 검증과 제품 활성화 제한은 [REVIEW](validation/launch-economy-foundation/REVIEW.md), [경제 정본](docs/ECONOMY.md)을 따른다. Save14/Profile2/generation3 유지, owner 진행 RPC 추가로 Protocol22다. Ads/IAP/Store/Audio/추가 progression은 구현하지 않았다.

Foundation2 현재 상태: **사용자 종료 요청에 따른 검증 중 체크포인트**. 최종 viewport 상세 Text 높이 검사가 남아 있으며 완료 선언 전 [재개 메모](validation/launch-economy-foundation/HANDOFF.md)를 먼저 따른다.
