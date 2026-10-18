using UnityEngine;
namespace Wildfolk
{
    public sealed partial class WorldChunkRuntime
    {
        private void MaterializeWater(WorldChunk key,Transform parent)
        {
            if(World.Map.Continental.Underground(key.Center(World.Definition.ChunkSize)))return;
            var origin=key.Origin(World.Definition.ChunkSize);for(var y=origin.y;y<origin.y+World.Definition.ChunkSize;y+=6)
            {var x=World.Map.RiverAt(y);WaterPoint(key,parent,new Vector2(x-5.8f,y));WaterPoint(key,parent,new Vector2(x+5.8f,y));}
            foreach(var center in new[]{World.Map.PondCenter,World.Map.LakeCenter})
            {var radius=center==World.Map.PondCenter ? World.Map.PondRadius : World.Map.LakeRadius;for(var i=0;i<24;i++){var angle=i*Mathf.PI*2/24;var direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));WaterPoint(key,parent,center+Vector2.Scale(direction,radius)+direction*.8f);}}
        }
        private void WaterPoint(WorldChunk key,Transform parent,Vector2 position)
        {
            if(!WorldChunk.At(position,World.Definition.ChunkSize).Equals(key) || !World.Map.IsWalkable(position,.1f))return;var go=new GameObject("Water source");go.transform.SetParent(parent,false);go.transform.position=position;go.AddComponent<WaterSource>().Definition=World.Water;var collider=go.AddComponent<CircleCollider2D>();collider.isTrigger=true;collider.radius=.45f;
        }
    }
}
