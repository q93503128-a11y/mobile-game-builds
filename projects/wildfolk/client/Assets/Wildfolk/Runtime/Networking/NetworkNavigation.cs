using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    // Only the stable bench ID is persisted. Coordinates below are a derived owner-only presentation cache.
    public struct NetworkBeacon : INetworkSerializable,System.IEquatable<NetworkBeacon>
    {public FixedString64Bytes Id;public int Number;public Vector2 Position;public void NetworkSerialize<T>(BufferSerializer<T> s)where T:IReaderWriter{s.SerializeValue(ref Id);s.SerializeValue(ref Number);s.SerializeValue(ref Position);}public bool Equals(NetworkBeacon b)=>Id.Equals(b.Id)&&Number==b.Number&&Position==b.Position;}
    public sealed class NetworkNavigation : NetworkBehaviour
    {
        public readonly NetworkVariable<FixedString64Bytes> Home=new NetworkVariable<FixedString64Bytes>(default,NetworkVariableReadPermission.Owner);
        public readonly NetworkVariable<Vector2> HomePosition=new NetworkVariable<Vector2>(default,NetworkVariableReadPermission.Owner);
        public readonly NetworkVariable<bool> HomeValid=new NetworkVariable<bool>(false,NetworkVariableReadPermission.Owner);
        public NetworkList<NetworkBeacon> Beacons;
        public readonly NetworkVariable<NavigationMode> Mode=new NetworkVariable<NavigationMode>(NavigationMode.Automatic,NetworkVariableReadPermission.Owner);
        public readonly NetworkVariable<FixedString64Bytes> Beacon=new NetworkVariable<FixedString64Bytes>(default,NetworkVariableReadPermission.Owner);
        private uint beaconRevision=uint.MaxValue;
        private PlayerNavigation navigation;
        private float nextCheck;
        private void Awake(){navigation=GetComponent<PlayerNavigation>();Beacons=new NetworkList<NetworkBeacon>();}
        public override void OnNetworkSpawn(){Home.OnValueChanged+=Changed;Mode.OnValueChanged+=ModeChanged;Beacon.OnValueChanged+=BeaconChanged;if(!IsServer)navigation.RestoreHome(Home.Value.ToString());}
        public override void OnNetworkDespawn(){Home.OnValueChanged-=Changed;Mode.OnValueChanged-=ModeChanged;Beacon.OnValueChanged-=BeaconChanged;}
        private void ModeChanged(NavigationMode a,NavigationMode b){if(!IsServer)navigation.RestoreNavigation(b,Beacon.Value.ToString());}
        private void BeaconChanged(FixedString64Bytes a,FixedString64Bytes b){if(!IsServer)navigation.RestoreNavigation(Mode.Value,b.ToString());}
        public void SelectNavigation(NavigationMode mode,string id){if(!IsSpawned || !IsOwner)return;if(IsServer)AssignNavigation(mode,id);else NavigationRpc(mode,new FixedString64Bytes(id??""));}
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]private void NavigationRpc(NavigationMode mode,FixedString64Bytes id,RpcParams rpc=default){if(rpc.Receive.SenderClientId==OwnerClientId)AssignNavigation(mode,id.ToString());}
        private void AssignNavigation(NavigationMode mode,string id){if(!GetComponent<NetworkInventory>().CanOperate || mode<NavigationMode.Automatic || mode>NavigationMode.Beacon || mode==NavigationMode.Beacon && !BeaconDirectory.Points.ContainsKey(id??""))return;navigation.RestoreNavigation(mode,id);Publish();}
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
            Home.Value=new FixedString64Bytes(navigation.HomeId??"");Mode.Value=navigation.Mode;Beacon.Value=new FixedString64Bytes(navigation.BeaconId??"");
            if(beaconRevision!=BeaconDirectory.Revision){beaconRevision=BeaconDirectory.Revision;Beacons.Clear();foreach(var p in BeaconDirectory.Points.Values)Beacons.Add(new NetworkBeacon{Id=new FixedString64Bytes(p.Id),Number=p.Number,Position=p.Position});}
            var world=NetworkCreatureWorld.Active;if(world==null)return;
            var valid=navigation.TryHome(world.World,out var position);HomeValid.Value=valid;if(valid)HomePosition.Value=position;
        }
    }
}
