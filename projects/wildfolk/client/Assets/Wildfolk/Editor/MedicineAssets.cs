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
    public static class MedicineAssets
    {
        private static T D<T>(string name)where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        private static T Make<T>(string name,Action<T> assign)where T:ScriptableObject
        {var value=GameplayAssets.Asset<T>(name,assign);assign(value);EditorUtility.SetDirty(value);return value;}
        public static void Build()
        {
            VisualArtImporter.Import();WorldArt.SpriteMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Wildfolk/Art/WorldSprite.mat");
            var catalog=D<ContentCatalog>("ContentCatalog");var items=new List<ItemDefinition>(catalog.Items);var recipes=new List<RecipeDefinition>(catalog.Recipes);
            var herb=Make<ItemDefinition>("MedicinalHerb",d=>{d.Id="medicinal_herb";d.DisplayName="Medicinal Herb";d.MaxStack=50;d.Category=ItemCategory.Material;d.Icon=VisualArtImporter.Sprite("medicinal_herb");});
            var dressing=Make<ItemDefinition>("FieldDressing",d=>{d.Id="field_dressing";d.DisplayName="Field Dressing";d.MaxStack=20;d.Category=ItemCategory.Consumable;d.Icon=VisualArtImporter.Sprite("field_dressing");});
            dressing.Consumable=Make<ConsumableDefinition>("FieldDressingMedicine",d=>{d.Kind=ConsumableKind.Medicine;d.HealthRestore=0;d.GradualHealthRestore=20;d.HealingDuration=8;d.UseCooldownSeconds=4;});
            var fiber=catalog.Item("plant_fiber");var gland=catalog.Item("medicinal_gland");
            var salve=catalog.Item("healing_salve").Consumable;salve.Kind=ConsumableKind.Medicine;salve.HealthRestore=30;salve.UseCooldownSeconds=4;EditorUtility.SetDirty(salve);
            var salveRecipe=catalog.Recipe("healing_salve");salveRecipe.Ingredients=new[]{new ItemAmount{Item=gland,Count=1},new ItemAmount{Item=herb,Count=1},new ItemAmount{Item=fiber,Count=2}};EditorUtility.SetDirty(salveRecipe);
            var recipe=Make<RecipeDefinition>("FieldDressingRecipe",d=>{d.Id="field_dressing";d.Result=dressing;d.ResultCount=1;d.RequiredStation=StationKind.None;d.RequiredWorkbenchTier=1;d.Category=RecipeCategory.Survival;d.SortOrder=645;d.Ingredients=new[]{new ItemAmount{Item=fiber,Count=2},new ItemAmount{Item=herb,Count=1}};});
            foreach(var item in new[]{herb,dressing})if(!items.Contains(item))items.Add(item);if(!recipes.Contains(recipe))recipes.Add(recipe);
            catalog.Items=items.ToArray();catalog.Recipes=recipes.ToArray();D<RecipeCatalog>("Recipes").Recipes=recipes.ToArray();EditorUtility.SetDirty(catalog);EditorUtility.SetDirty(D<RecipeCatalog>("Recipes"));
            var vitality=Make<VitalityDefinition>("MedicinalHerbVitality",d=>d.MaximumHealth=12);
            var resource=Make<ResourceDefinition>("MedicinalHerbResource",d=>{d.Kind=ResourceKind.MedicinalHerb;d.DropItem=herb;d.DropCount=2;d.Vitality=vitality;d.Harvest=new HarvestDefinition{Mode=HarvestMode.Gather,Method=HarvestMethod.Strike};});
            var pickup=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/WorldPickup.prefab").GetComponent<WorldPickup>();
            var prefab=WorldContentBuilder.Resource("MedicinalHerb","medicinal_herb_patch",resource,Vector2.zero,.4f,.75f,HitReaction.Tree,pickup,D<GameplaySettings>("GameplaySettings"),nonBlocking:true);
            var world=AssetDatabase.LoadAssetAtPath<SaveGameRules>("Assets/Wildfolk/Resources/SaveGameRules.asset").World;
            var rules=new List<ResourceGenerationRule>(world.Resources);var index=rules.FindIndex(r=>r.Prefab==prefab);
            if(index<0){index=rules.Count;rules.Add(new ResourceGenerationRule{Prefab=prefab,Count=12,IndependentGeneration=true,GenerationSalt=12317,DistributionConfigured=true,BiomeAffinity=1,ForestDensityRange=new Vector2(.55f,1),RockDensityRange=new Vector2(0,1),WaterDistanceRange=new Vector2(1.5f,1000),RoadDistanceRange=new Vector2(3,1000),DepthRange=new Vector2(0,1),SpawnClearance=8,StartRadius=1,ScaleRange=new Vector2(.9f,1.1f),Family=ResourceFamily.Plant,Rarity=ResourceRarity.Uncommon,RecoveryConfigured=true,NaturalRecovery=true,RespawnSeconds=240,Spacing=6,ClusterSpacing=3,TargetDensityRatio=1,HarvestExclusionRadius=6,HarvestExclusionSeconds=480});}
            world.Resources=rules.ToArray();
            var source=Make<NaturalSourceDefinition>("MedicinalHerbSurfaceSource",d=>{d.SourceId="medicinal_herb_surface";d.ResourceRule=index;d.Context=ResourceSourceContext.Surface;d.AnyTags=EnvironmentTag.ForestFloor|EnvironmentTag.WetGround;d.SurfaceBiomes=new[]{SurfaceBiome.Forest,SurfaceBiome.Wetlands};d.CandidatesPerChunk=6;d.Probability=.6f;d.MinimumSpacing=6;d.NaturalRecovery=true;d.VegetationStream=true;d.StarterSupply=0;});
            var sources=new List<NaturalSourceDefinition>(world.NaturalSources);if(!sources.Contains(source))sources.Add(source);world.NaturalSources=sources.ToArray();EditorUtility.SetDirty(world);
            RegisterNetwork(prefab.gameObject);AssetDatabase.SaveAssets();
            foreach(var path in new[]{GameplaySceneBuilder.ScenePath,MultiplayerSceneBuilder.ScenePath})
            {
                var scene=EditorSceneManager.OpenScene(path);
                foreach(var panel in UnityEngine.Object.FindObjectsByType<CraftingPanel>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {
                    var cards=new List<RecipeCardView>(panel.Cards);
                    if(!Array.Exists(panel.Cards,c=>c.Recipe==recipe))
                    {var card=UnityEngine.Object.Instantiate(panel.Cards[0],panel.Grids[(int)RecipeCategory.Survival].transform);card.Recipe=recipe;card.Crafting=panel.Owner;cards.Add(card);}
                    var ordered=RecipePresentation.Sorted(panel.Owner.Catalog);cards.Sort((a,b)=>Array.IndexOf(ordered,a.Recipe).CompareTo(Array.IndexOf(ordered,b.Recipe)));panel.Cards=cards.ToArray();
                    foreach(var card in panel.Cards)card.transform.SetAsLastSibling();
                }
                var treatmentHud=UnityEngine.Object.FindFirstObjectByType<MedicineHud>();
                if(treatmentHud==null)
                {
                    var profile=UnityEngine.Object.FindFirstObjectByType<ProfileHud>();var arena=UnityEngine.Object.FindFirstObjectByType<MultiplayerArena>();
                    var reference=profile!=null ? profile.HealthValue : arena.CampLabel;
                    var label=UnityEngine.Object.Instantiate(reference,reference.transform.parent);label.name="Treatment";label.gameObject.SetActive(true);label.text="";label.enabled=false;label.fontSize=22;label.raycastTarget=false;
                    var hud=label.gameObject.AddComponent<MedicineHud>();hud.Label=label;hud.Player=profile==null ? null : profile.Player.GetComponent<PlayerSurvival>();treatmentHud=hud;
                }
                var safe=UnityEngine.Object.FindFirstObjectByType<SafeAreaLayout>();
                treatmentHud.transform.SetParent(safe.transform,false);var rect=treatmentHud.Label.rectTransform;
                rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(36,-266);rect.sizeDelta=new Vector2(380,36);treatmentHud.Label.alignment=TextAnchor.MiddleLeft;
                EditorSceneManager.SaveScene(scene);
            }
            EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);AssetDatabase.SaveAssets();
        }
        private static void RegisterNetwork(GameObject source)
        {
            var root=UnityEngine.Object.Instantiate(source);root.name="NetworkMedicinalHerb";root.AddComponent<NetworkObject>();root.AddComponent<NetworkResource>();
            var network=PrefabUtility.SaveAsPrefabAsset(root,"Assets/Wildfolk/Prefabs/NetworkMedicinalHerb.prefab");UnityEngine.Object.DestroyImmediate(root);
            var ecology=PrefabUtility.LoadPrefabContents(NetworkEcologyBuilder.PrefabPath);var world=ecology.GetComponent<NetworkCreatureWorld>();var list=new List<NetworkSpawnEntry>(world.Prefabs);list.RemoveAll(e=>e.Source==source);list.Add(new NetworkSpawnEntry{Source=source,NetworkPrefab=network});world.Prefabs=list.ToArray();PrefabUtility.SaveAsPrefabAsset(ecology,NetworkEcologyBuilder.PrefabPath);PrefabUtility.UnloadPrefabContents(ecology);
            var session=PrefabUtility.LoadPrefabContents(MultiplayerSceneBuilder.SessionPrefabPath);session.GetComponent<NetworkManager>().NetworkConfig.ProtocolVersion=NetworkProtocol.Current;PrefabUtility.SaveAsPrefabAsset(session,MultiplayerSceneBuilder.SessionPrefabPath);PrefabUtility.UnloadPrefabContents(session);
        }
    }
}
