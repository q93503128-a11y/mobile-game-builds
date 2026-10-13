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
        private static GameplayAssets assets;
        private static PlayerInventory player;
        private static WorldPickup pickup;

        [MenuItem("Wildfolk/Build Playable Scene")]
        public static void Build()
        {
            if(EditorApplication.isPlaying) throw new System.InvalidOperationException("Stop Play Mode before rebuilding.");
            if(SceneManager.GetActiveScene().isDirty) throw new System.InvalidOperationException("Save the current scene before rebuilding.");
            Directory.CreateDirectory("Assets/Wildfolk/Scenes");Directory.CreateDirectory("Assets/Wildfolk/Prefabs");
            VisualArtImporter.Import();assets=GameplayAssets.Load();
            WorldArt.SpriteMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Wildfolk/Art/WorldSprite.mat");
            if(WorldArt.SpriteMaterial==null) throw new System.InvalidOperationException("World sprite material is required.");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var session=new GameObject("Game Session").AddComponent<GameplaySession>();
            session.gameObject.AddComponent<WorldPauseController>().Session=session;
            GameplayActorBuilder.Workbench(assets.Crafting.Workbench);
            var actors=new GameObject("Actors").transform;
            var playerObject=GameplayActorBuilder.Player(actors,assets);
            player=playerObject.GetComponent<PlayerInventory>();
            var motor=playerObject.GetComponent<PlayerMotor>();motor.Session=session;
            var combat=playerObject.GetComponent<PlayerCombat>();combat.Session=session;
            playerObject.GetComponent<PlayerAction>().Session=session;
            playerObject.GetComponent<PlayerPlacement>().Session=session;
            playerObject.GetComponent<PlayerCrafting>().Session=session;
            session.PlayerHealth=playerObject.GetComponent<Health>();
            var enemyObject=GameplayActorBuilder.Enemy(actors,assets);
            var enemy=enemyObject.GetComponent<EnemyController>();enemy.Session=session;enemy.PlayerHealth=session.PlayerHealth;
            pickup=GameplayActorBuilder.Pickup();
            var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();
            camera.tag="MainCamera";camera.transform.position=new Vector3(0,-.35f,-10);
            camera.orthographic=true;camera.orthographicSize=9;camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color32(106,126,73,255);camera.GetUniversalAdditionalCameraData().renderPostProcessing=false;
            var light=new GameObject("Global Light 2D").AddComponent<Light2D>();light.lightType=Light2D.LightType.Global;light.intensity=1;
            var terrain=new GameObject("Terrain").transform;
            var ground=Sprite("Forest Floor","ground",terrain,Vector2.zero,1,-1000);
            GameplayActorBuilder.PlacementPreview(playerObject.GetComponent<PlayerPlacement>(),actors,ground);
            var follow=camera.gameObject.AddComponent<CameraFollow>();follow.Target=playerObject.transform;follow.Ground=ground;follow.Settings=assets.Settings;
            Boundaries(terrain);
            var scenery=new GameObject("Environment").transform;
            Tree(scenery,-12.7f,6.4f,1.3f,false);Tree(scenery,-8.4f,7.4f,1.06f,true);
            Tree(scenery,-10.1f,3.5f,1.05f,false);Tree(scenery,-3.0f,1.8f,.9f,false);
            Tree(scenery,3.3f,7.6f,1.16f,true);Tree(scenery,11.2f,5.3f,1.35f,false);
            Tree(scenery,14.3f,1.9f,.96f,true);Tree(scenery,8.1f,-5.7f,1.23f,false);Tree(scenery,-8.3f,-6.0f,1.13f,true);
            Rock(scenery,2.8f,1.4f,1.02f,false);Rock(scenery,-6.5f,1.5f,.60f,true);
            Rock(scenery,6.8f,3.4f,.78f,true);Rock(scenery,12.2f,-3.0f,1.2f,false);
            Rock(scenery,10.5f,-3.9f,.6f,true);Rock(scenery,-13.5f,-2.1f,.8f,false);
            var log=Sprite("Fallen Log","fallen_log",scenery,new Vector2(-3.8f,-4),.95f,180);
            log.transform.rotation=Quaternion.Euler(0,0,15);
            var random=new System.Random(947);
            for(var i=0;i<42;i++)
            {
                var x=(float)random.NextDouble()*30-15;var y=(float)random.NextDouble()*16-8;
                if(Mathf.Abs(x)<3.3f && Mathf.Abs(y)<2.6f) continue;
                var type=i%4==0 ? "fern" : "grass";
                Sprite(type=="fern" ? "Fern" : "Grass Tuft",type,scenery,new Vector2(x,y),.42f+(float)random.NextDouble()*.25f,100-Mathf.RoundToInt(y*20));
            }
            GameplayHudBuilder.Build(assets,session,motor,player);
            RecordInstance(playerObject);RecordInstance(enemyObject);
            PlayerSettings.productName="Wildfolk";PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
            PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;
            PlayerSettings.runInBackground=true;
            EditorSceneManager.SaveScene(scene,ScenePath);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            AssetDatabase.SaveAssets();
        }

        private static void Tree(Transform parent,float x,float y,float scale,bool olive)
        {
            var root=Group(olive ? "Olive Oak" : "Forest Oak",parent,x,y);root.localScale=Vector3.one*scale;
            Shadow(root,3.15f,.9f);
            var visual=Sprite("Trunk and Canopy",olive ? "tree_olive" : "tree_oak",root,new Vector2(0,.82f),1,100-Mathf.RoundToInt(y*20));
            var collider=root.gameObject.AddComponent<CircleCollider2D>();collider.radius=.66f;collider.offset=new Vector2(0,-.30f);
            Resource(root,visual.transform,assets.Tree,HitReaction.Tree,3.0f);
        }

        private static void Rock(Transform parent,float x,float y,float scale,bool split)
        {
            var root=Group("Stone Outcrop",parent,x,y);root.localScale=Vector3.one*scale;
            Shadow(root,2.1f,.62f);
            var visual=Sprite("Stone",split ? "rock_split" : "rock_wide",root,Vector2.zero,1,100-Mathf.RoundToInt(y*20));
            root.gameObject.AddComponent<CircleCollider2D>().radius=.72f;
            Resource(root,visual.transform,assets.Stone,HitReaction.Stone,1.16f);
        }

        private static void Resource(Transform root,Transform visual,ResourceDefinition definition,HitReaction reaction,float barHeight)
        {
            var health=root.gameObject.AddComponent<Health>();health.Definition=definition.Vitality;
            var resource=root.gameObject.AddComponent<ResourceNode>();resource.Health=health;resource.Definition=definition;
            resource.Player=player;resource.PickupPrefab=pickup;resource.Settings=assets.Settings;
            Feedback(health,visual,reaction);HealthBar(health,barHeight,false);
        }

        private static void Boundaries(Transform parent)
        {
            var walls=Group("Field Bounds",parent);
            Wall(walls,"North",0,13.4f,48,1);Wall(walls,"South",0,-13.4f,48,1);
            Wall(walls,"West",-23.9f,0,1,27);Wall(walls,"East",23.9f,0,1,27);
        }
        private static void Wall(Transform parent,string name,float x,float y,float width,float height)
        { var collider=Group(name,parent,x,y).gameObject.AddComponent<BoxCollider2D>();collider.size=new Vector2(width,height); }

        private static void RecordInstance(GameObject instance)
        {
            foreach(var component in instance.GetComponentsInChildren<Component>(true))
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        }
    }
}
