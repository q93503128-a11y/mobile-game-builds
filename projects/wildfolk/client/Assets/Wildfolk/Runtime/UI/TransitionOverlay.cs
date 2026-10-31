using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Wildfolk.Presentation;
namespace Wildfolk
{
    // One transient, persistent surface for real scene/network operations. No fabricated percentage.
    public sealed class TransitionOverlay:MonoBehaviour
    {
        private static TransitionOverlay instance;
        private Text title,phase;private RectTransform spinner;private Button cancel;private string key;
        private NetworkSession network;private Action recovery;private float started;private bool warned,loading,awaitingScene;
        public static bool Visible=>instance!=null;
        private static TransitionOverlay Create(string phaseKey)
        {
            if(instance!=null)instance.Close();
            var root=new GameObject("Journey Transition",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));DontDestroyOnLoad(root);
            var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32000;
            var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            var surface=new GameObject("Forest shade",typeof(RectTransform),typeof(Image));surface.transform.SetParent(root.transform,false);var rect=(RectTransform)surface.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;surface.GetComponent<Image>().color=new Color32(25,42,31,255);
            var safe=new GameObject("Safe Area",typeof(RectTransform),typeof(SafeAreaLayout)).GetComponent<RectTransform>();safe.SetParent(root.transform,false);safe.anchorMin=Vector2.zero;safe.anchorMax=Vector2.one;safe.offsetMin=safe.offsetMax=Vector2.zero;
            instance=root.AddComponent<TransitionOverlay>();instance.key=phaseKey;instance.started=Time.realtimeSinceStartup;
            instance.title=Label(safe,"Lowfire",230,64);instance.title.text=L.Text("brand.name");instance.phase=Label(safe,"Phase",-20,38);
            var arc=new GameObject("Indeterminate",typeof(RectTransform),typeof(StaminaArc));arc.transform.SetParent(safe,false);instance.spinner=(RectTransform)arc.transform;instance.spinner.sizeDelta=new Vector2(110,110);instance.spinner.anchoredPosition=new Vector2(0,110);var graphic=arc.GetComponent<StaminaArc>();graphic.Fill=1;graphic.color=JournalStyle.Paper;graphic.raycastTarget=false;
            var action=new GameObject("Cancel",typeof(RectTransform),typeof(Image),typeof(Button));action.transform.SetParent(safe,false);var ar=(RectTransform)action.transform;ar.sizeDelta=new Vector2(650,132);ar.anchoredPosition=new Vector2(0,-230);instance.cancel=action.GetComponent<Button>();instance.cancel.targetGraphic=action.GetComponent<Image>();JournalStyle.Action(instance.cancel);var text=Label(ar,"Cancel label",0,36);text.rectTransform.sizeDelta=new Vector2(610,110);text.color=JournalStyle.Ink;text.text=L.Text("ui.action.cancel");instance.cancel.onClick.AddListener(instance.Cancel);action.SetActive(false);
            L.Changed+=instance.Refresh;instance.Refresh();return instance;
        }
        private static Text Label(Transform parent,string name,float y,int size)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);var text=go.GetComponent<Text>();text.rectTransform.sizeDelta=new Vector2(1500,130);text.rectTransform.anchoredPosition=new Vector2(0,y);text.font=Resources.Load<Font>("NotoSansKR");text.fontSize=size;text.alignment=TextAnchor.MiddleCenter;text.color=JournalStyle.Paper;text.raycastTarget=false;return text;
        }
        public static void Load(string scene,string phaseKey,Action failed)
        {var view=Create(phaseKey);view.loading=true;view.recovery=failed;view.StartCoroutine(view.LoadScene(scene));}
        private IEnumerator LoadScene(string scene)
        {
            yield return null;yield return null;AsyncOperation op=null;
            try{op=SceneManager.LoadSceneAsync(scene);}catch(Exception){Fail();}
            if(op==null)yield break;
            while(!op.isDone)yield return null;
            key=scene==SceneFlow.HomeScene?"loading.camp":"loading.world";Refresh();yield return null;
            if(scene==SceneFlow.GameplayScene)
            {
                var until=Time.realtimeSinceStartup+45;GameplaySession held=null;
                while(true)
                {
                    var save=FindFirstObjectByType<WorldSaveService>();
                    if(save!=null&&save.World.Map!=null)
                    {
                        if(held==null&&save.World.Session.IsRunning){held=save.World.Session;held.OpenPause();}
                        if(save.World.Chunks==null||save.World.Chunks.PendingCount==0&&save.World.Terrain.TerrainWorkCount==0)break;
                    }
                    if(Time.realtimeSinceStartup>until){Fail();yield break;}yield return null;
                }
                if(held!=null)held.Resume();
            }
            Close();
        }
        private void Fail(){loading=false;key="loading.failed";spinner.gameObject.SetActive(false);cancel.gameObject.SetActive(true);Refresh();}
        public static void Watch(NetworkSession session,string phaseKey="net.connecting")
        {var view=Create(phaseKey);view.network=session;view.awaitingScene=phaseKey=="net.entering";session.Changed+=view.NetworkChanged;view.cancel.gameObject.SetActive(!view.awaitingScene);}
        private void NetworkChanged()
        {
            if(network==null){Close();return;}
            if(network.State==NetworkSessionState.Idle||network.State==NetworkSessionState.Waiting&&!awaitingScene){Close();return;}
            if(network.State==NetworkSessionState.Waiting){key="net.entering";Refresh();return;}
            key=network.State==NetworkSessionState.Loading?"net.entering":network.State==NetworkSessionState.Playing?"loading.player":"net.connecting";cancel.gameObject.SetActive(network.State==NetworkSessionState.Connecting);Refresh();
        }
        private void Update()
        {
            spinner.Rotate(0,0,-180*Time.unscaledDeltaTime);
            if(network!=null&&network.State==NetworkSessionState.Playing){var arena=FindFirstObjectByType<MultiplayerArena>();if(arena!=null&&arena.LocalPlayer!=null){Close();return;}}
            if(!warned&&Time.realtimeSinceStartup-started>20){warned=true;if(network!=null&&network.State==NetworkSessionState.Playing)cancel.gameObject.SetActive(true);Refresh();}
        }
        private void Refresh(){title.text=L.Text("brand.name");phase.text=L.Text(key)+(warned&&key!="loading.failed"?"\n"+L.Text("loading.slow"):"");}
        private void Cancel(){if(loading)return;if(network!=null){network.Leave();Close();}else{var callback=recovery;Close();callback?.Invoke();}}
        private void Close(){if(instance==this)instance=null;gameObject.SetActive(false);Destroy(gameObject);}
        private void OnDestroy(){L.Changed-=Refresh;if(network!=null)network.Changed-=NetworkChanged;}
    }
}
