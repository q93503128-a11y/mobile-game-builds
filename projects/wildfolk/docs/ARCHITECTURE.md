# 현재 코드 연결

2026-10-23 현재: generation3 / Save14 / Protocol21, Item136 / Recipe111. v1~10 이관과 generation1/2 reader를 유지한다. 아래 날짜별 Foundation 절은 구조의 도입 기록이며 현재 데이터는 CONTENT_EXPANSION/CREATURES_AND_LOOT/SAVE_PERSISTENCE를 따른다.

시설 실행은 ProcessingScheduler의 공용0.1초 tick에 등록된 busy Host/SP 작업만 진행한다. HitFeedback는 피격 이벤트 동안 coroutine으로 동작한다. LootTable.Roll/ProcessingQueue.Costs/CavernNetwork의 exitNext가 중복 계산을 공유한다. UI는 visibility 변경에만 강제 layout, 처리 명칭은 recipe/language cache, LocalFirelight geometry는 definition 변경에만 설정한다. Save/Protocol 필드와 inactive logical advance는 유지한다.
Runtime/InputSystem/UGUI,Editor,EditMode/PlayMode Tests를 분리한다. 실제 수치는Data ScriptableObject가 정본이고 Editor는 없는 초기 데이터를 생성하며 기존 수치를 보존한다. 씬은 프리팹 연결과 월드Bootstrap을 저장하고 수동 테스트 월드 배치 코드는 제거했다.

| 책임 | 연결 |
|---|---|
| 월드 데이터 | BiomeDefinition/WorldGenerationDefinition/GenerationRule→WorldMap(seed) |
| 월드 연결 | WorldGenerator→WorldTerrainRenderer/물Collider/자원Prefab/수원/중립동물 |
| 정렬 | WorldSorting,BaseY DepthSort/SortingGroup |
| 이동/방향 | Device Adapter→PlayerInputCommands→PlayerMotor/Survival+Terrain Modifier,CharacterFacing |
| Primary | HoldAttackInput→PlayerPrimaryAction→Combat/Placement/Consume |
| Interact | InteractButtonView→PlayerInteraction→IPlayerInteractable |
| Inventory | InventoryModel36+선택→Hotbar/Inventory/Equipment |
| 드래그/소분 | SlotDrag/DragController→StackSplitPanel→공통 부분Transfer |
| 바닥 버리기 | InventoryDropZone→PlayerItemDrop→기존WorldPickup |
| 상자 | StorageChest24/PlayerStorage/ChestPanel,독립Model과공통Transfer |
| 제작/처리 | RecipeCatalog/자격→PlayerCrafting→TryExchange/StationProcessing→ProcessingQueue 또는Campfire Batch |
| 생존 | SurvivalDefinition/SurvivalModel→PlayerSurvival→Health 회복/피해 |
| 낮밤 | WorldClock/WorldTimeModel→URP GlobalLight/시설빛/TimeHud |
| Zombie | ZombieSpawner Budget→ZombieLifecycle Emergence/Active/Burrow→EnemyController |
| 빛 안전 | 등록GameplayLight/Definition→Spawn/새Aggro/기존추적Grace |
| Goblin | 별도GoblinSpawner Road Day Encounter→GoblinController Territory/Patrol/Leash |
| 중립 동물 | NeutralAnimal 이동/도망,Health+LootOnDeath |
| Loot/재화 | LootTable+Health 마지막직접공격자→WorldPickup/세션PlayerProgression XP/Gold |
| 설정/오디오 | LocalSettings/UserPreferences→ControlSizeLayout/GameAudio/L Language |
| UI 상태 | GameplaySession→WorldPauseController/입력차단/ModalLayout |

PlayerAction의 공격/상호작용 Context 혼합을 폐기하고 GUID를 보존해 Primary로 대체했다. 인터페이스는 PlayerInteraction을 받으며 선택 장비와 무관하게 시설을 찾는다. PlayerCombat은 설치물을 자동 타격하지 않고 Resource 채굴 Tier를 데이터로 확인한다.

Legacy generation1은 제한된 고정 영역을 생성한다. 현재 generation2/3은 독립 Seed domain과 WorldChunkRuntime의 가까운64unit 청크·행 단위 지형 생성/LRU를 사용한다. Camera/Placement/AI는 같은 terrain 경계를 참조한다. 무한 월드·복잡한 NavMesh를 사용하지 않는다.

Zombie/Goblin/Animal의 행동 책임은 분리했다. 자연 Despawn은 Health.Died를 호출하지 않아 Loot/XP/Gold를 지급하지 않는다. 직접 피해에 PlayerInventory를 기록하고 처치 때만 재화를 지급한다. 일반 피해 방어와 허기 피해의 방어 우회를 유지했다.

StationProcessing은 데이터의 Mode를 읽어 Campfire Batch 또는 순수 ProcessingQueue에 위임한다. Sequential은 예약 입력/현재 단위/완료 결과를 분리하고 InventoryModel.TryAddAmounts로 다중 재료 반환·출력을 원자적으로 처리한다. Recipe ID/시간/Result를 사용하며 광물 이름 분기가 없다. UI 텍스트는 수량/초 변화 시 갱신하고 매 프레임 임시 거래 배열을 만들지 않는다.

Inventory는 한 배열, Chest는 상자별 한 배열이다. Transfer는 두 모델을 변경한 뒤 알린다. 소분은 선택 수량을 들고 있는 UI 상태를 거쳐 명시적인 목적지로 부분 이동하고, Drop은 안전한 위치에 생성이 성공한 뒤 소비한다. Drag Ghost는 표시일 뿐 데이터 정본이 아니다. 상자 소분 선택도 테두리로 표시하고 이벤트는 종료 시 해제한다. UI 상태/Chest 변경 시 선택과 드래그를 취소한다.

세계 진행은 플레이어 입력 Running과 분리한다. Inventory는 세계를 진행하고, Singleplayer Pause/Defeat는 동결하며, Multiplayer 정책은 전역 동결을 하지 않는다. Player 상태와 Creature/전투 권위는 별도 Multiplayer 씬에서 제공한다. 현재 Foundation5는 Host 권위 Resource/시설/Building/Door/저장·재Host를 연결하고 scene NetworkObject로 terrain seed를 전달한다. 실제2인 통합 QA는 남아 있다. 월드 Seed/Delta와 재화는 Save/Load v6(v1~v5 migration)이며 사용자 설정 PlayerPrefs 및 미구현 Account 저장과 구분한다.

