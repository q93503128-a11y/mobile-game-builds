using System;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    public static class EconomyUiBuilder
    {
        [MenuItem("Wildfolk/Build Economy Journal")]
        public static void Build()
        {
            if(EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Save scene and stop Play first");
            var scene=EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);var home=UnityEngine.Object.FindFirstObjectByType<HomeScreen>();var existing=home.GetComponent<EconomyScreen>();if(existing!=null){UnityEngine.Object.DestroyImmediate(existing.AchievementView);UnityEngine.Object.DestroyImmediate(existing.AttendanceView);UnityEngine.Object.DestroyImmediate(existing.AchievementEntry.transform.parent.gameObject);UnityEngine.Object.DestroyImmediate(existing.AttendanceEntry.transform.parent.gameObject);UnityEngine.Object.DestroyImmediate(existing.ProfileSummary.gameObject);UnityEngine.Object.DestroyImmediate(existing);}var s=home.gameObject.AddComponent<EconomyScreen>();s.Home=home;s.Profile=home.GetComponent<AccountProfileScreen>();var profile=s.Profile.View.transform.GetChild(0);var safe=home.Menu.transform.parent;
            JournalUiBuilder.Action(profile,"Achievements Entry","economy.achievements",new Vector2(125,392),new Vector2(285,126),s.OpenAchievements,out s.AchievementEntry);
            JournalUiBuilder.Action(profile,"Attendance Entry","economy.attendance",new Vector2(425,392),new Vector2(285,126),s.OpenAttendance,out s.AttendanceEntry);
            var character=(RectTransform)s.Profile.Character.transform;character.anchoredPosition=new Vector2(-420,-281);character.sizeDelta=new Vector2(620,46);s.Profile.Character.fontSize=24;
            s.ProfileSummary=JournalUiBuilder.Label(profile,"Economy Summary","",new Vector2(-420,-373),new Vector2(640,130),25);
            s.HomeEntry=home.Menu.transform.Find("Field Journal/Account Entry/Label").GetComponent<Text>();foreach(var label in new[]{s.HomeEntry,s.AchievementEntry,s.AttendanceEntry}){var localized=label.GetComponent<LocalizedLabel>();if(localized!=null)UnityEngine.Object.DestroyImmediate(localized);}
            var a=JournalUiBuilder.Book(safe,"Achievements",out s.AchievementView);Header(a,"economy.achievements",s.Back,s.Sync,out s.Wallet);s.Summary=JournalUiBuilder.Label(a,"Points","",new Vector2(-375,329),new Vector2(740,58),28);
            s.Categories=new Button[7];s.CategoryLabels=new Text[7];for(var i=0;i<7;i++){var b=JournalUiBuilder.Action(a,"Category "+i,"",new Vector2(-660+i*220,231),new Vector2(210,126),null,out s.CategoryLabels[i]);UnityEventTools.AddIntPersistentListener(b.onClick,s.Category,i);s.Categories[i]=b;}
            var viewport=Rect("Family Viewport",a,Vector2.one*.5f,new Vector2(-410,-36),new Vector2(700,378));var image=viewport.gameObject.AddComponent<Image>();image.color=new Color(0,0,0,0);viewport.gameObject.AddComponent<RectMask2D>();s.FamilyScroll=viewport.gameObject.AddComponent<ScrollRect>();s.FamilyScroll.horizontal=false;s.FamilyScroll.viewport=viewport;var content=Rect("Families",viewport,new Vector2(.5f,1),Vector2.zero,new Vector2(700,648));content.pivot=new Vector2(.5f,1);s.FamilyScroll.content=content;s.FamilyScroll.movementType=ScrollRect.MovementType.Clamped;
            s.Rows=new Button[6];s.RowLabels=new Text[6];for(var i=0;i<6;i++){s.Rows[i]=JournalUiBuilder.Action(content,"Family "+i,"",new Vector2(0,-64-i*132),new Vector2(680,126),null,out s.RowLabels[i]);((RectTransform)s.Rows[i].transform).anchorMin=((RectTransform)s.Rows[i].transform).anchorMax=new Vector2(.5f,1);UnityEventTools.AddIntPersistentListener(s.Rows[i].onClick,s.Select,i);}
            var tierViewport=Rect("Tier Viewport",a,Vector2.one*.5f,new Vector2(417,-36),new Vector2(660,414));tierViewport.gameObject.AddComponent<Image>().color=Color.clear;tierViewport.gameObject.AddComponent<RectMask2D>();s.DetailScroll=tierViewport.gameObject.AddComponent<ScrollRect>();s.DetailScroll.horizontal=false;s.DetailScroll.viewport=tierViewport;s.DetailScroll.movementType=ScrollRect.MovementType.Clamped;
            s.Detail=JournalUiBuilder.Label(tierViewport,"Selected Achievement","",Vector2.zero,new Vector2(660,800),27,TextAnchor.UpperLeft);s.Detail.rectTransform.anchorMin=s.Detail.rectTransform.anchorMax=new Vector2(.5f,1);s.Detail.rectTransform.pivot=new Vector2(.5f,1);s.Detail.rectTransform.anchoredPosition=Vector2.zero;s.DetailScroll.content=s.Detail.rectTransform;
            s.Claim=JournalUiBuilder.Action(a,"Claim","economy.claim",new Vector2(440,-320),new Vector2(600,126),s.ClaimSelected,out _,true);
            s.ClaimAll=JournalUiBuilder.Action(a,"Claim All","economy.claim_all",new Vector2(-440,-320),new Vector2(600,126),s.ClaimEverything,out _,true);
            s.Status=JournalUiBuilder.Label(a,"Connection","",new Vector2(0,-404),new Vector2(1500,64),24,TextAnchor.MiddleCenter);
            var d=JournalUiBuilder.Book(safe,"Attendance",out s.AttendanceView);Header(d,"economy.attendance",s.Back,s.Sync,out s.AttendanceWallet);
            s.Tabs=new Button[3];var keys=new[]{"economy.newcomer_tab","economy.monthly_tab","economy.event"};for(var i=0;i<3;i++){s.Tabs[i]=JournalUiBuilder.Action(d,"Board "+i,keys[i],new Vector2(-560+i*560,288),new Vector2(510,126),null,out _);UnityEventTools.AddIntPersistentListener(s.Tabs[i].onClick,s.Board,i);}
            s.BoardTitle=JournalUiBuilder.Label(d,"Board Title","",new Vector2(0,195),new Vector2(1470,52),27,TextAnchor.MiddleCenter);
            s.Slots=new Button[28];s.SlotLabels=new Text[28];s.SlotMarks=new Image[28];for(var i=0;i<28;i++){s.Slots[i]=JournalUiBuilder.Action(d,"Visit "+(i+1),"",new Vector2(-630+i%7*210,101-i/7*96),new Vector2(196,92),null,out s.SlotLabels[i]);s.SlotMarks[i]=Image("Received seal",s.Slots[i].transform,"journal_seal",Vector2.one*.5f,new Vector2(72,34),new Vector2(25,25));}
            s.Attend=JournalUiBuilder.Action(d,"Attend","economy.claim",new Vector2(480,-320),new Vector2(530,126),s.ClaimAttendance,out _,true);
            s.ResetLabel=JournalUiBuilder.Label(d,"Next Reset","",new Vector2(-345,-320),new Vector2(775,100),26);
            s.AttendanceStatus=JournalUiBuilder.Label(d,"Connection","",new Vector2(0,-404),new Vector2(1500,60),24,TextAnchor.MiddleCenter);
            s.AchievementView.SetActive(false);s.AttendanceView.SetActive(false);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
        private static void Header(Transform p,string title,UnityEngine.Events.UnityAction back,UnityEngine.Events.UnityAction sync,out Text wallet)
        {JournalUiBuilder.Label(p,"Title",title,new Vector2(-420,427),new Vector2(620,70),34);wallet=JournalUiBuilder.Label(p,"Gem Wallet","",new Vector2(365,428),new Vector2(320,64),30);JournalUiBuilder.Action(p,"Back","ui.action.back",new Vector2(695,400),new Vector2(160,126),back,out _);JournalUiBuilder.Action(p,"Sync","economy.sync",new Vector2(40,425),new Vector2(270,126),sync,out _);}
    }
}
