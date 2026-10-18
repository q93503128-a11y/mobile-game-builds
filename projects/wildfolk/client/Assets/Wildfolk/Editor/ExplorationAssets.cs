using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Wildfolk.Presentation;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    public static class ExplorationAssets
    {
        private static T Data<T>(string name)where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        private static ItemAmount Cost(string item,int count)=>new ItemAmount{Item=Data<ContentCatalog>("ContentCatalog").Item(item),Count=count};
        public static void BuildBatch()
        {
            VisualArtImporter.Import();WorldArt.SpriteMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Wildfolk/Art/WorldSprite.mat");
            var light=GameplayAssets.Asset<GameplayLightDefinition>("TorchLight",d=>{d.Radius=7.5f;d.Strength=.9f;d.Color=new Color(1,.69f,.34f);d.DetersEnemies=false;});
            var torch=GameplayAssets.Asset<ItemDefinition>("Torch",i=>{i.Id="torch";i.DisplayName="Torch";i.Category=ItemCategory.Placeable;i.MaxStack=50;i.Icon=VisualArtImporter.Sprite("torch");i.HeldSprite=VisualArtImporter.Sprite("torch");i.PortableLight=light;});
            var compass=GameplayAssets.Asset<ItemDefinition>("Compass",i=>{i.Id="compass";i.DisplayName="Compass";i.Category=ItemCategory.Tool;i.MaxStack=1;i.Icon=VisualArtImporter.Sprite("compass");i.NavigationInstrument=true;});
            var vitality=GameplayAssets.Asset<VitalityDefinition>("TorchVitality",v=>v.MaximumHealth=40);
            var place=GameplayAssets.Asset<PlaceableDefinition>("TorchPlaceable",p=>{p.Item=torch;p.Vitality=vitality;p.Footprint=new Vector2(.5f,.5f);p.MaximumDistance=3.5f;p.Station=StationKind.None;p.WorkbenchTier=1;});torch.Placement=place;EditorUtility.SetDirty(torch);
            GameplayActorBuilder.Facility(place,AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/WorldPickup.prefab").GetComponent<WorldPickup>(),Data<GameplaySettings>("GameplaySettings"));
            var placed=PrefabUtility.LoadPrefabContents("Assets/Wildfolk/Prefabs/Torch.prefab");placed.GetComponent<WorldPlaceable>().Visual.sprite=VisualArtImporter.Sprite("torch_placed");AddFirelight(placed.transform,light);PrefabUtility.SaveAsPrefabAsset(placed,"Assets/Wildfolk/Prefabs/Torch.prefab");PrefabUtility.UnloadPrefabContents(placed);
            var catalog=Data<ContentCatalog>("ContentCatalog");var items=new List<ItemDefinition>(catalog.Items);foreach(var i in new[]{torch,compass})if(!items.Contains(i))items.Add(i);catalog.Items=items.ToArray();
            var recipes=new List<RecipeDefinition>(Data<RecipeCatalog>("Recipes").Recipes);var content=new List<RecipeDefinition>(catalog.Recipes);
            foreach(var r in new[]{Recipe("Torch",torch,4,StationKind.None,new[]{Cost("wood",1),Cost("coal",1)}),Recipe("Compass",compass,1,StationKind.Workbench,new[]{Cost("wood",2),Cost("iron_ingot",1)})}){if(!recipes.Contains(r))recipes.Add(r);if(!content.Contains(r))content.Add(r);}
            var recipeCatalog=Data<RecipeCatalog>("Recipes");recipeCatalog.Recipes=recipes.ToArray();catalog.Recipes=content.ToArray();EditorUtility.SetDirty(catalog);EditorUtility.SetDirty(recipeCatalog);
            foreach(var path in new[]{"Assets/Wildfolk/Prefabs/Player.prefab",MultiplayerSceneBuilder.PlayerPrefabPath})
            {
                var player=PrefabUtility.LoadPrefabContents(path);ConfigurePlayer(player,compass);if(player.GetComponent<NetworkPlayer>()!=null && player.GetComponent<NetworkNavigation>()==null)player.AddComponent<NetworkNavigation>();PrefabUtility.SaveAsPrefabAsset(player,path);PrefabUtility.UnloadPrefabContents(player);
            }
            ConfigureNetworkTorch();
            var session=PrefabUtility.LoadPrefabContents(MultiplayerSceneBuilder.SessionPrefabPath);session.GetComponent<NetworkManager>().NetworkConfig.ProtocolVersion=NetworkProtocol.Current;PrefabUtility.SaveAsPrefabAsset(session,MultiplayerSceneBuilder.SessionPrefabPath);PrefabUtility.UnloadPrefabContents(session);
            foreach(var path in new[]{GameplaySceneBuilder.ScenePath,MultiplayerSceneBuilder.ScenePath})
            {
                var scene=EditorSceneManager.OpenScene(path);var world=UnityEngine.Object.FindFirstObjectByType<WorldGenerator>(FindObjectsInactive.Include);
                var camera=UnityEngine.Object.FindFirstObjectByType<Camera>();var global=Array.Find(UnityEngine.Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None),l=>l.lightType==Light2D.LightType.Global);
                var lighting=UnityEngine.Object.FindFirstObjectByType<WorldLighting>()??global.gameObject.AddComponent<WorldLighting>();lighting.Global=global;lighting.World=world;lighting.Clock=world.GetComponent<WorldClock>();
                var actor=UnityEngine.Object.FindFirstObjectByType<PlayerEquipment>();
                if(actor!=null){ConfigurePlayer(actor.gameObject,compass);lighting.Player=actor.transform;}
                var safe=UnityEngine.Object.FindFirstObjectByType<SafeAreaLayout>().transform;var layer=safe.Find("Gameplay Layer")??safe;
                if(UnityEngine.Object.FindFirstObjectByType<NavigationHud>(FindObjectsInactive.Include)==null)BuildNavigation(layer,actor==null ? null : actor.GetComponent<PlayerNavigation>(),camera);
                foreach(var panel in UnityEngine.Object.FindObjectsByType<CraftingPanel>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {
                    // Preserve existing card objects and their modal/drag references.
                    if(panel.HomeButton==null)panel.HomeButton=QualityUiBuilder.Action(panel.Title.transform.parent,"Set Home","ui.navigation.set_home",new Vector2(310,320),new Vector2(350,62),panel.SetHome,out panel.HomeLabel);
                    var cards=new List<RecipeCardView>();
                    foreach(var recipe in RecipePresentation.Sorted(panel.Owner.Catalog))
                    {
                        var view=Array.Find(panel.Cards,c=>c.Recipe==recipe);
                        if(view==null){var clone=UnityEngine.Object.Instantiate(panel.Cards[0].gameObject,panel.Grids[(int)recipe.Category].transform);clone.name="Recipe";view=clone.GetComponent<RecipeCardView>();view.Recipe=recipe;view.Crafting=panel.Owner;}
                        view.transform.SetAsLastSibling();cards.Add(view);
                    }
                    panel.Cards=cards.ToArray();
                }
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);
        }
        private static RecipeDefinition Recipe(string name,ItemDefinition item,int count,StationKind station,ItemAmount[] cost)
        {return GameplayAssets.Asset<RecipeDefinition>(name+"Recipe",r=>{r.Id=item.Id;r.Result=item;r.ResultCount=count;r.Ingredients=cost;r.RequiredStation=station;r.RequiredWorkbenchTier=1;r.Category=RecipeCategory.Survival;r.SortOrder=item.Id=="torch" ? 30 : 40;});}
        internal static void ConfigurePlayer(GameObject actor,ItemDefinition compass)
        {
            var navigation=actor.GetComponent<PlayerNavigation>()??actor.AddComponent<PlayerNavigation>();navigation.Compass=compass;
            var equipment=actor.GetComponent<PlayerEquipment>();var source=actor.GetComponent<PlayerLightSource>();
            if(source==null){source=actor.AddComponent<PlayerLightSource>();source.Equipment=equipment;var emitter=AddFirelight(actor.transform,null);source.Gameplay=emitter.GetComponent<GameplayLight>();source.Presentation=emitter;}
            source.Presentation.transform.SetParent(actor.transform,false);
            if(actor.transform.Find("Near visibility")==null){var near=new GameObject("Near visibility");near.transform.SetParent(actor.transform,false);var light=near.AddComponent<Light2D>();light.lightType=Light2D.LightType.Point;light.pointLightOuterRadius=2.8f;light.pointLightInnerRadius=.75f;light.color=new Color(.62f,.7f,.8f);light.intensity=.28f;}
            var floor=actor.transform.Find("Near visibility");var falloff=floor.GetComponent<LocalFirelight>()??floor.gameObject.AddComponent<LocalFirelight>();falloff.VisibilityFloor=true;falloff.Light=floor.GetComponent<Light2D>();
        }
        private static LocalFirelight AddFirelight(Transform parent,GameplayLightDefinition definition)
        {
            var go=new GameObject("Warm light");go.transform.SetParent(parent,false);var light=go.AddComponent<Light2D>();light.lightType=Light2D.LightType.Point;light.intensity=0;light.shadowIntensity=0;
            var gameplay=go.AddComponent<GameplayLight>();gameplay.Definition=definition;
            var presentation=go.AddComponent<LocalFirelight>();presentation.Light=light;presentation.Definition=definition;return presentation;
        }
        private static void ConfigureNetworkTorch()
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/Torch.prefab");var go=(GameObject)PrefabUtility.InstantiatePrefab(source);go.AddComponent<NetworkObject>();go.AddComponent<NetworkBuilding>();
            var prefab=PrefabUtility.SaveAsPrefabAsset(go,"Assets/Wildfolk/Prefabs/NetworkTorch.prefab");UnityEngine.Object.DestroyImmediate(go);
            var ecology=PrefabUtility.LoadPrefabContents(NetworkEcologyBuilder.PrefabPath);var world=ecology.GetComponent<NetworkCreatureWorld>();var entries=new List<NetworkSpawnEntry>(world.Prefabs);entries.RemoveAll(e=>e.Source==source);entries.Add(new NetworkSpawnEntry{Source=source,NetworkPrefab=prefab});world.Prefabs=entries.ToArray();PrefabUtility.SaveAsPrefabAsset(ecology,NetworkEcologyBuilder.PrefabPath);PrefabUtility.UnloadPrefabContents(ecology);
        }
        internal static void BuildNavigation(Transform parent,PlayerNavigation player,Camera camera)
        {
            var root=Rect("Navigation",parent,new Vector2(0,1),new Vector2(166,-300),new Vector2(270,74));
            var hud=root.gameObject.AddComponent<NavigationHud>();hud.Player=player;hud.Camera=camera;
            var view=Rect("Bearing",root,new Vector2(.5f,.5f),Vector2.zero,new Vector2(270,74));hud.View=view.gameObject;
            var dial=Image("North Dial",view,"compass",new Vector2(.5f,.5f),new Vector2(-94,0),new Vector2(62,62));hud.North=(RectTransform)dial.transform;
            Text("North",dial.transform,"N",new Vector2(0,22),new Vector2(20,18),14);
            var arrow=Image("Home Direction",view,"bearing_arrow",new Vector2(.5f,.5f),new Vector2(-40,0),new Vector2(26,36));hud.HomeArrow=(RectTransform)arrow.transform;
            hud.Destination=Text("Destination",view,"",new Vector2(49,0),new Vector2(142,64),20,TextAnchor.MiddleLeft);hud.Destination.horizontalOverflow=HorizontalWrapMode.Wrap;hud.View.SetActive(false);
        }
    }
}
