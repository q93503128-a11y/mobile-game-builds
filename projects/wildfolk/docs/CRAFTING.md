# 제작·업그레이드·병렬 처리

RecipeDefinition/Catalog가 실제17개 Recipe의 단일 정본이다. StationKind None/Workbench/Campfire/Furnace와 ProcessingKind None/Cooking/Smelting을 구분한다. 큰 Primary는 설치/공격/섭취, 작은 Interact는 시설 열기다. 시설 중심2.6유닛/살아 있음/앞 방향dot≥.35를 확인하며 소비품 선택 여부와 무관하게 Interact할 수 있다.

| 결과1 | 시설 | 비용 | 처리 |
|---|---|---|---|
| Workbench | Hand | Wood6 | 즉시 |
| Campfire | Hand | Wood4+Stone3 | 즉시 |
| Wood Axe / Pickaxe / Sword(각각) | Workbench | Wood5 | 즉시 |
| Stone Axe / Pickaxe(각각) | Workbench | 해당 Wood 장비1+Stone4 | 즉시 |
| Stone Sword | Workbench | Wood Sword1+Stone5 | 즉시 |
| Iron Axe / Pickaxe / Sword(각각) | Workbench | 해당 Stone 장비1+Ingot3 | 즉시 |
| Furnace | Workbench | Stone8+Wood4 | 즉시 |
| Wood Wall | Workbench | Wood2 | 즉시 |
| Wood Door | Workbench | Wood3 | 즉시 |
| Storage Chest | Workbench | Wood6 | 즉시 |
| Cooked Meat | Campfire | Raw1 | 5초, 1개 |
| Iron Ingot | Furnace | Ore 1/개 | 6초, 최대3개 |

Wood→Stone→Iron은 이전 장비를 소비하는 명시적인Upgrade Recipe다. 자동 진화/내구도/강제로 하위 장비 삭제가 없다. 실패하면 재료/장비를 보존한다. 원래 장비가 선택된 슬롯에서 소비되면 즉시 장착이 갱신된다.

StationProcessing은 한 Batch를 보관하며 시작 시 가능한 최대Batch를 재료 보유량과PlaceableDefinition.ParallelCapacity로 결정한다. Furnace Tier1 Capacity3/Speed1: Ore3→6초→Ingot3,Ore2→6초→Ingot2. Campfire Capacity1/Speed1이다. 재료는 시작 때 예약 소비하고 처리 시간은Recipe.ProcessingSeconds/Station.ProcessingSpeed다. 결과Take/공간 부족 보존/시설 파괴 시 처리 중 원재료 또는 완성 결과 드랍을 공유한다. 창을 닫아도 진행한다. 여러Recipe Queue/연료/자동 재투입은 없다.

Furnace UI는 보유/투입량·Batch n/capacity·결과×n·남은 초/Progress·Smelt/Take를 표시한다. 작업대 목록은ScrollRect/Mask/작은Scrollbar를 사용하고 같은 시설에서 재료가 바뀌어도 스크롤을 유지한다.

시설은 기존 Inventory→Hotbar→Preview→Primary 배치다. 앞2.1/최대2.8유닛, Footprint/Collider/월드 경계 검사, 실패 소비 없음. 벽/문은0/90도 배치와 점유된 닫기 거절을 사용한다. Grid/Snap/회수/수리는 없다.

General Workbench가 향후 제작Progression의 중심이다. 실제WorkbenchTier1/RequiredWorkbenchTier1을 사용한다. 이후 범용작업대 II/III가 일반 장비와 전문 시설 Recipe를 해금할 방향이다. 현재Wood/Stone/Iron만 실제이며 Diamond/WeaponBench/ArmorBench/ToolBench/Engineering/Defense는 향후 방향이다. 후반 장비는 특수 능력·공격/채집/방어 효과와 특수 재료를 고려하되 현재 구현하지 않았다.
