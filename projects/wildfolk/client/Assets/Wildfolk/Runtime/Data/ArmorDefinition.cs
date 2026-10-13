using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Armor")]
    public sealed class ArmorDefinition : ScriptableObject
    {
        [Min(0)] public int Defense;
        public Sprite WornSprite;
        public EquipmentTier Tier;
    }
}
