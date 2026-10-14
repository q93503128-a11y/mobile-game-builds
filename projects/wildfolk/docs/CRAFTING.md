# 제작과 처리

현재25 Recipe, 실제 Workbench Tier1/2, Equipment Tier Wood/Stone/Iron이다. 정본은 RecipeDefinition/RecipeCatalog/ContentCatalog이며 이름별 Runtime 분기는 없다.

| 시설 | 결과 | 비용/처리 |
|---|---|---|
| Hand | Workbench | Wood6 |
| Hand | Campfire | Wood4+Stone3 |
| Hand | Wooden Cup | Wood2 |
| Workbench | Wood Axe/Pickaxe/Sword | 각각 Wood5 |
| Workbench | Stone Axe/Pickaxe | 이전 Wood 장비1+Stone4 |
| Workbench | Stone Sword | Wood Sword1+Stone5 |
| Workbench | Iron Axe/Pickaxe/Sword | 이전 Stone 장비1+Ingot3 |
| Workbench | Furnace | Stone8+Wood4 |
| Workbench | Wall/Door/Chest | Wood2/3/6 |
| Campfire | Cooked Meat1 | Raw Meat1 /5초 |
| Campfire | Clean Water Cup1 | Raw Water Cup1 /5초 |
| Furnace | Ingot1~3 | Ore1~3 /6초 |

기존 StationProcessing을 고기/물/제련에 공유한다. ParallelCapacity와 ProcessingSpeed를 가진다. Campfire1/Speed1, Furnace3/Speed1이다. 한 Batch를 예약하고 끝나면 Take한다. 다른 Recipe를 동시에 처리하거나 Queue/연료/자동 수령하지 않는다. 수령 공간 부족은 결과를 유지하고 시설 파괴 시 완료 결과 또는 예약 재료를 월드에 돌려놓는다. Recipe/Batch/남은 시간을 Save에 포함한다.

물은 용기 포함 상태를 교환한다. 빈 컵→Raw Cup→Clean Cup→빈 컵이다. 빈 컵20 Stack, 채운 컵1 Stack으로 각 상태를 구분한다. 채우기/마시기는 공간을 확인한 atomic inventory exchange이며 물이나 용기를 유실하지 않는다.

장비 Upgrade는 이전 장비를 명시적인 Recipe 재료로 소비한다. 상위 장비 Drop은 하위 장비를 자동 제거하지 않는다. 내구도는 없다.

General Workbench가 향후 Tier Progression의 중심이다. RequiredWorkbenchTier는 현재 사용하며 FutureUnlocks는 PlayerLevel/Skill/DiscoveredItem/Blueprint/SpecialContent를 표현할 확장 데이터다. FutureUnlocks는 비어 있으며 Recipe의 추가 Level/Skill/Blueprint 조건은 활성화하지 않는다. Character Level/Skill은 별도 제공한다. Workbench II는 실제이며 III/Diamond/전문 제작대/Defense는 미래 방향이다.

## Armor와 Workbench II

| 작업 | 비용 | 조건 |
|---|---|---|
| Leather Cap /Guard |Leather1+Wood2 /Leather2+Wood3|Workbench I 또는II|
| Iron Helmet /Guard |Ingot2+Leather1 /Ingot3+Leather1|Workbench I 또는II|
| 설치 Workbench I→II |Wood8+Stone6+Ingot2|가까이Interact→Upgrade II|
| Reinforced Chest32 |Wood8+Ingot2|Workbench II|
| Stone Wall HP220 |Stone3+Wood1|Workbench II|

업그레이드는 기존 설치물을 교체하며 위치/회전/Save ID/HP 비율을 유지한다. 별도 Workbench II 제작 Recipe는 없고 II 아이템 참조는 설치/저장/Editor Profile용이다. I의 모든 Recipe를 유지한다. Armor Bench/빈 전문 시설은 없다.

Category는Survival/Tools/Weapons/Armor/Building/Facilities이고 현재 Station에 실제 Recipe가 있는 항목만 메뉴에 나타난다. Show all/Craftable only와 조합한다. Tier(Rank)→고정SortOrder→ID 순이며 NEW/재료 보유량은 정렬에 영향을 주지 않는다. II 잠금 Recipe는 I에서도 위치를 유지하고 조건을 표시한다. 해금 후 NEW는 카드 확인/제작 때 해제되며 SeenRecipe IDs를 저장한다. 잠금 상태에서 확인해도 미래 NEW를 소모하지 않는다.

Leather의 미래 사용 후보는Backpack/Strap/Quiver/Tent/Bedding 등이며 이번에는 Armor 이외 생활 장비를 만들지 않았다.
