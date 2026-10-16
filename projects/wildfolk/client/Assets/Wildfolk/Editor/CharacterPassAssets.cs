using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    internal static class CharacterPassAssets
    {
        internal static CharacterGrowthDefinition Growth()=>GameplayAssets.Asset<CharacterGrowthDefinition>("CharacterGrowth",d=>d.Skills=new[]{
            Skill("vitality","Vitality",SkillEffect.MaximumHealth,10),
            Skill("endurance","Endurance",SkillEffect.MaximumStamina,15),
            Skill("recovery","Recovery",SkillEffect.StaminaRecovery,.15f),
            Skill("stride","Steady Stride",SkillEffect.SprintEfficiency,.12f)});
        private static CharacterSkill Skill(string id,string name,SkillEffect effect,float amount)=>new CharacterSkill{Id=id,Name=name,MaximumRank=3,Effect=effect,PerRank=amount,Icon=VisualArtImporter.Sprite("skill_"+id)};
        internal static LocomotionDefinition Locomotion()=>GameplayAssets.Asset<LocomotionDefinition>("Locomotion",d=>{});
        private static T Data<T>(string name) where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        private static ItemAmount Cost(ItemDefinition item,int count)=>new ItemAmount{Item=item,Count=count};
        internal static PlaceableDefinition[] Load(GameplayAssets assets)
        {
            Growth();Locomotion();
            var wood=Data<ItemDefinition>("Wood");var stone=Data<ItemDefinition>("Stone");var leather=Data<ItemDefinition>("Leather");var ingot=Data<ItemDefinition>("IronIngot");
            var list=new List<RecipeDefinition>(assets.Crafting.Catalog.Recipes);
            foreach(var iron in new[]{false,true})foreach(var head in new[]{true,false})
            {
                var material=iron ? "iron" : "leather";var slot=head ? "head" : "body";var name=(iron ? "Iron" : "Leather")+(head ? "Head" : "Body");
                var armor=GameplayAssets.Asset<ArmorDefinition>(name+"Armor",d=>{d.Slot=head ? ArmorSlot.Head : ArmorSlot.Body;d.Defense=iron ? head ? 2 : 4 : head ? 1 : 2;d.Tier=iron ? assets.Progression.IronTier : assets.Progression.WoodTier;d.WornSprite=VisualArtImporter.Sprite(material+"_"+slot+"_worn");d.HandSprite=head ? null : VisualArtImporter.Sprite(material+"_hand_worn");});
                var item=GameplayAssets.Asset<ItemDefinition>(name,i=>{i.Id=material+"_"+slot;i.DisplayName=iron ? head ? "Iron Helmet" : "Iron Guard" : head ? "Leather Cap" : "Leather Guard";i.Category=ItemCategory.Armor;i.MaxStack=1;i.Armor=armor;i.Icon=VisualArtImporter.Sprite(i.Id);});
                var costs=iron ? new[]{Cost(ingot,head ? 2 : 3),Cost(leather,1)} : new[]{Cost(leather,head ? 1 : 2),Cost(wood,head ? 2 : 3)};
                AddRecipe(list,name,item,costs,1);
            }
            var bench=Facility("WorkbenchII","workbench_ii","Workbench II",StationKind.Workbench,assets.Progression.StoneTier,120,new Vector2(1.8f,1.15f));
            bench.WorkbenchTier=2;EditorUtility.SetDirty(bench);
            assets.Crafting.Workbench.UpgradeTo=bench;assets.Crafting.Workbench.UpgradeCosts=new[]{Cost(wood,8),Cost(stone,6),Cost(ingot,2)};EditorUtility.SetDirty(assets.Crafting.Workbench);
            var chest=Facility("ReinforcedChest","reinforced_chest","Reinforced Chest",StationKind.None,assets.Progression.StoneTier,140,new Vector2(1.6f,1.15f),36);
            var wall=Facility("StoneWall","stone_wall","Stone Wall",StationKind.None,assets.Progression.StoneTier,220,new Vector2(1.8f,.5f),barrier:true);
            AddRecipe(list,"ReinforcedChest",chest.Item,new[]{Cost(wood,8),Cost(ingot,2)},2);
            AddRecipe(list,"StoneWall",wall.Item,new[]{Cost(stone,3),Cost(wood,1)},2);
            var order=0;foreach(var recipe in list)
            {
                if(recipe.SortOrder<0)
                {recipe.Category=recipe.Result.Tool!=null ? recipe.Result.Category==ItemCategory.Weapon ? RecipeCategory.Weapons : RecipeCategory.Tools : recipe.Result.Armor!=null ? RecipeCategory.Armor : recipe.Result.Placement!=null ? recipe.Result.Placement.IsBarrier ? RecipeCategory.Building : RecipeCategory.Facilities : RecipeCategory.Survival;
                recipe.SortOrder=order;EditorUtility.SetDirty(recipe);}
                order++;
            }
            assets.Crafting.Catalog.Recipes=list.ToArray();EditorUtility.SetDirty(assets.Crafting.Catalog);return new[]{bench,chest,wall};
        }
        private static void AddRecipe(List<RecipeDefinition> list,string name,ItemDefinition item,ItemAmount[] costs,int tier)
        {var recipe=GameplayAssets.Asset<RecipeDefinition>(name+"Recipe",d=>{d.Id=item.Id;d.Result=item;d.ResultCount=1;d.Ingredients=costs;d.RequiredStation=StationKind.Workbench;d.RequiredWorkbenchTier=tier;d.Tier=item.Tier;});if(!list.Contains(recipe))list.Add(recipe);}
        private static PlaceableDefinition Facility(string name,string id,string display,StationKind station,EquipmentTier tier,int hp,Vector2 footprint,int storage=0,bool barrier=false)
        {
            var vitality=GameplayAssets.Asset<VitalityDefinition>(name+"Vitality",d=>d.MaximumHealth=hp);
            var placed=GameplayAssets.Asset<PlaceableDefinition>(name+"Placement",d=>{d.Station=station;d.Tier=tier;d.Vitality=vitality;d.Footprint=footprint;d.MaximumDistance=2.8f;d.StorageSlots=storage;d.IsBarrier=barrier;});
            var item=GameplayAssets.Asset<ItemDefinition>(name,d=>{d.Id=id;d.DisplayName=display;d.Placement=placed;d.MaxStack=10;d.Category=ItemCategory.Placeable;d.Icon=VisualArtImporter.Sprite(id);});placed.Item=item;EditorUtility.SetDirty(placed);return placed;
        }
    }
}
