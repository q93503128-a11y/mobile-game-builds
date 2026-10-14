using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    internal sealed class SurvivalAssets
    {
        internal SurvivalDefinition Needs;
        internal ItemDefinition Raw,Cooked,EmptyCup,RawWater,CleanWater;
        internal WaterContainerDefinition Container;
        internal RecipeDefinition CupRecipe,BoilingRecipe;
        internal WaterSourceDefinition Source;
        internal VitalityDefinition Boar;
        internal PlaceableDefinition Campfire;
        internal RecipeDefinition FireRecipe, CookingRecipe;
        internal static SurvivalAssets Load(ItemDefinition wood,ItemDefinition stone)
        {
            var a=new SurvivalAssets();
            a.Needs=GameplayAssets.Asset<SurvivalDefinition>("Survival",s=>{});
            a.Raw=Food("RawMeat","raw_meat","Raw Meat",8,0);
            a.Cooked=Food("CookedMeat","cooked_meat","Cooked Meat",35,0);
            a.Container=GameplayAssets.Asset<WaterContainerDefinition>("WoodenCupContainer",c=>c.Id="wooden_cup");
            a.EmptyCup=GameplayAssets.Asset<ItemDefinition>("EmptyCup",i=>{i.Id="empty_cup";i.DisplayName="Wooden Cup";i.Icon=VisualArtImporter.Sprite("empty_cup");i.MaxStack=20;i.Category=ItemCategory.Container;i.Container=a.Container;i.Liquid=LiquidState.Empty;});
            a.RawWater=Cup("RawWaterCup","raw_water_cup","Cup of Raw Water",a.Container,LiquidState.Raw,15,60,1.5f);
            a.CleanWater=Cup("CleanWaterCup","clean_water_cup","Cup of Clean Water",a.Container,LiquidState.Clean,38,0,1);
            a.Container.Empty=a.EmptyCup;a.Container.Raw=a.RawWater;a.Container.Clean=a.CleanWater;EditorUtility.SetDirty(a.Container);
            a.Source=GameplayAssets.Asset<WaterSourceDefinition>("NaturalWater",d=>{});a.Source.Container=a.Container;EditorUtility.SetDirty(a.Source);
            a.CupRecipe=GameplayAssets.Asset<RecipeDefinition>("EmptyCupRecipe",r=>{r.Id="empty_cup";r.Result=a.EmptyCup;r.ResultCount=1;r.RequiredStation=StationKind.None;r.Ingredients=new[]{new ItemAmount{Item=wood,Count=2}};});
            a.BoilingRecipe=GameplayAssets.Asset<RecipeDefinition>("CleanWaterCupRecipe",r=>{r.Id="clean_water_cup";r.Result=a.CleanWater;r.ResultCount=1;r.RequiredStation=StationKind.Campfire;r.Processing=ProcessingKind.Cooking;r.ProcessingSeconds=5;r.Ingredients=new[]{new ItemAmount{Item=a.RawWater,Count=1}};});
            var animalHealth=GameplayAssets.Asset<VitalityDefinition>("BoarVitality",v=>v.MaximumHealth=20);
            a.Boar=animalHealth;
            var fireHealth=GameplayAssets.Asset<VitalityDefinition>("CampfireVitality",v=>v.MaximumHealth=60);
            a.Campfire=GameplayAssets.Asset<PlaceableDefinition>("CampfirePlacement",p=>
            {p.Vitality=fireHealth;p.Footprint=new Vector2(1.5f,1.15f);p.MaximumDistance=2.8f;p.Station=StationKind.Campfire;});
            var fire=GameplayAssets.Asset<ItemDefinition>("Campfire",i=>
            {i.Id="campfire";i.DisplayName="Campfire";i.Icon=VisualArtImporter.Sprite("campfire");i.MaxStack=10;i.Category=ItemCategory.Placeable;i.Placement=a.Campfire;});
            a.Campfire.Item=fire;EditorUtility.SetDirty(a.Campfire);
            a.FireRecipe=GameplayAssets.Asset<RecipeDefinition>("CampfireRecipe",r=>
            {r.Id="campfire";r.Result=fire;r.ResultCount=1;r.RequiredStation=StationKind.None;r.Ingredients=new[]{new ItemAmount{Item=wood,Count=4},new ItemAmount{Item=stone,Count=3}};});
            a.CookingRecipe=GameplayAssets.Asset<RecipeDefinition>("CookedMeatRecipe",r=>
            {r.Id="cooked_meat";r.Result=a.Cooked;r.ResultCount=1;r.RequiredStation=StationKind.Campfire;r.ProcessingSeconds=5;r.Ingredients=new[]{new ItemAmount{Item=a.Raw,Count=1}};});
            return a;
        }
        private static ItemDefinition Cup(string name,string id,string display,WaterContainerDefinition container,LiquidState state,float restore,float penalty,float multiplier)
        {
            var effect=GameplayAssets.Asset<ConsumableDefinition>(name+"Effect",c=>{c.ThirstRestore=restore;c.RecoveryBlockSeconds=penalty;c.DrainPenaltySeconds=penalty;c.ThirstDrainMultiplier=multiplier;});
            return GameplayAssets.Asset<ItemDefinition>(name,i=>{i.Id=id;i.DisplayName=display;i.Icon=VisualArtImporter.Sprite(id);i.MaxStack=1;i.Category=ItemCategory.Consumable;i.Consumable=effect;i.Container=container;i.Liquid=state;});
        }
        private static ItemDefinition Food(string name,string id,string display,float hunger,float thirst)
        {
            var effect=GameplayAssets.Asset<ConsumableDefinition>(name+"Effect",c=>{c.HungerRestore=hunger;c.ThirstRestore=thirst;});
            return GameplayAssets.Asset<ItemDefinition>(name,i=>
            {i.Id=id;i.DisplayName=display;i.Icon=VisualArtImporter.Sprite(id);i.MaxStack=20;i.Category=ItemCategory.Consumable;i.Consumable=effect;});
        }
    }
}
