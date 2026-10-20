using System;
using UnityEngine;
namespace Wildfolk
{
    public enum CreatureFamily { Animal,Undead,Humanoid,Invertebrate }
    public enum CreatureFaction { Player,Wildlife,Goblin,Undead }
    [Flags]public enum CreatureTrait { None=0,LightSensitive=1,Nocturnal=2,Herding=4,PackAggro=8,AllyAssist=16,Territorial=32,Ranged=64,Scout=128,CampBound=256,FleeTogether=512,Flying=1024,Camouflage=2048 }
    [Flags]public enum CreatureHabitat { None=0,Grassland=1,Forest=2,ForestEdge=4,Roadside=8,Camp=16,Ruin=32,NightSurface=64,Rocky=128,Wetlands=256,Shore=512,Caverns=1024,DeepCaverns=2048,CrystalPocket=4096 }
    [Serializable]public sealed class LightSensitivity
    {
        [Range(.1f,1)]public float DimDetection=.8f,SafeMovement=.75f,DimMovement=.9f;
        public bool SuppressSafeAggro=true;
        [Min(0)]public float ChaseGraceMultiplier=1;
    }
    [CreateAssetMenu(menuName="Wildfolk/Creature")]
    public sealed class CreatureDefinition : ScriptableObject
    {
        public string Id;
        public CreatureFamily Family;
        public CreatureFaction Faction;
        public CreatureTrait Traits;
        public CreatureHabitat Habitat;
        public WildlifeActionDefinition Actions;
        public LightSensitivity Light=new LightSensitivity();
        [Min(.1f)]public float SeparationRadius=.5f,PreferredSpacing=1.8f,AlertRadius=7,AlertMaxDistance=12,RoamRadius=8;
        [Range(1,8)]public int MinimumGroup=1,MaximumGroup=3;
        public bool Has(CreatureTrait trait)=>(Traits&trait)!=0;
        public bool Matches(WorldMap map,Vector2 p)
        {
            if(!map.IsWalkable(p,SeparationRadius+.15f) || map.IsDeepWater(p) || map.IsRoad(p))return false;
            if(map.Streamed)
            {
                var e=map.Continental.Environment(p);
                if(map.Continental.Underground(p))
                {var room=map.Continental.CaveAt(p).Nearest(p);return (Habitat&CreatureHabitat.Caverns)!=0 || (Habitat&CreatureHabitat.DeepCaverns)!=0 && room.Depth>=.6f || (Habitat&CreatureHabitat.CrystalPocket)!=0 && (e.Tags&EnvironmentTag.CrystalPocket)!=0;}
                if((Habitat&CreatureHabitat.Shore)!=0)return e.Biome==SurfaceBiome.Wetlands && e.WaterDistance>=.2f && e.WaterDistance<=4;
                if(map.IsWater(p,1) || e.WaterDistance<2)return false;
                if((Habitat&CreatureHabitat.Rocky)!=0 && e.Biome==SurfaceBiome.Rocky)return true;
                if((Habitat&CreatureHabitat.Wetlands)!=0 && e.Biome==SurfaceBiome.Wetlands)return true;
                if((Habitat&CreatureHabitat.ForestEdge)!=0 && (Habitat&CreatureHabitat.Rocky)!=0)return e.Biome==SurfaceBiome.Forest && e.Rockiness>=.45f;
                if(e.Rockiness>.9f)return false;
            }
            else if(map.IsWater(p,1))return false;
            var density=map.ForestDensity(p);
            return (Habitat&CreatureHabitat.Grassland)!=0 && density<.52f || (Habitat&CreatureHabitat.Forest)!=0 && density>=.6f || (Habitat&CreatureHabitat.ForestEdge)!=0 && density>=.36f && density<.66f;
        }
    }
}
