using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    public static class WorldChunkInterest
    {
        public static bool Visible(NetworkCreatureWorld world,ulong client,Vector2 position)
        {
            if(world.World.Map==null || !world.World.Map.Streamed || client==NetworkManager.ServerClientId)return true;
            foreach(var p in NetworkPlayer.Active)if(p.IsSpawned && p.OwnerClientId==client)
            {var a=WorldChunk.At(p.transform.position,world.World.Definition.ChunkSize);var b=WorldChunk.At(position,world.World.Definition.ChunkSize);return Mathf.Abs(a.X-b.X)<=1 && Mathf.Abs(a.Y-b.Y)<=1;}return false;
        }
        public static void Refresh(NetworkCreatureWorld world)
        {
            if(!world.IsAuthority || world.World.Map==null || !world.World.Map.Streamed)return;var manager=NetworkManager.Singleton;
            foreach(var pair in manager.SpawnManager.SpawnedObjects)
            {
                var obj=pair.Value;if(obj==world.GetComponent<NetworkObject>() || obj.GetComponent<NetworkPlayer>()!=null)continue;
                foreach(var client in manager.ConnectedClientsIds){if(client==NetworkManager.ServerClientId)continue;var visible=Visible(world,client,obj.transform.position);var observed=obj.IsNetworkVisibleTo(client);if(visible && !observed)obj.NetworkShow(client);else if(!visible && observed)obj.NetworkHide(client);}
            }
        }
    }
}
