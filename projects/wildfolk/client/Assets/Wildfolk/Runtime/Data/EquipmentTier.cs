using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Equipment Tier")]
    public sealed class EquipmentTier : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        [Min(1)] public int Rank;
    }
}
