using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Zombie Spawn")]
    public sealed class ZombieSpawnDefinition : ScriptableObject
    {
        [Min(1)] public int DayLimit=1;
        [Min(1)] public int NightLimit=6;
        [Min(1)] public float DayInterval=90;
        [Min(1)] public float NightInterval=15;
        [Min(1)] public float PlayerDistance=12;
        [Min(0)] public float BuildingDistance=5;
        [Min(.1f)] public float FootprintRadius=.65f;
        [Min(.01f)] public float CameraMargin=.05f;
        [Min(1)] public int Attempts=32;
    }
}
