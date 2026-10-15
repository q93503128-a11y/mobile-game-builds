using Unity.Netcode;
using UnityEngine;

namespace Wildfolk
{
    public sealed class NetworkPlayer : NetworkBehaviour,IPlayerMovementAuthority
    {
        public ContentCatalog Catalog;
        public GameObject LocalMarker;
        public readonly NetworkVariable<PlayerMotionState> Motion=new NetworkVariable<PlayerMotionState>();
        public readonly NetworkVariable<PlayerStatusState> Status=new NetworkVariable<PlayerStatusState>();
        public PlayerInputCommands Commands {get;private set;}
        public PlayerMotor Motor {get;private set;}
        public Health Health {get;private set;}
        public PlayerEquipment Equipment {get;private set;}
        private PlayerInputAdapter input;
        private Vector2 pendingMove,lastSent;
        private float sentAt,receivedAt;

        private void Awake()
        {
            Commands=GetComponent<PlayerInputCommands>();Motor=GetComponent<PlayerMotor>();Health=GetComponent<Health>();Equipment=GetComponent<PlayerEquipment>();input=GetComponent<PlayerInputAdapter>();
            input.enabled=false;Motor.enabled=false;Motor.Locomotion.enabled=false;Equipment.enabled=false;GetComponent<CharacterStats>().enabled=false;
        }
        public override void OnNetworkSpawn()
        {
            Commands.MovementAuthority=this;
            Motor.Terrain=FindFirstObjectByType<WorldNavigation>();
            Motor.enabled=IsServer;Motor.Locomotion.enabled=IsServer;Equipment.enabled=IsServer;GetComponent<CharacterStats>().enabled=IsServer;
            input.enabled=IsOwner;LocalMarker.SetActive(IsOwner);
            NetworkManager.NetworkTickSystem.Tick+=Tick;
            Motion.OnValueChanged+=OnMotion;Status.OnValueChanged+=OnStatus;
            if(IsServer){Health.Changed+=PublishStatus;Equipment.Changed+=PublishStatus;PublishStatus();receivedAt=Time.unscaledTime;}
            else ApplyStatus(Status.Value);
            ApplyMotion(Motion.Value);
            if(IsOwner)FindFirstObjectByType<MultiplayerArena>()?.BindLocal(this);
        }
        public override void OnNetworkDespawn()
        {
            if(NetworkManager!=null && NetworkManager.NetworkTickSystem!=null)NetworkManager.NetworkTickSystem.Tick-=Tick;
            Motion.OnValueChanged-=OnMotion;Status.OnValueChanged-=OnStatus;Health.Changed-=PublishStatus;Equipment.Changed-=PublishStatus;
            Commands.MovementAuthority=null;
        }
        public void SetMove(Vector2 move){if(IsSpawned && IsOwner)pendingMove=NetworkInputRules.Sanitize(move);}
        public bool Dash()
        {
            if(!IsSpawned || !IsOwner)return false;
            if(IsServer){AcceptMove(pendingMove);return Motor.Locomotion.TryDash();}
            DashRpc(pendingMove);return true;
        }
        [Rpc(SendTo.Server,Delivery=RpcDelivery.Unreliable,InvokePermission=RpcInvokePermission.Owner)]
        private void MoveRpc(Vector2 move,RpcParams rpc=default)
        {if(rpc.Receive.SenderClientId==OwnerClientId)AcceptMove(move);}
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]
        private void DashRpc(Vector2 move,RpcParams rpc=default)
        {if(rpc.Receive.SenderClientId!=OwnerClientId)return;AcceptMove(move);Motor.Locomotion.TryDash();}
        private void AcceptMove(Vector2 move){Commands.ApplyMove(NetworkInputRules.Sanitize(move));receivedAt=Time.unscaledTime;}
        private void Tick()
        {
            if(!IsSpawned)return;
            if(IsOwner && ((pendingMove-lastSent).sqrMagnitude>.0001f || Time.unscaledTime-sentAt>.15f))
            {if(IsServer)AcceptMove(pendingMove);else MoveRpc(pendingMove);lastSent=pendingMove;sentAt=Time.unscaledTime;}
            if(!IsServer)return;
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
            var next=new PlayerStatusState{HP=Health.Current,MaximumHP=Health.Maximum,Head=ItemIndex(Equipment.HeadArmor),Body=ItemIndex(Equipment.BodyArmor),Held=ItemIndex(Equipment.SelectedItem)};
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
            Health.SetMaximumBonus(Mathf.Max(0,state.MaximumHP-Health.Definition.MaximumHealth));Health.RestoreHealth(Mathf.Clamp(state.HP,0,Health.Maximum));
            Equipment.ApplyReplicatedAppearance(Item(state.Head),Item(state.Body),Item(state.Held));
        }
    }
}
