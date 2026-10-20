# Baseline source -> processing -> recipe -> final use

Fetched main: 252972006bdc6436cafb31147574f50e0c62a3cc

|Item|실제 source / processing|직접 Recipe sink|최종 사용|Final use|
|---|---|---|---|---|
|ammo_box|Tool|Recipe ammo_box||module|
|arrow|Material|Recipe arrow||ammo|
|auto_aim|Tool|Recipe auto_aim||module|
|auto_loader|Tool|Recipe auto_loader||module|
|axe|Tool|Recipe axe|stone_axe x1|tool|
|ballista|Placeable|Recipe ballista||place|
|ballista_bolt|Material, AbandonedWorkshop cargo|Recipe ballista_bolt|Ballista가 발사 시 실제 소비||
|berry|Consumable|BerryBushResource x3|mushroom_stew x2; meat_stew x2; fish_stew x2; mixed_stew x2|consume|
|blast_furnace|Placeable|Recipe blast_furnace||place|
|bow|Weapon|Recipe bow|reinforced_bow x1|tool|
|brazier|Placeable|Recipe brazier||place|
|bronze|Material|Recipe bronze|blast_furnace x3; reinforced_bow x2; bronze_arrow x1; crossbow x3; lantern x2; reinforced_wall x1; reinforced_door x2; reinforced_gate x3; spike_trap x1; brazier x3; mechanical_part x1; telescope x1; crystal_lantern x1; turret_workbench x4; signal_beacon x1; repeater x3; auto_loader x1; auto_aim x1; rotation_gear x2; Workbench upgrade (cost in WorkbenchProgression asset)||
|bronze_arrow|Material|Recipe bronze_arrow||ammo|
|campfire|Placeable|Recipe campfire||place|
|clean_water_cup|Consumable|Recipe clean_water_cup||consume, water|
|coal|Material|CoalNodeResource x10|copper_ingot x1; tin_ingot x1; steel x1; torch x1; lantern x2; brazier x2||
|compass|Tool|Recipe compass||navigation|
|cooked_fish|Consumable|Recipe cooked_fish||consume|
|cooked_meat|Consumable|Recipe cooked_meat||consume|
|cooking_pot|Placeable|Recipe cooking_pot||place|
|copper_ingot|Material|Recipe copper_ingot|bronze x2; prospecting_pan x2||
|copper_ore|Material|CopperOreNodeResource x6|copper_ingot x1||
|crossbow|Weapon|Recipe crossbow||tool|
|crossbow_bolt|Material, Deep POI cargo|Recipe crossbow_bolt||ammo|
|crystal|Material|CrystalNodeResource x6|telescope x2; crystal_lantern x2; turret_workbench x2; signal_beacon x2; auto_aim x2; Workbench upgrade (cost in WorkbenchProgression asset)||
|crystal_lantern|Tool|Recipe crystal_lantern||utility light|
|diamond|Material|DiamondNodeResource x3|diamond_pickaxe x2; diamond_axe x1; diamond_sword x2; diamond_spear x2; diamond_head x2; diamond_body x3; Workbench upgrade (cost in WorkbenchProgression asset)||
|diamond_axe|Tool|Recipe diamond_axe||tool|
|diamond_body|Armor|Recipe diamond_body||armor|
|diamond_head|Armor|Recipe diamond_head||armor|
|diamond_pickaxe|Tool|Recipe diamond_pickaxe||tool|
|diamond_spear|Weapon|Recipe diamond_spear||tool|
|diamond_sword|Weapon|Recipe diamond_sword||tool|
|empty_cup|Container|Recipe empty_cup; Drinking cup returns vessel||water|
|feather|Material|ChickenLoot normal 1-1 @1.0|arrow x1; bronze_arrow x1; crossbow_bolt x1; steel_bolt x1||
|fish_stew|Consumable|Recipe fish_stew||consume|
|fishing_rod|Tool|Recipe fishing_rod||fishing|
|furnace|Placeable|Recipe furnace||place|
|industrial_container|DeepMine/AbandonedWorkshop POI 최초 생성, persistent cargo ID (WorldChunkDeepSites.cs)|||place|
|iron_axe|Tool|Recipe iron_axe|steel_axe x1|tool|
|iron_body|Armor|Recipe iron_body|steel_body x1|armor|
|iron_head|Armor|Recipe iron_head|steel_head x1|armor|
|iron_ingot|Material|Recipe iron_ingot|iron_axe x3; iron_body x3; iron_head x2; iron_pickaxe x3; iron_sword x3; reinforced_chest x2; steel x1; blast_furnace x3; compass x1; iron_spear x3; cooking_pot x2; Workbench upgrade (cost in WorkbenchProgression asset)||
|iron_ore|Material|SkeletonLoot normal 1-1 @0.2; SkeletonSwordLoot normal 1-1 @0.2; IronOreResource x6|iron_ingot x1||
|iron_pickaxe|Tool|Recipe iron_pickaxe|steel_pickaxe x1|tool|
|iron_spear|Weapon|Recipe iron_spear|steel_spear x1|tool|
|iron_sword|Weapon|Recipe iron_sword|steel_sword x1|tool|
|lantern|Tool|Recipe lantern|crystal_lantern x1|utility light|
|leather|Material|BoarLoot normal 1-1 @1.0; DeerLoot normal 1-1 @0.75; ZombieLoot normal 1-1 @0.2|iron_body x1; iron_head x1; leather_body x2; leather_head x1; steel_sword x1; steel_head x1; steel_body x1; waterskin x2; reinforced_bow x1; crossbow x2; lantern x1; steel_spear x1; telescope x1; ballista x2; reinforced_frame x2||
|leather_body|Armor|Recipe leather_body||armor|
|leather_head|Armor|Recipe leather_head||armor|
|meat_stew|Consumable|Recipe meat_stew||consume|
|mechanical_part|Material, Deep POI cargo의 확률 보너스|Recipe mechanical_part; Industrial chest bonus|turret_workbench x2; signal_beacon x1; ballista x1; repeater x2; auto_loader x2; auto_aim x1; rotation_gear x1||
|mixed_stew|Consumable|Recipe mixed_stew||consume|
|mushroom|Consumable|MushroomPatchResource x2|roasted_mushroom x1; mushroom_stew x2; meat_stew x1; fish_stew x1; mixed_stew x1|consume|
|mushroom_stew|Consumable|Recipe mushroom_stew||consume|
|pickaxe|Tool|Recipe pickaxe|stone_pickaxe x1|tool|
|plant_fiber|Material|SmallPlantResource x2|bow x4; wood_spear x2; fishing_rod x3; waterskin x3; wood_fence x1; wood_fence_gate x2; wood_gate x2||
|prospecting_pan|Tool|Recipe prospecting_pan||tool|
|raw_fish|Consumable|Fishing|cooked_fish x1; fish_stew x1; mixed_stew x1|consume|
|raw_meat|Consumable|BoarLoot normal 2-2 @1.0; ChickenLoot normal 1-1 @1.0; DeerLoot normal 2-3 @1.0; RabbitLoot normal 1-1 @1.0; SheepLoot normal 2-2 @1.0|cooked_meat x1; meat_stew x1; mixed_stew x1|consume|
|raw_water_cup|Consumable|Natural water + empty cup|clean_water_cup x1|consume, water|
|reinforced_bow|Weapon|Recipe reinforced_bow||tool|
|reinforced_chest|Placeable|Recipe reinforced_chest||place|
|reinforced_door|Placeable|Recipe reinforced_door||place|
|reinforced_frame|Tool|Recipe reinforced_frame||module|
|reinforced_gate|Placeable|Recipe reinforced_gate||place|
|reinforced_wall|Placeable|Recipe reinforced_wall||place|
|repeater|Placeable|Recipe repeater||place|
|roasted_mushroom|Consumable|Recipe roasted_mushroom||consume|
|rotation_gear|Tool|Recipe rotation_gear||module|
|rusted_sword|Weapon|SkeletonSwordLoot rare 1-1 @0.12||tool|
|signal_beacon|Placeable|Recipe signal_beacon||place|
|spike_trap|Placeable|Recipe spike_trap||place|
|steel|Material|Recipe steel|steel_axe x3; steel_pickaxe x3; steel_sword x4; steel_head x2; steel_body x3; steel_bolt x1; steel_spear x3; mechanical_part x1; turret_workbench x4; ballista_bolt x1; ballista x3; repeater x2; auto_loader x1; ammo_box x1; reinforced_frame x2; Workbench upgrade (cost in WorkbenchProgression asset)||
|steel_axe|Tool|Recipe steel_axe|diamond_axe x1|tool|
|steel_body|Armor|Recipe steel_body|diamond_body x1|armor|
|steel_bolt|Material|Recipe steel_bolt||ammo|
|steel_head|Armor|Recipe steel_head|diamond_head x1|armor|
|steel_pickaxe|Tool|Recipe steel_pickaxe|diamond_pickaxe x1|tool|
|steel_spear|Weapon|Recipe steel_spear|diamond_spear x1|tool|
|steel_sword|Weapon|Recipe steel_sword|diamond_sword x1|tool|
|stone|Material|GoblinLoot normal 1-3 @0.8; SkeletonLoot normal 1-2 @0.75; SkeletonSwordLoot normal 1-2 @0.75; StoneResource x6|campfire x3; furnace x8; stone_axe x4; stone_pickaxe x4; stone_sword x5; stone_wall x3; blast_furnace x8; stone_spear x4; cooking_pot x6; stone_door x4; stone_fence x4; stone_fence_gate x4; stone_gate x7; reinforced_wall x3; reinforced_door x3; reinforced_gate x5; spike_trap x2; brazier x4; turret_workbench x8; Workbench upgrade (cost in WorkbenchProgression asset)||
|stone_axe|Tool|Recipe stone_axe|iron_axe x1|tool|
|stone_door|Placeable|Recipe stone_door||place|
|stone_fence|Placeable|Recipe stone_fence||place|
|stone_fence_gate|Placeable|Recipe stone_fence_gate||place|
|stone_gate|Placeable|Recipe stone_gate||place|
|stone_pickaxe|Tool|Recipe stone_pickaxe|iron_pickaxe x1|tool|
|stone_spear|Weapon|Recipe stone_spear|iron_spear x1|tool|
|stone_sword|Weapon|Recipe stone_sword|iron_sword x1|tool|
|stone_wall|Placeable|Recipe stone_wall||place|
|storage_chest|Placeable|Recipe storage_chest||place|
|telescope|Tool|Recipe telescope||scope|
|tin_ingot|Material|Recipe tin_ingot|bronze x1||
|tin_ore|Material|TinOreNodeResource x5|tin_ingot x1||
|torch|Placeable|Recipe torch||place|
|turret_workbench|Placeable|Recipe turret_workbench||place|
|waterskin|Container|Recipe waterskin; Recipe purify_waterskin|purify_waterskin x1|water|
|wood|Material|GoblinLoot normal 1-2 @0.6; ZombieLoot normal 1-2 @0.6; TreeResource x6; FallenLogResource x4|axe x5; campfire x4; empty_cup x2; furnace x4; leather_body x3; leather_head x2; pickaxe x5; reinforced_chest x8; stone_wall x1; storage_chest x6; wood_door x3; wood_sword x5; wood_wall x2; workbench x6; prospecting_pan x2; blast_furnace x4; steel_axe x2; steel_pickaxe x2; torch x1; compass x2; bow x5; arrow x2; wood_spear x4; fishing_rod x4; wood_fence x3; wood_fence_gate x3; wood_gate x6; stone_door x2; stone_fence x1; stone_fence_gate x2; stone_gate x3; bronze_arrow x2; crossbow x6; crossbow_bolt x2; steel_bolt x2; reinforced_wall x2; reinforced_door x2; reinforced_gate x4; spike_trap x4; turret_workbench x8; signal_beacon x2; ballista_bolt x2; ballista x10; repeater x8; ammo_box x4; Workbench upgrade (cost in WorkbenchProgression asset)||
|wood_door|Placeable|Recipe wood_door||place|
|wood_fence|Placeable|Recipe wood_fence||place|
|wood_fence_gate|Placeable|Recipe wood_fence_gate||place|
|wood_gate|Placeable|Recipe wood_gate||place|
|wood_spear|Weapon|Recipe wood_spear|stone_spear x1|tool|
|wood_sword|Weapon|Recipe wood_sword|stone_sword x1|tool|
|wood_wall|Placeable|Recipe wood_wall||place|
|workbench|Placeable|Recipe workbench||place|
|workbench_ii|Placeable|Workbench upgrade II||place|
|workbench_iii|Placeable|Workbench upgrade III||place|

## Station inventory

|Station / minimum tier|New recipes|Cumulative workbench recipes|
|---|---:|---:|
|BlastFurnace 1|1|-|
|Campfire 1|5|-|
|CookingPot 1|4|-|
|Furnace 1|4|-|
|None 1|4|-|
|TurretWorkbench 3|8|-|
|Workbench 1|33|33|
|Workbench 2|21|54|
|Workbench 3|11|65|
