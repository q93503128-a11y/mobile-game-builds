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
        public int PopulationFor(int night)=>Mathf.Min(HardCap,InitialPopulation+Mathf.Max(0,(night-1)/NightsPerStep)*PopulationStep);
    }
}
