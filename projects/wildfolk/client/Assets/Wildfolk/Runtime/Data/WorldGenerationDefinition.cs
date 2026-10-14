using System;
using UnityEngine;
namespace Wildfolk
{
    [Serializable] public struct ResourceGenerationRule
    {
        public ResourceNode Prefab;
        public Sprite AlternateSprite;
        [Min(1)]public int Count;
        [Min(0)]public int StartCount;
        [Min(1)]public float StartRadius,SpawnClearance,Spacing;
        [Range(0,1)]public float ForestPreference,RockPreference;
        public Vector2 ScaleRange;
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
    }
}
