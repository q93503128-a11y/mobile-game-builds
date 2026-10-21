using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Wildfolk.Presentation;
namespace Wildfolk.Editor
{
    public static class FoodAssets
    {
        private static T D<T>(string name)where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        private static T Make<T>(string name,Action<T> assign)where T:ScriptableObject
        {var d=GameplayAssets.Asset<T>(name,assign);assign(d);EditorUtility.SetDirty(d);return d;}
        public static void Build()
        {
            VisualArtImporter.Import();var catalog=D<ContentCatalog>("ContentCatalog");var items=new List<ItemDefinition>(catalog.Items);var recipes=new List<RecipeDefinition>(catalog.Recipes);var added=new List<RecipeDefinition>();
            Action<string,float,float> nutrition=(id,h,t)=>{var c=catalog.Item(id).Consumable;c.HungerRestore=h;c.ThirstRestore=t;EditorUtility.SetDirty(c);};
            nutrition("mushroom_stew",35,18);nutrition("meat_stew",52,10);nutrition("fish_stew",42,26);
            var names=new[]{"HuntersStew","ForagerSoup","TrailBroth"};var ids=new[]{"hunters_stew","forager_soup","trail_broth"};var labels=new[]{"Hunter's Stew","Forager Soup","Trail Broth"};var hunger=new[]{44,30,28};var thirst=new[]{12,18,22};var amounts=new[]{8f,.15f,.10f};
            var costs=new[]{new[]{"raw_meat:2","bone:1","mushroom:1","berry:1"},new[]{"medicinal_herb:1","mushroom:2","berry:1"},new[]{"raw_fish:1","medicinal_herb:1","berry:2"}};
            for(var n=0;n<3;n++)
            {
                var i=n;var meal=Make<MealDefinition>(names[i]+"Meal",d=>{d.Effect=(MealEffect)i;d.Amount=amounts[i];d.Duration=150;});
                var food=Make<ConsumableDefinition>(names[i]+"Food",d=>{d.Kind=ConsumableKind.Food;d.Meal=meal;d.HungerRestore=hunger[i];d.ThirstRestore=thirst[i];d.HealthRestore=0;d.StaminaRestore=0;d.UseCooldownSeconds=4;});
                var item=Make<ItemDefinition>(names[i],d=>{d.Id=ids[i];d.DisplayName=labels[i];d.MaxStack=20;d.Category=ItemCategory.Consumable;d.Icon=VisualArtImporter.Sprite(ids[i]);d.Consumable=food;});if(!items.Contains(item))items.Add(item);
                var ingredients=new List<ItemAmount>();foreach(var cost in costs[i]){var pair=cost.Split(':');ingredients.Add(new ItemAmount{Item=catalog.Item(pair[0]),Count=int.Parse(pair[1])});}
                var recipe=Make<RecipeDefinition>(names[i]+"Recipe",d=>{d.Id=ids[i];d.Result=item;d.ResultCount=1;d.RequiredStation=StationKind.CookingPot;d.RequiredWorkbenchTier=1;d.Category=RecipeCategory.Survival;d.Processing=ProcessingKind.Cooking;d.ProcessingSeconds=10;d.SortOrder=670+i;d.Ingredients=ingredients.ToArray();});if(!recipes.Contains(recipe))recipes.Add(recipe);added.Add(recipe);
            }
            catalog.Items=items.ToArray();catalog.Recipes=recipes.ToArray();D<RecipeCatalog>("Recipes").Recipes=recipes.ToArray();EditorUtility.SetDirty(catalog);EditorUtility.SetDirty(D<RecipeCatalog>("Recipes"));AssetDatabase.SaveAssets();
            var session=PrefabUtility.LoadPrefabContents(MultiplayerSceneBuilder.SessionPrefabPath);session.GetComponent<NetworkManager>().NetworkConfig.ProtocolVersion=NetworkProtocol.Current;PrefabUtility.SaveAsPrefabAsset(session,MultiplayerSceneBuilder.SessionPrefabPath);PrefabUtility.UnloadPrefabContents(session);
            foreach(var path in new[]{GameplaySceneBuilder.ScenePath,MultiplayerSceneBuilder.ScenePath})
            {
                var scene=EditorSceneManager.OpenScene(path);
                foreach(var panel in UnityEngine.Object.FindObjectsByType<CraftingPanel>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {var cards=new List<RecipeCardView>(panel.Cards);foreach(var r in added)if(!cards.Exists(c=>c.Recipe==r)){var card=UnityEngine.Object.Instantiate(panel.Cards[0],panel.Grids[(int)RecipeCategory.Survival].transform);card.Recipe=r;card.Crafting=panel.Owner;cards.Add(card);}var sorted=RecipePresentation.Sorted(panel.Owner.Catalog);cards.Sort((a,b)=>Array.IndexOf(sorted,a.Recipe).CompareTo(Array.IndexOf(sorted,b.Recipe)));panel.Cards=cards.ToArray();foreach(var card in panel.Cards)card.transform.SetAsLastSibling();}
                foreach(var panel in UnityEngine.Object.FindObjectsByType<CraftingPanel>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(panel.FoodPreview==null)
                {var preview=UnityEngine.Object.Instantiate(panel.SelectedTitle,panel.SelectedTitle.transform.parent);preview.name="Food details";preview.text="";preview.fontSize=18;preview.raycastTarget=false;preview.enabled=false;preview.alignment=TextAnchor.UpperLeft;preview.rectTransform.anchoredPosition=panel.SelectedTitle.rectTransform.anchoredPosition+Vector2.down*80;preview.rectTransform.sizeDelta=new Vector2(358,112);panel.FoodPreview=preview;}
                if(UnityEngine.Object.FindFirstObjectByType<MealHud>()==null)
                {
                    var safe=UnityEngine.Object.FindFirstObjectByType<SafeAreaLayout>();var source=UnityEngine.Object.FindFirstObjectByType<MedicineHud>().Label;var label=UnityEngine.Object.Instantiate(source,safe.transform);UnityEngine.Object.DestroyImmediate(label.GetComponent<MedicineHud>());label.name="Meal";label.text="";label.enabled=false;label.raycastTarget=false;
                    label.rectTransform.anchorMin=label.rectTransform.anchorMax=label.rectTransform.pivot=new Vector2(0,1);label.rectTransform.anchoredPosition=new Vector2(78,-310);label.rectTransform.sizeDelta=new Vector2(440,36);
                    var icon=new GameObject("Meal icon",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image)).GetComponent<Image>();icon.transform.SetParent(safe.transform,false);icon.raycastTarget=false;icon.enabled=false;icon.preserveAspect=true;icon.rectTransform.anchorMin=icon.rectTransform.anchorMax=icon.rectTransform.pivot=new Vector2(0,1);icon.rectTransform.anchoredPosition=new Vector2(36,-310);icon.rectTransform.sizeDelta=new Vector2(34,34);
                    var hud=label.gameObject.AddComponent<MealHud>();hud.Label=label;hud.Icon=icon;hud.Player=source.GetComponent<MedicineHud>().Player;
                }
                EditorSceneManager.SaveScene(scene);
            }
            EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);AssetDatabase.SaveAssets();
        }
    }
}
