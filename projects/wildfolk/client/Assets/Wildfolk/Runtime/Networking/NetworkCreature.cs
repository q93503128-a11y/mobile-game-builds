using System.Collections;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    [DefaultExecutionOrder(200)]
    public sealed class NetworkCreature : NetworkBehaviour
    {
        public readonly NetworkVariable<CreatureIdentityState> Identity=new NetworkVariable<CreatureIdentityState>();
        public readonly NetworkVariable<CreatureFrame> Frame=new NetworkVariable<CreatureFrame>();
        public Health Health {get;private set;}
        public CreatureMember Member {get;private set;}
        public Transform Visual;
        public bool DeathResolved {get;private set;}
        private NeutralAnimal animal;
        private EnemyController enemy;
        private GoblinController goblin;
        private SlingshotAttack ranged;
        private ZombieLifecycle lifecycle;
        private CreatureVariant variant;
        private WildlifeActionBrain wildlife;
        private float nextWildlifePublish;
        private CharacterFacing facing;
        private ushort hits;
        private void Awake()
        {
            Health=GetComponent<Health>();Member=GetComponent<CreatureMember>();animal=GetComponent<NeutralAnimal>();enemy=GetComponent<EnemyController>();goblin=GetComponent<GoblinController>();
            ranged=GetComponent<SlingshotAttack>();lifecycle=GetComponent<ZombieLifecycle>();variant=GetComponent<CreatureVariant>();facing=GetComponent<CharacterFacing>();
            wildlife=GetComponent<WildlifeActionBrain>();Health.IsReplica=true;SetSimulation(false);
        }
        private void SetSimulation(bool enabled)
        {if(animal!=null)animal.enabled=enabled;if(enemy!=null)enemy.enabled=enabled;if(goblin!=null)goblin.enabled=enabled;if(lifecycle!=null)lifecycle.enabled=enabled;if(ranged!=null)ranged.enabled=enabled;if(wildlife!=null)wildlife.enabled=enabled;}
        public override void OnNetworkSpawn()
        {
            Health.IsReplica=!IsServer;SetSimulation(IsServer);
            if(IsServer){Health.Damaged+=Hit;Publish();NetworkManager.NetworkTickSystem.Tick+=Publish;}
            else {ApplyIdentity(Identity.Value);ApplyFrame(Frame.Value);}
            Identity.OnValueChanged+=IdentityChanged;Frame.OnValueChanged+=FrameChanged;
        }
        public override void OnNetworkDespawn()
        {
            if(NetworkManager!=null && NetworkManager.NetworkTickSystem!=null)NetworkManager.NetworkTickSystem.Tick-=Publish;
            Health.Damaged-=Hit;Identity.OnValueChanged-=IdentityChanged;Frame.OnValueChanged-=FrameChanged;SetSimulation(false);
        }
        private void Hit()=>hits++;
        private static Vector2 Q(Vector3 v)=>new Vector2(Mathf.Round(v.x*100)/100,Mathf.Round(v.y*100)/100);
        private void Publish()
        {
            if(!IsServer || !IsSpawned)return;
            if(wildlife!=null && Time.unscaledTime<nextWildlifePublish && Health.Current==Frame.Value.HP)return;
            nextWildlifePublish=Time.unscaledTime+.1f;
            var identity=GetComponent<GoblinIdentity>();
            var id=new CreatureIdentityState{Species=Member.Definition.Id,Group=Member.GroupId??string.Empty,Camp=identity==null ? string.Empty : identity.State.CampId??string.Empty,Anchor=Member.Anchor,Traits=(int)Member.AdditionalTraits,Variant=variant!=null && variant.Equipped};
            if(!Identity.Value.Equals(id))Identity.Value=id;
            var target=enemy!=null ? enemy.PlayerHealth : goblin!=null ? goblin.PlayerHealth : animal!=null && animal.Player!=null ? animal.Player.GetComponent<Health>() : null;
            var obj=target==null ? null : target.GetComponent<NetworkObject>();
            var next=new CreatureFrame{HP=Health.Current,Hits=hits,Target=obj==null ? ulong.MaxValue : obj.NetworkObjectId,Brain=enemy!=null ? (byte)enemy.State : goblin!=null ? (byte)goblin.State : (byte)(animal!=null && animal.IsFleeing ? 1 : 0),Lifecycle=lifecycle==null ? (byte)0 : (byte)lifecycle.Phase,Facing=facing==null ? Vector2.down : Q(facing.Direction),VisualPosition=Visual==null ? Vector2.zero : Q(Visual.localPosition),VisualScale=Visual==null ? Vector2.one : Q(Visual.localScale),Rotation=Visual==null ? 0 : Mathf.Round(Visual.localEulerAngles.z),Ranged=ranged!=null && ranged.IsBusy,ShotStarted=ranged==null ? 0 : ranged.NetworkStarted};
            if(facing!=null){next.Left=Q(facing.LeftOffset);next.Right=Q(facing.RightOffset);next.LeftArc=Mathf.Round(facing.LeftArc);next.RightArc=Mathf.Round(facing.RightArc);}
            if(wildlife!=null){next.Brain=(byte)wildlife.Phase;next.ShotStarted=wildlife.ServerStarted;next.Facing=Q(wildlife.Direction);next.Ranged=wildlife.Awakened;next.VisualPosition=Vector2.zero;next.VisualScale=Vector2.one;next.Rotation=0;}
            if(!Frame.Value.Equals(next))Frame.Value=next;
        }
        private void IdentityChanged(CreatureIdentityState old,CreatureIdentityState next){if(!IsServer)ApplyIdentity(next);}
        private void ApplyIdentity(CreatureIdentityState state)
        {Member.GroupId=state.Group.ToString();Member.Anchor=state.Anchor;Member.AdditionalTraits=(CreatureTrait)state.Traits;if(variant!=null)variant.SetEquipped(state.Variant);}
        private void FrameChanged(CreatureFrame old,CreatureFrame next){if(!IsServer)ApplyFrame(next,old.Hits!=next.Hits);}
        private void ApplyFrame(CreatureFrame state,bool hit=false)
        {Health.ApplyReplica(state.HP,hit);foreach(var collider in GetComponents<Collider2D>())collider.enabled=state.HP>0 && (lifecycle==null || state.Lifecycle==(byte)ZombiePhase.Active);}
        private void Update()
        {
            if(!IsSpawned || IsServer)return;
            var state=Frame.Value;var blend=1-Mathf.Exp(-Time.deltaTime*25);
            if(wildlife!=null)wildlife.PresentReplica(state.Brain,state.ShotStarted,state.Facing,NetworkManager.ServerTime.Time,state.Ranged);
            else if(Visual!=null){Visual.localPosition=Vector3.Lerp(Visual.localPosition,new Vector3(state.VisualPosition.x,state.VisualPosition.y,Visual.localPosition.z),blend);Visual.localScale=Vector3.Lerp(Visual.localScale,new Vector3(state.VisualScale.x,state.VisualScale.y,1),blend);Visual.localRotation=Quaternion.Slerp(Visual.localRotation,Quaternion.Euler(0,0,state.Rotation),blend);}
            if(facing!=null){facing.Face(state.Facing);facing.SetHandPose(0,state.Left,state.LeftArc);facing.SetHandPose(1,state.Right,state.RightArc);}
            if(ranged!=null)ranged.PresentReplica(state.Ranged,(float)(NetworkManager.ServerTime.Time-state.ShotStarted),state.Facing);
        }
        public void ResolveDeath(LootTable table)
        {
            if(!IsServer || !IsSpawned || DeathResolved)return;DeathResolved=true;
            var attacker=Health.LastAttacker==null ? null : Health.LastAttacker.GetComponent<NetworkObject>();
            NetworkCreatureWorld.Active?.RecordDeath(new NetworkDeathResult(NetworkObjectId,attacker==null ? ulong.MaxValue : attacker.NetworkObjectId,table));
            SetSimulation(false);GetComponent<HitFeedback>()?.BeginDeath();
            var body=GetComponent<Rigidbody2D>();if(body!=null)body.linearVelocity=Vector2.zero;
            Publish();foreach(var collider in GetComponents<Collider2D>())collider.enabled=false;StartCoroutine(RemoveDead());
        }
        private IEnumerator RemoveDead(){yield return new WaitForSeconds(HitFeedback.DeathDuration);if(IsSpawned && IsServer)NetworkObject.Despawn();}
    }
}
