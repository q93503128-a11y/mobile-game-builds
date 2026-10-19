using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Wildfolk.Editor
{
    public static class TierThreeAssets
    {
        private static ContentCatalog catalog;private static List<ItemDefinition> items;private static List<RecipeDefinition> recipes;
        private static readonly List<GameObject> sources=new List<GameObject>();
        private static T D<T>(string name)where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        private static ItemDefinition I(string id)=>items.Find(i=>i.Id==id)??throw new InvalidOperationException(id);
        private static ItemAmount C(string id,int n)=>new ItemAmount{Item=I(id),Count=n};
        private static T Clone<T>(T source,string name)where T:UnityEngine.Object
        {var path="Assets/Wildfolk/Data/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old!=null)return old;var next=UnityEngine.Object.Instantiate(source);next.name=name;AssetDatabase.CreateAsset(next,path);return next;}
        private static ItemDefinition Item(string name,string id,ItemCategory category=ItemCategory.Material,int count=50)
        {var item=GameplayAssets.Asset<ItemDefinition>(name,i=>{i.Id=id;i.DisplayName=name;i.MaxStack=count;i.Category=category;});item.Icon=VisualArtImporter.Sprite(id);if(!items.Contains(item))items.Add(item);EditorUtility.SetDirty(item);return item;}
        private static RecipeDefinition Recipe(string name,ItemDefinition result,int count,int tier,RecipeCategory category,ItemAmount[] cost,StationKind station=StationKind.Workbench)
        {var recipe=GameplayAssets.Asset<RecipeDefinition>(name+"Recipe",r=>{r.Id=result.Id;r.Result=result;r.ResultCount=count;r.RequiredWorkbenchTier=tier;r.RequiredStation=station;r.Category=category;r.Ingredients=cost;r.Tier=result.Tier;r.SortOrder=500+recipes.Count;});if(!recipes.Contains(recipe))recipes.Add(recipe);return recipe;}
        private static ItemDefinition Equipment(string name,string id,string previous,int diamond,bool armor=false)
        {
            var old=I(previous);var item=Item(name,id,old.Category,1);var tier=D<EquipmentTier>("DiamondTier");
            if(armor){item.Armor=Clone(old.Armor,name+"Armor");item.Armor.Tier=tier;item.Armor.Defense=old.Armor.Defense+1;item.Armor.WornSprite=VisualArtImporter.Sprite(id+"_worn");if(item.Armor.HandSprite!=null)item.Armor.HandSprite=VisualArtImporter.Sprite("diamond_hand_worn");EditorUtility.SetDirty(item.Armor);}
            else{item.Tool=Clone(old.Tool,name+"Tool");item.Tool.Tier=tier;item.Tool.HeldSprite=item.Icon;item.Tool.Damage=old.Tool.Kind==ToolKind.Axe || old.Tool.Kind==ToolKind.Pickaxe ? old.Tool.Damage : Mathf.RoundToInt(old.Tool.Damage*1.2f);var efficiencies=new List<ResourceEfficiency>();foreach(var e in old.Tool.Efficiencies)efficiencies.Add(new ResourceEfficiency{Kind=e.Kind,Multiplier=e.Multiplier>1 ? e.Multiplier*1.22f : e.Multiplier});item.Tool.Efficiencies=efficiencies.ToArray();EditorUtility.SetDirty(item.Tool);}
            var r=Recipe(name,item,1,3,armor ? RecipeCategory.Armor : item.Tool.Kind==ToolKind.Axe || item.Tool.Kind==ToolKind.Pickaxe ? RecipeCategory.Tools : RecipeCategory.Weapons,new[]{C(previous,1),C("diamond",diamond)});r.EssentialPerPlayer=1;var oldRecipe=recipes.Find(x=>x.Result==old);if(oldRecipe!=null){oldRecipe.SupersededBy=r;EditorUtility.SetDirty(oldRecipe);}EditorUtility.SetDirty(r);EditorUtility.SetDirty(item);return item;
        }
        private static ItemDefinition Building(string name,string id,int hp,string template,StationKind station=StationKind.None)
        {
            var item=Item(name,id,ItemCategory.Placeable,10);var old=I(template).Placement;var place=Clone(old,name+"Placement");place.Item=item;place.Vitality=GameplayAssets.Asset<VitalityDefinition>(name+"Vitality",v=>v.MaximumHealth=hp);place.Station=station;place.StorageSlots=station==StationKind.None ? 0 : old.StorageSlots;place.IsDoor=place.IsBarrier=false;place.UpgradeTo=null;place.UpgradeCosts=Array.Empty<ItemAmount>();place.TierDisplayKey="item."+id+".name";place.TierVisual=item.Icon;item.Placement=place;
            var path="Assets/Wildfolk/Prefabs/"+name+".prefab";if(AssetDatabase.LoadAssetAtPath<GameObject>(path)==null)AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(old.Prefab),path);
            var root=PrefabUtility.LoadPrefabContents(path);var placed=root.GetComponent<WorldPlaceable>();placed.Definition=place;placed.Health.Definition=place.Vitality;placed.Visual.sprite=item.Icon;placed.Visual.transform.localScale=Vector3.one*(2.2f/item.Icon.bounds.size.x);var crafting=root.GetComponent<CraftingStation>();if(crafting!=null)crafting.Definition=place;
            if(station==StationKind.None && crafting!=null)UnityEngine.Object.DestroyImmediate(crafting);
            var processor=root.GetComponent<StationProcessing>();if(processor!=null)UnityEngine.Object.DestroyImmediate(processor);
            place.Prefab=PrefabUtility.SaveAsPrefabAsset(root,path).GetComponent<WorldPlaceable>();PrefabUtility.UnloadPrefabContents(root);EditorUtility.SetDirty(item);EditorUtility.SetDirty(place);sources.Add(place.Prefab.gameObject);return item;
        }
        private static void Turret(string name,string id,bool repeater,ItemAmount[] cost)
        {
            var item=Building(name,id,repeater ? 160 : 180,"workbench");var place=item.Placement;var def=GameplayAssets.Asset<TurretDefinition>(name+"Turret",t=>{t.Ammunition=I(repeater ? "crossbow_bolt" : "ballista_bolt");t.Damage=repeater ? 16 : 62;t.Range=repeater ? 17 : 26;t.ReloadSeconds=repeater ? .55f : 2.3f;t.RotationSpeed=repeater ? 105 : 45;t.ProjectileSpeed=repeater ? 25 : 30;});def.Projectile=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/Pebble.prefab").GetComponent<StoneProjectile>();def.HeadSprite=item.Icon;place.Turret=def;place.Footprint=new Vector2(2.3f,1.8f);
            var path=AssetDatabase.GetAssetPath(place.Prefab);var root=PrefabUtility.LoadPrefabContents(path);var p=root.GetComponent<WorldPlaceable>();p.Visual.sprite=VisualArtImporter.Sprite("turret_base");var turret=root.GetComponent<WorldTurret>()??root.AddComponent<WorldTurret>();turret.Definition=def;if(turret.Head==null){var head=new GameObject("Aim assembly");head.transform.SetParent(p.Visual.transform.parent,false);var sr=head.AddComponent<SpriteRenderer>();sr.sprite=item.Icon;sr.sharedMaterial=p.Visual.sharedMaterial;sr.sortingOrder=p.Visual.sortingOrder+1;head.transform.localScale=Vector3.one*1.5f;turret.Head=head.transform;}
            PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);Recipe(name,item,1,3,RecipeCategory.Building,cost,StationKind.TurretWorkbench);EditorUtility.SetDirty(place);EditorUtility.SetDirty(def);
        }
        private static void Module(string name,string id,TurretModuleSlot slot,ItemAmount[] cost,float multiplier=1,int extra=0)
        {var item=Item(name,id,ItemCategory.Tool,1);item.TurretModule=GameplayAssets.Asset<TurretModuleDefinition>(name+"Module",d=>{d.Slot=slot;d.Multiplier=multiplier;d.ExtraAmmoSlots=extra;});Recipe(name,item,1,3,RecipeCategory.Tools,cost,StationKind.TurretWorkbench);EditorUtility.SetDirty(item);}
        public static void Build()
        {
            sources.Clear();VisualArtImporter.Import();WorldArt.SpriteMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Wildfolk/Art/WorldSprite.mat");catalog=D<ContentCatalog>("ContentCatalog");items=new List<ItemDefinition>(catalog.Items);recipes=new List<RecipeDefinition>(catalog.Recipes);
            Item("Diamond","diamond");Item("Crystal","crystal");Item("MechanicalPart","mechanical_part");
            var tier=GameplayAssets.Asset<EquipmentTier>("DiamondTier",t=>{t.Id="diamond";t.DisplayName="Diamond";t.Rank=5;});
            // Spear already has an Iron tier; Steel fills its prerequisite inside the same weapon family.
            var spear=Item("SteelSpear","steel_spear",ItemCategory.Weapon,1);spear.Tool=Clone(I("iron_spear").Tool,"SteelSpearTool");spear.Tool.Tier=D<EquipmentTier>("SteelTier");spear.Tool.Damage=35;spear.Tool.HeldSprite=spear.Icon;EditorUtility.SetDirty(spear.Tool);Recipe("SteelSpear",spear,1,2,RecipeCategory.Weapons,new[]{C("iron_spear",1),C("steel",3),C("leather",1)});
            Equipment("DiamondPickaxe","diamond_pickaxe","steel_pickaxe",2);Equipment("DiamondAxe","diamond_axe","steel_axe",1);Equipment("DiamondSword","diamond_sword","steel_sword",2);Equipment("DiamondSpear","diamond_spear","steel_spear",2);Equipment("DiamondHead","diamond_head","steel_head",2,true);Equipment("DiamondBody","diamond_body","steel_body",3,true);
            Recipe("MechanicalPart",I("mechanical_part"),2,3,RecipeCategory.Facilities,new[]{C("bronze",1),C("steel",1)});
            var scope=Item("Telescope","telescope",ItemCategory.Tool,1);scope.Telescope=true;var sr=Recipe("Telescope",scope,1,3,RecipeCategory.Tools,new[]{C("crystal",2),C("bronze",1),C("leather",1)});sr.EssentialPerPlayer=1;EditorUtility.SetDirty(sr);EditorUtility.SetDirty(scope);
            var lantern=Item("CrystalLantern","crystal_lantern",ItemCategory.Tool,1);lantern.EquippedLight=Clone(I("lantern").EquippedLight,"CrystalLanternLight");lantern.EquippedLight.Radius=I("lantern").EquippedLight.Radius*1.35f;lantern.EquippedLight.Color=new Color(.79f,.86f,.77f);EditorUtility.SetDirty(lantern.EquippedLight);EditorUtility.SetDirty(lantern);var lr=Recipe("CrystalLantern",lantern,1,3,RecipeCategory.Survival,new[]{C("lantern",1),C("crystal",2),C("bronze",1)});lr.EssentialPerPlayer=1;recipes.Find(r=>r.Result==I("lantern")).SupersededBy=lr;EditorUtility.SetDirty(lr);
            var wb=Building("WorkbenchIII","workbench_iii",120,"workbench_ii",StationKind.Workbench);wb.Placement.WorkbenchTier=3;wb.Placement.TierDisplayKey="ui.workbench.III";var two=I("workbench_ii").Placement;two.UpgradeTo=wb.Placement;two.UpgradeCosts=new[]{C("wood",10),C("stone",10),C("bronze",4),C("steel",4),C("crystal",4),C("diamond",1)};EditorUtility.SetDirty(two);
            var tw=Building("TurretWorkbench","turret_workbench",140,"workbench",StationKind.TurretWorkbench);var tr=Recipe("TurretWorkbench",tw,1,3,RecipeCategory.Facilities,new[]{C("wood",8),C("stone",8),C("bronze",4),C("steel",4),C("crystal",2),C("mechanical_part",2)});tr.EssentialShared=1;EditorUtility.SetDirty(tr);
            var beacon=Building("SignalBeacon","signal_beacon",80,"workbench");beacon.Placement.IsBeacon=true;var bp=AssetDatabase.GetAssetPath(beacon.Placement.Prefab);var br=PrefabUtility.LoadPrefabContents(bp);if(br.GetComponent<SignalBeacon>()==null)br.AddComponent<SignalBeacon>();PrefabUtility.SaveAsPrefabAsset(br,bp);PrefabUtility.UnloadPrefabContents(br);Recipe("SignalBeacon",beacon,1,3,RecipeCategory.Survival,new[]{C("crystal",2),C("bronze",1),C("mechanical_part",1),C("wood",2)});
            var bolt=Item("BallistaBolt","ballista_bolt",ItemCategory.Material,100);Recipe("BallistaBolt",bolt,6,3,RecipeCategory.Weapons,new[]{C("wood",2),C("steel",1)},StationKind.TurretWorkbench);
            Turret("Ballista","ballista",false,new[]{C("wood",10),C("steel",3),C("leather",2),C("mechanical_part",1)});Turret("Repeater","repeater",true,new[]{C("wood",8),C("bronze",3),C("steel",2),C("mechanical_part",2)});
            Module("AutoLoader","auto_loader",TurretModuleSlot.Loader,new[]{C("mechanical_part",2),C("steel",1),C("bronze",1)});Module("AutoAim","auto_aim",TurretModuleSlot.Aim,new[]{C("mechanical_part",1),C("crystal",2),C("bronze",1)});Module("AmmoBox","ammo_box",TurretModuleSlot.Ammo,new[]{C("wood",4),C("steel",1)},extra:2);Module("RotationGear","rotation_gear",TurretModuleSlot.Rotation,new[]{C("mechanical_part",1),C("bronze",2)},1.35f);Module("ReinforcedFrame","reinforced_frame",TurretModuleSlot.Frame,new[]{C("steel",2),C("leather",2)},1.5f);
            var container=Building("IndustrialContainer","industrial_container",100,"storage_chest");container.Placement.StorageSlots=6;var cp=AssetDatabase.GetAssetPath(container.Placement.Prefab);var cr=PrefabUtility.LoadPrefabContents(cp);cr.GetComponent<StorageChest>().Placeable=cr.GetComponent<WorldPlaceable>();PrefabUtility.SaveAsPrefabAsset(cr,cp);PrefabUtility.UnloadPrefabContents(cr);EditorUtility.SetDirty(container.Placement);
            ConfigureBudgets();AddSources();
            catalog.Items=items.ToArray();catalog.Recipes=recipes.ToArray();D<RecipeCatalog>("Recipes").Recipes=recipes.ToArray();EditorUtility.SetDirty(catalog);EditorUtility.SetDirty(D<RecipeCatalog>("Recipes"));foreach(var i in items)EditorUtility.SetDirty(i);foreach(var r in recipes)EditorUtility.SetDirty(r);RegisterNetwork();ConfigurePlayersAndScenes();AssetDatabase.SaveAssets();
        }
        private static void ConfigureBudgets()
        {
            foreach(var pair in new[]{new {Id="signal_beacon",Count=1},new {Id="ballista",Count=1},new {Id="repeater",Count=1},new {Id="auto_loader",Count=2},new {Id="auto_aim",Count=2},new {Id="ammo_box",Count=2},new {Id="rotation_gear",Count=2},new {Id="reinforced_frame",Count=2},new {Id="ballista_bolt",Count=60}})recipes.Find(r=>r.Id==pair.Id).ProgressionShared=pair.Count;
            recipes.Find(r=>r.Id=="crossbow_bolt").ProgressionShared=120;
        }
        private static void AddSources()
        {
            var world=D<WorldGenerationDefinition>("WorldGeneration");var rules=new List<ResourceGenerationRule>(world.Resources);var natural=new List<NaturalSourceDefinition>(world.NaturalSources);natural.RemoveAll(x=>x.SourceId=="diamond_pocket");AssetDatabase.DeleteAsset("Assets/Wildfolk/Data/DiamondPocketSource.asset");
            foreach(var id in new[]{"diamond","crystal"})
            {
                var name=id=="diamond" ? "DiamondNode" : "CrystalNode";var gem=id=="diamond";var vitality=GameplayAssets.Asset<VitalityDefinition>(name+"Vitality",v=>v.MaximumHealth=gem ? 160 : 130);var def=GameplayAssets.Asset<ResourceDefinition>(name+"Resource",d=>{d.Kind=gem ? ResourceKind.Diamond : ResourceKind.Crystal;d.DropItem=I(id);d.DropCount=gem ? 3 : 6;d.Vitality=vitality;d.MinimumTier=D<EquipmentTier>("SteelTier");d.RequiredTool=ToolKind.Pickaxe;d.RequiredToolItem=I("steel_pickaxe");d.Harvest.Mode=HarvestMode.Mine;});
                var node=WorldContentBuilder.Resource(name,id+"_node",def,Vector2.zero,.65f,1.5f,HitReaction.Stone,AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/WorldPickup.prefab").GetComponent<WorldPickup>(),D<GameplaySettings>("GameplaySettings"));sources.Add(node.gameObject);var index=rules.FindIndex(r=>r.Prefab==node);if(index<0){index=rules.Count;rules.Add(new ResourceGenerationRule{Prefab=node,IndependentGeneration=true,Generation2Only=true,DistributionConfigured=true,Family=ResourceFamily.Mineral,Rarity=gem ? ResourceRarity.Rare : ResourceRarity.Uncommon,ScaleRange=new Vector2(.9f,1.1f),NaturalRecovery=true,RecoveryConfigured=true,TargetDensityRatio=gem ? .7f : .85f,RespawnSeconds=gem ? 300 : 150,HarvestExclusionRadius=4,HarvestExclusionSeconds=gem ? 300 : 150,ClusterSpacing=5,Spacing=4,DepthRange=new Vector2(.6f,1)});}
                // Deep rooms cannot accommodate a five-unit clear disc among the existing minerals.
                // Keep authored initial spacing separate from the two-unit recovery clearance.
                var recoveryRule=rules[index];recoveryRule.Spacing=2;recoveryRule.ClusterSpacing=2;rules[index]=recoveryRule;
                var source=GameplayAssets.Asset<NaturalSourceDefinition>(name+"Source",d=>{d.SourceId=id+"_deep";d.ResourceRule=index;d.MinimumGenerationVersion=3;d.Context=ResourceSourceContext.Cave;d.AnyTags=EnvironmentTag.DeepCave;d.DepthRange=new Vector2(gem ? .75f : .6f,1);d.CandidatesPerChunk=gem ? 8 : 14;d.Probability=gem ? .45f : .75f;d.MinimumSpacing=4;d.NaturalRecovery=true;});if(!natural.Contains(source))natural.Add(source);
            }
            world.Resources=rules.ToArray();world.NaturalSources=natural.ToArray();EditorUtility.SetDirty(world);
            foreach(var item in items)if(item.Tool!=null && item.Tool.Kind==ToolKind.Pickaxe && item.Tool.Tier.Rank>=4){var eff=new List<ResourceEfficiency>(item.Tool.Efficiencies);foreach(var kind in new[]{ResourceKind.Diamond,ResourceKind.Crystal})if(!eff.Exists(x=>x.Kind==kind))eff.Add(new ResourceEfficiency{Kind=kind,Multiplier=item.Tool.Tier.Rank>=5 ? 4.27f : 3.5f});item.Tool.Efficiencies=eff.ToArray();EditorUtility.SetDirty(item.Tool);}
        }
        private static void RegisterNetwork()
        {
            var root=PrefabUtility.LoadPrefabContents(NetworkEcologyBuilder.PrefabPath);var world=root.GetComponent<NetworkCreatureWorld>();var list=new List<NetworkSpawnEntry>(world.Prefabs);
            foreach(var source in sources){var clone=(GameObject)PrefabUtility.InstantiatePrefab(source);clone.AddComponent<Unity.Netcode.NetworkObject>();if(clone.GetComponent<ResourceNode>()!=null)clone.AddComponent<NetworkResource>();else{clone.AddComponent<NetworkBuilding>();if(clone.GetComponent<CraftingStation>()!=null || clone.GetComponent<StorageChest>()!=null)clone.AddComponent<NetworkFacility>();if(clone.GetComponent<WorldTurret>()!=null)clone.AddComponent<NetworkTurret>();}var prefab=PrefabUtility.SaveAsPrefabAsset(clone,"Assets/Wildfolk/Prefabs/Network"+source.name+".prefab");UnityEngine.Object.DestroyImmediate(clone);list.RemoveAll(e=>e.Source==source);list.Add(new NetworkSpawnEntry{Source=source,NetworkPrefab=prefab});}
            world.Prefabs=list.ToArray();PrefabUtility.SaveAsPrefabAsset(root,NetworkEcologyBuilder.PrefabPath);PrefabUtility.UnloadPrefabContents(root);
            var session=PrefabUtility.LoadPrefabContents(MultiplayerSceneBuilder.SessionPrefabPath);session.GetComponent<Unity.Netcode.NetworkManager>().NetworkConfig.ProtocolVersion=NetworkProtocol.Current;PrefabUtility.SaveAsPrefabAsset(session,MultiplayerSceneBuilder.SessionPrefabPath);PrefabUtility.UnloadPrefabContents(session);
        }
        private static void ConfigurePlayersAndScenes()
        {
            foreach(var path in new[]{"Assets/Wildfolk/Prefabs/Player.prefab",MultiplayerSceneBuilder.PlayerPrefabPath}){var root=PrefabUtility.LoadPrefabContents(path);ConfigurePlayer(root);PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);}
            foreach(var path in new[]{GameplaySceneBuilder.ScenePath,MultiplayerSceneBuilder.ScenePath}){var scene=EditorSceneManager.OpenScene(path);var actor=UnityEngine.Object.FindFirstObjectByType<PlayerInventory>();if(actor!=null)ConfigurePlayer(actor.gameObject);var world=UnityEngine.Object.FindFirstObjectByType<WorldGenerator>();if(world!=null){world.DeepPoiSprites=new[]{VisualArtImporter.Sprite("crystal_pocket"),VisualArtImporter.Sprite("diamond_pocket"),VisualArtImporter.Sprite("deep_mine"),VisualArtImporter.Sprite("abandoned_workshop")};}foreach(var ui in UnityEngine.Object.FindObjectsByType<CraftingPanel>(FindObjectsInactive.Include,FindObjectsSortMode.None))ConfigureCards(ui);var hud=UnityEngine.Object.FindFirstObjectByType<TierThreeHud>();if(hud==null)new GameObject("Exploration and turret controls").AddComponent<TierThreeHud>();EditorSceneManager.SaveScene(scene);}
            // Multiplayer creates WorldGenerator from its ecology prefab.
            var e=PrefabUtility.LoadPrefabContents(NetworkEcologyBuilder.PrefabPath);var w=e.GetComponent<NetworkCreatureWorld>().World;w.DeepPoiSprites=new[]{VisualArtImporter.Sprite("crystal_pocket"),VisualArtImporter.Sprite("diamond_pocket"),VisualArtImporter.Sprite("deep_mine"),VisualArtImporter.Sprite("abandoned_workshop")};PrefabUtility.SaveAsPrefabAsset(e,NetworkEcologyBuilder.PrefabPath);PrefabUtility.UnloadPrefabContents(e);EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);
        }
        public static void RefreshPresentation(){ConfigurePlayersAndScenes();AssetDatabase.SaveAssets();}
        private static void ConfigureCards(CraftingPanel panel)
        {
            var sorted=RecipePresentation.Sorted(panel.Owner.Catalog);var cards=new List<RecipeCardView>(panel.Cards);var result=new List<RecipeCardView>();
            foreach(var recipe in sorted){var card=cards.Find(c=>c.Recipe==recipe);if(card==null){var template=cards.Find(c=>c.Recipe.Category==recipe.Category)??cards[0];var copy=UnityEngine.Object.Instantiate(template.gameObject,panel.Grids[(int)recipe.Category].transform);copy.name="Recipe "+recipe.Id;card=copy.GetComponent<RecipeCardView>();card.Recipe=recipe;card.Crafting=panel.Owner;cards.Add(card);}card.transform.SetAsLastSibling();result.Add(card);}panel.Cards=result.ToArray();EditorUtility.SetDirty(panel);
        }
        private static void ConfigurePlayer(GameObject root){if(root.GetComponent<PlayerTelescope>()==null)root.AddComponent<PlayerTelescope>();if(root.GetComponent<NetworkPlayer>()!=null && root.GetComponent<NetworkTierThreePlayer>()==null)root.AddComponent<NetworkTierThreePlayer>();}
    }
}
