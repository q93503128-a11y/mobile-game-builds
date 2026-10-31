using System;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    public static class AccountUiBuilder
    {
        [MenuItem("Wildfolk/Build Account Journal")]
        public static void Build()
        {
            if(EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Save scene and stop Play first");
            var scene=EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);var home=UnityEngine.Object.FindFirstObjectByType<HomeScreen>();
            if(home.GetComponent<AccountProfileScreen>()!=null)throw new InvalidOperationException("Account journal already authored");
            var screen=home.gameObject.AddComponent<AccountProfileScreen>();screen.Home=home;
            var settings=(RectTransform)home.SettingsButton.transform;settings.anchoredPosition=new Vector2(-140,-355);settings.sizeDelta=new Vector2(260,116);var settingsLabel=home.SettingsButton.GetComponentInChildren<Text>();settingsLabel.rectTransform.anchoredPosition=Vector2.zero;settingsLabel.rectTransform.sizeDelta=new Vector2(240,96);settingsLabel.alignment=TextAnchor.MiddleCenter;settingsLabel.fontSize=32;
            JournalUiBuilder.Action(home.Menu.transform.Find("Field Journal"),"Account Entry","account.title",new Vector2(140,-355),new Vector2(260,116),screen.Open,out _,false);
            var safe=home.Menu.transform.parent;var page=JournalUiBuilder.Sheet(safe,"Account Journal",out screen.View);
            JournalUiBuilder.Label(page,"Title","account.title",new Vector2(-420,391),new Vector2(600,55),34);
            JournalUiBuilder.Action(page,"Back","ui.action.back",new Vector2(695,394),new Vector2(180,120),screen.Back,out _);
            JournalUiBuilder.Rule(page,new Vector2(-760,342),new Vector2(760,342));
            screen.Status=Field(page,"Account state","account.status",265,out _);
            screen.Provider=Field(page,"Provider","account.provider",150,out _);
            screen.Cloud=Field(page,"Cloud","account.cloud",35,out _);screen.Cloud.rectTransform.sizeDelta=new Vector2(620,100);
            screen.Support=Field(page,"Support ID","account.support",-100,out _);screen.Support.fontSize=28;
            screen.CopySupportButton=JournalUiBuilder.Action(page,"Copy Support","account.copy",new Vector2(-160,-130),new Vector2(150,110),screen.CopySupport,out _);
            screen.Alpha=Field(page,"Alpha","account.alpha",-245,out _);
            screen.Character=JournalUiBuilder.Label(page,"Character","",new Vector2(-420,355),new Vector2(620,60),30,TextAnchor.MiddleCenter);screen.Character.gameObject.SetActive(false);
            var actions=Rect("Account Actions",page,Vector2.one*.5f,Vector2.zero,new Vector2(1720,1000));screen.NormalActions=actions.gameObject;
            screen.ConnectButton=Action(actions,"Connect","account.connect",new Vector2(430,250),screen.Connect);JournalStyle.Action(screen.ConnectButton,true);
            screen.SyncButton=Action(actions,"Sync","account.sync",new Vector2(430,250),screen.Sync);
            screen.AlphaButton=Action(actions,"Record Alpha","account.enroll",new Vector2(430,110),screen.Enroll);
            Action(actions,"Help","account.help",new Vector2(430,-30),screen.Help);
            Action(actions,"Privacy","account.privacy",new Vector2(270,-170),screen.Privacy,new Vector2(280,120));
            screen.SignOutButton=Action(actions,"Sign out","account.sign_out",new Vector2(590,-170),screen.SignOut,new Vector2(280,120));
            screen.DeleteButton=Action(actions,"Delete account","account.delete",new Vector2(430,-310),screen.Delete);
            screen.ExistingButton=Action(page,"Use existing account","account.use_existing",new Vector2(430,100),screen.UseExisting);
            screen.UseCloudButton=Action(page,"Use cloud profile","account.use_cloud",new Vector2(430,100),screen.UseCloud);
            screen.KeepLocalButton=Action(page,"Keep local","account.keep_local",new Vector2(430,-50),screen.KeepLocal);
            page.Find("Back").SetAsLastSibling();Help(safe,screen);Deletion(safe,screen);screen.View.SetActive(false);screen.FeedbackView.SetActive(false);screen.DeleteView.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
        private static Text Field(Transform page,string name,string heading,float y,out Text label)
        {
            var row=Rect(name+" row",page,Vector2.one*.5f,new Vector2(-430,y),new Vector2(640,110));
            label=JournalUiBuilder.Label(row,name+" heading",heading,new Vector2(0,34),new Vector2(610,38),28);
            return JournalUiBuilder.Label(row,name,"",new Vector2(0,-15),new Vector2(610,62),32);
        }
        private static Button Action(Transform page,string name,string key,Vector2 p,UnityEngine.Events.UnityAction call,Vector2? size=null)=>JournalUiBuilder.Action(page,name,key,p,size??new Vector2(600,120),call,out _);
        private static void Help(Transform safe,AccountProfileScreen s)
        {
            var page=JournalUiBuilder.Sheet(safe,"Help and Feedback",out s.FeedbackView);
            JournalUiBuilder.Label(page,"Title","account.help",new Vector2(-420,391),new Vector2(650,60),34);
            Action(page,"Back","ui.action.back",new Vector2(695,394),s.CloseHelp,new Vector2(180,120));
            JournalUiBuilder.Label(page,"Category Heading","account.category",new Vector2(-420,275),new Vector2(560,50),27);
            var img=Image("Category",page,"white",Vector2.one*.5f,new Vector2(-420,195),new Vector2(570,120),true);img.color=new Color32(230,220,190,255);s.Category=img.gameObject.AddComponent<Dropdown>();
            var caption=JournalUiBuilder.Label(img.transform,"Caption","",Vector2.zero,new Vector2(530,100),34);s.Category.captionText=caption;s.Category.options.Clear();foreach(var c in new[]{"Bug Report","Game Feedback","Account / Cloud","Purchase","Player Report","Other"})s.Category.options.Add(new Dropdown.OptionData(c));
            // Dropdown's standard template is authored so touch selection is functional.
            var template=Rect("Template",img.transform,new Vector2(.5f,0),new Vector2(0,-4),new Vector2(570,440));var ti=template.gameObject.AddComponent<Image>();ti.color=new Color32(235,225,198,255);var scroll=template.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.viewport=template;
            template.gameObject.AddComponent<RectMask2D>();var content=Rect("Content",template,new Vector2(.5f,1),Vector2.zero,new Vector2(570,576));content.pivot=new Vector2(.5f,1);scroll.content=content;
            var item=Image("Item",content,"white",new Vector2(.5f,1),new Vector2(0,-48),new Vector2(560,96),true);item.color=new Color32(218,208,176,255);var toggle=item.gameObject.AddComponent<Toggle>();toggle.targetGraphic=item;toggle.graphic=item;var itemText=JournalUiBuilder.Label(item.transform,"Item Label","",Vector2.zero,new Vector2(520,90),32);s.Category.itemText=itemText;s.Category.template=template;template.gameObject.SetActive(false);
            JournalUiBuilder.Label(page,"Consent explanation","account.consent",new Vector2(-420,-45),new Vector2(570,180),32);
            var consentImage=Image("Diagnostic Consent",page,"white",Vector2.one*.5f,new Vector2(-650,-235),new Vector2(112,112),true);consentImage.color=new Color32(210,198,160,255);s.Consent=consentImage.gameObject.AddComponent<Toggle>();s.Consent.targetGraphic=consentImage;var mark=Image("Check",consentImage.transform,"white",Vector2.one*.5f,Vector2.zero,new Vector2(60,60));mark.color=JournalStyle.Ember;s.Consent.graphic=mark;s.Consent.isOn=false;
            JournalUiBuilder.Label(page,"Consent label","account.attach",new Vector2(-345,-235),new Vector2(440,112),32);
            var box=Image("Message",page,"white",Vector2.one*.5f,new Vector2(430,70),new Vector2(620,490),true);box.color=new Color32(242,233,206,255);s.FeedbackText=box.gameObject.AddComponent<InputField>();s.FeedbackText.lineType=InputField.LineType.MultiLineNewline;s.FeedbackText.characterLimit=1500;
            var input=JournalUiBuilder.Label(box.transform,"Input text","",Vector2.zero,new Vector2(570,440),34,TextAnchor.UpperLeft);s.FeedbackText.textComponent=input;var hint=JournalUiBuilder.Label(box.transform,"Placeholder","account.feedback_hint",Vector2.zero,new Vector2(570,440),34,TextAnchor.UpperLeft);hint.color=new Color32(125,125,102,255);s.FeedbackText.placeholder=hint;
            s.SubmitButton=Action(page,"Send feedback","account.send",new Vector2(430,-280),s.Submit);s.FeedbackStatus=JournalUiBuilder.Label(page,"Feedback status","",new Vector2(430,-380),new Vector2(620,100),22);page.Find("Back").SetAsLastSibling();
        }
        private static void Deletion(Transform safe,AccountProfileScreen s)
        {
            var page=JournalUiBuilder.Sheet(safe,"Delete Account",out s.DeleteView);JournalUiBuilder.Label(page,"Title","account.delete",new Vector2(-420,391),new Vector2(620,60),34);
            s.DeleteMessage=JournalUiBuilder.Label(page,"Warning","account.delete_warning",new Vector2(0,25),new Vector2(1400,580),36,TextAnchor.UpperLeft);
            Action(page,"Cancel","ui.action.cancel",new Vector2(-420,-350),s.CancelDelete);var confirm=Action(page,"Confirm deletion","account.delete_confirm_button",new Vector2(430,-350),s.ConfirmDelete);JournalStyle.Action(confirm,destructive:true);
        }
    }
}
