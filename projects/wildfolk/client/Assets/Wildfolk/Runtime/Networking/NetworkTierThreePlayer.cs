using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    public sealed class NetworkTierThreePlayer : NetworkBehaviour
    {
        public readonly NetworkVariable<bool> Scouting=new NetworkVariable<bool>(false,NetworkVariableReadPermission.Owner);
        private ulong sequence,lastRequest;private float nextAim;
        private PlayerInventory bag;private NetworkInventory inventory;private PlayerTelescope telescope;
        private void Awake(){bag=GetComponent<PlayerInventory>();inventory=GetComponent<NetworkInventory>();telescope=GetComponent<PlayerTelescope>();}
        public override void OnNetworkSpawn(){Scouting.OnValueChanged+=ScopeChanged;}
        public override void OnNetworkDespawn(){Scouting.OnValueChanged-=ScopeChanged;telescope.SetViewing(false);}
        private void ScopeChanged(bool old,bool value){if(IsOwner)telescope.SetViewing(value);}
        public bool Scope(bool enabled)
        {if(!IsSpawned || !IsOwner)return false;if(IsServer)return SetScope(enabled);ScopeRpc(enabled);return true;}
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]private void ScopeRpc(bool enabled,RpcParams rpc=default){if(rpc.Receive.SenderClientId==OwnerClientId)SetScope(enabled);}
        private bool SetScope(bool value)
        {if(value && (!inventory.CanOperate || !GetComponent<PlayerInputCommands>().Session.IsRunning || bag.SelectedStack.IsEmpty || !bag.SelectedStack.Item.Telescope))return false;Scouting.Value=value;telescope.SetViewing(value);return true;}
        public bool Operate(WorldTurret turret,TurretOperation op,int slot=0,float angle=0)
        {
            if(!IsSpawned || !IsOwner || turret==null)return false;var target=turret.GetComponent<NetworkTurret>();if(target==null || !target.IsSpawned)return false;
            if(op==TurretOperation.Aim){if(Time.unscaledTime<nextAim)return true;nextAim=Time.unscaledTime+.1f;}
            if(IsServer)return Execute(target.NetworkObjectId,op,slot,angle,inventory.Revision.Value,target.Revision.Value);
            OperateRpc(++sequence,target.NetworkObjectId,op,slot,angle,inventory.Revision.Value,target.Revision.Value);return true;
        }
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]
        private void OperateRpc(ulong serial,ulong id,TurretOperation op,int slot,float angle,uint bagRevision,uint turretRevision,RpcParams rpc=default)
        {if(rpc.Receive.SenderClientId!=OwnerClientId || serial<=lastRequest)return;lastRequest=serial;if(!Execute(id,op,slot,angle,bagRevision,turretRevision))DeniedRpc();}
        private bool Execute(ulong id,TurretOperation op,int slot,float angle,uint bagRevision,uint turretRevision)
        {
            if(!IsServer || !inventory.CanOperate || Scouting.Value || !NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(id,out var obj))return false;
            var turret=obj.GetComponent<WorldTurret>();var net=obj.GetComponent<NetworkTurret>();if(turret==null || net==null || !turret.InRange(bag))return false;
            if(op!=TurretOperation.Aim && op!=TurretOperation.Fire && (bagRevision!=inventory.Revision.Value || turretRevision!=net.Revision.Value))return false;
            var session=GetComponent<PlayerInputCommands>().Session;if(session.State!=SessionState.Inventory && !session.IsRunning)return false;
            return turret.Execute(bag,op,slot,angle);
        }
        [Rpc(SendTo.Owner,InvokePermission=RpcInvokePermission.Server)]private void DeniedRpc()=>PlayerFeedback.ShowKey("feedback.turret_unavailable");
        public void RecoverBeacon(WorldPlaceable beacon)
        {if(!IsSpawned || !IsOwner || beacon==null)return;var obj=beacon.GetComponent<NetworkObject>();if(obj==null || !obj.IsSpawned)return;if(IsServer)Recover(obj.NetworkObjectId);else RecoverRpc(obj.NetworkObjectId);}
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]private void RecoverRpc(ulong id,RpcParams rpc=default){if(rpc.Receive.SenderClientId==OwnerClientId)Recover(id);}
        private bool Recover(ulong id){if(!inventory.CanOperate || !NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(id,out var obj))return false;return obj.GetComponent<SignalBeacon>()?.Recover(bag)??false;}
    }
}
