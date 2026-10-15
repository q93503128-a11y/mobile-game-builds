# 현재 코드 연결

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

세계 진행은 플레이어 입력 Running과 분리한다. Inventory는 세계를 진행하고, Singleplayer Pause/Defeat는 동결하며, Multiplayer 정책은 전역 동결을 하지 않는다. 실제 네트워크는 없다. 월드 Seed/Delta와 재화는 Save/Load v4(v1~v3 migration)이며 사용자 설정 PlayerPrefs 및 미구현 Account 저장과 구분한다.

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

Save/Load는 영구 저장 v4(v1~v3 호환)이며 Cloud/계정/실제 네트워크 권위를 구현하지 않는다. 미래 Unlock/무기 공격 방식/Vehicle/Animal Breeding은 데이터 확장 방향과 실제 제공 기능을 구분한다.

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

Build Settings는 MainMenu→Wildfolk다. FrontendSceneBuilder는 기존 Gameplay 씬/Prefab을 재생성하지 않고 공유 Rules 참조, Pause 행동 정리, 초상화 손 비율을 갱신한 뒤 MainMenu를 저장한다. Build Playable Scene의 마지막에도 Home 빌더를 호출하므로 재생성 후 시작 씬이 사라지지 않는다.

- SceneFlow: NewGame/Continue/Home/Restart/Load의 유일한 SceneManager.LoadScene 경계, 전환 중 중복 진입 차단, Time.timeScale 복원, 일회성 로드 Snapshot 전달.
- SaveGameRules: 기존 migration/SaveValidation/적 검증 코드를 WorldSaveService에서 분리해 Home도 공유한다. authored Player Prefab/World/Catalog/종별 데이터 참조만 보관하며 세계 GameObject를 만들지 않는다.
- SaveRepository.Default: 기존 Editor UserData/기기 persistentDataPath의 단일 파일 위치. SceneFlow.Repository는 같은 파일 접근을 다음 씬에 전달하며 별도 Save 형식이나 영구 캐시를 만들지 않는다.
- HomeScreen: Home/Settings/Multiplayer/ConfirmNew 상태와 버튼 동선, Home 재진입/Continue 클릭 때만 파일 검증. 언어 이벤트는 표시만 갱신한다. Update나 파일 polling은 없다.
- UserPreferences/GameAudio/EventSystem은 씬 소유다. DontDestroyOnLoad 서비스를 추가하지 않으며 이전 씬은 Single 모드로 정리된다.
- SurvivalUiBuilder.Settings는 Back 콜백을 받아 Home/Pause가 같은 SettingsPanel 제작 코드와 LocalSettings 키를 사용한다.

Quiet Woodland는 SpriteRenderer/Light만 사용하며 WorldGenerator,CreatureMember,Health,PrefabPool,Population/Combat은 없다. SafeAreaLayout은 기존 크기 변화 guard를 사용한다. QualityUiBuilder.AlignHands는 실제 Player 몸통/손 크기와 Facing 간격으로 초상화 손과 장갑 비율을 계산한다. 월드 손 크기/공격 모션은 변경하지 않았다.


Lowfire Home은 같은 FrontendSceneBuilder에서 제작한다. JournalAction은 이어하기/종이 위 보조 행동의 시각 표현만 담당하며 HomeScreen/SceneFlow/SaveRepository의 기존 기능을 재사용한다. ConfigurePauseActions는 Editor 씬 제작 시 이전 Load Game 오브젝트를 제거하고 네 행동을 재배치한다. Runtime 호환용 숨김/우회 컴포넌트는 추가하지 않는다. 쓰이지 않게 된 LoadFromMenu wrapper는 제거했으며 실제 WorldSaveService.Load는 유지한다. Krita 타이틀/수첩은 UI Sprite 두 개이고 월드 아트/시뮬레이션 코드는 변경하지 않았다.
