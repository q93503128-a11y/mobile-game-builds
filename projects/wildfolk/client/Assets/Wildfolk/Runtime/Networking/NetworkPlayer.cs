using Unity.Netcode;
using UnityEngine;

namespace Wildfolk
{
    public sealed class NetworkPlayer : NetworkBehaviour,IPlayerMovementAuthority,IPlayerCombatAuthority
    {
        private static readonly System.Collections.Generic.List<NetworkPlayer> players=new System.Collections.Generic.List<NetworkPlayer>();
        public static System.Collections.Generic.IReadOnlyList<NetworkPlayer> Active=>players;
        public string ProfileId {get;internal set;}
        public ContentCatalog Catalog;
        public GameObject LocalMarker;
        public readonly NetworkVariable<PlayerMotionState> Motion=new NetworkVariable<PlayerMotionState>();
        public readonly NetworkVariable<PlayerStatusState> Status=new NetworkVariable<PlayerStatusState>();
        public readonly NetworkVariable<PlayerAttackState> AttackState=new NetworkVariable<PlayerAttackState>();
        private PlayerCombat combat;
        public PlayerInputCommands Commands {get;private set;}
        public PlayerMotor Motor {get;private set;}
        public Health Health {get;private set;}
        public PlayerEquipment Equipment {get;private set;}
        private PlayerInputAdapter input;
        private PlayerSurvival survival;
        private ulong actionSequence,lastActionRequest;
        private Vector2 pendingMove,lastSent;
        private float sentAt,receivedAt,nextDenial;

