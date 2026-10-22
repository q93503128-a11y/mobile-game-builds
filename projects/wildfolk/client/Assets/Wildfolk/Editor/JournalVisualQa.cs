using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Wildfolk.Editor
{
    public static class JournalVisualQa
    {
        public static string Phase {get;private set;}="idle";
        private const string Root="Captures/ui-redesign";
        public static void Begin()
        {
            if(!Application.isPlaying || !DevelopmentProfile.Isolated)throw new InvalidOperationException("Isolated Play required");
            var runner=new GameObject("Journal Visual QA").AddComponent<DevelopmentCoroutineHost>();UnityEngine.Object.DontDestroyOnLoad(runner.gameObject);runner.StartCoroutine(Guard(Flow()));
        }
        private static void Log(string text){Directory.CreateDirectory(Root);File.AppendAllText(Root+"/visual-qa.tsv",DateTime.UtcNow.ToString("O")+"\t"+text+"\n");}
        private static void Check(bool value,string text){Log((value?"PASS ":"FAIL ")+text);if(!value)throw new InvalidOperationException(text);}
        private static IEnumerator Guard(IEnumerator flow)
        {
            Phase="running";var stack=new Stack<IEnumerator>();stack.Push(flow);
            while(stack.Count>0){bool more;object value;try{more=stack.Peek().MoveNext();value=more?stack.Peek().Current:null;}catch(Exception e){Phase="failed";Log(e.ToString());yield break;}if(!more){stack.Pop();continue;}if(value is IEnumerator child)stack.Push(child);else yield return value;}
            Phase="complete";Log("complete");
        }
        private static IEnumerator Shot(string name)
        {yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Root+"/"+name+".png");yield return new WaitForEndOfFrame();yield return null;Log("capture "+name+" "+Screen.width+"x"+Screen.height);}
        private static void TextFits(Transform view)
        {
            Canvas.ForceUpdateCanvases();var book=view.GetComponentInChildren<ModalLayout>().transform;var corners=new Vector3[4];foreach(var t in view.GetComponentsInChildren<Text>())
            {
                if(!t.isActiveAndEnabled || string.IsNullOrEmpty(t.text))continue;Check(t.preferredHeight<=t.rectTransform.rect.height+1,"text fits "+t.name+" height="+t.preferredHeight+"/"+t.rectTransform.rect.height+" "+t.text.Replace('\n',' '));
                t.rectTransform.GetWorldCorners(corners);foreach(var corner in corners){var p=book.InverseTransformPoint(corner);Check(Mathf.Abs(p.x)<=791 && Mathf.Abs(p.y)<=431,"text within page "+t.name+" "+p);}
            }
        }
        private static void Inputs(Transform view)
        {
            Canvas.ForceUpdateCanvases();var corners=new Vector3[4];foreach(var b in view.GetComponentsInChildren<Button>())
            {
                if(!b.isActiveAndEnabled || !b.interactable)continue;var r=(RectTransform)b.transform;r.GetWorldCorners(corners);var a=RectTransformUtility.WorldToScreenPoint(null,corners[0]);var z=RectTransformUtility.WorldToScreenPoint(null,corners[2]);
                Check(z.x-a.x>=47.5f && z.y-a.y>=47.5f,"touch target "+b.name+" "+(z-a));
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=(a+z)*.5f},hits);Check(hits.Count>0 && (hits[0].gameObject.transform==b.transform || hits[0].gameObject.transform.IsChildOf(b.transform)),"top raycast "+b.name);
            }
        }
        private static AccountProgression Sample()
        {var a=AccountProgression.New();a.Selected="hunter";foreach(var p in a.Classes){if(p.Id=="engineer")continue;p.Owned=true;p.XP=1800;p.Nodes=new[]{"A1","A2","A3","B1","B2","B3"};}return a;}
        private static IEnumerator Flow()
        {
            var home=UnityEngine.Object.FindFirstObjectByType<HomeScreen>();Check(home!=null,"MainMenu active before visual QA");ClassProfiles.Store.Write(ClassProfiles.LocalId,AccountProgression.New());
            var s=home.GetComponent<ClassLobbyScreen>();
            foreach(var width in new[]{1920,1280})
            {
                var language=width==1920?"ko":"en";VisualGameView.Show(width,width==1920?1080:720);L.SetLanguage(language);home.ShowHome();yield return new WaitForSecondsRealtime(.4f);yield return Shot("20-main-"+width);
                s.Open();s.SelectCard(1);Check(!s.SelectButton.interactable && s.PurchaseButton.gameObject.activeSelf,"Gold class locked");TextFits(s.View.transform);yield return Shot("21-gold-locked-"+width);
                ClassProfiles.Store.Write(ClassProfiles.LocalId,Sample());s.Refresh();
                for(var i=0;i<6;i++)
                {
                    s.SelectCard(i);for(var n=0;n<9;n++){s.SelectNode(n);TextFits(s.View.transform);}s.SelectNode(i==5?1:0);Inputs(s.View.transform);yield return Shot("22-class-"+ClassCatalog.Current.Classes[i].Id+"-"+width);
                }
                Check(!s.PurchaseButton.interactable && s.PurchaseButton.gameObject.activeSelf,"Engineer premium locked without provider");
                s.SelectCard(2);s.RespecButton.onClick.Invoke();Check(ClassProfiles.Local.Find("hunter").Nodes.Length==0,"free respec UI");s.SelectNode(0);Check(s.InvestButton.interactable,"learn button available after respec");s.InvestButton.onClick.Invoke();Check(ClassProfiles.Local.Find("hunter").Nodes.Length==1,"learn UI spends one point");s.SelectCard(1);s.SelectButton.onClick.Invoke();Check(ClassProfiles.Local.Selected=="warrior","select UI commits ownership");s.Back();ClassProfiles.Store.Write(ClassProfiles.LocalId,AccountProgression.New());
            }
            yield return ClassFoundationQa.SharedGrowth();
            var character=UnityEngine.Object.FindFirstObjectByType<CharacterScreen>();var tree=UnityEngine.Object.FindFirstObjectByType<SkillTreeScreen>();
            foreach(var width in new[]{1920,1280})
            {
                VisualGameView.Show(width,width==1920?1080:720);L.SetLanguage(width==1920?"ko":"en");character.Session.OpenCharacter();yield return new WaitForSecondsRealtime(.35f);TextFits(character.View.transform);Inputs(character.View.transform);yield return Shot("23-character-"+width);
                character.TreeButton.onClick.Invoke();yield return null;for(var i=0;i<6;i++){tree.Nodes[i].onClick.Invoke();TextFits(tree.View.transform);}tree.Select(0);Inputs(tree.View.transform);yield return Shot("24-shared-tree-"+width);var rankBefore=tree.Growth.Rank(0);tree.InvestButton.onClick.Invoke();Check(tree.Growth.Rank(0)==rankBefore+1,"shared learn UI invokes existing authority");tree.Back();character.Close();
            }
            // Simulated safe-area reduction uses the existing layout transform; no device/window input.
            VisualGameView.ShowReference();character.Session.OpenCharacter();yield return null;var layout=character.View.GetComponentInChildren<ModalLayout>();var safe=layout.SafeArea;var min=safe.offsetMin;var max=safe.offsetMax;safe.offsetMin=new Vector2(90,0);safe.offsetMax=new Vector2(-90,0);yield return null;yield return null;TextFits(character.View.transform);yield return Shot("25-character-safe-area");safe.offsetMin=min;safe.offsetMax=max;character.Close();
            var save=UnityEngine.Object.FindFirstObjectByType<WorldSaveService>();Check(save.Repository.Path.StartsWith(DevelopmentProfile.Directory,StringComparison.OrdinalIgnoreCase),"isolated save only");Check(save.Save(),"journal QA isolated Save13");SceneFlow.Home(save.Repository);yield return null;yield return null;L.SetLanguage("ko");VisualGameView.ShowReference();ClassProfiles.Store.Write(ClassProfiles.LocalId,AccountProgression.New());
        }
    }
}
