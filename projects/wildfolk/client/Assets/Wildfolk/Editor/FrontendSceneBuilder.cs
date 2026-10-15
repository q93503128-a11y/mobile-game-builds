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
            audio.Music=new GameObject("Music Channel",typeof(AudioSource)).GetComponent<AudioSource>();audio.Music.transform.SetParent(services.transform);audio.Music.playOnAwake=false;audio.Music.loop=true;
            audio.Sfx=new GameObject("SFX Channel",typeof(AudioSource)).GetComponent<AudioSource>();audio.Sfx.transform.SetParent(services.transform);audio.Sfx.playOnAwake=false;
            var canvas=new GameObject("Home Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            var shade=Image("Atmosphere",canvas.transform,"white",new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);var shadeRect=(RectTransform)shade.transform;shadeRect.anchorMin=Vector2.zero;shadeRect.anchorMax=Vector2.one;shadeRect.offsetMin=shadeRect.offsetMax=Vector2.zero;shade.color=new Color(0.02f,.06f,.04f,.13f);
            var safe=Rect("Safe Area",canvas.transform,Vector2.zero,Vector2.zero,Vector2.zero);safe.anchorMax=Vector2.one;safe.offsetMin=safe.offsetMax=Vector2.zero;safe.gameObject.AddComponent<SafeAreaLayout>();
            var home=canvas.AddComponent<HomeScreen>();home.Rules=rules;home.Preferences=prefs;
            var menu=Rect("Home",safe,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);menu.anchorMin=Vector2.zero;menu.anchorMax=Vector2.one;menu.offsetMin=menu.offsetMax=Vector2.zero;home.Menu=menu.gameObject;
            var brand=Image(L.Text("brand.name"),menu,"lowfire_wordmark",new Vector2(.5f,.5f),new Vector2(-405,255),new Vector2(660,174));brand.preserveAspect=true;
            var panel=Image("Field Journal",menu,"lowfire_journal",new Vector2(.77f,.5f),Vector2.zero,new Vector2(610,730),true);
            var heading=Text("Menu Title",panel.transform,"home.camp",new Vector2(26,237),new Vector2(422,40),22,TextAnchor.MiddleLeft);heading.color=Ink;
            home.ContinueButton=JournalAction(panel.transform,"Continue","home.continue",new Vector2(23,139),new Vector2(434,100),home.TryContinue,true);
            home.SaveSummary=Text("Saved World",panel.transform,"",new Vector2(26,51),new Vector2(414,66),21,TextAnchor.MiddleLeft);home.SaveSummary.color=Ink;
            foreach(var y in new[]{-2f,-108f,-213f}){var rule=Image("Page Rule",panel.transform,"white",new Vector2(.5f,.5f),new Vector2(26,y),new Vector2(418,1));rule.color=new Color32(158,147,115,180);}
            home.SingleButton=JournalAction(panel.transform,"Single Player","home.single",new Vector2(23,-49),new Vector2(434,82),home.SinglePlayer,false);
            home.MultiplayerButton=JournalAction(panel.transform,"Multiplayer","home.multi",new Vector2(23,-154),new Vector2(434,82),home.OpenMultiplayer,false);
            home.SettingsButton=JournalAction(panel.transform,"Settings","ui.settings.title",new Vector2(-44,-255),new Vector2(300,66),home.OpenSettings,false);
            home.Status=Text("Save Status",panel.transform,"",new Vector2(26,-321),new Vector2(422,42),18,TextAnchor.MiddleLeft);home.Status.color=new Color32(106,57,38,255);
            home.Settings=SurvivalUiBuilder.Settings(safe,prefs,home.ShowHome);home.Settings.SetActive(false);
            var multi=QualityUiBuilder.Surface(safe,"Multiplayer",new Vector2(900,630),out home.Multiplayer);
            var icon=Image("Camp",multi.transform,"campfire",new Vector2(.5f,.5f),new Vector2(0,173),new Vector2(100,100));icon.preserveAspect=true;
            Text("Title",multi.transform,"home.multi_title",new Vector2(0,72),new Vector2(770,70),38);
            Text("Availability",multi.transform,"home.multi_body",new Vector2(0,-34),new Vector2(740,120),26);
            Action(multi.transform,"Single Player","home.single",new Vector2(-140,-219),new Vector2(360,72),home.SinglePlayer,true);
            Action(multi.transform,"Back","ui.action.back",new Vector2(250,-219),new Vector2(250,72),home.ShowHome,false);home.Multiplayer.SetActive(false);
            var confirm=QualityUiBuilder.Surface(safe,"New World Confirmation",new Vector2(940,500),out home.Confirm);
            Text("Title",confirm.transform,"home.confirm_title",new Vector2(0,153),new Vector2(820,70),38);
            Text("Message",confirm.transform,"home.confirm_body",new Vector2(0,26),new Vector2(810,142),26);
            Action(confirm.transform,"Cancel","ui.action.cancel",new Vector2(-230,-160),new Vector2(340,72),home.ShowHome,false);
            Action(confirm.transform,"Start New World","home.new_action",new Vector2(190,-160),new Vector2(400,72),home.BeginNew,true);home.Confirm.SetActive(false);
            var events=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));events.GetComponent<EventSystem>().sendNavigationEvents=false;var input=events.GetComponent<InputSystemUIInputModule>();input.UnassignActions();input.actionsAsset=uiActions;input.point=point;input.leftClick=click;
            EditorSceneManager.SaveScene(scene,ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true),new EditorBuildSettingsScene(GameplaySceneBuilder.ScenePath,true)};AssetDatabase.SaveAssets();
        }
        private static readonly Color32 Ink=new Color32(49,63,48,255);
        private static Button JournalAction(Transform parent,string name,string key,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action,bool primary)
        {
            var image=Image(name,parent,"white",new Vector2(.5f,.5f),position,size,true);image.color=primary ? new Color32(137,88,56,255) : new Color32(91,99,72,14);
            var label=Text("Label",image.transform,key,new Vector2(-8,0),size-new Vector2(54,12),primary ? 32 : 29,TextAnchor.MiddleLeft);label.color=primary ? new Color32(248,237,207,255) : Ink;
            if(primary){var arrow=Text("Arrow",image.transform,"›",new Vector2(size.x*.5f-28,0),new Vector2(32,44),34);arrow.color=label.color;}
            var button=Button(image);var colors=button.colors;
            if(primary){colors.normalColor=image.color;image.color=Color.white;colors.highlightedColor=new Color32(156,106,71,255);colors.pressedColor=new Color32(100,68,46,255);colors.disabledColor=new Color32(143,140,118,255);}
            else{colors.highlightedColor=new Color(1,.95f,.85f,1);colors.pressedColor=new Color(.66f,.72f,.59f,1);}
            button.colors=colors;
            UnityEventTools.AddPersistentListener(button.onClick,action);return button;
        }
        private static Button Action(Transform parent,string name,string key,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action,bool primary)
        {
            var image=Image(name,parent,"white",new Vector2(.5f,.5f),position,size,true);image.color=primary ? new Color32(159,168,105,255) : new Color32(45,65,49,255);
            var label=Text("Label",image.transform,key,new Vector2(14,0),size-new Vector2(70,12),29,TextAnchor.MiddleLeft);label.color=primary ? new Color32(22,39,29,255) : new Color32(226,222,193,255);
            var bar=Image("Accent",image.transform,"white",new Vector2(0,.5f),new Vector2(2,0),new Vector2(4,size.y));bar.color=primary ? new Color32(219,207,146,255) : new Color32(103,125,85,255);
            var button=Button(image);var colors=button.colors;colors.disabledColor=new Color(.53f,.57f,.48f,1);button.colors=colors;UnityEventTools.AddPersistentListener(button.onClick,action);return button;
        }
        private static void Background()
        {
            WorldArt.SpriteMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Wildfolk/Art/WorldSprite.mat");var root=new GameObject("Quiet Woodland").transform;
            var floor=WorldArt.Sprite("Forest Floor","ground",root,Vector2.zero,1,WorldSorting.Ground);var bounds=floor.sprite.bounds.size;floor.transform.localScale=new Vector3(40/bounds.x,23/bounds.y,1);
            var trees=new[]{new Vector2(-14,7),new Vector2(-11,8),new Vector2(-8,8),new Vector2(-15,3),new Vector2(-12,5.5f),new Vector2(-11,-6),new Vector2(-15,-5),new Vector2(-1,7),new Vector2(3,8),new Vector2(10,-7),new Vector2(14,6)};
            for(var i=0;i<trees.Length;i++)Prop(root,i%2==0 ? "tree_oak" : "tree_olive",trees[i],i%3==0 ? 1.5f : 1.1f);
            Prop(root,"campfire",new Vector2(-5,-1.1f),1.55f);Prop(root,"fallen_log",new Vector2(-8,-2.7f),1.3f);Prop(root,"storage_chest",new Vector2(-2.7f,-1.5f),1.1f);Prop(root,"workbench",new Vector2(-7.5f,.5f),1.1f);
            Prop(root,"rock_wide",new Vector2(-1.1f,2.2f),1.25f);Prop(root,"rock_split",new Vector2(-10,-.7f),.8f);
            var random=new System.Random(452);for(var i=0;i<34;i++){var p=new Vector2((float)random.NextDouble()*31-16,(float)random.NextDouble()*16-8);WorldArt.Sprite("Grass","grass",root,p,.6f,WorldSorting.GroundDecoration);}
        }
        private static void Prop(Transform parent,string art,Vector2 position,float scale)
        {var holder=WorldArt.Group(art,parent,position.x,position.y);WorldArt.Shadow(holder,2.1f*scale,.7f*scale);WorldArt.Sprite(art,art,holder,Vector2.zero,scale,20-Mathf.RoundToInt(position.y*10));}
    }
}
