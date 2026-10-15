using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Creature Variant")]
    public sealed class CreatureVariantDefinition : ScriptableObject
    {
        public string Id;
        public Sprite Weapon;
        public int DamageBonus=2;
        public float ReachBonus=.25f;
        public LootTable Loot;
    }
}
