using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public readonly struct ResourcePlacement
    {
        public readonly int Rule;public readonly Vector2 Position;public readonly float Scale;public readonly string Id;
        public ResourcePlacement(int rule,Vector2 position,float scale,string id=null){Rule=rule;Position=position;Scale=scale;Id=id;}
    }
    // Version dispatch is explicit. Only version-one saved worlds use the frozen layout reader.
    public sealed class WorldMap
    {
        public WorldGenerationDefinition Definition {get;}
        public int Seed {get;}
        public int GenerationVersion {get;}
        public ContinentalWorldLayout Continental {get;}
        private readonly LegacyWorldLayout legacy;
        public bool Streamed=>GenerationVersion>=2;
        public Vector2 Size=>Streamed ? Continental.Size : Definition.Size;
        public Bounds Bounds=>new Bounds(Vector3.zero,new Vector3(Size.x,Size.y,1));
        public Vector2 Spawn=>Vector2.zero;
        public Vector2 LakeCenter=>Streamed ? Continental.LakeCenter : legacy.LakeCenter;
        public Vector2 LakeRadius=>Streamed ? new Vector2(105,70) : legacy.LakeRadius;
        public Vector2 PondCenter=>Streamed ? new Vector2(-18,12) : legacy.PondCenter;
        public Vector2 PondRadius=>Streamed ? new Vector2(7,5) : legacy.PondRadius;
        public float RiverX=>Streamed ? 600 : legacy.RiverX;
        public float CurveOffset=>Streamed ? Continental.Phase : legacy.CurveOffset;
        public float FordY=>Streamed ? 0 : legacy.FordY;
        public const float FordHalfWidth=2.2f;
        public List<Vector2[]> Roads=>Streamed ? Continental.Roads : legacy.Roads;
        public List<ResourcePlacement> Resources=>Streamed ? Continental.GuaranteedResources : legacy.Resources;
        public WorldMap(WorldGenerationDefinition definition,int seed,int generationVersion=1,bool validateGeneration=true)
        {
            if(!WorldGenerationVersion.Valid(generationVersion))throw new ArgumentOutOfRangeException(nameof(generationVersion));
            Definition=definition;Seed=seed;GenerationVersion=generationVersion;
            if(Streamed)Continental=new ContinentalWorldLayout(definition,seed,validateGeneration);else legacy=new LegacyWorldLayout(definition,seed);
        }
        public bool Contains(Vector2 p)=>Streamed ? Continental.Contains(p) : Bounds.Contains(p);
        public Bounds RegionBounds(Vector2 p)=>Streamed && Continental.Underground(p) ? new Bounds(Continental.CaveAt(p).Origin,new Vector3(400,400,1)) : Bounds;
        public float RiverAt(float y)=>Streamed ? Continental.RiverAt(y) : legacy.RiverAt(y);
        public float RiverDistance(Vector2 p)=>Streamed ? Mathf.Abs(p.x-RiverAt(p.y)) : legacy.RiverDistance(p);
        public bool IsFord(Vector2 p)=>Streamed ? Continental.Ford(p) : legacy.IsFord(p);
        public Vector2 ShorePoint(Vector2 c,Vector2 r,float a)=>Streamed ? c+new Vector2(Mathf.Cos(a)*r.x,Mathf.Sin(a)*r.y) : legacy.ShorePoint(c,r,a);
        public float LakeDistance(Vector2 p,Vector2 c,Vector2 r)=>Streamed ? (new Vector2((p.x-c.x)/r.x,(p.y-c.y)/r.y).magnitude-1)*Mathf.Min(r.x,r.y) : legacy.LakeDistance(p,c,r);
        public float WaterDistance(Vector2 p)=>Streamed ? Continental.WaterDistance(p) : legacy.WaterDistance(p);
        public bool IsWater(Vector2 p,float margin=0)=>Streamed ? WaterDistance(p)<margin : legacy.IsWater(p,margin);
        public bool IsDeepWater(Vector2 p)=>Streamed ? Continental.DeepWater(p) : legacy.IsDeepWater(p);
        public bool IsWalkable(Vector2 p,float margin=.7f)=>Streamed ? Continental.Walkable(p,margin) : legacy.IsWalkable(p,margin);
        public float ForestDensity(Vector2 p)=>Streamed ? Continental.Environment(p).Forestation : legacy.ForestDensity(p);
        public float RockDensity(Vector2 p)=>Streamed ? Continental.Environment(p).Rockiness : legacy.RockDensity(p);
        public float RoadDistance(Vector2 p)=>Streamed ? Continental.RoadDistance(p) : legacy.RoadDistance(p);
        public bool IsRoad(Vector2 p)=>RoadDistance(p)<=Definition.RoadWidth;
        public bool ResourceSiteAllowed(int index,Vector2 p)=>Streamed ? Continental.SourceAllowed(index,p) : legacy.ResourceSiteAllowed(index,p);
        public float ResourceDensity(int index,Vector2 p)=>Streamed ? 1 : legacy.ResourceDensity(index,p);
        public static bool Supports(ResourceGenerationRule r,BiomeDefinition b,ResourceSourceContext context,float depth)=>LegacyWorldLayout.Supports(r,b,context,depth);
        public bool Renewable(int rule)=>!Streamed ? Definition.Resources[rule].NaturalRecovery : Continental.Renewable(rule);
        public int PopulationRegion(Vector2 p)=>Streamed ? Encode(WorldChunk.At(p,Definition.ChunkSize)) : 0;
        public static int Encode(WorldChunk k)=>(k.X+2048)*8192+k.Y+2048;
        public static WorldChunk Decode(int region)=>new WorldChunk(region/8192-2048,region%8192-2048);
        public bool ValidPopulationRegion(int region)=>Streamed ? region>=0 && Continental.ContainsChunk(Decode(region)) : region>=0 && region<12;
    }
}
