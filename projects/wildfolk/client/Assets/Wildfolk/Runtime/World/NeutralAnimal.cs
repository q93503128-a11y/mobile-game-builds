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
        public CreatureMember Member;
        public SoftSeparation Separation;
        public bool IsFleeing => Time.time<fleeUntil;
        private Rigidbody2D body;
        private Vector2 home,direction;
        private float fleeUntil,nextWander;
        private readonly RaycastHit2D[] obstacles=new RaycastHit2D[8];
        private void Awake() => body=GetComponent<Rigidbody2D>();
        private void Start() {home=string.IsNullOrEmpty(Member.GroupId) ? body.position : Member.Anchor;ChooseDirection();}
        private void OnEnable(){Health.Damaged+=Flee;Member.Alerted+=ReceiveAlert;}
        private void OnDisable(){Health.Damaged-=Flee;Member.Alerted-=ReceiveAlert;}
        private void ReceiveAlert(Vector2 origin,CreatureAlert alert){if(alert==CreatureAlert.Flee)FleeFrom(origin,false);}
        private void Flee()
        {var threat=Health.LastAttacker==null ? Player : Health.LastAttacker.transform;if(threat!=null)FleeFrom(threat.position,true);}
        public void FleeFrom(Vector2 origin,bool broadcast=true)
        {
            if(!SimulationAuthority.Allows(this))return;
            var wasFleeing=IsFleeing;var away=body.position-origin;direction=away.sqrMagnitude>.01f ? away.normalized : Vector2.right;
            fleeUntil=Time.time+Definition.FleeSeconds;
            if(!wasFleeing && broadcast && Member.Definition.Has(CreatureTrait.FleeTogether))Member.Broadcast(origin,CreatureAlert.Flee);
        }
        private void ChooseDirection()
        {
            direction=Random.insideUnitCircle.normalized;
            if(Vector2.Distance(body.position,home)>Definition.HomeRadius)direction=(home-body.position).normalized;
            nextWander=Time.time+Definition.WanderInterval;
        }
        private void FixedUpdate()
        {
            if(!SimulationAuthority.Allows(this) || !Session.WorldIsRunning || !Health.IsAlive)return;
            if(GetComponent<NetworkCreature>()!=null){var target=SimulationAuthority.Target(this,null);if(target==null)return;Player=target.transform;}
            if(Vector2.Distance(body.position,Player.position)<Definition.DangerRange)Flee();
            else if(!IsFleeing && Time.time>=nextWander)ChooseDirection();
            Separation.Player=Player;
            var step=(IsFleeing ? Definition.FleeSpeed : Definition.WanderSpeed)*Time.fixedDeltaTime;
            var blocked=false;var hits=Physics2D.CircleCast(body.position,Member.Definition.SeparationRadius,direction,new ContactFilter2D{useTriggers=false},obstacles,step+.12f);
            for(var i=0;i<hits;i++)if(!obstacles[i].collider.transform.IsChildOf(Player)){blocked=true;break;}
            if(blocked){direction=(Vector2)(Quaternion.Euler(0,0,90)*direction);return;}
            var desired=direction+(IsFleeing ? Vector2.zero : Member.Cohesion());
            var position=body.position+Vector2.ClampMagnitude(desired,1)*step+Separation.Correction(Time.fixedDeltaTime);var bounds=Region.Bounds;
            if(!bounds.Contains(position)){direction=-direction;return;}
            body.MovePosition(position);
            var angle=Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg+90;
            Visual.rotation=Quaternion.RotateTowards(Visual.rotation,Quaternion.Euler(0,0,angle),360*Time.fixedDeltaTime);
        }
    }
}
