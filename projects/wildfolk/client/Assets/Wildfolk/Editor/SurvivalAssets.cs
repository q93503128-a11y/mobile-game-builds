using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    internal sealed class SurvivalAssets
    {
        internal SurvivalDefinition Needs;
        internal ItemDefinition Raw, Cooked, Water;
        internal WaterSourceDefinition Source;
        internal ResourceDefinition Meat;
        internal PlaceableDefinition Campfire;
        internal RecipeDefinition FireRecipe, CookingRecipe;
        internal static SurvivalAssets Load(ItemDefinition wood,ItemDefinition stone)
        {
            var a=new SurvivalAssets();
            a.Needs=GameplayAssets.Asset<SurvivalDefinition>("Survival",s=>{});
            a.Raw=Food("RawMeat","raw_meat","Raw Meat",8,0);
            a.Cooked=Food("CookedMeat","cooked_meat","Cooked Meat",35,0);
            a.Water=Food("Water","water","Water",0,30);
            a.Source=GameplayAssets.Asset<WaterSourceDefinition>("NaturalWater",s=>{s.Water=a.Water;});
            var animalHealth=GameplayAssets.Asset<VitalityDefinition>("BoarVitality",v=>v.MaximumHealth=20);
            a.Meat=GameplayAssets.Asset<ResourceDefinition>("MeatResource",r=>{r.Kind=ResourceKind.Meat;r.Vitality=animalHealth;r.DropItem=a.Raw;r.DropCount=2;});
            var fireHealth=GameplayAssets.Asset<VitalityDefinition>("CampfireVitality",v=>v.MaximumHealth=60);
            a.Campfire=GameplayAssets.Asset<PlaceableDefinition>("CampfirePlacement",p=>
            {p.Vitality=fireHealth;p.Footprint=new Vector2(1.5f,1.15f);p.MaximumDistance=2.8f;p.Station=StationKind.Campfire;});
            var fire=GameplayAssets.Asset<ItemDefinition>("Campfire",i=>
            {i.Id="campfire";i.DisplayName="Campfire";i.Icon=VisualArtImporter.Sprite("campfire");i.MaxStack=10;i.Category=ItemCategory.Placeable;i.Placement=a.Campfire;});
            a.Campfire.Item=fire;EditorUtility.SetDirty(a.Campfire);
            a.FireRecipe=GameplayAssets.Asset<RecipeDefinition>("CampfireRecipe",r=>
            {r.Id="campfire";r.Result=fire;r.ResultCount=1;r.RequiredStation=StationKind.None;r.Ingredients=new[]{new ItemAmount{Item=wood,Count=4},new ItemAmount{Item=stone,Count=3}};});
            a.CookingRecipe=GameplayAssets.Asset<RecipeDefinition>("CookedMeatRecipe",r=>
            {r.Id="cooked_meat";r.Result=a.Cooked;r.ResultCount=1;r.RequiredStation=StationKind.Campfire;r.CookingSeconds=5;r.Ingredients=new[]{new ItemAmount{Item=a.Raw,Count=1}};});
            return a;
        }
        private static ItemDefinition Food(string name,string id,string display,float hunger,float thirst)
        {
            var effect=GameplayAssets.Asset<ConsumableDefinition>(name+"Effect",c=>{c.HungerRestore=hunger;c.ThirstRestore=thirst;});
            return GameplayAssets.Asset<ItemDefinition>(name,i=>
            {i.Id=id;i.DisplayName=display;i.Icon=VisualArtImporter.Sprite(id);i.MaxStack=20;i.Category=ItemCategory.Consumable;i.Consumable=effect;});
        }
    }
}
