# 현재 코드 연결

2026-10-19 현재: generation2 / Save9 / Protocol11. v1~v8 지원, generation1 동결 reader 유지. 아래 v5~v8/구 Protocol 설명은 해당 단계 기록이다. 현재 동작은 WORLD_GENERATION/CONTENT_EXPANSION/RESOURCE_TEMPO 및 [통합 감사](GAMEFEEL_AUDIT.md)를 따른다.

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

WorldMap은 System.Random(seed)와 공통 물 경계를 사용하며 UnityRandom을 건드리지 않는다. 지형은 한 Texture/Sprite, 물은 몇 PolygonCollider, 자원259개와 최대480장식을 제한 생성한다. Camera와 Placement는 생성 Ground Bounds를 공유한다. 무한 스트리밍, 복잡한 NavMesh, 풀링 최적화 완료를 주장하지 않는다.

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

현재 III 확장: Save10/Protocol12, 새 world generation3. WorldChunkRuntime의 표시 전용 요청은 terrain/정적 POI만 만들며 simulation wanted와 분리한다. WorldTurret는 공용 TurretScheduler+InventoryModel+기존 projectile를 사용한다. NetworkTurret는 변경된 cargo/module state와 sampled heading, NetworkTierThreePlayer는 owner request 검증을 맡는다. BeaconDirectory는 휴면을 포함한 ID/위치/순번을 유지한다. 상세는 CONTENT_EXPANSION의 III 절을 따른다.
