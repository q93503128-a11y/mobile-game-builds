# Workbench I–II Source / Sink Audit

2026-10-18, 실제 Item86/Recipe71. Recipe의 수량은 변경하지 않았다. `waterskin` 제작과 `purify_waterskin` 상태 변환은 같은 Item을 출력하지만 중복 대체 제작법이 아니다. Recipe graph 예산은 충전 상태 변환을 원재료 제작 경로에서 제외한다.

|Item|SOURCE|SINK / 사용|
|---|---|---|
|arrow|Recipe arrow ×12|shot ammunition|
|axe|Recipe axe ×1|stone_axe ×1; equip/harvest/combat (not consumed)|
|berry|Harvest Rule8|mushroom_stew ×2; meat_stew ×2; fish_stew ×2; mixed_stew ×2; consumption|
|blast_furnace|Recipe blast_furnace ×1|world placement|
|bow|Recipe bow ×1|reinforced_bow ×1; equip/harvest/combat (not consumed); equip/ranged use (not consumed)|
|brazier|Recipe brazier ×1|world placement|
|bronze|Recipe bronze ×3|blast_furnace ×3; reinforced_bow ×2; bronze_arrow ×1; crossbow ×3; lantern ×2; reinforced_wall ×1; reinforced_door ×2; reinforced_gate ×3; spike_trap ×1; brazier ×3|
|bronze_arrow|Recipe bronze_arrow ×12|shot ammunition|
|campfire|Recipe campfire ×1|world placement|
|clean_water_cup|Recipe clean_water_cup ×1|consumption|
|coal|Harvest Rule7|copper_ingot ×1; tin_ingot ×1; steel ×1; torch ×1; lantern ×2; brazier ×2|
|compass|Recipe compass ×1|navigation (not consumed)|
|cooked_fish|Recipe cooked_fish ×1|consumption|
|cooked_meat|Recipe cooked_meat ×1|consumption|
|cooking_pot|Recipe cooking_pot ×1|world placement|
|copper_ingot|Recipe copper_ingot ×1|bronze ×2; prospecting_pan ×2|
|copper_ore|Harvest Rule5|copper_ingot ×1|
|crossbow|Recipe crossbow ×1|equip/harvest/combat (not consumed); equip/ranged use (not consumed)|
|crossbow_bolt|Recipe crossbow_bolt ×12|shot ammunition|
|empty_cup|Recipe empty_cup ×1; Drink raw/clean cup returns container|Fill natural water|
|feather|ChickenLoot 1–1 @1|arrow ×1; bronze_arrow ×1; crossbow_bolt ×1; steel_bolt ×1|
|fish_stew|Recipe fish_stew ×1|consumption|
|fishing_rod|Recipe fishing_rod ×1|fishing (not consumed)|
|furnace|Recipe furnace ×1|world placement|
|iron_axe|Recipe iron_axe ×1|steel_axe ×1; equip/harvest/combat (not consumed)|
|iron_body|Recipe iron_body ×1|steel_body ×1; equip (not consumed)|
|iron_head|Recipe iron_head ×1|steel_head ×1; equip (not consumed)|
|iron_ingot|Recipe iron_ingot ×1|iron_axe ×3; iron_body ×3; iron_head ×2; iron_pickaxe ×3; iron_sword ×3; reinforced_chest ×2; steel ×1; blast_furnace ×3; compass ×1; iron_spear ×3; cooking_pot ×2|
|iron_ore|Harvest Rule2; SkeletonLoot 1–1 @0.2; SkeletonSwordLoot 1–1 @0.2|iron_ingot ×1|
|iron_pickaxe|Recipe iron_pickaxe ×1|steel_pickaxe ×1; equip/harvest/combat (not consumed)|
|iron_spear|Recipe iron_spear ×1|equip/harvest/combat (not consumed)|
|iron_sword|Recipe iron_sword ×1|steel_sword ×1; equip/harvest/combat (not consumed)|
|lantern|Recipe lantern ×1|equip utility (not consumed)|
|leather|BoarLoot 1–1 @1; DeerLoot 1–1 @0.75; ZombieLoot 1–1 @0.2|iron_body ×1; iron_head ×1; leather_body ×2; leather_head ×1; steel_sword ×1; steel_head ×1; steel_body ×1; waterskin ×2; reinforced_bow ×1; crossbow ×2; lantern ×1|
|leather_body|Recipe leather_body ×1|equip (not consumed)|
|leather_head|Recipe leather_head ×1|equip (not consumed)|
|meat_stew|Recipe meat_stew ×1|consumption|
|mixed_stew|Recipe mixed_stew ×1|consumption|
|mushroom|Harvest Rule9|roasted_mushroom ×1; mushroom_stew ×2; meat_stew ×1; fish_stew ×1; mixed_stew ×1; consumption|
|mushroom_stew|Recipe mushroom_stew ×1|consumption|
|pickaxe|Recipe pickaxe ×1|stone_pickaxe ×1; equip/harvest/combat (not consumed)|
|plant_fiber|Harvest Rule4|bow ×4; wood_spear ×2; fishing_rod ×3; waterskin ×3; wood_fence ×1; wood_fence_gate ×2; wood_gate ×2|
|prospecting_pan|Recipe prospecting_pan ×1|equip/harvest/combat (not consumed)|
|raw_fish|Fishing catch ×1|cooked_fish ×1; fish_stew ×1; mixed_stew ×1; consumption|
|raw_meat|BoarLoot 2–2 @1; ChickenLoot 1–1 @1; DeerLoot 2–3 @1; RabbitLoot 1–1 @1; SheepLoot 2–2 @1|cooked_meat ×1; meat_stew ×1; mixed_stew ×1; consumption|
|raw_water_cup|Natural water + Empty Cup|clean_water_cup ×1; consumption|
|reinforced_bow|Recipe reinforced_bow ×1|equip/harvest/combat (not consumed); equip/ranged use (not consumed)|
|reinforced_chest|Recipe reinforced_chest ×1|world placement|
|reinforced_door|Recipe reinforced_door ×1|world placement|
|reinforced_gate|Recipe reinforced_gate ×1|world placement|
|reinforced_wall|Recipe reinforced_wall ×1|world placement|
|roasted_mushroom|Recipe roasted_mushroom ×1|consumption|
|rusted_sword|SkeletonSwordLoot 1–1 @0.12|equip/harvest/combat (not consumed)|
|spike_trap|Recipe spike_trap ×1|world placement|
|steel|Recipe steel ×1|steel_axe ×3; steel_pickaxe ×3; steel_sword ×4; steel_head ×2; steel_body ×3; steel_bolt ×1|
|steel_axe|Recipe steel_axe ×1|equip/harvest/combat (not consumed)|
|steel_body|Recipe steel_body ×1|equip (not consumed)|
|steel_bolt|Recipe steel_bolt ×8|shot ammunition|
|steel_head|Recipe steel_head ×1|equip (not consumed)|
|steel_pickaxe|Recipe steel_pickaxe ×1|equip/harvest/combat (not consumed)|
|steel_sword|Recipe steel_sword ×1|equip/harvest/combat (not consumed)|
|stone|Harvest Rule1; GoblinLoot 1–3 @0.8; SkeletonLoot 1–2 @0.75; SkeletonSwordLoot 1–2 @0.75|campfire ×3; furnace ×8; stone_axe ×4; stone_pickaxe ×4; stone_sword ×5; stone_wall ×3; blast_furnace ×8; stone_spear ×4; cooking_pot ×6; stone_door ×4; stone_fence ×4; stone_fence_gate ×4; stone_gate ×7; reinforced_wall ×3; reinforced_door ×3; reinforced_gate ×5; spike_trap ×2; brazier ×4|
|stone_axe|Recipe stone_axe ×1|iron_axe ×1; equip/harvest/combat (not consumed)|
|stone_door|Recipe stone_door ×1|world placement|
|stone_fence|Recipe stone_fence ×2|world placement|
|stone_fence_gate|Recipe stone_fence_gate ×1|world placement|
|stone_gate|Recipe stone_gate ×1|world placement|
|stone_pickaxe|Recipe stone_pickaxe ×1|iron_pickaxe ×1; equip/harvest/combat (not consumed)|
|stone_spear|Recipe stone_spear ×1|iron_spear ×1; equip/harvest/combat (not consumed)|
|stone_sword|Recipe stone_sword ×1|iron_sword ×1; equip/harvest/combat (not consumed)|
|stone_wall|Recipe stone_wall ×1|world placement|
|storage_chest|Recipe storage_chest ×1|world placement|
|tin_ingot|Recipe tin_ingot ×1|bronze ×1|
|tin_ore|Harvest Rule6|tin_ingot ×1|
|torch|Recipe torch ×4|world placement; held light|
|waterskin|Recipe waterskin ×1; Recipe purify_waterskin ×1|purify_waterskin ×1; fill/drink/refill (container retained)|
|wood|Harvest Rule0; Harvest Rule3; GoblinLoot 1–2 @0.6; ZombieLoot 1–2 @0.6|axe ×5; campfire ×4; empty_cup ×2; furnace ×4; leather_body ×3; leather_head ×2; pickaxe ×5; reinforced_chest ×8; stone_wall ×1; storage_chest ×6; wood_door ×3; wood_sword ×5; wood_wall ×2; workbench ×6; prospecting_pan ×2; blast_furnace ×4; steel_axe ×2; steel_pickaxe ×2; torch ×1; compass ×2; bow ×5; arrow ×2; wood_spear ×4; fishing_rod ×4; wood_fence ×3; wood_fence_gate ×3; wood_gate ×6; stone_door ×2; stone_fence ×1; stone_fence_gate ×2; stone_gate ×3; bronze_arrow ×2; crossbow ×6; crossbow_bolt ×2; steel_bolt ×2; reinforced_wall ×2; reinforced_door ×2; reinforced_gate ×4; spike_trap ×4|
|wood_door|Recipe wood_door ×1|world placement|
|wood_fence|Recipe wood_fence ×2|world placement|
|wood_fence_gate|Recipe wood_fence_gate ×1|world placement|
|wood_gate|Recipe wood_gate ×1|world placement|
|wood_spear|Recipe wood_spear ×1|stone_spear ×1; equip/harvest/combat (not consumed)|
|wood_sword|Recipe wood_sword ×1|stone_sword ×1; equip/harvest/combat (not consumed)|
|wood_wall|Recipe wood_wall ×1|world placement|
|workbench|Recipe workbench ×1|Upgrade II; world placement|
|workbench_ii|Workbench upgrade|world placement|

