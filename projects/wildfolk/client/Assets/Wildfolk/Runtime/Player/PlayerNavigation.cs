using System;
using UnityEngine;
namespace Wildfolk
{
    public sealed class PlayerNavigation : MonoBehaviour
    {
        public ItemDefinition Compass;
        public string HomeId {get;private set;}
        public event Action Changed;
        public bool HasCompass=>GetComponent<PlayerInventory>().Model!=null && Compass!=null && GetComponent<PlayerInventory>().Model.CountItem(Compass)>0;
        public void RestoreHome(string id){HomeId=id;Changed?.Invoke();}
        public bool SelectHome(CraftingStation station)
        {
            var net=GetComponent<NetworkNavigation>();if(net!=null)return net.Select(station);
            return AssignHome(station);
        }
        public bool AssignHome(CraftingStation station)
        {
            var player=GetComponent<PlayerCrafting>();
            if(!GetComponent<Health>().IsAlive || station==null || station.Definition.Station!=StationKind.Workbench || !station.CanInteract(transform.position,player.Settings.StationInteractionRange))return false;
            var placed=station.GetComponent<WorldPlaceable>();if(placed==null || !placed.Health.IsAlive)return false;
            RestoreHome(placed.SaveId);return true;
        }
        public bool TryHome(WorldGenerator world,out Vector2 position)
        {
            position=default;if(string.IsNullOrEmpty(HomeId))return false;
            foreach(var f in WorldPlaceable.Active)if(f.SaveId==HomeId && f.Health.IsAlive && f.Definition.Station==StationKind.Workbench){position=f.transform.position;return true;}
            return world.Chunks!=null && world.Map.Streamed && world.Chunks.TryHome(HomeId,out position);
        }
    }
    public static class NavigationBearing
    {
        // Camera projection, never player facing. Fixed north and home remain independent of held tools.
        public static float Angle(Vector2 direction,Camera camera)
        {
            var right=(Vector2)camera.transform.right;var up=(Vector2)camera.transform.up;
            return -Mathf.Atan2(Vector2.Dot(direction,right),Vector2.Dot(direction,up))*Mathf.Rad2Deg;
        }
        public static Vector2 CaveExit(CavernNetwork cave,Vector2 point)
        {
            var n=cave.Rooms.Count;var distances=new float[n];var next=new int[n];var used=new bool[n];
            for(var i=0;i<n;i++){distances[i]=float.MaxValue;next[i]=-1;}
            distances[0]=0;if(cave.SurfaceEntrances.Count>1)distances[2]=0;
            for(var pass=0;pass<n;pass++)
            {
                var at=-1;for(var i=0;i<n;i++)if(!used[i] && (at<0 || distances[i]<distances[at]))at=i;
                if(at<0)break;used[at]=true;
                foreach(var j in cave.Rooms[at].Neighbors){var d=distances[at]+Vector2.Distance(cave.Rooms[at].Center,cave.Rooms[j].Center);if(d<distances[j]){distances[j]=d;next[j]=at;}}
            }
            var nearest=cave.Rooms.IndexOf(cave.Nearest(point));var room=cave.Rooms[nearest];
            // Return to a chamber center before following its next corridor, so arrows do not cut cave walls.
            if(Vector2.Distance(point,room.Center)>2)return room.Center;
            return next[nearest]<0 ? room.Center : cave.Rooms[next[nearest]].Center;
        }
    }
}
