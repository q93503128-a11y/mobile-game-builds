using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
namespace Wildfolk
{
    public sealed partial class WorldTerrainRenderer
    {
        private sealed class TerrainTile
        {
            public Texture2D Texture;public Sprite Sprite;public SpriteRenderer Renderer;
            public WorldChunk Key;public WorldMap Map;public int Row;public Color32[] Colors;
        }
        public const int InactiveCacheCapacity=24;
        private readonly Dictionary<WorldChunk,TerrainTile> tiles=new Dictionary<WorldChunk,TerrainTile>();
        private readonly Dictionary<WorldChunk,TerrainTile> cache=new Dictionary<WorldChunk,TerrainTile>();
        private readonly LinkedList<WorldChunk> lru=new LinkedList<WorldChunk>();
        private readonly LinkedList<TerrainTile> work=new LinkedList<TerrainTile>();
        public int TerrainTileCount=>tiles.Count;
        public int CachedTileCount=>cache.Count;
        public int TerrainWorkCount=>work.Count;
        public int CacheHits {get;private set;}
        public int GeneratedTiles {get;private set;}
        public double LastTerrainSliceMs {get;private set;}
        public double MaximumTerrainSliceMs {get;private set;}
        public long TerrainTextureBytes
        {get{long count=0;foreach(var t in tiles.Values)if(t.Texture!=null)count+=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t.Texture);foreach(var t in cache.Values)count+=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t.Texture);return count;}}
        public void BuildChunk(WorldMap map,WorldChunk chunk,Transform parent)
        {
            if(tiles.ContainsKey(chunk))return;
            TerrainTile tile;
            if(cache.TryGetValue(chunk,out tile)){cache.Remove(chunk);lru.Remove(chunk);CacheHits++;}
            else tile=new TerrainTile{Map=map,Key=chunk};
            var go=new GameObject("Terrain");go.transform.SetParent(parent,false);go.transform.position=chunk.Origin(map.Definition.ChunkSize);
            var sr=go.AddComponent<SpriteRenderer>();sr.sharedMaterial=Ground.sharedMaterial;sr.sortingOrder=WorldSorting.Ground;sr.sprite=tile.Sprite;tile.Renderer=sr;tiles.Add(chunk,tile);
            if(tile.Texture==null)work.AddLast(tile);
        }
        // Sample whole rows under a frame budget; upload only one small texture when its rows finish.
        public void ProcessTerrainWork(double budgetMilliseconds)
        {
            var watch=Stopwatch.StartNew();
            while(work.First!=null)
            {
                var t=work.First.Value;var pixels=t.Map.Definition.ChunkTexturePixels;var edge=pixels+2;
                if(t.Colors==null)t.Colors=new Color32[edge*edge];
                var origin=t.Key.Origin(t.Map.Definition.ChunkSize);var step=(float)t.Map.Definition.ChunkSize/pixels;
                for(var x=0;x<edge;x++)t.Colors[t.Row*edge+x]=SampleContinental(t.Map,origin+new Vector2(x-.5f,t.Row-.5f)*step);
                t.Row++;
                if(t.Row==edge)
                {
                    t.Texture=new Texture2D(edge,edge,TextureFormat.RGB24,false){name="Local landscape",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
                    t.Texture.SetPixels32(t.Colors);t.Texture.Apply(false,true);t.Colors=null;
                    t.Sprite=Sprite.Create(t.Texture,new Rect(1,1,pixels,pixels),Vector2.zero,1/step);
                    if(t.Renderer!=null)t.Renderer.sprite=t.Sprite;work.RemoveFirst();GeneratedTiles++;
                }
                if(watch.Elapsed.TotalMilliseconds>=budgetMilliseconds)break;
            }
            watch.Stop();LastTerrainSliceMs=watch.Elapsed.TotalMilliseconds;if(!double.IsPositiveInfinity(budgetMilliseconds))MaximumTerrainSliceMs=System.Math.Max(MaximumTerrainSliceMs,LastTerrainSliceMs);
        }
        public void ReleaseChunk(WorldChunk key)
        {
            if(!tiles.TryGetValue(key,out var tile))return;tiles.Remove(key);tile.Renderer=null;
            if(tile.Texture==null){work.Remove(tile);tile.Colors=null;return;}
            cache.Add(key,tile);lru.AddLast(key);
            while(cache.Count>InactiveCacheCapacity){var oldest=lru.First.Value;lru.RemoveFirst();var old=cache[oldest];Destroy(old.Sprite);Destroy(old.Texture);cache.Remove(oldest);}
        }
        private void ClearChunks()
        {
            foreach(var t in tiles.Values){if(t.Sprite!=null)Destroy(t.Sprite);if(t.Texture!=null)Destroy(t.Texture);}
            foreach(var t in cache.Values){Destroy(t.Sprite);Destroy(t.Texture);}tiles.Clear();cache.Clear();lru.Clear();work.Clear();CacheHits=GeneratedTiles=0;LastTerrainSliceMs=MaximumTerrainSliceMs=0;
        }
        public static Color SampleContinental(WorldMap map,Vector2 p)
        {
            var layout=map.Continental;var e=layout.Environment(p);var grain=WorldSeeds.Field(map.Seed,SeedDomain.Vegetation,p,1.8f);
            if(layout.Underground(p))
            {var c=layout.CaveAt(p);var distance=c.FloorDistance(p);var floor=Color.Lerp(new Color(.31f,.32f,.28f),new Color(.19f,.25f,.25f),c.Nearest(p).Depth);floor*=Mathf.Lerp(.8f,1.08f,grain);var wall=Color.Lerp(new Color(.075f,.095f,.09f),new Color(.16f,.18f,.16f),grain);return c.Water(p) ? new Color(.11f,.23f,.25f) : Color.Lerp(floor,wall,Mathf.SmoothStep(0,1,Mathf.InverseLerp(-1,.8f,distance)));}
            var color=Color.black;foreach(var profile in map.Definition.SurfaceEnvironments){var v=e.Weights[(int)profile.Biome];color+=Color.Lerp(profile.Ground,profile.DenseGround,e.Forestation)*v;}color*=Mathf.Lerp(.88f,1.08f,grain);
            if((e.Tags&EnvironmentTag.RockFace)!=0){var stratum=Mathf.SmoothStep(.8f,1,Mathf.Abs(Mathf.Sin(p.x*.09f+p.y*.035f+grain*.3f)));color=Color.Lerp(color,new Color(.27f,.3f,.27f),stratum*e.Rockiness*.55f);}
            var road=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.2f,3.2f,layout.RoadDistance(p)));color=Color.Lerp(color,new Color(.43f,.38f,.27f),road*.8f);
            var distanceWater=e.WaterDistance;var coast=layout.CoastDistance(p);
            if(coast<18)color=Color.Lerp(color,new Color(.57f,.53f,.39f),1-Mathf.Clamp01(coast/18));
            var bank=1-Mathf.Clamp01((distanceWater-1)/3);color=Color.Lerp(color,new Color(.35f,.37f,.27f),bank*.6f);
            if(distanceWater<1.4f){var water=coast< -10 ? new Color(.085f,.2f,.26f) : layout.DeepWater(p) ? new Color(.19f,.36f,.37f) : new Color(.33f,.46f,.41f);color=Color.Lerp(color,water,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.6f,1.4f,distanceWater)));}return color;
        }
    }
}