해상도 도구는 기존 Game View만 Repaint하고 Play Unfocused를 적용한다. Focus/GetWindow/Windows 입력 호출은 하지 않는다. 자동 테스트는 별도 hidden headless Unity와 내부 가상 포인터를 사용한다. 로그/검증 복사본은 Git에서 제외한다. 검수 사진의 시설/아이템/시간 fixture는 Play 종료 때 버린다.

## 기반 안정화 책임

- AttackProfile/CombatAffiliation: 공격 형상/대상 수/관계 필터. PlayerCombat은 선택·타격·모션, 입력 장치 API는 모른다.
- PlayerInputCommands/PlayerInputAdapter/IPlayerInputSource: Move/Primary/Interact/Inventory/Pause와 장치/자동 입력 분리.
- WaterContainerDefinition/LiquidState/ConsumableDefinition: 컵 상태 관계/소비 효과/예측 가능한 불이익. 기존 InventoryModel exchange와 StationProcessing을 공유한다.
- StackSplitPanel/InventoryDragController: 액션/수량 UI/가상 부분 Stack. 모델 변경은 목적지 배치가 성공할 때만 atomic하게 반영한다.
- WorldSaveService/SavedGame/SaveRepository/SaveValidation: Snapshot 구성, Version/검증, 원자 파일 교체와 backup, Seed+Delta 적용. ContentCatalog는 Item/Recipe 정본 자산의 참조 목록이다.
- ResourceIdentity/ResourceRegeneration: 안정 ID/변경 기록/게임 시간 재생. AnimalIdentity/AnimalPopulation: 종별 지역 Population/Group/새 ID 보충/Timer 저장.
- WorldNavigation/EnemySteering: 생성 지형/공통 장애물/Spawn 적합성/부드러운 분리/국소 우회.
- PrefabPool/IPoolReset: 반복되는 적/WorldPickup의 생명주기·HP·피격·AI·수량 초기화.
- Editor DeveloperHarness/Profile/Window/ProjectDataValidator: 제품 Runtime/HUD와 분리한 도구와 데이터 검사.

Save/Load는 영구 저장 v6(v1~v5 호환)이며 Cloud/계정 저장을 구현하지 않는다. 멀티 권위는 별도 NetworkSession이며 SP/MP가 같은 Repository·snapshot codec을 별도 파일로 사용한다. 미래 Unlock/무기 공격 방식/Vehicle/Animal Breeding은 데이터 확장 방향과 실제 제공 기능을 구분한다.

## 캐릭터 패스 책임

- CharacterGrowthDefinition/PlayerProgression: XP Curve/Level/Point/Rank/이벤트. Experience는 누적 획득,CurrentXP는 현재Level의 남은 XP다.
- CharacterStats: Skill로 계산되는 HP/Stamina 최대·회복·효율 Modifier의 집계 지점. 향후 Status Modifier를 이 경계에서 합성할 방향이며 빈 범용 Framework는 만들지 않는다.
- PlayerEquipment: Head/Body 장착,교체/해제 Inventory 교환,Defense/표현 이벤트. Health는 최소 피해/생존 우회를 유지한다.
- CharacterVisualLayers: Base Skin/Head/Body/Hand/단일Back Slot/Optional FX. 얼굴/손Facing과 기본몸통을 유지하며 외곽 장비만 방향을 따른다. 장비가 Skin Sprite를 덮어쓰지 않는다. Backpack/Cape/Premium/Transmog는 없다.
- LocomotionDefinition/StaminaModel/PlayerLocomotion: 설정/순수Stamina·Sprint/물리Dash. PlayerMotor가 이동을 연결하고 WorldNavigation은 자기Collider를 제외한 Sweep/지형을 검사한다.
- ProfileHud/CharacterScreen/SkillTreeScreen: Profile과 성장 정보/독립 Node Tree. InventoryEquipment만 장착·비교·해제를 담당하며 PlayerPortrait는 읽기 전용 표현을 공유한다. Character/SkillTree는 입력차단/세계진행이다.
- RecipePresentation/CraftingPanel: 고정정렬/Category/Filter/잠금/NEW. PlayerCrafting은 비용 소비/시설 Upgrade를 담당한다.
- GrowthValidation/WorldSaveService: XP총합·Point Budget·Rank·Armor Slot·Stamina 검증,v1→v2→v3→v4변환.

Health의 피해/회복,SurvivalModel의Hunger/Thirst와Raw Water 불이익,CharacterStats+Locomotion의이동/회복,CombatAffiliation+PlayerCombat의관계/타격 경계를 유지한다. 향후Poison/Bleed/Burn/Slow/Weakness/Curse/Defense Down,Cold/Dehydration/Food Poisoning/Water Sickness,Regeneration/Movement/Gathering Buff를 해당Modifier 경계로 확장한다. 확률 질병/주기피해/상태아이콘 콘텐츠는 없으며 Raw Water 동작은 유지했다.


ResourcePopulation은 종류·Region Count/Target/Pending/Harvest/Candidate Cursor를 관리한다. ResourceRegeneration은 Physics/Camera/지형 검사를 적용하고 WorldGenerator에 신규 ID 생성만 요청한다. SaveMigration은 v2 Delta를 밀도 회복 상태로 이관한다. Shared Motion Root는 Avatar Bob을 모든 Gear에 전달하고 Hand child는 공격 모션을 상속한다.

L/LanguageCatalog/LocalizedLabel은 Resources Localization 데이터와 즉시 Refresh를 담당한다. Catalog 행의 번역 수와 Placeholder를 검증한다. UserPreferences에 언어를 저장하며 WorldSave에는 넣지 않는다. 주요 동적 UI는 Model/언어 이벤트와 처리 중0.2초 Refresh를 사용한다. Runtime/Editor 경계는 유지하고 Resource 시간 가속·상태 확인 도구는 Developer Harness에만 둔다.


## 생태 패스 책임

