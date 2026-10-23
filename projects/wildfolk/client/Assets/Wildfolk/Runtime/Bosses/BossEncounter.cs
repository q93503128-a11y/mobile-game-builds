using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    // Shared encounter simulation. Nothing in this scheduler depends on a boss name.
    public sealed class BossEncounter:MonoBehaviour
    {
        public BossDefinition Definition;
        public Health Health;
        public BossWorld World {get;private set;}
        public BossSnapshot Record {get;private set;}
        public BossState State {get;private set;}
        public int Phase {get;private set;}=1;
        public int AttackIndex {get;private set;}=-1;
        public Vector2 Direction {get;private set;}=Vector2.down;
        public double Started {get;private set;}
        public Health Target {get;private set;}
        public Health Marked {get;private set;}
        public Vector2 Home=>Record==null?(Vector2)transform.position:Record.Home;
        public bool BarVisible=>World!=null && State!=BossState.Awakening && State!=BossState.Ended && State!=BossState.Dying;
        public BossAttack Attack=>AttackIndex>=0 && AttackIndex<Definition.Attacks.Length?Definition.Attacks[AttackIndex]:null;
        public double Now=>GetComponent<NetworkBoss>()?.Clock??Time.timeAsDouble;
        public float Age=>(float)Math.Max(0,Now-Started);
        public bool Authority {get{var n=GetComponent<NetworkBoss>();return n==null || n.IsSpawned && n.IsServer;}}
        public int LivingAdds {get{var count=0;foreach(var m in minions)if(m!=null && m.Health.IsAlive)count++;return count;}}
        public event Action Changed;
        private readonly List<BossMinion> minions=new List<BossMinion>(5);
        private readonly HashSet<PlayerInventory> participants=new HashSet<PlayerInventory>();
        public IEnumerable<PlayerInventory> Participants=>participants;
        private readonly Collider2D[] contacts=new Collider2D[64];
        private readonly HashSet<Health> hit=new HashSet<Health>();
        private readonly RaycastHit2D[] walls=new RaycastHit2D[32];
        private static readonly float[] ReturnAngles={55,-55,100,-100};
        private Rigidbody2D body;private Collider2D footprint;private DamageProtection protection;
        private float tick,missing,regen,decision,retarget,summonAt;private int cursor,repeat,chain;
        private void Awake(){body=GetComponent<Rigidbody2D>();footprint=GetComponent<Collider2D>();protection=GetComponent<DamageProtection>();protection.Blocked=true;}
        private void OnEnable(){Health.Died+=Died;}
        private void OnDisable(){Health.Died-=Died;ClearAdds();}
        public void Initialize(BossWorld world,BossSnapshot record,bool restored)
        {
            World=world;Record=record;transform.position=record.Position;body.position=record.Position;
            Health.RestoreHealth(record.HP);Phase=Definition.Phase(Health.Ratio);summonAt=(float)Now+Definition.SummonInterval;
            SetState(record.PendingReward?BossState.Dying:restored?BossState.Returning:BossState.Awakening);
        }
        public void ApplyReplica(BossState state,int phase,int attack,Vector2 direction,double started,Health marked)
        {State=state;Phase=phase;AttackIndex=attack;Direction=direction;Started=started;Marked=marked;Changed?.Invoke();GetComponent<BossIdentity>().NotifyState();}
        private void SetState(BossState state)
        {
            State=state;Started=Now;protection.Blocked=state==BossState.Awakening || state==BossState.Returning || state==BossState.Dying || state==BossState.Ended;
            body.linearVelocity=Vector2.zero;Changed?.Invoke();GetComponent<BossIdentity>().NotifyState();GetComponent<NetworkBoss>()?.Publish();
        }
        private void Update()
        {
            if(!Authority || World==null || !World.World.Session.WorldIsRunning || State==BossState.Ended)return;
            tick+=Time.deltaTime;if(tick<.05f)return;var dt=tick;tick=0;Advance(dt);
        }
        public void Advance(float dt)
        {
            if(!Authority || World==null)return;
            if(State==BossState.Dying){if(Age>=Definition.DeathSeconds+Definition.LootPause && World.ResolveReward(this)){SetState(BossState.Ended);World.Release(this);}return;}
            if(State==BossState.Awakening){if(Age>=Definition.Awakening)SetState(BossState.Chase);return;}
            var living=0;foreach(var p in World.Players)if(Valid(p.GetComponent<Health>())){living++;participants.Add(p);}
            missing=living==0?missing+dt:0;
            if(State==BossState.Returning)
            {
                if(living>0){Phase=Definition.Phase(Health.Ratio);missing=regen=0;decision=Definition.DecisionDelay;SetState(BossState.Chase);return;}
                if(Age>=Definition.RegenDelay){regen+=Health.Maximum*Definition.RegenFraction*dt;var n=(int)regen;if(n>0){Health.Heal(n);regen-=n;}}
                if(Vector2.Distance(transform.position,Home)<1 && Health.Current==Health.Maximum && World.Clock.Model.Phase==DayPhase.Day){Record.Active=false;SetState(BossState.Ended);World.Checkpoint();World.Release(this);}return;
            }
            if(missing>=Definition.Grace){BeginReturn();return;}
            UpdateMark();
            var phase=Definition.Phase(Health.Ratio);
            if(phase>Phase){Phase=phase;chain=repeat=0;AttackIndex=-1;SetState(BossState.Transition);return;}
            if(State==BossState.Transition)
            {if(Age>=Definition.Ritual){if(Phase==2)Summon();SetState(BossState.Chase);}return;}
            if(State==BossState.Ritual){if(Age>=Definition.Ritual){Summon();SetState(BossState.Recovery);}return;}
            if(State==BossState.Windup){if(Age>=Attack.Windup){hit.Clear();SetState(BossState.Strike);}return;}
            if(State==BossState.Strike)
            {if(Age>=.14f){Damage();if(++repeat<Attack.Repeats)SetState(BossState.Windup);else SetState(BossState.Recovery);}return;}
            if(State==BossState.Recovery)
            {
                var recovery=Attack==null?1:Attack.Recovery;if(Phase==3)recovery*=Definition.LastPhaseRecovery;
                if(Age<recovery)return;
                if(Phase==3 && chain==0 && Attack!=null && Attack.FollowUp>=0){var next=Attack.FollowUp;chain=1;BeginAttack(next);return;}
                chain=0;decision=Definition.DecisionDelay;SetState(BossState.Chase);return;
            }
            retarget-=dt;if(retarget<=0 || !Valid(Target)){Target=ChooseTarget();retarget=2.5f;}
            if(Target==null)return;
            var delta=(Vector2)Target.transform.position-(Vector2)transform.position;Direction=delta.sqrMagnitude>.01f?delta.normalized:Direction;
            decision-=dt;if(decision>0)return;
            if(Phase==2 && Now>=summonAt && LivingAdds==0){AttackIndex=-1;SetState(BossState.Ritual);return;}
            // Structures actually shooting at the king can become secondary targets.
            foreach(var placed in WorldPlaceable.Active)if(placed.Health.IsAlive && (placed.transform.position-transform.position).sqrMagnitude<144 && placed.GetComponent<WorldTurret>() is WorldTurret turret && turret.Target==Health)
            {Target=placed.Health;delta=(Vector2)Target.transform.position-(Vector2)transform.position;Direction=delta.normalized;break;}
            if(delta.magnitude>2.5f)
            {
                var step=Definition.Speed*.06f;
                if(!World.Navigation.CanMove(transform.position,Direction,step,.65f,footprint))
                {var wall=BlockingStructure(Direction);if(wall!=null){Target=wall;BeginAttack(0);}}return;
            }
            for(var i=0;i<Definition.Schedule.Length;i++)
            {var index=Definition.Schedule[cursor++%Definition.Schedule.Length];var a=Definition.Attacks[index];if(Phase<a.FirstPhase || Phase>a.LastPhase)continue;BeginAttack(index);break;}
        }
        private void BeginAttack(int index){AttackIndex=index;repeat=0;var d=Target==null?Direction:(Vector2)(Target.transform.position-transform.position);if(d.sqrMagnitude>.01f)Direction=d.normalized;SetState(BossState.Windup);}
        private bool Valid(Health p)=>p!=null && p.IsAlive && p.GetComponent<PlayerInventory>()!=null && Vector2.Distance(p.transform.position,Home)<=Definition.Territory;
        private Health ChooseTarget()
        {Health chosen=null;var score=float.MaxValue;foreach(var p in World.Players){var h=p.GetComponent<Health>();if(!Valid(h))continue;var d=(p.transform.position-transform.position).sqrMagnitude;if(h==Target)d*=1.2f;if(p==Health.LastAttacker)d*=.85f;if(d<score){score=d;chosen=h;}}return chosen;}
        private void FixedUpdate()
        {
            if(!Authority || World==null || !World.World.Session.WorldIsRunning)return;
            if(State==BossState.Returning){Move(Home-(Vector2)transform.position,Definition.Speed*Definition.ReturnSpeed);return;}
            if(State==BossState.Chase && Target!=null){var d=(Vector2)(Target.transform.position-transform.position);if(d.magnitude>2.3f)Move(d,Definition.Speed*(Phase==3?Definition.LastPhaseSpeed:1));}
            else if(State==BossState.Strike && Attack!=null && Attack.Advance>0)Move(Direction,Attack.Advance/.3f);
        }
        private void Move(Vector2 d,float speed)
        {
            if(d.magnitude<.08f)return;var step=Mathf.Min(d.magnitude,speed*Time.fixedDeltaTime);var dir=d.normalized;
            if(World.Navigation.CanMove(body.position,dir,step,.65f,footprint)){body.MovePosition(body.position+dir*step);return;}
            if(State==BossState.Returning || State==BossState.Chase){foreach(var angle in ReturnAngles){var side=(Vector2)(Quaternion.Euler(0,0,angle)*dir);if(World.Navigation.CanMove(body.position,side,step,.65f,footprint)){body.MovePosition(body.position+side*step);break;}}}
        }
        private Health BlockingStructure(Vector2 d)
        {var count=Physics2D.CircleCast(transform.position,.65f,d,new ContactFilter2D{useTriggers=false},walls,1.3f);for(var i=0;i<count;i++){var p=walls[i].collider.GetComponentInParent<WorldPlaceable>();if(p!=null && p.Health.IsAlive)return p.Health;}return null;}
        private bool ClearLine(Collider2D target)
        {
            var d=target.ClosestPoint(transform.position)-(Vector2)transform.position;if(d.sqrMagnitude<.01f)return true;
            var n=Physics2D.Raycast(transform.position,d.normalized,new ContactFilter2D{useTriggers=false},walls,d.magnitude);
            for(var i=0;i<n;i++)if(walls[i].collider!=footprint && walls[i].collider.GetComponentInParent<Health>()!=target.GetComponentInParent<Health>() && !walls[i].collider.isTrigger)return false;return true;
        }
        private void Damage()
        {
            var a=Attack;var count=Physics2D.OverlapCircle(transform.position,a.Range+.6f,new ContactFilter2D{useTriggers=true},contacts);hit.Clear();
            for(var i=0;i<count;i++)
            {
                var c=contacts[i];var h=c.GetComponentInParent<Health>();if(h==null || h==Health || !h.IsAlive || !hit.Add(h))continue;
                if(h.GetComponent<PlayerInventory>()==null && h.GetComponent<WorldPlaceable>()==null)continue;
                var delta=c.ClosestPoint(transform.position)-(Vector2)transform.position;var along=Vector2.Dot(delta,Direction);var across=Mathf.Abs(delta.x*Direction.y-delta.y*Direction.x);
                var inside=a.Shape==BossAttackShape.Circle?delta.magnitude<=a.Range:a.Shape==BossAttackShape.Line?along>=0 && along<=a.Range && across<=a.Width*.5f:delta.magnitude<=a.Range && (delta.sqrMagnitude<.01f || Vector2.Angle(Direction,delta)<=a.Angle*.5f);
                if(inside && ClearLine(c))h.ApplyDamage(Mathf.RoundToInt(a.Damage*(h.GetComponent<WorldPlaceable>()!=null?Definition.StructureDamage:1)),false,null,Health);
            }
        }
        private void Summon()
        {
            if(Phase!=2 || LivingAdds!=0)return;minions.Clear();Marked=ChooseTarget();if(Marked==null)return;
            var count=0;foreach(var p in World.Players)if(Valid(p.GetComponent<Health>()))count++;
            for(var i=0;i<Definition.AddCount(count);i++)
            {
                var angle=i*2.399963f;var position=Home+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*5;
                if(!World.Navigation.CanSpawn(position,.35f,0,false))position=(Vector2)transform.position+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*2.5f;
                var e=PrefabPool.Spawn(Definition.Minion,position,Quaternion.identity);e.Member.GroupId="encounter:"+Definition.Id+":"+Record.LastNight;e.Session=World.World.Session;e.Clock=World.Clock;e.PlayerHealth=Marked;e.PersistentGuard=true;e.GuardHome=Home;e.GuardLeash=Definition.Territory;
                e.Lifecycle.Clock=World.Clock;e.Lifecycle.RestoreActive();e.Lifecycle.enabled=false;e.Steering.Navigation=World.Navigation;
                e.GetComponent<LootOnDeath>().enabled=false;var m=e.GetComponent<BossMinion>();if(m==null)m=e.gameObject.AddComponent<BossMinion>();m.Initialize(this,e);minions.Add(m);
            }
            summonAt=(float)Now+Definition.SummonInterval;UpdateMark();Changed?.Invoke();GetComponent<NetworkBoss>()?.Publish();
        }
        private void UpdateMark()
        {
            if(LivingAdds==0){if(Marked!=null){Marked=null;Changed?.Invoke();GetComponent<NetworkBoss>()?.Publish();}return;}
            if(!Valid(Marked)){Marked=ChooseTarget();Changed?.Invoke();GetComponent<NetworkBoss>()?.Publish();}
            foreach(var m in minions)if(m!=null)m.Target(Marked);
        }
        public void BeginReturn(){if(!Authority)return;ClearAdds();Marked=null;AttackIndex=-1;Target=null;regen=0;SetState(BossState.Returning);}
        private void ClearAdds(){foreach(var m in minions)if(m!=null)m.Retire();minions.Clear();Marked=null;}
        private void Died(){if(!Authority || World==null || Record.PendingReward)return;ClearAdds();World.RollReward(this);SetState(BossState.Dying);World.Checkpoint();}
        public void Capture(){if(Record==null)return;Record.Position=transform.position;Record.HP=Health.Current;}
    }
}
