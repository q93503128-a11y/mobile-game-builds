using UnityEngine;
namespace Wildfolk
{
    public sealed class WorldSpawnRegion : MonoBehaviour
    {
        public Vector2 Size=new Vector2(44,24);
        public Bounds Bounds => new Bounds(transform.position,new Vector3(Size.x,Size.y,1));
        public Vector2 PerimeterPoint(int side,float along)
        {
            var bounds=Bounds;along=Mathf.Clamp01(along);
            switch(side%4)
            {
                case 0:return new Vector2(Mathf.Lerp(bounds.min.x,bounds.max.x,along),bounds.max.y);
                case 1:return new Vector2(Mathf.Lerp(bounds.min.x,bounds.max.x,along),bounds.min.y);
                case 2:return new Vector2(bounds.min.x,Mathf.Lerp(bounds.min.y,bounds.max.y,along));
                default:return new Vector2(bounds.max.x,Mathf.Lerp(bounds.min.y,bounds.max.y,along));
            }
        }
    }
}