- CreatureDefinition: Family/Faction/Traits/Habitat/LightSensitivity/Group·간격 데이터. CreatureMember: Group ID/Anchor/직접 이웃 경보/Cohesion. CampBound는 거주 개체의 AdditionalTraits로 지정한다.
- SoftSeparation: 생물 간 및 Player 접근 시 생물 쪽 위치 보정. NeutralAnimal: 종별 이동/도주 결과. GoblinController: 순찰/전투/Leash/제한 재배치. 분류 이름으로 런타임 행동을 분기하지 않는다.
- AnimalPopulation:4×3 서식지 목표/종별 Count/대기/후보/Group Spawn/새 ID. 고정 개별 Slot 부활 구현은 제거했다. WorldClock은 WorldGenerator보다 먼저 초기화한다.
- GoblinSpawner/GoblinIdentity: Road Warband/Camp 거주 예산/보충/저장 정체성. GoblinCamp는 실제 작은 천막·불·상자 Prop와 위치/보충 상태다.
- EnemyController: Zombie/Skeleton 공통 근접 준비→Swing→Hit→회복, Trait 기반 빛 계산. 기존 ZombieLifecycle 이름의 컴포넌트는 두 Night 종의 출현/잠복을 공유한다. CreatureVariant가 손의 Weapon/피해/Reach/Loot 차이를 담당한다.
- SlingshotAttack/StoneProjectile: 손·프레임·Band Pose와0.64초 Release/단일 발사, 풀링된 돌의 실제 충돌 피해. AI 코드 복제 또는 Player 탄약 아이템을 추가하지 않았다.
- PlayerEquipment.TryEquipFromSlot/Unequip: 버튼·Armor Drag·빠른 Use가 공유하는 슬롯 단위 교환. InventoryModel은 먼저 완전한 변경 가능성을 검사한다.
- GameplayUiLayers: 한 Canvas의 Gameplay→Modal→Modal Drag 순서/입력 차단. ModalLayout은 Safe Area 중심과 여백 안 Fit을 담당한다. 개별 sortingOrder 우회 없음.
- WorldDoor/WallVisual: 고정 Frame·경첩 Leaf·시간 Pose/Collider,인접 끝단 연결. Debug 더미 두 개는 DeveloperHarness에서 제거했으며 실제 생물로 Sandbox를 구성한다.
- WorldSaveService/SaveMigration: v4 종/Group/새 ID/동물 보충/Goblin Camp·Variant/Night 적 Variant를 저장하며 공격 중 Pose/Aggro/투사체는 재개하지 않는다.

## Home / 씬 전환

Build Settings는 MainMenu/싱글 Wildfolk/멀티 Multiplayer다. FrontendSceneBuilder는 기존 Gameplay 씬/Prefab을 재생성하지 않고 공유 Rules 참조, Pause 행동 정리, 초상화 손 비율을 갱신한 뒤 MainMenu를 저장한다. Build Playable Scene의 마지막에도 Home 빌더를 호출하므로 재생성 후 시작 씬이 사라지지 않는다.

- SceneFlow: NewGame/Continue/Home/Restart/Load의 유일한 SceneManager.LoadScene 경계, 전환 중 중복 진입 차단, Time.timeScale 복원, 일회성 로드 Snapshot 전달.
- SaveGameRules: 기존 migration/SaveValidation/적 검증 코드를 WorldSaveService에서 분리해 Home도 공유한다. authored Player Prefab/World/Catalog/종별 데이터 참조만 보관하며 세계 GameObject를 만들지 않는다.
- SaveRepository.Default: 기존 Editor UserData/기기 persistentDataPath의 단일 파일 위치. SceneFlow.Repository는 같은 파일 접근을 다음 씬에 전달하며 별도 Save 형식이나 영구 캐시를 만들지 않는다.
- HomeScreen: Home/Settings/Multiplayer/ConfirmNew 상태와 버튼 동선, Home 재진입/Continue 클릭 때만 파일 검증. 언어 이벤트는 표시만 갱신한다. Update나 파일 polling은 없다.
- UserPreferences/GameAudio/EventSystem은 씬 소유다. DontDestroyOnLoad 서비스를 추가하지 않으며 이전 씬은 Single 모드로 정리된다.
- SurvivalUiBuilder.Settings는 Back 콜백을 받아 Home/Pause가 같은 SettingsPanel 제작 코드와 LocalSettings 키를 사용한다.

Quiet Woodland는 SpriteRenderer/Light만 사용하며 WorldGenerator,CreatureMember,Health,PrefabPool,Population/Combat은 없다. SafeAreaLayout은 기존 크기 변화 guard를 사용한다. QualityUiBuilder.AlignHands는 실제 Player 몸통/손 크기와 Facing 간격으로 초상화 손과 장갑 비율을 계산한다. 월드 손 크기/공격 모션은 변경하지 않았다.


Lowfire Home은 같은 FrontendSceneBuilder에서 제작한다. JournalAction은 이어하기/종이 위 보조 행동의 시각 표현만 담당하며 HomeScreen/SceneFlow/SaveRepository의 기존 기능을 재사용한다. ConfigurePauseActions는 Editor 씬 제작 시 이전 Load Game 오브젝트를 제거하고 네 행동을 재배치한다. Runtime 호환용 숨김/우회 컴포넌트는 추가하지 않는다. 쓰이지 않게 된 LoadFromMenu wrapper는 제거했으며 실제 WorldSaveService.Load는 유지한다. Krita 타이틀/수첩은 UI Sprite 두 개이고 월드 아트/시뮬레이션 코드는 변경하지 않았다.


## Multiplayer Foundation 1

