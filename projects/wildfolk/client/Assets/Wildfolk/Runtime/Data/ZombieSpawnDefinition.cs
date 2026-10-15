using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Zombie Spawn")]
    public sealed class ZombieSpawnDefinition : ScriptableObject
    {
        [Min(1)]public int InitialPopulation=6,PopulationStep=2,NightsPerStep=2,HardCap=18;
        [Range(0,1)]public float InitialFraction=.65f,RefillThreshold=.5f;
        [Min(.1f)]public float InitialInterval=.9f,RefillInterval=15,EmergeSeconds=1.2f,BurrowSeconds=1.1f;
        [Min(1)]public float PlayerDistance=12,SpawnNear=20,SpawnFar=32;
        [Min(0)]public float BuildingDistance=5;
        [Min(.1f)]public float FootprintRadius=.65f;
        [Min(.01f)]public float CameraMargin=.05f;
        [Min(1)]public int Attempts=48;
        [Min(1)]public float LocalThreatRadius=45,InactiveDistance=60,InactiveSeconds=20;
        [Min(1)]public float DetectionRadius=36,LoseRange=50,LostTargetSeconds=15;
        [Range(.1f,1)]public float DimDetectionMultiplier=.8f,SafeSpeedMultiplier=.75f;
        [Range(0,1)]public float SecondarySpeciesChance=.35f,SwordVariantChance=.25f;
        public int PopulationFor(int night)=>Mathf.Min(HardCap,InitialPopulation+Mathf.Max(0,(night-1)/NightsPerStep)*PopulationStep);
    }
}
