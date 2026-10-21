using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Wildfolk.Editor
{
    public static class CreatureSidegradeAssets
    {
        private static T D<T>(string n)where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+n+".asset");
        private static T Make<T>(string name,Action<T> assign)where T:ScriptableObject
        {var d=GameplayAssets.Asset<T>(name,assign);assign(d);EditorUtility.SetDirty(d);return d;}
        public static void Build()
        {
            VisualArtImporter.Import();var c=D<ContentCatalog>("ContentCatalog");
            var tool=Make<ToolDefinition>("HuntersSpearTool",t=>{t.Kind=ToolKind.Spear;t.Tier=D<EquipmentTier>("SteelTier");t.HeldSprite=VisualArtImporter.Sprite("hunters_spear_held");t.Damage=27;t.Duration=.63f;t.Cooldown=.90f;t.Attack=new AttackProfile{Range=3.35f,ArcAngle=24,MaxTargets=1};t.HandExtension=1.52f;t.SwingArc=1;t.Efficiencies=new[]{new ResourceEfficiency{Kind=ResourceKind.Wood,Multiplier=.25f},new ResourceEfficiency{Kind=ResourceKind.Stone,Multiplier=.25f}};});
            var item=Make<ItemDefinition>("HuntersSpear",d=>{d.Id="hunters_spear";d.DisplayName="Hunter's Spear";d.MaxStack=1;d.Category=ItemCategory.Weapon;d.Icon=VisualArtImporter.Sprite("hunters_spear");d.Tool=tool;d.HeldSprite=tool.HeldSprite;});
            var r=Make<RecipeDefinition>("HuntersSpearRecipe",d=>{d.Id=item.Id;d.Result=item;d.ResultCount=1;d.RequiredStation=StationKind.Workbench;d.RequiredWorkbenchTier=2;d.Category=RecipeCategory.Weapons;d.Tier=tool.Tier;d.SortOrder=680;d.Ingredients=new[]{new ItemAmount{Item=c.Item("iron_spear"),Count=1},new ItemAmount{Item=c.Item("bone"),Count=6},new ItemAmount{Item=c.Item("leather"),Count=2}};});
            var items=new List<ItemDefinition>(c.Items);if(!items.Contains(item))items.Add(item);c.Items=items.ToArray();var recipes=new List<RecipeDefinition>(c.Recipes);if(!recipes.Contains(r))recipes.Add(r);c.Recipes=recipes.ToArray();D<RecipeCatalog>("Recipes").Recipes=c.Recipes;EditorUtility.SetDirty(c);EditorUtility.SetDirty(D<RecipeCatalog>("Recipes"));AssetDatabase.SaveAssets();
            foreach(var path in new[]{GameplaySceneBuilder.ScenePath,MultiplayerSceneBuilder.ScenePath})
            {
                var scene=EditorSceneManager.OpenScene(path);
                foreach(var panel in UnityEngine.Object.FindObjectsByType<CraftingPanel>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {
                    var cards=new List<RecipeCardView>(panel.Cards);
                    if(!cards.Exists(card=>card.Recipe==r)){var card=UnityEngine.Object.Instantiate(panel.Cards[0],panel.Grids[(int)RecipeCategory.Weapons].transform);card.Recipe=r;card.Crafting=panel.Owner;cards.Add(card);}
                    var sorted=RecipePresentation.Sorted(panel.Owner.Catalog);cards.Sort((a,b)=>Array.IndexOf(sorted,a.Recipe).CompareTo(Array.IndexOf(sorted,b.Recipe)));panel.Cards=cards.ToArray();foreach(var card in panel.Cards)card.transform.SetAsLastSibling();
                }
                EditorSceneManager.SaveScene(scene);
            }
            EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);AssetDatabase.SaveAssets();
        }
    }
}
