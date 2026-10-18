using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public static class ChunkActivation
    {
        public const float RetentionMargin=4;
        public static bool Retains(WorldChunk key,Vector2 center,int size)
        {var delta=key.Center(size)-center;return Mathf.Abs(delta.x)<=size*1.5f+RetentionMargin && Mathf.Abs(delta.y)<=size*1.5f+RetentionMargin;}
        public static void Build(WorldMap map,IReadOnlyList<Vector2> centers,IEnumerable<WorldChunk> previous,HashSet<WorldChunk> result)
        {
            result.Clear();var size=map.Definition.ChunkSize;
            foreach(var center in centers){var key=WorldChunk.At(center,size);for(var y=-1;y<=1;y++)for(var x=-1;x<=1;x++){var n=new WorldChunk(key.X+x,key.Y+y);if(map.Continental.ContainsChunk(n))result.Add(n);}}
            foreach(var key in previous)foreach(var center in centers)if(Retains(key,center,size)){result.Add(key);break;}
        }
    }
}
