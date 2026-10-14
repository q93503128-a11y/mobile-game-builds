using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Consumable")]
    public sealed class ConsumableDefinition : ScriptableObject
    {
        [Min(0)] public float HungerRestore;
        [Min(0)] public float ThirstRestore;
        [Min(0)] public float RecoveryBlockSeconds;
        [Min(0)] public float DrainPenaltySeconds;
        [Min(1)] public float ThirstDrainMultiplier=1;
    }
}
