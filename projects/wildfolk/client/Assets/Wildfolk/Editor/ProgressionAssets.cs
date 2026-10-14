using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    internal sealed class ProgressionAssets
    {
        internal EquipmentTier WoodTier,StoneTier,IronTier;
        internal PlaceableDefinition Furnace,Wall,Door,Chest;
        internal ResourceDefinition Ore;
        internal VitalityDefinition Chicken;
        internal AnimalMovementDefinition Animal;
        internal DayNightDefinition DayNight;
        internal ZombieSpawnDefinition Spawn;
        internal static ProgressionAssets Load(ItemDefinition wood,ItemDefinition stone,CraftingAssets crafting,SurvivalAssets survival)
        {
            var a=new ProgressionAssets();
            a.WoodTier=Tier("Wood",1);a.StoneTier=Tier("Stone",2);a.IronTier=Tier("Iron",3);
            foreach(var recipe in crafting.Catalog.Recipes)
            {
                var tool=recipe.Result.Tool;if(tool==null)continue;
                if(tool.Tier==null){tool.Tier=a.WoodTier;tool.Kind=recipe.Result.Category==ItemCategory.Weapon ? ToolKind.Sword : recipe.Result.Id=="pickaxe" ? ToolKind.Pickaxe : ToolKind.Axe;EditorUtility.SetDirty(tool);}
            }
            var ore=Item("IronOre","iron_ore","Iron Ore",ItemCategory.Material,50);
            var ingot=Item("IronIngot","iron_ingot","Iron Ingot",ItemCategory.Material,50);
            var oreHealth=GameplayAssets.Asset<VitalityDefinition>("IronOreVitality",v=>v.MaximumHealth=75);
            a.Ore=GameplayAssets.Asset<ResourceDefinition>("IronOreResource",r=>{r.Kind=ResourceKind.IronOre;r.DropItem=ore;r.DropCount=3;r.Vitality=oreHealth;r.MinimumTier=a.StoneTier;r.RequiredTool=ToolKind.Pickaxe;});
            a.Furnace=Facility("Furnace","furnace",StationKind.Furnace,new Vector2(1.8f,1.6f),100,a.StoneTier);
            a.Wall=Facility("WoodWall","wood_wall",StationKind.None,new Vector2(1.8f,.45f),100,a.WoodTier,barrier:true);
            a.Door=Facility("WoodDoor","wood_door",StationKind.None,new Vector2(1.8f,.45f),80,a.WoodTier,barrier:true,door:true);
            a.Chest=Facility("StorageChest","storage_chest",StationKind.None,new Vector2(1.5f,1.1f),70,a.WoodTier,storage:24);
            var recipes=new List<RecipeDefinition>();
            foreach(var tier in new[]{a.StoneTier,a.IronTier})
            {
                var metal=tier==a.StoneTier ? stone : ingot;
                foreach(var kind in new[]{ToolKind.Axe,ToolKind.Pickaxe,ToolKind.Sword})
                {
                    ItemDefinition previous=null;
                    foreach(var candidate in crafting.Catalog.Recipes)if(candidate.Result.Tool!=null && candidate.Result.Tool.Kind==kind && candidate.Result.Tool.Tier.Rank==tier.Rank-1)previous=candidate.Result;
                    foreach(var candidate in recipes)if(candidate.Result.Tool!=null && candidate.Result.Tool.Kind==kind && candidate.Result.Tool.Tier.Rank==tier.Rank-1)previous=candidate.Result;
                    if(previous==null)throw new System.InvalidOperationException("Equipment predecessor is required.");
                    recipes.Add(Recipe(tier.DisplayName+kind,Equipment(tier,kind),StationKind.Workbench,new[]{Cost(previous,1),Cost(metal,tier==a.StoneTier ? kind==ToolKind.Sword ? 5 : 4 : 3)}));
                }
            }
            recipes.Add(Recipe("Furnace",a.Furnace.Item,StationKind.Workbench,new[]{Cost(stone,8),Cost(wood,4)}));
            recipes.Add(Recipe("WoodWall",a.Wall.Item,StationKind.Workbench,new[]{Cost(wood,2)}));
            recipes.Add(Recipe("WoodDoor",a.Door.Item,StationKind.Workbench,new[]{Cost(wood,3)}));
            recipes.Add(Recipe("StorageChest",a.Chest.Item,StationKind.Workbench,new[]{Cost(wood,6)}));
            recipes.Add(Recipe("IronIngot",ingot,StationKind.Furnace,new[]{Cost(ore,1)},6,ProcessingKind.Smelting));
            var catalog=new List<RecipeDefinition>(crafting.Catalog.Recipes);
            foreach(var recipe in recipes)if(!catalog.Contains(recipe))catalog.Add(recipe);
            if(catalog.Count!=crafting.Catalog.Recipes.Length){crafting.Catalog.Recipes=catalog.ToArray();EditorUtility.SetDirty(crafting.Catalog);}
            foreach(var recipe in catalog)if(recipe.Tier==null && recipe.Result.Tier!=null){recipe.Tier=recipe.Result.Tier;EditorUtility.SetDirty(recipe);}
            crafting.Workbench.Tier=a.WoodTier;if(string.IsNullOrEmpty(crafting.Workbench.StationTitle))crafting.Workbench.StationTitle="Workbench";EditorUtility.SetDirty(crafting.Workbench);
            survival.Campfire.Tier=a.WoodTier;if(string.IsNullOrEmpty(survival.Campfire.StationTitle))survival.Campfire.StationTitle="Campfire cooking";EditorUtility.SetDirty(survival.Campfire);
            if(survival.CookingRecipe.Processing==ProcessingKind.None){survival.CookingRecipe.Processing=ProcessingKind.Cooking;EditorUtility.SetDirty(survival.CookingRecipe);}
            a.Animal=GameplayAssets.Asset<AnimalMovementDefinition>("AnimalMovement",d=>{});
            var chickenHealth=GameplayAssets.Asset<VitalityDefinition>("ChickenVitality",v=>v.MaximumHealth=20);
            a.Chicken=chickenHealth;
            a.DayNight=GameplayAssets.Asset<DayNightDefinition>("DayNight",d=>{d.SunIcon=VisualArtImporter.Sprite("sun");d.MoonIcon=VisualArtImporter.Sprite("moon");});
            a.Spawn=GameplayAssets.Asset<ZombieSpawnDefinition>("ZombieSpawn",d=>{});
            return a;
        }
        private static EquipmentTier Tier(string name,int rank) => GameplayAssets.Asset<EquipmentTier>(name+"Tier",t=>{t.Id=name.ToLowerInvariant();t.DisplayName=name;t.Rank=rank;});
        private static ItemAmount Cost(ItemDefinition item,int count) => new ItemAmount{Item=item,Count=count};
        private static ItemDefinition Item(string name,string id,string display,ItemCategory category,int stack) => GameplayAssets.Asset<ItemDefinition>(name,i=>{i.Id=id;i.DisplayName=display;i.Category=category;i.MaxStack=stack;i.Icon=VisualArtImporter.Sprite(id);});
        private static ItemDefinition Equipment(EquipmentTier tier,ToolKind kind)
        {
            var name=tier.DisplayName+kind;var id=tier.Id+"_"+kind.ToString().ToLowerInvariant();
            var iron=tier.Rank==3;var sword=kind==ToolKind.Sword;var axe=kind==ToolKind.Axe;
            var tool=GameplayAssets.Asset<ToolDefinition>(name+"Tool",t=>
            {
                t.Tier=tier;t.Kind=kind;t.HeldSprite=VisualArtImporter.Sprite(id);t.Damage=sword ? iron ? 36 : 28 : iron ? 15 : 12;
                t.Attack.Range=1.6f;t.Duration=sword ? iron ? .28f : .31f : axe ? iron ? .3f : .36f : iron ? .36f : .44f;
                t.Cooldown=sword ? iron ? .38f : .42f : axe ? iron ? .42f : .5f : iron ? .5f : .6f;
                t.SwingArc=sword ? 75 : axe ? 65 : 55;t.HandExtension=.25f;
                var efficient=iron ? 4 : 3.5f;
                t.Efficiencies=new[]{new ResourceEfficiency{Kind=ResourceKind.Wood,Multiplier=axe ? efficient : sword ? .25f : .5f},new ResourceEfficiency{Kind=ResourceKind.Stone,Multiplier=kind==ToolKind.Pickaxe ? efficient : sword ? .25f : .5f},new ResourceEfficiency{Kind=ResourceKind.IronOre,Multiplier=kind==ToolKind.Pickaxe ? iron ? 3 : 2.5f : .25f}};
            });
            var item=Item(name,id,tier.DisplayName+" "+kind,sword ? ItemCategory.Weapon : ItemCategory.Tool,1);
            if(item.Tool==null){item.Tool=tool;EditorUtility.SetDirty(item);}return item;
        }
        private static PlaceableDefinition Facility(string name,string id,StationKind station,Vector2 footprint,int hp,EquipmentTier tier,bool barrier=false,bool door=false,int storage=0)
        {
            var vitality=GameplayAssets.Asset<VitalityDefinition>(name+"Vitality",v=>v.MaximumHealth=hp);
            var definition=GameplayAssets.Asset<PlaceableDefinition>(name+"Placement",p=>{p.Vitality=vitality;p.Footprint=footprint;p.MaximumDistance=2.8f;p.Station=station;p.Tier=tier;p.IsBarrier=barrier;p.IsDoor=door;p.StorageSlots=storage;p.ParallelCapacity=station==StationKind.Furnace ? 3 : 1;p.StationTitle=station==StationKind.Furnace ? "Furnace smelting" : name;});
            var display=name=="StorageChest" ? "Storage Chest" : name=="WoodWall" ? "Wood Wall" : name=="WoodDoor" ? "Wood Door" : name;
            var item=Item(name,id,display,ItemCategory.Placeable,10);if(item.Placement==null){item.Placement=definition;EditorUtility.SetDirty(item);}
            definition.Item=item;EditorUtility.SetDirty(definition);return definition;
        }
        private static RecipeDefinition Recipe(string name,ItemDefinition result,StationKind station,ItemAmount[] costs,float seconds=0,ProcessingKind process=ProcessingKind.None) => GameplayAssets.Asset<RecipeDefinition>(name+"Recipe",r=>{r.Id=result.Id;r.Result=result;r.ResultCount=1;r.Ingredients=costs;r.RequiredStation=station;r.ProcessingSeconds=seconds;r.Processing=process;r.Tier=result.Tier;});
    }
}
