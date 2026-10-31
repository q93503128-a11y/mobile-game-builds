using System;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    public static class StoreUiBuilder
    {
        [MenuItem("Wildfolk/Build Gem Store")]
        public static void Build(){if(EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Clean edit mode required");var scene=EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);BuildInScene(UnityEngine.Object.FindFirstObjectByType<EconomyScreen>());EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();}
        internal static void BuildInScene(EconomyScreen meta)
        {
            var old=meta.GetComponent<StoreScreen>();if(old!=null){if(old.View!=null)UnityEngine.Object.DestroyImmediate(old.View);if(old.Entry!=null)UnityEngine.Object.DestroyImmediate(old.Entry.gameObject);if(old.HomeEntry!=null)UnityEngine.Object.DestroyImmediate(old.HomeEntry.gameObject);UnityEngine.Object.DestroyImmediate(old);}
            var s=meta.gameObject.AddComponent<StoreScreen>();s.Meta=meta;var safe=meta.Home.Menu.transform.parent;var hub=meta.HubView.transform.GetChild(0);
            s.Entry=JournalUiBuilder.Action(hub,"Gem Store Entry","store.title",new Vector2(300,410),new Vector2(430,110),s.Open,out var entryLabel);entryLabel.fontSize=32;
            s.HomeEntry=JournalUiBuilder.Action(meta.Home.Menu.transform,"Gem Balance","store.title",new Vector2(-650,-190),new Vector2(350,120),s.Open,out s.HomeBalance);var localizedBalance=s.HomeBalance.GetComponent<LocalizedLabel>();if(localizedBalance!=null)UnityEngine.Object.DestroyImmediate(localizedBalance);
            var sheet=JournalUiBuilder.Sheet(safe,"Gem Store",out s.View);
            JournalUiBuilder.Label(sheet,"Title","store.title",new Vector2(-400,410),new Vector2(680,80),42);
            s.Wallet=JournalUiBuilder.Label(sheet,"Wallet","",new Vector2(300,405),new Vector2(430,80),34,TextAnchor.MiddleRight);
            JournalUiBuilder.Action(sheet,"Back","ui.action.back",new Vector2(690,410),new Vector2(180,126),s.Close,out _);
            s.FixtureNote=JournalUiBuilder.Label(sheet,"Fixture only","",new Vector2(0,-240),new Vector2(1400,50),26,TextAnchor.MiddleCenter);
            s.Cards=new Button[3];s.Amounts=new Text[3];s.Prices=new Text[3];s.Uses=new Text[3];s.CardMarks=new Image[3];
            for(var i=0;i<3;i++){
                var b=JournalUiBuilder.Action(sheet,"Gem package "+i,"",new Vector2(-505+i*505,90),new Vector2(465,350),null,out var hidden);UnityEngine.Object.DestroyImmediate(hidden.gameObject);UnityEventTools.AddIntPersistentListener(b.onClick,s.Select,i);s.Cards[i]=b;
                Image("Gem",b.transform,"journal_gem",Vector2.one*.5f,new Vector2(0,112),new Vector2(94,94));s.Amounts[i]=JournalUiBuilder.Label(b.transform,"Amount","",new Vector2(0,28),new Vector2(400,75),42,TextAnchor.MiddleCenter);s.Prices[i]=JournalUiBuilder.Label(b.transform,"Platform price","",new Vector2(0,-50),new Vector2(420,65),30,TextAnchor.MiddleCenter);
                s.Uses[i]=JournalUiBuilder.Label(b.transform,"Uses","",new Vector2(0,-123),new Vector2(415,90),28,TextAnchor.MiddleCenter);s.CardMarks[i]=Image("Selected seal",b.transform,"journal_seal",Vector2.one*.5f,new Vector2(190,135),new Vector2(42,42));
                JournalUiBuilder.Rule(b.transform,new Vector2(-232,175),new Vector2(232,175));JournalUiBuilder.Rule(b.transform,new Vector2(-232,-175),new Vector2(-232,175));JournalUiBuilder.Rule(b.transform,new Vector2(232,-175),new Vector2(232,175));

            }
            var selection=Rect("Selection",sheet,Vector2.one*.5f,Vector2.zero,new Vector2(1500,1000));s.SelectionView=selection.gameObject;s.Detail=JournalUiBuilder.Label(selection,"Selected details","",new Vector2(-270,-105),new Vector2(900,110),32);
            s.Purchase=JournalUiBuilder.Action(selection,"Purchase","store.purchase",new Vector2(520,-155),new Vector2(440,126),s.Buy,out var buyLabel,true);buyLabel.fontSize=34;
            var viewport=Rect("Purchase history",sheet,Vector2.one*.5f,new Vector2(0,60),new Vector2(1430,430));s.HistoryView=viewport.gameObject;viewport.gameObject.AddComponent<Image>().color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();s.HistoryScroll=viewport.gameObject.AddComponent<ScrollRect>();s.HistoryScroll.horizontal=false;s.HistoryScroll.viewport=viewport;s.HistoryScroll.movementType=ScrollRect.MovementType.Clamped;
            var content=Rect("History content",viewport,new Vector2(.5f,1),Vector2.zero,new Vector2(1400,430));content.pivot=new Vector2(.5f,1);s.HistoryScroll.content=content;s.History=JournalUiBuilder.Label(content,"History","",Vector2.zero,new Vector2(1400,430),30,TextAnchor.UpperLeft);s.History.rectTransform.anchorMin=s.History.rectTransform.anchorMax=new Vector2(.5f,1);s.History.rectTransform.pivot=new Vector2(.5f,1);var layout=content.gameObject.AddComponent<VerticalLayoutGroup>();layout.childControlWidth=layout.childControlHeight=true;layout.childForceExpandHeight=false;content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            s.Retry=JournalUiBuilder.Link(sheet,"Check unfinished","store.recover",new Vector2(-500,-350),new Vector2(480,120),s.Recover,out var retryLabel);retryLabel.fontSize=30;
            s.HelpButton=JournalUiBuilder.Link(sheet,"History","store.history",new Vector2(10,-350),new Vector2(400,120),s.ToggleHistory,out s.HelpLabel);s.HelpLabel.fontSize=30;
            s.SupportButton=JournalUiBuilder.Link(sheet,"Purchase help","store.help",new Vector2(530,-350),new Vector2(400,120),s.Contact,out var supportLabel);supportLabel.fontSize=30;
            s.Status=JournalUiBuilder.Label(sheet,"Status","",new Vector2(0,310),new Vector2(1460,90),36,TextAnchor.MiddleCenter);s.HistoryView.SetActive(false);s.View.SetActive(false);
        }
    }
}
