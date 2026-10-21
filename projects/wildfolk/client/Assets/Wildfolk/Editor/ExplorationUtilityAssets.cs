using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.Netcode;
namespace Wildfolk.Editor
{
    public static class ExplorationUtilityAssets
    {
        private static T D<T>(string n)where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+n+".asset");
        private static T Make<T>(string name,Action<T> assign)where T:ScriptableObject{var d=GameplayAssets.Asset<T>(name,assign);assign(d);EditorUtility.SetDirty(d);return d;}
        public static void Build()
        {
            VisualArtImporter.Import();var c=D<ContentCatalog>("ContentCatalog");var items=new List<ItemDefinition>(c.Items);var recipes=new List<RecipeDefinition>(c.Recipes);var added=new List<RecipeDefinition>();
            var names=new[]{"OreSurveyor","SignalWhistle"};var ids=new[]{"ore_surveyor","signal_whistle"};
            for(var n=0;n<2;n++)
            {
                var i=n;var d=Make<ExplorationUtilityDefinition>(names[i]+"Utility",v=>{v.Kind=i==0 ? ExplorationUtilityKind.Survey : ExplorationUtilityKind.Signal;v.Range=i==0 ? 18 : 60;v.ChannelSeconds=i==0 ? 1.2f : 0;v.CooldownSeconds=i==0 ? 12 : 30;v.LifetimeSeconds=i==0 ? 6 : 20;});
                var item=Make<ItemDefinition>(names[i],v=>{v.Id=ids[i];v.DisplayName=names[i];v.MaxStack=1;v.Category=ItemCategory.Tool;v.Icon=VisualArtImporter.Sprite(ids[i]);v.HeldSprite=v.Icon;v.ExplorationUtility=d;});if(!items.Contains(item))items.Add(item);
                var recipe=Make<RecipeDefinition>(names[i]+"Recipe",r=>{r.Id=ids[i];r.Result=item;r.ResultCount=1;r.RequiredStation=StationKind.Workbench;r.RequiredWorkbenchTier=i==0 ? 2 : 1;r.Category=RecipeCategory.Tools;r.SortOrder=710+i;r.Ingredients=i==0 ? new[]{new ItemAmount{Item=c.Item("wood"),Count=2},new ItemAmount{Item=c.Item("bronze"),Count=2},new ItemAmount{Item=c.Item("leather"),Count=1}} : new[]{new ItemAmount{Item=c.Item("wood"),Count=2},new ItemAmount{Item=c.Item("plant_fiber"),Count=2}};});if(!recipes.Contains(recipe))recipes.Add(recipe);added.Add(recipe);
            }
            c.Items=items.ToArray();c.Recipes=recipes.ToArray();D<RecipeCatalog>("Recipes").Recipes=c.Recipes;EditorUtility.SetDirty(c);EditorUtility.SetDirty(D<RecipeCatalog>("Recipes"));AssetDatabase.SaveAssets();
            var basePath="Assets/Wildfolk/Prefabs/Player.prefab";var p=PrefabUtility.LoadPrefabContents(basePath);var player=p.GetComponent<PlayerExplorationUtility>()??p.AddComponent<PlayerExplorationUtility>();player.Catalog=c;PrefabUtility.SaveAsPrefabAsset(p,basePath);PrefabUtility.UnloadPrefabContents(p);
            var netPath="Assets/Wildfolk/Prefabs/NetworkPlayer.prefab";p=PrefabUtility.LoadPrefabContents(netPath);player=p.GetComponent<PlayerExplorationUtility>()??p.AddComponent<PlayerExplorationUtility>();player.Catalog=c;if(p.GetComponent<NetworkExplorationUtility>()==null)p.AddComponent<NetworkExplorationUtility>();PrefabUtility.SaveAsPrefabAsset(p,netPath);PrefabUtility.UnloadPrefabContents(p);
            p=PrefabUtility.LoadPrefabContents(MultiplayerSceneBuilder.SessionPrefabPath);p.GetComponent<NetworkManager>().NetworkConfig.ProtocolVersion=NetworkProtocol.Current;PrefabUtility.SaveAsPrefabAsset(p,MultiplayerSceneBuilder.SessionPrefabPath);PrefabUtility.UnloadPrefabContents(p);
            foreach(var path in new[]{GameplaySceneBuilder.ScenePath,MultiplayerSceneBuilder.ScenePath})
            {
                var scene=EditorSceneManager.OpenScene(path);
                foreach(var inventory in UnityEngine.Object.FindObjectsByType<PlayerInventory>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var component=inventory.GetComponent<PlayerExplorationUtility>()??inventory.gameObject.AddComponent<PlayerExplorationUtility>();component.Catalog=c;}
                foreach(var panel in UnityEngine.Object.FindObjectsByType<CraftingPanel>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {var cards=new List<RecipeCardView>(panel.Cards);foreach(var r in added)if(!cards.Exists(card=>card.Recipe==r)){var card=UnityEngine.Object.Instantiate(panel.Cards[0],panel.Grids[(int)RecipeCategory.Tools].transform);card.Recipe=r;card.Crafting=panel.Owner;cards.Add(card);}var sorted=RecipePresentation.Sorted(panel.Owner.Catalog);cards.Sort((a,b)=>Array.IndexOf(sorted,a.Recipe).CompareTo(Array.IndexOf(sorted,b.Recipe)));panel.Cards=cards.ToArray();foreach(var card in panel.Cards)card.transform.SetAsLastSibling();}EditorSceneManager.SaveScene(scene);
            }
            EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);AssetDatabase.SaveAssets();
        }
    }
}
