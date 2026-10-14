"""Validate the committed playable Unity source without requiring the Editor."""
from pathlib import Path
import json
import re
import sys

root=Path(sys.argv[1] if len(sys.argv)>1 else Path(__file__).resolve().parents[1]/'client')
errors=[]
def check(condition,message):
    if not condition: errors.append(message)

scene=root/'Assets/Wildfolk/Scenes/Wildfolk.unity'
check(scene.is_file(),'Playable scene is missing')
required=['Assets','Packages','ProjectSettings']
for name in required: check((root/name).is_dir(),f'Missing Unity directory: {name}')
check('6000.3.19f1' in (root/'ProjectSettings/ProjectVersion.txt').read_text(),'Unexpected Unity version')
settings=(root/'ProjectSettings/ProjectSettings.asset').read_text()
for token in ['defaultScreenOrientation: 3','defaultScreenWidth: 1920','defaultScreenHeight: 1080','allowedAutorotateToPortrait: 0','allowedAutorotateToPortraitUpsideDown: 0','allowedAutorotateToLandscapeLeft: 1','allowedAutorotateToLandscapeRight: 1']:
    check(token in settings,f'Mobile setting missing: {token}')
build=(root/'ProjectSettings/EditorBuildSettings.asset').read_text()
check('Assets/Wildfolk/Scenes/Wildfolk.unity' in build,'Playable scene absent from build settings')
check('WildfolkVisual' not in build,'Replaced visual scene still enabled')
check('SampleScene' not in build,'Replaced sample scene is still enabled')
manifest=json.loads((root/'Packages/manifest.json').read_text())
check(manifest['dependencies'].get('com.unity.render-pipelines.universal')=='17.3.0','Unexpected URP version')
for name in ['WorkbenchII','ReinforcedChest','StoneWall','Player','Enemy','WorldPickup','Workbench','Campfire','Tree','Stone','FallenLog','SmallPlant','Goblin','Boar','Chicken','IronOreNode','Furnace','WoodWall','WoodDoor','StorageChest']:
    check((root/f'Assets/Wildfolk/Prefabs/{name}.prefab').is_file(),f'Missing playable prefab: {name}')
for name in ['PlayerVisual','EnemyVisual']:
    check(not (root/f'Assets/Wildfolk/Prefabs/{name}.prefab').exists(),f'Unused visual prefab remains: {name}')
check(not (root/'Assets/Wildfolk/Art/icon_food.png').exists(),'Unused food icon remains')
for name in ['profile','icon_dash','leather_head','leather_body','leather_head_worn','leather_body_worn','iron_head','iron_body','iron_head_worn','iron_body_worn','workbench_ii','reinforced_chest','stone_wall','player_eye','zombie_eye','zombie_hand','axe','pickaxe','wood_sword','workbench','icon_pause','raw_meat','cooked_meat','water','goblin','feather','leather','icon_interact','campfire','boar','chicken','furnace','wood_wall','wood_door','storage_chest','iron_ore_node','iron_ore','iron_ingot','stone_axe','stone_pickaxe','stone_sword','iron_axe','iron_pickaxe','iron_sword','small_shrub','empty_cup','raw_water_cup','clean_water_cup','sun','moon']:
    check((root/f'Assets/Wildfolk/Art/{name}.png').is_file(),f'Missing character/tool art: {name}')
for name in ['CharacterGrowth','Locomotion','LeatherHead','LeatherBody','IronHead','IronBody','WorkbenchIIPlacement','ReinforcedChestPlacement','StoneWallPlacement','Axe','Pickaxe','AxeTool','PickaxeTool','WoodSword','WoodSwordTool','Workbench','WorkbenchPlacement','Recipes','WorkbenchRecipe','AxeRecipe','PickaxeRecipe','WoodSwordRecipe','Campfire','CampfirePlacement','CampfireRecipe','RawMeat','CookedMeat','EmptyCup','RawWaterCup','CleanWaterCup','WoodenCupContainer','ContentCatalog','AnimalPopulation','EnemySteering','Survival','NaturalWater','CookedMeatRecipe','WoodTier','StoneTier','IronTier','IronOre','IronIngot','Furnace','StorageChest','WoodWall','WoodDoor','DayNight','ZombieSpawn','AnimalMovement','WorldGeneration','TemperateBiome','FacilityLight','Goblin','ChickenLoot','BoarLoot','ZombieLoot','GoblinLoot','Feather','Leather']:
    check((root/f'Assets/Wildfolk/Data/{name}.asset').is_file(),f'Missing item/crafting definition: {name}')
for name in ['Data/StartingEquipment.asset','Runtime/Data/StartingLoadout.cs']:
    check(not (root/f'Assets/Wildfolk/{name}').exists(),f'Removed starting loadout remains: {name}')
