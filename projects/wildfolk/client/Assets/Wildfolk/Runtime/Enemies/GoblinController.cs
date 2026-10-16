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
        public CreatureMember Member;
        public SoftSeparation Separation;
        public SlingshotAttack Ranged;
        public Vector2 Home=>home;
        private float repositionUntil,nextReposition;
        public GoblinState State {get;private set;}
        public bool InCombat=>State==GoblinState.Chase || State==GoblinState.Windup || State==GoblinState.Recovery;
        private Rigidbody2D body;private Vector2 home,direction,strike;private float timer,nextPatrol;
        private readonly RaycastHit2D[] obstacles=new RaycastHit2D[8];
        private void Awake()=>body=GetComponent<Rigidbody2D>();
        private void Start(){if(string.IsNullOrEmpty(Member.GroupId))home=body.position;else home=Member.Anchor;direction=Vector2.right;if(PlayerHealth!=null)Separation.Player=PlayerHealth.transform;}
        public void SetHome(Vector2 anchor){home=anchor;Member.Anchor=anchor;}
        public void ResetForSpawn(){State=GoblinState.Patrol;home=body.position;direction=Vector2.right;timer=nextPatrol=0;Pose.localScale=Vector3.one;}
        private void OnEnable(){OwnHealth.Damaged+=Aggro;Member.Alerted+=Assist;}
        private void OnDisable(){OwnHealth.Damaged-=Aggro;Member.Alerted-=Assist;if(Ranged!=null)Ranged.Cancel();Facing.ResetPose();}
        private void Aggro(){if(!SimulationAuthority.Allows(this) || OwnHealth.LastAttacker==null)return;PlayerHealth=OwnHealth.LastAttacker.GetComponent<Health>();if(PlayerHealth!=null)EnterCombat(PlayerHealth.transform.position,true);}
        private void Assist(Vector2 origin,CreatureAlert alert){if(alert==CreatureAlert.Fight && PlayerHealth!=null && Vector2.Distance(origin,home)<=Definition.Leash)EnterCombat(origin,false);}
        public void EnterCombat(Vector2 origin,bool broadcast=true){if(!SimulationAuthority.Allows(this) || InCombat)return;State=GoblinState.Chase;timer=0;if(broadcast)Member.Broadcast(origin,CreatureAlert.Fight);}
        private void Update()
        {
            if(!SimulationAuthority.Allows(this))return;
            if(PlayerHealth==null || !PlayerHealth.IsAlive){if(Ranged!=null)Ranged.Cancel();State=GoblinState.Patrol;timer=0;Facing.ResetPose();}
            if(State!=GoblinState.Windup && !(Ranged!=null && Ranged.IsBusy))PlayerHealth=SimulationAuthority.Target(this,PlayerHealth);
            if(!Session.WorldIsRunning || !OwnHealth.IsAlive || PlayerHealth==null || !PlayerHealth.IsAlive)return;
            Separation.Player=PlayerHealth.transform;
            timer+=Time.deltaTime;var delta=(Vector2)PlayerHealth.transform.position-body.position;
            if(State==GoblinState.Patrol || State==GoblinState.Watch)
            {
                var territory=Vector2.Distance(PlayerHealth.transform.position,home);
                if(territory<Definition.Territory || delta.magnitude<Definition.PersonalSpace)EnterCombat(PlayerHealth.transform.position);
                else if(territory<Definition.OuterTerritory){State=GoblinState.Watch;Facing.Face(delta);}
                else State=GoblinState.Patrol;
            }
            if(InCombat && (Vector2.Distance(body.position,home)>Definition.Leash || delta.magnitude>Definition.Leash)){State=GoblinState.Return;Pose.localScale=Vector3.one;timer=0;if(Ranged!=null)Ranged.Cancel();}
            if(Ranged!=null)
            {
                if(State==GoblinState.Chase && !Ranged.IsBusy && Time.time>=nextReposition && delta.magnitude<Ranged.Definition.MinimumDistance){repositionUntil=Time.time+Ranged.Definition.RepositionSeconds;nextReposition=Time.time+Ranged.Definition.RepositionCooldown;}
                if(State==GoblinState.Chase && !Ranged.IsBusy && Time.time>=repositionUntil && delta.magnitude<=Ranged.Definition.MaximumDistance && timer>=Definition.Cooldown){Ranged.Begin(PlayerHealth);timer=0;}
                return;
            }
            if(State==GoblinState.Chase && delta.magnitude<=Definition.AttackReach){State=GoblinState.Windup;timer=0;strike=delta.normalized;}
            if(State==GoblinState.Windup)
            {
                Pose.localScale=new Vector3(1.05f,.95f,1);Facing.Face(strike);Facing.SetHandPose(1,new Vector2(0,.22f*Mathf.Clamp01(timer/Definition.Windup)),-50+100*Mathf.Clamp01(timer/Definition.Windup));
                if(timer>=Definition.Windup){if(delta.magnitude<=Definition.AttackReach && Vector2.Dot(strike,delta.normalized)>.4f)PlayerHealth.ApplyDamage(Definition.Damage);State=GoblinState.Recovery;timer=0;Pose.localScale=Vector3.one;}
            }
            if(State==GoblinState.Recovery && timer>=Definition.Cooldown){State=GoblinState.Chase;Facing.ResetPose();}
        }
        private void FixedUpdate()
        {
            if(!SimulationAuthority.Allows(this))return;
            if(PlayerHealth==null || !PlayerHealth.IsAlive){if(Ranged!=null)Ranged.Cancel();State=GoblinState.Patrol;timer=0;Facing.ResetPose();}
            if(State!=GoblinState.Windup && !(Ranged!=null && Ranged.IsBusy))PlayerHealth=SimulationAuthority.Target(this,PlayerHealth);
            if(!Session.WorldIsRunning || !OwnHealth.IsAlive || PlayerHealth==null || !PlayerHealth.IsAlive)return;
            Separation.Player=PlayerHealth.transform;
            var speed=Definition.WanderSpeed;
            if(State==GoblinState.Chase){direction=((Vector2)PlayerHealth.transform.position-body.position).normalized;speed=Definition.Speed;
                if(Ranged!=null){if(Ranged.IsBusy){Steering.Move(Vector2.zero,.5f);return;}if(Time.time<repositionUntil)direction=-direction;else if(Vector2.Distance(body.position,PlayerHealth.transform.position)<=Ranged.Definition.PreferredDistance)direction=Vector2.zero;}}
            else if(State==GoblinState.Return){direction=(home-body.position).normalized;if(Vector2.Distance(body.position,home)<.5f)State=GoblinState.Patrol;}
            else if(State==GoblinState.Patrol)
            {if(Time.time>=nextPatrol){direction=Random.insideUnitCircle.normalized;nextPatrol=Time.time+3;}if(Vector2.Distance(body.position,home)>Member.Definition.RoamRadius)direction=(home-body.position).normalized;}
            else {if(Steering!=null)Steering.Move(Vector2.zero,speed);return;}
            if(State==GoblinState.Patrol)direction=Vector2.ClampMagnitude(direction+Member.Cohesion(),1);
            var step=speed*Time.fixedDeltaTime;
            if(Steering!=null){if(Steering.Move(direction,speed))Facing.Face(direction);}
            else if(Physics2D.CircleCast(body.position,.45f,direction,new ContactFilter2D{useTriggers=false},obstacles,step+.1f)==0){body.MovePosition(body.position+direction*step);Facing.Face(direction);}
        }
    }
}
