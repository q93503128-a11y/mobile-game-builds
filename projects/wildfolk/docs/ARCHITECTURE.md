# 현재 코드 연결

Runtime/InputSystem/UGUI,Editor,EditMode/PlayMode Tests를 분리한다. 실제 수치는Data ScriptableObject가 정본이고 Editor는 없는 초기 데이터를 생성하며 기존 수치를 보존한다. 씬은 프리팹 연결과 월드Bootstrap을 저장하고 수동 테스트 월드 배치 코드는 제거했다.

| 책임 | 연결 |
|---|---|
| 월드 데이터 | BiomeDefinition/WorldGenerationDefinition/GenerationRule→WorldMap(seed) |
| 월드 연결 | WorldGenerator→WorldTerrainRenderer/물Collider/자원Prefab/수원/중립동물 |
| 정렬 | WorldSorting,BaseY DepthSort/SortingGroup |
| 이동/방향 | VirtualJoystick→PlayerMotor/Survival MovementCondition,CharacterFacing |
| Primary | HoldAttackInput→PlayerPrimaryAction→Combat/Placement/Consume |
| Interact | InteractButtonView→PlayerInteraction→IPlayerInteractable |
| Inventory | InventoryModel36+선택→Hotbar/Inventory/Equipment |
| 드래그/소분 | SlotDrag/DragController→공통Transfer/SplitHalf |
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

WorldMap은 System.Random(seed)와 공통 물 경계를 사용하며 UnityRandom을 건드리지 않는다. 지형은 한 Texture/Sprite, 물은 몇 PolygonCollider, 자원223개와 최대480장식을 제한 생성한다. Camera와 Placement는 생성 Ground Bounds를 공유한다. 무한 스트리밍, 복잡한 NavMesh, 풀링 최적화 완료를 주장하지 않는다.

Zombie/Goblin/Animal의 행동 책임은 분리했다. 자연 Despawn은 Health.Died를 호출하지 않아 Loot/XP/Gold를 지급하지 않는다. 직접 피해에 PlayerInventory를 기록하고 처치 때만 재화를 지급한다. 일반 피해 방어와 허기 피해의 방어 우회를 유지했다.

StationProcessing은 ParallelCapacity/Speed로 Batch 재료를 원자적으로 소비하고 결과를 보존한다. 한 Batch만 처리하며 Cooking/Smelting의 Station과 Recipe를 구분한다. UI 텍스트는 수량/초 변화 시 갱신하고 매 프레임 임시 거래 배열을 만들지 않는다.

Inventory는 한 배열, Chest는 상자별 한 배열이다. Transfer는 두 모델을 변경한 뒤 알린다. SplitHalf는 같은 모델의 빈 슬롯으로 부분 이동하고, Drop은 안전한 위치에 생성이 성공한 뒤 소비한다. Drag Ghost는 표시일 뿐 데이터 정본이 아니다. 상자 소분 선택도 테두리로 표시하고 이벤트는 종료 시 해제한다. UI 상태/Chest 변경 시 선택과 드래그를 취소한다.

세계 진행은 플레이어 입력 Running과 분리한다. Inventory는 세계를 진행하고, Singleplayer Pause/Defeat는 동결하며, Multiplayer 정책은 전역 동결을 하지 않는다. 실제 네트워크는 없다. 월드 Seed와 재화는 현재 세션이며 사용자 설정 PlayerPrefs와 구분한다.

해상도 도구는 기존 Game View만 Repaint하고 Play Unfocused를 적용한다. Focus/GetWindow/Windows 입력 호출은 하지 않는다. 자동 테스트는 별도 hidden headless Unity와 내부 가상 포인터를 사용한다. 로그/검증 복사본은 Git에서 제외한다. 검수 사진의 시설/아이템/시간 fixture는 Play 종료 때 버린다.
