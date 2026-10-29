using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using static Wildfolk.Editor.WorldArt;
namespace Wildfolk.Editor
{
    public static class GameplaySceneBuilder
    {
        public const string ScenePath="Assets/Wildfolk/Scenes/Wildfolk.unity";
        [MenuItem("Wildfolk/Build Playable Scene")]
        public static void Build()
        {
            if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play Mode before rebuilding.");
            if(SceneManager.GetActiveScene().isDirty)throw new System.InvalidOperationException("Save the current scene before rebuilding.");
            Directory.CreateDirectory("Assets/Wildfolk/Scenes");Directory.CreateDirectory("Assets/Wildfolk/Prefabs");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            VisualArtImporter.Import();
            var assets=GameplayAssets.Load();SpriteMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Wildfolk/Art/WorldSprite.mat");
            SpriteMaterial.shader=Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");EditorUtility.SetDirty(SpriteMaterial);
            var session=new GameObject("Game Session").AddComponent<GameplaySession>();session.gameObject.AddComponent<WorldPauseController>().Session=session;
            session.gameObject.AddComponent<PrefabPool>();var saves=session.gameObject.AddComponent<WorldSaveService>();
            var clock=session.gameObject.AddComponent<WorldClock>();clock.Definition=assets.Progression.DayNight;clock.Session=session;
            var preferences=session.gameObject.AddComponent<UserPreferences>();var audio=session.gameObject.AddComponent<GameAudio>();audio.Preferences=preferences;
            EcologyAssets.Load(assets);var pickup=GameplayActorBuilder.Pickup();WorldContentBuilder.Build(assets,pickup);QualityAssets.Configure(assets);
            foreach(var facility in new[]{assets.Crafting.Workbench,assets.Survival.Campfire,assets.Progression.Furnace,assets.Progression.Wall,assets.Progression.Door,assets.Progression.Chest})GameplayActorBuilder.Facility(facility,pickup,assets.Settings);
            foreach(var facility in CharacterPassAssets.Load(assets))GameplayActorBuilder.Facility(facility,pickup,assets.Settings);
            var actors=new GameObject("Actors").transform;var playerObject=GameplayActorBuilder.Player(actors,assets);var player=playerObject.GetComponent<PlayerInventory>();
            var motor=playerObject.GetComponent<PlayerMotor>();motor.Session=session;playerObject.GetComponent<PlayerCombat>().Session=session;
            playerObject.GetComponent<PlayerInputCommands>().Session=session;playerObject.GetComponent<PlayerPrimaryAction>().Session=session;playerObject.GetComponent<PlayerInteraction>().Session=session;
            playerObject.GetComponent<PlayerSurvival>().Session=session;playerObject.GetComponent<PlayerPlacement>().Session=session;playerObject.GetComponent<PlayerCrafting>().Session=session;playerObject.GetComponent<PlayerStorage>().Session=session;
            session.PlayerHealth=playerObject.GetComponent<Health>();
            var enemyObject=GameplayActorBuilder.Enemy(null,assets);Object.DestroyImmediate(enemyObject);
            EcologyContentBuilder.Skeleton();
            var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.tag="MainCamera";camera.transform.position=new Vector3(0,0,-10);camera.orthographic=true;camera.orthographicSize=9;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color32(106,126,73,255);camera.GetUniversalAdditionalCameraData().renderPostProcessing=false;
            var light=new GameObject("Global Light 2D").AddComponent<Light2D>();light.lightType=Light2D.LightType.Global;light.intensity=1;
            var lighting=session.gameObject.AddComponent<WorldLighting>();lighting.Clock=clock;lighting.Global=light;
            var terrain=new GameObject("Terrain").transform;var region=Group("Spawn Region",terrain).gameObject.AddComponent<WorldSpawnRegion>();region.Size=assets.World.Generation.Size-Vector2.one*3;
            var ground=Sprite("Forest Floor","ground",terrain,Vector2.zero,1,WorldSorting.Ground);
            var size=ground.sprite.bounds.size;ground.transform.localScale=new Vector3(assets.World.Generation.Size.x/size.x,assets.World.Generation.Size.y/size.y,1);
            GameplayActorBuilder.PlacementPreview(playerObject.GetComponent<PlayerPlacement>(),actors,ground);
            var follow=camera.gameObject.AddComponent<CameraFollow>();follow.Target=playerObject.transform;follow.Ground=ground;follow.Settings=assets.Settings;
            var world=session.gameObject.AddComponent<WorldGenerator>();world.Definition=assets.World.Generation;world.Region=region;world.Environment=new GameObject("Generated Environment").transform;world.Player=player;world.Session=session;world.Pickup=pickup;world.Settings=assets.Settings;world.Water=assets.Survival.Source;world.SpriteMaterial=SpriteMaterial;world.Grass=VisualArtImporter.Sprite("grass");world.Fern=VisualArtImporter.Sprite("fern");world.PoiSprites=WorldFoundationAssets.PoiArt();
            world.Terrain=terrain.gameObject.AddComponent<WorldTerrainRenderer>();world.Terrain.Ground=ground;
            var navigation=session.gameObject.AddComponent<WorldNavigation>();navigation.World=world;navigation.Camera=camera;navigation.Player=player.transform;motor.Terrain=navigation;playerObject.GetComponent<PlayerPlacement>().Terrain=navigation;
            var renewal=session.gameObject.AddComponent<ResourceRegeneration>();renewal.World=world;renewal.Clock=clock;renewal.Navigation=navigation;world.Renewal=renewal;
            var animals=session.gameObject.AddComponent<AnimalPopulation>();animals.World=world;animals.Clock=clock;animals.Navigation=navigation;animals.Definition=StabilityAssets.Animals();world.Animals=animals;
            saves.World=world;saves.Clock=clock;saves.Player=player;saves.Catalog=StabilityAssets.Catalog();
            var drop=playerObject.GetComponent<PlayerItemDrop>();drop.Session=session;drop.BindWorld(world);
            var spawner=session.gameObject.AddComponent<ZombieSpawner>();spawner.Definition=assets.Progression.Spawn;spawner.Prefab=Prefab<EnemyController>("Enemy");spawner.Clock=clock;spawner.Session=session;spawner.Player=session.PlayerHealth;spawner.Camera=camera;spawner.Region=region;spawner.World=world;
            spawner.SecondaryPrefab=Prefab<EnemyController>("Skeleton");
            var goblins=session.gameObject.AddComponent<GoblinSpawner>();goblins.Definition=assets.World.Goblin;goblins.Prefab=Prefab<GoblinController>("Goblin");goblins.Clock=clock;goblins.Session=session;goblins.Player=session.PlayerHealth;goblins.Camera=camera;goblins.World=world;
            goblins.RangedPrefab=Prefab<GoblinController>("SlingshotGoblin");goblins.CampDefinition=EcologyAssets.Data<GoblinCampDefinition>("GoblinCamp");
            GameplayHudBuilder.Build(assets,session,motor,player);
            foreach(var component in playerObject.GetComponentsInChildren<Component>(true))PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            PlayerSettings.productName="Wildfolk";PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;PlayerSettings.runInBackground=true;
            EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();FrontendSceneBuilder.Build();
        }
        private static T Prefab<T>(string name) where T:Component=>AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/"+name+".prefab").GetComponent<T>();
    }
}
