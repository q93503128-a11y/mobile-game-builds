using System;
using UnityEngine;
namespace Wildfolk
{
    public enum ResourceFamily { Timber, Stone, Mineral, Plant }
    public enum ResourceRarity { Common, Uncommon, Rare }
    public enum ResourceSourceContext { Surface, Cave, PointOfInterest }
    public enum ResourceTerrain { Land, Shore }
    [Serializable] public struct ResourceGenerationRule
    {
        // Appended generation streams preserve all legacy r<index> placements and random draws.
        public bool IndependentGeneration,DistributionConfigured;
        public bool Generation2Only;
        // Immutable generation2 baseline counts; current yields and recipe costs must not move saved IDs.
        [Min(0)]public int LayoutSourceCount,LayoutSupplementCount;
        public int GenerationSalt;
        public BiomeDefinition[] AllowedBiomes;
        public ResourceSourceContext SourceContext;
        public ResourceTerrain Terrain;
        public Vector2 DepthRange,WaterDistanceRange,RoadDistanceRange;
        public Vector2 ForestDensityRange,RockDensityRange;
        [Range(0,1)]public float BiomeAffinity;
        [Min(0)]public float ClusterRadius;
        [Min(1)]public int ClusterSize;
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
        public SurfaceEnvironmentDefinition[] SurfaceEnvironments=Array.Empty<SurfaceEnvironmentDefinition>();
        public NaturalSourceDefinition[] NaturalSources=Array.Empty<NaturalSourceDefinition>();
        public ContentCatalog ProgressionCatalog;
        public PoiContentDefinition PoiContent;
        public float WalkingSpeed=4.2f,TravelMinutesAcross=52,PoiTravelMinutes=2.5f,ProgressionSafetyFactor=1.5f;
        public int ChunkSize=64,ChunkTexturePixels=128;
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
