using UnityEngine;

namespace Wildfolk.Editor
{
    internal sealed class CraftingAssets
    {
        internal RecipeCatalog Catalog;
        internal PlaceableDefinition Workbench;

        internal static CraftingAssets Load(ItemDefinition wood,ItemDefinition axe,ItemDefinition pickaxe,SurvivalAssets survival)
        {
            var assets=new CraftingAssets();
            var vitality=GameplayAssets.Asset<VitalityDefinition>("WorkbenchVitality",v=>v.MaximumHealth=60);
            assets.Workbench=GameplayAssets.Asset<PlaceableDefinition>("WorkbenchPlacement",p=>
            {p.Vitality=vitality;p.Footprint=new Vector2(1.8f,1.15f);p.MaximumDistance=2.8f;p.Station=StationKind.Workbench;});
            var workbench=GameplayAssets.Asset<ItemDefinition>("Workbench",i=>
            {i.Id="workbench";i.DisplayName="Workbench";i.Icon=VisualArtImporter.Sprite("workbench");i.MaxStack=10;i.Category=ItemCategory.Placeable;i.Placement=assets.Workbench;});
            assets.Workbench.Item=workbench;
            UnityEditor.EditorUtility.SetDirty(assets.Workbench);
            var swordTool=GameplayAssets.Tool("WoodSwordTool","wood_sword",.34f,.46f,75,.25f,.25f,20);
            var sword=GameplayAssets.Asset<ItemDefinition>("WoodSword",i=>
            {i.Id="wood_sword";i.DisplayName="Wood Sword";i.Icon=VisualArtImporter.Sprite("wood_sword");i.MaxStack=1;i.Category=ItemCategory.Weapon;i.Tool=swordTool;});
            var hand=Recipe("WorkbenchRecipe","workbench",workbench,wood,6,StationKind.None);
            var axeRecipe=Recipe("AxeRecipe","axe",axe,wood,5,StationKind.Workbench);
            var pickRecipe=Recipe("PickaxeRecipe","pickaxe",pickaxe,wood,5,StationKind.Workbench);
            var swordRecipe=Recipe("WoodSwordRecipe","wood_sword",sword,wood,5,StationKind.Workbench);
            assets.Catalog=GameplayAssets.Asset<RecipeCatalog>("Recipes",c=>c.Recipes=new[]{hand,survival.FireRecipe,axeRecipe,pickRecipe,swordRecipe,survival.CookingRecipe});
            return assets;
        }
        private static RecipeDefinition Recipe(string name,string id,ItemDefinition result,ItemDefinition wood,int cost,StationKind station)
        {
            return GameplayAssets.Asset<RecipeDefinition>(name,r=>
            {r.Id=id;r.Result=result;r.ResultCount=1;r.Ingredients=new[]{new ItemAmount{Item=wood,Count=cost}};r.RequiredStation=station;});
        }
    }
}
