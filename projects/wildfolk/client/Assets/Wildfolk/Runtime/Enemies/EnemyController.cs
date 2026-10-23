using UnityEngine;
namespace Wildfolk
{
    public enum EnemyState { Idle, Chase, Windup, Swing, Recovery }
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyController : MonoBehaviour,IPoolReset
    {
        public GameplaySettings Settings;
        public GameplaySession Session;
        public WorldClock Clock;
        public Health OwnHealth,PlayerHealth;
        public Transform AttackPose;
        public CharacterFacing Facing;
        public ZombieLifecycle Lifecycle;
        public EnemySteering Steering;
        public CreatureMember Member;
        public CreatureCombatDefinition Combat;
        public CreatureVariant Variant;
        public Health PreferredTarget;public float MovementMultiplier=1;
        private Health SelectTarget()=>PreferredTarget!=null && PreferredTarget.IsAlive?PreferredTarget:SimulationAuthority.Target(this,PlayerHealth);
        public bool PersistentGuard {get;set;}
        public Vector2 GuardHome {get;set;}public float GuardLeash {get;set;}=12;
        private bool ReturningGuard=>PersistentGuard && (Vector2.Distance(body.position,GuardHome)>GuardLeash || PlayerHealth!=null && Vector2.Distance(PlayerHealth.transform.position,GuardHome)>GuardLeash+4);
        private bool Sensitive=>Member!=null && Member.Definition.Has(CreatureTrait.LightSensitive);
        private float Reach=>Combat==null ? Settings.EnemyAttackReach : Combat.Reach+(Variant==null ? 0 : Variant.ReachBonus);
        private float Speed=>(Combat==null ? Settings.EnemySpeed : Combat.Speed)*MovementMultiplier;
        private float Windup=>Combat==null ? Settings.EnemyWindup : Combat.Windup;
        private float Recovery=>Combat==null ? Settings.EnemyCooldown : Combat.Recovery;
        private int Damage=>Combat==null ? Settings.EnemyDamage : Combat.Damage+(Variant==null ? 0 : Variant.DamageBonus);
        public EnemyState State { get; private set; }
        private Rigidbody2D body;
        private Vector3 restPosition,restScale;
        private float stateStarted;
        private float safeSince=-1;
        private float lostSince=-1;
        private Vector2 strikeDirection;
        private Health strikeTarget;
        private Collider2D strikeCollider;
        private readonly RaycastHit2D[] obstacles=new RaycastHit2D[8];
        private void Awake() {body=GetComponent<Rigidbody2D>();restPosition=AttackPose.localPosition;restScale=AttackPose.localScale;}
        public void ResetForSpawn(){PersistentGuard=false;PreferredTarget=null;MovementMultiplier=1;State=EnemyState.Idle;stateStarted=Time.time;safeSince=lostSince=-1;strikeTarget=null;strikeCollider=null;AttackPose.localPosition=restPosition;AttackPose.localScale=restScale;}
        private bool NightHunt => PersistentGuard || Clock!=null && Clock.Model.Phase==DayPhase.Night;
        private void Update()
        {
            if(!SimulationAuthority.Allows(this))return;
            if(PlayerHealth==null || !PlayerHealth.IsAlive)CancelPursuit();
            if(State==EnemyState.Idle || State==EnemyState.Chase)PlayerHealth=SelectTarget();
            if(!CanAct)return;
            if(ReturningGuard){if(State!=EnemyState.Idle)CancelPursuit();return;}
            if(Sensitive && Member.Definition.Light.SuppressSafeAggro && GameplayLight.IsSafe(PlayerHealth.transform.position))
            {
                if(State==EnemyState.Idle)return;
                if(safeSince<0)safeSince=Time.time;
                if(Time.time-safeSince>=GameplayLight.Grace(PlayerHealth.transform.position)*Member.Definition.Light.ChaseGraceMultiplier){CancelPursuit();return;}
            }
            else safeSince=-1;
            var delta=(Vector2)PlayerHealth.transform.position-body.position;var distance=delta.magnitude;
            if(State==EnemyState.Chase && Lifecycle!=null)
            {
                if(distance>(Combat==null ? Lifecycle.Definition.LoseRange : Combat.LoseRange)){if(lostSince<0)lostSince=Time.time;if(Time.time-lostSince>=Lifecycle.Definition.LostTargetSeconds){CancelPursuit();return;}}
                else lostSince=-1;
            }
            if(State==EnemyState.Windup || State==EnemyState.Swing || State==EnemyState.Recovery)Facing.Face(strikeDirection);
            else if(State==EnemyState.Chase)Facing.Face(delta);
            switch(State)
            {
                case EnemyState.Idle:
                    var detection=Combat!=null ? Combat.Detection : Lifecycle==null ? Settings.EnemyDetectionRange : Lifecycle.Definition.DetectionRadius;
                    if(Sensitive && GameplayLight.IsDim(PlayerHealth.transform.position))detection*=Member.Definition.Light.DimDetection;
                    if(!(Sensitive && Member.Definition.Light.SuppressSafeAggro && GameplayLight.IsSafe(PlayerHealth.transform.position)) && distance<=detection)SetState(EnemyState.Chase);
                    break;
                case EnemyState.Chase:
                    if(!NightHunt && distance>Settings.EnemyLoseRange)SetState(EnemyState.Idle);
                    else if(distance<=(Combat==null ? Settings.EnemyStopDistance : Combat.StopDistance+(Variant==null ? 0 : Variant.ReachBonus)))
                    {
                        var barrier=BarrierAhead(delta.normalized);
                        if(barrier!=null)BeginStrike(barrier.GetComponentInParent<Health>(),barrier);
                        else BeginStrike(PlayerHealth,PlayerHealth.GetComponent<Collider2D>());
                    }
                    break;
                case EnemyState.Windup:
                    var progress=Mathf.Clamp01((Time.time-stateStarted)/Windup);
                    AttackPose.localScale=new Vector3(restScale.x*(1+progress*.07f),restScale.y*(1-progress*.07f),restScale.z);
                    Facing.SetHandPose(1,new Vector2(0,-.16f*progress),-45*progress);
                    if(progress<1)break;
                    SetState(EnemyState.Swing);break;
                case EnemyState.Swing:
                    var swing=Mathf.Clamp01((Time.time-stateStarted)/(Combat==null ? .12f : Combat.Swing));
                    Facing.SetHandPose(1,new Vector2(-.1f,.25f*swing),Mathf.Lerp(-45,45,swing));
                    if(swing<.65f)break;
                    if(strikeTarget!=null && strikeTarget.IsAlive && strikeCollider!=null && strikeCollider.enabled)
                    {
                        var toward=(strikeTarget==PlayerHealth ? (Vector2)PlayerHealth.transform.position : strikeCollider.ClosestPoint(body.position))-body.position;
                        if(toward.magnitude<=Reach && (toward.sqrMagnitude<.001f || Vector2.Dot(strikeDirection,toward.normalized)>.4f))strikeTarget.ApplyDamage(Damage,false,null,OwnHealth);
                    }
                    SetState(EnemyState.Recovery);break;
                case EnemyState.Recovery:
                    var recover=Mathf.Clamp01((Time.time-stateStarted)/.18f);
                    AttackPose.localPosition=restPosition+(Vector3)strikeDirection*((1-recover)*.16f);AttackPose.localScale=restScale;
                    if(recover>=1)Facing.ResetPose();
                    if(Time.time-stateStarted>=Recovery)SetState(EnemyState.Chase);
                    break;
            }
        }
        private void FixedUpdate()
        {
            if(!SimulationAuthority.Allows(this))return;
            if(PlayerHealth==null || !PlayerHealth.IsAlive)CancelPursuit();
            if(State==EnemyState.Idle || State==EnemyState.Chase)PlayerHealth=SelectTarget();
            if(!CanAct)return;
            if(ReturningGuard){var home=GuardHome-body.position;if(home.magnitude>1 && Steering!=null)Steering.Move(home.normalized,Speed);return;}
            var delta=body.position-(Vector2)PlayerHealth.transform.position;
            if(delta.magnitude<Settings.EnemySeparation)
            {
                var away=delta.sqrMagnitude>.001f ? delta.normalized : Vector2.left;
                var movement=Settings.EnemySeparation-delta.magnitude;
                if(Clear(away,movement))body.MovePosition((Vector2)PlayerHealth.transform.position+away*Settings.EnemySeparation);
                return;
            }
            if(State!=EnemyState.Chase){if(Steering!=null)Steering.Move(Vector2.zero,Settings.EnemySpeed*.45f);return;}
            var direction=-delta.normalized;
            var stop=Combat==null ? Settings.EnemyStopDistance : Combat.StopDistance;
            var step=Mathf.Min(Speed*Time.fixedDeltaTime,Mathf.Max(0,delta.magnitude-stop));
            if(step<=0)return;
            if(!Clear(direction,step))
            {
                var barrier=BarrierAhead(direction);
                if(barrier!=null){BeginStrike(barrier.GetComponentInParent<Health>(),barrier);return;}
                var left=(Vector2)(Quaternion.Euler(0,0,55)*direction);var right=(Vector2)(Quaternion.Euler(0,0,-55)*direction);
                if(Clear(left,step))direction=left;else if(Clear(right,step))direction=right;else return;
            }
            var multiplier=Sensitive ? GameplayLight.IsSafe(body.position) ? Member.Definition.Light.SafeMovement : GameplayLight.IsDim(body.position) ? Member.Definition.Light.DimMovement : 1 : 1;
            var speed=step/Time.fixedDeltaTime*multiplier;
            if(Steering!=null)Steering.Move(direction,speed);else body.MovePosition(body.position+direction*step);
        }
        private Collider2D BarrierAhead(Vector2 direction)
        {
            var count=Physics2D.CircleCast(body.position,.4f,direction,new ContactFilter2D{useTriggers=false},obstacles,Reach);
            var closest=float.MaxValue;Collider2D barrier=null;
            for(var i=0;i<count;i++)
            {
                var placeable=obstacles[i].collider.GetComponentInParent<WorldPlaceable>();
                if(placeable==null || !placeable.Definition.IsBarrier || !placeable.Health.IsAlive)continue;
                var distance=Vector2.Distance(body.position,obstacles[i].collider.ClosestPoint(body.position));
                if(distance<=Reach && distance<closest){closest=distance;barrier=obstacles[i].collider;}
            }
            return barrier;
        }
        private bool Clear(Vector2 direction,float step) => Steering!=null ? Steering.Clear(direction,step) : Physics2D.CircleCast(body.position,.4f,direction,new ContactFilter2D{useTriggers=false},obstacles,step+.12f)==0;
        private bool CanAct=>SimulationAuthority.Allows(this) && Session.WorldIsRunning && OwnHealth.IsAlive && PlayerHealth!=null && PlayerHealth.IsAlive && NightHunt && (Lifecycle==null || Lifecycle.Phase==ZombiePhase.Active);
        public void CancelPursuit(){safeSince=-1;SetState(EnemyState.Idle);strikeTarget=null;strikeCollider=null;Facing.ResetPose();}
        private void BeginStrike(Health target,Collider2D collider)
        {
            strikeTarget=target;strikeCollider=collider;
            var point=target==PlayerHealth ? (Vector2)PlayerHealth.transform.position : collider.ClosestPoint(body.position);
            var delta=point-body.position;strikeDirection=delta.sqrMagnitude>.001f ? delta.normalized : Vector2.left;
            SetState(EnemyState.Windup);
        }
        private void SetState(EnemyState state) {State=state;stateStarted=Time.time;AttackPose.localPosition=restPosition;AttackPose.localScale=restScale;}
    }
}
