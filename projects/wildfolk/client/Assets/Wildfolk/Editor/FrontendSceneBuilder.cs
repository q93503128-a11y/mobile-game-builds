using System;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Wildfolk.Presentation;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    public static class FrontendSceneBuilder
    {
        public const string ScenePath="Assets/Wildfolk/Scenes/MainMenu.unity";
        [MenuItem("Wildfolk/Build Home Scene")]
        public static void Build()
        {
            if(EditorApplication.isPlaying || SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Stop Play Mode and save the scene first.");
            if(SceneManager.GetActiveScene().path!=GameplaySceneBuilder.ScenePath)EditorSceneManager.OpenScene(GameplaySceneBuilder.ScenePath);
            var saves=UnityEngine.Object.FindFirstObjectByType<WorldSaveService>();
            var rules=AssetDatabase.LoadAssetAtPath<SaveGameRules>("Assets/Wildfolk/Resources/SaveGameRules.asset");
            if(rules==null){rules=ScriptableObject.CreateInstance<SaveGameRules>();AssetDatabase.CreateAsset(rules,"Assets/Wildfolk/Resources/SaveGameRules.asset");}
            rules.Catalog=saves.Catalog;rules.World=saves.World.Definition;rules.Animals=saves.World.Animals.Definition;rules.Clock=saves.Clock.Definition;
            rules.Player=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/Player.prefab").GetComponent<PlayerInventory>();
            var spawner=saves.GetComponent<ZombieSpawner>();rules.Night=spawner.Definition;rules.NightPrefabs=new[]{spawner.Prefab,spawner.SecondaryPrefab};
            rules.Goblin=saves.GetComponent<GoblinSpawner>().Prefab;rules.Camps=saves.GetComponent<GoblinSpawner>().CampDefinition;
            saves.Rules=rules;EditorUtility.SetDirty(rules);
            foreach(var portrait in UnityEngine.Object.FindObjectsByType<PlayerPortrait>(FindObjectsInactive.Include,FindObjectsSortMode.None))QualityUiBuilder.AlignHands(portrait,saves.Player);
            var pause=UnityEngine.Object.FindFirstObjectByType<PauseMenu>(FindObjectsInactive.Include);SurvivalUiBuilder.ConfigurePauseActions(pause.Menu.transform,saves);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            var sourceInput=UnityEngine.Object.FindFirstObjectByType<InputSystemUIInputModule>();var uiActions=sourceInput.actionsAsset;var point=sourceInput.point;var click=sourceInput.leftClick;
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.tag="MainCamera";camera.orthographic=true;camera.orthographicSize=9;camera.transform.position=new Vector3(0,0,-10);camera.backgroundColor=new Color32(62,80,49,255);
            var light=new GameObject("Global Light 2D").AddComponent<Light2D>();light.lightType=Light2D.LightType.Global;light.intensity=.78f;
            Background();
            var services=new GameObject("Home Preferences");var prefs=services.AddComponent<UserPreferences>();var audio=services.AddComponent<GameAudio>();audio.Preferences=prefs;
            var canvas=new GameObject("Home Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            var shade=Image("Atmosphere",canvas.transform,"white",new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);var shadeRect=(RectTransform)shade.transform;shadeRect.anchorMin=Vector2.zero;shadeRect.anchorMax=Vector2.one;shadeRect.offsetMin=shadeRect.offsetMax=Vector2.zero;shade.color=new Color(0.02f,.06f,.04f,.13f);
            var safe=Rect("Safe Area",canvas.transform,Vector2.zero,Vector2.zero,Vector2.zero);safe.anchorMax=Vector2.one;safe.offsetMin=safe.offsetMax=Vector2.zero;safe.gameObject.AddComponent<SafeAreaLayout>();
            var home=canvas.AddComponent<HomeScreen>();home.Rules=rules;home.Preferences=prefs;
            var menu=Rect("Home",safe,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);menu.anchorMin=Vector2.zero;menu.anchorMax=Vector2.one;menu.offsetMin=menu.offsetMax=Vector2.zero;home.Menu=menu.gameObject;
            var brand=Image(L.Text("brand.name"),menu,"lowfire_wordmark",new Vector2(.5f,.5f),new Vector2(-405,255),new Vector2(660,174));brand.preserveAspect=true;
            var panel=Image("Field Journal",menu,"lowfire_journal",new Vector2(.77f,.5f),Vector2.zero,new Vector2(700,950),true);
            var heading=Text("Menu Title",panel.transform,"home.camp",new Vector2(0,365),new Vector2(530,56),32,TextAnchor.MiddleLeft);heading.color=Ink;
            home.ContinueButton=JournalAction(panel.transform,"Continue","home.continue",new Vector2(0,260),new Vector2(530,132),home.TryContinue,true);
            home.SaveSummary=Text("Saved World",panel.transform,"",new Vector2(0,165),new Vector2(530,54),28,TextAnchor.MiddleLeft);home.SaveSummary.color=Ink;
            foreach(var y in new[]{125f,-10f,-145f}){var rule=Image("Page Rule",panel.transform,"white",new Vector2(.5f,.5f),new Vector2(26,y),new Vector2(418,1));rule.color=new Color32(158,147,115,180);}
            home.SingleButton=JournalAction(panel.transform,"Single Player","home.single",new Vector2(0,60),new Vector2(530,120),home.SinglePlayer,false);
            home.MultiplayerButton=JournalAction(panel.transform,"Multiplayer","home.multi",new Vector2(0,-75),new Vector2(530,120),home.OpenMultiplayer,false);
            home.SettingsButton=JournalAction(panel.transform,"Settings","ui.settings.title",new Vector2(-44,-235),new Vector2(300,72),home.OpenSettings,false);
            home.Status=Text("Save Status",panel.transform,"",new Vector2(0,-427),new Vector2(530,52),26,TextAnchor.MiddleLeft);home.Status.color=new Color32(106,57,38,255);
            home.Settings=SurvivalUiBuilder.Settings(safe,prefs,home.ShowHome);home.Settings.SetActive(false);
            home.Multiplayer=MultiplayerUiBuilder.Build(safe,home);home.Multiplayer.SetActive(false);
            var confirm=QualityUiBuilder.Surface(safe,"New World Confirmation",new Vector2(1000,620),out home.Confirm);
            Text("Title",confirm.transform,"home.confirm_title",new Vector2(0,240),new Vector2(850,80),44);
            Text("Message",confirm.transform,"home.confirm_body",new Vector2(0,65),new Vector2(850,220),34);
            Action(confirm.transform,"Cancel","ui.action.cancel",new Vector2(-230,-205),new Vector2(340,72),home.ShowHome,false);
            Action(confirm.transform,"Start New World","home.new_action",new Vector2(190,-205),new Vector2(400,72),home.BeginNew,true);home.Confirm.SetActive(false);
            var events=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));events.GetComponent<EventSystem>().sendNavigationEvents=false;var input=events.GetComponent<InputSystemUIInputModule>();input.UnassignActions();input.actionsAsset=uiActions;input.point=point;input.leftClick=click;
            EditorSceneManager.SaveScene(scene,ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true),new EditorBuildSettingsScene(GameplaySceneBuilder.ScenePath,true),new EditorBuildSettingsScene(MultiplayerSceneBuilder.ScenePath,true)};AssetDatabase.SaveAssets();AccountUiBuilder.Build();
        }
        private static readonly Color32 Ink=new Color32(49,63,48,255);
        private static Button JournalAction(Transform parent,string name,string key,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action,bool primary)
        {
            var button=JournalUiBuilder.Action(parent,name,key,position,size,action,out var label,primary);label.fontSize=36;return button;
        }
        private static Button Action(Transform parent,string name,string key,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action,bool primary)
        {
            size.y=Mathf.Max(120,size.y);return JournalUiBuilder.Action(parent,name,key,position,size,action,out _,primary);
        }
        private static void Background()
        {
            WorldArt.SpriteMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Wildfolk/Art/WorldSprite.mat");var root=new GameObject("Quiet Woodland").transform;
            var floor=WorldArt.Sprite("Forest Floor","ground",root,Vector2.zero,1,WorldSorting.Ground);var bounds=floor.sprite.bounds.size;floor.transform.localScale=new Vector3(40/bounds.x,23/bounds.y,1);
            var trees=new[]{new Vector2(-14,7),new Vector2(-11,8),new Vector2(-8,8),new Vector2(-15,3),new Vector2(-12,5.5f),new Vector2(-11,-6),new Vector2(-15,-5),new Vector2(-1,7),new Vector2(3,8),new Vector2(10,-7),new Vector2(14,6)};
            for(var i=0;i<trees.Length;i++)Prop(root,i%2==0 ? "tree_oak" : "tree_olive",trees[i],i%3==0 ? 1.5f : 1.1f);
            Prop(root,"campfire",new Vector2(-5,-1.1f),1.55f);Prop(root,"fallen_log",new Vector2(-8,-2.7f),1.3f);Prop(root,"storage_chest",new Vector2(-1.5f,-3.1f),1.1f);Prop(root,"workbench",new Vector2(-7.5f,.5f),1.1f);
            Prop(root,"rock_wide",new Vector2(-1.1f,2.2f),1.25f);Prop(root,"rock_split",new Vector2(-10,-.7f),.8f);
            var random=new System.Random(452);for(var i=0;i<34;i++){var p=new Vector2((float)random.NextDouble()*31-16,(float)random.NextDouble()*16-8);WorldArt.Sprite("Grass","grass",root,p,.6f,WorldSorting.GroundDecoration);}
        }
        private static void Prop(Transform parent,string art,Vector2 position,float scale)
        {var holder=WorldArt.Group(art,parent,position.x,position.y);WorldArt.Shadow(holder,2.1f*scale,.7f*scale);WorldArt.Sprite(art,art,holder,Vector2.zero,scale,20-Mathf.RoundToInt(position.y*10));}
    }
}
