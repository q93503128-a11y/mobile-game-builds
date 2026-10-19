using System;
using UnityEngine;
namespace Wildfolk
{
    public enum NavigationMode { Automatic,Off,Home,CaveExit,Beacon }
    public sealed class PlayerNavigation : MonoBehaviour
    {
        public ItemDefinition Compass;
        public NavigationMode Mode {get;private set;}
        public string BeaconId {get;private set;}
        public void RestoreNavigation(NavigationMode mode,string id){Mode=mode;BeaconId=id;Changed?.Invoke();}
        public void SelectNavigation(NavigationMode mode,string id=null){RestoreNavigation(mode,id);GetComponent<NetworkNavigation>()?.SelectNavigation(mode,id);}
        public bool TryBeacon(out Vector2 point,out int number){point=default;number=0;var net=GetComponent<NetworkNavigation>();if(net!=null && !net.IsServer){foreach(var b in net.Beacons)if(b.Id.ToString()==BeaconId){point=b.Position;number=b.Number;return true;}return false;}if(!string.IsNullOrEmpty(BeaconId) && BeaconDirectory.Points.TryGetValue(BeaconId,out var p)){point=p.Position;number=p.Number;return true;}return false;}
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
        public static Vector2 CaveExit(CavernNetwork cave,Vector2 point)=>cave.ExitWaypoint(point);
    }
}
