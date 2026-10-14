using UnityEngine;
namespace Wildfolk
{
    public enum GoblinState { Patrol,Chase,Windup,Recovery,Return,Watch }
    public sealed class GoblinController : MonoBehaviour,IPoolReset
    {
        public GoblinDefinition Definition;
        public GameplaySession Session;
        public Health OwnHealth,PlayerHealth;
        public CharacterFacing Facing;
        public Transform Pose;
        public EnemySteering Steering;
        public GoblinState State {get;private set;}
        public bool InCombat=>State==GoblinState.Chase || State==GoblinState.Windup || State==GoblinState.Recovery;
        private Rigidbody2D body;private Vector2 home,direction,strike;private float timer,nextPatrol;
        private readonly RaycastHit2D[] obstacles=new RaycastHit2D[8];
        private void Awake()=>body=GetComponent<Rigidbody2D>();
        private void Start(){home=body.position;direction=Vector2.right;}
        public void ResetForSpawn(){State=GoblinState.Patrol;home=body.position;direction=Vector2.right;timer=nextPatrol=0;Pose.localScale=Vector3.one;}
        private void OnEnable()=>OwnHealth.Damaged+=Aggro;
        private void OnDisable()=>OwnHealth.Damaged-=Aggro;
        private void Aggro(){if(OwnHealth.LastAttacker!=null){State=GoblinState.Chase;timer=0;}}
        private void Update()
        {
            if(!Session.WorldIsRunning || !OwnHealth.IsAlive || !PlayerHealth.IsAlive)return;
            timer+=Time.deltaTime;var delta=(Vector2)PlayerHealth.transform.position-body.position;
            if(State==GoblinState.Patrol || State==GoblinState.Watch)
            {
                var territory=Vector2.Distance(PlayerHealth.transform.position,home);
                if(territory<Definition.Territory || delta.magnitude<Definition.PersonalSpace)State=GoblinState.Chase;
                else if(territory<Definition.OuterTerritory){State=GoblinState.Watch;Facing.Face(delta);}
                else State=GoblinState.Patrol;
            }
            if(InCombat && (Vector2.Distance(body.position,home)>Definition.Leash || delta.magnitude>Definition.Leash)){State=GoblinState.Return;Pose.localScale=Vector3.one;timer=0;}
            if(State==GoblinState.Chase && delta.magnitude<=Definition.AttackReach){State=GoblinState.Windup;timer=0;strike=delta.normalized;}
            if(State==GoblinState.Windup)
            {
                Pose.localScale=new Vector3(1.05f,.95f,1);Facing.Face(strike);
                if(timer>=Definition.Windup){if(delta.magnitude<=Definition.AttackReach && Vector2.Dot(strike,delta.normalized)>.4f)PlayerHealth.ApplyDamage(Definition.Damage);State=GoblinState.Recovery;timer=0;Pose.localScale=Vector3.one;}
            }
            if(State==GoblinState.Recovery && timer>=Definition.Cooldown)State=GoblinState.Chase;
        }
        private void FixedUpdate()
        {
            if(!Session.WorldIsRunning || !OwnHealth.IsAlive || !PlayerHealth.IsAlive)return;
            var speed=Definition.WanderSpeed;
            if(State==GoblinState.Chase){direction=((Vector2)PlayerHealth.transform.position-body.position).normalized;speed=Definition.Speed;}
            else if(State==GoblinState.Return){direction=(home-body.position).normalized;if(Vector2.Distance(body.position,home)<.5f)State=GoblinState.Patrol;}
            else if(State==GoblinState.Patrol)
            {if(Time.time>=nextPatrol){direction=Random.insideUnitCircle.normalized;nextPatrol=Time.time+3;}if(Vector2.Distance(body.position,home)>3)direction=(home-body.position).normalized;}
            else {if(Steering!=null)Steering.Move(Vector2.zero,speed);return;}
            var step=speed*Time.fixedDeltaTime;
            if(Steering!=null){if(Steering.Move(direction,speed))Facing.Face(direction);}
            else if(Physics2D.CircleCast(body.position,.45f,direction,new ContactFilter2D{useTriggers=false},obstacles,step+.1f)==0){body.MovePosition(body.position+direction*step);Facing.Face(direction);}
        }
    }
}
