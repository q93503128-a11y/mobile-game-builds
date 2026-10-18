using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed class LegacyWorldLayout
    {
        public WorldGenerationDefinition Definition { get; }
        public int Seed { get; }
        public Bounds Bounds => new Bounds(Vector3.zero,new Vector3(Definition.Size.x,Definition.Size.y,1));
        public Vector2 Spawn => Vector2.zero;
        public Vector2 LakeCenter { get; }
        public Vector2 LakeRadius { get; }
        public Vector2 PondCenter { get; }
        public Vector2 PondRadius => new Vector2(3.5f,2.7f);
        public float RiverX,CurveOffset,FordY;
        public readonly List<Vector2[]> Roads=new List<Vector2[]>();
        public readonly List<ResourcePlacement> Resources=new List<ResourcePlacement>();
        private readonly float noiseX,noiseY;
        public LegacyWorldLayout(WorldGenerationDefinition definition,int seed)
        {
            Definition=definition;Seed=seed;var random=new System.Random(seed);
            noiseX=(float)random.NextDouble()*1000;noiseY=(float)random.NextDouble()*1000;
            RiverX=definition.Size.x*(.15f+(float)random.NextDouble()*.07f);CurveOffset=(float)random.NextDouble()*6;
            FordY=-definition.Size.y*.19f;
            LakeCenter=new Vector2(-definition.Size.x*(.22f+(float)random.NextDouble()*.07f),definition.Size.y*.2f);
            LakeRadius=new Vector2(10+(float)random.NextDouble()*3,7+(float)random.NextDouble()*2);
            PondCenter=new Vector2(-9+(float)random.NextDouble()*2,8+(float)random.NextDouble()*1.5f);
            Roads.Add(new[]{new Vector2(-definition.Size.x*.48f,FordY-7),new Vector2(-24,FordY+3),new Vector2(0,-8),new Vector2(RiverAt(FordY),FordY),new Vector2(definition.Size.x*.48f,FordY-10)});
            Roads.Add(new[]{new Vector2(-definition.Size.x*.35f,-definition.Size.y*.48f),new Vector2(-22,-10),new Vector2(-16,12),new Vector2(-24,definition.Size.y*.48f)});
            GenerateResources(random);
        }
        public float RiverAt(float y)=>RiverX+Mathf.Sin(y*.045f+CurveOffset)*8+Mathf.Sin(y*.11f+CurveOffset)*2;
        public float RiverDistance(Vector2 p)=>Mathf.Abs(p.x-RiverAt(p.y));
        public const float FordHalfWidth=2.2f;
        public bool IsFord(Vector2 p)=>Mathf.Abs(p.y-FordY)<FordHalfWidth;
        private float ShoreFactor(float angle)=>1+Mathf.Sin(angle*3+CurveOffset)*.08f+Mathf.Sin(angle*5-CurveOffset)*.045f;
        public Vector2 ShorePoint(Vector2 center,Vector2 radius,float angle)=>center+new Vector2(Mathf.Cos(angle)*radius.x,Mathf.Sin(angle)*radius.y)*ShoreFactor(angle);
        public float LakeDistance(Vector2 p,Vector2 center,Vector2 radius)
        {
            var v=p-center;var outside=Mathf.Max(Mathf.Abs(v.x)-radius.x*1.14f,Mathf.Abs(v.y)-radius.y*1.14f);
            if(outside>3)return outside;
            var normalized=new Vector2(v.x/radius.x,v.y/radius.y);return (normalized.magnitude-ShoreFactor(Mathf.Atan2(normalized.y,normalized.x)))*Mathf.Min(radius.x,radius.y);
        }
        public float WaterDistance(Vector2 p)=>Mathf.Min(RiverDistance(p)-Definition.RiverWidth,Mathf.Min(LakeDistance(p,LakeCenter,LakeRadius),LakeDistance(p,PondCenter,PondRadius)));
        public bool IsWater(Vector2 p,float margin=0)=>WaterDistance(p)<margin;
        public bool IsDeepWater(Vector2 p)=>RiverDistance(p)<Definition.RiverWidth && !IsFord(p) || LakeDistance(p,LakeCenter,LakeRadius)<0 || LakeDistance(p,PondCenter,PondRadius)<0;
        public bool IsWalkable(Vector2 p,float margin=.7f)=>Bounds.Contains(p-Vector2.one*margin) && Bounds.Contains(p+Vector2.one*margin) && !IsDeepWater(p) && (!IsWater(p,margin) || IsFord(p) && RiverDistance(p)<Definition.RiverWidth+margin);
        public float ForestDensity(Vector2 p)=>Mathf.PerlinNoise(noiseX+p.x*Definition.Biome.DensityFrequency,noiseY+p.y*Definition.Biome.DensityFrequency);
        public float RockDensity(Vector2 p)=>Mathf.PerlinNoise(noiseX+370+p.x*.045f,noiseY+480+p.y*.045f);
        public float RoadDistance(Vector2 p)
        {
            var best=float.MaxValue;
            foreach(var road in Roads)for(var i=1;i<road.Length;i++)
            {var a=road[i-1];var d=road[i]-a;var t=Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude);best=Mathf.Min(best,Vector2.Distance(p,a+d*t));}
            return best;
        }
        public bool IsRoad(Vector2 p)=>RoadDistance(p)<=Definition.RoadWidth;
        private void GenerateResources(System.Random random)
        {
            for(var pass=0;pass<2;pass++)for(var r=0;r<Definition.Resources.Length;r++)
            {
                var rule=Definition.Resources[r];if(rule.IndependentGeneration)continue;var count=0;var attempts=0;var required=pass==0 ? rule.StartCount : rule.Count-rule.StartCount;
                while(count<required && attempts++<Mathf.Max(1,required)*250)
                {
                    var near=pass==0;
                    var p=near ? new Vector2((float)random.NextDouble()*2-1,(float)random.NextDouble()*2-1)*rule.StartRadius :
                        new Vector2(((float)random.NextDouble()-.5f)*(Definition.Size.x-6),((float)random.NextDouble()-.5f)*(Definition.Size.y-6));
                    if(p.magnitude<rule.SpawnClearance || !IsWalkable(p,1.4f) || IsWater(p,1.4f) || IsRoad(p))continue;
                    if(!near)
                    {
                        var density=rule.ForestPreference*ForestDensity(p)+rule.RockPreference*RockDensity(p)+(1-rule.ForestPreference-rule.RockPreference)*.6f;
                        if(random.NextDouble()>Mathf.Clamp01(density))continue;
                    }
                    var clear=true;
                    foreach(var placed in Resources)
                    {var gap=Mathf.Max(rule.Spacing,Definition.Resources[placed.Rule].Spacing);if(Vector2.Distance(placed.Position,p)<gap){clear=false;break;}}
                    if(!clear)continue;
                    Resources.Add(new ResourcePlacement(r,p,Mathf.Lerp(rule.ScaleRange.x,rule.ScaleRange.y,(float)random.NextDouble())));count++;
                }
                if(count<required)throw new InvalidOperationException("World generation could not place required resources.");
            }
            for(var r=0;r<Definition.Resources.Length;r++)
            {
                var rule=Definition.Resources[r];if(!rule.IndependentGeneration || !Supports(rule,Definition.Biome,ResourceSourceContext.Surface,0))continue;
                var stream=new System.Random(unchecked(Seed*397 ^ rule.GenerationSalt));var count=0;var attempts=0;var anchor=Vector2.zero;
                while(count<rule.Count && attempts++<Mathf.Max(1,rule.Count)*1000)
                {
                    var clustered=rule.ClusterSize>1 && count%rule.ClusterSize!=0 && anchor!=Vector2.zero;
                    var p=clustered ? anchor+new Vector2((float)stream.NextDouble()*2-1,(float)stream.NextDouble()*2-1)*rule.ClusterRadius :
                        new Vector2(((float)stream.NextDouble()-.5f)*(Definition.Size.x-6),((float)stream.NextDouble()-.5f)*(Definition.Size.y-6));
                    if(!ResourceSiteAllowed(r,p) || stream.NextDouble()>ResourceDensity(r,p))continue;
                    var clear=true;foreach(var placed in Resources)if(Vector2.Distance(placed.Position,p)<Mathf.Max(rule.Spacing,Definition.Resources[placed.Rule].Spacing)){clear=false;break;}
                    if(!clear)continue;
                    Resources.Add(new ResourcePlacement(r,p,Mathf.Lerp(rule.ScaleRange.x,rule.ScaleRange.y,(float)stream.NextDouble())));
                    if(!clustered)anchor=p;count++;
                }
                if(count<rule.Count)throw new InvalidOperationException("World generation could not place configured resource rule "+r);
            }
        }
        public static bool Supports(ResourceGenerationRule rule,BiomeDefinition biome,ResourceSourceContext context,float depth)
        {
            if(rule.SourceContext!=context)return false;
            if(rule.AllowedBiomes!=null && rule.AllowedBiomes.Length>0 && Array.IndexOf(rule.AllowedBiomes,biome)<0)return false;
            return !rule.DistributionConfigured || depth>=rule.DepthRange.x && depth<=rule.DepthRange.y;
        }
        public bool ResourceSiteAllowed(int index,Vector2 p)
        {
            var rule=Definition.Resources[index];
            if(!Supports(rule,Definition.Biome,ResourceSourceContext.Surface,0) || p.magnitude<rule.SpawnClearance || !IsWalkable(p,1.4f) || IsWater(p,1.4f) || IsRoad(p))return false;
            if(!rule.DistributionConfigured)return true;
            var water=WaterDistance(p);var road=RoadDistance(p);var forest=ForestDensity(p);var rock=RockDensity(p);
            return Within(water,rule.WaterDistanceRange) && Within(road,rule.RoadDistanceRange) && Within(forest,rule.ForestDensityRange) && Within(rock,rule.RockDensityRange) && (rule.Terrain!=ResourceTerrain.Shore || water<=12);
        }
        public float ResourceDensity(int index,Vector2 p)
        {var r=Definition.Resources[index];var density=r.ForestPreference*ForestDensity(p)+r.RockPreference*RockDensity(p)+(1-r.ForestPreference-r.RockPreference)*.6f;return Mathf.Clamp01(density*(r.DistributionConfigured ? r.BiomeAffinity : 1));}
        private static bool Within(float value,Vector2 range)=>value>=range.x && value<=range.y;
    }
}
