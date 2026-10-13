using UnityEngine;
namespace Wildfolk
{
    public enum EnemyState { Idle, Chase, Windup, Recovery }
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyController : MonoBehaviour
    {
        public GameplaySettings Settings;
        public GameplaySession Session;
        public WorldClock Clock;
        public Health OwnHealth,PlayerHealth;
        public Transform AttackPose;
        public CharacterFacing Facing;
        public EnemyState State { get; private set; }
        private Rigidbody2D body;
        private Vector3 restPosition,restScale;
        private float stateStarted;
        private Vector2 strikeDirection;
        private Health strikeTarget;
        private Collider2D strikeCollider;
        private readonly RaycastHit2D[] obstacles=new RaycastHit2D[8];
        private void Awake() {body=GetComponent<Rigidbody2D>();restPosition=AttackPose.localPosition;restScale=AttackPose.localScale;}
        private void OnEnable() => OwnHealth.Died+=Die;
        private void OnDisable() => OwnHealth.Died-=Die;
        private void Die() => Destroy(gameObject);
        private bool NightHunt => Clock!=null && Clock.Model.Phase==DayPhase.Night;
        private void Update()
        {
            if(!Session.WorldIsRunning || !OwnHealth.IsAlive || !PlayerHealth.IsAlive)return;
            var delta=(Vector2)PlayerHealth.transform.position-body.position;var distance=delta.magnitude;
            if(State==EnemyState.Windup || State==EnemyState.Recovery)Facing.Face(strikeDirection);
            else if(State==EnemyState.Chase)Facing.Face(delta);
            switch(State)
            {
                case EnemyState.Idle:
                    if(NightHunt || distance<=Settings.EnemyDetectionRange)SetState(EnemyState.Chase);
                    break;
                case EnemyState.Chase:
                    if(!NightHunt && distance>Settings.EnemyLoseRange)SetState(EnemyState.Idle);
                    else if(distance<=Settings.EnemyStopDistance)
                    {
                        var barrier=BarrierAhead(delta.normalized);
                        if(barrier!=null)BeginStrike(barrier.GetComponentInParent<Health>(),barrier);
                        else BeginStrike(PlayerHealth,PlayerHealth.GetComponent<Collider2D>());
                    }
                    break;
                case EnemyState.Windup:
                    var progress=Mathf.Clamp01((Time.time-stateStarted)/Settings.EnemyWindup);
                    AttackPose.localScale=new Vector3(restScale.x*(1+progress*.07f),restScale.y*(1-progress*.07f),restScale.z);
                    if(progress<1)break;
                    if(strikeTarget!=null && strikeTarget.IsAlive && strikeCollider!=null && strikeCollider.enabled)
                    {
                        var toward=(strikeTarget==PlayerHealth ? (Vector2)PlayerHealth.transform.position : strikeCollider.ClosestPoint(body.position))-body.position;
                        if(toward.magnitude<=Settings.EnemyAttackReach && (toward.sqrMagnitude<.001f || Vector2.Dot(strikeDirection,toward.normalized)>.4f))strikeTarget.ApplyDamage(Settings.EnemyDamage);
                    }
                    SetState(EnemyState.Recovery);break;
                case EnemyState.Recovery:
                    var recover=Mathf.Clamp01((Time.time-stateStarted)/.18f);
                    AttackPose.localPosition=restPosition+(Vector3)strikeDirection*((1-recover)*.16f);AttackPose.localScale=restScale;
                    if(Time.time-stateStarted>=Settings.EnemyCooldown)SetState(EnemyState.Chase);
                    break;
            }
        }
        private void FixedUpdate()
        {
            if(!Session.WorldIsRunning || !OwnHealth.IsAlive || !PlayerHealth.IsAlive)return;
            var delta=body.position-(Vector2)PlayerHealth.transform.position;
            if(delta.magnitude<Settings.EnemySeparation)
            {
                var away=delta.sqrMagnitude>.001f ? delta.normalized : Vector2.left;
                var movement=Settings.EnemySeparation-delta.magnitude;
                if(Clear(away,movement))body.MovePosition((Vector2)PlayerHealth.transform.position+away*Settings.EnemySeparation);
                return;
            }
            if(State!=EnemyState.Chase)return;
            var direction=-delta.normalized;
            var step=Mathf.Min(Settings.EnemySpeed*Time.fixedDeltaTime,Mathf.Max(0,delta.magnitude-Settings.EnemyStopDistance));
            if(step<=0)return;
            if(!Clear(direction,step))
            {
                var barrier=BarrierAhead(direction);
                if(barrier!=null){BeginStrike(barrier.GetComponentInParent<Health>(),barrier);return;}
                var left=(Vector2)(Quaternion.Euler(0,0,55)*direction);var right=(Vector2)(Quaternion.Euler(0,0,-55)*direction);
                if(Clear(left,step))direction=left;else if(Clear(right,step))direction=right;else return;
            }
            body.MovePosition(body.position+direction*step);
        }
        private Collider2D BarrierAhead(Vector2 direction)
        {
            var count=Physics2D.CircleCast(body.position,.4f,direction,new ContactFilter2D{useTriggers=false},obstacles,Settings.EnemyAttackReach);
            var closest=float.MaxValue;Collider2D barrier=null;
            for(var i=0;i<count;i++)
            {
                var placeable=obstacles[i].collider.GetComponentInParent<WorldPlaceable>();
                if(placeable==null || !placeable.Definition.IsBarrier || !placeable.Health.IsAlive)continue;
                var distance=Vector2.Distance(body.position,obstacles[i].collider.ClosestPoint(body.position));
                if(distance<=Settings.EnemyAttackReach && distance<closest){closest=distance;barrier=obstacles[i].collider;}
            }
            return barrier;
        }
        private bool Clear(Vector2 direction,float step) => Physics2D.CircleCast(body.position,.4f,direction,new ContactFilter2D{useTriggers=false},obstacles,step+.12f)==0;
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
