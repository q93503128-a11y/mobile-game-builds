using UnityEngine;
namespace Wildfolk
{
    public enum ArmorSlot { Body, Head, Utility }
    [CreateAssetMenu(menuName="Wildfolk/Armor")]
    public sealed class ArmorDefinition : ScriptableObject
    {
        public ArmorSlot Slot;
        [Min(0)]public int Defense;
        [Min(0)]public float StaminaRecoveryBonus;
        public Sprite WornSprite,HandSprite;
        public EquipmentTier Tier;
    }
}
