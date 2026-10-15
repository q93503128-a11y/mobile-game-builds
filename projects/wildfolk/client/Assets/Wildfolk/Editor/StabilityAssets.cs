using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    internal static class StabilityAssets
    {
        internal static ContentCatalog Catalog()
        {
            var catalog=GameplayAssets.Asset<ContentCatalog>("ContentCatalog",c=>{});
            var items=new List<ItemDefinition>();foreach(var guid in AssetDatabase.FindAssets("t:ItemDefinition",new[]{"Assets/Wildfolk/Data"}))items.Add(AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(guid)));
            var recipes=new List<RecipeDefinition>();foreach(var guid in AssetDatabase.FindAssets("t:RecipeDefinition",new[]{"Assets/Wildfolk/Data"}))recipes.Add(AssetDatabase.LoadAssetAtPath<RecipeDefinition>(AssetDatabase.GUIDToAssetPath(guid)));
            items.Sort((a,b)=>string.CompareOrdinal(a.Id,b.Id));recipes.Sort((a,b)=>string.CompareOrdinal(a.Id,b.Id));catalog.Items=items.ToArray();catalog.Recipes=recipes.ToArray();EditorUtility.SetDirty(catalog);
            foreach(var rank in new[]{1,2,3,4})
            {
                var name=rank==1 ? "Early Game" : rank==2 ? "Stone Tier" : rank==3 ? "Iron Tier" : "Workbench II";
                var profile=GameplayAssets.Asset<DeveloperTestProfile>("DeveloperProfile"+rank,p=>
                {
                    p.ProfileName=name;var supplies=new List<ItemAmount>();
                    foreach(var item in catalog.Items)
                    {
                        if(item.Tool!=null && item.Tier.Rank==Mathf.Min(rank,3))supplies.Add(new ItemAmount{Item=item,Count=1});
                        if(item.Id=="wood" || item.Id=="stone" || rank>=2 && item.Id=="iron_ore" || rank>=3 && item.Id=="iron_ingot")supplies.Add(new ItemAmount{Item=item,Count=30});
                        if(item.Id=="empty_cup" || item.Id=="cooked_meat")supplies.Add(new ItemAmount{Item=item,Count=3});
                        if(item.Placement!=null)supplies.Add(new ItemAmount{Item=item,Count=1});
                    }
                    p.Items=supplies.ToArray();
                });
            }
            return catalog;
        }
        internal static EnemySteeringDefinition Steering()=>GameplayAssets.Asset<EnemySteeringDefinition>("EnemySteering",d=>{});
        internal static AnimalPopulationDefinition Animals()=>GameplayAssets.Asset<AnimalPopulationDefinition>("AnimalPopulation",d=>d.Rules=new[]{
            new AnimalPopulationRule{Prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/Chicken.prefab").GetComponent<NeutralAnimal>(),Population=1,RespawnSeconds=180},
            new AnimalPopulationRule{Prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/Boar.prefab").GetComponent<NeutralAnimal>(),Population=1,RespawnSeconds=300}});
    }
}
