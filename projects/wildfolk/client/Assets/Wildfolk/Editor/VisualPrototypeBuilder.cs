using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Wildfolk.Editor
{
    public static class VisualPrototypeBuilder
    {
        private const string ScenePath = "Assets/Wildfolk/Scenes/WildfolkVisual.unity";
        private const string PrefabPath = "Assets/Wildfolk/Prefabs/";
        private static Material spriteMaterial;

        [MenuItem("Wildfolk/Build Visual Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Stop Play Mode before rebuilding.");
            var current = SceneManager.GetActiveScene();
            if (current.isDirty) throw new System.InvalidOperationException("Save the current scene before rebuilding.");
            Directory.CreateDirectory("Assets/Wildfolk/Scenes");
            Directory.CreateDirectory(PrefabPath);
            VisualArtImporter.Import();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Wildfolk/Art/WorldSprite.mat");
            if (!spriteMaterial)
            {
                var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (!shader) throw new System.InvalidOperationException("URP 2D unlit sprite shader is missing.");
                spriteMaterial = new Material(shader);
                AssetDatabase.CreateAsset(spriteMaterial, "Assets/Wildfolk/Art/WorldSprite.mat");
            }

            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0,0,-10);
            camera.orthographic = true;
            camera.orthographicSize = 9;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(106,126,73,255);
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            var light = new GameObject("Global Light 2D").AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1;

            var terrain = new GameObject("Terrain").transform;
            Sprite("Forest Floor", "ground", terrain, Vector2.zero, 1, -1000);
            var scenery = new GameObject("Environment").transform;
            // Open center and intentionally uneven edge clusters.
            Tree(scenery,-12.7f,6.4f,1.3f,false);
            Tree(scenery,-8.4f,7.4f,1.06f,true);
            Tree(scenery,-10.1f,3.5f,1.05f,false);
            Tree(scenery,-5.4f,5.5f,.9f,false);
            Tree(scenery,3.3f,7.6f,1.16f,true);
            Tree(scenery,11.2f,5.3f,1.35f,false);
            Tree(scenery,14.3f,1.9f,.96f,true);
            Tree(scenery,8.1f,-5.7f,1.23f,false);
            Tree(scenery,-8.3f,-6.0f,1.13f,true);
            Rock(scenery,-4.7f,2.2f,1.02f,false);
            Rock(scenery,-6.5f,1.5f,.60f,true);
            Rock(scenery,6.8f,3.4f,.78f,true);
            Rock(scenery,12.2f,-3.0f,1.2f,false);
            Rock(scenery,10.5f,-3.9f,.6f,true);
            Rock(scenery,-13.5f,-2.1f,.8f,false);
            Prop(scenery,"Fallen Log","fallen_log",-3.8f,-4.0f,.95f,15);
            var random = new System.Random(947);
            for (var i=0;i<42;i++)
            {
                var x = (float)random.NextDouble()*30-15;
                var y = (float)random.NextDouble()*16-8;
                if (Mathf.Abs(x)<3.3f && Mathf.Abs(y)<2.6f) continue;
                var type = i%4 == 0 ? "fern" : "grass";
                Prop(scenery,type == "fern" ? "Fern" : "Grass Tuft",type,x,y,.42f+(float)random.NextDouble()*.25f,0);
            }
            var actors = new GameObject("Actors").transform;
            Player(actors);
            Enemy(actors);
            VisualHudBuilder.Build();
            PlayerSettings.productName = "Wildfolk";
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            EditorSceneManager.SaveScene(scene,ScenePath);
            AssetDatabase.SaveAssets();
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath,true) };
            Selection.activeGameObject = camera.gameObject;
            VisualGameView.ShowReference();
        }

        private static SpriteRenderer Sprite(string name, string art, Transform parent, Vector2 position, float scale, int order)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent,false);
            obj.transform.localPosition = position;
            obj.transform.localScale = Vector3.one*scale;
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = VisualArtImporter.Sprite(art);
            renderer.sharedMaterial = spriteMaterial;
            renderer.sortingOrder = order;
            return renderer;
        }

        private static Transform Group(string name, Transform parent, float x, float y)
        {
            var obj = new GameObject(name).transform;
            obj.SetParent(parent,false);
            obj.localPosition = new Vector3(x,y,0);
            return obj;
        }

        private static int Order(float y) => 100-Mathf.RoundToInt(y*20);

        private static void Shadow(Transform parent, float width, float height)
        {
            var shadow = Sprite("Contact Shadow","contact_shadow",parent,new Vector2(.14f,-.35f),1,-900);
            shadow.transform.localScale = new Vector3(width/2.56f,height/1.28f,1);
        }

        private static void Tree(Transform parent, float x, float y, float scale, bool olive)
        {
            var root = Group(olive ? "Olive Oak" : "Forest Oak",parent,x,y);
            root.localScale = Vector3.one*scale;
            Shadow(root,3.15f,.9f);
            Sprite("Trunk and Canopy",olive ? "tree_olive" : "tree_oak",root,new Vector2(0,.82f),1,Order(y));
        }

        private static void Rock(Transform parent,float x,float y,float scale,bool split)
        {
            var root=Group("Stone Outcrop",parent,x,y);
            root.localScale=Vector3.one*scale;
            Shadow(root,2.1f,.62f);
            Sprite("Stone",split ? "rock_split" : "rock_wide",root,Vector2.zero,1,Order(y));
        }

        private static void Prop(Transform parent,string name,string art,float x,float y,float scale,float angle)
        {
            var renderer = Sprite(name,art,parent,new Vector2(x,y),scale,Order(y));
            renderer.transform.rotation = Quaternion.Euler(0,0,angle);
        }

        private static void Player(Transform parent)
        {
            var root = Group("Player",parent,0,-.35f);
            Shadow(root,1.75f,.55f);
            Sprite("Body","player_body",root,Vector2.zero,1.0f,110);
            Sprite("Left Hand","player_hand",root,new Vector2(-.85f,-.3f),.78f,111);
            Sprite("Right Hand","player_hand",root,new Vector2(.85f,-.3f),.78f,111);
            Group("Equipment Socket",root,.89f,-.3f);
            Group("Headwear Socket",root,0,.45f);
            var hp = Sprite("Health Track","white",root,new Vector2(0,1.13f),1,500);
            hp.transform.localScale=new Vector3(1.45f/.08f,.15f/.08f,1);
            hp.color=new Color32(32,46,35,255);
            var fill=Sprite("Health","white",root,new Vector2(-.055f,1.13f),1,501);
            fill.transform.localScale=new Vector3(1.25f/.08f,.08f/.08f,1);
            fill.color=new Color32(150,173,105,255);
            SavePrefab(root,"PlayerVisual");
        }

        private static void Enemy(Transform parent)
        {
            var root=Group("Feral Shade",parent,4.6f,1.0f);
            Shadow(root,1.7f,.6f);
            Sprite("Body","enemy",root,Vector2.zero,.88f,Order(1));
            SavePrefab(root,"EnemyVisual");
        }

        private static void SavePrefab(Transform root,string name)
        {
            var prefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,PrefabPath+name+".prefab");
            var position=root.position;
            var parent=root.parent;
            Object.DestroyImmediate(root.gameObject);
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);
            instance.transform.position=position;
        }
    }
}
