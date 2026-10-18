using Unity.Netcode;
namespace Wildfolk
{
    public sealed class NetworkContainer:NetworkBehaviour
    {
        private double nextRequest;
        public bool Fill(){if(!IsSpawned || !IsOwner)return false;if(IsServer)return Apply();FillRpc();return true;}
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]
        private void FillRpc(RpcParams rpc=default){if(rpc.Receive.SenderClientId==OwnerClientId)Apply();}
        private bool Apply(){if(!GetComponent<NetworkInventory>().CanOperate || !GetComponent<PlayerInputCommands>().Session.IsRunning || NetworkManager.ServerTime.Time<nextRequest)return false;nextRequest=NetworkManager.ServerTime.Time+.25;return GetComponent<PlayerWaterskin>().Fill();}
    }
}
