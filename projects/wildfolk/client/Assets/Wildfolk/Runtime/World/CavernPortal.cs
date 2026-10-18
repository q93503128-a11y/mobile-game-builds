using UnityEngine;
namespace Wildfolk
{
    public sealed class CavernPortal : MonoBehaviour,IPlayerInteractable
    {
        public WorldGenerator World;public string AnchorId;public bool ReturnToSurface;
        public Transform Location=>transform;
        public Sprite Icon=>GetComponent<SpriteRenderer>().sprite;
        public bool CanInteract(PlayerInteraction p)=>Vector2.Distance(p.transform.position,transform.position)<=p.Settings.StationInteractionRange;
        public bool Use(PlayerInteraction p)
        {if(!CanInteract(p))return false;var network=p.GetComponent<NetworkRegionTravel>();return network!=null ? network.Travel(AnchorId,ReturnToSurface) : Travel(World,p.gameObject,AnchorId,ReturnToSurface);}
        public static bool Travel(WorldGenerator world,GameObject player,string anchor,bool exit)
        {
            if(!SimulationAuthority.Allows(world) || !player.GetComponent<Health>().IsAlive || !player.GetComponent<PlayerInputCommands>().Session.IsRunning || !world.Map.Streamed || !anchor.StartsWith("cave"))return false;var pieces=anchor.Substring(4).Split(':');if(pieces.Length!=2 || !int.TryParse(pieces[0],out var caveId) || !int.TryParse(pieces[1],out var entrance))return false;
            if(caveId<0 || caveId>=world.Map.Continental.Caves.Count)return false;var cave=world.Map.Continental.Caves[caveId];if(entrance<0 || entrance>=cave.SurfaceEntrances.Count)return false;
            var source=exit ? cave.Entry(entrance) : cave.SurfaceEntrances[entrance];if(Vector2.Distance(player.transform.position,source)>4)return false;var destination=exit ? cave.SurfaceEntrances[entrance]+Vector2.down*2 : cave.Entry(entrance)+Vector2.down*2;
            if(!world.Map.IsWalkable(destination,.3f))return false;player.transform.position=destination;var body=player.GetComponent<Rigidbody2D>();body.position=destination;body.linearVelocity=Vector2.zero;player.GetComponent<PlayerCombat>().CancelAttack();world.Chunks.Reconcile();return true;
        }
    }
}
