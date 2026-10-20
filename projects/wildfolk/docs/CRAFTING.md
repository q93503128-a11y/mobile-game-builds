# 제작과 처리 · 현재 정본

2026-10-20. Item123 / Recipe100, Workbench I→II→III, Equipment Wood/Stone/Iron/Steel/Diamond. 전체 실제 비용·출력·Station·시간은 [CONTENT_EXPANSION](CONTENT_EXPANSION.md)의100개 표를 따른다. RecipeDefinition/RecipeCatalog/ContentCatalog가 코드 정본이다.

## Station 경계

Hand5 / Workbench I35·II전용23·III전용11 / Campfire6 / Cooking Pot6 / Furnace4 / Blast Furnace1 / Turret Workbench8이다. Workbench 누적35/58/69다. 이번에는 새 전문 Station을 추가하지 않았다. 현장 연고는 Hand, 단순 휴대식은 Campfire, 복합 Broth는 Pot, 장비·탄약·함정은 I/II에 배치했다. III 신규 Recipe0이며 Diamond 진행을 유지한다. [조사·밀도·대안 비교](../validation/creature-loot-expansion/REVIEW.md).

Furnace는 Iron/Copper/Tin/Bronze, Blast는 Steel 전용이다. Iron은 무연료2초, Copper/Tin/Bronze/Steel의 재료·Coal·처리시간은 에셋을 따른다. 생물 재료를 금속로에 넣지 않는다. 새 광물 tier/가짜 가공 중간재가 없다.

## 처리와 교환

StationProcessing은 데이터의 Batch/Sequential Mode·Speed를 읽는다. Campfire는 단일 예약 Batch+수령, Pot/Furnace/Blast는 공용 순차 queue를 쓴다. 순차 시설은 +1/Add all/대기−1/결과 Take, 입력·대기·진행·출력 수량과 남은 시간을 보여준다. 재료를 예약 소비하고 완료 결과는 자동 Inventory 수령하지 않는다. 공간 부족이면 입력·결과를 보존한다. Output 한계에서는 완료 단위를 유지하고 수령 후 재개한다. 파괴 시 현재·대기 재료/완료 결과를 stack 한계로 반환한다. 혼합 예약 순서와 Charges/Quality도 저장한다.

물은 빈 컵→Raw Cup→Clean Cup→빈 컵 상태 교환이다. Waterskin은 같은 상태/Charges codec을 쓴다. 음식·의약품의 HP/Stamina 즉시 효과와 공용 소비 cooldown은 [FOOD_AND_FISHING](FOOD_AND_FISHING.md)을 따른다.

## 제작 UI와 업그레이드

가까운 시설 Interact로 전용 제작 화면을 연다. 36칸 가방은 별도 화면이다. Category(Survival/Tools/Weapons/Armor/Building/Facilities)는 해당 Station의 실제 Recipe가 있을 때만 보인다. 두 열 카드, Show all/Craftable only, Tier→SortOrder→ID 고정 정렬, NEW/SeenRecipe 저장을 유지한다. 각 재료의 보유/필요량을 독립 표시한다. 새 Armor/음식/탄약의 실제 효과를 설명 첫 부분에 보여주고 긴 업그레이드 비용·설명은 각각 scroll한다. English/Korean을 지원한다.

설치 I→II 비용 Wood8+Stone6+Iron2, II→III Wood10+Stone10+Bronze4+Steel4+Crystal4+Diamond1. 싱글은 설치물 교체, 멀티는 같은 NetworkObject의 Definition 전환으로 Save ID/위치/HP 비율/열린 화면을 유지한다. 별도 II/III Recipe가 없다. FutureUnlocks(Level/Skill/Blueprint 등)는 비어 있고 추가 gate는 활성화하지 않았다. IV~VI는 미구성으로 실행/UI 노출하지 않는다.

## Multiplayer

Host가 PlayerCrafting.GetStatus/Craft와 InventoryModel.TryExchange로 Station/tier/재료/공간을 확인한다. 투입·회수·수령·완료 경계에서 facility revision을 올린다. 오래된 요청/full bag은 cargo를 유지한다. Client는 변경 snapshot과 server 종료 시각으로 진행을 표시한다. Goblin Camp는 플레이어가 넣을 수 없는 별도6칸 보급품 UI와 Host claim 경계를 사용한다.

Medicine2-A: Hand6개 중 응급 붕대(Fiber2+Herb1)·연고(Gland1+Herb1+Fiber2)를 현장 제작한다. 새 의약 Station은0개이며 기존 제작 카드·정렬·공간/재료 검증을 재사용한다. 상세 결정은 [이번 REVIEW](../validation/medicine-expansion/REVIEW.md).