- NetworkSession: 단일 NetworkManager/UnityTransport의 Host/Join/approval/NGO 씬 로드/접속 해제/종료 책임. prefab 등록은 양쪽 프로세스에서 연결 시작 전에 수행한다. Manager 및 Session 한 root만 DontDestroyOnLoad로 씬을 넘는다. Host 종료 시 모두 종료하며 migration은 없다.
- IRoomConnection → RelayRoomConnection/LocalRoomConnection: 인터넷의 실제 Relay+DTLS 코드 발급/참가와 명시적인 IPv4 로컬 접속을 분리한다. IP를 Room Code로 표시하지 않는다. SDK의 익명 인증은 Relay 연결용이며 제품 Account/Login 기능이 아니다.
- NetworkPlayer: owner 입력 RPC만 수락하고 Host에서 기존 PlayerMotor/PlayerLocomotion/StaminaModel/Health/PlayerEquipment를 실행한다. NGO server NetworkTransform/NetworkRigidbody2D가 위치를 전달하고 Motion/Status 두 상태는 server write만 허용한다.
- PlayerInputCommands.MovementAuthority: 싱글은 기존 직접 적용, 멀티는 입력 전달 adapter. Primary/Interact가 없는 이동 전용 actor도 같은 입력 장치 adapter를 사용한다. 공통 Avatar의 읽기 전용 MovingOverride는 원격 표현용이며 이동 권위가 아니다.
- NetworkPlayer는 기존 Player의 prefab variant다. movement/armor 코드를 복제하지 않고 이번 범위 밖인 상호작용 component를 authoring에서 제거했다. replica의 Motor/Locomotion/Stats/Survival은 실행하지 않는다.
- MultiplayerArena/MultiplayerMenu는 상태·언어 이벤트로 UI를 갱신한다. 야영지는 static Sprite/경계이며 WorldGenerator/Population/Save/Creature AI/시설 시뮬레이션이 없다. WorldNavigation.StaticAreaSize는 같은 이동·대시 충돌 경계에 유한한 검수 구역을 제공한다.
- 씬 전환의 정책은 SceneFlow를 유지하고 네트워크 씬 로드는 공식 NGO SceneManager가 전파한다. Single Player/Continue는 기존 SceneFlow/SaveRepository 경로다.

전송 빈도·검증·제한과 미래 확장은 MULTIPLAYER_FOUNDATION.md를 따른다.

## Multiplayer Foundation 2

- NetworkCreatureWorld/NetworkSpawnFactory: 공통 World/AnimalPopulation/GoblinSpawner/ZombieSpawner의 Host 전용 시뮬레이션과 초기화 후 NGO Spawn 경계. Client는 정적 terrain과 replica만 제공한다. NetworkEcologyBuilder가 기존 prefab variants와 공유 데이터 참조를 authoring한다.
- SimulationAuthority/NetworkPlayer.Active: 기존 싱글 진입을 유지하면서 멀티 AI·group·population·damage 실행을 거절하고 Host에서 생존 Player를 선택한다. Windup target이 죽으면 추적을 초기화한다.
- NetworkCreature/CreatureIdentityState/CreatureFrame: Host transform, identity/pose/health/target/lifecycle 변경 상태와 client-only presentation. NetworkCamp는 camp/refill 읽기 상태, NetworkProjectile은 단일 Host 돌과 client trajectory visual이다.
- PlayerInputCommands.CombatAuthority/NetworkPlayer.AttackRpc: 소유자 공격 요청→Host의 공통 PlayerCombat 판정. Client가 damage/target/hit 결과를 보내는 경로는 없다. PlayerCombat.Present와 SlingshotAttack.PresentReplica는 피해 없는 공통 표현 경계다.
- Health/NetworkDeathResult: lethal latch와 replica health를 분리한다. LootOnDeath는 멀티에서 Host-only 결과 이벤트만 만들고 local pickup/reward를 지급하지 않는다. Ground Item/소유권/보상 지급은 다음 범위다.

이전 Foundation 1의 static 야영지 설명은 당시 구조다. 현재 Multiplayer는 기존 default-seed 지형 위 Host ecology를 사용한다. 구현/자동검사와 미수행 실제2인 통합 검증은 MULTIPLAYER_FOUNDATION을 따른다.

## Multiplayer Foundation 3

- NetworkResource: 기존 ResourceIdentity/Health/ResourceRegeneration의 Host 권위 상태와 static NGO spawn/Despawn. Client는 초기 자원을 자체 생성하지 않는다. 기존 Recovery Snapshot/새 p-ID를 유지하며 멀티 Save는 연결하지 않았다.
- NetworkGroundItem/WorldPickup: 공통 Drop 데이터/표현과 Host-only whole-stack claim. Trigger는 owner 후보만 등록하며 Inventory revision/범위 변화 때 요청을 허용한다. 로컬 pickup simulation은 실행하지 않는다.
- NetworkInventory/IInventoryAuthority: owner RPC·sequence·revision 검증, 서버 공통 Model/Equipment/Drop 실행, Owner-only NetworkList delta/선택/XP·Gold. 초기 snapshot과 개별 변경은 읽기 전용 모델에 적용한다.
- InventoryModel.TryDropSlot/PlayerItemDrop.BindWorld: 감소→생성 commit/rollback과 공통 월드 참조 경계다. 이전 OnSpawn local-owner만 바인딩하는 가정을 사용하지 않는다.
- NetworkInventoryUi/InventoryUiBuilder: 기존 UI에 actor를 주입하며 Foundation 3 당시에는 시설 없는 멀티 UI를 제공했다. 현재 Foundation 4는 아래 시설 UI를 함께 연결한다. 새로운 가방 디자인/별도 가방 모델은 없다.
- NetworkDeathResult.TryCommit/NetworkCreatureWorld.RecordDeath: 한 번 계산한 table 결과를 Ground와 근처24유닛 생존·접속 Player의 개인 지갑 지급으로 연결한다. 계정/Cloud ownership이 아니다.

이전 Foundation 2의 Resource/Item 제외 단락은 당시 범위다. 현재 구현과 실제2인 검증 미수행은 MULTIPLAYER_FOUNDATION을 따른다.


## Multiplayer Foundation 4

- NetworkFacility: 서버 소유 Chest NetworkList slot delta, Workbench Upgraded, ordered processing cargo delta/현재 작업·server end time/revision. 공통 StorageChest/StationProcessing의 모델이 실제 truth이며 Client는 읽기 전용 snapshot/presentation이다.
- NetworkFacilityInteractor: owner RPC/sender/sequence, 가방·시설 revision/연결·생존/거리/open context 검증. shared lock 없이 기존 Transfer/PlayerCrafting.Craft/StationProcessing을 Host에서 실행한다. Hand/Workbench 별도 제작 truth가 없다.
- StationProcessing.Changed/ProcessingQueue.Version/CraftingStation.Changed: 완료/예약/수령/blocked 경계 이벤트. Client Advance/Restore 권위와 표시용 snapshot 적용을 분리한다. UI timer tick은 진행 표시만 갱신한다.
- NetworkInventoryUi: PlayerStorage/PlayerCrafting/기존 Chest/Crafting/Interact와 모든 owner callback을 재바인딩한다. Host 승인 open의 view RPC 반향을 막고 Bag/Hand 전환에서도 Host facility context를 해제한다.
- NetworkEcologyBuilder/NetworkCreatureWorld: 기존 시설 variants5개 등록/Host 초기 배치. Protocol4/Player 포함23 prefab. Building/문/멀티 persistence는 별도 후속 범위다.

