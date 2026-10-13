using UnityEngine;

namespace Wildfolk
{
    public enum EnemyState { Idle, Chase, Windup, Recovery }

    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyController : MonoBehaviour
    {
        public GameplaySettings Settings;
        public GameplaySession Session;
        public Health OwnHealth;
        public Health PlayerHealth;
        public Transform AttackPose;
        public EnemyState State { get; private set; }
        private Rigidbody2D body;
        private Vector3 restPosition;
        private Vector3 restScale;
        private float stateStarted;
        private Vector2 strikeDirection;
        private readonly RaycastHit2D[] obstacles = new RaycastHit2D[8];

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            restPosition = AttackPose.localPosition;
            restScale = AttackPose.localScale;
        }
        private void OnEnable() => OwnHealth.Died += Die;
        private void OnDisable() => OwnHealth.Died -= Die;
        private void Die() => Destroy(gameObject);

        private void Update()
        {
            if (!Session.IsRunning || !OwnHealth.IsAlive || !PlayerHealth.IsAlive) return;
            var delta = (Vector2)PlayerHealth.transform.position - body.position;
            var distance = delta.magnitude;
            switch (State)
            {
                case EnemyState.Idle:
                    if (distance <= Settings.EnemyDetectionRange) SetState(EnemyState.Chase);
                    break;
                case EnemyState.Chase:
                    if (distance > Settings.EnemyLoseRange) SetState(EnemyState.Idle);
                    else if (distance <= Settings.EnemyStopDistance)
                    {
                        strikeDirection = delta.sqrMagnitude > .001f ? delta.normalized : Vector2.left;
                        SetState(EnemyState.Windup);
                    }
                    break;
                case EnemyState.Windup:
                    var progress = Mathf.Clamp01((Time.time-stateStarted)/Settings.EnemyWindup);
                    AttackPose.localScale = new Vector3(restScale.x*(1+progress*.07f),restScale.y*(1-progress*.07f),restScale.z);
                    if (progress < 1) break;
                    if (distance <= Settings.EnemyAttackReach && Vector2.Dot(strikeDirection, delta.normalized) > .4f)
                        PlayerHealth.ApplyDamage(Settings.EnemyDamage);
                    SetState(EnemyState.Recovery);
                    break;
                case EnemyState.Recovery:
                    var recover = Mathf.Clamp01((Time.time-stateStarted)/.18f);
                    AttackPose.localPosition = restPosition + (Vector3)strikeDirection * ((1-recover)*.16f);
                    AttackPose.localScale = restScale;
                    if (Time.time-stateStarted >= Settings.EnemyCooldown) SetState(EnemyState.Chase);
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (!Session.IsRunning || !OwnHealth.IsAlive || !PlayerHealth.IsAlive) return;
            var delta = body.position - (Vector2)PlayerHealth.transform.position;
            // Move the enemy away from overlap. Its trigger collider cannot push the player.
            if (delta.magnitude < Settings.EnemySeparation)
            {
                var away = delta.sqrMagnitude > .001f ? delta.normalized : Vector2.left;
                body.MovePosition((Vector2)PlayerHealth.transform.position + away * Settings.EnemySeparation);
                return;
            }
            if (State != EnemyState.Chase) return;
            var direction = -delta.normalized;
            var step = Mathf.Min(Settings.EnemySpeed * Time.fixedDeltaTime, Mathf.Max(0,delta.magnitude-Settings.EnemyStopDistance));
            if (step <= 0) return;
            if (!Clear(direction,step))
            {
                var left = (Vector2)(Quaternion.Euler(0,0,55) * direction);
                var right = (Vector2)(Quaternion.Euler(0,0,-55) * direction);
                if (Clear(left,step)) direction=left;
                else if (Clear(right,step)) direction=right;
                else return;
            }
            body.MovePosition(body.position + direction * step);
        }

        private bool Clear(Vector2 direction,float step)
        {
            var filter = new ContactFilter2D { useTriggers=false };
            return Physics2D.CircleCast(body.position,.4f,direction,filter,obstacles,step+.12f)==0;
        }

        private void SetState(EnemyState state)
        {
            State=state;
            stateStarted=Time.time;
            AttackPose.localPosition=restPosition;
            AttackPose.localScale=restScale;
        }
    }
}