## 판정

- Bronze는 Blast Furnace뿐 아니라 Reinforced Bow/Crossbow/Lantern/강화 건축/Bronze Arrow에 사용된다. Copper/Tin은 Pan·Bronze 기술/탄약 수요를 가진다. full armor tier를 추가할 이유가 없다.
- Feather는 Arrow/Crossbow Bolt/Bronze Arrow/Steel Bolt에 실제 사용된다. 고아 drop이 아니다.
- Leather는 armor 업그레이드, Sword, ranged equipment, Waterskin/Lantern에 사용된다. 내구도/수리 소모는 없으므로 엄밀한 무한 반복 sink는 아니다. 억지 수리/recipe를 추가하지 않았다.
- Iron은 개인 장비의 이전 장비 소비와 Steel 제작에 각각 들어가는 주 병목이다. Coal은 제련·Steel·조명에 걸쳐 부담이 컸다. source 보상과 지역 회복을 개선했다.
- Rusted Sword는 loot 장비, Workbench II는 upgrade 결과, Raw Water Cup은 수원 결과다. Recipe가 없다는 이유로 죽은 item으로 처리하지 않았다. 현재 활성 item에서 SOURCE나 gameplay 사용이 모두 비어 있는 고아는 발견하지 않았다.
- Mine/Quarry는 현재 landmark이며 새 광석 상자/merchant/광맥 보상은 없다. Iron은 지상/동굴과 Skeleton loot(75%, 1–2)에서 나온다. Coal은 지상 seam/동굴이 실제 경로이며 새로운 적 drop을 만들지 않았다.
