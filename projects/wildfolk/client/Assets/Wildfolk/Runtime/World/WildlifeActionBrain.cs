using UnityEngine;
namespace Wildfolk
{
    // Behaviours are composed from movement, temperament and action data; species IDs never select AI.
    public sealed class WildlifeActionBrain : MonoBehaviour,IPoolReset
    {
        public NeutralAnimal Animal;
        public Transform Pose,LeftAppendage,RightAppendage;
        public SpriteRenderer Body,Tell;
        public Sprite RestSprite,ActiveSprite;
        public WildlifePhase Phase {get;private set;}
        public float Started {get;private set;}
        public double ServerStarted {get;private set;}
        public Vector2 Direction {get;private set;}=Vector2.down;
        public bool Awakened {get;private set;}
        public Health Target {get;private set;}
        public int Impacts {get;private set;}
        public WildlifeActionDefinition Definition=>Animal.Member.Definition.Actions;
        private Rigidbody2D body;
        private WorldNavigation navigation;
        private Vector3 restScale;
        private Vector2 attackDirection,wander;
        private float nextWander,lastTick,alarmUntil;
        private bool impact,sharedAlert;
        private readonly Collider2D[] contacts=new Collider2D[32];
        private readonly Health[] struck=new Health[32];
        private readonly RaycastHit2D[] blocks=new RaycastHit2D[16];
        public void Bind(WorldNavigation worldNavigation)=>navigation=worldNavigation;
        private void Awake(){body=GetComponent<Rigidbody2D>();restScale=Pose.localScale;}
        private void OnEnable(){lastTick=Time.time;Animal.Health.Damaged+=Alert;Animal.Member.Alerted+=Receive;WildlifeActionScheduler.Register(this);}
        private void OnDisable(){Animal.Health.Damaged-=Alert;Animal.Member.Alerted-=Receive;WildlifeActionScheduler.Unregister(this);Target=null;}
        public void ResetForSpawn(){Phase=WildlifePhase.Idle;Started=Time.time;ServerStarted=0;Target=null;Awakened=false;impact=sharedAlert=false;alarmUntil=0;lastTick=Time.time;Pose.localScale=restScale;Impacts=0;}
        private void Receive(Vector2 origin,CreatureAlert alert)
        {if(!SimulationAuthority.Allows(this))return;alarmUntil=Time.time+6;Awakened=true;sharedAlert=true;}
        private void Alert()
        {
            if(!SimulationAuthority.Allows(this) || !Animal.Health.IsAlive)return;
            alarmUntil=Time.time+8;
            if(Animal.Member.Has(CreatureTrait.FleeTogether))Animal.Member.Broadcast(transform.position,CreatureAlert.Flee);
            if(Phase==WildlifePhase.Idle && Definition.Attack!=WildlifeAttack.None)Enter(WildlifePhase.Warning);
        }
        private void Enter(WildlifePhase phase)
        {Phase=phase;Started=Time.time;var net=GetComponent<NetworkCreature>();ServerStarted=net!=null && net.IsSpawned ? net.NetworkManager.ServerTime.Time : Time.time;if(phase==WildlifePhase.Action)impact=false;if(phase==WildlifePhase.Windup && Target!=null)attackDirection=((Vector2)(Target.transform.position-transform.position)).normalized;if(phase==WildlifePhase.Warning){if(!sharedAlert && Animal.Member.Has(CreatureTrait.PackAggro))Animal.Member.Broadcast(transform.position,CreatureAlert.Fight);sharedAlert=false;}}
        public void Tick()
        {
            var now=Time.time;var dt=Mathf.Clamp(now-lastTick,0,.15f);lastTick=now;
            if(!SimulationAuthority.Allows(this) || Animal.Session==null || !Animal.Session.WorldIsRunning || !Animal.Health.IsAlive)return;
            if(navigation==null && Animal.Region!=null)navigation=Animal.Region.GetComponent<WorldNavigation>();
            var world=navigation==null ? null : navigation.World;
            if(world!=null && world.Map.Streamed && !world.Chunks.Wants(transform.position))return;
            Target=SimulationAuthority.Target(this,Animal.Player==null ? null : Animal.Player.GetComponent<Health>());
            if(Target!=null && (!Target.IsAlive || world!=null && world.Map.Streamed && world.Map.Continental.Underground(Target.transform.position)!=world.Map.Continental.Underground(transform.position)))Target=null;
            var distance=Target==null ? float.MaxValue : Vector2.Distance(transform.position,Target.transform.position);
            var d=Definition;var elapsed=now-Started;
            if(d.Attack==WildlifeAttack.None)
            {
                var flee=distance<d.Detection || now<alarmUntil;
                if(flee && Target!=null){Direction=((Vector2)(transform.position-Target.transform.position)).normalized;Move(Direction,d.FleeSpeed,dt);}
                else Wander(dt);
                Present(now,(byte)WildlifePhase.Idle,0,Direction);return;
            }
            if(Phase==WildlifePhase.Idle)
            {
                var threatened=(distance<d.Detection || now<alarmUntil && distance<d.Leash) && (!d.Defensive || now<alarmUntil || distance<d.Detection*.55f);
                if(threatened){Enter(WildlifePhase.Warning);}
                else if(!d.Camouflage)Wander(dt);
            }
            else if(Phase==WildlifePhase.Warning)
            {if(elapsed>=d.WarningSeconds){Awakened=true;Enter(WildlifePhase.Windup);}}
            else if(Phase==WildlifePhase.Windup)
            {
                if(Target==null || distance>d.Leash || Vector2.Distance(transform.position,Animal.Member.Anchor)>d.Leash){Enter(WildlifePhase.Recovery);}
                else if(distance>d.EngageRange){Direction=((Vector2)(Target.transform.position-transform.position)).normalized;Move(Direction,d.ChaseSpeed,dt);attackDirection=Direction;Started=now;ServerStarted=GetComponent<NetworkCreature>() is NetworkCreature network && network.IsSpawned ? network.NetworkManager.ServerTime.Time : now;}
                else if(elapsed>=d.WindupSeconds)Enter(WildlifePhase.Action);
            }
            else if(Phase==WildlifePhase.Action)
            {
                var before=body.position;var after=before;Direction=attackDirection;
                if(d.Attack!=WildlifeAttack.Pulse && d.Attack!=WildlifeAttack.Bite && !Move(Direction,d.ActionSpeed,dt,out after))Enter(WildlifePhase.Recovery);
                if(!impact)Strike(before,after);
                if(elapsed>=d.ActionSeconds)Enter(WildlifePhase.Recovery);
            }
            else
            {
                if(d.Attack==WildlifeAttack.Swoop)Move(-attackDirection,d.ChaseSpeed,dt);
                if(elapsed>=d.RecoverySeconds){if(Target!=null && distance<d.Detection && Vector2.Distance(transform.position,Animal.Member.Anchor)<d.Leash)Enter(WildlifePhase.Windup);else{Enter(WildlifePhase.Idle);alarmUntil=0;}}
            }
            if(Vector2.Distance(body.position,Animal.Member.Anchor)>d.Leash && Phase!=WildlifePhase.Action)Move((Animal.Member.Anchor-body.position).normalized,d.WalkSpeed,dt);
            Present(now,(byte)Phase,Started,Direction);
        }
        private void Wander(float dt)
        {
            if(Time.time>=nextWander){wander=Random.insideUnitCircle.normalized;nextWander=Time.time+2.3f;}
            if(Vector2.Distance(body.position,Animal.Member.Anchor)>Animal.Member.Definition.RoamRadius)wander=(Animal.Member.Anchor-body.position).normalized;
            Direction=Vector2.ClampMagnitude(wander+Animal.Member.Cohesion(),1);Move(Direction,Definition.WalkSpeed,dt);
        }
        private bool Move(Vector2 direction,float speed,float dt)
        {return Move(direction,speed,dt,out _);}
        private bool Move(Vector2 direction,float speed,float dt,out Vector2 after)
        {
            after=body.position;if(direction.sqrMagnitude<.001f || dt<=0)return true;var delta=direction*speed*dt;if(Phase!=WildlifePhase.Action)delta+=Animal.Separation.Correction(dt);var step=delta.magnitude;direction=delta.normalized;
            // The same terrain and solid-obstacle sweep protects every locomotion style, including flight.
            var allowed=navigation==null ? step : navigation.AllowedDistance(body.position,direction,step,Animal.Member.Definition.SeparationRadius,GetComponent<Collider2D>());
            after=body.position+direction*allowed;body.MovePosition(after);return allowed>=step;
        }
        private void Strike(Vector2 before,Vector2 after)
        {
            var d=Definition;
            if(d.Attack==WildlifeAttack.Pulse)
            {
                impact=true;var count=Physics2D.OverlapCircle(after,d.Reach,new ContactFilter2D{useTriggers=true},contacts);var unique=0;
                for(var i=0;i<count;i++){var target=contacts[i].GetComponentInParent<Health>();if(target==null || target.GetComponent<PlayerInventory>()==null || !target.IsAlive || !LineClear(after,target))continue;var duplicate=false;for(var j=0;j<unique;j++)if(struck[j]==target)duplicate=true;if(duplicate)continue;struck[unique++]=target;if(target.ApplyDamage(d.Damage,false,null,GetComponent<Health>())>0)Impacts++;}
                return;
            }
            if(Target==null || ContactDistance(Target,before,after)>d.Reach || !LineClear(after,Target))return;
            if(Target.ApplyDamage(d.Damage,false,null,GetComponent<Health>())>0)Impacts++;impact=true;
        }
        private static float ContactDistance(Health target,Vector2 before,Vector2 after)
        {
            var delta=after-before;var center=(Vector2)target.transform.position;
            var closest=before+delta*(delta.sqrMagnitude>.0001f ? Mathf.Clamp01(Vector2.Dot(center-before,delta)/delta.sqrMagnitude) : 0);
            var collider=target.GetComponent<Collider2D>();
            return Vector2.Distance(closest,collider==null ? center : collider.ClosestPoint(closest));
        }
        private bool LineClear(Vector2 origin,Health target)
        {
            var delta=(Vector2)target.transform.position-origin;
            var n=Physics2D.Raycast(origin,delta.normalized,new ContactFilter2D{useTriggers=false},blocks,delta.magnitude);
            // The target's solid body is the hit surface, while walls and other solid bodies still occlude it.
            if(n==blocks.Length)return false;
            for(var i=0;i<n;i++)if(blocks[i].collider.GetComponentInParent<Health>()!=target)return false;
            return true;
        }
        public void PresentReplica(byte phase,double serverStarted,Vector2 direction,double serverNow,bool awakened)
        {Awakened=awakened;Present((float)serverNow,phase,(float)serverStarted,direction);}
        private void Present(float now,byte phase,float started,Vector2 direction)
        {
            var d=Definition;var state=(WildlifePhase)phase;var t=now-started;var active=state==WildlifePhase.Action;var preparing=state==WildlifePhase.Warning || state==WildlifePhase.Windup;
            var disguised=d.Camouflage && !Awakened && (state==WildlifePhase.Idle || state==WildlifePhase.Warning && t<d.WarningSeconds*.7f);
            if(Body!=null)Body.sprite=disguised ? RestSprite : ActiveSprite;
            var flight=d.Movement==WildlifeMovement.Flight;var hop=d.Movement==WildlifeMovement.Hop;
            var height=flight ? .35f+Mathf.Sin(now*7)*.1f : hop ? Mathf.Max(0,Mathf.Sin(now*7))*.24f : 0;
            Pose.localPosition=new Vector3(0,height,0);
            var angle=direction.sqrMagnitude>.01f ? Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg+90 : 0;
            Pose.localRotation=Quaternion.Euler(0,0,d.Camouflage && !Awakened ? 0 : angle);
            var stretch=active && d.Attack==WildlifeAttack.Charge ? new Vector3(.86f,1.24f,1) : preparing ? new Vector3(1.04f,.94f,1) : Vector3.one;
            if(d.Camouflage && !Awakened)stretch=Vector3.one*(1+Mathf.Sin(now*2)*.008f);
            Pose.localScale=Vector3.Scale(restScale,stretch);
            var flap=flight ? Mathf.Sin(now*(active?27:17))*32 : preparing ? Mathf.Sin(t*17)*8 : Mathf.Sin(now*6)*5;
            if(LeftAppendage!=null){LeftAppendage.localRotation=Quaternion.Euler(0,0,flap);LeftAppendage.gameObject.SetActive(!disguised);}
            if(RightAppendage!=null){RightAppendage.localRotation=Quaternion.Euler(0,0,-flap);RightAppendage.gameObject.SetActive(!disguised);}
            if(Tell!=null){Tell.enabled=preparing || active && d.Attack==WildlifeAttack.Pulse;Tell.color=state==WildlifePhase.Warning ? new Color(.8f,.7f,.36f,.6f) : new Color(.75f,.42f,.25f,.8f);Tell.transform.localScale=Vector3.one*(d.Attack==WildlifeAttack.Pulse ? d.Reach*2 : d.Reach*1.2f);}
        }
    }
}
