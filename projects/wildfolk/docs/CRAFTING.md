# 제작과 처리

현재19 Recipe, 실제 Workbench Tier1, Equipment Tier Wood/Stone/Iron이다. 정본은 RecipeDefinition/RecipeCatalog/ContentCatalog이며 이름별 Runtime 분기는 없다.

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

General Workbench가 향후 Tier Progression의 중심이다. RequiredWorkbenchTier는 현재 사용하며 FutureUnlocks는 PlayerLevel/Skill/DiscoveredItem/Blueprint/SpecialContent를 표현할 확장 데이터다. 현재 추가 Unlock 조건은 모두 비어 있으며 실제 Level/Skill/Blueprint 기능은 없다. Workbench II/III, Diamond, 전문 제작대/Defense 제작 계층은 기획 방향만 기록한다.
