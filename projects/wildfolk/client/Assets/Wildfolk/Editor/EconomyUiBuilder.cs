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
            var scene=EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);var home=UnityEngine.Object.FindFirstObjectByType<HomeScreen>();var existing=home.GetComponent<EconomyScreen>();if(existing!=null){UnityEngine.Object.DestroyImmediate(existing.AchievementView);UnityEngine.Object.DestroyImmediate(existing.AttendanceView);UnityEngine.Object.DestroyImmediate(existing.AchievementEntry.transform.parent.gameObject);UnityEngine.Object.DestroyImmediate(existing.AttendanceEntry.transform.parent.gameObject);UnityEngine.Object.DestroyImmediate(existing.ProfileSummary.gameObject);if(existing.HubView!=null)UnityEngine.Object.DestroyImmediate(existing.HubView);UnityEngine.Object.DestroyImmediate(existing);}var s=home.gameObject.AddComponent<EconomyScreen>();s.Home=home;s.Profile=home.GetComponent<AccountProfileScreen>();var profile=s.Profile.View.transform.GetChild(0);var safe=home.Menu.transform.parent;
            foreach(var name in new[]{"Direct Achievements","Direct Attendance"}){var previous=home.Menu.transform.Find(name);if(previous!=null)UnityEngine.Object.DestroyImmediate(previous.gameObject);}
            JournalUiBuilder.Action(home.Menu.transform,"Direct Achievements","economy.achievements",new Vector2(-650,-360),new Vector2(350,132),s.OpenAchievements,out s.DirectAchievementEntry);
            JournalUiBuilder.Action(home.Menu.transform,"Direct Attendance","economy.attendance",new Vector2(-260,-360),new Vector2(350,132),s.OpenAttendance,out s.DirectAttendanceEntry);
            var hub=JournalUiBuilder.Sheet(safe,"Profile and Records",out s.HubView);
            JournalUiBuilder.Label(hub,"Title","meta.title",new Vector2(-400,414),new Vector2(680,74),40);
            JournalUiBuilder.Action(hub,"Back","ui.action.back",new Vector2(690,410),new Vector2(180,126),s.CloseHub,out _);
            s.HubIdentity=JournalUiBuilder.Label(hub,"Identity","",new Vector2(0,285),new Vector2(1450,110),30);
            s.ProfileSummary=JournalUiBuilder.Label(hub,"Record summary","",new Vector2(-370,60),new Vector2(680,260),34);
            JournalUiBuilder.Action(hub,"Achievements Entry","economy.achievements",new Vector2(405,155),new Vector2(650,142),s.OpenAchievements,out s.AchievementEntry);
            JournalUiBuilder.Action(hub,"Attendance Entry","economy.attendance",new Vector2(405,-10),new Vector2(650,142),s.OpenAttendance,out s.AttendanceEntry);
            JournalUiBuilder.Action(hub,"Account Settings","account.title",new Vector2(-385,-130),new Vector2(650,126),s.OpenAccount,out _);
            s.WatchAd=JournalUiBuilder.Action(hub,"Optional Gem ad","",new Vector2(405,-160),new Vector2(650,126),s.WatchReward,out s.AdLabel);
            s.AdStatus=JournalUiBuilder.Label(hub,"Ad availability","",new Vector2(-385,-245),new Vector2(720,94),28);
            s.RetryAd=JournalUiBuilder.Action(hub,"Retry reward","ads.retry",new Vector2(580,-350),new Vector2(310,126),s.RetryReward,out _);
            s.AdPrivacy=JournalUiBuilder.Action(hub,"Ad privacy","ads.privacy",new Vector2(-385,-360),new Vector2(650,126),s.AdPrivacyOptions,out _);s.AdPrivacy.gameObject.SetActive(false);
            s.HubView.SetActive(false);
            var character=(RectTransform)s.Profile.Character.transform;character.anchoredPosition=new Vector2(-420,-337);character.sizeDelta=new Vector2(620,100);
            s.HomeEntry=home.Menu.transform.Find("Field Journal/Account Entry/Label").GetComponent<Text>();
            var entry=s.HomeEntry.GetComponentInParent<Button>();while(entry.onClick.GetPersistentEventCount()>0)UnityEventTools.RemovePersistentListener(entry.onClick,0);UnityEventTools.AddPersistentListener(entry.onClick,s.OpenHub);
            foreach(var label in new[]{s.HomeEntry,s.AchievementEntry,s.AttendanceEntry}){var localized=label.GetComponent<LocalizedLabel>();if(localized!=null)UnityEngine.Object.DestroyImmediate(localized);}
            var a=JournalUiBuilder.Sheet(safe,"Achievements",out s.AchievementView);Header(a,"economy.achievements",s.Back,s.Sync,out s.Wallet);s.Summary=JournalUiBuilder.Label(a,"Points","",new Vector2(-375,329),new Vector2(740,58),28);
            s.Categories=new Button[7];s.CategoryLabels=new Text[7];for(var i=0;i<7;i++){var b=JournalUiBuilder.Action(a,"Category "+i,"",new Vector2(-660+i*220,231),new Vector2(210,126),null,out s.CategoryLabels[i]);UnityEventTools.AddIntPersistentListener(b.onClick,s.Category,i);s.CategoryLabels[i].fontSize=32;s.Categories[i]=b;}
            var viewport=Rect("Family Viewport",a,Vector2.one*.5f,new Vector2(-410,-36),new Vector2(700,378));var image=viewport.gameObject.AddComponent<Image>();image.color=new Color(0,0,0,0);viewport.gameObject.AddComponent<RectMask2D>();s.FamilyScroll=viewport.gameObject.AddComponent<ScrollRect>();s.FamilyScroll.horizontal=false;s.FamilyScroll.viewport=viewport;var content=Rect("Families",viewport,new Vector2(.5f,1),Vector2.zero,new Vector2(700,648));content.pivot=new Vector2(.5f,1);s.FamilyScroll.content=content;s.FamilyScroll.movementType=ScrollRect.MovementType.Clamped;
            s.Rows=new Button[6];s.RowLabels=new Text[6];s.RowProgress=new Image[6];for(var i=0;i<6;i++){s.Rows[i]=JournalUiBuilder.Action(content,"Family "+i,"",new Vector2(0,-64-i*132),new Vector2(680,126),null,out s.RowLabels[i]);s.RowLabels[i].fontSize=30;s.RowLabels[i].rectTransform.sizeDelta=new Vector2(630,95);s.RowLabels[i].rectTransform.anchoredPosition=new Vector2(0,8);var track=Image("Progress track",s.Rows[i].transform,"white",Vector2.one*.5f,new Vector2(0,-48),new Vector2(620,8));track.color=JournalStyle.Muted;s.RowProgress[i]=Image("Progress",track.transform,"white",Vector2.one*.5f,Vector2.zero,new Vector2(620,8));s.RowProgress[i].color=JournalStyle.Ember;s.RowProgress[i].type=UnityEngine.UI.Image.Type.Filled;s.RowProgress[i].fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;((RectTransform)s.Rows[i].transform).anchorMin=((RectTransform)s.Rows[i].transform).anchorMax=new Vector2(.5f,1);UnityEventTools.AddIntPersistentListener(s.Rows[i].onClick,s.Select,i);}
            var tierViewport=Rect("Tier Viewport",a,Vector2.one*.5f,new Vector2(417,-30),new Vector2(660,390));tierViewport.gameObject.AddComponent<Image>().color=Color.clear;tierViewport.gameObject.AddComponent<RectMask2D>();s.DetailScroll=tierViewport.gameObject.AddComponent<ScrollRect>();s.DetailScroll.horizontal=false;s.DetailScroll.viewport=tierViewport;s.DetailScroll.movementType=ScrollRect.MovementType.Clamped;
            s.Detail=JournalUiBuilder.Label(tierViewport,"Selected Achievement","",Vector2.zero,new Vector2(660,800),34,TextAnchor.UpperLeft);s.Detail.rectTransform.anchorMin=s.Detail.rectTransform.anchorMax=new Vector2(.5f,1);s.Detail.rectTransform.pivot=new Vector2(.5f,1);s.Detail.rectTransform.anchoredPosition=Vector2.zero;s.DetailScroll.content=s.Detail.rectTransform;var detailFit=s.Detail.gameObject.AddComponent<ContentSizeFitter>();detailFit.horizontalFit=ContentSizeFitter.FitMode.Unconstrained;detailFit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            s.Claim=JournalUiBuilder.Action(a,"Claim","economy.claim",new Vector2(440,-300),new Vector2(600,126),s.ClaimSelected,out _,true);
            s.ClaimAll=JournalUiBuilder.Action(a,"Claim All","economy.claim_all",new Vector2(-440,-300),new Vector2(600,126),s.ClaimEverything,out _,true);
            s.Status=JournalUiBuilder.Label(a,"Connection","",new Vector2(0,-395),new Vector2(1500,50),26,TextAnchor.MiddleCenter);
            var d=JournalUiBuilder.Sheet(safe,"Attendance",out s.AttendanceView);Header(d,"economy.attendance",s.Back,s.Sync,out s.AttendanceWallet);
            s.Tabs=new Button[3];var keys=new[]{"economy.newcomer_tab","economy.monthly_tab","economy.event"};for(var i=0;i<3;i++){s.Tabs[i]=JournalUiBuilder.Action(d,"Board "+i,keys[i],new Vector2(-560+i*560,288),new Vector2(510,126),null,out _);UnityEventTools.AddIntPersistentListener(s.Tabs[i].onClick,s.Board,i);}
            s.BoardTitle=JournalUiBuilder.Label(d,"Board Title","",new Vector2(0,195),new Vector2(1470,52),34,TextAnchor.MiddleCenter);
            s.Slots=new Button[28];s.SlotLabels=new Text[28];s.SlotMarks=new Image[28];s.SlotIcons=new Image[28];for(var i=0;i<28;i++){s.Slots[i]=JournalUiBuilder.Action(d,"Visit "+(i+1),"",new Vector2(-630+i%7*210,122-i/7*104),new Vector2(196,108),null,out s.SlotLabels[i]);s.SlotLabels[i].fontSize=28;s.SlotLabels[i].rectTransform.sizeDelta=new Vector2(180,86);s.SlotIcons[i]=Image("Gem reward",s.Slots[i].transform,"journal_gem",Vector2.one*.5f,new Vector2(-72,-25),new Vector2(26,26));s.SlotMarks[i]=Image("Received seal",s.Slots[i].transform,"journal_seal",Vector2.one*.5f,new Vector2(72,34),new Vector2(25,25));}
            s.Attend=JournalUiBuilder.Action(d,"Attend","economy.claim",new Vector2(480,-335),new Vector2(530,126),s.ClaimAttendance,out _,true);
            s.ResetLabel=JournalUiBuilder.Label(d,"Next Reset","",new Vector2(-345,-315),new Vector2(775,100),26);
            s.AttendanceStatus=JournalUiBuilder.Label(d,"Connection","",new Vector2(-355,-395),new Vector2(800,105),26,TextAnchor.MiddleCenter);
            foreach(var view in new[]{s.HubView,s.AchievementView,s.AttendanceView})foreach(var label in view.GetComponentsInChildren<Text>(true))if(label.name=="Label"&&label.fontSize==26)label.fontSize=32;
            s.AchievementView.SetActive(false);s.AttendanceView.SetActive(false);StoreUiBuilder.BuildInScene(s);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
        private static void Header(Transform p,string title,UnityEngine.Events.UnityAction back,UnityEngine.Events.UnityAction sync,out Text wallet)
        {JournalUiBuilder.Label(p,"Title",title,new Vector2(-420,427),new Vector2(620,70),34);wallet=JournalUiBuilder.Label(p,"Gem Wallet","",new Vector2(365,428),new Vector2(320,64),30);JournalUiBuilder.Action(p,"Back","ui.action.back",new Vector2(695,400),new Vector2(160,126),back,out _);JournalUiBuilder.Action(p,"Sync","economy.sync",new Vector2(40,425),new Vector2(270,126),sync,out _);}
    }
}
