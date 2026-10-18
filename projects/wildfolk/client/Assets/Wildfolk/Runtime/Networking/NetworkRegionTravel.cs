using Unity.Netcode;
namespace Wildfolk
{
    public sealed class NetworkRegionTravel : NetworkBehaviour
    {
        public bool Travel(string anchor,bool exit)
        {if(!IsSpawned || !IsOwner || !GetComponent<Health>().IsAlive)return false;if(IsServer)return CavernPortal.Travel(NetworkCreatureWorld.Active.World,gameObject,anchor,exit);TravelRpc(anchor,exit);return true;}
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]private void TravelRpc(string anchor,bool exit,RpcParams rpc=default)
        {if(rpc.Receive.SenderClientId==OwnerClientId && GetComponent<Health>().IsAlive && GetComponent<PlayerInputCommands>().Session.IsRunning)CavernPortal.Travel(NetworkCreatureWorld.Active.World,gameObject,anchor,exit);}
    }
}
