using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    // Extends the existing canvas once. Simulation, authority and persistence live in their own components.
    public sealed class TierThreeHud : MonoBehaviour
    {
        private static TierThreeHud current;
        private bool focused;public static bool IsFocused=>current!=null && current.focused;
        private Sprite white;
        private PlayerInventory player;private GameplaySession session;private PlayerNavigation navigation;
        private WorldTurret turret;private SignalBeacon beacon;private GameObject modal,scopeView,navButton;
        private RectTransform panel;private Text title,status,help;private Button fire,reload,load;
        private readonly List<GameObject> modeViews=new List<GameObject>();
        private readonly Button[] ammo=new Button[3],module=new Button[5],remove=new Button[5];
        private Text[] ammoText=new Text[3],moduleText=new Text[5];
        private GameObject turretView,beaconView,navView;private Text beaconLabel,navLabel;
        private readonly List<BeaconPoint> choices=new List<BeaconPoint>();private int choice;
        private uint shownRevision=uint.MaxValue;private string shownLanguage;
        private Font font;private Sprite background;private float nextBind,nextStatus;
        private void Awake(){current=this;}
        public static bool OpenTurret(WorldTurret target,PlayerInventory owner)
        {if(current==null || owner==null || !target.InRange(owner))return false;current.Bind(owner);if(current.turret!=null)current.turret.Changed-=current.RefreshTurret;current.shownRevision=uint.MaxValue;current.turret=target;target.Changed+=current.RefreshTurret;current.beacon=null;current.Open(current.turretView);current.RefreshTurret();return true;}
        public static void OpenBeacon(SignalBeacon target,PlayerInventory owner)
        {if(current==null)return;current.Bind(owner);current.beacon=target;current.turret=null;current.Open(current.beaconView);current.beaconLabel.text=L.Format("ui.beacon.number",target.Number);}
        private void Bind(PlayerInventory owner)
        {if(owner==player)return;player=owner;session=owner.GetComponent<PlayerInputCommands>().Session;navigation=owner.GetComponent<PlayerNavigation>();if(modal==null)Build();}
        private RectTransform Rect(string name,Transform parent,Vector2 position,Vector2 size)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=position;r.sizeDelta=size;return r;}
        private Text Label(Transform parent,Vector2 p,Vector2 size,string key,int pixels=24)
        {var t=Rect("Label",parent,p,size).gameObject.AddComponent<Text>();t.font=font;t.fontSize=pixels;t.verticalOverflow=VerticalWrapMode.Overflow;t.lineSpacing=.9f;t.color=new Color32(226,222,193,255);t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;t.text=L.Has(key) ? L.Text(key) : key;if(L.Has(key)){var label=t.gameObject.AddComponent<LocalizedLabel>();label.Key=key;}return t;}
        private Button Button(Transform parent,Vector2 p,Vector2 size,string key,UnityEngine.Events.UnityAction action,out Text text)
        {var r=Rect("Action",parent,p,size);var image=r.gameObject.AddComponent<Image>();image.sprite=background;image.type=Image.Type.Sliced;image.color=new Color32(52,65,52,255);var b=AudioButton.Add(r.gameObject);b.targetGraphic=image;b.navigation=new Navigation{mode=Navigation.Mode.None};var colors=b.colors;colors.pressedColor=new Color(.65f,.72f,.6f,1);b.colors=colors;b.onClick.AddListener(action);text=Label(r,Vector2.zero,size-Vector2.one*12,key,22);return b;}
        private Button Button(Transform parent,Vector2 p,Vector2 size,string key,UnityEngine.Events.UnityAction action)=>Button(parent,p,size,key,action,out _);
        private void Build()
        {
            var canvas=FindFirstObjectByType<GameplayHud>()?.GetComponentInParent<Canvas>();if(canvas==null){var arena=FindFirstObjectByType<MultiplayerArena>();if(arena!=null)canvas=arena.GetComponentInChildren<Canvas>();}if(canvas==null)canvas=FindFirstObjectByType<Canvas>();
            font=Resources.Load<Font>("NotoSansKR");background=player.GetComponent<PlayerCrafting>().Catalog.Recipes[0].Result.Icon;
            var existing=canvas.GetComponentsInChildren<Image>(true);foreach(var image in existing)if(image.sprite!=null && image.sprite.name=="white"){white=image.sprite;break;}foreach(var image in existing)if(image.sprite!=null && image.sprite.name=="panel"){background=image.sprite;break;}
            Transform safe=canvas.transform;var layout=canvas.GetComponentInChildren<ModalLayout>(true);if(layout!=null)safe=layout.SafeArea;
            var nb=Button(safe,new Vector2(655,385),new Vector2(170,54),"ui.navigation.choose",OpenNavigation);navButton=nb.gameObject;
            scopeView=Rect("Telescope controls",safe,Vector2.zero,new Vector2(1000,650)).gameObject;
            var aim=Rect("Look direction",scopeView.transform,Vector2.zero,new Vector2(1100,600));var a=aim.gameObject.AddComponent<Image>();a.color=new Color(0,0,0,.005f);aim.gameObject.AddComponent<DirectionDrag>().Aim=d=>player.GetComponent<PlayerTelescope>().Aim(d);
            Label(scopeView.transform,new Vector2(0,300),new Vector2(700,42),"ui.telescope.aim");Button(scopeView.transform,new Vector2(0,-270),new Vector2(220,60),"ui.telescope.close",()=>player.GetComponent<PlayerTelescope>().Exit());scopeView.SetActive(false);
            var mr=Rect("Focused controls",safe,Vector2.zero,Vector2.zero);mr.anchorMin=Vector2.zero;mr.anchorMax=Vector2.one;mr.offsetMin=mr.offsetMax=Vector2.zero;modal=mr.gameObject;var scrim=modal.AddComponent<Image>();scrim.color=new Color(.04f,.07f,.05f,.8f);
            panel=Rect("Control panel",mr,Vector2.zero,new Vector2(1100,780));var pi=panel.gameObject.AddComponent<Image>();pi.sprite=white;pi.type=Image.Type.Simple;pi.color=new Color32(32,45,35,255);panel.gameObject.SetActive(false);var fitting=panel.gameObject.AddComponent<ModalLayout>();fitting.SafeArea=(RectTransform)safe;fitting.ReferenceSize=panel.sizeDelta;panel.gameObject.SetActive(true);
            title=Label(panel,new Vector2(0,334),new Vector2(860,60),"",32);Button(panel,new Vector2(465,329),new Vector2(96,58),"ui.action.close",Close);
            turretView=Rect("Turret",panel,Vector2.zero,new Vector2(1050,640)).gameObject;beaconView=Rect("Beacon",panel,Vector2.zero,new Vector2(1050,640)).gameObject;navView=Rect("Navigation",panel,Vector2.zero,new Vector2(1050,640)).gameObject;modeViews.Add(turretView);modeViews.Add(beaconView);modeViews.Add(navView);
            status=Label(turretView.transform,new Vector2(-230,238),new Vector2(510,80),"",24);help=Label(turretView.transform,new Vector2(-230,154),new Vector2(510,64),"ui.turret.manual",21);
            var pad=Rect("Turret aim",turretView.transform,new Vector2(-235,-25),new Vector2(370,230));var padImage=pad.gameObject.AddComponent<Image>();padImage.sprite=background;padImage.color=new Color32(49,61,48,255);Label(pad,Vector2.zero,new Vector2(340,70),"ui.turret.aim");pad.gameObject.AddComponent<DirectionDrag>().Aim=d=>Operate(TurretOperation.Aim,0,Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg-90);
            fire=Button(turretView.transform,new Vector2(-125,-230),new Vector2(190,64),"ui.turret.fire",()=>Operate(TurretOperation.Fire));reload=Button(turretView.transform,new Vector2(-345,-230),new Vector2(190,64),"ui.turret.reload",()=>Operate(TurretOperation.Reload));
            load=Button(turretView.transform,new Vector2(-235,-314),new Vector2(410,58),"ui.turret.deposit",Deposit);
            for(var i=0;i<3;i++){var index=i;ammo[i]=Button(turretView.transform,new Vector2(52+i*156,233),new Vector2(144,100),"",()=>Operate(TurretOperation.Withdraw,index),out ammoText[i]);}
            Label(turretView.transform,new Vector2(215,152),new Vector2(470,42),"ui.turret.modules",24);
            for(var i=0;i<5;i++){var index=i;module[i]=Button(turretView.transform,new Vector2(176,85-i*88),new Vector2(370,70),"",()=>Install(index),out moduleText[i]);remove[i]=Button(turretView.transform,new Vector2(430,85-i*88),new Vector2(120,70),"ui.turret.remove",()=>Operate(TurretOperation.Remove,index));}
            beaconLabel=Label(beaconView.transform,new Vector2(0,180),new Vector2(800,70),"",30);Button(beaconView.transform,new Vector2(0,35),new Vector2(500,72),"ui.navigation.track",()=>{navigation.SelectNavigation(NavigationMode.Beacon,beacon.GetComponent<WorldPlaceable>().SaveId);Close();});Button(beaconView.transform,new Vector2(0,-80),new Vector2(500,72),"ui.beacon.recover",()=>{var net=player.GetComponent<NetworkTierThreePlayer>();if(net==null)beacon.Recover(player);else net.RecoverBeacon(beacon.GetComponent<WorldPlaceable>());Close();});
            Button(navView.transform,new Vector2(-230,215),new Vector2(400,68),"ui.navigation.off",()=>Choose(NavigationMode.Off));Button(navView.transform,new Vector2(230,215),new Vector2(400,68),"ui.navigation.auto",()=>Choose(NavigationMode.Automatic));Button(navView.transform,new Vector2(-230,115),new Vector2(400,68),"ui.navigation.home",()=>Choose(NavigationMode.Home));Button(navView.transform,new Vector2(230,115),new Vector2(400,68),"ui.navigation.exit",()=>Choose(NavigationMode.CaveExit));
            navLabel=Label(navView.transform,new Vector2(0,-30),new Vector2(700,80),"ui.navigation.no_target",28);Button(navView.transform,new Vector2(-380,-130),new Vector2(100,64),"‹",()=>Cycle(-1));Button(navView.transform,new Vector2(380,-130),new Vector2(100,64),"›",()=>Cycle(1));Button(navView.transform,new Vector2(0,-130),new Vector2(600,64),"ui.navigation.track",()=>{if(choices.Count>0){navigation.SelectNavigation(NavigationMode.Beacon,choices[choice].Id);Close();}});modal.SetActive(false);
        }
        private void Open(GameObject view){focused=true;session.OpenInventory();foreach(var mode in modeViews)mode.SetActive(mode==view);modal.SetActive(true);modal.transform.SetAsLastSibling();title.text=turret!=null ? turret.GetComponent<WorldPlaceable>().Definition.Item.LocalizedName : beacon!=null ? L.Text("item.signal_beacon.name") : L.Text("ui.navigation.choose");}
        public static bool CloseFocused(){if(current==null || current.modal==null || !current.modal.activeSelf)return false;current.Close();return true;}
        public void Close(){focused=false;if(turret!=null)turret.Changed-=RefreshTurret;if(modal!=null)modal.SetActive(false);if(session!=null)session.CloseInventory();turret=null;beacon=null;}
        private void Choose(NavigationMode mode){navigation.SelectNavigation(mode);Close();}
        private void OpenNavigation()
        {turret=null;beacon=null;choices.Clear();var net=player.GetComponent<NetworkNavigation>();if(net!=null && !net.IsServer){foreach(var b in net.Beacons)choices.Add(new BeaconPoint(b.Id.ToString(),b.Number,b.Position));}else choices.AddRange(BeaconDirectory.Points.Values);choices.Sort((a,b)=>a.Number.CompareTo(b.Number));choice=0;Cycle(0);Open(navView);}
        private void Cycle(int step){if(choices.Count==0){navLabel.text=L.Text("ui.navigation.no_target");return;}choice=(choice+step+choices.Count)%choices.Count;navLabel.text=L.Format("ui.beacon.number",choices[choice].Number);}
        public bool Operate(TurretOperation op,int slot=0,float angle=0)
        {if(turret==null)return false;var net=player.GetComponent<NetworkTierThreePlayer>();var ok=net==null ? turret.Execute(player,op,slot,angle) : net.Operate(turret,op,slot,angle);if(!ok)PlayerFeedback.ShowKey("feedback.turret_unavailable");RefreshTurret();return ok;}
        private void Deposit(){if(turret==null)return;for(var i=0;i<player.Model.Capacity;i++)if(player.Model.GetSlot(i).Item==turret.Definition.Ammunition){Operate(TurretOperation.Deposit,i);return;}PlayerFeedback.ShowKey("feedback.no_ammo");}
        private void Install(int role){for(var i=0;i<player.Model.Capacity;i++){var s=player.Model.GetSlot(i);if(!s.IsEmpty && s.Item.TurretModule!=null && (int)s.Item.TurretModule.Slot==role){Operate(TurretOperation.Install,i);return;}}PlayerFeedback.ShowKey("feedback.turret_module");}
        private void RefreshTurret()
        {
            if(turret==null)return;status.text=turret.GetComponent<Health>().Current+" / "+turret.GetComponent<Health>().Maximum+"   ·   "+turret.Definition.Ammunition.LocalizedName+" ×"+turret.AmmoCount()+"\n"+(turret.ReloadRemaining>.05f ? L.Format("ui.turret.reloading",turret.ReloadRemaining.ToString("0.0")) : L.Text(turret.Loaded ? "ui.turret.ready" : "ui.turret.unloaded"));
            if(turret.OverclockRemaining>0)status.text+=" · "+L.Text("class.overclock")+" "+Mathf.CeilToInt(turret.OverclockRemaining)+"s";
            fire.interactable=turret.Loaded && turret.ReloadRemaining<=0 && turret.AmmoCount()>0;reload.interactable=!turret.Loaded && turret.ReloadRemaining<=0 && turret.AmmoCount()>0;
            if(shownRevision==turret.Revision && shownLanguage==L.Language)return;shownRevision=turret.Revision;shownLanguage=L.Language;
            for(var i=0;i<3;i++){var s=turret.Ammo.GetSlot(i);ammo[i].interactable=i<turret.AmmoSlots && !s.IsEmpty;ammoText[i].text=i>=turret.AmmoSlots ? L.Text("ui.turret.locked_slot") : s.IsEmpty ? L.Text("ui.turret.empty") : s.Item.LocalizedName+"\n×"+s.Count;}
            for(var i=0;i<5;i++){moduleText[i].text=L.Text("module."+(TurretModuleSlot)i)+"\n"+(turret.Modules[i]==null ? L.Text("ui.turret.install") : turret.Modules[i].LocalizedName);remove[i].interactable=turret.Modules[i]!=null;}
        }
        private void Update()
        {
            if(player==null){if(Time.unscaledTime<nextBind)return;nextBind=Time.unscaledTime+.5f;var arena=FindFirstObjectByType<MultiplayerArena>();if(arena!=null){if(arena.LocalPlayer!=null)Bind(arena.LocalPlayer.GetComponent<PlayerInventory>());}else{var actor=FindFirstObjectByType<PlayerInventory>();if(actor!=null)Bind(actor);}return;}
            scopeView.SetActive(player.GetComponent<PlayerTelescope>().IsViewing);navButton.SetActive(navigation.HasCompass && session.IsRunning);
            if(!modal.activeSelf)return;if(session.State!=SessionState.Inventory || turret!=null && !turret.InRange(player) || turretView.activeSelf && turret==null){Close();return;}
            if(turret!=null && Time.unscaledTime>=nextStatus){nextStatus=Time.unscaledTime+.15f;RefreshTurret();}
        }
        private void OnDestroy(){if(modal!=null)Destroy(modal);if(scopeView!=null)Destroy(scopeView);if(navButton!=null)Destroy(navButton);if(current==this)current=null;}
    }
}
