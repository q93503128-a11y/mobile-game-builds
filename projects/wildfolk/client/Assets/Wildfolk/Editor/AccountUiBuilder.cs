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
            var settings=(RectTransform)home.SettingsButton.transform;settings.anchoredPosition=new Vector2(-88,-235);settings.sizeDelta=new Vector2(206,72);home.SettingsButton.GetComponentInChildren<Text>().fontSize=25;
            JournalUiBuilder.Action(home.Menu.transform.Find("Field Journal"),"Account Entry","account.title",new Vector2(140,-235),new Vector2(206,72),screen.Open,out _,false);
            var safe=home.Menu.transform.parent;var page=JournalUiBuilder.Book(safe,"Account Journal",out screen.View);
            JournalUiBuilder.Label(page,"Title","account.title",new Vector2(-420,391),new Vector2(600,55),34);
            JournalUiBuilder.Action(page,"Back","ui.action.back",new Vector2(695,394),new Vector2(160,72),screen.Back,out _);
            JournalUiBuilder.Rule(page,new Vector2(-760,342),new Vector2(760,342));
            screen.Status=Field(page,"Account state","account.status",280,out _);
            screen.Provider=Field(page,"Provider","account.provider",180,out _);
            screen.Cloud=Field(page,"Cloud","account.cloud",80,out _);
            screen.Support=Field(page,"Support ID","account.support",-20,out _);screen.Support.fontSize=22;
            JournalUiBuilder.Action(page,"Copy Support","account.copy",new Vector2(-155,-20),new Vector2(135,60),screen.CopySupport,out var copy);copy.fontSize=22;
            screen.Alpha=Field(page,"Alpha","account.alpha",-120,out _);
            screen.Created=Field(page,"Created","account.created",-220,out _);
            screen.Character=JournalUiBuilder.Label(page,"Character","",new Vector2(-420,-337),new Vector2(620,100),25,TextAnchor.MiddleCenter);
            screen.ConnectButton=Action(page,"Connect","account.connect",new Vector2(430,280),screen.Connect);
            screen.SyncButton=Action(page,"Sync","account.sync",new Vector2(430,190),screen.Sync);
            screen.AlphaButton=Action(page,"Record Alpha","account.enroll",new Vector2(430,100),screen.Enroll);
            Action(page,"Help","account.help",new Vector2(430,10),screen.Help);
            Action(page,"Privacy","account.privacy",new Vector2(270,-80),screen.Privacy,new Vector2(280,72));
            screen.SignOutButton=Action(page,"Sign out","account.sign_out",new Vector2(590,-80),screen.SignOut,new Vector2(280,72));
            Action(page,"Delete account","account.delete",new Vector2(430,-170),screen.Delete);
            screen.ExistingButton=Action(page,"Use existing account","account.use_existing",new Vector2(430,-265),screen.UseExisting);
            screen.UseCloudButton=Action(page,"Use cloud profile","account.use_cloud",new Vector2(430,-265),screen.UseCloud);
            screen.KeepLocalButton=Action(page,"Keep local","account.keep_local",new Vector2(430,-355),screen.KeepLocal);
            page.Find("Back").SetAsLastSibling();Help(safe,screen);Deletion(safe,screen);screen.View.SetActive(false);screen.FeedbackView.SetActive(false);screen.DeleteView.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
        private static Text Field(Transform page,string name,string heading,float y,out Text label)
        {label=JournalUiBuilder.Label(page,name+" heading",heading,new Vector2(-430,y+34),new Vector2(610,34),22);return JournalUiBuilder.Label(page,name,"",new Vector2(-430,y-3),new Vector2(610,44),27);}
        private static Button Action(Transform page,string name,string key,Vector2 p,UnityEngine.Events.UnityAction call,Vector2? size=null)=>JournalUiBuilder.Action(page,name,key,p,size??new Vector2(600,72),call,out _);
        private static void Help(Transform safe,AccountProfileScreen s)
        {
            var page=JournalUiBuilder.Book(safe,"Help and Feedback",out s.FeedbackView);
            JournalUiBuilder.Label(page,"Title","account.help",new Vector2(-420,391),new Vector2(650,60),34);
            Action(page,"Back","ui.action.back",new Vector2(695,394),s.CloseHelp,new Vector2(160,72));
            JournalUiBuilder.Label(page,"Category Heading","account.category",new Vector2(-420,275),new Vector2(560,50),27);
            var img=Image("Category",page,"white",Vector2.one*.5f,new Vector2(-420,195),new Vector2(570,70),true);img.color=new Color32(230,220,190,255);s.Category=img.gameObject.AddComponent<Dropdown>();
            var caption=JournalUiBuilder.Label(img.transform,"Caption","",Vector2.zero,new Vector2(530,60),27);s.Category.captionText=caption;s.Category.options.Clear();foreach(var c in new[]{"Bug Report","Game Feedback","Account / Cloud","Purchase","Player Report","Other"})s.Category.options.Add(new Dropdown.OptionData(c));
            // Dropdown's standard template is authored so touch selection is functional.
            var template=Rect("Template",img.transform,new Vector2(.5f,0),new Vector2(0,-4),new Vector2(570,330));var ti=template.gameObject.AddComponent<Image>();ti.color=new Color32(235,225,198,255);var scroll=template.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.viewport=template;
            var content=Rect("Content",template,new Vector2(.5f,1),Vector2.zero,new Vector2(570,330));content.pivot=new Vector2(.5f,1);scroll.content=content;
            var item=Image("Item",content,"white",new Vector2(.5f,1),new Vector2(0,-27),new Vector2(560,54),true);item.color=new Color32(218,208,176,255);var toggle=item.gameObject.AddComponent<Toggle>();toggle.targetGraphic=item;toggle.graphic=item;var itemText=JournalUiBuilder.Label(item.transform,"Item Label","",Vector2.zero,new Vector2(520,50),24);s.Category.itemText=itemText;s.Category.template=template;template.gameObject.SetActive(false);
            JournalUiBuilder.Label(page,"Consent explanation","account.consent",new Vector2(-420,-50),new Vector2(570,150),25);
            var consentImage=Image("Diagnostic Consent",page,"white",Vector2.one*.5f,new Vector2(-650,-185),new Vector2(52,52),true);consentImage.color=new Color32(210,198,160,255);s.Consent=consentImage.gameObject.AddComponent<Toggle>();s.Consent.targetGraphic=consentImage;var mark=Image("Check",consentImage.transform,"white",Vector2.one*.5f,Vector2.zero,new Vector2(32,32));mark.color=JournalStyle.Ember;s.Consent.graphic=mark;s.Consent.isOn=false;
            JournalUiBuilder.Label(page,"Consent label","account.attach",new Vector2(-375,-185),new Vector2(470,64),25);
            var box=Image("Message",page,"white",Vector2.one*.5f,new Vector2(430,70),new Vector2(620,490),true);box.color=new Color32(242,233,206,255);s.FeedbackText=box.gameObject.AddComponent<InputField>();s.FeedbackText.lineType=InputField.LineType.MultiLineNewline;s.FeedbackText.characterLimit=1500;
            var input=JournalUiBuilder.Label(box.transform,"Input text","",Vector2.zero,new Vector2(570,440),26,TextAnchor.UpperLeft);s.FeedbackText.textComponent=input;var hint=JournalUiBuilder.Label(box.transform,"Placeholder","account.feedback_hint",Vector2.zero,new Vector2(570,440),26,TextAnchor.UpperLeft);hint.color=new Color32(125,125,102,255);s.FeedbackText.placeholder=hint;
            s.SubmitButton=Action(page,"Send feedback","account.send",new Vector2(430,-280),s.Submit);s.FeedbackStatus=JournalUiBuilder.Label(page,"Feedback status","",new Vector2(430,-380),new Vector2(620,100),22);page.Find("Back").SetAsLastSibling();
        }
        private static void Deletion(Transform safe,AccountProfileScreen s)
        {
            var page=JournalUiBuilder.Book(safe,"Delete Account",out s.DeleteView);JournalUiBuilder.Label(page,"Title","account.delete",new Vector2(-420,391),new Vector2(620,60),34);
            s.DeleteMessage=JournalUiBuilder.Label(page,"Warning","account.delete_warning",Vector2.zero,new Vector2(1360,440),32,TextAnchor.MiddleCenter);
            Action(page,"Cancel","ui.action.cancel",new Vector2(-420,-320),s.CancelDelete);Action(page,"Confirm deletion","account.delete_confirm_button",new Vector2(430,-320),s.ConfirmDelete);
        }
    }
}
