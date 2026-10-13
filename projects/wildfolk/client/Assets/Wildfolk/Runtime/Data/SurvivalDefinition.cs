using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Survival")]
    public sealed class SurvivalDefinition : ScriptableObject
    {
        [Min(1)] public float MaximumHunger = 100;
        [Min(1)] public float MaximumThirst = 100;
        [Min(0)] public float HungerPerSecond = .08f;
        [Min(0)] public float ThirstPerSecond = .12f;
        [Min(.1f)] public float DamageInterval = 5;
        [Min(1)] public int EmptyDamage = 2;
    }
}
