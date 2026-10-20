# 현재 전체 source → processing → recipe → final use

2026-10-20 개연성 교정 후 Unity authored data,121 items /99 recipes. Recipe sink 수는 소비 빈도와 다르다. 발사·장착·설치·음식·용기 상태·업그레이드는 직접 사용처다. 전체 비용과 시간은 CONTENT_EXPANSION을 따른다.

|Item|실제 source / processing|직접 Recipe sink|최종 사용|
|---|---|---|---|
|ammo_box|ammo_box@TurretWorkbench|—|module|
|arrow|arrow@Workbench|—|ammo, shot Arrow|
|auto_aim|auto_aim@TurretWorkbench|—|module|
|auto_loader|auto_loader@TurretWorkbench|—|module|
|axe|axe@Workbench|stone_axe|tool|
|ballista|ballista@TurretWorkbench|—|place, place/use world object|
|ballista_bolt|ballista_bolt@TurretWorkbench, AbandonedWorkshop cargo|—|Ballista가 발사 시 실제 소비|
|barbed_bolt|barbed_bolt@Workbench, Goblin Camp supplies 최초 보너스|—|shot Bolt|
|berry|BerryBushResource|fish_stew, meat_stew, mixed_stew, mushroom_stew, restorative_broth, trail_ration|consume, consume H/T/HP/Stamina|
|blast_furnace|blast_furnace@Workbench|—|place, place/use world object|
|bone|BatLoot, DeerLoot, GiantBoarLoot, MountainGoatLoot, RabbitLoot, SkeletonLoot, SkeletonSwordLoot, WolfLoot, ZombieLoot|bone_arrow, bone_broth, bone_trap|recipe ingredient|
|bone_arrow|bone_arrow@Workbench, Goblin Camp supplies 최초 보너스|—|shot Arrow|
|bone_broth|bone_broth@CookingPot|—|consume H/T/HP/Stamina|
|bone_trap|bone_trap@Workbench|—|place/use world object|
|bow|bow@Workbench|reinforced_bow|tool|
|brazier|brazier@Workbench|—|place, place/use world object|
|bronze|bronze@Furnace, Goblin Camp supplies 최초 보너스|auto_aim, auto_loader, blast_furnace, brazier, bronze_arrow, crossbow, crystal_lantern, lantern, mechanical_part, reinforced_bow, reinforced_door, reinforced_gate, reinforced_wall, repeater, rotation_gear, signal_beacon, spike_trap, telescope, turret_workbench|recipe ingredient|
|bronze_arrow|bronze_arrow@Workbench|—|ammo, shot Arrow|
|campfire|campfire@None|—|place, place/use world object|
|carapace_vest|carapace_vest@Workbench|—|equip Body|
|chitin|CrystalCrawlerLoot, StoneCrawlerLoot|barbed_bolt, carapace_vest, reinforced_frame|recipe ingredient|
|clean_water_cup|clean_water_cup@Campfire|—|consume, consume H/T/HP/Stamina, water|
|coal|CoalNodeResource|brazier, copper_ingot, lantern, steel, tin_ingot, torch|recipe ingredient|
|compass|compass@Workbench|—|navigation|
|cooked_fish|cooked_fish@Campfire|—|consume, consume H/T/HP/Stamina|
|cooked_meat|cooked_meat@Campfire|—|consume, consume H/T/HP/Stamina|
|cooking_pot|cooking_pot@Workbench|—|place, place/use world object|
|copper_ingot|copper_ingot@Furnace|bronze, prospecting_pan|recipe ingredient|
|copper_ore|CopperOreNodeResource|copper_ingot|recipe ingredient|
|crossbow|crossbow@Workbench|—|tool|
|crossbow_bolt|crossbow_bolt@Workbench, Deep POI cargo|—|ammo, shot Bolt|
|crystal|CrystalCrawlerLoot, CrystalNodeResource|auto_aim, crystal_lantern, signal_beacon, telescope, turret_workbench|recipe ingredient|
|crystal_lantern|crystal_lantern@Workbench|—|utility light|
|diamond|DiamondNodeResource|diamond_axe, diamond_body, diamond_head, diamond_pickaxe, diamond_spear, diamond_sword|recipe ingredient|
|diamond_axe|diamond_axe@Workbench|—|tool|
|diamond_body|diamond_body@Workbench|—|armor, equip Body|
|diamond_head|diamond_head@Workbench|—|armor, equip Head|
|diamond_pickaxe|diamond_pickaxe@Workbench|—|tool|
|diamond_spear|diamond_spear@Workbench|—|tool|
|diamond_sword|diamond_sword@Workbench|—|tool|
|empty_cup|empty_cup@None|—|water|
|feather|ChickenLoot, CrowLoot|arrow, barbed_bolt, bone_arrow, bronze_arrow, crossbow_bolt, steel_bolt|recipe ingredient|
|fish_stew|fish_stew@CookingPot|—|consume, consume H/T/HP/Stamina|
|fishing_rod|fishing_rod@Workbench|—|fishing|
|furnace|furnace@Workbench|—|place, place/use world object|
|healing_salve|healing_salve@None, Goblin Camp supplies 최초 보너스|—|consume H/T/HP/Stamina|
|industrial_container|DeepMine/AbandonedWorkshop POI 최초 생성, persistent cargo ID (WorldChunkDeepSites.cs)|—|place, place/use world object|
|iron_axe|iron_axe@Workbench|steel_axe|tool|
|iron_body|iron_body@Workbench|steel_body|armor, equip Body|
|iron_head|iron_head@Workbench|steel_head|armor, equip Head|
|iron_ingot|iron_ingot@Furnace|blast_furnace, compass, cooking_pot, iron_axe, iron_body, iron_head, iron_pickaxe, iron_spear, iron_sword, reinforced_chest, steel|recipe ingredient|
|iron_ore|IronOreResource|iron_ingot|recipe ingredient|
|iron_pickaxe|iron_pickaxe@Workbench|steel_pickaxe|tool|
|iron_spear|iron_spear@Workbench|steel_spear|tool|
|iron_sword|iron_sword@Workbench|steel_sword|tool|
|lantern|lantern@Workbench|crystal_lantern|utility light|
|leather|BoarLoot, DeerLoot, GiantBoarLoot, GoblinLoot, MountainGoatLoot, SlingshotGoblinLoot, WolfLoot, Goblin Camp supplies 최초 보너스|ballista, carapace_vest, crossbow, iron_body, iron_head, lantern, leather_body, leather_head, reinforced_bow, reinforced_frame, steel_body, steel_head, steel_spear, steel_sword, telescope, waterskin|recipe ingredient|
|leather_body|leather_body@Workbench|—|armor, equip Body|
|leather_head|leather_head@Workbench|—|armor, equip Head|
|meat_stew|meat_stew@CookingPot|—|consume, consume H/T/HP/Stamina|
|mechanical_part|mechanical_part@Workbench, Deep POI cargo의 확률 보너스, Goblin Camp supplies 최초 보너스|auto_aim, auto_loader, ballista, repeater, rotation_gear, signal_beacon, turret_workbench|recipe ingredient|
|medicinal_gland|BatLoot, BogLeechLoot|healing_salve, restorative_broth|recipe ingredient|
|mixed_stew|mixed_stew@CookingPot|—|consume, consume H/T/HP/Stamina|
|mushroom|MushroomPatchResource|bone_broth, fish_stew, meat_stew, mixed_stew, mushroom_stew, restorative_broth, roasted_mushroom, trail_ration|consume, consume H/T/HP/Stamina|
|mushroom_stew|mushroom_stew@CookingPot|—|consume, consume H/T/HP/Stamina|
|pickaxe|pickaxe@Workbench|stone_pickaxe|tool|
|plant_fiber|SmallPlantResource|bow, carapace_vest, fishing_rod, healing_salve, waterskin, wood_fence, wood_fence_gate, wood_gate, wood_spear|recipe ingredient|
|prospecting_pan|prospecting_pan@Workbench|—|tool|
|raw_fish|fishing|cooked_fish, fish_stew, mixed_stew|consume, consume H/T/HP/Stamina|
|raw_meat|BoarLoot, ChickenLoot, CrowLoot, DeerLoot, GiantBoarLoot, MountainGoatLoot, RabbitLoot, SheepLoot, WolfLoot|bone_broth, cooked_meat, meat_stew, mixed_stew, trail_ration|consume, consume H/T/HP/Stamina|
|raw_water_cup|water source/container state|clean_water_cup|consume, consume H/T/HP/Stamina, water|
|reinforced_bow|reinforced_bow@Workbench|—|tool|
|reinforced_chest|reinforced_chest@Workbench|—|place, place/use world object|
|reinforced_door|reinforced_door@Workbench|—|place, place/use world object|
|reinforced_frame|reinforced_frame@TurretWorkbench|—|module|
|reinforced_gate|reinforced_gate@Workbench|—|place, place/use world object|
|reinforced_wall|reinforced_wall@Workbench|—|place, place/use world object|
|repeater|repeater@TurretWorkbench|—|place, place/use world object|
|restorative_broth|restorative_broth@CookingPot|—|consume H/T/HP/Stamina|
|roasted_mushroom|roasted_mushroom@Campfire|—|consume, consume H/T/HP/Stamina|
|rotation_gear|rotation_gear@TurretWorkbench|—|module|
|rusted_sword|SkeletonSwordLoot|—|tool|
|signal_beacon|signal_beacon@Workbench|—|place, place/use world object|
|spike_trap|spike_trap@Workbench|—|place, place/use world object|
|steel|steel@BlastFurnace|ammo_box, auto_loader, ballista, ballista_bolt, mechanical_part, reinforced_frame, repeater, steel_axe, steel_body, steel_bolt, steel_head, steel_pickaxe, steel_spear, steel_sword, turret_workbench|recipe ingredient|
|steel_axe|steel_axe@Workbench|diamond_axe|tool|
|steel_body|steel_body@Workbench|diamond_body|armor, equip Body|
|steel_bolt|steel_bolt@Workbench|—|ammo, shot Bolt|
|steel_head|steel_head@Workbench|diamond_head|armor, equip Head|
|steel_pickaxe|steel_pickaxe@Workbench|diamond_pickaxe|tool|
|steel_spear|steel_spear@Workbench|diamond_spear|tool|
|steel_sword|steel_sword@Workbench|diamond_sword|tool|
|stone|StoneCrawlerLoot, StoneResource|blast_furnace, brazier, campfire, cooking_pot, furnace, reinforced_door, reinforced_gate, reinforced_wall, spike_trap, stone_axe, stone_door, stone_fence, stone_fence_gate, stone_gate, stone_pickaxe, stone_spear, stone_sword, stone_wall, turret_workbench|recipe ingredient|
|stone_axe|stone_axe@Workbench|iron_axe|tool|
|stone_door|stone_door@Workbench|—|place, place/use world object|
|stone_fence|stone_fence@Workbench|—|place, place/use world object|
|stone_fence_gate|stone_fence_gate@Workbench|—|place, place/use world object|
|stone_gate|stone_gate@Workbench|—|place, place/use world object|
|stone_pickaxe|stone_pickaxe@Workbench|iron_pickaxe|tool|
|stone_spear|stone_spear@Workbench|iron_spear|tool|
|stone_sword|stone_sword@Workbench|iron_sword|tool|
|stone_wall|stone_wall@Workbench|—|place, place/use world object|
|storage_chest|storage_chest@Workbench|—|place, place/use world object|
|telescope|telescope@Workbench|—|scope|
|tin_ingot|tin_ingot@Furnace|bronze|recipe ingredient|
|tin_ore|TinOreNodeResource|tin_ingot|recipe ingredient|
|torch|torch@None|—|place, place/use world object|
|trail_ration|trail_ration@Campfire|—|consume H/T/HP/Stamina|
|turret_workbench|turret_workbench@Workbench|—|place, place/use world object|
|waterskin|purify_waterskin@Campfire, waterskin@Workbench|purify_waterskin|water|
|wood|FallenLogResource, TreeResource|ammo_box, arrow, axe, ballista, ballista_bolt, barbed_bolt, blast_furnace, bone_arrow, bone_trap, bow, bronze_arrow, campfire, compass, crossbow, crossbow_bolt, empty_cup, fishing_rod, furnace, leather_body, leather_head, pickaxe, prospecting_pan, reinforced_chest, reinforced_door, reinforced_gate, reinforced_wall, repeater, signal_beacon, spike_trap, steel_axe, steel_bolt, steel_pickaxe, stone_door, stone_fence, stone_fence_gate, stone_gate, stone_wall, storage_chest, torch, turret_workbench, wood_door, wood_fence, wood_fence_gate, wood_gate, wood_spear, wood_sword, wood_wall, workbench|recipe ingredient|
|wood_door|wood_door@Workbench|—|place, place/use world object|
|wood_fence|wood_fence@Workbench|—|place, place/use world object|
|wood_fence_gate|wood_fence_gate@Workbench|—|place, place/use world object|
|wood_gate|wood_gate@Workbench|—|place, place/use world object|
|wood_spear|wood_spear@Workbench|stone_spear|tool|
|wood_sword|wood_sword@Workbench|stone_sword|tool|
|wood_wall|wood_wall@Workbench|—|place, place/use world object|
|workbench|workbench@None|—|place, place/use world object|
|workbench_ii|installed upgrade|—|place, place/use world object|
|workbench_iii|installed upgrade|—|place, place/use world object|
