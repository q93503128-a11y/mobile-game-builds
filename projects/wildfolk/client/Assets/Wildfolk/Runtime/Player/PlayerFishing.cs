using UnityEngine;
namespace Wildfolk
{
    public enum FishingPhase { Idle,Casting,Waiting,Bite,Caught }
    public sealed class PlayerFishing : MonoBehaviour
    {
        public FishingLootTable CatchTable;public Sprite BobberSprite,BiteSprite;
        public FishingPhase Phase {get;private set;}
        public Vector2 WaterPoint {get;private set;}
        public int Catches {get;private set;}
        public FishingWaterContext Context {get;private set;}
        public int PendingCatch {get;private set;}=-1;
        private ItemDefinition castRod;private uint presentedCatch;
        private FishingPhase lastPresented;private float started;private Vector2 origin;private LineRenderer line;private SpriteRenderer bobber;
        private PlayerInventory bag;private PlayerInputCommands commands;private Health health;private PlayerEquipment equipment;private PlayerAvatar avatar;private NetworkActivities net;private NetworkPlayer networkPlayer;private WorldGenerator world;
        private void Awake(){bag=GetComponent<PlayerInventory>();commands=GetComponent<PlayerInputCommands>();health=GetComponent<Health>();equipment=GetComponent<PlayerEquipment>();avatar=GetComponent<PlayerAvatar>();net=GetComponent<NetworkActivities>();networkPlayer=GetComponent<NetworkPlayer>();}
        public bool Use()
        {
            if(!SimulationAuthority.Allows(this))return false;
            AdvancePhase();
            if(!CanFish())return false;
            if(Phase==FishingPhase.Bite)
            {var result=CatchTable.Entries[PendingCatch];if(bag.Model.TryExchange(System.Array.Empty<ItemAmount>(),result.Output,result.Amount)!=ExchangeResult.Success){Feedback("feedback.bag_full");return false;}Phase=FishingPhase.Caught;Catches++;EconomyTelemetry.Record(this,"fish");started=Time.time;if(net!=null && net.IsSpawned)net.PublishCatch(PendingCatch,result.Amount,WaterPoint);else PlayerFeedback.ShowCatch(result,result.Amount);return true;}
            if(Phase!=FishingPhase.Idle)return false;
            if(world==null)world=FindFirstObjectByType<WorldGenerator>();if(world==null)return false;
            var p=(Vector2)transform.position;var direction=avatar.Facing.Direction;
            if(!FishingWaterContext.TryCast(world.Map,p,direction,out var point) || !FishingWaterContext.TrySample(world.Map,point,world.GetComponent<WorldClock>().Model.Phase==DayPhase.Night,out var context) || CatchTable==null){Feedback("feedback.fishing_water");return false;}
            var entry=CatchTable.Roll(context,Random.value);if(entry<0)return false;
            WaterPoint=point;Context=context;PendingCatch=entry;castRod=bag.SelectedStack.Item;
            origin=p;started=Time.time;Phase=FishingPhase.Casting;net?.Publish(ActivityKind.Fishing,WaterPoint,5.2f);ShowContext(context);return true;
        }
        private bool CanFish()=>health.IsAlive && commands.Session.IsRunning && !bag.SelectedStack.IsEmpty && bag.SelectedStack.Item.FishingRod;
        private void Feedback(string key){if(net!=null && net.IsSpawned)net.Feedback(key=="feedback.bag_full");else PlayerFeedback.ShowKey(key);}
        private void ShowContext(FishingWaterContext c){if(net!=null && net.IsSpawned)net.PublishContext(c);else PlayerFeedback.ShowWater(c);}
        private void AdvancePhase()
        {
            if(Phase==FishingPhase.Idle)return;
            if(!CanFish() || bag.SelectedStack.Item!=castRod || Vector2.Distance(origin,transform.position)>1.2f){Cancel();return;}
            var t=Time.time-started;if(Phase==FishingPhase.Caught){if(t>.6f)Cancel();return;}
            if(t>=5.2f){Cancel();return;}Phase=t<.45f ? FishingPhase.Casting : t<3 ? FishingPhase.Waiting : FishingPhase.Bite;
        }
        private void Update()
        {
            if(net!=null && net.IsSpawned && !net.IsServer){PresentReplica(net.Activity.Value);return;}
            AdvancePhase();if(Phase==FishingPhase.Idle){Show(false);return;}
            var t=Time.time-started;
            if(net!=null && net.IsSpawned)PresentCatch(net.Activity.Value);
            Present(t,Phase,WaterPoint);
        }
        private void PresentReplica(ActivityState s)
        {if(equipment.SelectedItem==null || !equipment.SelectedItem.FishingRod){Show(false);lastPresented=FishingPhase.Idle;return;}var kind=s.Kind;if(kind!=ActivityKind.Fishing && kind!=ActivityKind.FishingCaught){Show(false);lastPresented=FishingPhase.Idle;return;}var t=(float)(net.NetworkManager.ServerTime.Time-s.Started);if(t>s.Duration){Show(false);lastPresented=FishingPhase.Idle;return;}PresentCatch(s);Present(t,kind==ActivityKind.FishingCaught ? FishingPhase.Caught : t<.45f ? FishingPhase.Casting : t<3 ? FishingPhase.Waiting : FishingPhase.Bite,s.Point);}
        private void PresentCatch(ActivityState s)
        {if(!net.IsOwner || s.Kind!=ActivityKind.FishingCaught || s.Sequence==presentedCatch || CatchTable==null || s.CatchIndex<0 || s.CatchIndex>=CatchTable.Entries.Length)return;presentedCatch=s.Sequence;PlayerFeedback.ShowCatch(CatchTable.Entries[s.CatchIndex],s.CatchCount);}
        private void Show(bool enabled)
        {
            if(line==null && !enabled)return;
            if(line==null){var go=new GameObject("Fishing line");go.transform.SetParent(transform,false);line=go.AddComponent<LineRenderer>();line.sharedMaterial=new Material(Shader.Find("Sprites/Default"));line.startColor=line.endColor=new Color(.64f,.62f,.46f);line.startWidth=line.endWidth=.025f;line.positionCount=3;line.sortingOrder=WorldSorting.Overlay;var floatGo=new GameObject("Bobber");floatGo.transform.SetParent(transform,false);bobber=floatGo.AddComponent<SpriteRenderer>();bobber.sortingOrder=WorldSorting.Overlay+1;floatGo.transform.localScale=Vector3.one*.3f;}
            line.enabled=bobber.enabled=enabled;
        }
        private void Present(float t,FishingPhase phase,Vector2 point)
        {if(phase==FishingPhase.Bite && lastPresented!=FishingPhase.Bite && (networkPlayer==null || networkPlayer.IsOwner))PlayerFeedback.ShowKey("feedback.fishing_bite");lastPresented=phase;Show(phase!=FishingPhase.Idle);if(phase==FishingPhase.Idle)return;var tip=equipment.HeldVisual.transform.position;var end=phase==FishingPhase.Caught ? (Vector3)Vector2.Lerp(point,transform.position,Mathf.Clamp01(t/.5f)) : (Vector3)Vector2.Lerp(transform.position,point,Mathf.Clamp01(t/.45f));end.y+=phase==FishingPhase.Bite ? Mathf.Sin(t*18)*.1f : Mathf.Sin(t*3)*.025f;line.SetPosition(0,tip);line.SetPosition(1,(tip+end)*.5f+Vector3.down*.3f);line.SetPosition(2,end);bobber.transform.localScale=Vector3.one*(phase==FishingPhase.Bite ? .7f : .3f);bobber.transform.position=end;bobber.sprite=phase==FishingPhase.Bite ? BiteSprite : BobberSprite;}
        public void ResetTransient()=>Cancel();
        private void Cancel(){if(Phase!=FishingPhase.Idle || net!=null && net.IsSpawned && net.IsServer && (net.Activity.Value.Kind==ActivityKind.Fishing || net.Activity.Value.Kind==ActivityKind.FishingCaught))net?.Publish(ActivityKind.None,default,0);Phase=FishingPhase.Idle;PendingCatch=-1;castRod=null;lastPresented=FishingPhase.Idle;Show(false);}
        private void OnDisable()=>Cancel();
        private void OnDestroy(){if(line!=null)Destroy(line.sharedMaterial);}
    }
}
