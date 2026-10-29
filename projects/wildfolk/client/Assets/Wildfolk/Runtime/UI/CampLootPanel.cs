using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class CampLootPanel : MonoBehaviour
    {
        private static CampLootPanel current;
        public static bool IsOpen=>current!=null && current.camp!=null && current.camp.IsAvailable;
        private IWorldSupplies camp;private PlayerInventory player;private GameplaySession session;
        private Font font;private Text status;private readonly Button[] takes=new Button[6];private readonly Text[] labels=new Text[6];private readonly Image[] icons=new Image[6];
        private RectTransform Rect(string name,Transform parent,Vector2 point,Vector2 size)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=point;r.sizeDelta=size;return r;}
        private Text Label(Transform parent,Vector2 point,Vector2 size,string text,int pixels=24)
        {var t=Rect("Text",parent,point,size).gameObject.AddComponent<Text>();t.font=font;t.fontSize=pixels;t.text=text;t.color=new Color32(231,226,202,255);t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;return t;}
        private Text Localized(Transform parent,Vector2 point,Vector2 size,string key,int pixels=24){var t=Label(parent,point,size,L.Text(key),pixels);var label=t.gameObject.AddComponent<LocalizedLabel>();label.Key=key;label.Refresh();return t;}
        private Button Button(Transform parent,Vector2 point,Vector2 size,string text,UnityEngine.Events.UnityAction action)
        {var r=Rect("Take action",parent,point,size);var i=r.gameObject.AddComponent<Image>();i.color=new Color32(67,82,62,255);var b=AudioButton.Add(r.gameObject);b.targetGraphic=i;b.navigation=new Navigation{mode=Navigation.Mode.None};b.onClick.AddListener(action);Localized(r,Vector2.zero,size-Vector2.one*8,text,22);return b;}
        public static void Open(IWorldSupplies target,PlayerInventory visitor)
        {
            CloseFocused();var canvas=visitor.GetComponentInParent<Canvas>();if(canvas==null)canvas=Object.FindFirstObjectByType<Canvas>();if(canvas==null)return;
            var go=new GameObject("Camp supplies",typeof(RectTransform));go.transform.SetParent(canvas.transform,false);current=go.AddComponent<CampLootPanel>();current.camp=target;current.player=visitor;current.session=visitor.GetComponent<PlayerInputCommands>().Session;
            current.Build(canvas);target.Changed+=current.Refresh;visitor.Model.Changed+=current.Refresh;L.Changed+=current.Refresh;current.session.StateChanged+=current.StateChanged;current.session.OpenInventory();current.Refresh();
        }
        private void Build(Canvas canvas)
        {
            font=Resources.Load<Font>("NotoSansKR");var root=(RectTransform)transform;root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;var scrim=gameObject.AddComponent<Image>();scrim.color=new Color(.04f,.07f,.05f,.88f);
            var panel=Rect("Supplies panel",root,Vector2.zero,new Vector2(1080,760));panel.gameObject.SetActive(false);panel.gameObject.AddComponent<Image>().color=new Color32(31,43,33,255);var existing=canvas.GetComponentInChildren<ModalLayout>(true);var fit=panel.gameObject.AddComponent<ModalLayout>();fit.SafeArea=existing!=null ? existing.SafeArea : root;fit.ReferenceSize=panel.sizeDelta;
            Localized(panel,new Vector2(0,312),new Vector2(760,60),camp.TitleKey,32);Button(panel,new Vector2(450,310),new Vector2(110,60),"ui.action.close",()=>CloseFocused());
            Localized(panel,new Vector2(0,240),new Vector2(960,65),camp.ExplainKey,22);
            for(var i=0;i<6;i++)
            {var slot=i;var x=i%2==0 ? -250 : 250;var y=120-i/2*130;var row=Rect("Supply",panel,new Vector2(x,y),new Vector2(480,112));row.gameObject.AddComponent<Image>().color=new Color32(45,59,43,255);icons[i]=Rect("Item",row,new Vector2(-190,0),new Vector2(64,64)).gameObject.AddComponent<Image>();icons[i].preserveAspect=true;labels[i]=Label(row,new Vector2(-10,0),new Vector2(250,90),"",23);takes[i]=Button(row,new Vector2(175,0),new Vector2(110,68),"ui.camp.take",()=>Take(slot));}
            status=Label(panel,new Vector2(0,-310),new Vector2(940,70),"",22);panel.gameObject.SetActive(true);
        }
        public bool Take(int slot)
        {if(camp==null || !camp.IsAvailable)return false;var ok=camp.RequestTake(player,slot);if(!ok)PlayerFeedback.ShowKey("feedback.slot_space");return ok;}
        private void Refresh()
        {if(camp==null || !camp.IsAvailable || camp.Location==null || camp.Loot==null)return;var empty=true;for(var i=0;i<6;i++){var s=i<camp.Loot.Length ? camp.Loot[i] : null;var item=s!=null && s.Count>0 ? camp.Catalog.Item(s.Item) : null;empty&=item==null;icons[i].enabled=item!=null;icons[i].sprite=item==null ? null : item.Icon;labels[i].text=item==null ? L.Text("ui.camp.empty_slot") : item.LocalizedName+"\n×"+s.Count;takes[i].interactable=item!=null && camp.Secured;}status.text=L.Text(!camp.Secured ? "ui.camp.guarded" : empty ? "ui.camp.empty" : "ui.camp.remaining");}
        private void StateChanged(){if(session.State!=SessionState.Inventory)CloseFocused();}
        private void Update(){if(camp==null || !camp.IsAvailable || camp.Location==null || player==null || !camp.InRange(player)){CloseFocused();}}
        public static bool CloseFocused()
        {if(current==null)return false;var panel=current;current=null;var session=panel.session;panel.Unbind();Object.Destroy(panel.gameObject);if(session!=null)session.CloseInventory();return true;}
        private void Unbind(){if(camp!=null)camp.Changed-=Refresh;if(player!=null && player.Model!=null)player.Model.Changed-=Refresh;L.Changed-=Refresh;if(session!=null)session.StateChanged-=StateChanged;camp=null;}
        private void OnDestroy(){Unbind();if(current==this)current=null;}
    }
}