관련 자동검사와 실제2인 Relay 통합 QA 미수행은 MULTIPLAYER_FOUNDATION을 따른다.


## Multiplayer Foundation 5

- NetworkConstruction/PlayerPlacement: owner 미리보기·공통 지면/거리/충돌/방향 검사, Host revision/slot/연결 검증과 감소/NGO spawn commit·실패 rollback.
- NetworkBuilding/WorldDoor/WallVisual: server-write ID/type/HP/creator/door target·start/revision. 문은 timestamp 표현, Wall은 topology 변화 시 근처 연결 갱신. 플레이어 removal API는 기존에 없으므로 추가하지 않았다.
- WorldSnapshotCodec/PlayerSnapshotCodec: 기존 SP Capture/Apply를 추출한 공통 truth. NGO spawn factory/Release를 사용해 Host restore의 이중 생성·pending actor 누수를 막는다. Resource/Animal/Camp release와 Night restore lifecycle을 공통화했다.
- SavedGame v6/MultiplayerSaveRules: IsMultiplayer discriminant와 local participant snapshots,기존 player 검증/legacy migrations/checksum·atomic backup 재사용. SP world-v1.json과 Host multiplayer-world-v5.json을 구분한다.
- NetworkWorldPersistence/NetworkWorldState: Host capture/save/load/참가자 기억, in-scene seed sync. World Ready 전 Player spawn을 대기하고 restored ecology를 Start에서 중복 생성하지 않는다.
- NetworkSession: 로컬 Guid approval/중복 거절/actor retire와 replay-safe spawn, 유효한 저장 Continue→새 connection, 저장 성공 후 Shutdown·실패 시 세션 유지. UserData/Account identity/Relay allocation을 혼합하지 않는다.

정상 remote UI·scene/delta 수신/실제 rejoin·Relay 통합 QA는 다음 패스다. 이전 Foundation2~4의 제외 단락은 당시 범위이며 현재 정본은 MULTIPLAYER_FOUNDATION을 따른다.

## Workbench / 자원 공급 경계

WorkbenchProgression은1~6 검증·구성된 다음 단계·Recipe/전문 시설 지원 판정을 공유한다. CraftingStation은 해당 시설의 상태 변경만 통지한다. 근접 Workbench 지원 registry는 제거했다. InventoryModel.TryConsumeCosts(costs,commit)는 모든 재료 예약 후 시설 변경을 commit하며 실패/예외 시 슬롯을 복구한다. SP 교체와 NGO 동일 객체 전환은 기존 Save ID/Creator/HP 비율을 유지한다. NetworkFacility는 server-write int Tier를 사용하고 Protocol7으로 이전 bool layout과 세션 호환을 분리한다.

ResourceDefinition의 Tool/Rank와 ResourceNode.Harvest, WorldMap의 공급 문맥/독립 생성 stream, StationProcessing의 Recipe prerequisite는 각각 별도 경계다. 기존 Seed 자원 ID를 보존하고 공통 v6 codec을 사용한다. MetalProgressionAssets는 신규 authored assets/variant/기존 제작 화면만 갱신하는 Editor 빌더다. 자동 렌더 증거는 MetalRenderPlayTests이며 실제 MCP/Game View 조작 테스트와 구분한다.

## Exploration Foundation 1

ItemDefinition.PortableLight/HeldSprite→PlayerLightSource(held/equipped hook)→공유 LocalFirelight/GameplayLight. Torch는 DetersEnemies=false로 기존 AI의 시설 Safe/Dim 규칙과 분리한다. WorldLighting은 실제 Global Light를 참조하고 local player 영역의 cave ambient를 적용한다. 가까운 readability floor도 ambient에 따라 감쇠한다.

PlayerNavigation의 영속 truth는 PlayerSnapshot.HomeId 하나다. WorldPlaceable/sleeping Places에서 위치를 해결한다. NetworkNavigation은 선택 검증과 owner-only 파생 위치 cache를 제공한다. NavigationHud는 카메라 축의 북쪽/기지, 동굴 graph의 다음 출구 안내 지점만 표시한다. 기존 placement/transaction/Save codec을 재사용한다.

WorldChunkRuntime 하나의 bounded scheduler/ChunkActivation4unit retention을 사용한다. WorldTerrainRenderer는 활성 tiles와 inactive LRU24를 분리하고 row work queue를2.5ms 목표로 처리한다. 전체42k Update/GO/Texture/새 chunk manager는 없다. 명시적인 DrainPending은 fixture 동기화다. Editor 전용 ExplorationProfiler는 ProfilerRecorder와 frame 샘플을 파일로 기록하고 게임 HUD에 붙지 않는다.

멀티 야외 조명 시간은 기존 NetworkWorldState의 Host 시간 sample(Elapsed/Server timestamp/Advancing)을0.5초마다 전달하고 Client가 server time으로 보간한다. 멈춘 Client WorldClock 때문에 밤에도 낮 광원으로 표시되던 경계를 연결했다. 별도 매 frame light/position RPC는 없으며 실제 remote Relay 화면 동기화는 미검수다.

## Workbench I–II Content Expansion (2026-10-18)

ItemStack/SavedStack/NetworkStack/GroundItemState에 작은 Charges/Quality 값을 공통 전달한다. PlayerRanged는 기존 projectile, Spear는 기존 melee, Cooking Pot은 기존 처리기, Gate는 기존 WorldDoor를 재사용한다. Utility는 기존 equipment/PlayerLightSource, 새 자연 source는 기존 청크/자원 회복에 연결한다. Editor QA profile은 release에서 비활성이다.

## Resource Tempo (2026-10-18)

현재 공유/개인 필수와 선택 예산은 canonical recipe metadata와 batch 잔량으로 계산한다. 비용/결과는 기존 processing save와 동일하다. 광물 source당 수율·chunk population 보충·Protocol11은 [RESOURCE_TEMPO](RESOURCE_TEMPO.md)를 따른다.


