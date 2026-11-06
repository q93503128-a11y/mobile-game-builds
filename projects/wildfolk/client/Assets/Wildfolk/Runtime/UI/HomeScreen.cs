using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public enum FrontendPage { Home,Settings,Multiplayer,ConfirmNew }
    public sealed class HomeScreen : MonoBehaviour
    {
        public SaveGameRules Rules;
        public UserPreferences Preferences;
        public GameObject Menu,Settings,Multiplayer,Confirm;
        public Button ContinueButton,SingleButton,MultiplayerButton,SettingsButton,ClassButton;
        public Button ProfileButton,GemStoreButton,AchievementButton,AttendanceButton;
        public Sprite PrimaryDefault,PrimaryPressed,PrimaryDisabled;
        public Sprite SecondaryDefault,SecondarySelected,SecondaryPressed,SecondaryDisabled,DialogFrame;
        public Text SaveSummary,Status;
        public FrontendPage Page {get;private set;}
        public bool CanContinue=>saved!=null;
        private SavedGame saved;
        private string statusKey;
        private int lastScreenWidth=-1,lastScreenHeight=-1;
        private void OnEnable(){L.Changed+=RefreshText;SceneFlow.HomeReady();ShowHome();}
        private void OnDisable(){L.Changed-=RefreshText;Preferences.Flush();}
        private void Update()
        {
            if(lastScreenWidth==Screen.width&&lastScreenHeight==Screen.height)return;
            lastScreenWidth=Screen.width;lastScreenHeight=Screen.height;ApplyResponsiveLayout();
        }
        public void ShowHome(){SetPage(FrontendPage.Home);RefreshSave();}
        public void OpenSettings()=>SetPage(FrontendPage.Settings);
        public void OpenMultiplayer()=>SetPage(FrontendPage.Multiplayer);
        public void SinglePlayer(){if(SceneFlow.IsTransitioning)return;if(SceneFlow.Repository.Exists)SetPage(FrontendPage.ConfirmNew);else BeginNew();}
        public void BeginNew(){Preferences.Flush();SceneFlow.NewGame();}
        public void TryContinue()
        {
            if(SceneFlow.IsTransitioning)return;RefreshSave();if(saved==null)return;
            Preferences.Flush();SceneFlow.Continue(saved,SceneFlow.Repository);
        }
        public void RefreshSave()
        {
            saved=null;statusKey=null;
            if(SceneFlow.Repository.Exists)
            {
                try{saved=SceneFlow.Repository.Read(Rules.Validate);}
                catch(Exception error)when(error is InvalidDataException || error is IOException || error is ArgumentException || error is InvalidOperationException){statusKey="home.save_unavailable";}
            }
            ContinueButton.interactable=saved!=null;RefreshText();
        }
        private void SetPage(FrontendPage page)
        {Page=page;Menu.SetActive(page==FrontendPage.Home);Settings.SetActive(page==FrontendPage.Settings);Multiplayer.SetActive(page==FrontendPage.Multiplayer);Confirm.SetActive(page==FrontendPage.ConfirmNew);}
        private void RefreshText()
        {
            ContinueButton.gameObject.SetActive(saved!=null);
            SetButtonSurface(ContinueButton,true);
            SetButtonSurface(SingleButton,saved==null);
            SetButtonSurface(MultiplayerButton,false);
            SetButtonSurface(SettingsButton,false);
            SetButtonSurface(ClassButton,false);
            ProfileButton=ProfileButton??FindMenuButton("Account Entry");
            GemStoreButton=GemStoreButton??FindMenuButton("Gem Balance");
            AchievementButton=AchievementButton??FindMenuButton("Direct Achievements");
            AttendanceButton=AttendanceButton??FindMenuButton("Direct Attendance");
            SetButtonSurface(ProfileButton,false);SetButtonSurface(GemStoreButton,false);
            SetButtonSurface(AchievementButton,false);SetButtonSurface(AttendanceButton,false);
            var label=SingleButton.GetComponentInChildren<Text>(true);var singleKey=saved==null?"home.new_action":"home.single";
            var localized=label.GetComponent<LocalizedLabel>();if(localized!=null)localized.Key=singleKey;label.text=L.Text(singleKey);
            SaveSummary.text=saved==null ? L.Text("home.no_save") : L.Format("home.saved_day",Math.Floor(saved.Elapsed/Rules.Clock.CycleSeconds)+1);
            Status.text=statusKey==null ? string.Empty : L.Text(statusKey);
            ApplyResponsiveLayout();
        }
        public void SetButtonSurface(Button button,bool primary,bool selected=false)
        {
            if(button==null)return;
            var surface=button.GetComponent<Image>();if(surface==null)return;
            surface.type=Image.Type.Sliced;surface.raycastTarget=true;surface.color=Color.white;
            var normal=primary?PrimaryDefault:SecondaryDefault;
            if(normal!=null)surface.sprite=normal;
            button.targetGraphic=surface;button.transition=Selectable.Transition.SpriteSwap;
            var state=button.spriteState;
            state.highlightedSprite=primary?PrimaryDefault:SecondarySelected;
            state.selectedSprite=selected?SecondarySelected:primary?PrimaryDefault:SecondarySelected;
            state.pressedSprite=primary?PrimaryPressed:SecondaryPressed;
            state.disabledSprite=primary?PrimaryDisabled:SecondaryDisabled;
            button.spriteState=state;
            var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=Color.white;
            colors.selectedColor=Color.white;colors.pressedColor=Color.white;colors.disabledColor=Color.white;
            colors.colorMultiplier=1;colors.fadeDuration=0;button.colors=colors;
        }
        public void RefreshMenuLayout(){lastScreenWidth=-1;lastScreenHeight=-1;ApplyResponsiveLayout();}
        private Button FindMenuButton(string name)
        {
            if(Menu==null)return null;
            var t=Menu.transform.Find("Field Journal/"+name)??Menu.transform.Find("Camp Records/"+name);
            return t==null?null:t.GetComponent<Button>();
        }
        private void ApplyResponsiveLayout()
        {
            if(Menu==null||ContinueButton==null||SingleButton==null||MultiplayerButton==null)return;
            var action=Menu.transform.Find("Field Journal") as RectTransform;
            if(action==null)return;
            var records=Menu.transform.Find("Camp Records") as RectTransform;
            var compact=Screen.width<1000;
            SetContainer(action,compact?new Vector2(.51f,.045f):new Vector2(.51f,.11f),compact?new Vector2(.985f,.955f):new Vector2(.985f,.89f));
            if(records!=null)
                SetContainer(records,compact?new Vector2(.51f,.045f):new Vector2(.04f,.035f),compact?new Vector2(.985f,.955f):new Vector2(.49f,.19f));
            var w=action.rect.width;var h=action.rect.height;
            if(w<1||h<1)return;
            var gap=compact?Mathf.Clamp(h*.04f,30,36):Mathf.Clamp(h*.02f,16,24);
            var headerH=compact?58f:56f;
            var headerGap=compact?32f:12f;
            var primaryH=compact?Mathf.Clamp(h*.14f,140,150):Mathf.Clamp(h*.13f,112,128);
            var rowH=compact?Mathf.Clamp(h*.16f,142,152):Mathf.Clamp(h*.11f,88,108);
            var utilityH=compact?Mathf.Clamp(h*.16f,142,152):Mathf.Clamp(h*.10f,82,100);
            var footerH=compact?Mathf.Clamp(h*.24f,202,220):0f;
            var top=h*.5f-headerH-headerGap;
            var title=action.Find("Menu Title")?.GetComponent<Text>();
            if(title!=null)
            {
                var rect=title.rectTransform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,1f);rect.pivot=new Vector2(.5f,1f);
                rect.anchoredPosition=new Vector2(0,-12);rect.sizeDelta=new Vector2(w*.92f,headerH);
                title.alignment=TextAnchor.MiddleLeft;title.fontSize=compact?52:34;title.resizeTextForBestFit=true;
                title.resizeTextMinSize=compact?40:26;title.resizeTextMaxSize=compact?52:34;
            }
            if(Status!=null)
            {
                var rect=Status.rectTransform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,1f);rect.pivot=new Vector2(.5f,1f);
                rect.anchoredPosition=new Vector2(0,-headerH-4);rect.sizeDelta=new Vector2(w*.92f,42);
                Status.alignment=TextAnchor.MiddleLeft;Status.fontSize=compact?30:24;Status.resizeTextForBestFit=true;
                Status.resizeTextMinSize=compact?24:20;Status.resizeTextMaxSize=compact?30:24;
            }
            var y=top-primaryH*.5f;var primary=CanContinue?ContinueButton:SingleButton;
            Place(primary,0,y,w,primaryH);ArrangePrimaryButton(primary,w,compact?56:50,compact?52:40);
            top-=primaryH+(compact?20:gap);
            y=top-rowH*.5f;
            if(CanContinue)
            {
                var half=(w-gap)*.5f;
                Place(SingleButton,-(half+gap)*.5f,y,half,rowH);ArrangeButton(SingleButton,half,compact?44:42,compact?42:34);
                Place(MultiplayerButton,(half+gap)*.5f,y,half,rowH);ArrangeButton(MultiplayerButton,half,compact?44:42,compact?42:34);
            }
            else{Place(MultiplayerButton,0,y,w,rowH);ArrangeButton(MultiplayerButton,w,compact?44:42,compact?42:34);}
            top-=rowH+(compact?16:gap);var third=(w-gap*2)/3f;y=top-utilityH*.5f;
            Place(ClassButton,-third-gap,y,third,utilityH);ArrangeButton(ClassButton,third,compact?36:30,compact?36:28);
            Place(SettingsButton,0,y,third,utilityH);ArrangeButton(SettingsButton,third,compact?36:30,compact?36:28);
            var profile=ProfileButton??FindMenuButton("Account Entry");ProfileButton=profile;
            Place(profile,third+gap,y,third,utilityH);ArrangeButton(profile,third,compact?36:30,compact?36:28);
            var store=GemStoreButton??FindMenuButton("Gem Balance");GemStoreButton=store;
            var achievements=AchievementButton??FindMenuButton("Direct Achievements");AchievementButton=achievements;
            var attendance=AttendanceButton??FindMenuButton("Direct Attendance");AttendanceButton=attendance;
            if(compact)
            {
                top-=utilityH+68f;y=top-footerH*.5f;
                Place(store,-third-gap,y,third,footerH);ArrangeFooterButton(store,third,footerH,52,34);
                Place(achievements,0,y,third,footerH);ArrangeFooterButton(achievements,third,footerH,52,34);
                Place(attendance,third+gap,y,third,footerH);ArrangeFooterButton(attendance,third,footerH,52,34);
            }
            else if(records!=null)
            {
                var footerWidth=records.rect.width;var footerHeight=Mathf.Clamp(records.rect.height*.70f,88,118);
                var footerGap=Mathf.Clamp(footerWidth*.02f,16,24);var footerThird=(footerWidth-footerGap*2)/3f;
                Place(store,-footerThird-footerGap,0,footerThird,footerHeight);ArrangeFooterButton(store,footerThird,footerHeight,38,24);
                Place(achievements,0,0,footerThird,footerHeight);ArrangeFooterButton(achievements,footerThird,footerHeight,38,24);
                Place(attendance,footerThird+footerGap,0,footerThird,footerHeight);ArrangeFooterButton(attendance,footerThird,footerHeight,38,24);
            }
        }
        private void SetContainer(RectTransform rect,Vector2 min,Vector2 max)
        {rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;rect.pivot=Vector2.one*.5f;}
        private static void Place(Selectable item,float x,float y,float width,float height)
        {if(item!=null)Place((RectTransform)item.transform,x,y,width,height);}
        private static void Place(RectTransform rect,float x,float y,float width,float height)
        {if(rect==null)return;rect.anchorMin=rect.anchorMax=Vector2.one*.5f;rect.pivot=Vector2.one*.5f;rect.anchoredPosition=new Vector2(x,y);rect.sizeDelta=new Vector2(width,height);}
        private static void ArrangeButton(Button button,float width,float iconSize,int fontSize)
        {
            if(button==null)return;
            var icon=button.transform.Find("Action Icon")??button.transform.Find("Navigation Symbol");
            if(icon!=null)
            {
                var iconRect=icon as RectTransform;if(iconRect!=null){iconRect.anchorMin=iconRect.anchorMax=Vector2.one*.5f;iconRect.pivot=Vector2.one*.5f;iconRect.anchoredPosition=new Vector2(-width*.5f+iconSize*.5f+16,0);iconRect.sizeDelta=Vector2.one*iconSize;}
                var image=icon.GetComponent<Image>();if(image!=null){image.preserveAspect=true;image.raycastTarget=false;image.color=new Color32(244,238,211,255);}
            }
            var label=button.GetComponentInChildren<Text>(true);if(label==null)return;
            var rect=label.rectTransform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.pivot=Vector2.one*.5f;
            rect.offsetMin=new Vector2(iconSize+30,4);rect.offsetMax=new Vector2(-12,-4);
            label.alignment=TextAnchor.MiddleCenter;label.color=new Color32(244,238,211,255);label.fontSize=fontSize;
            label.resizeTextForBestFit=true;label.resizeTextMinSize=Mathf.Max(20,fontSize-6);label.resizeTextMaxSize=fontSize;
            label.horizontalOverflow=HorizontalWrapMode.Overflow;label.verticalOverflow=VerticalWrapMode.Truncate;label.raycastTarget=false;
        }
        private void ArrangePrimaryButton(Button button,float width,float iconSize,int fontSize)
        {
            if(button==null)return;
            var icon=button.transform.Find("Action Icon")??button.transform.Find("Navigation Symbol");
            if(icon!=null)
            {
                var iconRect=icon as RectTransform;if(iconRect!=null){iconRect.anchorMin=iconRect.anchorMax=Vector2.one*.5f;iconRect.pivot=Vector2.one*.5f;iconRect.anchoredPosition=new Vector2(-width*.5f+iconSize*.5f+18,0);iconRect.sizeDelta=Vector2.one*iconSize;}
                var image=icon.GetComponent<Image>();if(image!=null){image.preserveAspect=true;image.raycastTarget=false;image.color=new Color32(244,238,211,255);}
            }
            var label=button.GetComponentInChildren<Text>(true);if(label!=null)
            {
                var rect=label.rectTransform;rect.anchorMin=new Vector2(0,.43f);rect.anchorMax=Vector2.one;rect.offsetMin=new Vector2(iconSize+28,0);rect.offsetMax=new Vector2(-16,-2);
                label.alignment=TextAnchor.MiddleCenter;label.color=new Color32(244,238,211,255);label.fontSize=fontSize;
                label.resizeTextForBestFit=true;label.resizeTextMinSize=fontSize-8;label.resizeTextMaxSize=fontSize;
                label.horizontalOverflow=HorizontalWrapMode.Overflow;label.verticalOverflow=VerticalWrapMode.Truncate;label.raycastTarget=false;
            }
            if(SaveSummary!=null)
            {
                if(SaveSummary.transform.parent!=button.transform)SaveSummary.transform.SetParent(button.transform,false);
                var rect=SaveSummary.rectTransform;rect.anchorMin=Vector2.zero;rect.anchorMax=new Vector2(1,.46f);rect.offsetMin=new Vector2(iconSize+28,2);rect.offsetMax=new Vector2(-16,0);
                SaveSummary.fontSize=compactFontSize(fontSize);SaveSummary.resizeTextForBestFit=true;SaveSummary.resizeTextMinSize=SaveSummary.fontSize-6;SaveSummary.resizeTextMaxSize=SaveSummary.fontSize;
                SaveSummary.alignment=TextAnchor.MiddleCenter;SaveSummary.horizontalOverflow=HorizontalWrapMode.Overflow;SaveSummary.verticalOverflow=VerticalWrapMode.Truncate;SaveSummary.raycastTarget=false;
            }
        }
        private static int compactFontSize(int primaryFontSize)=>primaryFontSize>40?28:24;
        private static void ArrangeFooterButton(Button button,float width,float height,float iconSize,int fontSize)
        {
            if(button==null)return;
            var icon=button.transform.Find("Action Icon")??button.transform.Find("Navigation Symbol");
            if(icon!=null)
            {
                var iconRect=icon as RectTransform;if(iconRect!=null){iconRect.anchorMin=iconRect.anchorMax=new Vector2(.5f,.68f);iconRect.pivot=Vector2.one*.5f;iconRect.anchoredPosition=Vector2.zero;iconRect.sizeDelta=Vector2.one*iconSize;}
                var image=icon.GetComponent<Image>();if(image!=null){image.preserveAspect=true;image.raycastTarget=false;image.color=new Color32(244,238,211,255);}
            }
            var label=button.GetComponentInChildren<Text>(true);if(label==null)return;
            var rect=label.rectTransform;rect.anchorMin=new Vector2(.04f,0);rect.anchorMax=new Vector2(.96f,.40f);rect.offsetMin=new Vector2(2,1);rect.offsetMax=new Vector2(-2,0);
            label.alignment=TextAnchor.MiddleCenter;label.color=new Color32(244,238,211,255);label.fontSize=fontSize;
            label.resizeTextForBestFit=true;label.resizeTextMinSize=fontSize-6;label.resizeTextMaxSize=fontSize;
            label.horizontalOverflow=HorizontalWrapMode.Overflow;label.verticalOverflow=VerticalWrapMode.Truncate;label.raycastTarget=false;
        }
    }
}
