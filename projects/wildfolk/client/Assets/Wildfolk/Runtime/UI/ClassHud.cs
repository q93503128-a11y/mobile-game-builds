using System;
using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class ClassHud:MonoBehaviour
    {
        public PlayerInventory Player;public GameObject ActiveView,MarkerView,BuildingView,MedicineView;
        public Text ClassName,Ability,QuarryText,BuildingText,MarkerText,ServiceProgress,MeaningText;
        public Button ActiveButton,MarkButton,MeaningButton,RepairButton,DismantleButton,MedicineButton;
        public ClassHoldButton GuardHold,RepairHold,DismantleHold;
        public Text[] TeamMarkers;
        private PlayerClass role;private NetworkClassActions network;private PlayerInputCommands commands;private PlayerTeamMarker marker;
        private StructureMaintenance target;private WorldTurret turret;private Health ally;
        private float next;private byte meaning;private readonly Collider2D[] nearby=new Collider2D[64];
        private void Start(){GuardHold.Held+=Guard;RepairHold.Held+=h=>Service(BuildingServiceKind.Repair,h);DismantleHold.Held+=h=>Service(BuildingServiceKind.Dismantle,h);ActiveButton.onClick.AddListener(Overclock);MarkButton.onClick.AddListener(Mark);MedicineButton.onClick.AddListener(Medicine);}
        private bool Bind()
        {
            if(Player==null){foreach(var p in NetworkPlayer.Active)if(p.IsOwner){Player=p.GetComponent<PlayerInventory>();break;}}
            if(Player==null)return false;if(role!=null)return true;role=Player.GetComponent<PlayerClass>();network=Player.GetComponent<NetworkClassActions>();commands=Player.GetComponent<PlayerInputCommands>();marker=Player.GetComponent<PlayerTeamMarker>();return role!=null;
        }
        private void Guard(bool held){if(!Bind() || !role.Has(ClassTrait.Guard))return;if(network!=null)network.Request(ClassAction.Guard,held);else role.SetGuard(held);}
        private void Service(BuildingServiceKind kind,bool held){if(!Bind())return;if(network!=null)network.Request(ClassAction.Building,held,target,default,(byte)kind);else Player.GetComponent<PlayerBuildingService>().Hold(target,kind,held);}
        public void Overclock(){if(!Bind() || !role.FreeRepair)return;if(network!=null)network.Request(ClassAction.Overclock,false,turret);else role.Overclock(turret);}
        public void Medicine(){if(!Bind() || ally==null)return;if(network!=null)network.Request(ClassAction.Medicine,false,ally);else Player.GetComponent<PlayerMedicine>().Begin(ally);}
        public void Mark(){if(!Bind())return;var point=Player.GetComponent<IAimableExplorationTool>().ObservedPoint;if(network!=null)network.Request(ClassAction.Mark,false,null,point,meaning);else marker.Place(point,meaning);}
        public void CycleMeaning(){meaning=(byte)((meaning+1)%3);MeaningText.text=L.Text("class.meaning."+meaning);}
        private void Set(Text label,string value){if(label.text!=value)label.text=value;}
        private void Update()
        {
            if(Time.unscaledTime<next)return;next=Time.unscaledTime+.2f;if(!Bind() || role.Definition==null)return;
            var running=commands.Session.IsRunning && Player.GetComponent<Health>().IsAlive;
            Set(ClassName,role.Definition.Name);ClassName.gameObject.SetActive(running);
            var state=network!=null && network.IsSpawned?network.State.Value:default;var remote=network!=null && !network.IsServer;
            ActiveView.SetActive(running && role.HasActive);GuardHold.enabled=role.Has(ClassTrait.Guard);
            var guarded=remote?state.Guarding:role.Guarding;var ready=remote?state.ActiveReady:role.ActiveReady;
            Set(Ability,L.Text(role.Has(ClassTrait.Guard)?guarded?"class.guarding":"class.guard":"class.overclock")+(ready>role.Now?"\n"+Mathf.CeilToInt((float)(ready-role.Now))+"s":""));
            ActiveButton.interactable=role.Has(ClassTrait.Guard) || ready<=role.Now;
            MarkerView.SetActive(running && marker.Available);MeaningButton.gameObject.SetActive(role.Value(ClassEffect.SignalMeaning)>0);MeaningText.text=L.Text("class.meaning."+meaning);
            Set(MarkerText,marker.ObservedType??L.Text("class.mark"));
            // One bounded context query at5Hz. No global placeable/creature scan.
            var count=running?Physics2D.OverlapCircle(Player.transform.position,Player.Settings.StationInteractionRange,ContactFilter2D.noFilter,nearby):0;target=null;turret=null;ally=null;float best=float.MaxValue,turretBest=float.MaxValue,allyBest=float.MaxValue;
            for(var i=0;i<count;i++)
            {
                var delta=(Vector2)(nearby[i].transform.position-Player.transform.position);var d=delta.sqrMagnitude;
                var structure=nearby[i].GetComponentInParent<StructureMaintenance>();if(structure!=null && structure.GetComponent<WorldPlaceable>().PlayerBuilt && d<best && ClassActionRules.Clear(Player.GetComponent<Health>(),structure.GetComponent<Health>())){target=structure;best=d;}
                var gun=nearby[i].GetComponentInParent<WorldTurret>();if(gun!=null && d<turretBest){turret=gun;turretBest=d;}
                var peer=nearby[i].GetComponentInParent<NetworkPlayer>();if(peer!=null && peer.gameObject!=Player.gameObject && peer.Health.IsAlive && d<allyBest){ally=peer.Health;allyBest=d;}
            }
            BuildingView.SetActive(running && target!=null);
            if(target!=null)
            {
                var place=target.GetComponent<WorldPlaceable>();var text=place.Definition.Item.LocalizedName+"  "+place.Health.Current+" / "+place.Health.Maximum+(target.Buffer>0?"  +"+target.Buffer:"");
                if(!role.FreeRepair && place.Health.Current<place.Health.Maximum)foreach(var cost in target.RepairCosts())text+=" · "+cost.Item.LocalizedName+"×"+cost.Count;
                Set(BuildingText,text);RepairButton.interactable=target.CanRepair(role);DismantleButton.interactable=!target.HasContents();
                if(target.HasContents())Set(ServiceProgress,L.Text("class.empty_first"));else{var service=Player.GetComponent<PlayerBuildingService>();Set(ServiceProgress,service.Active?L.Format("class.holding",Mathf.Max(0,(float)(service.Until-role.Now)).ToString("0.0")):L.Text("class.hold_hint"));}
            }
            MedicineView.SetActive(running && role.CanHealAlly && ally!=null && Player.SelectedStack.Item?.Consumable?.Kind==ConsumableKind.Medicine);
            RefreshQuarry(remote,state);var at=0;
            if(network==null){if(marker.Until>role.Now)ShowMarker(at++,marker.Point,marker.Meaning);}
            else foreach(var peer in NetworkPlayer.Active){var m=peer.GetComponent<NetworkClassActions>().State.Value;if(m.MarkerUntil>role.Now && at<TeamMarkers.Length)ShowMarker(at++,m.Marker,m.Meaning);}
            for(;at<TeamMarkers.Length;at++)TeamMarkers[at].gameObject.SetActive(false);
        }
        private void ShowMarker(int index,Vector2 point,byte purpose)
        {var d=point-(Vector2)Player.transform.position;TeamMarkers[index].gameObject.SetActive(true);Set(TeamMarkers[index],L.Text("class.meaning."+purpose)+" · "+L.Text("utility.bearing."+PlayerExplorationUtility.Bearing(d))+" · "+Mathf.RoundToInt(d.magnitude)+"m");}
        private void RefreshQuarry(bool remote,ClassActionState state)
        {
            var active=role.Has(ClassTrait.Quarry) && (remote?state.QuarryUntil>role.Now:role.Quarry!=null);QuarryText.gameObject.SetActive(active);if(!active)return;
            var detailed=role.Value(ClassEffect.QuarryDetail)>0;var delta=remote?Vector2.zero:(Vector2)(role.Quarry.transform.position-Player.transform.position);
            var bearing=remote?state.Bearing:PlayerExplorationUtility.Bearing(delta);var distance=remote?state.Distance:detailed?delta.magnitude<8?0:delta.magnitude<20?1:delta.magnitude<40?2:3:delta.magnitude<15?0:delta.magnitude<40?1:2;
            var hp=remote?state.Condition:detailed?role.Quarry.Ratio>.75f?0:role.Quarry.Ratio>.5f?1:role.Quarry.Ratio>.25f?2:3:role.Quarry.Ratio>.6f?0:role.Quarry.Ratio>.25f?1:2;
            Set(QuarryText,L.Text("class.quarry")+" · "+L.Text("utility.bearing."+bearing)+"\n"+L.Text("class.distance."+distance)+" · "+L.Text("class.condition."+(detailed?"fine.":"coarse.")+hp));
        }
    }
}
