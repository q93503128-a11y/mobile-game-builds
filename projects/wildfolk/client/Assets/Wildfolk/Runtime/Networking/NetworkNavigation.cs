using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    // Only the stable bench ID is persisted. Coordinates below are a derived owner-only presentation cache.
    public sealed class NetworkNavigation : NetworkBehaviour
    {
        public readonly NetworkVariable<FixedString64Bytes> Home=new NetworkVariable<FixedString64Bytes>(default,NetworkVariableReadPermission.Owner);
        public readonly NetworkVariable<Vector2> HomePosition=new NetworkVariable<Vector2>(default,NetworkVariableReadPermission.Owner);
        public readonly NetworkVariable<bool> HomeValid=new NetworkVariable<bool>(false,NetworkVariableReadPermission.Owner);
        private PlayerNavigation navigation;
        private float nextCheck;
        private void Awake()=>navigation=GetComponent<PlayerNavigation>();
        public override void OnNetworkSpawn(){Home.OnValueChanged+=Changed;if(!IsServer)navigation.RestoreHome(Home.Value.ToString());}
        public override void OnNetworkDespawn()=>Home.OnValueChanged-=Changed;
        private void Changed(FixedString64Bytes previous,FixedString64Bytes next){if(!IsServer)navigation.RestoreHome(next.ToString());}
        public bool Select(CraftingStation station)
        {
            if(!IsSpawned || !IsOwner || station==null)return false;var facility=station.GetComponent<NetworkObject>();if(facility==null || !facility.IsSpawned)return false;
            if(IsServer)return Assign(facility.NetworkObjectId);SelectRpc(facility.NetworkObjectId);return true;
        }
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]
        private void SelectRpc(ulong id,RpcParams rpc=default){if(rpc.Receive.SenderClientId==OwnerClientId)Assign(id);}
        public bool Assign(ulong id)
        {
            if(!IsServer || !GetComponent<NetworkInventory>().CanOperate || !NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(id,out var obj) || !navigation.AssignHome(obj.GetComponent<CraftingStation>()))return false;
            Publish();return true;
        }
        private void Update(){if(!IsSpawned || !IsServer || Time.unscaledTime<nextCheck)return;nextCheck=Time.unscaledTime+.5f;Publish();}
        private void Publish()
        {
            Home.Value=new FixedString64Bytes(navigation.HomeId??"");
            var world=NetworkCreatureWorld.Active;if(world==null)return;
            var valid=navigation.TryHome(world.World,out var position);HomeValid.Value=valid;if(valid)HomePosition.Value=position;
        }
    }
}
