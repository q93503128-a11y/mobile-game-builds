# 현재 코드 연결

Runtime(Input System/UGUI), Editor 구성 도구, Test 어셈블리를 분리한다. 런타임에 씬 전체를 재생성하지 않는다.

| 책임 | 연결 |
|---|---|
| 이동/방향 | VirtualJoystick → PlayerMotor, 공통 CharacterFacing과 PlayerAvatar |
| 선택/장착 | InventoryModel 36칸 + SelectedIndex → PlayerEquipment |
| Action | PlayerAction이 선택 Placeable/가까운 시설/일반 공격 중 하나를 선택 |
| 공격 | 기존 PlayerCombat/ToolDefinition 공유, 검도 같은 사용 프로필 |
| 설치 | PlayerPlacement 검사/Preview → WorldPlaceable prefab → 선택 슬롯 소비 |
| 제작 | RecipeCatalog/RecipeDefinition → PlayerCrafting 시설 자격 → InventoryModel.TryExchange |
| 시설 | CraftingStation.CanInteract로 Health/중심 거리 공유, 멀어짐/제거 시 자격 해제 |
| UI | InventoryPanel/HotbarView/RecipeCardView 이벤트 갱신, SlotDrag/DragController의 Pointer 흐름 |
| 상태 | GameplaySession UI/입력 상태, WorldPauseController 싱글플레이 동결 정책 |
| 기존 월드 | Health/WorldHealthBar/HitFeedback, ResourceNode/WorldPickup, EnemyController, CameraFollow, Boss HUD |

PlayerAction이 사용 버튼의 공격/배치/시설 상호작용과 아이콘을 중앙에서 결정한다. Item ID별 if/else를 여러 런타임에 흩뿌리지 않는다. 입력 HoldAttackInput은 공격만 반복하며 배치/상호작용은 단일 사용이다. PlayerEquipment는 슬롯 변경 즉시 도구/설치물 상태를 갱신하고 진행 중 사용을 취소한다.

InventoryModel의 MoveStack은 이동/합치기/교환 한 곳이며 TryExchange는 임시 배열에서 검증한 뒤 한 번 확정한다. 비용 소비/결과 추가 중간 상태를 UI에 노출하지 않는다. 모든 UI는 하나의 인벤토리를 읽으며 Ghost가 별도 아이템 정본이 되지 않는다.

GameplaySession에는 Time.timeScale 쓰기가 없다. 적은 플레이어 입력 상태가 아닌 WorldIsRunning을 사용한다. 플레이어의 인벤토리가 열려도 세계가 계속 진행한다. 실제 동결은 별도 WorldPauseController의 현재 로컬 정책이다. 실제 네트워크 동기화는 없다.

StartingLoadout과 StartingEquipment 에셋/참조를 제거했다. 실제 수치/아이템/레시피/설치물은 Data의 ScriptableObject가 단일 정본이고 생성 도구는 기존 데이터를 보존한다. Workbench는 공통 Health와 별도의 WorldPlaceable/CraftingStation 책임을 사용한다.

Editor GameplayUiFactory가 슬롯 표시를 공유하고 InventoryUiBuilder가 36칸/제작/Ghost를 구성한다. 씬 참조를 프리팹 인스턴스 변경으로 기록한 뒤 저장한다. 해상도 도구는 이미 열린 Game View만 갱신하고 Focus/GetWindow 호출이나 씬 생성 후 자동 전환을 하지 않는다.

이벤트는 종료 시 해제하고 Ghost는 UI 상태 변경에 취소한다. 테스트는 공유 PlayPointerDriver로 Unity 내부 장치 이벤트를 전달하며 사용자 Windows 입력을 조작하지 않는다. 검증 복사본/로그는 Git에서 제외한다. 저장/멀티플레이/고급 시설/회수·수리/풀링 최적화 완료를 주장하지 않는다.
