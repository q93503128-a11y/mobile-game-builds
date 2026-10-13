# 제작·시설·조리

RecipeDefinition/Catalog가 재료·결과·StationKind·ProcessingSeconds/ProcessingKind의 정본이다. UI는 시설에 해당하는 레시피만 표시한다. 기본 가방은 Hand, 가까운 시설 Use는 Workbench/Campfire/Furnace다.

| 레시피 | 시설 | 비용 | 결과·시간 |
|---|---|---|---|
| workbench | None | Wood6 | Workbench1 즉시 |
| campfire | None | Wood4 + Stone3 | Campfire1 즉시 |
| axe | Workbench | Wood5 | Wood Axe1 즉시 |
| pickaxe | Workbench | Wood5 | Wood Pickaxe1 즉시 |
| wood_sword | Workbench | Wood5 | Wood Sword1 즉시 |
| cooked_meat | Campfire | Raw Meat1 | Cooked Meat1, 5초 |

PlayerCrafting은 공통 CraftingStation.CanInteract(살아 있음/중심 2.6유닛)을 검사한다. Action 대상은 앞 방향 dot≥0.35도 확인한다. 멀어지거나 시설이 제거되면 Hand로 돌아간다. None 레시피의 데이터상 자격은 시설 불필요지만 작업대 UI는 현재 도구·검·시설·건축을 보여준다.

즉시 제작은 InventoryModel.TryExchange로 비용/결과를 한 번에 확정한다. 부족한 재료/공간/시설이면 변경하지 않는다. Timed recipe는 StationProcessing에 한 작업만 보관한다. 시작 시 TryConsumeCosts가 재료를 먼저 원자적으로 소비하고, scaled time으로 진행한 뒤 Take로 결과를 가져온다. 인벤토리를 닫거나 멀어져도 진행/결과는 시설에 남으며 다시 접근해 수령한다. 공간 부족이면 Inventory full로 알려주고 결과를 보존한다. 시설 파괴 시 조리 중 원재료 또는 완성 결과를 공통 WorldPickup으로 드랍한다.

입력/결과 아이콘·이름·재료·Cook/Take·남은 초·진행 게이지를 기존 RecipeCardView에 표시한다. 손/작업대/모닥불의 별도 중복 인벤토리나 건축 시스템은 없다.

ItemDefinition.Placement→PlaceableDefinition→WorldPlaceable prefab과 기존 PlayerPlacement를 재사용한다. Preview 2.1/최대2.8유닛, Collider/Footprint/필드 경계 검사 후 선택 슬롯1개만 소비한다. Workbench 1.8×1.15, Campfire1.5×1.15 Footprint, 각 HP60/스택10이다. 설치/수집/섭취/시설 Use는 단일 입력이고 공격만 반복한다.

Raw Meat는 한 중립 멧돼지에서 2개 나온다. Zombie 드랍/개발자 지급 버튼은 없다. 물은 재사용 수원에서 한 회분씩 얻는다. 소비품이 선택되어 있으면 섭취가 시설/물 상호작용보다 우선이므로 빈 슬롯/도구를 선택해 시설을 연다.

고급 작업대/포탑/복잡한 조리 Queue/용기/정수/설치물 회수·수리/무한 식량 공급은 없다.

## Stone / Iron / 기지 Recipe

| 결과1 | 시설 | 비용 | 시간 |
|---|---|---|---:|
| Stone Axe | Workbench | Wood3 + Stone4 | 즉시 |
| Stone Pickaxe | Workbench | Wood3 + Stone4 | 즉시 |
| Stone Sword | Workbench | Wood2 + Stone5 | 즉시 |
| Furnace | Workbench | Stone8 + Wood4 | 즉시 |
| Wood Wall | Workbench | Wood2 | 즉시 |
| Wood Door | Workbench | Wood3 | 즉시 |
| Storage Chest | Workbench | Wood6 | 즉시 |
| Iron Ingot | Furnace | Iron Ore1 | 6초 |
| Iron Axe / Pickaxe / Sword (각각) | Workbench | Wood3 + Iron Ingot3 | 즉시 |

전체 실제 Recipe17개다. Workbench의 많은 목록은 ScrollRect/Mask/동적 높이로 표시하며 재료 변화 시 스크롤을 강제로 되돌리지 않는다. 시설 종류를 바꿀 때만 맨 위로 돌아간다. Cooking와 Smelting은 공통 시간 처리만 공유하고 Station/Processing 데이터로 분리한다. 연료는 없고 이후 재료 비용에 Wood/Coal 등을 추가할 수 있으나 별도 연료 UI/자동화는 없다.

벽/문은 기존 PlayerPlacement에서 Facing 방향에 맞춘 0/90도 회전과 회전 Footprint 검사로 배치한다. 문 열림은 Visual90도 회전/Barrier 비활성, 닫힘은 점유 검사 후 복원이다. 상자/기본 시설은 회전하지 않는다. 붙이기 Grid/Snap은 없다.

긴 작업대 목록은 작은 스크롤바로 추가 레시피가 있음을 알린다. 목록 전체를 드래그할 수 있고 짧은 Hand/음식/제련 목록에서는 스크롤바를 숨긴다.
