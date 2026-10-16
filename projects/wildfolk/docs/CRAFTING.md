# 제작과 처리

현재31 Recipe, 실제 플레이 Workbench Tier1/2(구조적 cap VI), Equipment Tier Wood/Stone/Iron이다. 정본은 RecipeDefinition/RecipeCatalog/ContentCatalog이며 이름별 Runtime 분기는 없다.

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
| Furnace | Ingot1 | Ore1 /2초, 순차 처리 |

StationProcessing은 시설 데이터의 Batch/Sequential Mode와 Speed를 읽는다. Campfire의 단일 예약 Batch/5초/Take는 유지한다. Furnace는 Recipe별 한 단위를 처리하고 다음 대기 단위를 자동 시작한다. +1/Add all/대기 −1/결과 Take를 제공한다. 투입 단위/대기/현재 Ore→Result/완료 수량/남은 시간과 Progress를 표시한다. 현재 Input50/Output100 단위 보관 한계는 초기값이다. 대기 재료 회수와 결과 수령은 atomic inventory transaction이며 공간 부족이면 예약/완료 결과를 유지한다. Output 보관 한계에서는 완료된 현재 단위를 보존하고 결과 수령 후 재개한다. 시설 파괴 시 완료 결과와 현재·대기 재료를 Stack 한계에 맞춰 바닥으로 반환한다. Copper/Tin 제련과 Steel 가공은 Recipe에 Coal을 함께 예약 소비한다. 별도 연료 탱크/자동 Inventory 수령은 없다. 기존 Iron Recipe는 무연료2초를 유지한다.

ProcessingQueue는 Recipe 참조/예약 순서/단위 시간/ResultCount/Output을 사용한다. Iron Item 이름 분기가 없다. 혼합 Recipe 순서도 저장·복원하며 향후 Ore/Metal/Alloy/Crystal과 시설별 처리기를 같은 경계에서 확장한다. 현재 Iron/Copper/Tin/Coal과 Copper/Tin Ingot/Bronze/Steel을 제공하며 실제 Recipe는 METAL_PROGRESSION을 따른다.

물은 용기 포함 상태를 교환한다. 빈 컵→Raw Cup→Clean Cup→빈 컵이다. 빈 컵20 Stack, 채운 컵1 Stack으로 각 상태를 구분한다. 채우기/마시기는 공간을 확인한 atomic inventory exchange이며 물이나 용기를 유실하지 않는다.

장비 Upgrade는 이전 장비를 명시적인 Recipe 재료로 소비한다. 상위 장비 Drop은 하위 장비를 자동 제거하지 않는다. 내구도는 없다.

General Workbench가 향후 Tier Progression의 중심이다. RequiredWorkbenchTier는 현재 사용하며 FutureUnlocks는 PlayerLevel/Skill/DiscoveredItem/Blueprint/SpecialContent를 표현할 확장 데이터다. FutureUnlocks는 비어 있으며 Recipe의 추가 Level/Skill/Blueprint 조건은 활성화하지 않는다. Character Level/Skill은 별도 제공한다. Workbench I~VI를 검증하는 정식 구조가 있으며 실제 구성은 I→II뿐이다. III~VI 비용/Recipe/추가 전문 제작대/Defense는 미정이다. 미구성 업그레이드는 실행하거나 UI에 노출하지 않는다.

## Armor와 Workbench II

| 작업 | 비용 | 조건 |
|---|---|---|
| Leather Cap /Guard |Leather1+Wood2 /Leather2+Wood3|Workbench I 또는II|
| Iron Helmet /Guard |Ingot2+Leather1 /Ingot3+Leather1|Workbench I 또는II|
| 설치 Workbench I→II |Wood8+Stone6+Ingot2|가까이Interact→Upgrade II|
| Reinforced Chest32 |Wood8+Ingot2|Workbench II|
| Stone Wall HP220 |Stone3+Wood1|Workbench II|

