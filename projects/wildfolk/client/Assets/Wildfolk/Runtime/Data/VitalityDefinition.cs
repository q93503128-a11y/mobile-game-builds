using UnityEngine;

namespace Wildfolk
{
    [CreateAssetMenu(menuName = "Wildfolk/Vitality")]
    public sealed class VitalityDefinition : ScriptableObject
    {
        [Min(1)] public int MaximumHealth;
    }
}
