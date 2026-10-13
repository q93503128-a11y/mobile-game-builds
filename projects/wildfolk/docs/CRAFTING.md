# 제작·시설·조리

RecipeDefinition/Catalog가 재료·결과·StationKind·CookingSeconds의 정본이다. UI는 시설에 해당하는 레시피만 표시한다. 기본 가방은 Hand, 가까운 시설 Use는 Workbench 또는 Campfire다.

| 레시피 | 시설 | 비용 | 결과·시간 |
|---|---|---|---|
| workbench | None | Wood6 | Workbench1 즉시 |
| campfire | None | Wood4 + Stone3 | Campfire1 즉시 |
| axe | Workbench | Wood5 | Wood Axe1 즉시 |
| pickaxe | Workbench | Wood5 | Wood Pickaxe1 즉시 |
| wood_sword | Workbench | Wood5 | Wood Sword1 즉시 |
| cooked_meat | Campfire | Raw Meat1 | Cooked Meat1, 5초 |

PlayerCrafting은 공통 CraftingStation.CanInteract(살아 있음/중심 2.6유닛)을 검사한다. Action 대상은 앞 방향 dot≥0.35도 확인한다. 멀어지거나 시설이 제거되면 Hand로 돌아간다. None 레시피의 데이터상 자격은 시설 불필요지만 작업대 UI는 도구/검만 보여준다.

즉시 제작은 InventoryModel.TryExchange로 비용/결과를 한 번에 확정한다. 부족한 재료/공간/시설이면 변경하지 않는다. Timed recipe는 StationCooking에 한 작업만 보관한다. 시작 시 TryConsumeCosts가 재료를 먼저 원자적으로 소비하고, scaled time으로 진행한 뒤 Take로 결과를 가져온다. 인벤토리를 닫거나 멀어져도 진행/결과는 시설에 남으며 다시 접근해 수령한다. 공간 부족이면 Inventory full로 알려주고 결과를 보존한다. 시설 파괴 시 조리 중 원재료 또는 완성 결과를 공통 WorldPickup으로 드랍한다.

입력/결과 아이콘·이름·재료·Cook/Take·남은 초·진행 게이지를 기존 RecipeCardView에 표시한다. 손/작업대/모닥불의 별도 중복 인벤토리나 건축 시스템은 없다.

ItemDefinition.Placement→PlaceableDefinition→WorldPlaceable prefab과 기존 PlayerPlacement를 재사용한다. Preview 2.1/최대2.8유닛, Collider/Footprint/필드 경계 검사 후 선택 슬롯1개만 소비한다. Workbench 1.8×1.15, Campfire1.5×1.15 Footprint, 각 HP60/스택10이다. 설치/수집/섭취/시설 Use는 단일 입력이고 공격만 반복한다.

Raw Meat는 한 중립 멧돼지에서 2개 나온다. Zombie 드랍/개발자 지급 버튼은 없다. 물은 재사용 수원에서 한 회분씩 얻는다. 소비품이 선택되어 있으면 섭취가 시설/물 상호작용보다 우선이므로 빈 슬롯/도구를 선택해 시설을 연다.

Furnace/고급 작업대/포탑/복잡한 조리 Queue/용기/정수/설치물 회수·수리/무한 식량 공급은 없다.
