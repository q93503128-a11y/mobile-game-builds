using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Wildfolk.Editor
{
    // Idempotent authoring pass. Existing definitions, IDs and scene art remain authoritative.
    public static class MetalProgressionAssets
    {
        private static T Data<T>(string name)where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        private static ItemAmount Cost(ItemDefinition item,int count)=>new ItemAmount{Item=item,Count=count};
        private static ItemDefinition Item(string name,string id)=>GameplayAssets.Asset<ItemDefinition>(name,i=>{i.Id=id;i.DisplayName=id;i.Category=ItemCategory.Material;i.MaxStack=50;i.Icon=VisualArtImporter.Sprite(id);});
        private static RecipeDefinition Recipe(string name,ItemDefinition result,int amount,int tier,ItemAmount[] costs,float seconds=0,string display=null)
        {
            return GameplayAssets.Asset<RecipeDefinition>(name+"Recipe",r=>{r.Id=display==null ? result.Id : display.Substring("recipe.".Length);r.Result=result;r.ResultCount=amount;r.Ingredients=costs;r.RequiredStation=seconds>0 ? StationKind.Furnace : StationKind.Workbench;r.RequiredWorkbenchTier=tier;r.ProcessingSeconds=seconds;r.Processing=seconds>0 ? ProcessingKind.Smelting : ProcessingKind.None;r.Category=seconds>0 ? RecipeCategory.Survival : result.Placement.IsBarrier ? RecipeCategory.Building : RecipeCategory.Facilities;r.SortOrder=200;r.DisplayKey=display;});
        }
        public static void BuildBatch()
        {
            VisualArtImporter.Import();WorldArt.SpriteMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Wildfolk/Art/WorldSprite.mat");
            var world=Data<WorldGenerationDefinition>("WorldGeneration");var catalog=Data<ContentCatalog>("ContentCatalog");var recipes=Data<RecipeCatalog>("Recipes");
            var copper=Item("CopperOre","copper_ore");var tin=Item("TinOre","tin_ore");var coal=Item("Coal","coal");var copperBar=Item("CopperIngot","copper_ingot");var tinBar=Item("TinIngot","tin_ingot");var bronze=Item("Bronze","bronze");var steel=Item("Steel","steel");
            var items=new List<ItemDefinition>(catalog.Items);foreach(var item in new[]{copper,tin,coal,copperBar,tinBar,bronze,steel})if(!items.Contains(item))items.Add(item);catalog.Items=items.ToArray();
            var list=new List<RecipeDefinition>(recipes.Recipes);var contentRecipes=new List<RecipeDefinition>(catalog.Recipes);
            foreach(var r in new[]{Recipe("CopperIngot",copperBar,1,1,new[]{Cost(copper,1),Cost(coal,1)},3),Recipe("TinIngot",tinBar,1,1,new[]{Cost(tin,1),Cost(coal,1)},3),Recipe("Bronze",bronze,3,1,new[]{Cost(copperBar,2),Cost(tinBar,1)},5),Recipe("Steel",steel,1,2,new[]{Cost(Data<ItemDefinition>("IronIngot"),1),Cost(coal,1)},6),Recipe("BronzeChest",Data<ItemDefinition>("ReinforcedChest"),1,2,new[]{Cost(Data<ItemDefinition>("Wood"),6),Cost(bronze,2)},display:"recipe.bronze_chest"),Recipe("SteelWall",Data<ItemDefinition>("StoneWall"),2,2,new[]{Cost(Data<ItemDefinition>("Stone"),4),Cost(steel,1)},display:"recipe.steel_wall")}){if(!list.Contains(r))list.Add(r);if(!contentRecipes.Contains(r))contentRecipes.Add(r);}
            recipes.Recipes=list.ToArray();catalog.Recipes=contentRecipes.ToArray();EditorUtility.SetDirty(catalog);EditorUtility.SetDirty(recipes);
            var rules=new List<ResourceGenerationRule>(world.Resources);
            AddResource(rules,"CopperOreNode","copper_ore_node",copper,ResourceKind.CopperOre,"Pickaxe",90,4,14,18,701,ResourceTerrain.Land,.38f,600,2,11);
            AddResource(rules,"TinOreNode","tin_ore_node",tin,ResourceKind.TinOre,"StonePickaxe",75,3,10,18,709,ResourceTerrain.Shore,0,720,1,0);
            AddResource(rules,"CoalNode","coal_node",coal,ResourceKind.Coal,"Pickaxe",90,4,12,22,719,ResourceTerrain.Land,.48f,600,3,14);
            world.Resources=rules.ToArray();EditorUtility.SetDirty(world);
            foreach(var rule in world.Resources)
            {
                var resource=rule.Prefab.Definition;
                if(resource.MinimumTier==null)continue;
                foreach(var item in catalog.Items)if(item.Tool!=null && item.Tool.Kind==resource.RequiredTool && item.Tool.Tier==resource.MinimumTier){resource.RequiredToolItem=item;EditorUtility.SetDirty(resource);break;}
            }
            // Add only the three new registered variants. Preserve old NGO hashes/scene identities.
            var root=PrefabUtility.LoadPrefabContents(NetworkEcologyBuilder.PrefabPath);var ecology=root.GetComponent<NetworkCreatureWorld>();var entries=new List<NetworkSpawnEntry>(ecology.Prefabs);
            foreach(var name in new[]{"CopperOreNode","TinOreNode","CoalNode"})
            {
                var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/"+name+".prefab");if(entries.Exists(e=>e.Source==source))continue;
                var go=(GameObject)PrefabUtility.InstantiatePrefab(source);go.AddComponent<Unity.Netcode.NetworkObject>();go.AddComponent<NetworkResource>();
                var prefab=PrefabUtility.SaveAsPrefabAsset(go,"Assets/Wildfolk/Prefabs/Network"+name+".prefab");UnityEngine.Object.DestroyImmediate(go);entries.Add(new NetworkSpawnEntry{Source=source,NetworkPrefab=prefab});
            }
            ecology.Prefabs=entries.ToArray();PrefabUtility.SaveAsPrefabAsset(root,NetworkEcologyBuilder.PrefabPath);PrefabUtility.UnloadPrefabContents(root);
            var session=PrefabUtility.LoadPrefabContents(MultiplayerSceneBuilder.SessionPrefabPath);session.GetComponent<Unity.Netcode.NetworkManager>().NetworkConfig.ProtocolVersion=6;PrefabUtility.SaveAsPrefabAsset(session,MultiplayerSceneBuilder.SessionPrefabPath);PrefabUtility.UnloadPrefabContents(session);
            foreach(var path in new[]{GameplaySceneBuilder.ScenePath,MultiplayerSceneBuilder.ScenePath})
            {
                EditorSceneManager.OpenScene(path);var panel=UnityEngine.Object.FindFirstObjectByType<CraftingPanel>(FindObjectsInactive.Include);var owner=panel.Owner;var ui=panel.gameObject;var safe=panel.View.transform.parent;
                UnityEngine.Object.DestroyImmediate(panel.View);UnityEngine.Object.DestroyImmediate(panel);CraftingUiBuilder.Build(ui,safe,owner);
                var service=UnityEngine.Object.FindFirstObjectByType<NetworkCreatureWorld>();if(service!=null){service.Prefabs=entries.ToArray();EditorUtility.SetDirty(service);}
                EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }
            AssetDatabase.SaveAssets();ProjectDataValidator.Run();
        }
        private static void AddResource(List<ResourceGenerationRule> rules,string name,string art,ItemDefinition drop,ResourceKind kind,string toolName,int hp,int count,int population,float clearance,int salt,ResourceTerrain terrain,float rock,float recovery,int clusterSize,float clusterRadius)
        {
            var tool=Data<ItemDefinition>(toolName).Tool;
            var vitality=GameplayAssets.Asset<VitalityDefinition>(name+"Vitality",v=>v.MaximumHealth=hp);
            var resource=GameplayAssets.Asset<ResourceDefinition>(name+"Resource",d=>{d.Kind=kind;d.RequiredTool=ToolKind.Pickaxe;d.MinimumTier=tool.Tier;d.RequiredToolItem=Data<ItemDefinition>(toolName);d.Vitality=vitality;d.DropItem=drop;d.DropCount=count;});
            var prefab=WorldContentBuilder.Resource(name,art,resource,Vector2.zero,.7f,1.15f,HitReaction.Stone,AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/WorldPickup.prefab").GetComponent<WorldPickup>(),Data<GameplaySettings>("GameplaySettings"));
            if(rules.Exists(r=>r.Prefab==prefab))return;
            rules.Add(new ResourceGenerationRule{Prefab=prefab,Count=population,StartRadius=8,SpawnClearance=clearance,Spacing=3.4f,ScaleRange=new Vector2(.9f,1.1f),RockPreference=.8f,ForestPreference=.1f,RecoveryConfigured=true,NaturalRecovery=true,WorldRecoveryScope=true,Family=ResourceFamily.Mineral,Rarity=ResourceRarity.Uncommon,TargetDensityRatio=.9f,RespawnSeconds=recovery,HarvestExclusionRadius=18,HarvestExclusionSeconds=1200,ClusterSpacing=9,IndependentGeneration=true,GenerationSalt=salt,DistributionConfigured=true,AllowedBiomes=new[]{Data<BiomeDefinition>("TemperateBiome")},BiomeAffinity=1,SourceContext=ResourceSourceContext.Surface,Terrain=terrain,DepthRange=Vector2.zero,WaterDistanceRange=new Vector2(1.4f,terrain==ResourceTerrain.Shore ? 12 : 1000),RoadDistanceRange=new Vector2(2.2f,1000),ForestDensityRange=new Vector2(0,1),RockDensityRange=new Vector2(rock,1),ClusterSize=clusterSize,ClusterRadius=clusterRadius});
        }
    }
}
