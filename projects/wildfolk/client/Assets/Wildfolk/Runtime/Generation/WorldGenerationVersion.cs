using System;
using UnityEngine;
namespace Wildfolk
{
    public static class WorldGenerationVersion
    {
        public const int Legacy=1,Continental=2,Current=3;
        public static bool Valid(int version)=>version>=Legacy && version<=Current;
    }
    public enum SeedDomain { Continental=101,Terrain=211,Hydrology=307,Biome=401,Cave=503,Resource=601,POI=701,Road=809,Vegetation=907,Creature=1009 }
    public static class WorldSeeds
    {
        public static int StableText(string text){unchecked{uint hash=2166136261;foreach(var c in text)hash=(hash^c)*16777619;return (int)hash;}}
        public static int Derive(int root,SeedDomain domain,int x=0,int y=0,int layer=0)
        {unchecked{uint h=(uint)root^2166136261u;h=(h^(uint)domain)*16777619;h=(h^(uint)x)*16777619;h=(h^(uint)y)*16777619;h=(h^(uint)layer)*16777619;h^=h>>16;h*=2246822519u;h^=h>>13;return (int)(h^(h>>16));}}
        public static float Unit(int root,SeedDomain domain,int x=0,int y=0,int layer=0)=>(uint)Derive(root,domain,x,y,layer)/(float)uint.MaxValue;
        public static float Field(int seed,SeedDomain domain,Vector2 p,float scale)
        {var h=Derive(seed,domain);return Mathf.PerlinNoise((h&65535)*.017f+p.x/scale,((uint)h>>16)*.017f+p.y/scale);}
    }
    [Serializable]public readonly struct WorldChunk : IEquatable<WorldChunk>
    {
        public readonly int X,Y;
        public WorldChunk(int x,int y){X=x;Y=y;}
        public static WorldChunk At(Vector2 p,int size)=>new WorldChunk(Mathf.FloorToInt(p.x/size),Mathf.FloorToInt(p.y/size));
        public Vector2 Origin(int size)=>new Vector2(X*size,Y*size);
        public Vector2 Center(int size)=>Origin(size)+Vector2.one*size*.5f;
        public bool Equals(WorldChunk other)=>X==other.X && Y==other.Y;
        public override bool Equals(object obj)=>obj is WorldChunk other && Equals(other);
        public override int GetHashCode()=>unchecked(X*397^Y);
        public override string ToString()=>X+":"+Y;
    }
}
