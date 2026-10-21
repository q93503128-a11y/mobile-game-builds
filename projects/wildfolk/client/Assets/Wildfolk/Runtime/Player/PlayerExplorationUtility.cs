using UnityEngine;
namespace Wildfolk
{
    // Only an accepted, completed survey performs a bounded physics query. No world generation API is called.
    public sealed class PlayerExplorationUtility:MonoBehaviour
    {
        public ContentCatalog Catalog;
        public SurveyState LocalSurvey {get;private set;}
        public ExplorationSignal LocalSignal {get;private set;}
        public double LocalSignalReady {get;private set;}
        public int QueryCount {get;private set;}
        public int LastColliderCount {get;private set;}
        public double LastQueryMilliseconds {get;private set;}
        private readonly Collider2D[] candidates=new Collider2D[128];
        private readonly RaycastHit2D[] barriers=new RaycastHit2D[32];
        private PlayerInventory bag;private PlayerInputCommands commands;private Health health;
        private NetworkExplorationUtility network;private WorldGenerator world;
        private ItemDefinition channelItem;private Vector2 origin;private double nextTick;
        public double Now=>network!=null && network.IsSpawned ? network.NetworkManager.ServerTime.Time : Time.unscaledTimeAsDouble;
        private bool Remote=>network!=null && network.IsSpawned && !network.IsServer;
        public SurveyState Survey=>Remote ? network.Survey.Value : LocalSurvey;
        public ExplorationSignal Signal=>Remote ? network.Signal.Value : LocalSignal;
        public double SignalReady=>Remote ? network.SignalReady.Value : LocalSignalReady;
        private void Awake(){bag=GetComponent<PlayerInventory>();commands=GetComponent<PlayerInputCommands>();health=GetComponent<Health>();network=GetComponent<NetworkExplorationUtility>();}
        private void OnEnable()=>health.Damaged+=Cancel;
        private void OnDisable(){health.Damaged-=Cancel;ResetTransient();}
        private WorldGenerator World {get{if(world==null)world=FindFirstObjectByType<WorldGenerator>();return world;}}
        public bool Use()
        {
            if(network==null)network=GetComponent<NetworkExplorationUtility>();
            if(Remote || !SimulationAuthority.Allows(this) || !health.IsAlive || !commands.Session.IsRunning || bag.SelectedStack.IsEmpty)return false;
            var item=bag.SelectedStack.Item;var d=item.ExplorationUtility;if(d==null || World==null || World.Map==null)return false;
            var inventory=GetComponent<NetworkInventory>();if(inventory!=null && inventory.IsSpawned && !inventory.CanOperate)return false;
            if(d.Kind==ExplorationUtilityKind.Signal)
            {
                if(Now<LocalSignalReady)return false;
                LocalSignalReady=Now+d.CooldownSeconds;LocalSignal=new ExplorationSignal{Point=transform.position,Started=Now,Until=Now+d.LifetimeSeconds,Range=d.Range};network?.Publish();return true;
            }
            if(Now<LocalSurvey.Ready || LocalSurvey.Phase==SurveyPhase.Channel || commands.Move.sqrMagnitude>.01f)return false;
            GetComponent<PlayerCombat>().CancelAttack();GetComponent<PlayerLocomotion>().CancelDash();
            channelItem=item;origin=transform.position;LocalSurvey=new SurveyState{Phase=SurveyPhase.Channel,Item=-1,Until=Now+d.ChannelSeconds,Ready=Now+d.CooldownSeconds};network?.Publish();return true;
        }
        public void Cancel()
        {
            if(Remote || LocalSurvey.Phase!=SurveyPhase.Channel)return;
            var s=LocalSurvey;s.Phase=SurveyPhase.Cancelled;s.Until=Now+2;LocalSurvey=s;channelItem=null;network?.Publish();
        }
        public void ResetTransient(){LocalSurvey=default;LocalSignal=default;LocalSignalReady=0;channelItem=null;nextTick=0;if(network!=null && network.IsSpawned && network.IsServer)network.Publish();}
        private void Update()
        {
            if(Remote || Now<nextTick)return;nextTick=Now+.1;
            if(!health.IsAlive){Cancel();if(LocalSignal.Until>0){LocalSignal=default;network?.Publish();}return;}
            if(LocalSignal.Until>0 && Now>=LocalSignal.Until){LocalSignal=default;network?.Publish();}
            var s=LocalSurvey;
            if(s.Phase==SurveyPhase.Channel)
            {
                if(!commands.Session.IsRunning || bag.SelectedStack.Item!=channelItem || commands.Move.sqrMagnitude>.01f || Vector2.Distance(origin,transform.position)>.2f){Cancel();return;}
                if(Now>=s.Until){s.Phase=Query(channelItem.ExplorationUtility.Range,out var result,out var bearing);s.Item=result;s.Bearing=bearing;s.Until=Now+channelItem.ExplorationUtility.LifetimeSeconds;LocalSurvey=s;channelItem=null;network?.Publish();}
            }
            else if(s.Phase!=SurveyPhase.None && Now>=s.Until){s.Phase=SurveyPhase.None;s.Item=-1;LocalSurvey=s;network?.Publish();}
        }
        public static byte Bearing(Vector2 delta)=>(byte)((Mathf.RoundToInt(Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg/45)+8)%8);
        public static Vector2 Direction(byte bearing)=>new Vector2(Mathf.Cos(bearing*45*Mathf.Deg2Rad),Mathf.Sin(bearing*45*Mathf.Deg2Rad));
        public static bool SameRegion(WorldMap map,Vector2 a,Vector2 b)
        {if(!map.Streamed)return map.Contains(a)&&map.Contains(b);return map.Continental.Underground(a)==map.Continental.Underground(b) && map.RegionBounds(a).Contains(b);}
        private bool CanMine(ResourceDefinition d)
        {for(var i=0;i<bag.Model.Capacity;i++){var stack=bag.Model.GetSlot(i);if(!stack.IsEmpty && stack.Item.Tool!=null && d.CanHarvest(stack.Item.Tool))return true;}return false;}
        private bool ClearPath(Vector2 from,Vector2 to)
        {
            var delta=to-from;var length=delta.magnitude;
            for(var step=.5f;step<length;step+=.5f)if(!World.Map.IsWalkable(from+delta*(step/length),0))return false;
            var count=Physics2D.Raycast(from,delta.normalized,ContactFilter2D.noFilter,barriers,length);
            if(count==barriers.Length)return false;
            for(var i=0;i<count;i++){var p=barriers[i].collider.GetComponentInParent<WorldPlaceable>();if(p!=null && p.Definition.IsBarrier)return false;}
            return true;
        }
        private SurveyPhase Query(float radius,out int item,out byte bearing)
        {
            var start=System.Diagnostics.Stopwatch.GetTimestamp();QueryCount++;item=-1;bearing=0;
            var from=(Vector2)transform.position;LastColliderCount=Physics2D.OverlapCircle(from,radius,ContactFilter2D.noFilter,candidates);
            if(LastColliderCount==candidates.Length){LastQueryMilliseconds=Elapsed(start);return SurveyPhase.Crowded;}
            var best=float.MaxValue;ResourceNode chosen=null;
            for(var i=0;i<LastColliderCount;i++)
            {
                var identity=candidates[i].GetComponentInParent<ResourceIdentity>();if(identity==null || identity.Owner.World!=World || identity.Rule<0 || identity.Rule>=World.Definition.Resources.Length)continue;
                var n=identity.Node;var p=(Vector2)n.transform.position;
                if(World.Definition.Resources[identity.Rule].Family!=ResourceFamily.Mineral || !n.Health.IsAlive || !SameRegion(World.Map,from,p) || World.Map.Streamed && !World.Chunks.IsActive(p) || !CanMine(n.Definition))continue;
                var distance=(p-from).sqrMagnitude;if(distance>=best || distance>radius*radius || !ClearPath(from,p))continue;best=distance;chosen=n;
            }
            if(chosen!=null){item=System.Array.IndexOf(Catalog.Items,chosen.Definition.DropItem);bearing=Bearing((Vector2)chosen.transform.position-from);}
            LastQueryMilliseconds=Elapsed(start);return item>=0 ? SurveyPhase.Found : SurveyPhase.Empty;
        }
        private static double Elapsed(long start)=>(System.Diagnostics.Stopwatch.GetTimestamp()-start)*1000.0/System.Diagnostics.Stopwatch.Frequency;
        public bool TryPeerSignal(out Vector2 point)
        {
            point=default;if(World==null || World.Map==null)return false;double newest=-1;
            foreach(var actor in NetworkPlayer.Active)
            {if(actor==null || !actor.IsSpawned || actor.gameObject==gameObject)continue;var utility=actor.GetComponent<PlayerExplorationUtility>();if(utility==null)continue;var signal=utility.Signal;if(signal.Until<=Now || signal.Started<newest || signal.Range<=0 || Vector2.Distance(transform.position,signal.Point)>signal.Range || !SameRegion(World.Map,transform.position,signal.Point))continue;newest=signal.Started;point=signal.Point;}
            return newest>=0;
        }
    }
}
