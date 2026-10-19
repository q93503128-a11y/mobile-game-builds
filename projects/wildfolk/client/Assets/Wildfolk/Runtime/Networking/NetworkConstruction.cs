using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    public sealed class NetworkConstruction : NetworkBehaviour
    {
        private NetworkInventory bag;private PlayerPlacement placement;
        private ulong sequence,lastRequest,pending;
        private uint waitRevision;private bool acknowledged;
        private void Awake(){bag=GetComponent<NetworkInventory>();placement=GetComponent<PlayerPlacement>();placement.enabled=false;}
        public override void OnNetworkSpawn()
        {
            placement.Session=bag.Player.Commands.Session;placement.Ground=NetworkCreatureWorld.Active.World.Terrain.Ground;placement.Terrain=NetworkCreatureWorld.Active.World.GetComponent<WorldNavigation>();placement.enabled=IsOwner;
        }
        public bool Place()
        {
            if(!IsSpawned || !IsOwner || pending!=0 || bag.Pending)return false;
            var definition=bag.Player.Equipment.ActivePlaceable;if(definition==null)return false;
            var position=(Vector2)transform.position+bag.Player.Motor.Avatar.Facing.Direction*placement.Settings.PlacementDistance;
            if(IsServer)return ExecutePlace(bag.Revision.Value,bag.Inventory.SelectedIndex,position);
            pending=++sequence;acknowledged=false;PlaceRpc(pending,bag.Revision.Value,bag.Inventory.SelectedIndex,position);return true;
        }
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]
        private void PlaceRpc(ulong request,uint revision,int slot,Vector2 position,RpcParams rpc=default)
        {
            if(rpc.Receive.SenderClientId!=OwnerClientId || request<=lastRequest)return;lastRequest=request;var ok=ExecutePlace(revision,slot,position);ResultRpc(request,ok,bag.Revision.Value);
        }
        [Rpc(SendTo.Owner,InvokePermission=RpcInvokePermission.Server)]
        private void ResultRpc(ulong request,bool ok,uint revision){if(pending!=request)return;acknowledged=true;waitRevision=revision;if(!ok)PlayerFeedback.ShowKey("feedback.slot_space");Finish();}
        private void Update(){if(acknowledged)Finish();}
        private void Finish(){if(bag.Revision.Value>=waitRevision){pending=0;acknowledged=false;}}
        public bool ExecutePlace(uint revision,int slot,Vector2 position)
        {
            if(!bag.CanOperate || bag.Player.Commands.IsScouting || revision!=bag.Revision.Value || slot!=bag.Inventory.SelectedIndex || !bag.Player.Commands.Session.IsRunning || !Finite(position))return false;
            var definition=bag.Player.Equipment.ActivePlaceable;Physics2D.SyncTransforms();if(definition==null || !placement.CanPlaceAt(definition,position))return false;
            WorldPlaceable placed=null;
            try
            {
                var committed=bag.Inventory.Model.TryDropSlot(slot,definition.Item,1,()=>
                {
                    placed=NetworkCreatureWorld.Active.Create(definition.Prefab,position,placement.RotationFor(definition));placed.CreatorId=bag.Player.ProfileId;
                    Physics2D.SyncTransforms();return NetworkCreatureWorld.Active.SpawnPendingNow(placed.GetComponent<NetworkObject>());
                });
                if(!committed && placed!=null)PrefabPool.Release(placed.gameObject);return committed;
            }
            catch(System.Exception error){if(placed!=null)PrefabPool.Release(placed.gameObject);Debug.LogError("Building transaction failed: "+error.GetType().Name);return false;}
        }
        private static bool Finite(Vector2 p)=>!float.IsNaN(p.x)&&!float.IsNaN(p.y)&&!float.IsInfinity(p.x)&&!float.IsInfinity(p.y);
        public bool Door(NetworkBuilding building)
        {
            if(!IsSpawned || !IsOwner || building==null)return false;
            if(IsServer)return ExecuteDoor(building.NetworkObjectId,building.Revision.Value,!building.Door.IsOpen);
            DoorRpc(++sequence,building.NetworkObjectId,building.Revision.Value,!building.Door.IsOpen);return true;
        }
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]
        private void DoorRpc(ulong request,ulong id,uint revision,bool desired,RpcParams rpc=default)
        {if(rpc.Receive.SenderClientId!=OwnerClientId || request<=lastRequest)return;lastRequest=request;ExecuteDoor(id,revision,desired);}
        public bool ExecuteDoor(ulong id,uint revision,bool desired)
        {return bag.CanOperate && bag.Player.Commands.Session.IsRunning && NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(id,out var obj) && obj.TryGetComponent<NetworkBuilding>(out var building) && building.SetDoor(bag.Player,desired,revision);}
        public override void OnNetworkDespawn(){pending=0;acknowledged=false;placement.enabled=false;if(placement.Preview!=null)placement.Preview.SetActive(false);}
    }
}
