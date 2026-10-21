using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.Netcode;
namespace Wildfolk.Editor
{
    public static class FishingAssets
    {
        private static T D<T>(string n)where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+n+".asset");
        private static T Make<T>(string name,Action<T> assign)where T:ScriptableObject
        {var d=GameplayAssets.Asset<T>(name,assign);assign(d);EditorUtility.SetDirty(d);return d;}
        public static void Build()
        {
            VisualArtImporter.Import();var c=D<ContentCatalog>("ContentCatalog");var items=new List<ItemDefinition>(c.Items);var recipes=new List<RecipeDefinition>(c.Recipes);var added=new List<RecipeDefinition>();
            var names=new[]{"CrucianCarp","Perch","Mackerel","Eel","FishPortions"};var ids=new[]{"crucian_carp","perch","mackerel","eel","fish_portions"};var hunger=new[]{6,6,6,8,20};var thirst=new[]{6,0,0,0,2};
            for(var n=0;n<ids.Length;n++)
            {var i=n;var food=Make<ConsumableDefinition>(names[i]+"Food",d=>{d.Kind=ConsumableKind.Food;d.HungerRestore=hunger[i];d.ThirstRestore=thirst[i];d.Meal=null;d.HealthRestore=0;d.StaminaRestore=0;d.UseCooldownSeconds=0;});var item=Make<ItemDefinition>(names[i],d=>{d.Id=ids[i];d.DisplayName=names[i];d.MaxStack=50;d.Category=ItemCategory.Consumable;d.Icon=VisualArtImporter.Sprite(ids[i]);d.Consumable=food;});if(!items.Contains(item))items.Add(item);}
            var counts=new[]{1,2,1,2};
            for(var n=0;n<4;n++)
            {var i=n;var r=Make<RecipeDefinition>(names[i]+"FilletRecipe",d=>{d.Id="fillet_"+ids[i];d.OptionalConversion=true;d.DisplayKey="recipe."+d.Id;d.Result=c.Item("raw_fish");d.ResultCount=counts[i];d.RequiredStation=StationKind.None;d.RequiredWorkbenchTier=1;d.Category=RecipeCategory.Survival;d.SortOrder=700+i;d.Ingredients=new[]{new ItemAmount{Item=items.Find(x=>x.Id==ids[i]),Count=1}};});if(!recipes.Contains(r))recipes.Add(r);added.Add(r);}
            var portions=Make<RecipeDefinition>("FishPortionsRecipe",d=>{d.Id="fish_portions";d.Result=items.Find(x=>x.Id=="fish_portions");d.ResultCount=2;d.RequiredStation=StationKind.Campfire;d.RequiredWorkbenchTier=1;d.Category=RecipeCategory.Survival;d.SortOrder=704;d.Processing=ProcessingKind.Cooking;d.ProcessingSeconds=8;d.Ingredients=new[]{new ItemAmount{Item=items.Find(x=>x.Id=="mackerel"),Count=1},new ItemAmount{Item=c.Item("berry"),Count=1}};});if(!recipes.Contains(portions))recipes.Add(portions);added.Add(portions);
            Func<string,string,FishingWater,float,float,FishingTime,FishingCatchDefinition> entry=(id,output,water,weight,inset,t)=>new FishingCatchDefinition{Id=id,NameKey=id=="small_fish" ? "fishing.small_fish" : "item."+output+".name",Output=items.Find(x=>x.Id==output),Amount=1,Waters=water,Weight=weight,MinimumShoreInset=inset,Time=t};
            var table=Make<FishingLootTable>("FishingCatches",t=>t.Entries=new[]{entry("small_fish","raw_fish",FishingWater.Inland,8,0,FishingTime.Any),entry("wetland_fish","raw_fish",FishingWater.Wetlands,6,0,FishingTime.Any),entry("cavern_fish","raw_fish",FishingWater.Cavern,1,0,FishingTime.Any),entry("crucian_carp","crucian_carp",FishingWater.Wetlands,4,0,FishingTime.Any),entry("perch","perch",FishingWater.Inland|FishingWater.Wetlands,2,2.25f,FishingTime.Any),entry("mackerel","mackerel",FishingWater.Ocean,1,0,FishingTime.Any),entry("eel","eel",FishingWater.Inland|FishingWater.Wetlands,.5f,0,FishingTime.Night)});
            c.Items=items.ToArray();c.Recipes=recipes.ToArray();D<RecipeCatalog>("Recipes").Recipes=c.Recipes;EditorUtility.SetDirty(c);EditorUtility.SetDirty(D<RecipeCatalog>("Recipes"));AssetDatabase.SaveAssets();
            foreach(var path in new[]{"Assets/Wildfolk/Prefabs/Player.prefab","Assets/Wildfolk/Prefabs/NetworkPlayer.prefab"}){var p=PrefabUtility.LoadPrefabContents(path);p.GetComponent<PlayerFishing>().CatchTable=table;PrefabUtility.SaveAsPrefabAsset(p,path);PrefabUtility.UnloadPrefabContents(p);}
            var session=PrefabUtility.LoadPrefabContents(MultiplayerSceneBuilder.SessionPrefabPath);session.GetComponent<NetworkManager>().NetworkConfig.ProtocolVersion=NetworkProtocol.Current;PrefabUtility.SaveAsPrefabAsset(session,MultiplayerSceneBuilder.SessionPrefabPath);PrefabUtility.UnloadPrefabContents(session);
            foreach(var path in new[]{GameplaySceneBuilder.ScenePath,MultiplayerSceneBuilder.ScenePath})
            {
                var scene=EditorSceneManager.OpenScene(path);foreach(var fishing in UnityEngine.Object.FindObjectsByType<PlayerFishing>(FindObjectsInactive.Include,FindObjectsSortMode.None))fishing.CatchTable=table;
                foreach(var panel in UnityEngine.Object.FindObjectsByType<CraftingPanel>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {var cards=new List<RecipeCardView>(panel.Cards);foreach(var r in added)if(!cards.Exists(card=>card.Recipe==r)){var card=UnityEngine.Object.Instantiate(panel.Cards[0],panel.Grids[(int)RecipeCategory.Survival].transform);card.Recipe=r;card.Crafting=panel.Owner;cards.Add(card);}var sorted=RecipePresentation.Sorted(panel.Owner.Catalog);cards.Sort((a,b)=>Array.IndexOf(sorted,a.Recipe).CompareTo(Array.IndexOf(sorted,b.Recipe)));panel.Cards=cards.ToArray();foreach(var card in panel.Cards)card.transform.SetAsLastSibling();}EditorSceneManager.SaveScene(scene);
            }
            EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);AssetDatabase.SaveAssets();
        }
    }
}
