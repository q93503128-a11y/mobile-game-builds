using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    // Authored post-Iron technology and equipment, invoked by the canonical metal builder.
    internal static class SteelProgressionAssets
    {
        private static T Data<T>(string name)where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        private static ItemAmount Cost(string name,int count)=>new ItemAmount{Item=Data<ItemDefinition>(name),Count=count};
        private static ItemDefinition Item(string name,string id,ItemCategory category,int stack=1)
        {var i=GameplayAssets.Asset<ItemDefinition>(name,d=>{d.Id=id;d.DisplayName=name;d.Category=category;d.MaxStack=stack;});i.Icon=VisualArtImporter.Sprite(id);EditorUtility.SetDirty(i);return i;}
        private static RecipeDefinition Recipe(string name,ItemDefinition result,ItemAmount[] costs)
        {var r=GameplayAssets.Asset<RecipeDefinition>(name+"Recipe",d=>{});r.Id=result.Id;r.Result=result;r.ResultCount=1;r.Ingredients=costs;r.RequiredStation=StationKind.Workbench;r.RequiredWorkbenchTier=2;r.Tier=result.Tier;r.SortOrder=200;r.Category=result.Armor!=null ? RecipeCategory.Armor : result.Tool!=null ? result.Tool.Kind==ToolKind.Sword ? RecipeCategory.Weapons : RecipeCategory.Tools : RecipeCategory.Facilities;EditorUtility.SetDirty(r);return r;}
        private static void Add<T>(List<T> list,T value){if(!list.Contains(value))list.Add(value);}
        internal static void Configure()
        {
            var catalog=Data<ContentCatalog>("ContentCatalog");var recipes=Data<RecipeCatalog>("Recipes");var items=new List<ItemDefinition>(catalog.Items);var list=new List<RecipeDefinition>(recipes.Recipes);var content=new List<RecipeDefinition>(catalog.Recipes);
            list.RemoveAll(r=>r.Id=="bronze_chest" || r.Id=="steel_wall");content.RemoveAll(r=>r.Id=="bronze_chest" || r.Id=="steel_wall");foreach(var name in new[]{"BronzeChestRecipe","SteelWallRecipe"})AssetDatabase.DeleteAsset("Assets/Wildfolk/Data/"+name+".asset");
            var steel=GameplayAssets.Asset<EquipmentTier>("SteelTier",d=>{d.Id="steel";d.DisplayName="Steel";d.Rank=4;});
            var pan=Item("ProspectingPan","prospecting_pan",ItemCategory.Tool);var tool=GameplayAssets.Asset<ToolDefinition>("ProspectingPanTool",d=>{});tool.Kind=ToolKind.Pan;tool.HarvestMethod=HarvestMethod.Panning;tool.Tier=Data<EquipmentTier>("IronTier");tool.HeldSprite=pan.Icon;tool.Damage=25;tool.Duration=1.1f;tool.Cooldown=1.4f;tool.Attack.Range=1.8f;tool.Attack.ArcAngle=180;tool.Efficiencies=Array.Empty<ResourceEfficiency>();pan.Tool=tool;EditorUtility.SetDirty(tool);EditorUtility.SetDirty(pan);Add(items,pan);var added=new List<RecipeDefinition>{Recipe("ProspectingPan",pan,new[]{Cost("CopperIngot",2),Cost("Wood",2)})};
            var furnace=GameplayAssets.Asset<PlaceableDefinition>("BlastFurnacePlacement",d=>{});furnace.Station=StationKind.BlastFurnace;furnace.Tier=steel;furnace.Vitality=GameplayAssets.Asset<VitalityDefinition>("BlastFurnaceVitality",v=>v.MaximumHealth=140);furnace.Footprint=new Vector2(2.1f,1.8f);furnace.MaximumDistance=2.8f;furnace.ProcessingMode=ProcessingMode.Sequential;furnace.ProcessingSpeed=1;furnace.InputCapacity=50;furnace.OutputCapacity=100;
            var item=Item("BlastFurnace","blast_furnace",ItemCategory.Placeable,10);item.Placement=furnace;item.DisplayName="Blast Furnace";furnace.Item=item;EditorUtility.SetDirty(item);EditorUtility.SetDirty(furnace);GameplayActorBuilder.Facility(furnace,AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/WorldPickup.prefab").GetComponent<WorldPickup>(),Data<GameplaySettings>("GameplaySettings"));Add(items,item);added.Add(Recipe("BlastFurnace",item,new[]{Cost("Bronze",3),Cost("IronIngot",3),Cost("Stone",8),Cost("Wood",4)}));
            var steelRecipe=Data<RecipeDefinition>("SteelRecipe");steelRecipe.RequiredStation=StationKind.BlastFurnace;steelRecipe.RequiredWorkbenchTier=1;EditorUtility.SetDirty(steelRecipe);var chest=Data<PlaceableDefinition>("ReinforcedChestPlacement");chest.StorageSlots=36;EditorUtility.SetDirty(chest);
            foreach(var kind in new[]{ToolKind.Axe,ToolKind.Pickaxe,ToolKind.Sword})
            {
                var name="Steel"+kind;var id="steel_"+kind.ToString().ToLowerInvariant();var t=GameplayAssets.Asset<ToolDefinition>(name+"Tool",d=>{});var sword=kind==ToolKind.Sword;t.Tier=steel;t.Kind=kind;t.HeldSprite=VisualArtImporter.Sprite(id);t.Damage=sword ? 43 : 18;t.Duration=sword ? .28f : kind==ToolKind.Axe ? .29f : .34f;t.Cooldown=sword ? .38f : kind==ToolKind.Axe ? .4f : .48f;t.SwingArc=sword ? 75 : kind==ToolKind.Axe ? 65 : 55;t.HandExtension=.25f;t.Attack.Range=1.6f;
                t.Efficiencies=new[]{new ResourceEfficiency{Kind=ResourceKind.Wood,Multiplier=kind==ToolKind.Axe ? 4.5f : sword ? .25f : .5f},new ResourceEfficiency{Kind=ResourceKind.Stone,Multiplier=kind==ToolKind.Pickaxe ? 4.5f : sword ? .25f : .5f},new ResourceEfficiency{Kind=ResourceKind.IronOre,Multiplier=kind==ToolKind.Pickaxe ? 3.5f : .25f},new ResourceEfficiency{Kind=ResourceKind.CopperOre,Multiplier=kind==ToolKind.Pickaxe ? 3.5f : .25f},new ResourceEfficiency{Kind=ResourceKind.Coal,Multiplier=kind==ToolKind.Pickaxe ? 3.5f : .25f}};EditorUtility.SetDirty(t);
                var equipment=Item(name,id,sword ? ItemCategory.Weapon : ItemCategory.Tool);equipment.Tool=t;EditorUtility.SetDirty(equipment);Add(items,equipment);added.Add(Recipe(name,equipment,new[]{Cost("Iron"+kind,1),Cost("Steel",sword ? 4 : 3),Cost(sword ? "Leather" : "Wood",sword ? 1 : 2)}));
            }
            foreach(var head in new[]{true,false})
            {
                var name=head ? "SteelHead" : "SteelBody";var id=head ? "steel_head" : "steel_body";var armor=GameplayAssets.Asset<ArmorDefinition>(name+"Armor",d=>{});armor.Tier=steel;armor.Slot=head ? ArmorSlot.Head : ArmorSlot.Body;armor.Defense=head ? 3 : 5;armor.WornSprite=VisualArtImporter.Sprite(id+"_worn");armor.HandSprite=head ? null : VisualArtImporter.Sprite("steel_hand_worn");EditorUtility.SetDirty(armor);var equipment=Item(name,id,ItemCategory.Armor);equipment.Armor=armor;EditorUtility.SetDirty(equipment);Add(items,equipment);added.Add(Recipe(name,equipment,new[]{Cost(head ? "IronHead" : "IronBody",1),Cost("Steel",head ? 2 : 3),Cost("Leather",1)}));
            }
            foreach(var recipe in added){Add(list,recipe);Add(content,recipe);}catalog.Items=items.ToArray();catalog.Recipes=content.ToArray();recipes.Recipes=list.ToArray();EditorUtility.SetDirty(catalog);EditorUtility.SetDirty(recipes);
            foreach(var entry in items)if(entry.Tool!=null && entry.Tool.Kind==ToolKind.Pickaxe)
            {var efficiencies=new List<ResourceEfficiency>(entry.Tool.Efficiencies);foreach(var kind in new[]{ResourceKind.CopperOre,ResourceKind.Coal})if(!efficiencies.Exists(e=>e.Kind==kind))efficiencies.Add(new ResourceEfficiency{Kind=kind,Multiplier=entry.Tool.Tier.Rank>=3 ? 3 : 2.5f});entry.Tool.Efficiencies=efficiencies.ToArray();EditorUtility.SetDirty(entry.Tool);}
            foreach(var name in new[]{"CopperOreNode","TinOreNode","CoalNode"})
            {
                var d=Data<ResourceDefinition>(name+"Resource");var panning=name=="TinOreNode";var required=panning ? pan : Data<ItemDefinition>(name=="CopperOreNode" ? "IronPickaxe" : "StonePickaxe");d.RequiredToolItem=required;d.MinimumTier=required.Tool.Tier;d.RequiredTool=required.Tool.Kind;d.HarvestMethod=panning ? HarvestMethod.Panning : HarvestMethod.Strike;EditorUtility.SetDirty(d);
                if(panning){var path="Assets/Wildfolk/Prefabs/TinOreNode.prefab";var root=PrefabUtility.LoadPrefabContents(path);UnityEngine.Object.DestroyImmediate(root.GetComponent<HitFeedback>());var collider=root.GetComponent<CircleCollider2D>();collider.isTrigger=true;root.GetComponent<ResourceNode>().Visual.sprite=VisualArtImporter.Sprite("tin_ore_node");PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);}
            }
            var ecologyRoot=PrefabUtility.LoadPrefabContents(NetworkEcologyBuilder.PrefabPath);var ecology=ecologyRoot.GetComponent<NetworkCreatureWorld>();var entries=new List<NetworkSpawnEntry>(ecology.Prefabs);var source=furnace.Prefab.gameObject;
            if(!entries.Exists(e=>e.Source==source)){var go=(GameObject)PrefabUtility.InstantiatePrefab(source);go.AddComponent<Unity.Netcode.NetworkObject>();go.AddComponent<NetworkBuilding>();go.AddComponent<NetworkFacility>();var prefab=PrefabUtility.SaveAsPrefabAsset(go,"Assets/Wildfolk/Prefabs/NetworkBlastFurnace.prefab");UnityEngine.Object.DestroyImmediate(go);entries.Add(new NetworkSpawnEntry{Source=source,NetworkPrefab=prefab});}
            ecology.Prefabs=entries.ToArray();PrefabUtility.SaveAsPrefabAsset(ecologyRoot,NetworkEcologyBuilder.PrefabPath);PrefabUtility.UnloadPrefabContents(ecologyRoot);
            var session=PrefabUtility.LoadPrefabContents(MultiplayerSceneBuilder.SessionPrefabPath);session.GetComponent<Unity.Netcode.NetworkManager>().NetworkConfig.ProtocolVersion=NetworkProtocol.Current;PrefabUtility.SaveAsPrefabAsset(session,MultiplayerSceneBuilder.SessionPrefabPath);PrefabUtility.UnloadPrefabContents(session);
        }
    }
}