        private void Awake()
        {
            Commands=GetComponent<PlayerInputCommands>();Motor=GetComponent<PlayerMotor>();Health=GetComponent<Health>();Equipment=GetComponent<PlayerEquipment>();input=GetComponent<PlayerInputAdapter>();
            combat=GetComponent<PlayerCombat>();survival=GetComponent<PlayerSurvival>();combat.enabled=false;Health.IsReplica=true;
            input.enabled=false;Motor.enabled=false;Motor.Locomotion.enabled=false;Equipment.enabled=false;GetComponent<CharacterStats>().enabled=false;
        }
        public override void OnNetworkSpawn()
        {
            players.Add(this);
            Commands.MovementAuthority=this;Commands.CombatAuthority=this;
            combat.CreatureTargetsOnly=false;combat.NetworkTargetsOnly=true;combat.enabled=IsServer;Health.IsReplica=!IsServer;
            if(IsServer){combat.AttackStarted+=PublishAttack;combat.HarvestDenied+=PublishDenial;}
            Motor.Terrain=FindFirstObjectByType<WorldNavigation>();
            Motor.enabled=IsServer;Motor.Locomotion.enabled=IsServer;Equipment.enabled=IsServer;GetComponent<CharacterStats>().enabled=IsServer;
            input.enabled=IsOwner;LocalMarker.SetActive(IsOwner);
            NetworkManager.NetworkTickSystem.Tick+=Tick;
            Motion.OnValueChanged+=OnMotion;Status.OnValueChanged+=OnStatus;
            if(IsServer){Health.Changed+=PublishStatus;Equipment.Changed+=PublishStatus;survival.TreatmentChanged+=PublishStatus;survival.MealChanged+=PublishStatus;PublishStatus();receivedAt=Time.unscaledTime;}
            else ApplyStatus(Status.Value);
            ApplyMotion(Motion.Value);
            if(IsOwner && NetworkObject.IsPlayerObject)FindFirstObjectByType<MultiplayerArena>()?.BindLocal(this);
        }
        public override void OnNetworkDespawn()
        {
            if(IsServer)NetworkWorldPersistence.Active?.Remember(this);
            players.Remove(this);
            if(NetworkManager!=null && NetworkManager.NetworkTickSystem!=null)NetworkManager.NetworkTickSystem.Tick-=Tick;
            Motion.OnValueChanged-=OnMotion;Status.OnValueChanged-=OnStatus;Health.Changed-=PublishStatus;Equipment.Changed-=PublishStatus;
            combat.AttackStarted-=PublishAttack;combat.HarvestDenied-=PublishDenial;Commands.MovementAuthority=null;Commands.CombatAuthority=null;
            survival.TreatmentChanged-=PublishStatus;survival.MealChanged-=PublishStatus;survival.ResetTransient();
        }
        public void SetMove(Vector2 move){if(IsSpawned && IsOwner)pendingMove=NetworkInputRules.Sanitize(move);}
        public bool Dash()
        {
            if(!IsSpawned || !IsOwner)return false;
            if(IsServer){AcceptMove(pendingMove);return !Commands.IsScouting && Motor.Locomotion.TryDash();}
            DashRpc(pendingMove);return true;
        }
        public bool Attack()
        {if(!IsSpawned || !IsOwner || !Health.IsAlive)return false;if(Equipment.SelectedItem!=null && Equipment.SelectedItem.Telescope)return GetComponent<PlayerTelescope>().Use();if(Commands.IsScouting)return false;if(Equipment.ActivePlaceable!=null)return GetComponent<NetworkConstruction>().Place();
            if(IsServer)return ExecutePrimary(++actionSequence);AttackRpc(++actionSequence);return true;}
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]
        private void AttackRpc(ulong request,RpcParams rpc=default)
        {if(rpc.Receive.SenderClientId==OwnerClientId)ExecutePrimary(request);}
        public bool ExecutePrimary(ulong request)
        {if(!IsSpawned || !IsServer || request==0 || request<=lastActionRequest)return false;lastActionRequest=request;return Health.IsAlive && Equipment.ActivePlaceable==null && GetComponent<PlayerPrimaryAction>().TryUse();}
        private void PublishDenial(ResourceDefinition resource)
        {
            if(Time.unscaledTime<nextDenial)return;nextDenial=Time.unscaledTime+2.5f;
            var index=System.Array.FindIndex(Catalog.Items,i=>i==resource.DropItem);if(index>=0)HarvestDeniedRpc(index);
        }
        [Rpc(SendTo.Owner,InvokePermission=RpcInvokePermission.Server)]
        private void HarvestDeniedRpc(int item)
        {if(item<0 || item>=Catalog.Items.Length || NetworkCreatureWorld.Active==null)return;foreach(var rule in NetworkCreatureWorld.Active.World.Definition.Resources)if(rule.Prefab.Definition.DropItem==Catalog.Items[item]){PlayerFeedback.ShowRequirement(rule.Prefab.Definition);return;}}
        private void PublishAttack()
        {var previous=AttackState.Value;AttackState.Value=new PlayerAttackState{Sequence=(ushort)(previous.Sequence+1),Hand=(byte)combat.HandIndex,Tool=ItemIndex(Equipment.SelectedItem),Started=NetworkManager.ServerTime.Time};}
        private void Update()
        {
            if(!IsSpawned || IsServer)return;
            var attack=AttackState.Value;if(attack.Sequence==0)return;
            var item=Item(attack.Tool);var tool=item==null ? null : item.Tool;var duration=tool==null ? combat.Settings.PunchDuration : tool.Duration;
            var elapsed=(float)(NetworkManager.ServerTime.Time-attack.Started);
            if(elapsed>=0 && elapsed<duration)combat.Present(elapsed/duration,tool,attack.Hand);else Motor.Avatar.Facing.ResetPose();
        }
        [Rpc(SendTo.Server,Delivery=RpcDelivery.Unreliable,InvokePermission=RpcInvokePermission.Owner)]
        private void MoveRpc(Vector2 move,RpcParams rpc=default)
        {if(rpc.Receive.SenderClientId==OwnerClientId)AcceptMove(move);}
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]
        private void DashRpc(Vector2 move,RpcParams rpc=default)
        {if(rpc.Receive.SenderClientId!=OwnerClientId)return;AcceptMove(move);if(!Commands.IsScouting)Motor.Locomotion.TryDash();}
        private void AcceptMove(Vector2 move){Commands.ApplyMove(Commands.IsScouting ? Vector2.zero : NetworkInputRules.Sanitize(move));receivedAt=Time.unscaledTime;}
        private void Tick()
        {
            if(!IsSpawned)return;
            if(IsOwner && ((pendingMove-lastSent).sqrMagnitude>.0001f || Time.unscaledTime-sentAt>.15f))
            {if(IsServer)AcceptMove(pendingMove);else MoveRpc(pendingMove);lastSent=pendingMove;sentAt=Time.unscaledTime;}
            if(!IsServer)return;
            survival.AdvanceTemporary(1f/NetworkManager.NetworkConfig.TickRate);survival.AdvanceNaturalRecovery(1f/NetworkManager.NetworkConfig.TickRate);
            if(Time.unscaledTime-receivedAt>.4f)Commands.ApplyMove(Vector2.zero);
            var direction=Motor.Avatar.Facing.Direction;
            var flags=(byte)((Motor.IsMoving || Motor.Locomotion.IsDashing ? 1 : 0)|(Motor.Locomotion.Stamina.IsSprinting ? 2 : 0)|(Motor.Locomotion.IsDashing ? 4 : 0));
            var next=new PlayerMotionState{Facing=new Vector2(Mathf.Round(direction.x*100)/100,Mathf.Round(direction.y*100)/100),Flags=flags};
            if(!Motion.Value.Equals(next))Motion.Value=next;
        }
        private short ItemIndex(ItemDefinition item)=>item==null ? (short)-1 : (short)System.Array.IndexOf(Catalog.Items,item);
        private ItemDefinition Item(short index)=>index<0 || index>=Catalog.Items.Length ? null : Catalog.Items[index];
        private void PublishStatus()
        {
            if(!IsSpawned || !IsServer)return;
            var next=new PlayerStatusState{HP=Health.Current,MaximumHP=Health.Maximum,Head=ItemIndex(Equipment.HeadArmor),Body=ItemIndex(Equipment.BodyArmor),Held=ItemIndex(Equipment.SelectedItem),Utility=ItemIndex(Equipment.Utility),MealItem=ItemIndex(survival.Meal.Item),MealUntil=survival.Meal.Active ? NetworkManager.ServerTime.Time+survival.Meal.Remaining : 0,TreatmentUntil=survival.Treatment.Active ? NetworkManager.ServerTime.Time+survival.Treatment.Remaining : 0};
            if(!Status.Value.Equals(next))Status.Value=next;
        }
        private void OnMotion(PlayerMotionState previous,PlayerMotionState next)=>ApplyMotion(next);
        private void ApplyMotion(PlayerMotionState state)
        {
            if(!IsServer){Motor.Avatar.MovingOverride=state.Moving;Motor.Avatar.Facing.Face(state.Facing);}
            Motor.Avatar.MotionRate=state.Dashing ? 1.7f : state.Sprinting ? 1.35f : 1;
        }
        private void OnStatus(PlayerStatusState previous,PlayerStatusState next){if(!IsServer)ApplyStatus(next);}
        private void ApplyStatus(PlayerStatusState state)
        {
            if(state.MaximumHP<=0)return;
            Health.SetMaximumBonus(Mathf.Max(0,state.MaximumHP-Health.Definition.MaximumHealth));Health.ApplyReplica(state.HP);if(state.HP<=0)Commands.Session.ReplicateDefeat();
            Equipment.ApplyReplicatedAppearance(Item(state.Head),Item(state.Body),Item(state.Held));Equipment.RestoreUtility(Item(state.Utility));
        }
    }
}