업그레이드는 기존 설치물을 교체하며 위치/회전/Save ID/HP 비율을 유지한다. 별도 Workbench II 제작 Recipe는 없고 II 아이템 참조는 설치/저장/Editor Profile용이다. I의 모든 Recipe를 유지한다. Armor Bench/빈 전문 시설은 없다.

Category는Survival/Tools/Weapons/Armor/Building/Facilities이고 현재 Station에 실제 Recipe가 있는 항목만 즉시 선택 Tab에 나타난다. Show all/Craftable only와 조합한다. All은 Category Section을 우선하고 각 Section에서 Tier(Rank)→고정SortOrder→ID 순이며 NEW/재료 보유량은 정렬에 영향을 주지 않는다. II 잠금 Recipe는 I에서도 위치를 유지하고 조건을 표시한다. 해금 후 NEW는 카드 확인/제작 때 해제되며 SeenRecipe IDs를 저장한다. 잠금 상태에서 확인해도 미래 NEW를 소모하지 않는다.

Leather의 미래 사용 후보는Backpack/Strap/Quiver/Tent/Bedding 등이며 이번에는 Armor 이외 생활 장비를 만들지 않았다.


제작은 별도 Recipe 중심 화면이다. 36칸 Inventory를 함께 펼치지 않는다. Category별 두 열 목록과 선택 결과의 보유량/실제 공격력·방어력·시설 HP/보관 용량을 표시한다. Inventory 버튼으로 Item 관리에 돌아간다. 재료 각각의 보유/필요 수량과 충분한 녹색·부족한 적색을 독립 표시하며 NEW는 순서를 바꾸지 않는다. English/Korean은 Localization Key를 사용한다.


## Multiplayer Foundation 4

Host가 기존 PlayerCrafting.GetStatus/Craft와 InventoryModel.TryExchange를 사용해 Hand/Workbench의 Recipe·Station·Tier·재료·공간을 검증한다. Workbench I→II는 기존 비용을 한 번 소비하고 같은 NetworkObject의 Definition/Visual/HP 비율을 전환해 열린 참조를 유지한다. 싱글의 기존 교체 방식은 유지한다.

Furnace sequential queue와 Campfire Meat/Raw→Clean Cup Batch는 기존 StationProcessing이 Host에서만 진행한다. 투입/회수/Take와 완료·출력 포화 경계가 facility revision을 올리며, 오래된 요청/full bag은 cargo를 유지한다. Client는 ordered cargo snapshot과 current/end time만 표시한다. 제작 UI Category/Filter/Sort/NEW를 유지하고 진행 tick은 목록 전체를 다시 만들지 않는다.

세션 시작 근처에 기존 시설5개를 Host 초기 배치한다. Player 설치/철거·Wall/Door·멀티 save/reconnect와 실제2인 Relay QA는 이 구현에 포함하지 않았다. 자동검사/한계는 MULTIPLAYER_FOUNDATION을 따른다.

## 초기 금속 진행

현재 신규 Recipe6종과 시설 지원/원자 소비/업그레이드 UI는 [METAL_PROGRESSION](METAL_PROGRESSION.md)이 정본이다. General Workbench는 int Tier1~6·UpgradeTo·임의 길이 UpgradeCosts·선택 TierVisual/TierDisplayKey를 사용한다. Recipe.RequiredWorkbenchTier는 일반 제작과 전문 시설의 선행 조건이다. Furnace Steel은 살아 있는 II 이상 Workbench가 시설의 WorkbenchSupportRange(현재12유닛) 안에 있어야 신규 예약 가능하다. 지원 제작대를 잃어도 이미 예약된 cargo는 완료·회수·저장을 유지한다. 시설 registry와 변경 이벤트를 사용하고 전수 매-frame 검색을 추가하지 않았다.

현재 Tier를 Roman 표기로 표시하고 구성된 다음 단계만 연다. 비용은 재료별 보유/필요·녹색/적색으로 표시하며 별도 ScrollRect가 임의 개수의 행을 수용한다. 실제 I→II는 기존3종 비용이다. 검수 이미지의5종은 레이아웃 fixture이며 제품 비용이 아니다. 신규 장비 세트/새 시설은 만들지 않았다.
