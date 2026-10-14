using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Enemy Steering")]
    public sealed class EnemySteeringDefinition : ScriptableObject
    {
        [Min(.1f)] public float SeparationRadius=1.35f;
        [Range(0,2)] public float SeparationStrength=.8f;
        [Min(.1f)] public float Footprint=.45f;
    }
}
