# 제작과 작업대

RecipeDefinition/RecipeCatalog가 ID, 결과 아이템/수량, ItemAmount 재료 목록, StationKind 요구를 보유한다. 제작 가능 여부는 현재 인벤토리/시설로 계산하며 UI에 레시피를 하드코딩하지 않는다. 실제 시설 종류는 None/Workbench만 있다.

| 레시피 ID | 시설 | 비용 | 결과 |
|---|---|---|---|
| workbench | None (Hand crafting) | Wood 6 | Workbench 1 |
| axe | Workbench | Wood 5 | Wood Axe 1 |
| pickaxe | Workbench | Wood 5 | Wood Pickaxe 1 |
| wood_sword | Workbench | Wood 5 | Wood Sword 1 |

Axe/Pickaxe 기존 아이템 ID/정의/효율 프로필을 재사용했다. Wood Sword만 새 무기이며 Workbench는 Placeable이다. None 레시피는 작업대에서도 사용할 수 있다. 기본 가방에서는 Workbench 레시피만 표시한다.

## 제작 안전성

PlayerCrafting이 시설 자격을 확인하고 InventoryModel.TryExchange가 재료 소비/결과 추가를 원자적으로 처리한다. 부족한 재료/공간/시설이면 실패하며 재료가 사라지지 않는다. 결과가 Storage에 들어가도 즉시 표시하고 Drag로 Hotbar에 옮길 수 있다. 공간 부족이면 Craft를 비활성화하고 Inventory full을 표시한다. 부족한 재료는 보유량/필요량과 색으로 알린다.

## 설치

ItemDefinition.Placement → PlaceableDefinition → Workbench.prefab을 사용한다. PlayerPlacement가 마지막 Facing 앞쪽 Preview를 표시하고 Collider/Footprint/월드 경계/최대 거리를 검사한다. PlayerAction이 공격 대신 배치를 선택하며 단일 사용으로 1개를 설치한다. 계속 누른다고 자동으로 여러 작업대를 설치하지 않는다.

현재 값: Preview 거리 2.1, 최대 2.8유닛, Footprint 1.8×1.15유닛, 최대 스택 10, 설치물 HP 60. 설치 후 아이템이 남아 있으면 계속 배치 선택 상태고 마지막 1개였다면 선택 슬롯이 비어 맨손으로 돌아간다.

## 시설 접근

시설 중심 2.6유닛 이내, 살아 있는 작업대를 바라보면 사용 버튼이 상호작용 아이콘으로 바뀐다. 바라보기는 현재 Facing과 대상 방향 dot ≥ 0.35다. CraftingStation.CanInteract의 중심 거리/Health 판정을 표시와 제작 권한이 공유한다. Collider 표면이 원형 검색 범위에 걸쳤다는 이유만으로 상호작용을 허용하지 않는다.

멀어지거나 시설이 제거되면 Hand crafting으로 전환하고 작업대 레시피를 사용할 수 없다. Inventory UI가 떠 있어도 월드가 계속 진행한다. Workbench는 WorldPlaceable/공통 Health/Collider/그림자/피격 반응을 가진다. 회수·수리·철거 UX와 Hammer/Wrench는 구현하지 않았다.

## 범위

새 게임의 StartingLoadout을 제거했다. 기본 진행은 맨손 채집 → Wood 6 → Workbench → Wood 각 5 → 도구/검이다. 시설/재료 타입을 데이터로 구분하며 미래 시설/요리/생존 수치/금속 티어를 이번에 생성하지 않았다.