check(not (root/'Assets/Wildfolk/Data/Water.asset').exists(),'Containerless Water asset remains')
check((root/'Assets/Wildfolk/Editor/DeveloperHarness.cs').is_file(),'Editor developer harness missing')
check((root/'Assets/Wildfolk/Editor/ProjectDataValidator.cs').is_file(),'Editor validator missing')
check(not (root/'Assets/Wildfolk/Runtime/Player/PlayerAction.cs').exists(),'Replaced contextual Action remains')
check('StorageSlots: 24' in (root/'Assets/Wildfolk/Data/StorageChestPlacement.asset').read_text(),'Chest capacity is not 24')
check('ParallelCapacity: 3' in (root/'Assets/Wildfolk/Data/FurnacePlacement.asset').read_text(),'Furnace capacity is not 3')
check(not (root/'Assets/Wildfolk/Runtime/Combat/FistCombat.cs').exists(),'Replaced fist-only combat remains')
for name in ['Presentation/CharacterVisualLayers','Player/CharacterStats','Player/PlayerLocomotion','Player/StaminaModel','UI/ProfileHud','UI/CharacterScreen','Persistence/GrowthValidation','Crafting/RecipePresentation','Presentation/CharacterFacing','Player/PlayerEquipment','Combat/PlayerCombat','UI/HotbarView','Player/PlayerPrimaryAction','Player/PlayerInteraction','Player/PlayerItemDrop','Generation/WorldGenerator','Generation/WorldMap','Generation/WorldTerrainRenderer','World/GameplayLight','Enemies/ZombieLifecycle','Enemies/GoblinController','Enemies/GoblinSpawner','World/LootOnDeath','Player/PlayerProgression','UI/InventoryDropZone','Crafting/PlayerCrafting','World/PlayerPlacement','UI/InventoryDragController','Core/WorldPauseController','Crafting/StationProcessing','World/WorldClock','World/WorldTimeModel','World/WorldSpawnRegion','World/StorageChest','World/WorldDoor','World/NeutralAnimal','Enemies/ZombieSpawner','Player/PlayerStorage','UI/ChestPanel','Persistence/WorldSaveService','Persistence/SaveRepository','Persistence/SaveValidation','Generation/ResourceRegeneration','Generation/AnimalPopulation','World/WorldNavigation','Enemies/EnemySteering','Core/PrefabPool','Combat/CombatAffiliation','Input/PlayerInputCommands','Input/PlayerInputAdapter','UI/StackSplitPanel']:
    check((root/f'Assets/Wildfolk/Runtime/{name}.cs').is_file(),f'Missing shared character/tool runtime: {name}')
for name in ['VisualPrototypeBuilder','VisualHudBuilder']:
    check(not (root/f'Assets/Wildfolk/Editor/{name}.cs').exists(),f'Replaced builder remains: {name}')
gameplay=(root/'Assets/Wildfolk/Data/GameplaySettings.asset').read_text()
check('InventorySlots: 36' in gameplay,'Inventory is not 36 slots')
view=(root/'Assets/Wildfolk/Editor/VisualGameView.cs').read_text()
check('window.Focus(' not in view and 'GetWindow(' not in view,'Resolution tool can steal window focus')

check('PlayUnfocused' in view,'Game View can request focused Play')
check(not (root/'Assets/Wildfolk/Audio/action.wav').exists(),'Unsuitable generic beep remains')
check(not (root/'Assets/Wildfolk/Runtime/Crafting/StationCooking.cs').exists(),'Replaced cooking-only processor remains')

guids={}
for meta in (root/'Assets').rglob('*.meta'):
    match=re.search(r'^guid: ([0-9a-f]{32})$',meta.read_text(),re.M)
    if not match: continue
    guid=match[1]
    check(guid not in guids,f'Duplicate GUID: {meta}')
    guids[guid]=meta
    check(Path(str(meta)[:-5]).exists(),f'Orphaned meta: {meta}')

for file in (root/'Assets/Wildfolk').rglob('*'):
    if file.is_file() and file.suffix!='.meta':
        check(Path(str(file)+'.meta').exists(),f'Missing meta: {file}')
    if file.suffix not in ['.unity','.prefab']: continue
    content=file.read_text()
    for script_guid in re.findall(r'm_Script: \{fileID: 11500000, guid: ([0-9a-f]{32})',content):
        # UI package scripts live outside Assets. Runtime project scripts must have metas.
        if script_guid in guids: check(Path(str(guids[script_guid])[:-5]).suffix=='.cs',f'Invalid script reference in {file}')
    for guid in re.findall(r'm_Sprite: \{fileID: [0-9-]+, guid: ([0-9a-f]{32})',content):
        check(guid in guids,f'Missing sprite in {file}: {guid}')
    for visible in re.findall(r'^\s*m_Text: (.*)$',content,re.M):
        check(not re.search(r'DEBUG|TEST|TODO|WIP|DMG|Hotfix|Override|placeholder',visible,re.I),f'Forbidden visible text: {visible}')

if errors:
    print('\n'.join(errors));sys.exit(1)
print(f'PASS: Unity source structure, Landscape settings, {len(guids)} unique asset GUIDs, sprite references and visible text.')
