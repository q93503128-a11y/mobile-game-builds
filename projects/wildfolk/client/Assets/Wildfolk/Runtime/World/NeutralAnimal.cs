using UnityEngine;
namespace Wildfolk
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class NeutralAnimal : MonoBehaviour
    {
        public AnimalMovementDefinition Definition;
        public GameplaySession Session;
        public Transform Player,Visual;
        public WorldSpawnRegion Region;
        public Health Health;
        public bool IsFleeing => Time.time<fleeUntil;
        private Rigidbody2D body;
        private Vector2 home,direction;
        private float fleeUntil,nextWander;
        private readonly RaycastHit2D[] obstacles=new RaycastHit2D[8];
        private void Awake() => body=GetComponent<Rigidbody2D>();
        private void Start() {home=body.position;ChooseDirection();}
        private void OnEnable() => Health.Damaged+=Flee;
        private void OnDisable() => Health.Damaged-=Flee;
        private void Flee()
        {
            var away=body.position-(Vector2)Player.position;direction=away.sqrMagnitude>.01f ? away.normalized : Vector2.right;
            fleeUntil=Time.time+Definition.FleeSeconds;
        }
        private void ChooseDirection()
        {
            direction=Random.insideUnitCircle.normalized;
            if(Vector2.Distance(body.position,home)>Definition.HomeRadius)direction=(home-body.position).normalized;
            nextWander=Time.time+Definition.WanderInterval;
        }
        private void FixedUpdate()
        {
            if(!Session.WorldIsRunning || !Health.IsAlive)return;
            if(Vector2.Distance(body.position,Player.position)<Definition.DangerRange)Flee();
            else if(!IsFleeing && Time.time>=nextWander)ChooseDirection();
            var step=(IsFleeing ? Definition.FleeSpeed : Definition.WanderSpeed)*Time.fixedDeltaTime;
            if(Physics2D.CircleCast(body.position,.4f,direction,new ContactFilter2D{useTriggers=false},obstacles,step+.12f)>0)
            {direction=(Vector2)(Quaternion.Euler(0,0,90)*direction);return;}
            var position=body.position+direction*step;var bounds=Region.Bounds;
            if(!bounds.Contains(position)){direction=-direction;return;}
            body.MovePosition(position);
            var angle=Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg+90;
            Visual.rotation=Quaternion.RotateTowards(Visual.rotation,Quaternion.Euler(0,0,angle),360*Time.fixedDeltaTime);
        }
    }
}
