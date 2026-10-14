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
| 제작/처리 | RecipeCatalog/자격→PlayerCrafting→TryExchange/StationProcessing Batch |
| 생존 | SurvivalDefinition/SurvivalModel→PlayerSurvival→Health 회복/피해 |
| 낮밤 | WorldClock/WorldTimeModel→URP GlobalLight/시설빛/TimeHud |
| Zombie | ZombieSpawner Budget→ZombieLifecycle Emergence/Active/Burrow→EnemyController |
| 빛 안전 | 등록GameplayLight/Definition→Spawn/새Aggro/기존추적Grace |
| Goblin | 별도GoblinSpawner Road Day Encounter→GoblinController Territory/Patrol/Leash |
| 중립 동물 | NeutralAnimal 이동/도망,Health+LootOnDeath |
| Loot/재화 | LootTable+Health 마지막직접공격자→WorldPickup/세션PlayerProgression XP/Gold |
| 설정/오디오 | LocalSettings/UserPreferences→ControlSizeLayout/GameAudio Master×채널 |
| UI 상태 | GameplaySession→WorldPauseController/입력차단/ModalLayout |

PlayerAction의 공격/상호작용 Context 혼합을 폐기하고 GUID를 보존해 Primary로 대체했다. 인터페이스는 PlayerInteraction을 받으며 선택 장비와 무관하게 시설을 찾는다. PlayerCombat은 설치물을 자동 타격하지 않고 Resource 채굴 Tier를 데이터로 확인한다.

WorldMap은 System.Random(seed)와 공통 물 경계를 사용하며 UnityRandom을 건드리지 않는다. 지형은 한 Texture/Sprite, 물은 몇 PolygonCollider, 자원259개와 최대480장식을 제한 생성한다. Camera와 Placement는 생성 Ground Bounds를 공유한다. 무한 스트리밍, 복잡한 NavMesh, 풀링 최적화 완료를 주장하지 않는다.

Zombie/Goblin/Animal의 행동 책임은 분리했다. 자연 Despawn은 Health.Died를 호출하지 않아 Loot/XP/Gold를 지급하지 않는다. 직접 피해에 PlayerInventory를 기록하고 처치 때만 재화를 지급한다. 일반 피해 방어와 허기 피해의 방어 우회를 유지했다.

StationProcessing은 ParallelCapacity/Speed로 Batch 재료를 원자적으로 소비하고 결과를 보존한다. 한 Batch만 처리하며 Cooking/Smelting의 Station과 Recipe를 구분한다. UI 텍스트는 수량/초 변화 시 갱신하고 매 프레임 임시 거래 배열을 만들지 않는다.

Inventory는 한 배열, Chest는 상자별 한 배열이다. Transfer는 두 모델을 변경한 뒤 알린다. 소분은 선택 수량을 들고 있는 UI 상태를 거쳐 명시적인 목적지로 부분 이동하고, Drop은 안전한 위치에 생성이 성공한 뒤 소비한다. Drag Ghost는 표시일 뿐 데이터 정본이 아니다. 상자 소분 선택도 테두리로 표시하고 이벤트는 종료 시 해제한다. UI 상태/Chest 변경 시 선택과 드래그를 취소한다.

세계 진행은 플레이어 입력 Running과 분리한다. Inventory는 세계를 진행하고, Singleplayer Pause/Defeat는 동결하며, Multiplayer 정책은 전역 동결을 하지 않는다. 실제 네트워크는 없다. 월드 Seed/Delta와 재화는 Save/Load v2(v1 migration)이며 사용자 설정 PlayerPrefs 및 미구현 Account 저장과 구분한다.

해상도 도구는 기존 Game View만 Repaint하고 Play Unfocused를 적용한다. Focus/GetWindow/Windows 입력 호출은 하지 않는다. 자동 테스트는 별도 hidden headless Unity와 내부 가상 포인터를 사용한다. 로그/검증 복사본은 Git에서 제외한다. 검수 사진의 시설/아이템/시간 fixture는 Play 종료 때 버린다.

## 기반 안정화 책임