## Workbench III 현재 상태 · 2026-10-19

III 확장 도입 당시: Save10/Protocol12, 새 world generation3. WorldChunkRuntime의 표시 전용 요청은 terrain/정적 POI만 만들며 simulation wanted와 분리한다. WorldTurret는 공용 TurretScheduler+InventoryModel+기존 projectile를 사용한다. NetworkTurret는 변경된 cargo/module state와 sampled heading, NetworkTierThreePlayer는 owner request 검증을 맡는다. BeaconDirectory는 휴면을 포함한 ID/위치/순번을 유지한다. 상세는 CONTENT_EXPANSION의 III 절을 따른다.


## 현재 Creature / Loot 확장

CreatureDefinition.Actions→WildlifeActionDefinition은 movement/attack/temperament/timing을 조합한다. 신규8종은 NeutralAnimal의 기존 FixedUpdate를 건너뛰고 WildlifeActionScheduler의0.05초/최대32개 round-robin으로 실행한다. Terrain/solid sweep·LOS·접촉을 공유하고 pulse는 고정32 collider buffer와 중복 Health 제거를 쓴다. Group 경보는 가까운 동일 pack에만1회 전달한다. 클라이언트는 NetworkCreature의 기존 phase/timestamp/facing/awakening frame으로 표현하며 행동 frame 최대10Hz·HP 변화 즉시다.

StreamedAnimalPopulation은 살아 있는+휴면 logical count, chunk sleeper index, deterministic 규칙 순서 분산과 expansion region 표시를 사용한다. 공유 활성80 cap·청크 quota·종 cap을 구분한다. 초기 적합 habitat의 충돌 실패는 회복 ticket으로 이어지며 tick은 최대32 ticket/2 group/24후보다. StreamedGoblinPopulation도 Camp/actor를 청크 index로 조회하고 휴면 거주 수를 유지한다. 전체 영속 상태 수에 비례하는 매 프레임 scan은 없다.

GoblinCamp는 IPlayerInteractable이며 StorageChest와 별개의6칸 보급품 truth를 가진다. NetworkCamp는 server-only Security/Opened/revision/변경 Loot list, NetworkFacilityInteractor는 owner RPC/sequence/가방·상자 revision/range/alive/space를 검사한다. 교환은 기존 InventoryModel.TryExchange와 일회 slot 소모를 사용한다. 개봉·잔량은 Save11이며 active/dormant capture를 deep copy한다. CampLootPanel은 take-only·Safe Area·localized labels와 기존 Inventory 입력 차단을 공유한다.

ConsumableDefinition.HealthRestore/StaminaRestore/UseCooldownSeconds→PlayerSurvival의 권위·alive·재료 소모 후 즉시 효과, ArmorDefinition.StaminaRecoveryBonus→CharacterStats 집계다. 복잡한 buff stack이나 새 processing engine은 없다. 신규 코드·아트 빌더/격리 QA는 Editor assembly와 runtime을 분리한다.

## Medicine2-A · 현재

ConsumableDefinition.Kind/GradualHealthRestore/HealingDuration은 Food와 Medicine 및 즉시/지속 효과를 표현한다. PlayerSurvival의 기존 원자적 소비·권위·alive·공용 nextConsume 경로를 재사용한다. TreatmentModel은 단일 transient 상태이며 누적 목표량의 정수 차이만 HP에 전달해 프레임별 반올림 손실·중첩을 방지한다. SP는 기존 Update, MP는 기존 server NetworkPlayer tick에서 진행한다. 기존 MP hunger/natural survival Update를 켜지 않는다. Client는 상태 종료 시각으로 표시만 한다. Owner Primary RPC는 요청 번호를 검사해 중복/지연 재생을 거절하며 피해/회복량을 client에서 받지 않는다. MedicineHud는 잔여 정수 초가 바뀔 때 작은 localized Text만 갱신한다. 새 Status Framework/Station/전역 resource scan은 없다.

## Food2-B · 현재

ConsumableDefinition.Meal→MealDefinition(효과1종/크기/기간),MealModel은플레이어당1상태다.PlayerSurvival의소비성공후교체·기존공용간격·AdvanceTemporary(SP Update/서버tick)가시간·사망을처리한다.CharacterStats가영구Skill+Meal최대HP/Stamina회복을집계하며Health.SetMaximumBonus는현재HP를증가시키지않고초과분만clamp한다.자연회복함수를분리해기존H/T/전투/RawWater조건과소수누적을SP·Host에공유한다.MP의기존Survival.Update는계속꺼져있고H/T소모를새로켜지않는다.Client는MealItem/종료서버시각과최종MaxHP를표시한다.영구Save는임시Meal상태를기록하지않는다.

MealHud는0.2초확인·정수초/아이템/메뉴표시상태변경시만Text/Icon을갱신한다.Pot대기열의음식효과누락은CraftingPanel.FoodPreview로영양·단일효과·교체/기간만기존빈영역에표시한다.새buffframework/Station/자원Update/전역scan없음.

Host의RawWater회복차단·소모불이익타이머는기존Model.AdvanceEffects로진행한다.MPH/T자연소모를켜지않으며SP구간별소모계산순서는보존한다.식사때문에일시차단이영구차단으로남지않는다.


## Fishing2-D · 현재

FishingLootTable의직렬화FishingCatchDefinition배열이waterflags/time/minimumshoreinset/weight/output/amount를정의한다.FishingWaterContext는기존WorldMap의실제수면query를합성하고PlayerFishing이cast시Host context/roll을확정한다.일회InventoryModel.TryExchange·phase변경이보상경계이며Use에서시간/이동/모달/장비를다시확인한다.PlayerSnapshotCodec.Apply와NetworkPlayerdespawn은낚시ResetTransient를호출한다.

새ingredientgroup언어없이Hand손질4개를기존exactIDRecipe로추가했다.RecipeDefinition.OptionalConversion은자연획득재의선택가공임을표시해ProgressionEconomy의유일필수제조경로에서제외한다.해당recipe에진행root수량을지정하면validator가거절한다.기존1P/4P II/III예산이동일함을검사한다.새런타임framework/skill/tree/station은없다.

Protocol16의ActivityState.CatchIndex/CatchCount는서버가결정한catch정체성을owner의작은아이콘/이름/수량에연결한다.NetworkActivities의ownerRPC는수역/가방오류안내만전달하며Client보상결정경로는없다.목록순서/필드를공유하는동일콘텐츠build를사용한다.

