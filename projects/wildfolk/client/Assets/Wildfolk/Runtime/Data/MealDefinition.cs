using UnityEngine;
namespace Wildfolk
{
    public enum MealEffect { MaximumHealth,NaturalRecovery,StaminaRecovery }
    [CreateAssetMenu(menuName="Wildfolk/Meal")]
    public sealed class MealDefinition : ScriptableObject
    {
        public MealEffect Effect;
        [Min(0)]public float Amount;
        [Min(0)]public float Duration;
    }
}
