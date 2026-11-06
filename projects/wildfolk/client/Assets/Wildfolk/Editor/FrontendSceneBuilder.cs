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
            VisualArtImporter.Import();
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
            ApplyMenuVisuals(home);
            EditorSceneManager.SaveScene(scene,ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true),new EditorBuildSettingsScene(GameplaySceneBuilder.ScenePath,true),new EditorBuildSettingsScene(MultiplayerSceneBuilder.ScenePath,true)};AssetDatabase.SaveAssets();AccountUiBuilder.Build();
        }
        [MenuItem("Wildfolk/Apply Main Menu Visual Pilot")]
        public static void ApplyVisualPilot()
        {
            if(EditorApplication.isPlaying||SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Stop Play Mode and save the scene first.");
            var scene=EditorSceneManager.OpenScene(ScenePath);var home=UnityEngine.Object.FindFirstObjectByType<HomeScreen>(FindObjectsInactive.Include);
            if(home==null)throw new InvalidOperationException("MainMenu has no HomeScreen.");
            VisualArtImporter.Import();ApplyMenuVisuals(home);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
        internal static void ApplyMenuVisuals(HomeScreen home)
        {
            if(home==null||home.Menu==null)throw new InvalidOperationException("Home menu references are missing.");
            home.PrimaryDefault=VisualArtImporter.Sprite("menu_a_primary_default");home.PrimaryPressed=VisualArtImporter.Sprite("menu_a_primary_pressed");home.PrimaryDisabled=VisualArtImporter.Sprite("menu_a_primary_disabled");
            home.SecondaryDefault=VisualArtImporter.Sprite("menu_a_secondary_default");home.SecondarySelected=VisualArtImporter.Sprite("menu_a_secondary_selected");home.SecondaryPressed=VisualArtImporter.Sprite("menu_a_secondary_pressed");home.SecondaryDisabled=VisualArtImporter.Sprite("menu_a_secondary_disabled");home.DialogFrame=VisualArtImporter.Sprite("menu_a_dialog_frame");
            var menu=home.Menu.transform;var field=menu.Find("Field Journal") as RectTransform;var records=menu.Find("Camp Records") as RectTransform;
            if(field==null)throw new InvalidOperationException("MainMenu is missing the Home action container.");
            ConfigureContainer(field,new Vector2(.51f,.11f),new Vector2(.985f,.89f));
            var fieldImage=field.GetComponent<Image>();if(fieldImage!=null){fieldImage.enabled=false;fieldImage.raycastTarget=false;}
            if(records!=null)
            {
                ConfigureContainer(records,new Vector2(.04f,.035f),new Vector2(.49f,.19f));
                var recordsImage=records.GetComponent<Image>();if(recordsImage!=null){recordsImage.enabled=false;recordsImage.raycastTarget=false;}
            }
            foreach(Transform child in field)if(child.name=="Page Rule")child.gameObject.SetActive(false);

            var brand=menu.Find("Lowfire")?.GetComponent<Image>();
            if(brand!=null)
            {
                var rect=(RectTransform)brand.transform;rect.anchorMin=rect.anchorMax=new Vector2(.145f,.89f);rect.pivot=Vector2.one*.5f;
                rect.anchoredPosition=Vector2.zero;rect.sizeDelta=new Vector2(440,132);brand.preserveAspect=true;brand.raycastTarget=false;
            }
            var heading=field.Find("Menu Title")??menu.Find("Menu Title");
            if(heading!=null)
            {
                heading.SetParent(field,false);
                var rect=heading as RectTransform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,1f);rect.pivot=new Vector2(.5f,1f);rect.anchoredPosition=new Vector2(0,-12);rect.sizeDelta=new Vector2(780,58);
                var text=heading.GetComponent<Text>();text.alignment=TextAnchor.MiddleLeft;text.fontSize=34;text.color=new Color32(245,238,210,255);text.resizeTextForBestFit=true;text.resizeTextMinSize=26;text.resizeTextMaxSize=34;text.verticalOverflow=VerticalWrapMode.Truncate;text.raycastTarget=false;
            }
            if(home.Status!=null)
            {
                home.Status.transform.SetParent(field,false);var rect=home.Status.rectTransform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.72f);rect.pivot=Vector2.one*.5f;rect.anchoredPosition=Vector2.zero;rect.sizeDelta=new Vector2(780,46);
                home.Status.color=new Color32(255,206,164,255);home.Status.fontSize=26;home.Status.alignment=TextAnchor.MiddleCenter;home.Status.resizeTextForBestFit=true;home.Status.resizeTextMinSize=21;home.Status.resizeTextMaxSize=26;home.Status.raycastTarget=false;
            }
            if(home.SaveSummary!=null)
            {home.SaveSummary.color=new Color32(245,238,210,255);home.SaveSummary.alignment=TextAnchor.MiddleCenter;home.SaveSummary.fontSize=30;home.SaveSummary.raycastTarget=false;}

            if(home.ClassButton==null)home.ClassButton=FindButton(field,"Class Journal Entry");
            home.ProfileButton=FindButton(field,"Account Entry");
            home.GemStoreButton=records==null?null:FindButton(records,"Gem Balance");
            home.AchievementButton=records==null?null:FindButton(records,"Direct Achievements");
            home.AttendanceButton=records==null?null:FindButton(records,"Direct Attendance");
            ApplyMenuButton(home,home.ContinueButton,true,"ui_next");ApplyMenuButton(home,home.SingleButton,false,"ui_play");
            ApplyMenuButton(home,home.MultiplayerButton,false,"ui_users");ApplyMenuButton(home,home.SettingsButton,false,"ui_settings");
            ApplyMenuButton(home,home.ClassButton,false,"class_survivor");ApplyMenuButton(home,home.ProfileButton,false,"ui_profile");
            ApplyMenuButton(home,home.GemStoreButton,false,"journal_gem");ApplyMenuButton(home,home.AchievementButton,false,"ui_award");ApplyMenuButton(home,home.AttendanceButton,false,"ui_calendar");
            ApplyConfirmation(home);
            home.RefreshMenuLayout();
        }
        private static void ConfigureContainer(RectTransform rect,Vector2 min,Vector2 max)
        {rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;rect.pivot=Vector2.one*.5f;}
        private static Button FindButton(Transform parent,string name)=>parent.Find(name)?.GetComponent<Button>();
        private static void ApplyMenuButton(HomeScreen home,Button button,bool primary,string iconName)
        {
            if(button==null)return;home.SetButtonSurface(button,primary);
            var surface=button.GetComponent<Image>();if(surface!=null)surface.raycastTarget=true;
            var icon=button.transform.Find("Action Icon")??button.transform.Find("Navigation Symbol");
            if(icon!=null)
            {
                var image=icon.GetComponent<Image>();if(image!=null){var sprite=VisualArtImporter.Sprite(iconName);if(sprite!=null)image.sprite=sprite;image.color=new Color32(244,238,211,255);image.preserveAspect=true;image.raycastTarget=false;}
            }
            var connection=button.transform.Find("Connection");if(connection!=null)connection.gameObject.SetActive(false);
            var arrow=button.transform.Find("Open");if(arrow!=null)arrow.gameObject.SetActive(false);
            var label=button.GetComponentInChildren<Text>(true);if(label!=null){label.color=new Color32(244,238,211,255);label.raycastTarget=false;}
        }
        private static void ApplyConfirmation(HomeScreen home)
        {
            if(home.Confirm==null)return;
            var frame=FindDeep(home.Confirm.transform,"New World Confirmation");if(frame==null)return;
            var image=frame.GetComponent<Image>();if(image!=null&&home.DialogFrame!=null){image.sprite=home.DialogFrame;image.type=UnityEngine.UI.Image.Type.Sliced;image.color=Color.white;image.raycastTarget=true;}
            var matte=FindDeep(frame,"Matte Surface");if(matte!=null){var matteImage=matte.GetComponent<Image>();if(matteImage!=null)matteImage.enabled=false;}
            var safe=home.Menu.transform.parent as RectTransform;var modal=frame.GetComponent<ModalLayout>();
            if(modal!=null){modal.SafeArea=safe;modal.ReferenceSize=new Vector2(1760,840);modal.HorizontalReserve=40;modal.TopReserve=32;modal.BottomReserve=32;}
            var panel=(RectTransform)frame;panel.sizeDelta=new Vector2(1760,840);
            var title=FindDeep(frame,"Title")?.GetComponent<Text>();
            if(title!=null){var rect=title.rectTransform;rect.anchorMin=rect.anchorMax=Vector2.one*.5f;rect.anchoredPosition=new Vector2(0,290);rect.sizeDelta=new Vector2(1580,96);title.alignment=TextAnchor.MiddleCenter;title.color=new Color32(245,238,210,255);title.fontSize=54;title.resizeTextForBestFit=true;title.resizeTextMinSize=42;title.resizeTextMaxSize=54;title.verticalOverflow=VerticalWrapMode.Truncate;title.raycastTarget=false;}
            var body=FindDeep(frame,"Message")?.GetComponent<Text>();
            if(body!=null){var rect=body.rectTransform;rect.anchorMin=rect.anchorMax=Vector2.one*.5f;rect.anchoredPosition=new Vector2(0,50);rect.sizeDelta=new Vector2(1580,250);body.alignment=TextAnchor.MiddleCenter;body.color=new Color32(245,238,210,255);body.fontSize=36;body.resizeTextForBestFit=true;body.resizeTextMinSize=28;body.resizeTextMaxSize=36;body.verticalOverflow=VerticalWrapMode.Overflow;body.horizontalOverflow=HorizontalWrapMode.Wrap;body.raycastTarget=false;}
            var cancel=FindDeep(frame,"Cancel")?.GetComponent<Button>();var start=FindDeep(frame,"Start New World")?.GetComponent<Button>();
            ApplyMenuButton(home,cancel,false,string.Empty);ApplyMenuButton(home,start,true,string.Empty);
            PlaceDialogButton(cancel,new Vector2(-292,-300),new Vector2(560,132));PlaceDialogButton(start,new Vector2(292,-300),new Vector2(640,132));
            ArrangeDialogButton(cancel,34);ArrangeDialogButton(start,36);
        }
        private static Transform FindDeep(Transform root,string name)
        {if(root==null)return null;if(root.name==name)return root;foreach(Transform child in root){var result=FindDeep(child,name);if(result!=null)return result;}return null;}
        private static void PlaceDialogButton(Button button,Vector2 position,Vector2 size)
        {if(button==null)return;var rect=(RectTransform)button.transform;rect.anchorMin=rect.anchorMax=Vector2.one*.5f;rect.pivot=Vector2.one*.5f;rect.anchoredPosition=position;rect.sizeDelta=size;}
        private static void ArrangeDialogButton(Button button,int fontSize)
        {if(button==null)return;var label=button.GetComponentInChildren<Text>(true);if(label==null)return;var rect=label.rectTransform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=new Vector2(38,8);rect.offsetMax=new Vector2(-24,-8);label.alignment=TextAnchor.MiddleCenter;label.color=new Color32(245,238,210,255);label.fontSize=fontSize;label.resizeTextForBestFit=true;label.resizeTextMinSize=28;label.resizeTextMaxSize=fontSize;label.horizontalOverflow=HorizontalWrapMode.Overflow;label.verticalOverflow=VerticalWrapMode.Truncate;label.raycastTarget=false;}
        private static readonly Color32 Ink=new Color32(49,63,48,255);
        private static Button JournalAction(Transform parent,string name,string key,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action,bool primary)
        {
            var icon=name=="Multiplayer"?"ui_users":name=="Settings"?"ui_settings":"ui_play";
            var button=JournalUiBuilder.IconAction(parent,name,key,position,size,action,VisualArtImporter.Sprite(icon),out var label,primary);JournalStyle.Action(button,primary);label.fontSize=36;return button;
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
