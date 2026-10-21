using System;
using UnityEngine;
namespace Wildfolk
{
    [Flags] public enum FishingWater { Inland=1,Wetlands=2,Ocean=4,Cavern=8 }
    public enum FishingTime { Any,Daylight,Night }
    public readonly struct FishingWaterContext
    {
        public readonly FishingWater Water;public readonly bool Night,Deep;public readonly float ShoreInset;
        public FishingWaterContext(FishingWater water,bool night,bool deep,float inset){Water=water;Night=night;Deep=deep;ShoreInset=inset;}
        public static bool TrySample(WorldMap map,Vector2 point,bool night,out FishingWaterContext context)
        {
            context=default;if(map==null || !map.Contains(point) || !map.IsWater(point))return false;
            var cave=map.Streamed && map.Continental.Underground(point);
            var water=cave ? FishingWater.Cavern : map.Streamed && map.Continental.CoastDistance(point)<0 ? FishingWater.Ocean : map.Streamed && map.Continental.Environment(point).Biome==SurfaceBiome.Wetlands ? FishingWater.Wetlands : FishingWater.Inland;
            context=new FishingWaterContext(water,night,!cave && map.IsDeepWater(point),cave ? 0 : -map.WaterDistance(point));return true;
        }
        // Cast into one continuous water span; never jump across an island or opposite bank.
        public static bool TryCast(WorldMap map,Vector2 origin,Vector2 direction,out Vector2 point)
        {
            point=default;if(map==null || direction.sqrMagnitude<.01f || !map.IsWalkable(origin,.1f))return false;
            direction.Normalize();var found=false;
            for(var d=1.5f;d<=5;d+=.5f){var p=origin+direction*d;var water=map.Contains(p) && map.IsWater(p);if(!water){if(found)break;continue;}point=p;found=true;}return found;
        }
    }
    [Serializable] public sealed class FishingCatchDefinition
    {
        public string Id,NameKey;public FishingWater Waters;public FishingTime Time;
        [Min(0)]public float MinimumShoreInset;
        [Min(.001f)]public float Weight=1;
        public ItemDefinition Output;[Min(1)]public int Amount=1;
        public bool Matches(FishingWaterContext c)=>(Waters&c.Water)!=0 && (Time==FishingTime.Any || (Time==FishingTime.Night)==c.Night) && (MinimumShoreInset<=0 || c.Deep && c.ShoreInset>=MinimumShoreInset);
    }
    [CreateAssetMenu(menuName="Wildfolk/Fishing Loot Table")]
    public sealed class FishingLootTable:ScriptableObject
    {
        public FishingCatchDefinition[] Entries=Array.Empty<FishingCatchDefinition>();
        public int Roll(FishingWaterContext context,float sample)
        {
            var total=0f;foreach(var e in Entries)if(e.Matches(context))total+=e.Weight;
            if(total<=0 || float.IsNaN(sample))return -1;var roll=Mathf.Clamp01(sample)*total;var last=-1;
            for(var i=0;i<Entries.Length;i++){var e=Entries[i];if(!e.Matches(context))continue;last=i;roll-=e.Weight;if(roll<0)return i;}return last;
        }
    }
}
