using System;
using UnityEngine;
namespace Wildfolk
{
    public enum CreatureFamily { Animal,Undead,Humanoid }
    public enum CreatureFaction { Player,Wildlife,Goblin,Undead }
    [Flags]public enum CreatureTrait { None=0,LightSensitive=1,Nocturnal=2,Herding=4,PackAggro=8,AllyAssist=16,Territorial=32,Ranged=64,Scout=128,CampBound=256,FleeTogether=512 }
    [Flags]public enum CreatureHabitat { None=0,Grassland=1,Forest=2,ForestEdge=4,Roadside=8,Camp=16,Ruin=32,NightSurface=64 }
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
        public LightSensitivity Light=new LightSensitivity();
        [Min(.1f)]public float SeparationRadius=.5f,PreferredSpacing=1.8f,AlertRadius=7,AlertMaxDistance=12,RoamRadius=8;
        [Range(1,8)]public int MinimumGroup=1,MaximumGroup=3;
        public bool Has(CreatureTrait trait)=>(Traits&trait)!=0;
        public bool Matches(WorldMap map,Vector2 p)
        {
            if(!map.IsWalkable(p,.8f) || map.IsWater(p,1) || map.IsRoad(p))return false;
            var density=map.ForestDensity(p);
            return (Habitat&CreatureHabitat.Grassland)!=0 && density<.52f || (Habitat&CreatureHabitat.Forest)!=0 && density>=.6f || (Habitat&CreatureHabitat.ForestEdge)!=0 && density>=.36f && density<.66f;
        }
    }
}
