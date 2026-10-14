using UnityEngine;
namespace Wildfolk
{
    public sealed class WorldSpawnRegion : MonoBehaviour
    {
        public Vector2 Size=new Vector2(44,24);
        public Bounds Bounds => new Bounds(transform.position,new Vector3(Size.x,Size.y,1));
    }
}
