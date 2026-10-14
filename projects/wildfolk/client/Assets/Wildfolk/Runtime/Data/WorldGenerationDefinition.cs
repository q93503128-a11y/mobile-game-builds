using System;
using UnityEngine;
namespace Wildfolk
{
    public enum ResourceFamily { Timber, Stone, Mineral, Plant }
    public enum ResourceRarity { Common, Uncommon, Rare }
    [Serializable] public struct ResourceGenerationRule
    {
        public ResourceNode Prefab;
        public Sprite AlternateSprite;
        [Min(1)]public int Count;
        [Min(0)]public int StartCount;
        [Min(1)]public float StartRadius,SpawnClearance,Spacing;
        [Range(0,1)]public float ForestPreference,RockPreference;
        public Vector2 ScaleRange;
        [Min(1)] public float RespawnSeconds;
        public bool RecoveryConfigured,NaturalRecovery,WorldRecoveryScope;
        public ResourceFamily Family;
        public ResourceRarity Rarity;
        [Range(.1f,1.2f)]public float TargetDensityRatio;
        [Min(0)]public float HarvestExclusionRadius,HarvestExclusionSeconds,ClusterSpacing;
    }
    [CreateAssetMenu(menuName="Wildfolk/World Generation")]
    public sealed class WorldGenerationDefinition : ScriptableObject
    {
        public BiomeDefinition Biome;
        public int DefaultSeed=9472026;
        public Vector2 Size=new Vector2(192,144);
        [Min(1)]public float RiverWidth=3;
        [Min(1)]public float RoadWidth=2.1f;
        [Min(64)]public int TextureWidth=1024;
        public ResourceGenerationRule[] Resources=Array.Empty<ResourceGenerationRule>();
        [Min(0)]public int DecorationCount=240;
        [Range(.1f,1)]public float ShallowMoveMultiplier=.7f;
    }
}
