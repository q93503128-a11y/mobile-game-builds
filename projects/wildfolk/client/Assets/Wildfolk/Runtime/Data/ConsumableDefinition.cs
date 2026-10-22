using UnityEngine;
namespace Wildfolk
{
    public enum ConsumableKind { Food,Medicine }
    [CreateAssetMenu(menuName="Wildfolk/Consumable")]
    public sealed class ConsumableDefinition : ScriptableObject
    {
        public ConsumableKind Kind;[Min(0)]public float ApplicationSeconds;
        public MealDefinition Meal;
        [Min(0)] public int HealthRestore;
        [Min(0)] public int GradualHealthRestore;
        [Min(0)] public float HealingDuration;
        [Min(0)] public float StaminaRestore;
        [Min(0)] public float UseCooldownSeconds;
        [Min(0)] public float HungerRestore;
        [Min(0)] public float ThirstRestore;
        [Min(0)] public float RecoveryBlockSeconds;
        [Min(0)] public float DrainPenaltySeconds;
        [Min(1)] public float ThirstDrainMultiplier=1;
    }
}
