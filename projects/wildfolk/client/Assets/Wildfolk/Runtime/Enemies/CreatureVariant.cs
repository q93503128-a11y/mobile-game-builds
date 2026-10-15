using UnityEngine;
namespace Wildfolk
{
    public sealed class CreatureVariant : MonoBehaviour,IPoolReset
    {
        public CreatureVariantDefinition Definition;
        public SpriteRenderer Weapon;
        public LootTable BaseLoot;
        public bool Equipped {get;private set;}
        public void SetEquipped(bool value){Equipped=value;Weapon.sprite=value ? Definition.Weapon : null;Weapon.enabled=value;GetComponent<LootOnDeath>().Table=value ? Definition.Loot : BaseLoot;}
        public void ResetForSpawn()=>SetEquipped(false);
        public int DamageBonus=>Equipped ? Definition.DamageBonus : 0;
        public float ReachBonus=>Equipped ? Definition.ReachBonus : 0;
    }
}