## Exploration Utility2-E

ItemDefinition.ExplorationUtility→ExplorationUtilityDefinition(Survey/Signal)과 PlayerExplorationUtility가 기존 PlayerPrimaryAction/owner Primary sequence를 재사용한다. Server만 수락·channel·cooldown·자원 query·신호 위치/만료를 결정한다. 새 NetworkExplorationUtility는 owner SurveyState(phase/item index/8sector/시각), shared ExplorationSignal(명시적 위치/시각/범위), owner SignalReady를 복제한다. Client는 resource query를 하지 않는다. 추가 RPC 입력 경로는 없다. NetworkAttackInput의 hold repeat에서 두 utility를 제외한다.

query는 channel 완료 때만 fixed128 OverlapCircle, terrain .5unit segment와 fixed32 barrier ray로 제한한다. 현재 active ResourceIdentity/Owner/살아 있는 Mineral·보유 도구 gate를 확인하며 saturation은 Crowded다. 전체 resource registry 순회/매frame scan/FindObjectsOfType/GenerateChunk/RequestScoutPresentation 호출이 없다. 장면 WorldGenerator 참조는 첫 접근 후 cache. peer 표시 조회는 NetworkPlayer.Active 최대4만 .15초 HUD tick에서 확인한다. 기존 north/한 arrow/text를 재사용한다.

## POI2-F 현재 연결

WorldGenerationDefinition.PoiContent→기존 anchor→WorldChunkRuntime.WorldChunkPoi→feature Site 논리 상태와 active PoiSite를 분리한다. Well은 지연 계산, Quarry는 기존 ResourceNode/ResourceIdentity를 사용하되 poi: ID를 embedded Nodes로 기록한다. 전체 POI의 frame scan/새 perPOI Update가 없다. 기존 중앙 청크 scheduler만 active guard 초기화를 재시도한다.

IWorldSupplies를 GoblinCamp/PoiSite가 구현하고 기존 CampLootPanel을 재사용한다. PlayerInteraction이 NetworkPoiSite를 시설 대상으로 허용하며 Well에서는 선택 물통의 자연 raw fill보다 우물 상호작용을 우선한다. NetworkFacilityInteractor의 owner request는 Client에서 전송 가능하고 서버 Execute에서만 Bag.CanOperate를 검사한다. NetworkPoiSite는 Host 초기화/잔량/보안 상태를 복제하고 Client는 roll/채광 생성/guard clear를 하지 않는다. 새 representation/RPC로 Protocol18,Save11/generation3,Item134/Recipe111이다.

## Death / Revive / Respawn (2026-10-22)

- PlayerDeath는 Alive/Downed/Reviving/Dead/Respawning과 개인 HomeAnchor/원정/광고 사용을 관리한다. GameplaySession은 입력/화면 상태이며 lifecycle의 권위가 아니다. SP는 Dead 직행, MP는 Downed/teammate policy를 사용한다. 구조자는 Alive인 채 SessionState.Rescuing으로 행동을 막는다.
- ReviveDefinition의24초/4초/2.6m/40%/2초를 사용하고 IReviveModifiers의 duration/speed/health 집계로 향후 Medic을 연결한다. class ID 분기는 없다. PartyWiped 이벤트는 world reset 없이 future encounter 경계를 제공한다.
- Host NetworkPlayer.LifeRpc는 owner/단조 sequence/target revision/range/LOS/HP/세션을 검증한다. 한 대상 한 구조자 lock,0.5초 heartbeat lease, timeout 우선·single commit. Client는 요청과 PlayerStatusState의 life/revision/server deadline/helping 표시만 제공한다.
- 기존 Health0/IsAlive 타깃 제외를 재사용한다. 부활 보호만 Health damage gate에 추가하며 실제 melee/ranged 시작에 보호를 끝낸다. reset은 기존 Meal/Treatment/fishing/survey/telescope/dash/ranged/combat 경계다. 이동·피격 등으로 hold를 해제하며 client도 정상 상태 복귀는 server replica를 기다린다.
- IRewardedReviveService는 availability와 실제 reward completion, IPremiumCurrencyWallet은 idempotent transaction+원자 debit/영속 revive commit 계약이다. 기본 공급자는 unavailable. Editor provider는 명시 주입, production provider/상점/계정은 없다. 미래 remote wallet은 durable receipt/reconciliation 계약을 구현한 뒤 활성화해야 하며 일반 Gem balance를 월드/Crystal/Gold에 저장하지 않는다.
- DeathHud는 기존 Canvas modal에 무료 Home을 항상 표시하고 MP Downed는 작은 banner/원정 표식/기존 navigation bearing으로 표시한다. 버튼/모달은 상태 이벤트, 시간·근처 구조 탐색은 제한된0.1~0.15초 갱신이다. 새 Canvas sorting override는 없다.
- SaveGameRules는 같은 seed/generation의 **불변 검증 지형만** 한 개 재사용한다. snapshot/player/거래/ID/타이머/Item 검증은 매번 실행한다. 새 seed/definition/generation 또는 Editor rules 재검증에서 지형을 교체한다.
- 복귀는 목적지 근처 청크를 제한적으로 materialize한 뒤 실제 terrain/solid 검증한다. 현장 보정은 최대3m, Home 막힘 보정은8m다. 실제 valid death 지점은 이동하지 않는다. NGO despawn callback에서는 참가자 snapshot만 기억하고 disconnect 파일 저장은 다음 프레임 수행한다.

## Class foundation boundaries

ClassCatalog(6 definition/54 node/milestones)→ClassProfiles(IAccountProgressionStore/IClassEntitlementStore/IClassPurchaseWallet)→admission lock→PlayerClass cached modifiers. Class identity는 PlayerClass에서만 판정하고 Combat/Medicine/Locomotion/Building/Exploration은 capability/modifier를 소비한다. SurvivorClassAdmission은 기본 remote claim을 Survivor/XP0/노드0으로 제한한다. Host 로컬만 자기 store를 읽고 DevelopmentAlphaClassAdmission은 명시 주입한 Editor/Development 세션 snapshot에만 적용한다. ClassAdmissionLocks/ClassProgressSession은 재접속 lock·remote ephemeral XP·bounded512 recent receipt를 유지하며 Host 디스크에 원격 claim을 쓰지 않는다. IClassAdmissionVerifier는 미래 인증 provider 경계다.