- AttackProfile/CombatAffiliation: 공격 형상/대상 수/관계 필터. PlayerCombat은 선택·타격·모션, 입력 장치 API는 모른다.
- PlayerInputCommands/PlayerInputAdapter/IPlayerInputSource: Move/Primary/Interact/Inventory/Pause와 장치/자동 입력 분리.
- WaterContainerDefinition/LiquidState/ConsumableDefinition: 컵 상태 관계/소비 효과/예측 가능한 불이익. 기존 InventoryModel exchange와 StationProcessing을 공유한다.
- StackSplitPanel/InventoryDragController: 액션/수량 UI/가상 부분 Stack. 모델 변경은 목적지 배치가 성공할 때만 atomic하게 반영한다.
- WorldSaveService/SavedGame/SaveRepository/SaveValidation: Snapshot 구성, Version/검증, 원자 파일 교체와 backup, Seed+Delta 적용. ContentCatalog는 Item/Recipe 정본 자산의 참조 목록이다.
- ResourceIdentity/ResourceRegeneration: 안정 ID/변경 기록/게임 시간 재생. AnimalIdentity/AnimalPopulation: 지역 Slot/자연 재생/Timer 저장.
- WorldNavigation/EnemySteering: 생성 지형/공통 장애물/Spawn 적합성/부드러운 분리/국소 우회.
- PrefabPool/IPoolReset: 반복되는 적/WorldPickup의 생명주기·HP·피격·AI·수량 초기화.
- Editor DeveloperHarness/Profile/Window/ProjectDataValidator: 제품 Runtime/HUD와 분리한 도구와 데이터 검사.

Save/Load는 영구 저장 v2(v1 호환)이며 Cloud/계정/실제 네트워크 권위를 구현하지 않는다. 미래 Unlock/무기 공격 방식/Vehicle/Animal Breeding은 데이터 확장 방향과 실제 제공 기능을 구분한다.

## 캐릭터 패스 책임

- CharacterGrowthDefinition/PlayerProgression: XP Curve/Level/Point/Rank/이벤트. Experience는 누적 획득,CurrentXP는 현재Level의 남은 XP다.
- CharacterStats: Skill로 계산되는 HP/Stamina 최대·회복·효율 Modifier의 집계 지점. 향후 Status Modifier를 이 경계에서 합성할 방향이며 빈 범용 Framework는 만들지 않는다.
- PlayerEquipment: Head/Body 장착,교체/해제 Inventory 교환,Defense/표현 이벤트. Health는 최소 피해/생존 우회를 유지한다.
- CharacterVisualLayers: Base Skin/Head/Body/Hand/단일Back Slot/Optional FX. 얼굴/손Facing과 기본몸통을 유지하며 외곽 장비만 방향을 따른다. 장비가 Skin Sprite를 덮어쓰지 않는다. Backpack/Cape/Premium/Transmog는 없다.
- LocomotionDefinition/StaminaModel/PlayerLocomotion: 설정/순수Stamina·Sprint/물리Dash. PlayerMotor가 이동을 연결하고 WorldNavigation은 자기Collider를 제외한 Sweep/지형을 검사한다.
- ProfileHud/CharacterScreen: HUD와세탭UI. Character 상태는입력차단/세계진행이며 Inventory/Pause와구분한다.
- RecipePresentation/CraftingPanel: 고정정렬/Category/Filter/잠금/NEW. PlayerCrafting은 비용 소비/시설 Upgrade를 담당한다.
- GrowthValidation/WorldSaveService: XP총합·Point Budget·Rank·Armor Slot·Stamina 검증,v1→v2변환.

Health의 피해/회복,SurvivalModel의Hunger/Thirst와Raw Water 불이익,CharacterStats+Locomotion의이동/회복,CombatAffiliation+PlayerCombat의관계/타격 경계를 유지한다. 향후Poison/Bleed/Burn/Slow/Weakness/Curse/Defense Down,Cold/Dehydration/Food Poisoning/Water Sickness,Regeneration/Movement/Gathering Buff를 해당Modifier 경계로 확장한다. 확률 질병/주기피해/상태아이콘 콘텐츠는 없으며 Raw Water 동작은 유지했다.
