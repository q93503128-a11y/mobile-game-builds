using UnityEngine;
namespace Wildfolk
{
    public enum TerrainSurface { Land, Shallow, Deep }
    public sealed class WorldNavigation : MonoBehaviour
    {
        public WorldGenerator World;
        public Camera Camera;
        public Transform Player;
        public Vector2 StaticAreaSize;
        private readonly Collider2D[] overlaps=new Collider2D[64];
        private readonly RaycastHit2D[] hits=new RaycastHit2D[32];
        private static readonly Vector2[] directions={Vector2.up,Vector2.down,Vector2.left,Vector2.right};
        public TerrainSurface Surface(Vector2 p)=>World==null ? TerrainSurface.Land : World.Map.IsDeepWater(p) ? TerrainSurface.Deep : World.Map.IsWater(p) ? TerrainSurface.Shallow : TerrainSurface.Land;
        public float MovementMultiplier(Vector2 p)=>Surface(p)==TerrainSurface.Deep ? 0 : Surface(p)==TerrainSurface.Shallow ? World.Definition.ShallowMoveMultiplier : 1;
        public bool OutsideCamera(Vector2 p,float margin=.05f){var v=Camera.WorldToViewportPoint(p);return v.x< -margin || v.x>1+margin || v.y< -margin || v.y>1+margin;}
        public bool Traversable(Vector2 p,float radius)=>World!=null ? World.Map.IsWalkable(p,radius) : Mathf.Abs(p.x)+radius<StaticAreaSize.x*.5f && Mathf.Abs(p.y)+radius<StaticAreaSize.y*.5f;
        public bool CanMove(Vector2 origin,Vector2 direction,float distance,float radius,Collider2D ignore=null)
        {if(!Traversable(origin+direction*distance,radius))return false;var count=Physics2D.CircleCast(origin,radius,direction,new ContactFilter2D{useTriggers=false},hits,distance+.08f);for(var i=0;i<count;i++)if(hits[i].collider!=ignore)return false;return true;}
        public float AllowedDistance(Vector2 origin,Vector2 direction,float distance,float radius,Collider2D ignore=null)
        {
            var allowed=distance;
            if(!Traversable(origin+direction*allowed,radius))
            {var low=0f;var high=allowed;for(var i=0;i<5;i++){var mid=(low+high)*.5f;if(Traversable(origin+direction*mid,radius))low=mid;else high=mid;}allowed=low;}
            var count=Physics2D.CircleCast(origin,radius,direction,new ContactFilter2D{useTriggers=false},hits,allowed+.08f);
            if(count==hits.Length)return 0;
            for(var i=0;i<count;i++)if(hits[i].collider!=ignore)allowed=Mathf.Min(allowed,Mathf.Max(0,hits[i].distance-.08f));
            return allowed;
        }
        public bool CanSpawn(Vector2 p,float radius,float playerDistance,bool outside=true)
        {
            if(NetworkCreatureWorld.Active!=null)foreach(var player in NetworkPlayer.Active)if(Vector2.Distance(p,player.transform.position)<playerDistance)return false;
            if(!Traversable(p,radius) || World.Map.IsWater(p,radius) || Vector2.Distance(p,Player.position)<playerDistance || outside && !OutsideCamera(p))return false;
            if(Physics2D.OverlapCircle(p,radius,new ContactFilter2D{useTriggers=true},overlaps)>0)return false;
            foreach(var direction in directions)
            {
                var count=Physics2D.Raycast(p,direction,new ContactFilter2D{useTriggers=false},hits,60);var barrier=false;
                for(var i=0;i<count;i++){var placed=hits[i].collider.GetComponentInParent<WorldPlaceable>();if(placed!=null && placed.Definition.IsBarrier){barrier=true;break;}}
                if(!barrier)return true;
            }
            return false;
        }
    }
}