PlayerMedicine generic item category/application/HoT, PlayerBuildingService hold+atomic transaction, StructureMaintenance buffer/refund, PlayerTeamMarker 관측/LOS·만료를 담당한다. ClassHud는5Hz bounded context overlap, marker는NetworkPlayer.Active 최대4 순회. per-frame 전역 creature/placeable/POI 검색은 없다. marker 생성 순간의 관측된 landmark 표시만 확인한다.

JournalUiBuilder는 기존 paper/font/icon을 사용하는 소형 Editor authoring helper이며 세 장면에 UI를 직렬화한다. JournalSkillNode는 Class/common 트리 상태만 표현한다. 런타임 UI 재생성·새 NetworkBehaviour·wire 변경은 없다.

## Reusable Boss Foundation

- BossDefinition/BossAttack: vitality/공격 shape·range·windup·recovery·phase 조건·연계/소환/territory/보상 config.
- BossEncounter: Awakening→Chase/Windup/Strike/Recovery, Transition/Ritual, Returning, Dying/Ended. Host simulation만 target/피해/소환을 결정한다. 기존 Health/Navigation/시설 피해를 재사용한다.
- BossWorld: boss ID별 전역 night identity, 기존 POI guard-cleared와 왕묘 연결, offering/저장 rollback, 고정한 pending roll→한 세계 snapshot 보상 commit.
- BossMinion: 기존 Skeleton의 임시 encounter 소유권/PreferredTarget/이동 modifier/무보상·정리. 자연 population에 등록하지 않는다.
- NetworkBoss: 서버 HP/state/phase/attack/direction/시작 timestamp/marked actor,10Hz와 상태 변경 즉시. 초기화가 NetworkObject.Spawn보다 먼저이므로 pre-spawn부터 listening server clock을 쓴다. 기존 server NetworkTransform으로 이동한다.
- NetworkBossWorld/NetworkPoiSite: 소비한 밤/active 및 수호자 해제 replica. Client owner offering은 기존 NetworkFacilityInteractor RPC 검증 경계를 통한다.
- UndeadKingVisual/BossTelegraph: 표현 전용 분절 포즈/범위·짧은 물리 균열·사망/ash/loot pause. BossIdentity/BossHud/TimeHud는 기존 경계를 확장했다.
- AccessoryDefinition/AccessoryVisual: Head/Body/Utility와 별개 한 칸. 공용 장착 transaction/short catalog index replica/ID save, Anchor 설정에 따른 작은 부유 시각. 공격 stat을 더하지 않는다.

## Post-Boss Stabilization

ClassRewardPersistence/ClassXpIntent는 로컬 boss 보상에 한정한 bounded profile journal이다. ClassXpCommit의 in-flight 구간 동안 Profile2 PendingXP→Save14 resolved receipt→Profile2 XP+clear를 순서대로 확정한다. PlayerClass는 영속/ephemeral 경계를 유지하고 완료 XP를 cached ClassProgressSession에 적용한다. LocalAccountProgressionStore는 남은 intent만 on-read reconciliation하며, WorldSaveService/NetworkWorldPersistence는 실제 save 이벤트에서만 미완료 보상을 재시도한다. Boss 사망 보상 저장 실패의 재시도는 최대1초에1회다. 새 공용 Level/Skill/계정/cloud/Host migration 경계는 없다.

P0의 잘못된 inactive NetworkStone은 Editor BossLanQa.Freeze에서 pending root를 꺼 NGO behaviour 목록이0개가 된 검증 fixture 결함이었다. runtime ResourceNetworkState/authority/packet 크기는 그대로다. ResourceReplicationQa는 실제 observer set/ID/rule/HP·채집·청크·late join·reHost를 비교하는 Editor-only 회귀이며 출하 scene에 붙지 않는다.

## Launch Backend Foundation1

UgsBootstrap은 기존 Relay와 계정의 인증 초기화를 공유한다. GooglePlayGamesIdentityProvider→Unity Authentication PlayerId→Cloud Code LowfireAccount→Protected CloudProfile1 경계다. world participant GUID/Host Save14와 account ID는 분리한다. AccountCloudCache는 guest 원본과 별개 디렉터리에 snapshot을 materialize하고 ClassProfiles의 기존 store 경계에 붙는다. local 변경은 dirty·ClassRevision 기반 명시 sync, conflict는 자동 merge하지 않는다. server premium entitlement와 unverified player-host Class mirror를 구분한다. 개발 adapter는 명시 격리/채널 제한이며 release build에서 차단한다. [운영 정본](BACKEND_OPERATIONS.md), [외부 설정](SETUP_REQUIRED.md), [1.0 범위](LAUNCH_SCOPE.md).

## Launch Backend Activation1.5

UgsBootstrap은 backend Enabled와 무관하게 verified channel mapping으로 SetEnvironmentName하고 Auth가 실제 반환한 환경 ID를 대조한다. Relay/Cloud는 검사 후 실행하며 잘못된 mapping의 production fallback은 없다. 서버는 런타임/128KB 제한에 맞춰 LowfireAccount와 LowfireIntegrityVerify로 분리한다. browser crypto 번들·Protected CAS·비밀이 아닌 context correlation·같은 player token의 서버 간 proof 호출을 사용한다. 실제 development/alpha 배포와 검증/미검증 경계는 [활성화 REVIEW](../validation/launch-backend-activation/REVIEW.md)를 따른다. World authority/participant GUID/Host Continue/Save14/Profile2/Protocol21은 바꾸지 않았다.

## Launch Economy Foundation2 · 현재 경계

경제는 EconomyService/UGS EconomyTransport → LowfireEconomy → Protected lf_economy 단일 CAS 정본이다. EconomyJournal은 offline outbox/cache/revive intent이며 wallet 권위가 아니다. existing NetworkCommonGrowth에 server→owner event RPC를 추가해 Protocol22를 사용한다. UI는 EconomyUiBuilder로 기존 MainMenu에 직렬화한 수첩이며 이벤트 때 갱신한다. [경제 전체 규칙](ECONOMY.md) · [실행·검증](../validation/launch-economy-foundation/REVIEW.md).
