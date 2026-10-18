using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
namespace Wildfolk.Editor
{
    // Content authoring is kept out of runtime. Existing scenes and object IDs are preserved.
    public static class ContentExpansionAssets
    {
        private static ContentCatalog catalog;
        private static List<ItemDefinition> items;
        private static List<RecipeDefinition> recipes;
        private static readonly List<GameObject> sources=new List<GameObject>();
        private static T D<T>(string name)where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        private static ItemDefinition I(string id)=>items.Find(i=>i.Id==id)??throw new InvalidOperationException(id);
        private static ItemAmount C(string id,int n)=>new ItemAmount{Item=I(id),Count=n};
        private static ItemDefinition Item(string name,string id,ItemCategory category=ItemCategory.Material,int stack=50)
        {
            var item=GameplayAssets.Asset<ItemDefinition>(name,d=>{d.Id=id;d.DisplayName=name;d.Category=category;d.MaxStack=stack;d.Icon=VisualArtImporter.Sprite(id);});
            if(!items.Contains(item))items.Add(item);return item;
        }
        private static RecipeDefinition Recipe(string name,ItemDefinition item,int n,int tier,RecipeCategory category,ItemAmount[] cost,StationKind station=StationKind.Workbench,float seconds=0,string id=null)
        {
            var recipe=GameplayAssets.Asset<RecipeDefinition>(name+"Recipe",d=>{d.Id=id??item.Id;d.Result=item;d.ResultCount=n;d.RequiredWorkbenchTier=tier;d.Category=category;d.RequiredStation=station;d.Ingredients=cost;d.ProcessingSeconds=seconds;d.Processing=seconds>0 ? ProcessingKind.Cooking : ProcessingKind.None;d.Tier=item.Tier;d.SortOrder=300+recipes.Count;});
            if(!recipes.Contains(recipe))recipes.Add(recipe);return recipe;
        }
        private static ItemDefinition Food(string name,string id,int hunger,int thirst)
        {var item=Item(name,id,ItemCategory.Consumable);item.Consumable=GameplayAssets.Asset<ConsumableDefinition>(name+"Food",d=>{d.HungerRestore=hunger;d.ThirstRestore=thirst;d.ThirstDrainMultiplier=1;});EditorUtility.SetDirty(item);return item;}
        private static ItemDefinition Weapon(string name,string id,string tier,ToolKind kind,int damage,float range,float arc,float seconds)
        {
            var item=Item(name,id,ItemCategory.Weapon,1);item.Tool=GameplayAssets.Asset<ToolDefinition>(name+"Tool",t=>{t.Kind=kind;t.Tier=D<EquipmentTier>(tier+"Tier");t.HeldSprite=item.Icon;t.Damage=damage;t.Duration=seconds*.7f;t.Cooldown=seconds;t.Attack.Range=range;t.Attack.ArcAngle=arc;t.Attack.MaxTargets=1;t.HandExtension=kind==ToolKind.Spear ? .95f : .25f;t.SwingArc=kind==ToolKind.Spear ? 1 : 35;t.Efficiencies=new[]{new ResourceEfficiency{Kind=ResourceKind.Wood,Multiplier=.25f},new ResourceEfficiency{Kind=ResourceKind.Stone,Multiplier=.25f}};});EditorUtility.SetDirty(item);return item;
        }
        private static ItemDefinition Ranged(string name,string id,bool crossbow,bool reinforced,ItemAmount[] costs,int tier)
        {
            var item=Weapon(name,id,reinforced || crossbow ? "Iron" : "Wood",crossbow ? ToolKind.Crossbow : ToolKind.Bow,crossbow ? 31 : reinforced ? 19 : 14,crossbow ? 19 : reinforced ? 16 : 14,30,crossbow ? 1.55f : reinforced ? .78f : .8f);
            item.Ranged=GameplayAssets.Asset<RangedWeaponDefinition>(name+"Ranged",d=>{d.Ammunition=crossbow ? AmmoKind.Bolt : AmmoKind.Arrow;d.Crossbow=crossbow;d.Damage=item.Tool.Damage;d.DrawSeconds=crossbow ? .08f : .35f;d.RecoverySeconds=crossbow ? 1.47f : reinforced ? .43f : .45f;d.Speed=crossbow ? 24 : reinforced ? 22 : 18;d.Range=item.Tool.Attack.Range;});EditorUtility.SetDirty(item);Recipe(name,item,1,tier,RecipeCategory.Weapons,costs);return item;
        }
        private static ItemDefinition Building(string name,string id,int tier,int hp,float width,ItemAmount[] cost,bool door=false,bool fence=false,StationKind station=StationKind.None,int count=1)
        {
            var item=Item(name,id,ItemCategory.Placeable,20);var place=GameplayAssets.Asset<PlaceableDefinition>(name+"Placement",p=>{p.Item=item;p.Vitality=GameplayAssets.Asset<VitalityDefinition>(name+"Vitality",v=>v.MaximumHealth=hp);p.Footprint=new Vector2(width,station==StationKind.None ? .6f : 1.3f);p.MaximumDistance=3.5f;p.IsBarrier=station==StationKind.None && id!="brazier" && id!="spike_trap";p.IsDoor=door;p.Station=station;p.WorkbenchTier=1;p.ProcessingMode=ProcessingMode.Sequential;p.InputCapacity=30;p.OutputCapacity=60;});item.Placement=place;EditorUtility.SetDirty(item);
            GameplayActorBuilder.Facility(place,AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/WorldPickup.prefab").GetComponent<WorldPickup>(),D<GameplaySettings>("GameplaySettings"));
            var path=AssetDatabase.GetAssetPath(place.Prefab);var root=PrefabUtility.LoadPrefabContents(path);var visual=root.GetComponent<WorldPlaceable>().Visual;
            visual.sprite=item.Icon;visual.transform.localScale=Vector3.one*(width/item.Icon.bounds.size.x);
            if(door)
            {var d=root.GetComponent<WorldDoor>();visual.sprite=VisualArtImporter.Sprite(id+"_frame");d.Hinge.localPosition=Vector3.left*width*.5f;var leaf=d.Hinge.GetComponentInChildren<SpriteRenderer>();leaf.sprite=VisualArtImporter.Sprite(id+"_leaf");leaf.transform.localPosition=Vector3.right*width*.5f;leaf.transform.localScale=Vector3.one*(width/leaf.sprite.bounds.size.x);}
            if(id=="spike_trap"){root.GetComponent<BoxCollider2D>().isTrigger=true;root.AddComponent<SpikeTrap>();}
            if(id=="brazier")
            {var light=GameplayAssets.Asset<GameplayLightDefinition>("BrazierLight",d=>{d.Radius=11;d.Strength=1.05f;d.Color=new Color(1,.73f,.4f);d.DetersEnemies=false;});AddLight(root,light);}
            PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);sources.Add(place.Prefab.gameObject);
            Recipe(name,item,count,tier,place.IsBarrier || id=="spike_trap" ? RecipeCategory.Building : RecipeCategory.Facilities,cost);return item;
        }
        private static void AddLight(GameObject root,GameplayLightDefinition data)
        {var go=new GameObject("Warm light");go.transform.SetParent(root.transform,false);var light=go.AddComponent<Light2D>();light.lightType=Light2D.LightType.Point;light.intensity=0;var effect=go.AddComponent<LocalFirelight>();effect.Light=light;effect.Definition=data;go.AddComponent<GameplayLight>().Definition=data;}
        public static void BuildBatch()
        {
            sources.Clear();VisualArtImporter.Import();WorldArt.SpriteMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Wildfolk/Art/WorldSprite.mat");catalog=D<ContentCatalog>("ContentCatalog");items=new List<ItemDefinition>(catalog.Items);recipes=new List<RecipeDefinition>(catalog.Recipes);
            var fiber=Item("PlantFiber","plant_fiber");var berry=Food("Berry","berry",8,3);var mushroom=Food("Mushroom","mushroom",5,0);var fish=Food("RawFish","raw_fish",6,0);
            var world=D<WorldGenerationDefinition>("WorldGeneration");foreach(var r in world.Resources)if(r.Prefab.name=="SmallPlant"){r.Prefab.Definition.Kind=ResourceKind.PlantFiber;r.Prefab.Definition.DropItem=fiber;r.Prefab.Definition.DropCount=2;EditorUtility.SetDirty(r.Prefab.Definition);}
            AddSource(world,"BerryBush","berry_bush",berry,18,3,false);AddSource(world,"MushroomPatch","mushroom_patch",mushroom,12,2,true);
            var bow=Ranged("Bow","bow",false,false,new[]{C("wood",5),C("plant_fiber",4)},1);
            Ammo("Arrow","arrow",AmmoKind.Arrow,0,1,12,new[]{C("wood",2),C("feather",1)});
            var spear=Weapon("WoodSpear","wood_spear","Wood",ToolKind.Spear,12,2.55f,32,.72f);Recipe("WoodSpear",spear,1,1,RecipeCategory.Weapons,new[]{C("wood",4),C("plant_fiber",2)});
            spear=Weapon("StoneSpear","stone_spear","Stone",ToolKind.Spear,20,2.65f,30,.76f);Recipe("StoneSpear",spear,1,1,RecipeCategory.Weapons,new[]{C("wood_spear",1),C("stone",4)});
            spear=Weapon("IronSpear","iron_spear","Iron",ToolKind.Spear,29,2.8f,28,.78f);Recipe("IronSpear",spear,1,1,RecipeCategory.Weapons,new[]{C("stone_spear",1),C("iron_ingot",3)});
            var rod=Item("FishingRod","fishing_rod",ItemCategory.Tool,1);rod.FishingRod=true;rod.HeldSprite=rod.Icon;EditorUtility.SetDirty(rod);Recipe("FishingRod",rod,1,1,RecipeCategory.Tools,new[]{C("wood",4),C("plant_fiber",3)});
            Recipe("RoastedMushroom",Food("RoastedMushroom","roasted_mushroom",16,0),1,1,RecipeCategory.Survival,new[]{C("mushroom",1)},StationKind.Campfire,5);
            Recipe("CookedFish",Food("CookedFish","cooked_fish",26,0),1,1,RecipeCategory.Survival,new[]{C("raw_fish",1)},StationKind.Campfire,5);
            var pot=Building("CookingPot","cooking_pot",1,120,1.7f,new[]{C("stone",6),C("iron_ingot",2)},station:StationKind.CookingPot);recipes.Find(r=>r.Result==pot).ProgressionShared=1;
            Recipe("MushroomStew",Food("MushroomStew","mushroom_stew",35,10),1,1,RecipeCategory.Survival,new[]{C("mushroom",2),C("berry",2)},StationKind.CookingPot,7);
            Recipe("MeatStew",Food("MeatStew","meat_stew",48,12),1,1,RecipeCategory.Survival,new[]{C("raw_meat",1),C("mushroom",1),C("berry",2)},StationKind.CookingPot,8);
            Recipe("FishStew",Food("FishStew","fish_stew",46,15),1,1,RecipeCategory.Survival,new[]{C("raw_fish",1),C("mushroom",1),C("berry",2)},StationKind.CookingPot,8);
            Recipe("MixedStew",Food("MixedStew","mixed_stew",65,18),1,1,RecipeCategory.Survival,new[]{C("raw_meat",1),C("raw_fish",1),C("mushroom",1),C("berry",2)},StationKind.CookingPot,10);
            var skin=Item("Waterskin","waterskin",ItemCategory.Container,1);skin.WaterCapacity=3;skin.RawDrink=I("raw_water_cup").Consumable;skin.CleanDrink=I("clean_water_cup").Consumable;EditorUtility.SetDirty(skin);Recipe("Waterskin",skin,1,1,RecipeCategory.Survival,new[]{C("leather",2),C("plant_fiber",3)});
            var purify=Recipe("PurifyWaterskin",skin,1,1,RecipeCategory.Survival,new[]{new ItemAmount{Item=skin,Count=1,Charges=3,Quality=LiquidState.Raw}},StationKind.Campfire,10,"purify_waterskin");purify.ResultCharges=3;purify.ResultQuality=LiquidState.Clean;purify.DisplayKey="recipe.purify_waterskin";EditorUtility.SetDirty(purify);
            Building("WoodFence","wood_fence",1,90,2.2f,new[]{C("wood",3),C("plant_fiber",1)},fence:true,count:2);
            Building("WoodFenceGate","wood_fence_gate",1,90,2.2f,new[]{C("wood",3),C("plant_fiber",2)},door:true,fence:true);
            Building("WoodGate","wood_gate",1,140,3.4f,new[]{C("wood",6),C("plant_fiber",2)},door:true);
            Building("StoneDoor","stone_door",1,200,2.1f,new[]{C("stone",4),C("wood",2)},door:true);
            Building("StoneFence","stone_fence",1,160,2.2f,new[]{C("stone",4),C("wood",1)},fence:true,count:2);
            Building("StoneFenceGate","stone_fence_gate",1,160,2.2f,new[]{C("stone",4),C("wood",2)},door:true,fence:true);
            Building("StoneGate","stone_gate",1,240,3.4f,new[]{C("stone",7),C("wood",3)},door:true);
            Ranged("ReinforcedBow","reinforced_bow",false,true,new[]{C("bow",1),C("bronze",2),C("leather",1)},2);recipes.Find(r=>r.Result.Id=="reinforced_bow").ProgressionPerPlayer=1;
            Ammo("BronzeArrow","bronze_arrow",AmmoKind.Arrow,6,2,12,new[]{C("wood",2),C("feather",1),C("bronze",1)});
            Ranged("Crossbow","crossbow",true,false,new[]{C("wood",6),C("bronze",3),C("leather",2)},2);recipes.Find(r=>r.Result.Id=="crossbow").ProgressionPerPlayer=1;
            Ammo("CrossbowBolt","crossbow_bolt",AmmoKind.Bolt,0,2,12,new[]{C("wood",2),C("feather",1)});
            Ammo("SteelBolt","steel_bolt",AmmoKind.Bolt,10,2,8,new[]{C("wood",2),C("feather",1),C("steel",1)});recipes.Find(r=>r.Result.Id=="steel_bolt").ProgressionPerPlayer=16;
            recipes.Find(r=>r.Result.Id=="bronze_arrow").ProgressionPerPlayer=24;
            var lantern=Item("Lantern","lantern",ItemCategory.Tool,1);lantern.EquippedLight=GameplayAssets.Asset<GameplayLightDefinition>("LanternLight",d=>{d.Radius=8.5f;d.Strength=.9f;d.Color=new Color(1,.73f,.4f);d.DetersEnemies=false;});EditorUtility.SetDirty(lantern);Recipe("Lantern",lantern,1,2,RecipeCategory.Survival,new[]{C("bronze",2),C("leather",1),C("coal",2)}).ProgressionPerPlayer=1;
            Building("ReinforcedWall","reinforced_wall",2,360,2.2f,new[]{C("stone",3),C("wood",2),C("bronze",1)});
            Building("ReinforcedDoor","reinforced_door",2,320,2.1f,new[]{C("stone",3),C("wood",2),C("bronze",2)},door:true);
            Building("ReinforcedGate","reinforced_gate",2,420,3.4f,new[]{C("stone",5),C("wood",4),C("bronze",3)},door:true);
            Building("SpikeTrap","spike_trap",2,100,1.7f,new[]{C("wood",4),C("stone",2),C("bronze",1)});
            Building("Brazier","brazier",2,150,1.5f,new[]{C("bronze",3),C("stone",4),C("coal",2)});
            foreach(var id in new[]{"reinforced_wall","reinforced_door","reinforced_gate","spike_trap","brazier"})recipes.Find(r=>r.Result.Id==id).ProgressionShared=id=="reinforced_wall" ? 6 : 1;
            foreach(var recipe in recipes)EditorUtility.SetDirty(recipe);
            catalog.Items=items.ToArray();catalog.Recipes=recipes.ToArray();var recipeCatalog=D<RecipeCatalog>("Recipes");recipeCatalog.Recipes=recipes.ToArray();EditorUtility.SetDirty(catalog);EditorUtility.SetDirty(recipeCatalog);EditorUtility.SetDirty(world);AssetDatabase.SaveAssets();
            var projectile=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/Pebble.prefab").GetComponent<StoneProjectile>();if(projectile==null)throw new InvalidOperationException("Existing projectile prefab missing");
            foreach(var path in new[]{"Assets/Wildfolk/Prefabs/Player.prefab",MultiplayerSceneBuilder.PlayerPrefabPath}){var player=PrefabUtility.LoadPrefabContents(path);ConfigurePlayer(player,projectile);PrefabUtility.SaveAsPrefabAsset(player,path);PrefabUtility.UnloadPrefabContents(player);}
            RegisterNetwork();var entries=AssetDatabase.LoadAssetAtPath<GameObject>(NetworkEcologyBuilder.PrefabPath).GetComponent<NetworkCreatureWorld>().Prefabs;
            var session=PrefabUtility.LoadPrefabContents(MultiplayerSceneBuilder.SessionPrefabPath);session.GetComponent<Unity.Netcode.NetworkManager>().NetworkConfig.ProtocolVersion=NetworkProtocol.Current;PrefabUtility.SaveAsPrefabAsset(session,MultiplayerSceneBuilder.SessionPrefabPath);PrefabUtility.UnloadPrefabContents(session);
            foreach(var path in new[]{GameplaySceneBuilder.ScenePath,MultiplayerSceneBuilder.ScenePath})
            {
                var scene=EditorSceneManager.OpenScene(path);var actor=UnityEngine.Object.FindFirstObjectByType<PlayerEquipment>();if(actor!=null)ConfigurePlayer(actor.gameObject,projectile);
                var networkWorld=UnityEngine.Object.FindFirstObjectByType<NetworkCreatureWorld>();if(networkWorld!=null)networkWorld.Prefabs=entries;
                foreach(var panel in UnityEngine.Object.FindObjectsByType<CraftingPanel>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {var cards=new List<RecipeCardView>();foreach(var recipe in RecipePresentation.Sorted(panel.Owner.Catalog)){var view=Array.Find(panel.Cards,c=>c.Recipe==recipe);if(view==null){var clone=UnityEngine.Object.Instantiate(panel.Cards[0].gameObject,panel.Grids[(int)recipe.Category].transform);clone.name="Recipe";view=clone.GetComponent<RecipeCardView>();view.Recipe=recipe;view.Crafting=panel.Owner;}view.transform.SetAsLastSibling();cards.Add(view);}panel.Cards=cards.ToArray();}
                foreach(var equipment in UnityEngine.Object.FindObjectsByType<InventoryEquipment>(FindObjectsInactive.Include,FindObjectsSortMode.None))ConfigureUtilityUi(equipment);
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);
        }
        public static void RefreshBindings()
        {
            catalog=D<ContentCatalog>("ContentCatalog");items=new List<ItemDefinition>(catalog.Items);
            var projectile=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/Pebble.prefab").GetComponent<StoneProjectile>();
            foreach(var path in new[]{"Assets/Wildfolk/Prefabs/Player.prefab",MultiplayerSceneBuilder.PlayerPrefabPath}){var root=PrefabUtility.LoadPrefabContents(path);ConfigurePlayer(root,projectile);PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);}
            foreach(var path in new[]{GameplaySceneBuilder.ScenePath,MultiplayerSceneBuilder.ScenePath}){var scene=EditorSceneManager.OpenScene(path);foreach(var ui in UnityEngine.Object.FindObjectsByType<InventoryEquipment>(FindObjectsInactive.Include,FindObjectsSortMode.None))ConfigureUtilityUi(ui);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}
            AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);
        }
        private static void Ammo(string name,string id,AmmoKind kind,int damage,int tier,int count,ItemAmount[] cost)
        {var item=Item(name,id,ItemCategory.Material,100);item.Ammo=kind;item.AmmoDamage=damage;EditorUtility.SetDirty(item);Recipe(name,item,count,tier,RecipeCategory.Weapons,cost);}
        private static void ConfigurePlayer(GameObject player,StoneProjectile projectile)
        {
            var primary=player.GetComponent<PlayerPrimaryAction>()??player.AddComponent<PlayerPrimaryAction>();primary.Equipment=player.GetComponent<PlayerEquipment>();primary.Combat=player.GetComponent<PlayerCombat>();primary.Placement=player.GetComponent<PlayerPlacement>();primary.Survival=player.GetComponent<PlayerSurvival>();primary.Session=player.GetComponent<PlayerInputCommands>().Session;
            var equipment=player.GetComponent<PlayerEquipment>();if(equipment.UtilityVisual==null){var g=new GameObject("Belt Lantern");g.transform.SetParent(player.transform,false);g.transform.localPosition=new Vector3(-.48f,-.25f,0);g.transform.localScale=Vector3.one*.27f;equipment.UtilityVisual=g.AddComponent<SpriteRenderer>();equipment.UtilityVisual.sharedMaterial=equipment.HeldVisual.sharedMaterial;equipment.UtilityVisual.sortingOrder=WorldSorting.Overlay;}
            if(player.GetComponent<PlayerWaterskin>()==null)player.AddComponent<PlayerWaterskin>();
            var ranged=player.GetComponent<PlayerRanged>()??player.AddComponent<PlayerRanged>();ranged.Projectile=projectile;
            var fishing=player.GetComponent<PlayerFishing>()??player.AddComponent<PlayerFishing>();fishing.Fish=I("raw_fish");fishing.BobberSprite=VisualArtImporter.Sprite("fishing_bobber");fishing.BiteSprite=VisualArtImporter.Sprite("fishing_bite");
            if(player.GetComponent<NetworkPlayer>()!=null){if(player.GetComponent<NetworkActivities>()==null)player.AddComponent<NetworkActivities>();if(player.GetComponent<NetworkContainer>()==null)player.AddComponent<NetworkContainer>();}
        }
        private static void ConfigureUtilityUi(InventoryEquipment ui)
        {
            if(ui.UtilityButton==null)
            {
                var clone=UnityEngine.Object.Instantiate(ui.HeadButton.gameObject,ui.HeadButton.transform.parent);clone.name="Utility";
                ui.UtilityButton=clone.GetComponent<Button>();ui.UtilityButton.onClick=new Button.ButtonClickedEvent();UnityEventTools.AddIntPersistentListener(ui.UtilityButton.onClick,ui.SelectSlot,2);
                var path=AnimationUtility.CalculateTransformPath(ui.HeadIcon.transform,ui.HeadButton.transform);ui.UtilityIcon=clone.transform.Find(path).GetComponent<Image>();ui.UtilityIcon.sprite=null;
                var drag=clone.GetComponent<EquipmentSlotDrag>();if(drag!=null)drag.Slot=ArmorSlot.Utility;
            }
            var buttons=new[]{ui.HeadButton,ui.BodyButton,ui.UtilityButton};for(var i=0;i<3;i++){var rect=(RectTransform)buttons[i].transform;rect.anchoredPosition=new Vector2(-662+i*106,-112);rect.sizeDelta=new Vector2(88,100);}
            var parent=ui.HeadButton.transform.parent;var labels=parent.GetComponentsInChildren<LocalizedLabel>(true);
            foreach(var pair in new[]{new {Key="ui.slot.Head",X=-662},new {Key="ui.slot.Body",X=-556},new {Key="ui.slot.Utility",X=-450}})
            {var label=Array.Find(labels,l=>l.Key==pair.Key);var text=label==null ? GameplayUiFactory.Text("Slot Name",parent,pair.Key,new Vector2(pair.X,-186),new Vector2(100,32),18) : label.GetComponent<Text>();text.rectTransform.anchoredPosition=new Vector2(pair.X,-186);text.rectTransform.sizeDelta=new Vector2(100,32);text.fontSize=18;}
            ui.Details.rectTransform.sizeDelta=new Vector2(324,190);ui.Details.rectTransform.anchoredPosition=new Vector2(544,-30);ui.Details.alignment=TextAnchor.UpperLeft;ui.Details.fontSize=22;
        }
        private static void AddSource(WorldGenerationDefinition world,string name,string art,ItemDefinition drop,int hp,int yield,bool cave)
        {
            var rules=new List<ResourceGenerationRule>(world.Resources);var vitality=GameplayAssets.Asset<VitalityDefinition>(name+"Vitality",d=>d.MaximumHealth=hp);var def=GameplayAssets.Asset<ResourceDefinition>(name+"Resource",d=>{d.Kind=cave ? ResourceKind.Mushroom : ResourceKind.Berry;d.DropItem=drop;d.DropCount=yield;d.Vitality=vitality;d.Harvest.MinimumTier=null;});
            var prefab=WorldContentBuilder.Resource(name,art,def,Vector2.zero,.5f,1,HitReaction.Tree,AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/WorldPickup.prefab").GetComponent<WorldPickup>(),D<GameplaySettings>("GameplaySettings"),nonBlocking:true);
            var index=rules.FindIndex(x=>x.Prefab==prefab);if(index<0){index=rules.Count;rules.Add(new ResourceGenerationRule{Prefab=prefab,Count=0,IndependentGeneration=true,Generation2Only=true,GenerationSalt=cave ? 911 : 907,DistributionConfigured=true,ScaleRange=new Vector2(.9f,1.1f),Family=ResourceFamily.Plant,RecoveryConfigured=true,NaturalRecovery=true,WorldRecoveryScope=true,RespawnSeconds=cave ? 240 : 180,Spacing=4,TargetDensityRatio=1,DepthRange=new Vector2(0,1),WaterDistanceRange=new Vector2(0,1000),RoadDistanceRange=new Vector2(0,1000),ForestDensityRange=new Vector2(0,1),RockDensityRange=new Vector2(0,1)});}world.Resources=rules.ToArray();
            var list=new List<NaturalSourceDefinition>(world.NaturalSources);foreach(var underground in cave ? new[]{false,true} : new[]{false})
            {var source=GameplayAssets.Asset<NaturalSourceDefinition>(name+(underground ? "Cave" : "Surface")+"Source",s=>{s.SourceId=art+(underground ? "_cave" : "_surface");s.ResourceRule=index;s.Context=underground ? ResourceSourceContext.Cave : ResourceSourceContext.Surface;s.AnyTags=underground ? EnvironmentTag.CaveFloor : cave ? EnvironmentTag.ForestFloor : EnvironmentTag.GrassSurface;s.CandidatesPerChunk=underground ? 7 : 5;s.Probability=.6f;s.VegetationStream=true;s.NaturalRecovery=true;s.MinimumSpacing=4;s.SurfaceBiomes=underground ? Array.Empty<SurfaceBiome>() : cave ? new[]{SurfaceBiome.Forest} : new[]{SurfaceBiome.Grassland,SurfaceBiome.Forest};});if(!list.Contains(source))list.Add(source);}world.NaturalSources=list.ToArray();sources.Add(prefab.gameObject);
        }
        private static void RegisterNetwork()
        {
            var root=PrefabUtility.LoadPrefabContents(NetworkEcologyBuilder.PrefabPath);var world=root.GetComponent<NetworkCreatureWorld>();var entries=new List<NetworkSpawnEntry>(world.Prefabs);
            foreach(var source in sources)
            {var clone=(GameObject)PrefabUtility.InstantiatePrefab(source);clone.AddComponent<Unity.Netcode.NetworkObject>();if(clone.GetComponent<ResourceNode>()!=null)clone.AddComponent<NetworkResource>();else{clone.AddComponent<NetworkBuilding>();if(clone.GetComponent<CraftingStation>()!=null)clone.AddComponent<NetworkFacility>();if(clone.GetComponent<SpikeTrap>()!=null)clone.AddComponent<NetworkTrap>();}var prefab=PrefabUtility.SaveAsPrefabAsset(clone,"Assets/Wildfolk/Prefabs/Network"+source.name+".prefab");UnityEngine.Object.DestroyImmediate(clone);entries.RemoveAll(x=>x.Source==source);entries.Add(new NetworkSpawnEntry{Source=source,NetworkPrefab=prefab});}
            world.Prefabs=entries.ToArray();PrefabUtility.SaveAsPrefabAsset(root,NetworkEcologyBuilder.PrefabPath);PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
